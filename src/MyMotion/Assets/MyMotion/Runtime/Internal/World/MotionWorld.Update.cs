using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
#if MYMOTION_PROFILE_MARKERS
using Unity.Profiling;
#endif

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        public void Update(double deltaTime)
        {
            Update(deltaTime, deltaTime, deltaTime);
        }

        // 配列の範囲は World 自身が管理しており、hot path の null / 範囲検査は生成コードから外す。
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        public unsafe void Update(double deltaTime, double unscaledDeltaTime, double realDeltaTime)
        {
            ThrowIfDisposed();
            if (_isUpdating)
            {
                throw new InvalidOperationException("Update cannot be called from a binding.");
            }
            if (!IsFinite(deltaTime) || !IsFinite(unscaledDeltaTime) || !IsFinite(realDeltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
            int count = _count;
            int parallelThreshold = _nativeCustomEaseCurveCount > 0
                ? CustomEaseParallelThreshold
                : ParallelThreshold;
            if (count == 0)
            {
                return;
            }
            _isUpdating = true;
            try
            {
                bool writeRecords = false;
                bool useOnlyFloatPath = IsOnlyFloatValueType();
                bool useOnlyVector3Path = IsOnlyVector3ValueType();
#if MYMOTION_PROFILE_MARKERS
                using (UpdateJobMarker.Auto())
#endif
                {
                    if (_adapterMotionCount == 0)
                    {
                        // Transform Job モード中も記録を書く。Job が書いた要素は記録から Binding の印を外すので、
                        // main thread は記録だけを読んで残りを処理できる。
                        writeRecords = true;
                        if (writeRecords) EnsureBindRecords(count);
                        var job = new UpdateJob
                        {
                            Records = _bindRecords,
                            WriteRecords = writeRecords,
                            Items = _items,
                            WideItems = _wideItems,
                            CustomEaseCurves = _nativeCustomEaseCurves,
                            DeltaTime = deltaTime,
                            UnscaledDeltaTime = unscaledDeltaTime,
                            RealDeltaTime = realDeltaTime,
                            UseOnlyFloatPath = useOnlyFloatPath,
                            UseOnlyVector3Path = useOnlyVector3Path
                        };
                        if (CanPipeline(count)) SchedulePipeline(job, count);
                        else if (count < parallelThreshold) MotionWorldDirectUpdate.Run(&job, count);
                        else job.ScheduleBatch(count, ParallelBatchSize).Complete();
                    }
                    else if (_adapterMotionCount < count)
                    {
                        var job = new UpdateMixedJob
                        {
                            Items = _items,
                            WideItems = _wideItems,
                            CustomEaseCurves = _nativeCustomEaseCurves,
                            DeltaTime = deltaTime,
                            UnscaledDeltaTime = unscaledDeltaTime,
                            RealDeltaTime = realDeltaTime,
                            UseOnlyFloatPath = useOnlyFloatPath,
                            UseOnlyVector3Path = useOnlyVector3Path
                        };
                        if (count < parallelThreshold) job.RunBatch(count);
                        else job.ScheduleBatch(count, ParallelBatchSize).Complete();
                    }
                }

                for (int i = 0; i < _adapterStorageCount; i++)
                {
                    _adapterStorages[i].Update(
                        _items,
                        deltaTime,
                        unscaledDeltaTime,
                        realDeltaTime);
                }
                RunTransformBindingJob(deltaTime, unscaledDeltaTime, realDeltaTime, writeRecords);
                bool transformJobBound = _transformJobBound;
                // ここ以降、World の入口が呼ばれると CompletePipeline が記録を無効にする。
                _bindRecordsValid = writeRecords;
                var records = _bindRecordsValid ? (BindRecord*)_bindRecords.GetUnsafeReadOnlyPtr() : null;
#if MYMOTION_PROFILE_MARKERS
                using (UpdateBindingMarker.Auto())
#endif
                {
                    // callback 経路では、Grow が実体配列を差し替えたときだけこのポインタを更新する。
                    var items = (Data*)_items.GetUnsafePtr();
                    int itemsCapacity = _items.Length;
                    object[] callbacks = _callbacks;
                    object[] states = _states;
                    object positionBinding = MyMotion.Extensions.TransformBindings.Position;
                    object localPositionBinding = MyMotion.Extensions.TransformBindings.LocalPosition;
                    object localScaleBinding = MyMotion.Extensions.TransformBindings.LocalScale;
                    object rotationBinding = MyMotion.Extensions.TransformBindings.Rotation;
                    object localRotationBinding = MyMotion.Extensions.TransformBindings.LocalRotation;
                    for (int i = 0; i < count; i++)
                    {
                        WaitPipeline(i);
                        if (_bindRecordsValid)
                        {
                            // 記録だけで Binding を済ませ、通知が要る要素だけ下の Data 経路へ進む。
                            ref BindRecord record = ref records[i];
                            byte bits = record.Bits;
                            if (bits == 0) continue;
                            if ((bits & RecordShouldBind) != 0)
                            {
                                try
                                {
                                    // 最頻の「状態1つ・float / Vector3 / Quaternion」は呼び出しを挟まずにここで delegate を呼ぶ。
                                    if ((record.Flags & (Data.StateCountMask | Data.IsValueStateBindingFlag)) == (1 << 12))
                                    {
                                        object callback = callbacks[i];
                                        object state = states[i];
                                        switch (record.ValueType)
                                        {
                                            case MotionValueType.Float:
                                                UnsafeUtility.As<object, Action<float, object>>(ref callback).Invoke(record.Value.Floats.x, state);
                                                break;
                                            case MotionValueType.Vector3:
                                            {
                                                var vector = new UnityEngine.Vector3(record.Value.Floats.x, record.Value.Floats.y, record.Value.Floats.z);
                                                // Transform 拡張の delegate なら、delegate を介さず setter を直接呼ぶ。
                                                if (DirectTransformBinding && callback == positionBinding) UnsafeUtility.As<object, UnityEngine.Transform>(ref state).position = vector;
                                                else if (DirectTransformBinding && callback == localPositionBinding) UnsafeUtility.As<object, UnityEngine.Transform>(ref state).localPosition = vector;
                                                else if (DirectTransformBinding && callback == localScaleBinding) UnsafeUtility.As<object, UnityEngine.Transform>(ref state).localScale = vector;
                                                else UnsafeUtility.As<object, Action<UnityEngine.Vector3, object>>(ref callback).Invoke(vector, state);
                                                break;
                                            }
                                            case MotionValueType.Quaternion:
                                            {
                                                var rotation = new UnityEngine.Quaternion(record.Value.Floats.x, record.Value.Floats.y, record.Value.Floats.z, record.Value.Floats.w);
                                                if (DirectTransformBinding && callback == rotationBinding) UnsafeUtility.As<object, UnityEngine.Transform>(ref state).rotation = rotation;
                                                else if (DirectTransformBinding && callback == localRotationBinding) UnsafeUtility.As<object, UnityEngine.Transform>(ref state).localRotation = rotation;
                                                else UnsafeUtility.As<object, Action<UnityEngine.Quaternion, object>>(ref callback).Invoke(rotation, state);
                                                break;
                                            }
                                            default:
                                                BindFromRecord(i, in record);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        BindFromRecord(i, in record);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (HandleBindingException(exception, i, ref items, ref itemsCapacity)) continue;
                                }
                                int capacityAfterBind = _items.Length;
                                if (capacityAfterBind != itemsCapacity)
                                {
                                    itemsCapacity = capacityAfterBind;
                                    items = (Data*)_items.GetUnsafePtr();
                                    callbacks = _callbacks;
                                    states = _states;
                                }
                            }
                            if ((bits & RecordNeedsEvent) == 0 && _bindRecordsValid) continue;
                            if (items[i].IsCanceled) continue;
                            goto Events;
                        }
                        ref Data item = ref items[i];
                        if (item.IsCanceled || item.IsInSequence || (item.IsCompletionNotified && !item.IsPreserved))
                        {
                            continue;
                        }
                        bool shouldBind = item.HasBinding && (item.IsComplete || !item.IsDelayed || !item.SkipValuesDuringDelay);
                        try
                        {
                            if (shouldBind && transformJobBound && IsBoundByTransformJob(item.Slot))
                            {
                                // Burst の Transform Job が書き込み済み。
                            }
                            else if (shouldBind)
                            {
                                if (item.Ease == MotionEase.CustomAnimationCurve && item.IsAdapter)
                                {
                                    ApplyCustomEase(ref item, i);
                                }
                                Bind(i, item);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (HandleBindingException(exception, i, ref items, ref itemsCapacity)) continue;
                        }

                        if (shouldBind)
                        {
                            int currentCapacity = _items.Length;
                            if (currentCapacity != itemsCapacity)
                            {
                                itemsCapacity = currentCapacity;
                                items = (Data*)_items.GetUnsafePtr();
                            }
                        }
                    Events:
                        Data* currentItems = items;
                        ref Data currentItem = ref currentItems[i];
                        if (currentItem.IsCompletionNotified && !currentItem.IsComplete)
                        {
                            // Preserve中に巻き戻された。再び完了したときに通知する。
                            currentItem.IsCompletionNotified = false;
                        }
                        bool loopCompleted = !currentItem.IsCompletionNotified && currentItem.CompletedLoops > currentItem.PreviousCompletedLoops;
                        ushort completedLoops = currentItem.CompletedLoops;
                        if (loopCompleted)
                        {
                            if (currentItem.IsCanceled) continue;
                            try
                            {
                                _notifications[i].OnLoopComplete?.Invoke(completedLoops);
                            }
                            catch (Exception exception)
                            {
                                ReportUnhandledException(exception);
                            }

                            int currentCapacity = _items.Length;
                            if (currentCapacity != itemsCapacity)
                            {
                                itemsCapacity = currentCapacity;
                                items = (Data*)_items.GetUnsafePtr();
                            }
                            currentItems = items;
                            currentItem = ref currentItems[i];
                            if (currentItem.IsCanceled) continue;
                        }

                        if (currentItem.IsComplete && !currentItem.IsCompletionNotified && !currentItem.IsCanceled)
                        {
                            currentItem.IsCompletionNotified = true;
                            if (!currentItem.IsInSequence && !currentItem.IsPreserved) QueueRemoval(i);
                            NotifyComplete(i);
                            int currentCapacity = _items.Length;
                            if (currentCapacity != itemsCapacity)
                            {
                                itemsCapacity = currentCapacity;
                                items = (Data*)_items.GetUnsafePtr();
                            }
                        }
                    }
                }
            }
            finally
            {
                CompletePipeline();
                _isUpdating = false;
                if (_hasPendingRemovals)
                {
                    _hasPendingRemovals = false;
                    if (_pendingRemovalOverflow)
                    {
                        ClearQueuedRemovalFlags();
                        var items = (Data*)_items.GetUnsafePtr();
                        for (int i = _count - 1; i >= 0; i--)
                        {
                            ref Data item = ref items[i];
                            if (item.IsCanceled || (item.IsComplete && !item.IsInSequence && !item.IsPreserved))
                            {
                                RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        CleanupQueuedRemovals();
                    }
                    _pendingRemovalCount = 0;
                    _pendingRemovalOverflow = false;
                    _pendingRemovalNeedsSort = false;
                }
            }
        }

        // Binding の例外を報告し、CancelOnError なら motion を cancel する。true なら呼び出し側は次の要素へ進む。
        private unsafe bool HandleBindingException(Exception exception, int index, ref Data* items, ref int itemsCapacity)
        {
            ReportUnhandledException(exception);
            Data current = _items[index];
            if (!current.CancelOnError) return false;
            if (!current.IsCanceled)
            {
                current.IsCanceled = true;
                _items[index] = current;
                QueueRemoval(index);
                NotifyCancel(index);
            }
            int currentCapacity = _items.Length;
            if (currentCapacity != itemsCapacity)
            {
                itemsCapacity = currentCapacity;
                items = (Data*)_items.GetUnsafePtr();
            }
            return true;
        }
    }
}

using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        public bool TrySetTime(CoreMotionHandle handle, double time, bool forceValueDuringDelay = false, bool allowSequenceAccess = false, bool markAsStarted = true)
        {
            if (!TryGetIndex(handle, out int index) || !IsFinite(time)) return false;
            return SetTimeAt(index, time, forceValueDuringDelay, allowSequenceAccess, markAsStarted);
        }

        /// <summary>
        /// 作った直後の motion に、Bind 時の初期値を書く (ImmediateBind)。
        /// CreateCore の直後なので、handle の検査と pipeline の完了待ちは要らない。
        /// </summary>
        internal void ApplyImmediateBind(CoreMotionHandle handle)
        {
            SetTimeAt(_slots[handle.Slot].DenseIndex, 0d, true, false, false);
        }

        private unsafe bool SetTimeAt(int index, double time, bool forceValueDuringDelay, bool allowSequenceAccess, bool markAsStarted)
        {
            // Data (約 100 byte) は配列の要素をその場で更新し、複製は Binding に渡す1回だけにする。
            // Binding の中で World が伸びると要素の位置が変わるため、Binding 以降は複製 (item) だけを読む。
            ref Data stored = ref ((Data*)_items.GetUnsafePtr())[index];
            if (stored.IsInSequence && !allowSequenceAccess) return false;
            ushort previousCompletedLoops = stored.CompletedLoops;
            bool previousIsComplete = stored.IsComplete;
            if (markAsStarted) stored.HasStarted = true;
            stored.IsCompletionNotified = false;
            if (!stored.IsCanceled)
            {
                stored.Elapsed = time;
                Evaluate(
                    ref stored,
                    IsOnlyFloatValueType(),
                    IsOnlyVector3ValueType(),
                    _wideItems,
                    _nativeCustomEaseCurves,
                    index,
                    !stored.IsAdapter,
                    forceValueDuringDelay);
            }
            bool shouldBind = stored.HasBinding && (forceValueDuringDelay || !stored.IsDelayed || !stored.SkipValuesDuringDelay);
            if (shouldBind && stored.Ease == MotionEase.CustomAnimationCurve && stored.IsAdapter)
            {
                ApplyCustomEase(ref stored, index);
            }
            Data item = stored;
            if (item.IsComplete && !item.IsInSequence && !item.IsPreserved)
            {
                QueueRemoval(index);
            }
            if (shouldBind)
            {
                try
                {
                    Bind(index, item);
                }
                catch (Exception exception)
                {
                    ReportUnhandledException(exception);
                    if (item.CancelOnError)
                    {
                        item.IsCanceled = true;
                        _items[index] = item;
                        QueueRemoval(index);
                        NotifyCancel(index);
                        return true;
                    }
                }
            }
            if (shouldBind && item.CompletedLoops > previousCompletedLoops) NotifyLoopComplete(index, item.CompletedLoops);
            if (item.IsComplete && !previousIsComplete)
            {
                item.IsCompletionNotified = true;
                _items[index] = item;
                NotifyComplete(index);
            }
            return true;
        }

        private void ApplyCustomEase(ref Data item, int index)
        {
            AnimationCurve curve = _customEaseCurves?[index];
            if (curve == null)
            {
                throw new InvalidOperationException("The custom ease curve has not been assigned.");
            }

            float t = item.Value.x;
            bool odd = ((item.CompletedLoops + (int)t) & 1) != 0;
            float progress = item.LoopType switch
            {
                MotionLoopType.Restart => curve.Evaluate(t),
                MotionLoopType.Flip => odd ? 1f - curve.Evaluate(t) : curve.Evaluate(t),
                MotionLoopType.Yoyo => curve.Evaluate(odd ? 1f - t : t),
                MotionLoopType.Incremental => _customEaseEndValues[index] * item.CompletedLoops + curve.Evaluate(t >= 1f ? 0f : t),
                _ => curve.Evaluate(t)
            };

            SetValue(ref item, progress, IsOnlyFloatValueType(), IsOnlyVector3ValueType(), _wideItems);
        }

        public double GetTime(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) throw new InvalidOperationException("Motion handle is not valid.");
            return _items[index].Elapsed;
        }

        internal void SetAdapterRoute(int slotIndex, MotionAdapterStorageBase storage, int adapterIndex)
        {
            _adapterRoutes[slotIndex] = new AdapterRoute { StorageIndex = storage.StorageIndex, Index = adapterIndex };
        }

        private bool TryGetIndex(CoreMotionHandle handle, out int index)
        {
            return TryGetIndexRegardlessOfCancellation(handle, out index) && !_items[index].IsCanceled;
        }

        private bool TryGetIndexRegardlessOfCancellation(CoreMotionHandle handle, out int index)
        {
            CompletePipeline();
            index = -1;
            if (_isDisposed || handle.Owner != _owner || handle.Version == 0 || handle.Slot < 0 || handle.Slot >= _slots.Length) return false;
            Slot slot = _slots[handle.Slot];
            if (slot.Version != handle.Version || slot.DenseIndex < 0) return false;
            index = slot.DenseIndex;
            return true;
        }

        public bool TrySetPlaybackSpeed(CoreMotionHandle handle, float playbackSpeed)
        {
            if (!IsActive(handle))
            {
                return false;
            }
            int index = _slots[handle.Slot].DenseIndex;
            Data item = _items[index];
            item.PlaybackSpeed = playbackSpeed;
            _items[index] = item;
            return true;
        }

        public float GetPlaybackSpeed(CoreMotionHandle handle)
        {
            if (!IsActive(handle))
            {
                throw new InvalidOperationException("Motion handle is not active.");
            }
            return _items[_slots[handle.Slot].DenseIndex].PlaybackSpeed;
        }
    }
}

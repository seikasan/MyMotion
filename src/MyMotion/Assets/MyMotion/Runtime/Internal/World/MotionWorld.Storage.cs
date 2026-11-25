using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        public void Dispose()
        {
            CompletePipeline();
            if (_isDisposed)
            {
                return;
            }
            if (_isUpdating)
            {
                throw new InvalidOperationException("Cannot dispose during Update.");
            }
            Unregister();
            _items.Dispose();
            if (_wideItems.IsCreated)
            {
                _wideItems.Dispose();
            }
            DisposeNativeCustomEaseCurves();
            DisposeTransformTargets();
            DisposeBindRecords();
            for (int i = 0; i < _adapterStorageCount; i++)
            {
                _adapterStorages[i].Dispose();
            }
            for (int i = 0; i < _valueStateBindingStorageCount; i++)
            {
                _valueStateBindingStorages[i].Dispose();
            }
            _valueStateBindingStorages = null;
            _valueStateBindingStorageCount = 0;
            _adapterStorages = null;
            _adapterStorageCount = 0;
            _adapterMotionCount = 0;
            _adapterRoutes = null;
            Array.Clear(_callbacks, 0, _callbacks.Length);
            Array.Clear(_states, 0, _states.Length);
            if (_states1 != null) Array.Clear(_states1, 0, _states1.Length);
            if (_states2 != null) Array.Clear(_states2, 0, _states2.Length);
            Array.Clear(_notifications, 0, _notifications.Length);
            if (_customEaseCurves != null) Array.Clear(_customEaseCurves, 0, _customEaseCurves.Length);
            if (_customEaseEndValues != null) Array.Clear(_customEaseEndValues, 0, _customEaseEndValues.Length);
            if (_debugNames != null) Array.Clear(_debugNames, 0, _debugNames.Length);
            _nativeCustomEaseCurveCount = 0;
            _isDisposed = true;
            _count = 0;
        }

        private MotionAdapterStorage<TValue, TOptions, TAdapter> GetOrCreateAdapterStorage<TValue, TOptions, TAdapter>()
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            for (int i = 0; i < _adapterStorageCount; i++)
            {
                if (_adapterStorages[i] is MotionAdapterStorage<TValue, TOptions, TAdapter> existing)
                {
                    return existing;
                }
            }

            if (_adapterStorageCount >= ushort.MaxValue)
            {
                throw new InvalidOperationException("A motion world cannot use more than 65,535 adapter types.");
            }
            if (_adapterStorages == null)
            {
                _adapterStorages = new MotionAdapterStorageBase[4];
            }
            else if (_adapterStorageCount == _adapterStorages.Length)
            {
                Array.Resize(ref _adapterStorages, checked(_adapterStorages.Length * 2));
            }

            var storage = new MotionAdapterStorage<TValue, TOptions, TAdapter>(4)
            {
                StorageIndex = (ushort)_adapterStorageCount
            };
            _adapterStorages[_adapterStorageCount++] = storage;
            return storage;
        }

        internal ValueStateBindingStorage<TValue, TState> GetOrCreateValueStateBindingStorage<TValue, TState>()
        {
            for (int i = 0; i < _valueStateBindingStorageCount; i++)
            {
                if (_valueStateBindingStorages[i] is ValueStateBindingStorage<TValue, TState> existing)
                {
                    return existing;
                }
            }

            if (_valueStateBindingStorages == null)
            {
                _valueStateBindingStorages = new ValueStateBindingStorageBase[4];
            }
            else if (_valueStateBindingStorageCount == _valueStateBindingStorages.Length)
            {
                Array.Resize(ref _valueStateBindingStorages, checked(_valueStateBindingStorages.Length * 2));
            }

            var storage = new ValueStateBindingStorage<TValue, TState>();
            _valueStateBindingStorages[_valueStateBindingStorageCount++] = storage;
            return storage;
        }

        internal void SetValueStateBinding<TValue, TState>(
            CoreMotionHandle handle,
            ValueStateBindingStorage<TValue, TState> storage,
            TState state,
            Action<TValue, TState> callback)
        {
            if (!TryGetIndexRegardlessOfCancellation(handle, out int index))
            {
                throw new InvalidOperationException("Motion handle is not valid.");
            }

            int stateIndex = storage.Add(state, callback);
            Slot slot = _slots[handle.Slot];
            slot.ValueStateIndex = stateIndex;
            _slots[handle.Slot] = slot;
            Data item = _items[index];
            item.IsValueStateBinding = true;
            _items[index] = item;
        }

        private unsafe CoreMotionHandle CreateCore(MotionValue start, MotionValue end, MotionValueType valueType, MotionRoundingMode roundingMode, float duration, MotionEase ease, object binding, object state, bool isStateful, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError, object state1 = null, object state2 = null, byte stateCount = 0)
        {
            CompletePipeline();
            ThrowIfDisposed();
            if (stateCount == 0 && isStateful) stateCount = 1;
            if (stateCount > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(stateCount));
            }
            // NaN は比較がすべて false になるので、「0 以上かつ有限」の1回の比較で NaN・無限大・負の値を弾く。
            if (!(duration >= 0f && duration < float.PositiveInfinity))
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }
            if (!(delay >= 0f && delay < float.PositiveInfinity))
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }
            if ((loops < 1 && loops != -1) || loopType > MotionLoopType.Yoyo || timeKind > MotionTimeKind.Realtime)
            {
                throw new ArgumentOutOfRangeException(nameof(loops));
            }
            if (ease > MotionEase.CustomAnimationCurve)
            {
                throw new ArgumentOutOfRangeException(nameof(ease));
            }
            if (_count == _items.Length)
            {
                Grow(checked(_count + 1));
            }
            if (stateCount >= 2 && _states1 == null) _states1 = new object[_items.Length];
            if (stateCount >= 3 && _states2 == null) _states2 = new object[_items.Length];
            bool isWide = valueType != MotionValueType.Float && valueType != MotionValueType.Vector3;
            int wideIndex = -1;
            if (isWide)
            {
                EnsureWideCapacity(_wideValueCount + 1);
                wideIndex = _wideValueCount;
                _wideItems[wideIndex] = new WideData { Start = start, End = end, Slot = _freeSlot };
            }
            int slotIndex = _freeSlot;
            ref Slot slot = ref _slots[slotIndex];
            _freeSlot = slot.Next;
            slot.DenseIndex = _count;
            slot.Next = -1;
            float3 narrowStart = valueType == MotionValueType.Vector3 ? start.Floats.xyz : new float3(start.Floats.x, 0f, 0f);
            float3 narrowEnd = valueType == MotionValueType.Vector3 ? end.Floats.xyz : new float3(end.Floats.x, 0f, 0f);
            // local に組んでから NativeArray へ複製せず、配列の要素へ直接書く。
            ((Data*)_items.GetUnsafePtr())[_count] = new Data { Start = narrowStart, End = narrowEnd, Duration = duration, Delay = delay, PlaybackSpeed = playbackSpeed, Slot = slotIndex, WideIndex = wideIndex, Ease = ease, TimeKind = timeKind, ValueType = valueType, RoundingMode = roundingMode, Flags = Data.CreateFlags(delayEveryLoop, stateCount, binding != null, delay > 0f, skipValuesDuringDelay, cancelOnError), Loops = loops, LoopType = loopType };
            // 空き要素は RemoveAt / Grow で null になっているため、null の書き込み (と GC の write barrier) は省く。
            if (binding != null) _callbacks[_count] = binding;
            if (state != null) _states[_count] = state;
            if (state1 != null) _states1[_count] = state1;
            if (state2 != null) _states2[_count] = state2;
            IncrementValueTypeCount(valueType);
            _count++;
            return new CoreMotionHandle(_owner, slotIndex, slot.Version);
        }
    }
}

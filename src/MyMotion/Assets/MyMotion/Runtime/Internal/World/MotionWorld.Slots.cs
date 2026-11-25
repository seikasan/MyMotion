using System.Runtime.CompilerServices;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        // 取り消しや完了のたびに呼ばれる。Data は複製せずポインタで読み、参照型の配列は値が変わるときだけ書く
        // (IL2CPP は参照型の要素への代入ごとに型検査と GC の write barrier を入れるため)。
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        private unsafe void RemoveAt(int index)
        {
            int last = --_count;
            var itemsPtr = (Data*)_items.GetUnsafePtr();
            ref Data removed = ref itemsPtr[index];
            if (removed.IsAdapter)
            {
                AdapterRoute route = _adapterRoutes[removed.Slot];
                _adapterStorages[route.StorageIndex].RemoveAt(route.Index, this);
                _adapterRoutes[removed.Slot] = default;
                _adapterMotionCount--;
            }
            if (removed.WideIndex >= 0)
            {
                RemoveWideAt(removed.WideIndex);
            }
            DecrementValueTypeCount(removed.ValueType);
            if (_transformEligibleCount != 0) ReleaseTransformTarget(removed.Slot);
            Slot freed = _slots[removed.Slot];
            if (removed.IsValueStateBinding)
            {
                ((ValueStateBindingStorageBase)_states[index]).RemoveAt(freed.ValueStateIndex);
            }
            if (_debugNames != null) _debugNames[removed.Slot] = null;
            freed.DenseIndex = -1;
            freed.Version++;
            if (freed.Version == 0)
            {
                freed.Version = 1;
            }
            freed.Next = _freeSlot;
            freed.ValueStateIndex = -1;
            _slots[removed.Slot] = freed;
            _freeSlot = removed.Slot;
            if (_hasNativeCustomEaseCurves)
            {
                CustomEaseCurveData removedCurve = _nativeCustomEaseCurves[index];
                if (removedCurve.IsAssigned) _nativeCustomEaseCurveCount--;
                if (removedCurve.Keys != null)
                {
                    UnsafeUtility.Free(removedCurve.Keys, Allocator.Persistent);
                }
                if (index != last)
                {
                    _nativeCustomEaseCurves[index] = _nativeCustomEaseCurves[last];
                }
                _nativeCustomEaseCurves[last] = default;
                if (_nativeCustomEaseCurveCount == 0) _hasNativeCustomEaseCurves = false;
            }
            if (index != last)
            {
                // removed は itemsPtr[index] を指す。ここで上書きするので、removed はこれ以降読まない。
                itemsPtr[index] = itemsPtr[last];
                ref Data moved = ref itemsPtr[index];
                MoveReference(_callbacks, index, last);
                MoveReference(_states, index, last);
                if (_states1 != null) MoveReference(_states1, index, last);
                if (_states2 != null) MoveReference(_states2, index, last);
                MoveNotifications(index, last);
                if (_customEaseCurves != null) _customEaseCurves[index] = _customEaseCurves[last];
                if (_customEaseEndValues != null) _customEaseEndValues[index] = _customEaseEndValues[last];
                Slot movedSlot = _slots[moved.Slot];
                movedSlot.DenseIndex = index;
                _slots[moved.Slot] = movedSlot;
                if (_transformLanes.IsCreated) MoveTransformLaneDenseIndex(moved.Slot, index);
                if (moved.IsAdapter)
                {
                    AdapterRoute route = _adapterRoutes[moved.Slot];
                    _adapterStorages[route.StorageIndex].SetCoreDenseIndex(route.Index, index);
                }
            }
            else
            {
                ClearReference(_callbacks, last);
                ClearReference(_states, last);
                if (_states1 != null) ClearReference(_states1, last);
                if (_states2 != null) ClearReference(_states2, last);
                ClearNotifications(ref _notifications[last]);
            }
            if (_customEaseCurves != null) _customEaseCurves[last] = null;
            if (_customEaseEndValues != null) _customEaseEndValues[last] = 0f;
        }

        // last の参照を index へ移し、last を空にする。どちらも null なら何も書かない。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveReference(object[] array, int index, int last)
        {
            object value = array[last];
            if (value != null)
            {
                array[index] = value;
                array[last] = null;
            }
            else if (array[index] != null)
            {
                array[index] = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearReference(object[] array, int index)
        {
            if (array[index] != null) array[index] = null;
        }

        private void MoveNotifications(int index, int last)
        {
            ref Notifications from = ref _notifications[last];
            ref Notifications to = ref _notifications[index];
            if (HasNotifications(in from))
            {
                to = from;
                from = default;
            }
            else
            {
                ClearNotifications(ref to);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasNotifications(in Notifications notifications) =>
            notifications.OnComplete != null || notifications.OnCancel != null || notifications.OnLoopComplete != null || notifications.Continuation != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearNotifications(ref Notifications notifications)
        {
            if (HasNotifications(in notifications)) notifications = default;
        }

        private bool IsOnlyFloatValueType()
        {
            return _floatValueCount > 0 && _vector3ValueCount == 0 && _wideValueCount == 0;
        }

        private bool IsOnlyVector3ValueType()
        {
            return _vector3ValueCount > 0 && _floatValueCount == 0 && _wideValueCount == 0;
        }

        private void IncrementValueTypeCount(MotionValueType valueType)
        {
            if (valueType == MotionValueType.Float) _floatValueCount++;
            else if (valueType == MotionValueType.Vector3) _vector3ValueCount++;
            else _wideValueCount++;
        }

        private void DecrementValueTypeCount(MotionValueType valueType)
        {
            if (valueType == MotionValueType.Float) _floatValueCount--;
            else if (valueType == MotionValueType.Vector3) _vector3ValueCount--;
            else _wideValueCount--;
        }

        private void EnsureNativeCustomEaseCurveCapacity(int capacity)
        {
            if (capacity <= _nativeCustomEaseCurves.Length) return;
            var next = new NativeArray<CustomEaseCurveData>(capacity, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            NativeArray<CustomEaseCurveData>.Copy(_nativeCustomEaseCurves, next, _nativeCustomEaseCurves.Length);
            _nativeCustomEaseCurves.Dispose();
            _nativeCustomEaseCurves = next;
        }

        private unsafe void DisposeNativeCustomEaseCurves()
        {
            if (!_nativeCustomEaseCurves.IsCreated) return;
            for (int i = 0; i < _nativeCustomEaseCurves.Length; i++)
            {
                CustomEaseCurveData curve = _nativeCustomEaseCurves[i];
                if (curve.Keys != null)
                {
                    UnsafeUtility.Free(curve.Keys, Allocator.Persistent);
                }
            }
            _nativeCustomEaseCurves.Dispose();
        }

        private void Grow(int minimumCapacity)
        {
            int previous = _items.Length;
            int doubledCapacity = previous <= int.MaxValue / 2 ? previous * 2 : int.MaxValue;
            int capacity = Math.Max(doubledCapacity, minimumCapacity);
            if (capacity <= previous)
            {
                throw new InvalidOperationException("Motion storage cannot grow beyond the maximum capacity.");
            }
            var next = new NativeArray<Data>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            NativeArray<Data>.Copy(_items, next, _count);
            _items.Dispose();
            _items = next;
            Array.Resize(ref _slots, capacity);
            Array.Resize(ref _callbacks, capacity);
            Array.Resize(ref _states, capacity);
            if (_states1 != null) Array.Resize(ref _states1, capacity);
            if (_states2 != null) Array.Resize(ref _states2, capacity);
            Array.Resize(ref _notifications, capacity);
            if (_adapterRoutes != null) Array.Resize(ref _adapterRoutes, capacity);
            if (_debugNames != null) Array.Resize(ref _debugNames, capacity);
            if (_customEaseCurves != null) Array.Resize(ref _customEaseCurves, capacity);
            if (_hasNativeCustomEaseCurves) EnsureNativeCustomEaseCurveCapacity(capacity);
            if (_customEaseEndValues != null) Array.Resize(ref _customEaseEndValues, capacity);
            int pendingRemovalCapacity = GetPendingRemovalCapacity(capacity);
            if (_pendingRemovalIndices.Length < pendingRemovalCapacity)
            {
                Array.Resize(ref _pendingRemovalIndices, pendingRemovalCapacity);
            }
            InitializeSlots(previous, capacity, _freeSlot);
        }

        private static int GetPendingRemovalCapacity(int capacity)
        {
            return Math.Min(capacity, Math.Max(16, capacity / 100));
        }
    }
}

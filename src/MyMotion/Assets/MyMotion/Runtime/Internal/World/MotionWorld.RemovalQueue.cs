using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        private unsafe void QueueRemoval(int index)
        {
            _hasPendingRemovals = true;
            if (_pendingRemovalOverflow) return;
            if (_count <= SparseCleanupMinMotionCount)
            {
                _pendingRemovalOverflow = true;
                return;
            }

            var items = (Data*)_items.GetUnsafePtr();
            ref Data item = ref items[index];
            if (item.IsRemovalQueued) return;
            if (_pendingRemovalCount == _pendingRemovalIndices.Length)
            {
                _pendingRemovalOverflow = true;
                return;
            }

            if (_pendingRemovalCount > 0 && index < _pendingRemovalIndices[_pendingRemovalCount - 1])
            {
                _pendingRemovalNeedsSort = true;
            }
            item.IsRemovalQueued = true;
            _pendingRemovalIndices[_pendingRemovalCount++] = index;
        }

        private unsafe void ClearQueuedRemovalFlags()
        {
            var items = (Data*)_items.GetUnsafePtr();
            for (int i = 0; i < _pendingRemovalCount; i++)
            {
                int index = _pendingRemovalIndices[i];
                items[index].IsRemovalQueued = false;
            }
        }

        private void CleanupQueuedRemovals()
        {
            int count = _pendingRemovalCount;
            if (count == 0) return;

            if (_pendingRemovalNeedsSort)
            {
                Array.Sort(_pendingRemovalIndices, 0, count);
            }
            ClearQueuedRemovalFlags();
            for (int i = count - 1; i >= 0; i--)
            {
                int index = _pendingRemovalIndices[i];
                Data item = _items[index];
                if (item.IsCanceled || (item.IsComplete && !item.IsInSequence && !item.IsPreserved))
                {
                    RemoveAt(index);
                }
            }
        }

        private void EnsureWideCapacity(int required)
        {
            int previous = _wideItems.IsCreated ? _wideItems.Length : 0;
            if (required <= previous)
            {
                return;
            }
            int capacity = math.max(4, previous);
            while (capacity < required)
            {
                capacity = checked(capacity * 2);
            }
            var next = new NativeArray<WideData>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            if (_wideItems.IsCreated)
            {
                NativeArray<WideData>.Copy(_wideItems, next, _wideValueCount);
                _wideItems.Dispose();
            }
            _wideItems = next;
        }

        private void RemoveWideAt(int index)
        {
            int last = _wideValueCount - 1;
            if (index == last)
            {
                return;
            }
            WideData moved = _wideItems[last];
            _wideItems[index] = moved;
            int denseIndex = _slots[moved.Slot].DenseIndex;
            Data movedItem = _items[denseIndex];
            movedItem.WideIndex = index;
            _items[denseIndex] = movedItem;
        }

        private void InitializeSlots(int start, int end, int next)
        {
            for (int i = start; i < end; i++)
            {
                _slots[i] = new Slot { DenseIndex = -1, Next = i + 1 < end ? i + 1 : next, Version = 1, ValueStateIndex = -1 };
            }
            _freeSlot = start;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(MotionWorld));
            }
        }
    }
}

using System;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        public string GetDebugName(CoreMotionHandle handle)
        {
            if (TryGetIndexRegardlessOfCancellation(handle, out _) && _debugNames != null)
            {
                string debugName = _debugNames[handle.Slot];
                if (debugName != null) return debugName;
            }
            return new MotionHandle(this, handle).ToString();
        }

        public void SetDebugName(CoreMotionHandle handle, string debugName)
        {
            if (debugName == null || !TryGetIndexRegardlessOfCancellation(handle, out _)) return;
            _debugNames ??= new string[_slots.Length];
            _debugNames[handle.Slot] = debugName;
        }

        public bool IsActive(CoreMotionHandle handle)
        {
            CompletePipeline();
            if (_isDisposed || handle.Owner != _owner || handle.Slot < 0 || handle.Slot >= _slots.Length)
            {
                return false;
            }
            Slot slot = _slots[handle.Slot];
            if (handle.Version == 0 || slot.Version != handle.Version || slot.DenseIndex < 0) return false;
            Data item = _items[slot.DenseIndex];
            return !item.IsCanceled && (!item.IsComplete || item.IsInSequence || item.IsPreserved);
        }

        public void Preserve(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index))
            {
                throw new InvalidOperationException("Motion handle is not valid.");
            }
            Data item = _items[index];
            item.IsPreserved = true;
            _items[index] = item;
        }

        public bool IsInfiniteLoop(CoreMotionHandle handle) =>
            TryGetIndexRegardlessOfCancellation(handle, out int index) && _items[index].Loops < 0;

        public bool IsPreserved(CoreMotionHandle handle)
        {
            return TryGetIndex(handle, out int index) && _items[index].IsPreserved;
        }

        public bool IsValid(CoreMotionHandle handle)
        {
            CompletePipeline();
            if (_isDisposed || handle.Owner != _owner || handle.Slot < 0 || handle.Slot >= _slots.Length) return false;
            Slot slot = _slots[handle.Slot];
            return handle.Version != 0 && slot.Version == handle.Version && slot.DenseIndex >= 0;
        }

        public bool IsPlaying(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) return false;
            Data item = _items[index];
            return !item.IsComplete && !item.IsDelayed;
        }

        public float GetDelay(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) throw new InvalidOperationException("Motion handle is not valid.");
            return _items[index].Delay;
        }

        public float GetDuration(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) throw new InvalidOperationException("Motion handle is not valid.");
            return _items[index].Duration;
        }

        public double GetTotalDuration(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) throw new InvalidOperationException("Motion handle is not valid.");
            Data item = _items[index];
            if (item.Loops < 0) return double.PositiveInfinity;
            return (double)item.Delay * (item.DelayEveryLoop ? item.Loops : 1) + (double)item.Duration * item.Loops;
        }

        public int GetLoops(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) throw new InvalidOperationException("Motion handle is not valid.");
            return _items[index].Loops;
        }

        public int GetCompletedLoops(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) throw new InvalidOperationException("Motion handle is not valid.");
            return _items[index].CompletedLoops;
        }

        public bool IsInSequence(CoreMotionHandle handle)
        {
            return TryGetIndex(handle, out int index) && _items[index].IsInSequence;
        }

        public double AddToSequence(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index))
            {
                throw new ArgumentException("Cannot add an invalid motion to a sequence.");
            }

            Data item = _items[index];
            if (item.IsInSequence)
            {
                throw new InvalidOperationException("Cannot access the motion in sequence.");
            }
            if (item.HasStarted)
            {
                throw new ArgumentException("Cannot add a running motion to a sequence.");
            }

            double duration = item.Loops < 0
                ? double.PositiveInfinity
                : (double)item.Delay * (item.DelayEveryLoop ? item.Loops : 1) + (double)item.Duration * item.Loops;
            if (double.IsInfinity(duration))
            {
                throw new ArgumentException("Cannot add an infinitely looping motion to a sequence.");
            }

            item.IsInSequence = true;
            _items[index] = item;
            return duration;
        }

        public void ReleaseSequence(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) return;
            Data item = _items[index];
            if (!item.IsInSequence) return;
            item.IsInSequence = false;
            _items[index] = item;
            if (item.IsCanceled || (item.IsComplete && !item.IsPreserved)) QueueRemoval(index);
        }

        public bool SetCallbacks(CoreMotionHandle handle, Action onComplete = null, Action onCancel = null, Action<int> onLoopComplete = null)
        {
            if (!IsActive(handle))
            {
                return false;
            }
            int index = _slots[handle.Slot].DenseIndex;
            Notifications notifications = _notifications[index];
            notifications.OnComplete = onComplete;
            notifications.OnCancel = onCancel;
            notifications.OnLoopComplete = onLoopComplete;
            _notifications[index] = notifications;
            return true;
        }
    }
}

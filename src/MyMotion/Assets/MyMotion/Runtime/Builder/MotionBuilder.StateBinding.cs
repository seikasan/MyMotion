using System;

namespace MyMotion
{
    internal sealed class SharedMotionBuilderConfiguration
    {
        public IMotionScheduler Scheduler;
        public MotionBuilderOptions Options;

        public SharedMotionBuilderConfiguration(IMotionScheduler scheduler, MotionBuilderOptions options)
        {
            Scheduler = scheduler;
            Options = options;
        }
    }

    internal abstract class ValueStateBindingStorageBase<TValue> : ValueStateBindingStorageBase
    {
        public abstract void Invoke(TValue value, int index);
    }

    internal abstract class ValueStateBindingStorageBase
    {
        public abstract void RemoveAt(int index);
        public abstract void Dispose();
    }

    internal sealed class ValueStateBindingStorage<TValue, TState> : ValueStateBindingStorageBase<TValue>
    {
        private struct Entry
        {
            public TState State;
            public Action<TValue, TState> Callback;
            public int NextFree;
        }

        private Entry[] _entries = new Entry[4];
        private int _count;
        private int _freeIndex = -1;

        public int Add(TState state, Action<TValue, TState> callback)
        {
            int index;
            if (_freeIndex >= 0)
            {
                index = _freeIndex;
                _freeIndex = _entries[index].NextFree;
            }
            else
            {
                index = _count++;
                EnsureCapacity(_count);
            }
            _entries[index] = new Entry { State = state, Callback = callback, NextFree = -1 };
            return index;
        }

        public override void RemoveAt(int index)
        {
            _entries[index] = new Entry { NextFree = _freeIndex };
            _freeIndex = index;
        }

        public override void Invoke(TValue value, int index)
        {
            ref Entry entry = ref _entries[index];
            entry.Callback(value, entry.State);
        }

        public override void Dispose()
        {
            Array.Clear(_entries, 0, _entries.Length);
            _entries = null;
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity <= _entries.Length) return;
            int nextCapacity = Math.Max(capacity, checked(_entries.Length * 2));
            Array.Resize(ref _entries, nextCapacity);
        }
    }
}

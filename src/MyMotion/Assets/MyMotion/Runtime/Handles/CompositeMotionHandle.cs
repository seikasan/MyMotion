using System;
using System.Collections;
using System.Collections.Generic;

namespace MyMotion
{
    /// <summary>
    /// 複数の motion handle をまとめて管理する。
    /// </summary>
    public sealed class CompositeMotionHandle : ICollection<MotionHandle>, IEnumerable<MotionHandle>
    {
        private readonly List<MotionHandle> _handles;

        public CompositeMotionHandle()
        {
            _handles = new List<MotionHandle>();
        }

        public CompositeMotionHandle(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _handles = new List<MotionHandle>(capacity);
        }

        public int Count => _handles.Count;

        public bool IsReadOnly => false;

        public void Add(MotionHandle handle) => _handles.Add(handle);

        public void Cancel()
        {
            for (int i = 0; i < _handles.Count; i++)
            {
                MotionHandle handle = _handles[i];
                if (handle.IsActive()) handle.TryCancel();
            }
            _handles.Clear();
        }

        public void Complete()
        {
            for (int i = 0; i < _handles.Count; i++)
            {
                MotionHandle handle = _handles[i];
                if (handle.IsActive()) handle.TryComplete();
            }
            _handles.Clear();
        }

        public void Clear() => _handles.Clear();

        public bool Contains(MotionHandle item) => _handles.Contains(item);

        public void CopyTo(MotionHandle[] array, int arrayIndex) => _handles.CopyTo(array, arrayIndex);

        public bool Remove(MotionHandle item) => _handles.Remove(item);

        public List<MotionHandle>.Enumerator GetEnumerator() => _handles.GetEnumerator();

        IEnumerator<MotionHandle> IEnumerable<MotionHandle>.GetEnumerator() => _handles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _handles.GetEnumerator();
    }
}

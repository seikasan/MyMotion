using System.Collections.Generic;
using UnityEngine;

namespace MyMotion.Internal
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    internal sealed class MotionHandleLinker : MonoBehaviour
    {
        private struct Entry
        {
            public MotionHandle Handle;
            public LinkBehavior Behavior;
        }

        private List<Entry> _entries;

        public void Register(MotionHandle handle, LinkBehavior behavior)
        {
            if (_entries == null)
            {
                _entries = new List<Entry>(4);
            }
            else if (_entries.Count == _entries.Capacity)
            {
                RemoveInactiveEntries();
            }

            _entries.Add(new Entry
            {
                Handle = handle, Behavior = behavior
            });
        }

        private void RemoveInactiveEntries()
        {
            int writeIndex = 0;
            for (int readIndex = 0; readIndex < _entries.Count; readIndex++)
            {
                Entry entry = _entries[readIndex];
                if (!entry.Handle.IsActive()) continue;

                _entries[writeIndex++] = entry;
            }

            if (writeIndex < _entries.Count)
            {
                _entries.RemoveRange(writeIndex, _entries.Count - writeIndex);
            }
        }

        private void OnDisable()
        {
            if (_entries == null) return;

            int count = _entries.Count;
            for (int i = 0; i < count; i++)
            {
                Entry entry = _entries[i];
                if (!entry.Handle.IsActive()) continue;

                if (entry.Behavior == LinkBehavior.CancelOnDisable)
                {
                    entry.Handle.TryCancel();
                }
                else if (entry.Behavior == LinkBehavior.CompleteOnDisable)
                {
                    entry.Handle.TryComplete();
                }
            }
        }

        private void OnDestroy()
        {
            if (_entries == null) return;

            int count = _entries.Count;
            for (int i = 0; i < count; i++)
            {
                Entry entry = _entries[i];
                if (entry.Behavior == LinkBehavior.CancelOnDestroy && entry.Handle.IsActive())
                {
                    entry.Handle.TryCancel();
                }
            }
        }
    }
}

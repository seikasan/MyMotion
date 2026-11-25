using System;
using System.Collections.Generic;

namespace MyMotion
{
    internal interface ISequenceReleaseHandler
    {
        void OnSequenceReleased();
    }

    /// <summary>
    /// 既存の motion を制御するタイムラインを作る。
    /// </summary>
    public static class MSequence
    {
        /// <summary>
        /// sequence builder を作る。
        /// </summary>
        public static MotionSequenceBuilder Create()
        {
            return new MotionSequenceBuilder(new MotionSequenceBuilderSource());
        }
    }

    /// <summary>
    /// motion・間隔・重なる motion からタイムラインを組み立てる。
    /// </summary>
    public struct MotionSequenceBuilder : IDisposable
    {
        private MotionSequenceBuilderSource _source;

        internal MotionSequenceBuilder(MotionSequenceBuilderSource source)
        {
            _source = source;
        }

        /// <summary>
        /// 現在の sequence の末尾の後ろに motion を追加する。
        /// </summary>
        public MotionSequenceBuilder Append(MotionHandle handle)
        {
            GetSource().Append(handle);
            return this;
        }

        /// <summary>
        /// 現在の sequence の末尾の後ろに間隔を追加する。
        /// </summary>
        public MotionSequenceBuilder AppendInterval(float interval)
        {
            GetSource().AppendInterval(interval);
            return this;
        }

        /// <summary>
        /// sequence 内の絶対位置に motion を追加する。
        /// </summary>
        public MotionSequenceBuilder Insert(float position, MotionHandle handle)
        {
            GetSource().Insert(position, handle);
            return this;
        }

        /// <summary>
        /// 直前に追加した motion と同時に始まるよう motion を追加する。
        /// </summary>
        public MotionSequenceBuilder Join(MotionHandle handle)
        {
            GetSource().Join(handle);
            return this;
        }

        /// <summary>
        /// 既定の motion 設定で sequence を実行する。
        /// </summary>
        public MotionHandle Run()
        {
            return Run(null);
        }

        /// <summary>
        /// sequence の駆動用 motion を設定して実行する。
        /// </summary>
        public MotionHandle Run(Action<MotionBuilder<double>> configuration)
        {
            MotionSequenceBuilderSource source = GetSource();
            MotionHandle handle = source.Run(configuration);
            _source = null;
            return handle;
        }

        /// <summary>
        /// 未完了の builder を解放し、含まれる motion を通常の更新に戻す。
        /// </summary>
        public void Dispose()
        {
            if (_source == null) return;
            _source.Dispose();
            _source = null;
        }

        private MotionSequenceBuilderSource GetSource()
        {
            if (_source == null)
            {
                throw new InvalidOperationException("MotionSequenceBuilder is not initialized or has already run.");
            }
            return _source;
        }
    }

    internal readonly struct MotionSequenceItem : IComparable<MotionSequenceItem>
    {
        public readonly double Position;
        public readonly MotionHandle Handle;

        public MotionSequenceItem(double position, MotionHandle handle)
        {
            Position = position;
            Handle = handle;
        }

        public int CompareTo(MotionSequenceItem other)
        {
            return Position.CompareTo(other.Position);
        }
    }

    internal sealed class MotionSequenceBuilderSource
    {
        private readonly List<MotionSequenceItem> _items = new List<MotionSequenceItem>();
        private double _tail;
        private double _lastTail;
        private double _duration;
        private bool _consumed;
        private bool _disposed;

        public void Append(MotionHandle handle)
        {
            ThrowIfUnavailable();
            double motionDuration = handle.AddToSequence();
            _items.Add(new MotionSequenceItem(_tail, handle));
            AppendInterval(motionDuration);
        }

        public void AppendInterval(double interval)
        {
            ThrowIfUnavailable();
            _lastTail = _tail;
            _tail += interval;
            _duration = Math.Max(_duration, _tail);
        }

        public void Insert(double position, MotionHandle handle)
        {
            ThrowIfUnavailable();
            double motionDuration = handle.AddToSequence();
            _items.Add(new MotionSequenceItem(position, handle));
            _duration = Math.Max(_duration, position + motionDuration);
        }

        public void Join(MotionHandle handle)
        {
            Insert(_lastTail, handle);
        }

        public MotionHandle Run(Action<MotionBuilder<double>> configuration)
        {
            ThrowIfUnavailable();
            MotionSequenceItem[] items = _items.ToArray();
            Array.Sort(items);
            var source = new MotionSequenceSource(items);

            try
            {
                MotionBuilder<double> builder = MMotion.Create(0d, _duration, (float)_duration)
                    .WithSharedConfiguration();
                configuration?.Invoke(builder);
                builder = builder.WithSequenceCallbacks(source.OnComplete, source.OnCancel);
                MotionHandle handle = builder.Bind(source, static (time, state) => state.SetTime(time));
                source.SetHandle(handle);
                _consumed = true;
                _items.Clear();
                return handle;
            }
            catch
            {
                ReleaseUnstartedItems();
                _disposed = true;
                throw;
            }
        }

        public void Dispose()
        {
            if (_consumed || _disposed) return;
            ReleaseUnstartedItems();
            _disposed = true;
        }

        private void ReleaseUnstartedItems()
        {
            foreach (MotionSequenceItem item in _items)
            {
                item.Handle.ReleaseSequence();
            }
            _items.Clear();
        }

        private void ThrowIfUnavailable()
        {
            if (_consumed || _disposed)
            {
                throw new InvalidOperationException("MotionSequenceBuilder is not initialized or has already run.");
            }
        }
    }

    internal sealed class MotionSequenceSource : ISequenceReleaseHandler
    {
        private readonly MotionSequenceItem[] _items;
        private MotionHandle _handle;
        private bool _childrenReleased;

        public MotionSequenceSource(MotionSequenceItem[] items)
        {
            _items = items;
        }

        public void SetHandle(MotionHandle handle)
        {
            _handle = handle;
        }

        public void SetTime(double time)
        {
            foreach (MotionSequenceItem item in _items)
            {
                item.Handle.TrySetTimeFromSequence(time - item.Position);
            }
        }

        public void OnComplete()
        {
            if (_handle.IsInSequence || _handle.IsPreserved) return;
            ReleaseChildren();
        }

        public void OnCancel()
        {
            ReleaseChildren();
        }

        public void OnSequenceReleased()
        {
            ReleaseChildren();
        }

        private void ReleaseChildren()
        {
            if (_childrenReleased) return;
            _childrenReleased = true;
            foreach (MotionSequenceItem item in _items)
            {
                item.Handle.FinishFromSequence();
            }
        }
    }
}

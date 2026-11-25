using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace MyMotion
{
    internal sealed class MotionValueTaskSource : IValueTaskSource
    {
        [ThreadStatic] private static MotionValueTaskSource s_cached;

        private readonly Action _motionContinuation;
        private ManualResetValueTaskSourceCore<bool> _core;
        private MotionHandle _handle;
        private CancelBehavior _cancelBehavior;
        private bool _cancelAwaitOnMotionCanceled;
        private CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationRegistration;
        private int _completionState;

        private MotionValueTaskSource()
        {
            _motionContinuation = OnMotionTerminated;
            _core.RunContinuationsAsynchronously = false;
        }

        public static ValueTask Create(
            MotionHandle handle,
            CancelBehavior cancelBehavior,
            bool cancelAwaitOnMotionCanceled,
            CancellationToken cancellationToken)
        {
            if (!handle.IsActive()) return default;
            if (cancellationToken.IsCancellationRequested)
            {
                MotionHandleExtensions.ApplyCancelBehavior(handle, cancelBehavior);
                return new ValueTask(Task.FromCanceled(cancellationToken));
            }

            MotionValueTaskSource source = s_cached;
            if (source == null) source = new MotionValueTaskSource();
            else s_cached = null;

            source.Initialize(handle, cancelBehavior, cancelAwaitOnMotionCanceled, cancellationToken);
            return new ValueTask(source, source._core.Version);
        }

        public void GetResult(short token)
        {
            try
            {
                _core.GetResult(token);
            }
            finally
            {
                ReturnToCache();
            }
        }

        public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);

        public void OnCompleted(
            Action<object> continuation,
            object state,
            short token,
            ValueTaskSourceOnCompletedFlags flags) =>
            _core.OnCompleted(continuation, state, token, flags);

        private void Initialize(
            MotionHandle handle,
            CancelBehavior cancelBehavior,
            bool cancelAwaitOnMotionCanceled,
            CancellationToken cancellationToken)
        {
            _handle = handle;
            _cancelBehavior = cancelBehavior;
            _cancelAwaitOnMotionCanceled = cancelAwaitOnMotionCanceled;
            _cancellationToken = cancellationToken;
            _completionState = 0;

            if (cancellationToken.CanBeCanceled)
            {
                _cancellationRegistration = MotionHandleExtensions.RegisterWithoutCaptureExecutionContext(
                    cancellationToken,
                    static state => ((MotionValueTaskSource)state).OnTokenCanceled(),
                    this);
            }

            if (Volatile.Read(ref _completionState) == 0)
            {
                handle.AddContinuation(_motionContinuation);
            }
        }

        private void OnMotionTerminated()
        {
            if (Interlocked.CompareExchange(ref _completionState, 1, 0) != 0) return;

            bool canceled = _cancellationToken.IsCancellationRequested
                || (_cancelAwaitOnMotionCanceled && _handle.WasCanceled);
            if (canceled)
            {
                _core.SetException(new OperationCanceledException(_cancellationToken));
            }
            else
            {
                _core.SetResult(true);
            }
        }

        private void OnTokenCanceled()
        {
            if (Interlocked.CompareExchange(ref _completionState, 1, 0) != 0) return;

            MotionHandle handle = _handle;
            CancellationToken cancellationToken = _cancellationToken;
            CancelBehavior cancelBehavior = _cancelBehavior;
            handle.RemoveContinuation(_motionContinuation);
            MotionHandleExtensions.ApplyCancelBehavior(handle, cancelBehavior);
            _core.SetException(new OperationCanceledException(cancellationToken));
        }

        private void ReturnToCache()
        {
            _cancellationRegistration.Dispose();
            _handle.RemoveContinuation(_motionContinuation);
            _cancellationRegistration = default;
            _handle = default;
            _cancelBehavior = default;
            _cancelAwaitOnMotionCanceled = false;
            _cancellationToken = CancellationToken.None;
            _completionState = 0;
            _core.Reset();

            s_cached ??= this;
        }
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyMotion.Extensions
{
    internal sealed class MotionUniTaskSource : IUniTaskSource, ITaskPoolNode<MotionUniTaskSource>
    {
        private static TaskPool<MotionUniTaskSource> s_pool;

        private readonly Action _motionContinuation;
        private MotionUniTaskSource _nextNode;
        private UniTaskCompletionSourceCore<AsyncUnit> _core;
        private MotionHandle _handle;
        private CancelBehavior _cancelBehavior;
        private bool _cancelAwaitOnMotionCanceled;
        private CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationRegistration;
        private int _completionState;

        private MotionUniTaskSource()
        {
            _motionContinuation = OnMotionTerminated;
        }

        public ref MotionUniTaskSource NextNode => ref _nextNode;

        public static IUniTaskSource Create(
            MotionHandle handle,
            CancelBehavior cancelBehavior,
            bool cancelAwaitOnMotionCanceled,
            CancellationToken cancellationToken,
            out short token)
        {
            if (!s_pool.TryPop(out MotionUniTaskSource source))
            {
                source = new MotionUniTaskSource();
            }

            source.Initialize(handle, cancelBehavior, cancelAwaitOnMotionCanceled, cancellationToken);
            token = source._core.Version;
            TaskTracker.TrackActiveTask(source, 3);
            return source;
        }

        public void GetResult(short token)
        {
            try
            {
                _core.GetResult(token);
            }
            finally
            {
                ReturnToPool();
            }
        }

        public UniTaskStatus GetStatus(short token) => _core.GetStatus(token);

        public UniTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token) =>
            _core.OnCompleted(continuation, state, token);

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
                    static state => ((MotionUniTaskSource)state).OnTokenCanceled(),
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
                _core.TrySetCanceled(_cancellationToken);
            }
            else
            {
                _core.TrySetResult(AsyncUnit.Default);
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
            _core.TrySetCanceled(cancellationToken);
        }

        private void ReturnToPool()
        {
            TaskTracker.RemoveTracking(this);
            _cancellationRegistration.Dispose();
            _handle.RemoveContinuation(_motionContinuation);
            _cancellationRegistration = default;
            _handle = default;
            _cancelBehavior = default;
            _cancelAwaitOnMotionCanceled = false;
            _cancellationToken = CancellationToken.None;
            _completionState = 0;
            _core.Reset();
            s_pool.TryPush(this);
        }
    }
}


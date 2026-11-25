#if UNITY_2023_1_OR_NEWER
using System;
using System.Threading;
using UnityEngine;

namespace MyMotion
{
    internal sealed class MotionAwaitableSource
    {
        private readonly Action _motionContinuation;
        private readonly AwaitableCompletionSource _completionSource = new();
        private MotionHandle _handle;
        private CancelBehavior _cancelBehavior;
        private bool _cancelAwaitOnMotionCanceled;
        private CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationRegistration;
        private int _completionState;

        private MotionAwaitableSource()
        {
            _motionContinuation = OnMotionTerminated;
        }

        public static Awaitable Create(
            MotionHandle handle,
            CancelBehavior cancelBehavior,
            bool cancelAwaitOnMotionCanceled,
            CancellationToken cancellationToken)
        {
            if (!handle.IsActive()) return CreateCompletedAwaitable();
            if (cancellationToken.IsCancellationRequested)
            {
                MotionHandleExtensions.ApplyCancelBehavior(handle, cancelBehavior);
                return CreateCanceledAwaitable();
            }

            MotionAwaitableSource source = new();
            source.Initialize(handle, cancelBehavior, cancelAwaitOnMotionCanceled, cancellationToken);
            return source.Awaitable;
        }

        private Awaitable Awaitable => _completionSource.Awaitable;

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

            if (cancellationToken.CanBeCanceled)
            {
                _cancellationRegistration = MotionHandleExtensions.RegisterWithoutCaptureExecutionContext(
                    cancellationToken,
                    static state => ((MotionAwaitableSource)state).OnTokenCanceled(),
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
            DisposeCancellationRegistration();

            if (canceled) _completionSource.SetCanceled();
            else _completionSource.SetResult();
        }

        private void OnTokenCanceled()
        {
            if (Interlocked.CompareExchange(ref _completionState, 1, 0) != 0) return;

            MotionHandle handle = _handle;
            CancelBehavior cancelBehavior = _cancelBehavior;
            handle.RemoveContinuation(_motionContinuation);
            MotionHandleExtensions.ApplyCancelBehavior(handle, cancelBehavior);
            DisposeCancellationRegistration();
            _completionSource.SetCanceled();
        }

        private void DisposeCancellationRegistration()
        {
            CancellationTokenRegistration registration = _cancellationRegistration;
            _cancellationRegistration = default;
            registration.Dispose();
        }

        private static Awaitable CreateCompletedAwaitable()
        {
            AwaitableCompletionSource source = new();
            source.SetResult();
            return source.Awaitable;
        }

        private static Awaitable CreateCanceledAwaitable()
        {
            AwaitableCompletionSource source = new();
            source.SetCanceled();
            return source.Awaitable;
        }
    }
}
#endif

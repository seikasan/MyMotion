using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using MyMotion.Internal;
using UnityEngine;

namespace MyMotion
{
    public static class MotionHandleExtensions
    {
        public static string GetDebugName(this MotionHandle handle) => handle.GetDebugNameInternal();

        public static MotionHandle Preserve(this MotionHandle handle)
        {
            handle.PreserveCore();
            return handle;
        }

        public static MotionHandle AddTo(this MotionHandle handle, CompositeMotionHandle compositeMotionHandle)
        {
            compositeMotionHandle.Add(handle);
            return handle;
        }

        public static MotionHandle AddTo(this MotionHandle handle, GameObject target) => AddTo(handle, target, LinkBehavior.CancelOnDestroy);

        public static MotionHandle AddTo(this MotionHandle handle, GameObject target, LinkBehavior linkBehavior)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            ValidateLinkBehavior(linkBehavior);
            GetOrAddLinker(target).Register(handle, linkBehavior);
            return handle;
        }

        public static MotionHandle AddTo(this MotionHandle handle, Component target) => AddTo(handle, target, LinkBehavior.CancelOnDestroy);

        public static MotionHandle AddTo(this MotionHandle handle, Component target, LinkBehavior linkBehavior)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return AddTo(handle, target.gameObject, linkBehavior);
        }

#if UNITY_2022_2_OR_NEWER
        public static MotionHandle AddTo(this MotionHandle handle, MonoBehaviour target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            target.destroyCancellationToken.Register(() => handle.TryCancel());
            return handle;
        }
#endif

        public static IDisposable ToDisposable(this MotionHandle handle, DisposeBehavior disposeBehavior = DisposeBehavior.Cancel) =>
            new MotionHandleDisposable(handle, disposeBehavior);

        public static IEnumerator ToYieldInstruction(this MotionHandle handle)
        {
            while (handle.IsActive())
            {
                yield return null;
            }
        }

        public static MotionAwaiter GetAwaiter(this MotionHandle handle) => new(handle);

        public static ValueTask ToValueTask(this MotionHandle handle, CancellationToken cancellationToken = default) =>
            MotionValueTaskSource.Create(handle, CancelBehavior.Cancel, true, cancellationToken);

        public static ValueTask ToValueTask(
            this MotionHandle handle,
            CancelBehavior cancelBehavior,
            CancellationToken cancellationToken = default) =>
            MotionValueTaskSource.Create(handle, cancelBehavior, true, cancellationToken);

        public static ValueTask ToValueTask(
            this MotionHandle handle,
            CancelBehavior cancelBehavior,
            bool cancelAwaitOnMotionCanceled,
            CancellationToken cancellationToken = default) =>
            MotionValueTaskSource.Create(handle, cancelBehavior, cancelAwaitOnMotionCanceled, cancellationToken);

#if UNITY_2023_1_OR_NEWER
        public static Awaitable ToAwaitable(this MotionHandle handle, CancellationToken cancellationToken = default) =>
            MotionAwaitableSource.Create(handle, CancelBehavior.Cancel, true, cancellationToken);

        public static Awaitable ToAwaitable(
            this MotionHandle handle,
            CancelBehavior cancelBehavior,
            CancellationToken cancellationToken = default) =>
            MotionAwaitableSource.Create(handle, cancelBehavior, true, cancellationToken);

        public static Awaitable ToAwaitable(
            this MotionHandle handle,
            CancelBehavior cancelBehavior,
            bool cancelAwaitOnMotionCanceled,
            CancellationToken cancellationToken = default) =>
            MotionAwaitableSource.Create(handle, cancelBehavior, cancelAwaitOnMotionCanceled, cancellationToken);
#endif

        internal static void ApplyCancelBehavior(MotionHandle handle, CancelBehavior cancelBehavior)
        {
            switch (cancelBehavior)
            {
                case CancelBehavior.Cancel:
                    handle.TryCancel();
                    break;
                case CancelBehavior.Complete:
                    handle.TryComplete();
                    break;
            }
        }

        internal static CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(
            CancellationToken cancellationToken,
            Action<object> callback,
            object state)
        {
            bool restoreExecutionContext = !ExecutionContext.IsFlowSuppressed();
            if (restoreExecutionContext) ExecutionContext.SuppressFlow();
            try
            {
                return cancellationToken.Register(callback, state, useSynchronizationContext: false);
            }
            finally
            {
                if (restoreExecutionContext) ExecutionContext.RestoreFlow();
            }
        }

        private static MotionHandleLinker GetOrAddLinker(GameObject target)
        {
            if (!target.TryGetComponent(out MotionHandleLinker linker)) linker = target.AddComponent<MotionHandleLinker>();
            return linker;
        }

        private static void ValidateLinkBehavior(LinkBehavior behavior)
        {
            if (behavior != LinkBehavior.CancelOnDestroy
                && behavior != LinkBehavior.CancelOnDisable
                && behavior != LinkBehavior.CompleteOnDisable)
            {
                throw new ArgumentOutOfRangeException(nameof(behavior));
            }
        }
    }

    public enum DisposeBehavior
    {
        Cancel,
        Complete
    }

    public enum LinkBehavior
    {
        CancelOnDestroy = 0,
        CancelOnDisable = 1,
        CompleteOnDisable = 2
    }

    internal sealed class MotionHandleDisposable : IDisposable
    {
        private readonly MotionHandle _handle;
        private readonly DisposeBehavior _disposeBehavior;

        public MotionHandleDisposable(MotionHandle handle, DisposeBehavior disposeBehavior)
        {
            _handle = handle;
            _disposeBehavior = disposeBehavior;
        }

        public void Dispose()
        {
            switch (_disposeBehavior)
            {
                case DisposeBehavior.Cancel:
                    _handle.TryCancel();
                    break;
                case DisposeBehavior.Complete:
                    _handle.TryComplete();
                    break;
            }
        }
    }
}

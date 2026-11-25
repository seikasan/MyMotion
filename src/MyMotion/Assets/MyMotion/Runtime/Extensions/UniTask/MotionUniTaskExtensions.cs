using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyMotion.Extensions
{
    /// <summary>
    /// motion の UniTask 連携を提供する。
    /// </summary>
    public static class MotionUniTaskExtensions
    {
        /// <summary>
        /// motion handle を UniTask へ変換する。
        /// </summary>
        public static UniTask ToUniTask(this MotionHandle handle, CancellationToken cancellationToken = default)
        {
            if (!handle.IsActive()) return UniTask.CompletedTask;
            return new UniTask(MotionUniTaskSource.Create(handle, CancelBehavior.Cancel, true, cancellationToken, out short token), token);
        }

        /// <summary>
        /// motion handle を UniTask へ変換する。
        /// </summary>
        public static UniTask ToUniTask(
            this MotionHandle handle,
            CancelBehavior cancelBehavior,
            CancellationToken cancellationToken = default)
        {
            if (!handle.IsActive()) return UniTask.CompletedTask;
            return new UniTask(MotionUniTaskSource.Create(handle, cancelBehavior, true, cancellationToken, out short token), token);
        }

        /// <summary>
        /// motion handle を UniTask へ変換する。
        /// </summary>
        public static UniTask ToUniTask(
            this MotionHandle handle,
            CancelBehavior cancelBehavior,
            bool cancelAwaitOnMotionCanceled,
            CancellationToken cancellationToken = default)
        {
            if (!handle.IsActive()) return UniTask.CompletedTask;
            return new UniTask(
                MotionUniTaskSource.Create(handle, cancelBehavior, cancelAwaitOnMotionCanceled, cancellationToken, out short token),
                token);
        }

        /// <summary>
        /// motion を非同期リアクティブプロパティへ bind する。
        /// </summary>
        public static MotionHandle BindToAsyncReactiveProperty<TValue>(
            this MotionBuilder<TValue> builder,
            AsyncReactiveProperty<TValue> reactiveProperty)
            where TValue : unmanaged
        {
            if (reactiveProperty == null) throw new ArgumentNullException(nameof(reactiveProperty));
            return builder.Bind(reactiveProperty, static (value, target) => target.Value = value);
        }

        /// <summary>
        /// adapter 付き motion を非同期リアクティブプロパティへ bind する。
        /// </summary>
        public static MotionHandle BindToAsyncReactiveProperty<TValue, TOptions, TAdapter>(
            this MotionBuilder<TValue, TOptions, TAdapter> builder,
            AsyncReactiveProperty<TValue> reactiveProperty)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (reactiveProperty == null) throw new ArgumentNullException(nameof(reactiveProperty));
            return builder.Bind(reactiveProperty, static (value, target) => target.Value = value);
        }
    }
}


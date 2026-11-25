using System;
using R3;

namespace MyMotion.Extensions
{
    /// <summary>
    /// motion の R3 連携を提供する。
    /// </summary>
    public static class MotionR3Extensions
    {
        /// <summary>
        /// 標準の motion builder から Observable を作る。
        /// </summary>
        public static Observable<TValue> ToObservable<TValue>(this MotionBuilder<TValue> builder)
            where TValue : unmanaged
        {
            var subject = new Subject<TValue>();
            builder
                .WithOnComplete(subject.OnCompleted)
                .WithOnCancel(subject.OnCompleted)
                .Bind(subject, static (value, target) => target.OnNext(value));
            return subject;
        }

        /// <summary>
        /// adapter 付き motion builder から Observable を作る。
        /// </summary>
        public static Observable<TValue> ToObservable<TValue, TOptions, TAdapter>(
            this MotionBuilder<TValue, TOptions, TAdapter> builder)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            var subject = new Subject<TValue>();
            builder
                .WithOnComplete(subject.OnCompleted)
                .WithOnCancel(subject.OnCompleted)
                .Bind(subject, static (value, target) => target.OnNext(value));
            return subject;
        }

        /// <summary>
        /// 標準の motion builder をリアクティブプロパティへ bind する。
        /// </summary>
        public static MotionHandle BindToReactiveProperty<TValue>(
            this MotionBuilder<TValue> builder,
            ReactiveProperty<TValue> reactiveProperty)
            where TValue : unmanaged
        {
            if (reactiveProperty == null) throw new ArgumentNullException(nameof(reactiveProperty));
            return builder.Bind(reactiveProperty, static (value, target) => target.Value = value);
        }

        /// <summary>
        /// adapter 付き motion builder をリアクティブプロパティへ bind する。
        /// </summary>
        public static MotionHandle BindToReactiveProperty<TValue, TOptions, TAdapter>(
            this MotionBuilder<TValue, TOptions, TAdapter> builder,
            ReactiveProperty<TValue> reactiveProperty)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (reactiveProperty == null) throw new ArgumentNullException(nameof(reactiveProperty));
            return builder.Bind(reactiveProperty, static (value, target) => target.Value = value);
        }

        /// <summary>
        /// motion handle を R3 の CompositeDisposable へ追加する。
        /// </summary>
        public static MotionHandle AddTo(
            this MotionHandle handle,
            CompositeDisposable disposable,
            DisposeBehavior disposeBehavior = DisposeBehavior.Cancel)
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            disposable.Add(handle.ToDisposable(disposeBehavior));
            return handle;
        }

        /// <summary>
        /// motion handle を R3 の SerialDisposable へ追加する。
        /// </summary>
        public static MotionHandle AddTo(
            this MotionHandle handle,
            SerialDisposable disposable,
            DisposeBehavior disposeBehavior = DisposeBehavior.Cancel)
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            disposable.Disposable = handle.ToDisposable(disposeBehavior);
            return handle;
        }

        /// <summary>
        /// motion handle を R3 の DisposableBag へ追加する。
        /// </summary>
        public static MotionHandle AddTo(
            this MotionHandle handle,
            ref DisposableBag disposable,
            DisposeBehavior disposeBehavior = DisposeBehavior.Cancel)
        {
            disposable.Add(handle.ToDisposable(disposeBehavior));
            return handle;
        }

        /// <summary>
        /// motion handle を R3 の DisposableBuilder へ追加する。
        /// </summary>
        public static MotionHandle AddTo(
            this MotionHandle handle,
            ref DisposableBuilder disposable,
            DisposeBehavior disposeBehavior = DisposeBehavior.Cancel)
        {
            disposable.Add(handle.ToDisposable(disposeBehavior));
            return handle;
        }
    }
}


using System;
using UniRx;

namespace MyMotion.Extensions
{
    /// <summary>
    /// motion の UniRx 連携を提供する。
    /// </summary>
    public static class MotionUniRxExtensions
    {
        /// <summary>
        /// 標準の motion builder から Observable を作る。
        /// </summary>
        public static IObservable<TValue> ToObservable<TValue>(this MotionBuilder<TValue> builder)
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
        public static IObservable<TValue> ToObservable<TValue, TOptions, TAdapter>(
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
    }
}


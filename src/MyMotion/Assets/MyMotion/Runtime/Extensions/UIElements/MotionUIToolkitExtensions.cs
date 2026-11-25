#if MYMOTION_SUPPORT_UIELEMENTS
using System;
using UnityEngine.UIElements;

namespace MyMotion.Extensions
{
    /// <summary>Unity UI Toolkit 向け bind 拡張。</summary>
    public static partial class MotionUIToolkitExtensions
    {
        private readonly struct RotateBindingState
        {
            public readonly VisualElement Target;
            public readonly AngleUnit Unit;

            public RotateBindingState(VisualElement target, AngleUnit unit)
            {
                Target = target;
                Unit = unit;
            }
        }

        private static void ThrowIfNull<TTarget>(TTarget target) where TTarget : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
        }

        private static MotionHandle Bind<TValue, TTarget>(MotionBuilder<TValue> builder, TTarget target, Action<TValue, TTarget> binding)
            where TValue : unmanaged
            where TTarget : class
        {
            ThrowIfNull(target);
            return builder.Bind(target, binding);
        }

        private static MotionHandle BindFormatted<TValue, TTarget, TState>(MotionBuilder<TValue> builder, TTarget target, TState state, Action<TValue, TTarget, TState> binding)
            where TValue : unmanaged
            where TTarget : class
            where TState : class
        {
            ThrowIfNull(target);
            return builder.Bind(target, state, binding);
        }

        private static MotionHandle Bind<TValue, TOptions, TAdapter, TTarget>(MotionBuilder<TValue, TOptions, TAdapter> builder, TTarget target, Action<TValue, TTarget> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
            where TTarget : class
        {
            ThrowIfNull(target);
            return builder.Bind(target, binding);
        }

        private static MotionHandle BindFormatted<TValue, TOptions, TAdapter, TTarget, TState>(MotionBuilder<TValue, TOptions, TAdapter> builder, TTarget target, TState state, Action<TValue, TTarget, TState> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
            where TTarget : class
            where TState : class
        {
            ThrowIfNull(target);
            return builder.Bind(target, state, binding);
        }

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleLeft(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.left = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleLeft<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.left = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleRight(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.right = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleRight<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.right = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTop(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.top = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTop<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.top = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBottom(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.bottom = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBottom<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.bottom = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleWidth(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.width = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleWidth<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.width = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleHeight(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.height = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleHeight<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.height = value;
            });
    }
}
#endif

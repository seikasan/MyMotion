#if MYMOTION_SUPPORT_UIELEMENTS
using UnityEngine;
using UnityEngine.UIElements;

namespace MyMotion.Extensions
{
    public static partial class MotionUIToolkitExtensions
    {
        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColor(this MotionBuilder<Color> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.color = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColor<TOptions, TAdapter>(this MotionBuilder<Color, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.color = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorR(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.r = value;
                element.style.color = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorR<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.r = value;
                element.style.color = color;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorG(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.g = value;
                element.style.color = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorG<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.g = value;
                element.style.color = color;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorB(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.b = value;
                element.style.color = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorB<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.b = value;
                element.style.color = color;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorA(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.a = value;
                element.style.color = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleColorA<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.color.value;
                color.a = value;
                element.style.color = color;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColor(this MotionBuilder<Color> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.backgroundColor = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColor<TOptions, TAdapter>(this MotionBuilder<Color, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.backgroundColor = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorR(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.r = value;
                element.style.backgroundColor = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorR<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.r = value;
                element.style.backgroundColor = color;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorG(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.g = value;
                element.style.backgroundColor = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorG<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.g = value;
                element.style.backgroundColor = color;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorB(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.b = value;
                element.style.backgroundColor = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorB<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.b = value;
                element.style.backgroundColor = color;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorA(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.a = value;
                element.style.backgroundColor = color;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleBackgroundColorA<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                Color color = element.style.backgroundColor.value;
                color.a = value;
                element.style.backgroundColor = color;
            });
    }
}
#endif

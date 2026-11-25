#if MYMOTION_SUPPORT_UIELEMENTS
using UnityEngine;
using UnityEngine.UIElements;

namespace MyMotion.Extensions
{
    public static partial class MotionUIToolkitExtensions
    {
        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleOpacity(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.opacity = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleOpacity<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.opacity = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleFontSize(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.fontSize = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleFontSize<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.fontSize = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleWordSpacing(this MotionBuilder<float> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.wordSpacing = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleWordSpacing<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.wordSpacing = value;
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTranslate(this MotionBuilder<Vector3> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.translate = new Translate(value.x, value.y, value.z);
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTranslate<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.translate = new Translate(value.x, value.y, value.z);
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTranslate(this MotionBuilder<Vector2> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.translate = new Translate(value.x, value.y);
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTranslate<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.translate = new Translate(value.x, value.y);
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleScale(this MotionBuilder<Vector3> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.scale = new Scale(value);
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleScale<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.scale = new Scale(value);
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTransformOrigin(this MotionBuilder<Vector3> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.transformOrigin = new TransformOrigin(value.x, value.y, value.z);
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTransformOrigin<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.transformOrigin = new TransformOrigin(value.x, value.y, value.z);
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTransformOrigin(this MotionBuilder<Vector2> builder, VisualElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.style.transformOrigin = new TransformOrigin(value.x, value.y);
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToStyleTransformOrigin<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, VisualElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.style.transformOrigin = new TransformOrigin(value.x, value.y);
            });
    }
}
#endif

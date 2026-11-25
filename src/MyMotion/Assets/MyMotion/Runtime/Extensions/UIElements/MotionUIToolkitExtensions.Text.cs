#if MYMOTION_SUPPORT_UIELEMENTS
using Unity.Collections;
using UnityEngine.UIElements;

namespace MyMotion.Extensions
{
    public static partial class MotionUIToolkitExtensions
    {
        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToProgressBar(this MotionBuilder<float> builder, AbstractProgressBar target)
            => Bind(builder, target, static (value, element) =>
            {
                element.value = value;
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToProgressBar<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, AbstractProgressBar target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.value = value;
            });

        public static MotionHandle BindToStyleRotate(this MotionBuilder<float> builder, VisualElement target, AngleUnit angleUnit = AngleUnit.Degree)
        {
            ThrowIfNull(target);
            var state = new RotateBindingState(target, angleUnit);
            return builder.Bind(state, static (value, binding) =>
                binding.Target.style.rotate = new Rotate(new Angle(value, binding.Unit)));
        }

        public static MotionHandle BindToStyleRotate<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, VisualElement target, AngleUnit angleUnit = AngleUnit.Degree)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            ThrowIfNull(target);
            var state = new RotateBindingState(target, angleUnit);
            return builder.Bind(state, static (value, binding) =>
                binding.Target.style.rotate = new Rotate(new Angle(value, binding.Unit)));
        }

        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<FixedString32Bytes, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString32Bytes, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ConvertToString();
            });

        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<FixedString64Bytes, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString64Bytes, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ConvertToString();
            });

        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<FixedString128Bytes, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString128Bytes, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ConvertToString();
            });

        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<FixedString512Bytes, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString512Bytes, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ConvertToString();
            });

        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<FixedString4096Bytes, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString4096Bytes, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ConvertToString();
            });

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText(this MotionBuilder<int> builder, TextElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ToString();
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<int, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ToString();
            });

        public static MotionHandle BindToText(this MotionBuilder<int> builder, TextElement target, string format)
            => BindFormatted(builder, target, format, static (value, element, state) => element.text = string.Format(state, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<int, TOptions, TAdapter> builder, TextElement target, string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions>
            => BindFormatted(builder, target, format, static (value, element, state) => element.text = string.Format(state, value));

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText(this MotionBuilder<long> builder, TextElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ToString();
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<long, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<long, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ToString();
            });

        public static MotionHandle BindToText(this MotionBuilder<long> builder, TextElement target, string format)
            => BindFormatted(builder, target, format, static (value, element, state) => element.text = string.Format(state, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<long, TOptions, TAdapter> builder, TextElement target, string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<long, TOptions>
            => BindFormatted(builder, target, format, static (value, element, state) => element.text = string.Format(state, value));

        /// <summary>motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText(this MotionBuilder<float> builder, TextElement target)
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ToString();
            });
        /// <summary>adapter motion を UI Toolkit の値へ bind する。</summary>
        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, TextElement target)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, target, static (value, element) =>
            {
                element.text = value.ToString();
            });

        public static MotionHandle BindToText(this MotionBuilder<float> builder, TextElement target, string format)
            => BindFormatted(builder, target, format, static (value, element, state) => element.text = string.Format(state, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, TextElement target, string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => BindFormatted(builder, target, format, static (value, element, state) => element.text = string.Format(state, value));
    }
}
#endif

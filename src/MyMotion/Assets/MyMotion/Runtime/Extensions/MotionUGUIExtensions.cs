#if MYMOTION_SUPPORT_UGUI
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MyMotion.Extensions
{
    /// <summary>
    /// Unity の uGUI コンポーネント向け bind 拡張。
    /// </summary>
    public static class MotionUGUIExtensions
    {
        public static MotionHandle BindToColor(this MotionBuilder<Color> builder, Graphic graphic) =>
            Bind(builder, graphic, static (value, target) => target.color = value);

        public static MotionHandle BindToColor<TOptions, TAdapter>(
            this MotionBuilder<Color, TOptions, TAdapter> builder,
            Graphic graphic)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions> =>
            Bind(builder, graphic, static (value, target) => target.color = value);

        public static MotionHandle BindToColorR(this MotionBuilder<float> builder, Graphic graphic) =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.r = value; target.color = color; });

        public static MotionHandle BindToColorR<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Graphic graphic)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.r = value; target.color = color; });

        public static MotionHandle BindToColorG(this MotionBuilder<float> builder, Graphic graphic) =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.g = value; target.color = color; });

        public static MotionHandle BindToColorG<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Graphic graphic)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.g = value; target.color = color; });

        public static MotionHandle BindToColorB(this MotionBuilder<float> builder, Graphic graphic) =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.b = value; target.color = color; });

        public static MotionHandle BindToColorB<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Graphic graphic)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.b = value; target.color = color; });

        public static MotionHandle BindToColorA(this MotionBuilder<float> builder, Graphic graphic) =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.a = value; target.color = color; });

        public static MotionHandle BindToColorA<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Graphic graphic)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, graphic, static (value, target) => { Color color = target.color; color.a = value; target.color = color; });

        public static MotionHandle BindToFillAmount(this MotionBuilder<float> builder, Image image) =>
            Bind(builder, image, static (value, target) => target.fillAmount = value);

        public static MotionHandle BindToFillAmount<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Image image)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, image, static (value, target) => target.fillAmount = value);

        public static MotionHandle BindToAlpha(this MotionBuilder<float> builder, CanvasGroup canvasGroup) =>
            Bind(builder, canvasGroup, static (value, target) => target.alpha = value);

        public static MotionHandle BindToAlpha<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            CanvasGroup canvasGroup)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, canvasGroup, static (value, target) => target.alpha = value);

        public static MotionHandle BindToFontSize(this MotionBuilder<int> builder, Text text) =>
            Bind(builder, text, static (value, target) => target.fontSize = value);

        public static MotionHandle BindToFontSize<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, static (value, target) => target.fontSize = value);

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString32Bytes, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString32Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ConvertToString());

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString64Bytes, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString64Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ConvertToString());

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString128Bytes, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString128Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ConvertToString());

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString512Bytes, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString512Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ConvertToString());

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString4096Bytes, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString4096Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ConvertToString());

        public static MotionHandle BindToText(this MotionBuilder<int> builder, Text text) =>
            Bind(builder, text, static (value, target) => target.text = value.ToString());

        public static MotionHandle BindToText(this MotionBuilder<int> builder, Text text, string format) =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ToString());

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            Text text,
            string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText(this MotionBuilder<long> builder, Text text) =>
            Bind(builder, text, static (value, target) => target.text = value.ToString());

        public static MotionHandle BindToText(this MotionBuilder<long> builder, Text text, string format) =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<long, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<long, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ToString());

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<long, TOptions, TAdapter> builder,
            Text text,
            string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<long, TOptions> =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText(this MotionBuilder<float> builder, Text text) =>
            Bind(builder, text, static (value, target) => target.text = value.ToString());

        public static MotionHandle BindToText(this MotionBuilder<float> builder, Text text, string format) =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => target.text = value.ToString());

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Text text,
            string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        private static MotionHandle Bind<TValue, TTarget>(
            MotionBuilder<TValue> builder,
            TTarget target,
            Action<TValue, TTarget> binding)
            where TTarget : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return builder.Bind(target, binding);
        }

        private static MotionHandle Bind<TValue, TTarget, TState>(
            MotionBuilder<TValue> builder,
            TTarget target,
            TState state,
            Action<TValue, TTarget, TState> binding)
            where TTarget : class
            where TState : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return builder.Bind(target, state, binding);
        }

        private static MotionHandle Bind<TValue, TOptions, TAdapter, TTarget>(
            MotionBuilder<TValue, TOptions, TAdapter> builder,
            TTarget target,
            Action<TValue, TTarget> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
            where TTarget : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return builder.Bind(target, binding);
        }

        private static MotionHandle Bind<TValue, TOptions, TAdapter, TTarget, TState>(
            MotionBuilder<TValue, TOptions, TAdapter> builder,
            TTarget target,
            TState state,
            Action<TValue, TTarget, TState> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
            where TTarget : class
            where TState : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return builder.Bind(target, state, binding);
        }
    }
}
#endif

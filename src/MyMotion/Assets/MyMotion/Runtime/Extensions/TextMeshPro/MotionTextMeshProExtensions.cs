#if MYMOTION_SUPPORT_TMP
using System;
using System.Buffers;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace MyMotion.Extensions
{
    /// <summary>
    /// TextMeshPro コンポーネント向け bind 拡張。
    /// </summary>
    public static class MotionTextMeshProExtensions
    {
        public static MotionHandle BindToFontSize(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.fontSize = value);

        public static MotionHandle BindToFontSize<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => target.fontSize = value);

        public static MotionHandle BindToMaxVisibleCharacters(this MotionBuilder<int> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.maxVisibleCharacters = value);

        public static MotionHandle BindToMaxVisibleCharacters<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, static (value, target) => target.maxVisibleCharacters = value);

        public static MotionHandle BindToMaxVisibleLines(this MotionBuilder<int> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.maxVisibleLines = value);

        public static MotionHandle BindToMaxVisibleLines<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, static (value, target) => target.maxVisibleLines = value);

        public static MotionHandle BindToMaxVisibleWords(this MotionBuilder<int> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.maxVisibleWords = value);

        public static MotionHandle BindToMaxVisibleWords<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, static (value, target) => target.maxVisibleWords = value);

        public static MotionHandle BindToCharacterSpacing(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.characterSpacing = value);

        public static MotionHandle BindToCharacterSpacing<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => target.characterSpacing = value);

        public static MotionHandle BindToWordSpacing(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.wordSpacing = value);

        public static MotionHandle BindToWordSpacing<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => target.wordSpacing = value);

        public static MotionHandle BindToParagraphSpacing(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.paragraphSpacing = value);

        public static MotionHandle BindToParagraphSpacing<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => target.paragraphSpacing = value);

        public static MotionHandle BindToLineSpacing(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.lineSpacing = value);

        public static MotionHandle BindToLineSpacing<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => target.lineSpacing = value);

        public static MotionHandle BindToColor(this MotionBuilder<Color> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.color = value);

        public static MotionHandle BindToColor<TOptions, TAdapter>(
            this MotionBuilder<Color, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions> =>
            Bind(builder, text, static (value, target) => target.color = value);

        public static MotionHandle BindToColorR(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.r = value; target.color = color; });

        public static MotionHandle BindToColorR<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.r = value; target.color = color; });

        public static MotionHandle BindToColorG(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.g = value; target.color = color; });

        public static MotionHandle BindToColorG<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.g = value; target.color = color; });

        public static MotionHandle BindToColorB(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.b = value; target.color = color; });

        public static MotionHandle BindToColorB<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.b = value; target.color = color; });

        public static MotionHandle BindToColorA(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.a = value; target.color = color; });

        public static MotionHandle BindToColorA<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => { Color color = target.color; color.a = value; target.color = color; });

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString32Bytes, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString32Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString64Bytes, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString64Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString128Bytes, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString128Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString512Bytes, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString512Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<FixedString4096Bytes, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<FixedString4096Bytes, TOptions> =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText(this MotionBuilder<int> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText(this MotionBuilder<int> builder, TMP_Text text, string format) =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            TMP_Text text,
            string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions> =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText(this MotionBuilder<long> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<long, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<long, TOptions> =>
            Bind(builder, text, static (value, target) => SetText(value, target));

        public static MotionHandle BindToText(this MotionBuilder<long> builder, TMP_Text text, string format) =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<long, TOptions, TAdapter> builder,
            TMP_Text text,
            string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<long, TOptions> =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText(this MotionBuilder<float> builder, TMP_Text text) =>
            Bind(builder, text, static (value, target) => target.SetText("{0}", value));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, static (value, target) => target.SetText("{0}", value));

        public static MotionHandle BindToText(this MotionBuilder<float> builder, TMP_Text text, string format) =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        public static MotionHandle BindToText<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            string format)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            Bind(builder, text, format, static (value, target, formatString) => target.text = string.Format(formatString, value));

        private static MotionHandle Bind<TValue>(
            MotionBuilder<TValue> builder,
            TMP_Text text,
            Action<TValue, TMP_Text> binding)
        {
            ThrowIfNull(text);
            return builder.Bind(text, binding);
        }

        private static MotionHandle Bind<TValue>(
            MotionBuilder<TValue> builder,
            TMP_Text text,
            string format,
            Action<TValue, TMP_Text, string> binding)
        {
            ThrowIfNull(text);
            return builder.Bind(text, format, binding);
        }

        private static MotionHandle Bind<TValue, TOptions, TAdapter>(
            MotionBuilder<TValue, TOptions, TAdapter> builder,
            TMP_Text text,
            Action<TValue, TMP_Text> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            ThrowIfNull(text);
            return builder.Bind(text, binding);
        }

        private static MotionHandle Bind<TValue, TOptions, TAdapter>(
            MotionBuilder<TValue, TOptions, TAdapter> builder,
            TMP_Text text,
            string format,
            Action<TValue, TMP_Text, string> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            ThrowIfNull(text);
            return builder.Bind(text, format, binding);
        }

        private static void ThrowIfNull(TMP_Text text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
        }

        private static unsafe void SetText(FixedString32Bytes value, TMP_Text text) => SetText(value.GetUnsafePtr(), value.Length, text);
        private static unsafe void SetText(FixedString64Bytes value, TMP_Text text) => SetText(value.GetUnsafePtr(), value.Length, text);
        private static unsafe void SetText(FixedString128Bytes value, TMP_Text text) => SetText(value.GetUnsafePtr(), value.Length, text);
        private static unsafe void SetText(FixedString512Bytes value, TMP_Text text) => SetText(value.GetUnsafePtr(), value.Length, text);
        private static unsafe void SetText(FixedString4096Bytes value, TMP_Text text) => SetText(value.GetUnsafePtr(), value.Length, text);

        private static unsafe void SetText(byte* utf8, int byteLength, TMP_Text text)
        {
            char[] buffer = ArrayPool<char>.Shared.Rent(Math.Max(1, byteLength * 2));
            try
            {
                int charLength;
                fixed (char* utf16 = buffer)
                {
                    Unicode.Utf8ToUtf16(utf8, byteLength, utf16, out charLength, buffer.Length);
                }

                text.SetText(buffer, 0, charLength);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private static void SetText(int value, TMP_Text text) => SetText((long)value, text);

        private static void SetText(long value, TMP_Text text)
        {
            char[] buffer = ArrayPool<char>.Shared.Rent(20);
            try
            {
                int charLength = FormatInt64(value, buffer, out int startIndex);
                text.SetText(buffer, startIndex, charLength);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private static int FormatInt64(long value, char[] buffer, out int startIndex)
        {
            bool isNegative = value < 0;
            ulong magnitude = isNegative ? (ulong)(-(value + 1)) + 1UL : (ulong)value;
            int index = buffer.Length;

            do
            {
                ulong digit = magnitude % 10;
                magnitude /= 10;
                buffer[--index] = (char)('0' + digit);
            }
            while (magnitude != 0);

            if (isNegative) buffer[--index] = '-';
            startIndex = index;
            return buffer.Length - index;
        }
    }
}
#endif

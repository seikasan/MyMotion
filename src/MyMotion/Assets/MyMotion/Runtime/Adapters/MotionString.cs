using System;
using Unity.Collections;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace MyMotion
{
    /// <summary>
    /// 文字列 motion でまだ表示されていない部分を埋める文字を選ぶ。
    /// </summary>
    public enum ScrambleMode : byte
    {
        None = 0,
        Uppercase = 1,
        Lowercase = 2,
        Numerals = 3,
        All = 4,
        Custom = 5
    }

    /// <summary>
    /// 固定長文字列 motion のオプション。
    /// </summary>
    [Serializable]
    public struct StringOptions : IMotionOptions, IEquatable<StringOptions>
    {
        public ScrambleMode ScrambleMode;
        public bool RichTextEnabled;
        public FixedString64Bytes CustomScrambleChars;
        public uint RandomSeed;

        public readonly bool Equals(StringOptions other) =>
            ScrambleMode == other.ScrambleMode
            && RichTextEnabled == other.RichTextEnabled
            && CustomScrambleChars.Equals(other.CustomScrambleChars)
            && RandomSeed == other.RandomSeed;

        public override readonly bool Equals(object obj) => obj is StringOptions other && Equals(other);

        public override readonly int GetHashCode() =>
            HashCode.Combine(ScrambleMode, RichTextEnabled, CustomScrambleChars, RandomSeed);
    }

    /// <summary>
    /// 文字列 motion 固有のオプションを設定する。
    /// </summary>
    public static class MotionStringBuilderExtensions
    {
        public static MotionBuilder<TValue, StringOptions, TAdapter> WithRichText<TValue, TAdapter>(
            this MotionBuilder<TValue, StringOptions, TAdapter> builder,
            bool richTextEnabled = true)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, StringOptions>
        {
            StringOptions options = builder.Options;
            options.RichTextEnabled = richTextEnabled;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, StringOptions, TAdapter> WithRandomSeed<TValue, TAdapter>(
            this MotionBuilder<TValue, StringOptions, TAdapter> builder,
            uint seed)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, StringOptions>
        {
            StringOptions options = builder.Options;
            options.RandomSeed = seed;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, StringOptions, TAdapter> WithScrambleChars<TValue, TAdapter>(
            this MotionBuilder<TValue, StringOptions, TAdapter> builder,
            ScrambleMode scrambleMode)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, StringOptions>
        {
            if (scrambleMode == ScrambleMode.Custom)
            {
                throw new ArgumentException(
                    "Use the FixedString64Bytes overload to set custom scramble characters.",
                    nameof(scrambleMode));
            }

            StringOptions options = builder.Options;
            options.ScrambleMode = scrambleMode;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, StringOptions, TAdapter> WithScrambleChars<TValue, TAdapter>(
            this MotionBuilder<TValue, StringOptions, TAdapter> builder,
            FixedString64Bytes customScrambleChars)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, StringOptions>
        {
            StringOptions options = builder.Options;
            options.ScrambleMode = ScrambleMode.Custom;
            options.CustomScrambleChars = customScrambleChars;
            return builder.WithOptions(options);
        }
    }

    public static partial class MMotion
    {
        /// <summary>
        /// 更新 Job 内でマネージド文字列を確保せずに、UTF-8 固定長文字列を補間する motion を作る。
        /// </summary>
        public static class String
        {
            public static MotionBuilder<FixedString32Bytes, StringOptions, Adapters.FixedString32BytesMotionAdapter> Create32Bytes(
                in FixedString32Bytes from,
                in FixedString32Bytes to,
                float duration) =>
                Create<FixedString32Bytes, StringOptions, Adapters.FixedString32BytesMotionAdapter>(from, to, duration);

            public static MotionBuilder<FixedString64Bytes, StringOptions, Adapters.FixedString64BytesMotionAdapter> Create64Bytes(
                in FixedString64Bytes from,
                in FixedString64Bytes to,
                float duration) =>
                Create<FixedString64Bytes, StringOptions, Adapters.FixedString64BytesMotionAdapter>(from, to, duration);

            public static MotionBuilder<FixedString128Bytes, StringOptions, Adapters.FixedString128BytesMotionAdapter> Create128Bytes(
                in FixedString128Bytes from,
                in FixedString128Bytes to,
                float duration) =>
                Create<FixedString128Bytes, StringOptions, Adapters.FixedString128BytesMotionAdapter>(from, to, duration);

            public static MotionBuilder<FixedString512Bytes, StringOptions, Adapters.FixedString512BytesMotionAdapter> Create512Bytes(
                in FixedString512Bytes from,
                in FixedString512Bytes to,
                float duration) =>
                Create<FixedString512Bytes, StringOptions, Adapters.FixedString512BytesMotionAdapter>(from, to, duration);

            public static MotionBuilder<FixedString4096Bytes, StringOptions, Adapters.FixedString4096BytesMotionAdapter> Create4096Bytes(
                in FixedString4096Bytes from,
                in FixedString4096Bytes to,
                float duration) =>
                Create<FixedString4096Bytes, StringOptions, Adapters.FixedString4096BytesMotionAdapter>(from, to, duration);
        }
    }
}

namespace MyMotion.Internal
{
    internal static class MotionStringRandomHelper
    {
        public static Random Create(uint seed, double time)
        {
            Hash128 hash = default;
            hash.Append(ref seed);
            hash.Append(ref time);
            return new Random((uint)hash.GetHashCode());
        }
    }
}

namespace MyMotion.Adapters
{
    using MyMotion.Internal;

    public readonly struct FixedString32BytesMotionAdapter : IMotionAdapter<FixedString32Bytes, StringOptions>
    {
        public FixedString32Bytes Evaluate(
            ref FixedString32Bytes startValue,
            ref FixedString32Bytes endValue,
            ref StringOptions options,
            in MotionEvaluationContext context)
        {
            FixedString64Bytes customScrambleChars = options.CustomScrambleChars;
            Random randomState = options.ScrambleMode == ScrambleMode.None
                ? default
                : MotionStringRandomHelper.Create(options.RandomSeed, context.Time);
            FixedStringHelper.Interpolate(
                ref startValue,
                ref endValue,
                context.Progress,
                options.ScrambleMode,
                options.RichTextEnabled,
                ref randomState,
                ref customScrambleChars,
                out FixedString32Bytes result);
            return result;
        }
    }

    public readonly struct FixedString64BytesMotionAdapter : IMotionAdapter<FixedString64Bytes, StringOptions>
    {
        public FixedString64Bytes Evaluate(
            ref FixedString64Bytes startValue,
            ref FixedString64Bytes endValue,
            ref StringOptions options,
            in MotionEvaluationContext context)
        {
            FixedString64Bytes customScrambleChars = options.CustomScrambleChars;
            Random randomState = options.ScrambleMode == ScrambleMode.None
                ? default
                : MotionStringRandomHelper.Create(options.RandomSeed, context.Time);
            FixedStringHelper.Interpolate(
                ref startValue,
                ref endValue,
                context.Progress,
                options.ScrambleMode,
                options.RichTextEnabled,
                ref randomState,
                ref customScrambleChars,
                out FixedString64Bytes result);
            return result;
        }
    }

    public readonly struct FixedString128BytesMotionAdapter : IMotionAdapter<FixedString128Bytes, StringOptions>
    {
        public FixedString128Bytes Evaluate(
            ref FixedString128Bytes startValue,
            ref FixedString128Bytes endValue,
            ref StringOptions options,
            in MotionEvaluationContext context)
        {
            FixedString64Bytes customScrambleChars = options.CustomScrambleChars;
            Random randomState = options.ScrambleMode == ScrambleMode.None
                ? default
                : MotionStringRandomHelper.Create(options.RandomSeed, context.Time);
            FixedStringHelper.Interpolate(
                ref startValue,
                ref endValue,
                context.Progress,
                options.ScrambleMode,
                options.RichTextEnabled,
                ref randomState,
                ref customScrambleChars,
                out FixedString128Bytes result);
            return result;
        }
    }

    public readonly struct FixedString512BytesMotionAdapter : IMotionAdapter<FixedString512Bytes, StringOptions>
    {
        public FixedString512Bytes Evaluate(
            ref FixedString512Bytes startValue,
            ref FixedString512Bytes endValue,
            ref StringOptions options,
            in MotionEvaluationContext context)
        {
            FixedString64Bytes customScrambleChars = options.CustomScrambleChars;
            Random randomState = options.ScrambleMode == ScrambleMode.None
                ? default
                : MotionStringRandomHelper.Create(options.RandomSeed, context.Time);
            FixedStringHelper.Interpolate(
                ref startValue,
                ref endValue,
                context.Progress,
                options.ScrambleMode,
                options.RichTextEnabled,
                ref randomState,
                ref customScrambleChars,
                out FixedString512Bytes result);
            return result;
        }
    }

    public readonly struct FixedString4096BytesMotionAdapter : IMotionAdapter<FixedString4096Bytes, StringOptions>
    {
        public FixedString4096Bytes Evaluate(
            ref FixedString4096Bytes startValue,
            ref FixedString4096Bytes endValue,
            ref StringOptions options,
            in MotionEvaluationContext context)
        {
            FixedString64Bytes customScrambleChars = options.CustomScrambleChars;
            Random randomState = options.ScrambleMode == ScrambleMode.None
                ? default
                : MotionStringRandomHelper.Create(options.RandomSeed, context.Time);
            FixedStringHelper.Interpolate(
                ref startValue,
                ref endValue,
                context.Progress,
                options.ScrambleMode,
                options.RichTextEnabled,
                ref randomState,
                ref customScrambleChars,
                out FixedString4096Bytes result);
            return result;
        }
    }
}

using System;
using UnityEngine;

namespace MyMotion
{
    [Serializable]
    public struct PunchOptions : IMotionOptions, IEquatable<PunchOptions>
    {
        public int Frequency;
        public float DampingRatio;

        public static PunchOptions Default => new()
        {
            Frequency = 10,
            DampingRatio = 1f
        };

        public readonly bool Equals(PunchOptions other) =>
            Frequency == other.Frequency && DampingRatio == other.DampingRatio;

        public override readonly bool Equals(object obj) => obj is PunchOptions other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(Frequency, DampingRatio);
    }

    [Serializable]
    public struct ShakeOptions : IMotionOptions, IEquatable<ShakeOptions>
    {
        public ShakeMode ShakeMode;
        public int Frequency;
        public float DampingRatio;
        public uint RandomSeed;

        public static ShakeOptions Default => new ShakeOptions
        {
            ShakeMode = ShakeMode.PerlinNoise,
            Frequency = 10,
            DampingRatio = 1f
        };

        public readonly bool Equals(ShakeOptions other) =>
            ShakeMode == other.ShakeMode
            && Frequency == other.Frequency
            && DampingRatio == other.DampingRatio
            && RandomSeed == other.RandomSeed;

        public override readonly bool Equals(object obj) => obj is ShakeOptions other && Equals(other);

        public override readonly int GetHashCode() =>
            HashCode.Combine(ShakeMode, Frequency, DampingRatio, RandomSeed);
    }

    public enum ShakeMode : byte
    {
        Random = 0,
        PerlinNoise = 1
    }

    public static class MotionVibrationBuilderExtensions
    {
        public static MotionBuilder<TValue, PunchOptions, TAdapter> WithFrequency<TValue, TAdapter>(
            this MotionBuilder<TValue, PunchOptions, TAdapter> builder,
            int frequency)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, PunchOptions>
        {
            PunchOptions options = builder.Options;
            options.Frequency = frequency;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, PunchOptions, TAdapter> WithDampingRatio<TValue, TAdapter>(
            this MotionBuilder<TValue, PunchOptions, TAdapter> builder,
            float dampingRatio)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, PunchOptions>
        {
            PunchOptions options = builder.Options;
            options.DampingRatio = dampingRatio;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, ShakeOptions, TAdapter> WithShakeMode<TValue, TAdapter>(
            this MotionBuilder<TValue, ShakeOptions, TAdapter> builder,
            ShakeMode shakeMode)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, ShakeOptions>
        {
            ShakeOptions options = builder.Options;
            options.ShakeMode = shakeMode;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, ShakeOptions, TAdapter> WithFrequency<TValue, TAdapter>(
            this MotionBuilder<TValue, ShakeOptions, TAdapter> builder,
            int frequency)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, ShakeOptions>
        {
            ShakeOptions options = builder.Options;
            options.Frequency = frequency;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, ShakeOptions, TAdapter> WithDampingRatio<TValue, TAdapter>(
            this MotionBuilder<TValue, ShakeOptions, TAdapter> builder,
            float dampingRatio)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, ShakeOptions>
        {
            ShakeOptions options = builder.Options;
            options.DampingRatio = dampingRatio;
            return builder.WithOptions(options);
        }

        public static MotionBuilder<TValue, ShakeOptions, TAdapter> WithRandomSeed<TValue, TAdapter>(
            this MotionBuilder<TValue, ShakeOptions, TAdapter> builder,
            uint seed)
            where TValue : unmanaged
            where TAdapter : unmanaged, IMotionAdapter<TValue, ShakeOptions>
        {
            ShakeOptions options = builder.Options;
            options.RandomSeed = seed;
            return builder.WithOptions(options);
        }
    }

    public static partial class MMotion
    {
        // LitMotion と同じ MMotion.Punch.Create(...) / MMotion.Shake.Create(...) の形にするため、外側の MMotion.Create と同名にしている。
        // 継承ではないので new は付けられず、Rider の「外側のクラスの static メンバーを隠す」警告だけを抑制する。
        // ReSharper disable MemberHidesStaticFromOuterClass
        public static class Punch
        {
            public static MotionBuilder<float, PunchOptions, Adapters.FloatPunchMotionAdapter> Create(
                float startValue,
                float strength,
                float duration) =>
                Create<float, Adapters.FloatPunchMotionAdapter>(startValue, strength, duration);

            public static MotionBuilder<Vector2, PunchOptions, Adapters.Vector2PunchMotionAdapter> Create(
                Vector2 startValue,
                Vector2 strength,
                float duration) =>
                Create<Vector2, Adapters.Vector2PunchMotionAdapter>(startValue, strength, duration);

            public static MotionBuilder<Vector3, PunchOptions, Adapters.Vector3PunchMotionAdapter> Create(
                Vector3 startValue,
                Vector3 strength,
                float duration) =>
                Create<Vector3, Adapters.Vector3PunchMotionAdapter>(startValue, strength, duration);

            private static MotionBuilder<TValue, PunchOptions, TAdapter> Create<TValue, TAdapter>(
                TValue startValue,
                TValue strength,
                float duration)
                where TValue : unmanaged
                where TAdapter : unmanaged, IMotionAdapter<TValue, PunchOptions> =>
                MMotion.Create<TValue, PunchOptions, TAdapter>(startValue, strength, duration)
                    .WithOptions(PunchOptions.Default);
        }

        public static class Shake
        {
            public static MotionBuilder<float, ShakeOptions, Adapters.FloatShakeMotionAdapter> Create(
                float startValue,
                float strength,
                float duration) =>
                Create<float, Adapters.FloatShakeMotionAdapter>(startValue, strength, duration);

            public static MotionBuilder<Vector2, ShakeOptions, Adapters.Vector2ShakeMotionAdapter> Create(
                Vector2 startValue,
                Vector2 strength,
                float duration) =>
                Create<Vector2, Adapters.Vector2ShakeMotionAdapter>(startValue, strength, duration);

            public static MotionBuilder<Vector3, ShakeOptions, Adapters.Vector3ShakeMotionAdapter> Create(
                Vector3 startValue,
                Vector3 strength,
                float duration) =>
                Create<Vector3, Adapters.Vector3ShakeMotionAdapter>(startValue, strength, duration);

            private static MotionBuilder<TValue, ShakeOptions, TAdapter> Create<TValue, TAdapter>(
                TValue startValue,
                TValue strength,
                float duration)
                where TValue : unmanaged
                where TAdapter : unmanaged, IMotionAdapter<TValue, ShakeOptions> =>
                MMotion.Create<TValue, ShakeOptions, TAdapter>(startValue, strength, duration)
                    .WithOptions(ShakeOptions.Default);
        }
        // ReSharper restore MemberHidesStaticFromOuterClass
    }
}

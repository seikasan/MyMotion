using System;
using UnityEngine;

namespace MyMotion
{
    public static partial class MMotion
    {
        public static MotionBuilder<float> Create(MotionSettings<float, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<double> Create(MotionSettings<double, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<int> Create(MotionSettings<int, IntegerOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(
                Create(settings.StartValue, settings.EndValue, settings.Duration)
                    .WithRoundingMode((IntegerRoundingMode)settings.Options.RoundingMode),
                settings);
        }

        public static MotionBuilder<long> Create(MotionSettings<long, IntegerOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(
                Create(settings.StartValue, settings.EndValue, settings.Duration)
                    .WithRoundingMode((IntegerRoundingMode)settings.Options.RoundingMode),
                settings);
        }

        public static MotionBuilder<Vector2> Create(MotionSettings<Vector2, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<Vector3> Create(MotionSettings<Vector3, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<Vector4> Create(MotionSettings<Vector4, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<Quaternion> Create(MotionSettings<Quaternion, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<Color> Create(MotionSettings<Color, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<Rect> Create(MotionSettings<Rect, NoOptions> settings)
        {
            ValidateSettings(settings);
            return ApplySettings(Create(settings.StartValue, settings.EndValue, settings.Duration), settings);
        }

        public static MotionBuilder<TValue, TOptions, TAdapter> Create<TValue, TOptions, TAdapter>(
            MotionSettings<TValue, TOptions> settings)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            ValidateSettings(settings);
            MotionBuilder<TValue, TOptions, TAdapter> builder = Create<TValue, TOptions, TAdapter>(
                    settings.StartValue, settings.EndValue, settings.Duration)
                .WithOptions(settings.Options);
            return ApplySettings(builder, settings);
        }

        private static MotionBuilder<TValue> ApplySettings<TValue, TOptions>(
            MotionBuilder<TValue> builder,
            MotionSettings<TValue, TOptions> settings)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
        {
            builder = settings.Ease == Ease.CustomAnimationCurve
                ? builder.WithEase(settings.CustomEaseCurve)
                : builder.WithEase(settings.Ease);
            builder = builder
                .WithDelay(settings.Delay, settings.DelayType, settings.SkipValuesDuringDelay)
                .WithLoops(settings.Loops, settings.LoopType)
                .WithPlaybackSpeed(settings.PlaybackSpeed)
                .WithTimeKind(settings.TimeKind)
                .WithCancelOnError(settings.CancelOnError)
                .WithImmediateBind(settings.ImmediateBind);
            return settings.Scheduler == null ? builder : builder.WithScheduler(settings.Scheduler);
        }

        private static MotionBuilder<TValue, TOptions, TAdapter> ApplySettings<TValue, TOptions, TAdapter>(
            MotionBuilder<TValue, TOptions, TAdapter> builder,
            MotionSettings<TValue, TOptions> settings)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            builder = settings.Ease == Ease.CustomAnimationCurve
                ? builder.WithEase(settings.CustomEaseCurve)
                : builder.WithEase(settings.Ease);
            builder = builder
                .WithDelay(settings.Delay, settings.DelayType, settings.SkipValuesDuringDelay)
                .WithLoops(settings.Loops, settings.LoopType)
                .WithPlaybackSpeed(settings.PlaybackSpeed)
                .WithTimeKind(settings.TimeKind)
                .WithCancelOnError(settings.CancelOnError)
                .WithImmediateBind(settings.ImmediateBind);
            return settings.Scheduler == null ? builder : builder.WithScheduler(settings.Scheduler);
        }

        private static void ValidateSettings<TValue, TOptions>(MotionSettings<TValue, TOptions> settings)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
        }
    }

    /// <summary>
    /// 組み込みの builder を、再利用できる設定へ変換する。
    /// </summary>
    public static class MotionBuilderSettingsExtensions
    {
        public static MotionSettings<TValue, NoOptions> ToMotionSettings<TValue>(this MotionBuilder<TValue> builder)
            where TValue : unmanaged
        {
            builder.GetSettings(
                out TValue startValue,
                out TValue endValue,
                out MotionBuilderOptions options,
                out IMotionScheduler scheduler);
            return FromBuilder<TValue, NoOptions>(startValue, endValue, default, options, scheduler);
        }

        public static MotionSettings<int, IntegerOptions> ToMotionSettings(this MotionBuilder<int> builder)
        {
            builder.GetSettings(out int startValue, out int endValue, out MotionBuilderOptions options, out IMotionScheduler scheduler);
            return FromBuilder(
                startValue,
                endValue,
                new IntegerOptions { RoundingMode = (RoundingMode)options.RoundingMode },
                options,
                scheduler);
        }

        public static MotionSettings<long, IntegerOptions> ToMotionSettings(this MotionBuilder<long> builder)
        {
            builder.GetSettings(out long startValue, out long endValue, out MotionBuilderOptions options, out IMotionScheduler scheduler);
            return FromBuilder(
                startValue,
                endValue,
                new IntegerOptions { RoundingMode = (RoundingMode)options.RoundingMode },
                options,
                scheduler);
        }

        internal static MotionSettings<TValue, TOptions> FromBuilder<TValue, TOptions>(
            TValue startValue,
            TValue endValue,
            TOptions adapterOptions,
            MotionBuilderOptions options,
            IMotionScheduler scheduler)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions =>
            new()
            {
                StartValue = startValue,
                EndValue = endValue,
                Duration = options.Duration,
                Options = adapterOptions,
                Ease = options.Ease,
                CustomEaseCurve = options.CustomEaseCurve,
                Delay = options.Delay,
                DelayType = options.DelayEveryLoop ? DelayType.EveryLoop : DelayType.FirstLoop,
                Loops = options.Loops,
                LoopType = options.LoopType,
                CancelOnError = options.CancelOnError,
                SkipValuesDuringDelay = options.SkipValuesDuringDelay,
                ImmediateBind = options.ImmediateBind,
                Scheduler = scheduler,
                PlaybackSpeed = options.PlaybackSpeed,
                TimeKind = options.TimeKind
            };
    }
}

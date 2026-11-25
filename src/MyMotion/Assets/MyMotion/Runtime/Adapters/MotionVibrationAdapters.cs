using Unity.Mathematics;
using UnityEngine;

namespace MyMotion.Adapters
{
    public readonly struct FloatPunchMotionAdapter : IMotionAdapter<float, PunchOptions>
    {
        public float Evaluate(
            ref float startValue,
            ref float endValue,
            ref PunchOptions options,
            in MotionEvaluationContext context)
        {
            MotionVibration.EvaluateStrength(
                endValue,
                options.Frequency,
                options.DampingRatio,
                context.Progress,
                out float result);
            return startValue + result;
        }
    }

    public readonly struct Vector2PunchMotionAdapter : IMotionAdapter<Vector2, PunchOptions>
    {
        public Vector2 Evaluate(
            ref Vector2 startValue,
            ref Vector2 endValue,
            ref PunchOptions options,
            in MotionEvaluationContext context)
        {
            MotionVibration.EvaluateStrength(
                endValue,
                options.Frequency,
                options.DampingRatio,
                context.Progress,
                out Vector2 result);
            return startValue + result;
        }
    }

    public readonly struct Vector3PunchMotionAdapter : IMotionAdapter<Vector3, PunchOptions>
    {
        public Vector3 Evaluate(
            ref Vector3 startValue,
            ref Vector3 endValue,
            ref PunchOptions options,
            in MotionEvaluationContext context)
        {
            MotionVibration.EvaluateStrength(
                endValue,
                options.Frequency,
                options.DampingRatio,
                context.Progress,
                out Vector3 result);
            return startValue + result;
        }
    }

    public readonly struct FloatShakeMotionAdapter : IMotionAdapter<float, ShakeOptions>
    {
        public float Evaluate(
            ref float startValue,
            ref float endValue,
            ref ShakeOptions options,
            in MotionEvaluationContext context)
        {
            MotionVibration.EvaluateStrength(
                endValue,
                options.Frequency,
                options.DampingRatio,
                context.Progress,
                out float strength);
            float multiplier = options.ShakeMode == ShakeMode.Random
                ? MotionVibration.NextFloat(options.RandomSeed, context.Time, -1f, 1f)
                : MotionVibration.PerlinNoise(options.RandomSeed, 0, context.Progress, options.Frequency);
            return startValue + strength * multiplier;
        }
    }

    public readonly struct Vector2ShakeMotionAdapter : IMotionAdapter<Vector2, ShakeOptions>
    {
        public Vector2 Evaluate(
            ref Vector2 startValue,
            ref Vector2 endValue,
            ref ShakeOptions options,
            in MotionEvaluationContext context)
        {
            MotionVibration.EvaluateStrength(
                endValue,
                options.Frequency,
                options.DampingRatio,
                context.Progress,
                out Vector2 strength);
            float2 multiplier = options.ShakeMode == ShakeMode.Random
                ? MotionVibration.NextFloat2(options.RandomSeed, context.Time, new float2(-1f), new float2(1f))
                : new float2(
                    MotionVibration.PerlinNoise(options.RandomSeed, 0, context.Progress, options.Frequency),
                    MotionVibration.PerlinNoise(options.RandomSeed, 1, context.Progress, options.Frequency));
            return startValue + new Vector2(strength.x * multiplier.x, strength.y * multiplier.y);
        }
    }

    public readonly struct Vector3ShakeMotionAdapter : IMotionAdapter<Vector3, ShakeOptions>
    {
        public Vector3 Evaluate(
            ref Vector3 startValue,
            ref Vector3 endValue,
            ref ShakeOptions options,
            in MotionEvaluationContext context)
        {
            MotionVibration.EvaluateStrength(
                endValue,
                options.Frequency,
                options.DampingRatio,
                context.Progress,
                out Vector3 strength);
            float3 multiplier = options.ShakeMode == ShakeMode.Random
                ? MotionVibration.NextFloat3(options.RandomSeed, context.Time, new float3(-1f), new float3(1f))
                : new float3(
                    MotionVibration.PerlinNoise(options.RandomSeed, 0, context.Progress, options.Frequency),
                    MotionVibration.PerlinNoise(options.RandomSeed, 1, context.Progress, options.Frequency),
                    MotionVibration.PerlinNoise(options.RandomSeed, 2, context.Progress, options.Frequency));
            return startValue + new Vector3(
                strength.x * multiplier.x,
                strength.y * multiplier.y,
                strength.z * multiplier.z);
        }
    }
}

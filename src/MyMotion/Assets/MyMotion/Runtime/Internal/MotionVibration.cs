using Unity.Mathematics;
using UnityEngine;

namespace MyMotion
{
    internal static class MotionVibration
    {
        internal static void EvaluateStrength(
            float strength,
            int frequency,
            float dampingRatio,
            float progress,
            out float result)
        {
            if (progress == 0f || progress == 1f)
            {
                result = 0f;
                return;
            }

            float angularFrequency = (frequency * progress - 0.5f) * math.PI;
            float dampingFactor = dampingRatio * frequency / (2f * math.PI);
            result = strength * math.exp(-dampingFactor * progress) * math.cos(angularFrequency);
        }

        internal static void EvaluateStrength(
            Vector2 strength,
            int frequency,
            float dampingRatio,
            float progress,
            out Vector2 result)
        {
            if (progress == 0f || progress == 1f)
            {
                result = Vector2.zero;
                return;
            }

            float angularFrequency = (frequency * progress - 0.5f) * math.PI;
            float dampingFactor = dampingRatio * frequency / (2f * math.PI);
            result = math.cos(angularFrequency) * math.exp(-dampingFactor * progress) * strength;
        }

        internal static void EvaluateStrength(
            Vector3 strength,
            int frequency,
            float dampingRatio,
            float progress,
            out Vector3 result)
        {
            if (progress == 0f || progress == 1f)
            {
                result = Vector3.zero;
                return;
            }

            float angularFrequency = (frequency * progress - 0.5f) * math.PI;
            float dampingFactor = dampingRatio * frequency / (2f * math.PI);
            result = math.cos(angularFrequency) * math.exp(-dampingFactor * progress) * strength;
        }

        internal static float PerlinNoise(uint seed, uint axis, float progress, int frequency)
        {
            float offset = (math.hash(new uint2(seed, axis)) & 65535u) / 256f;
            return noise.cnoise(new float2(progress * frequency, offset));
        }

        internal static float NextFloat(uint seed, double time, float min, float max) =>
            CreateRandom(seed, time).NextFloat(min, max);

        internal static float2 NextFloat2(uint seed, double time, in float2 min, in float2 max) =>
            CreateRandom(seed, time).NextFloat2(min, max);

        internal static float3 NextFloat3(uint seed, double time, in float3 min, in float3 max) =>
            CreateRandom(seed, time).NextFloat3(min, max);

        private static Unity.Mathematics.Random CreateRandom(uint seed, double time)
        {
            Hash128 hash = default;
            hash.Append(ref seed);
            hash.Append(ref time);
            uint state = (uint)hash.GetHashCode();
            return new Unity.Mathematics.Random(state == 0 ? 1u : state);
        }
    }
}

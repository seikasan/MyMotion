using Unity.Mathematics;
using UnityEngine;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        internal static unsafe class CustomEaseCurveEvaluator
        {
            private struct Cubic
            {
                public float A;
                public float B;
                public float C;
                public float D;

                public readonly float Evaluate(float t) => ((A * t + B) * t + C) * t + D;
                public readonly float Derivative(float t) => (3f * A * t + 2f * B) * t + C;

                public static Cubic From(float p0, float p1, float p2, float p3) => new()
                {
                    A = -p0 + 3f * p1 - 3f * p2 + p3,
                    B = 3f * p0 - 6f * p1 + 3f * p2,
                    C = -3f * p0 + 3f * p1,
                    D = p0
                };
            }

            public static float Evaluate(in CustomEaseCurveData curve, float time)
            {
                if (curve.Length == 0) return 0f;
                if (curve.Length == 1) return curve.Keys[0].value;

                Keyframe first = curve.Keys[0];
                Keyframe last = curve.Keys[curve.Length - 1];
                time = WrapTime(time, first.time, last.time, curve.PreWrapMode, curve.PostWrapMode);
                if (time <= first.time) return first.value;
                if (time >= last.time) return last.value;

                int lower = 0;
                int upper = curve.Length - 1;
                while (lower + 1 < upper)
                {
                    int middle = (lower + upper) >> 1;
                    if (curve.Keys[middle].time <= time) lower = middle;
                    else upper = middle;
                }

                Keyframe current = curve.Keys[lower];
                Keyframe next = curve.Keys[upper];
                if (time == current.time) return current.value;
                float timeDiff = next.time - current.time;
                if (!(timeDiff > 0f)) return current.value;
                if (!math.isfinite(current.outTangent) || !math.isfinite(next.inTangent))
                {
                    return current.value;
                }

                bool weightedOut = current.weightedMode == WeightedMode.Out || current.weightedMode == WeightedMode.Both;
                bool weightedIn = next.weightedMode == WeightedMode.In || next.weightedMode == WeightedMode.Both;
                float t = math.clamp((time - current.time) / timeDiff, 0f, 1f);
                if (!weightedOut && !weightedIn)
                {
                    const float oneThird = 1f / 3f;
                    Cubic unweightedY = Cubic.From(
                        current.value,
                        current.value + oneThird * current.outTangent * timeDiff,
                        next.value - oneThird * next.inTangent * timeDiff,
                        next.value);
                    return unweightedY.Evaluate(t);
                }

                float outWeight = weightedOut ? current.outWeight : (1f / 3f);
                float inWeight = weightedIn ? next.inWeight : (1f / 3f);
                outWeight = math.clamp(outWeight, 0f, 1f);
                inWeight = math.clamp(inWeight, 0f, 1f);

                Cubic x = Cubic.From(
                    current.time,
                    current.time + outWeight * timeDiff,
                    next.time - inWeight * timeDiff,
                    next.time);
                Cubic y = Cubic.From(
                    current.value,
                    current.value + outWeight * current.outTangent * timeDiff,
                    next.value - inWeight * next.inTangent * timeDiff,
                    next.value);

                float bottom = 0f;
                float top = 1f;
                const float accuracy = 1e-6f;
                for (int i = 0; i < 6; i++)
                {
                    float xAtT = x.Evaluate(t);
                    float difference = xAtT - time;
                    if (math.abs(difference) <= accuracy) return y.Evaluate(t);
                    if (difference < 0f) bottom = t;
                    else top = t;

                    float derivative = x.Derivative(t);
                    if (math.abs(derivative) <= 1e-7f) break;
                    float nextT = t - difference / derivative;
                    if (!(nextT > bottom && nextT < top)) break;
                    t = nextT;
                }

                for (int i = 0; i < 12; i++)
                {
                    float xAtT = x.Evaluate(t);
                    if (math.abs(xAtT - time) <= accuracy) break;
                    if (xAtT < time) bottom = t;
                    else top = t;
                    t = (bottom + top) * 0.5f;
                }
                return y.Evaluate(t);
            }

            private static float WrapTime(float time, float first, float last, WrapMode pre, WrapMode post)
            {
                float length = last - first;
                if (!(length > 0f)) return first;

                if (pre == WrapMode.Loop && time <= first)
                {
                    return time == first ? last : first + Repeat(time - first, length);
                }
                if (time < first)
                {
                    if (pre == WrapMode.PingPong) return first + PingPong(time - first, length);
                    return first;
                }

                if (post == WrapMode.Loop && time >= last)
                {
                    return first + Repeat(time - first, length);
                }
                if (time > last)
                {
                    if (post == WrapMode.PingPong) return first + PingPong(time - first, length);
                    return last;
                }
                return time;
            }

            private static float Repeat(float time, float length) =>
                time - math.floor(time / length) * length;

            private static float PingPong(float time, float length)
            {
                float repeated = Repeat(time, length * 2f);
                return length - math.abs(repeated - length);
            }
        }
    }
}

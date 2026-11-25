using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        private static float ApplyEase(MotionEase ease, float value)
        {
            if (ease == MotionEase.Linear) return value;
            return ease switch
            {
                MotionEase.InSine => EvaluateInSine(value),
                MotionEase.OutSine => EvaluateOutSine(value),
                MotionEase.InOutSine => EvaluateInOutSine(value),
                MotionEase.InQuad => value * value,
                MotionEase.OutQuad => value * (2f - value),
                MotionEase.InOutQuad => EvaluateInOutQuad(value),
                MotionEase.InCubic => value * value * value,
                MotionEase.OutCubic => EvaluateOutCubic(value),
                MotionEase.InOutCubic => EvaluateInOutCubic(value),
                MotionEase.InQuart => EvaluateInQuart(value),
                MotionEase.OutQuart => EvaluateOutQuart(value),
                MotionEase.InOutQuart => EvaluateInOutQuart(value),
                MotionEase.InQuint => EvaluateInQuint(value),
                MotionEase.OutQuint => EvaluateOutQuint(value),
                MotionEase.InOutQuint => EvaluateInOutQuint(value),
                MotionEase.InExpo => EvaluateInExpo(value),
                MotionEase.OutExpo => EvaluateOutExpo(value),
                MotionEase.InOutExpo => EvaluateInOutExpo(value),
                MotionEase.InCirc => EvaluateInCirc(value),
                MotionEase.OutCirc => EvaluateOutCirc(value),
                MotionEase.InOutCirc => EvaluateInOutCirc(value),
                MotionEase.InElastic => EvaluateInElastic(value),
                MotionEase.OutElastic => EvaluateOutElastic(value),
                MotionEase.InOutElastic => EvaluateInOutElastic(value),
                MotionEase.InBack => EvaluateInBack(value),
                MotionEase.OutBack => EvaluateOutBack(value),
                MotionEase.InOutBack => EvaluateInOutBack(value),
                MotionEase.InBounce => 1f - EvaluateOutBounce(1f - value),
                MotionEase.OutBounce => EvaluateOutBounce(value),
                MotionEase.InOutBounce => value < 0.5f
                    ? (1f - EvaluateOutBounce(1f - 2f * value)) / 2f
                    : (1f + EvaluateOutBounce(2f * value - 1f)) / 2f,
                _ => value
            };
        }

        private static float EvaluateInExpo(float value)
        {
            if (value == 0f) return 0f;
            float exponent = 10f * value - 10f;
            int wholeExponent = (int)math.floor(exponent);
            float fraction = exponent - wholeExponent;
            float polynomial = 1f + fraction * (0.6931547523f + fraction * (0.2401407659f + fraction * (0.05586328357f + fraction * (0.008946214803f + fraction * 0.001895107329f))));
            float scale = math.asfloat((wholeExponent + 127) << 23);
            return scale * polynomial;
        }

        private static float EvaluateOutExpo(float value)
        {
            if (value == 1f) return 1f;
            float exponent = -10f * value;
            int wholeExponent = (int)math.floor(exponent);
            float fraction = exponent - wholeExponent;
            float polynomial = 1f + fraction * (0.6931547523f + fraction * (0.2401407659f + fraction * (0.05586328357f + fraction * (0.008946214803f + fraction * 0.001895107329f))));
            float scale = math.asfloat((wholeExponent + 127) << 23);
            return 1f - scale * polynomial;
        }

        private static float EvaluateInOutExpo(float value)
        {
            switch (value)
            {
                case 0f:
                    return 0f;
                case 1f:
                    return 1f;
                case < 0.5f:
                    {
                        float exponent = 20f * value - 11f;
                        int wholeExponent = (int)math.floor(exponent);
                        float fraction = exponent - wholeExponent;
                        float polynomial = 1f + fraction * (0.6931547523f + fraction * (0.2401407659f + fraction * (0.05586328357f + fraction * (0.008946214803f + fraction * 0.001895107329f))));
                        float scale = math.asfloat((wholeExponent + 127) << 23);
                        return scale * polynomial;
                    }
            }

            float rightExponent = 9f - 20f * value;
            int rightWholeExponent = (int)math.floor(rightExponent);
            float rightFraction = rightExponent - rightWholeExponent;
            float rightPolynomial = 1f + rightFraction * (0.6931547523f + rightFraction * (0.2401407659f + rightFraction * (0.05586328357f + rightFraction * (0.008946214803f + rightFraction * 0.001895107329f))));
            float rightScale = math.asfloat((rightWholeExponent + 127) << 23);
            return 1f - rightScale * rightPolynomial;
        }

        private static float EvaluateExp2Polynomial(float exponent)
        {
            int wholeExponent = (int)math.floor(exponent);
            float fraction = exponent - wholeExponent;
            float polynomial = 1f + fraction * (0.6931547523f + fraction * (0.2401407659f + fraction * (0.05586328357f + fraction * (0.008946214803f + fraction * 0.001895107329f))));
            float scale = math.asfloat((wholeExponent + 127) << 23);
            return scale * polynomial;
        }

        private static float EvaluateInOutQuad(float value)
        {
            if (value < 0.5f) return 2f * value * value;
            float inverse = 1f - value;
            float inverseSquared = inverse * inverse;
            return 1f - 2f * inverseSquared;
        }

        private static float EvaluateOutCubic(float value)
        {
            float inverse = 1f - value;
            float inverseSquared = inverse * inverse;
            return 1f - inverseSquared * inverse;
        }

        private static float EvaluateInQuart(float value)
        {
            float squared = value * value;
            return squared * squared;
        }

        private static float EvaluateOutQuart(float value)
        {
            float inverse = 1f - value;
            float inverseSquared = inverse * inverse;
            return 1f - inverseSquared * inverseSquared;
        }

        private static float EvaluateInQuint(float value)
        {
            float squared = value * value;
            float fourth = squared * squared;
            return fourth * value;
        }

        private static float EvaluateOutQuint(float value)
        {
            float inverse = 1f - value;
            float squared = inverse * inverse;
            float fourth = squared * squared;
            return 1f - fourth * inverse;
        }

        private static float EvaluateInOutCubic(float value)
        {
            if (value < 0.5f) return 4f * value * value * value;
            float inverse = 1f - value;
            float inverseSquared = inverse * inverse;
            return 1f - 4f * inverseSquared * inverse;
        }

        private static float EvaluateInOutQuart(float value)
        {
            if (value < 0.5f)
            {
                float squared = value * value;
                return 8f * squared * squared;
            }
            float inverse = 1f - value;
            float inverseSquared = inverse * inverse;
            return 1f - 8f * inverseSquared * inverseSquared;
        }

        private static float EvaluateInOutQuint(float value)
        {
            if (value < 0.5f)
            {
                float squared = value * value;
                float fourth = squared * squared;
                return 16f * fourth * value;
            }
            float inverse = 1f - value;
            float inverseSquared = inverse * inverse;
            float inverseFourth = inverseSquared * inverseSquared;
            return 1f - 16f * inverseFourth * inverse;
        }

        private static float EvaluateInCirc(float value)
        {
            return value switch
            {
                0f => 0f,
                1f => 1f,
                _ => 1f - math.sqrt(1f - value * value)
            };
        }

        private static float EvaluateOutCirc(float value)
        {
            switch (value)
            {
                case 0f:
                    return 0f;
                case 1f:
                    return 1f;
                default:
                    {
                        float shifted = value - 1f;
                        return math.sqrt(1f - shifted * shifted);
                    }
            }
        }

        private static float EvaluateInOutCirc(float value)
        {
            switch (value)
            {
                case 0f:
                    return 0f;
                case 1f:
                    return 1f;
                case < 0.5f:
                    {
                        float doubled = 2f * value;
                        return (1f - math.sqrt(1f - doubled * doubled)) * 0.5f;
                    }
                default:
                    {
                        float shifted = 2f * value - 2f;
                        return (math.sqrt(1f - shifted * shifted) + 1f) * 0.5f;
                    }
            }
        }

        private static float EvaluateInSine(float value)
        {
            float squared = value * value;
            float remainder = 0.00085819951228667589f;
            remainder = remainder * squared - 0.019953231815016573f;
            remainder = remainder * squared + 0.23369870257974015f;
            float complement = (1f - squared) * (1f - squared * remainder);
            return 1f - complement;
        }

        private static float EvaluateOutSine(float value)
        {
            float z = 2f * value - 1f;
            float q = 0.0008987976470962167f;
            q = q * z - 0.00685570714995265f;
            q = q * z - 0.04392676055431366f;
            q = q * z + 0.22142855823040009f;
            q = q * z + 0.8284269571304321f;
            float weight = value * (1f - value);
            return value + weight * q;
        }

        private static float EvaluateInOutSine(float value)
        {
            float u = 2f * value - 1f;
            float squared = u * u;
            float q = 1.1415716097662956f + squared * (-0.15012121305838458f + squared * 0.00854960329208908f);
            return value + ((u * value) * (1f - value)) * q;
        }

        private static float EvaluateInBack(float value)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float squared = value * value;
            return squared * (c3 * value - c1);
        }

        private static float EvaluateOutBack(float value)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float shifted = value - 1f;
            float squared = shifted * shifted;
            return 1f + c3 * squared * shifted + c1 * squared;
        }

        private static float EvaluateInOutBack(float value)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            if (value < 0.5f)
            {
                float doubled = 2f * value;
                return doubled * doubled * ((c2 + 1f) * doubled - c2) / 2f;
            }
            float shifted = 2f * value - 2f;
            return (shifted * shifted * ((c2 + 1f) * shifted + c2) + 2f) / 2f;
        }

        private static float EvaluateElasticSine(float angle)
        {
            return math.sin(angle);
        }

        private static float EvaluateInElastic(float value)
        {
            const float c4 = 2f * math.PI / 3f;
            float phase = 10f * value;
            return value switch
            {
                0f => 0f,
                1f => 1f,
                _ => -EvaluateExp2Polynomial(phase - 10f) * EvaluateElasticSine((phase - 10.75f) * c4)
            };
        }

        private static float EvaluateOutElastic(float value)
        {
            const float c4 = 2f * math.PI / 3f;
            float phase = 10f * value;
            return value switch
            {
                0f => 0f,
                1f => 1f,
                _ => EvaluateExp2Polynomial(-phase) * EvaluateElasticSine((phase - 0.75f) * c4) + 1f
            };
        }

        private static float EvaluateInOutElastic(float value)
        {
            const float c5 = 2f * math.PI / 4.5f;
            float phase = 20f * value;
            return value == 0f ? 0f : value == 1f ? 1f : value < 0.5f
                ? -0.5f * EvaluateExp2Polynomial(phase - 10f) * EvaluateElasticSine((phase - 11.125f) * c5)
                : 0.5f * EvaluateExp2Polynomial(10f - phase) * EvaluateElasticSine((phase - 11.125f) * c5) + 1f;
        }

        private static float EvaluateOutBounce(float value)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            switch (value)
            {
                case < 1f / d1:
                    return n1 * value * value;
                case < 2f / d1:
                    value -= 1.5f / d1;
                    return n1 * value * value + 0.75f;
                case < 2.5f / d1:
                    value -= 2.25f / d1;
                    return n1 * value * value + 0.9375f;
                default:
                    value -= 2.625f / d1;
                    return n1 * value * value + 0.984375f;
            }
        }
    }
}

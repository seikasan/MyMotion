using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        private static unsafe void SetValue(
            ref Data item,
            float progress,
            bool useOnlyFloatPath,
            bool useOnlyVector3Path,
            NativeArray<WideData> wideItems)
        {
            if (useOnlyFloatPath)
            {
                item.Value.x = math.lerp(item.Start.x, item.End.x, progress);
                return;
            }

            if (useOnlyVector3Path)
            {
                item.Value = math.lerp(item.Start, item.End, progress);
                return;
            }

            if (item.ValueType == MotionValueType.Float)
            {
                item.Value.x = math.lerp(item.Start.x, item.End.x, progress);
                return;
            }

            if (item.ValueType == MotionValueType.Vector3)
            {
                item.Value = math.lerp(item.Start, item.End, progress);
                return;
            }

            WideData* wide = (WideData*)wideItems.GetUnsafePtr() + item.WideIndex;
            switch (item.ValueType)
            {
                case MotionValueType.Double:
                    wide->Value.Double = math.lerp(wide->Start.Double, wide->End.Double, (double)progress);
                    break;
                case MotionValueType.Int:
                    float intValue = math.lerp((float)wide->Start.Integer, (float)wide->End.Integer, progress);
                    wide->Value.Integer = RoundToInt(intValue, item.RoundingMode);
                    break;
                case MotionValueType.Long:
                    double longValue = math.lerp((double)wide->Start.Integer, (double)wide->End.Integer, (double)progress);
                    wide->Value.Integer = RoundToLong(longValue, item.RoundingMode);
                    break;
                case MotionValueType.Quaternion:
                    quaternion rotation = math.nlerp(new quaternion(wide->Start.Floats), new quaternion(wide->End.Floats), progress);
                    wide->Value.Floats = rotation.value;
                    break;
                case MotionValueType.Vector2:
                case MotionValueType.Vector4:
                case MotionValueType.Color:
                case MotionValueType.Rect:
                    wide->Value.Floats = math.lerp(wide->Start.Floats, wide->End.Floats, progress);
                    break;
                default:
                    wide->Value = default;
                    break;
            }
        }

        private static int RoundToInt(float value, MotionRoundingMode roundingMode)
        {
            return roundingMode switch
            {
                MotionRoundingMode.AwayFromZero => value >= 0f ? (int)math.ceil(value) : (int)math.floor(value),
                MotionRoundingMode.ToZero => (int)math.trunc(value),
                MotionRoundingMode.ToPositiveInfinity => (int)math.ceil(value),
                MotionRoundingMode.ToNegativeInfinity => (int)math.floor(value),
                _ => (int)math.round(value)
            };
        }

        private static long RoundToLong(double value, MotionRoundingMode roundingMode)
        {
            return roundingMode switch
            {
                MotionRoundingMode.AwayFromZero => value >= 0.0 ? (long)math.ceil(value) : (long)math.floor(value),
                MotionRoundingMode.ToZero => (long)math.trunc(value),
                MotionRoundingMode.ToPositiveInfinity => (long)math.ceil(value),
                MotionRoundingMode.ToNegativeInfinity => (long)math.floor(value),
                _ => (long)math.round(value)
            };
        }
    }
}

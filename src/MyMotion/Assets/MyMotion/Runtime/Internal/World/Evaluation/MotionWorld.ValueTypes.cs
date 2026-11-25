using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        internal enum MotionValueType : byte
        {
            Float,
            Double,
            Int,
            Long,
            Vector2,
            Vector3,
            Vector4,
            Quaternion,
            Color,
            Rect
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        internal struct MotionValue
        {
            [FieldOffset(0)] public float4 Floats;
            [FieldOffset(0)] public double Double;
            [FieldOffset(0)] public long Integer;
        }

        internal struct WideData
        {
            public MotionValue Start;
            public MotionValue End;
            public MotionValue Value;
            public int Slot;
        }

        internal unsafe struct CustomEaseCurveData
        {
            public Keyframe* Keys;
            public int Length;
            public WrapMode PreWrapMode;
            public WrapMode PostWrapMode;
            public float EndValue;
            public bool IsAssigned;
        }
    }
}

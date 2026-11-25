using Unity.Mathematics;
using UnityEngine;
using MyMotion.Adapters;

namespace MyMotion.Adapters
{
    // LitMotion と同じ名前の組み込み Adapter。
    // MotionBuilder<TValue, TOptions, TAdapter> の型引数として使うと、Adapter storage ではなく
    // 組み込み値型の高速経路で実行される。Evaluate は汎用コードから直接呼ばれた場合のための実装。

    public readonly struct FloatMotionAdapter : IMotionAdapter<float, NoOptions>
    {
        public float Evaluate(ref float startValue, ref float endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            math.lerp(startValue, endValue, context.Progress);
    }

    public readonly struct DoubleMotionAdapter : IMotionAdapter<double, NoOptions>
    {
        public double Evaluate(ref double startValue, ref double endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            math.lerp(startValue, endValue, context.Progress);
    }

    public readonly struct IntMotionAdapter : IMotionAdapter<int, IntegerOptions>
    {
        public int Evaluate(ref int startValue, ref int endValue, ref IntegerOptions options, in MotionEvaluationContext context)
        {
            float value = math.lerp(startValue, endValue, context.Progress);
            return options.RoundingMode switch
            {
                RoundingMode.AwayFromZero => value >= 0f ? (int)math.ceil(value) : (int)math.floor(value),
                RoundingMode.ToZero => (int)math.trunc(value),
                RoundingMode.ToPositiveInfinity => (int)math.ceil(value),
                RoundingMode.ToNegativeInfinity => (int)math.floor(value),
                _ => (int)math.round(value),
            };
        }
    }

    public readonly struct LongMotionAdapter : IMotionAdapter<long, IntegerOptions>
    {
        public long Evaluate(ref long startValue, ref long endValue, ref IntegerOptions options, in MotionEvaluationContext context)
        {
            double value = math.lerp((double)startValue, endValue, context.Progress);
            return options.RoundingMode switch
            {
                RoundingMode.AwayFromZero => value >= 0d ? (long)math.ceil(value) : (long)math.floor(value),
                RoundingMode.ToZero => (long)math.trunc(value),
                RoundingMode.ToPositiveInfinity => (long)math.ceil(value),
                RoundingMode.ToNegativeInfinity => (long)math.floor(value),
                _ => (long)math.round(value),
            };
        }
    }

    public readonly struct Vector2MotionAdapter : IMotionAdapter<Vector2, NoOptions>
    {
        public Vector2 Evaluate(ref Vector2 startValue, ref Vector2 endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            Vector2.LerpUnclamped(startValue, endValue, context.Progress);
    }

    public readonly struct Vector3MotionAdapter : IMotionAdapter<Vector3, NoOptions>
    {
        public Vector3 Evaluate(ref Vector3 startValue, ref Vector3 endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            Vector3.LerpUnclamped(startValue, endValue, context.Progress);
    }

    public readonly struct Vector4MotionAdapter : IMotionAdapter<Vector4, NoOptions>
    {
        public Vector4 Evaluate(ref Vector4 startValue, ref Vector4 endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            Vector4.LerpUnclamped(startValue, endValue, context.Progress);
    }

    public readonly struct QuaternionMotionAdapter : IMotionAdapter<Quaternion, NoOptions>
    {
        public Quaternion Evaluate(ref Quaternion startValue, ref Quaternion endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            Quaternion.LerpUnclamped(startValue, endValue, context.Progress);
    }

    public readonly struct ColorMotionAdapter : IMotionAdapter<Color, NoOptions>
    {
        public Color Evaluate(ref Color startValue, ref Color endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            Color.LerpUnclamped(startValue, endValue, context.Progress);
    }

    public readonly struct RectMotionAdapter : IMotionAdapter<Rect, NoOptions>
    {
        public Rect Evaluate(ref Rect startValue, ref Rect endValue, ref NoOptions options, in MotionEvaluationContext context) =>
            new(
                math.lerp(startValue.x, endValue.x, context.Progress),
                math.lerp(startValue.y, endValue.y, context.Progress),
                math.lerp(startValue.width, endValue.width, context.Progress),
                math.lerp(startValue.height, endValue.height, context.Progress));
    }
}

namespace MyMotion
{
    /// <summary>
    /// 組み込み Adapter 型から、組み込み値型の factory を引く。型ごとに一度だけ判定する。
    /// </summary>
    internal static class BuiltInMotionAdapter<TValue, TAdapter>
    {
        // 型引数の組ごとに別の値を持たせるための static field (意図した使い方)。
        // ReSharper disable StaticMemberInGenericType
        public static readonly bool IsBuiltIn;
        public static readonly MotionFactory<TValue> Factory;
        // ReSharper restore StaticMemberInGenericType

        static BuiltInMotionAdapter()
        {
            if (Resolve() is MotionFactory<TValue> factory)
            {
                Factory = factory;
                IsBuiltIn = true;
            }
        }

        private static object Resolve()
        {
            System.Type adapter = typeof(TAdapter);
            object factory =
                adapter == typeof(FloatMotionAdapter) ? MotionFactories.Float :
                adapter == typeof(DoubleMotionAdapter) ? MotionFactories.Double :
                adapter == typeof(IntMotionAdapter) ? MotionFactories.Int :
                adapter == typeof(LongMotionAdapter) ? MotionFactories.Long :
                adapter == typeof(Vector2MotionAdapter) ? MotionFactories.Vector2 :
                adapter == typeof(Vector3MotionAdapter) ? MotionFactories.Vector3 :
                adapter == typeof(Vector4MotionAdapter) ? MotionFactories.Vector4 :
                adapter == typeof(QuaternionMotionAdapter) ? MotionFactories.Quaternion :
                adapter == typeof(ColorMotionAdapter) ? MotionFactories.Color :
                adapter == typeof(RectMotionAdapter) ? MotionFactories.Rect :
                null;
            return factory;
        }
    }
}

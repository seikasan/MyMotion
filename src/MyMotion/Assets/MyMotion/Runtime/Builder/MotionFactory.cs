using System;
using MyMotion.Internal;

namespace MyMotion
{
    internal delegate CoreMotionHandle CoreCreate<TValue>(MotionWorld world, TValue start, TValue end, in MotionBuilderOptions options, Action<TValue> binding);
    internal delegate CoreMotionHandle CoreCreateWithState<in TValue>(MotionWorld world, TValue start, TValue end, in MotionBuilderOptions options, byte stateCount, object state0, object state1, object state2, object binding);

    // builder を値渡しで何度も複製するため、delegate 2つを持つ struct ではなく参照1つで済む class にしている。
    internal sealed class MotionFactory<TValue>
    {
        public readonly CoreCreate<TValue> Create;
        public readonly CoreCreateWithState<TValue> CreateWithState;

        /// <summary>組み込み型の factory。builder はこれと同じ factory を field に持たず、null のままにする。</summary>
        internal static MotionFactory<TValue> Default;

        public MotionFactory(CoreCreate<TValue> create, CoreCreateWithState<TValue> createWithState)
        {
            Create = create;
            CreateWithState = createWithState;
        }
    }

    internal static class MotionFactories
    {
        public static readonly MotionFactory<float> Float = new(CreateFloat, CreateFloatWithState);
        public static readonly MotionFactory<double> Double = new(CreateDouble, CreateDoubleWithState);
        public static readonly MotionFactory<int> Int = new(CreateInt, CreateIntWithState);
        public static readonly MotionFactory<long> Long = new(CreateLong, CreateLongWithState);
        public static readonly MotionFactory<UnityEngine.Vector2> Vector2 = new(CreateVector2, CreateVector2WithState);
        public static readonly MotionFactory<UnityEngine.Vector3> Vector3 = new(CreateVector3, CreateVector3WithState);
        public static readonly MotionFactory<UnityEngine.Vector4> Vector4 = new(CreateVector4, CreateVector4WithState);
        public static readonly MotionFactory<UnityEngine.Quaternion> Quaternion = new(CreateQuaternion, CreateQuaternionWithState);
        public static readonly MotionFactory<UnityEngine.Color> Color = new(CreateColor, CreateColorWithState);
        public static readonly MotionFactory<UnityEngine.Rect> Rect = new(CreateRect, CreateRectWithState);

        static MotionFactories()
        {
            MotionFactory<float>.Default = Float;
            MotionFactory<double>.Default = Double;
            MotionFactory<int>.Default = Int;
            MotionFactory<long>.Default = Long;
            MotionFactory<UnityEngine.Vector2>.Default = Vector2;
            MotionFactory<UnityEngine.Vector3>.Default = Vector3;
            MotionFactory<UnityEngine.Vector4>.Default = Vector4;
            MotionFactory<UnityEngine.Quaternion>.Default = Quaternion;
            MotionFactory<UnityEngine.Color>.Default = Color;
            MotionFactory<UnityEngine.Rect>.Default = Rect;
        }

        private static CoreMotionHandle CreateFloat(MotionWorld w, float a, float b, in MotionBuilderOptions o, Action<float> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateFloatWithState(MotionWorld w, float a, float b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateDouble(MotionWorld w, double a, double b, in MotionBuilderOptions o, Action<double> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateDoubleWithState(MotionWorld w, double a, double b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateInt(MotionWorld w, int a, int b, in MotionBuilderOptions o, Action<int> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError, (Internal.MotionRoundingMode)o.RoundingMode);
        private static CoreMotionHandle CreateIntWithState(MotionWorld w, int a, int b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError, (Internal.MotionRoundingMode)o.RoundingMode);
        private static CoreMotionHandle CreateLong(MotionWorld w, long a, long b, in MotionBuilderOptions o, Action<long> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError, (Internal.MotionRoundingMode)o.RoundingMode);
        private static CoreMotionHandle CreateLongWithState(MotionWorld w, long a, long b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError, (Internal.MotionRoundingMode)o.RoundingMode);
        private static CoreMotionHandle CreateVector2(MotionWorld w, UnityEngine.Vector2 a, UnityEngine.Vector2 b, in MotionBuilderOptions o, Action<UnityEngine.Vector2> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateVector2WithState(MotionWorld w, UnityEngine.Vector2 a, UnityEngine.Vector2 b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateVector3(MotionWorld w, UnityEngine.Vector3 a, UnityEngine.Vector3 b, in MotionBuilderOptions o, Action<UnityEngine.Vector3> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateVector3WithState(MotionWorld w, UnityEngine.Vector3 a, UnityEngine.Vector3 b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateVector4(MotionWorld w, UnityEngine.Vector4 a, UnityEngine.Vector4 b, in MotionBuilderOptions o, Action<UnityEngine.Vector4> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateVector4WithState(MotionWorld w, UnityEngine.Vector4 a, UnityEngine.Vector4 b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateQuaternion(MotionWorld w, UnityEngine.Quaternion a, UnityEngine.Quaternion b, in MotionBuilderOptions o, Action<UnityEngine.Quaternion> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateQuaternionWithState(MotionWorld w, UnityEngine.Quaternion a, UnityEngine.Quaternion b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateColor(MotionWorld w, UnityEngine.Color a, UnityEngine.Color b, in MotionBuilderOptions o, Action<UnityEngine.Color> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateColorWithState(MotionWorld w, UnityEngine.Color a, UnityEngine.Color b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateRect(MotionWorld w, UnityEngine.Rect a, UnityEngine.Rect b, in MotionBuilderOptions o, Action<UnityEngine.Rect> cb) => w.Create(a, b, o.Duration, (MotionEase)o.Ease, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
        private static CoreMotionHandle CreateRectWithState(MotionWorld w, UnityEngine.Rect a, UnityEngine.Rect b, in MotionBuilderOptions o, byte count, object s0, object s1, object s2, object cb) => w.CreateWithStates(a, b, o.Duration, (MotionEase)o.Ease, count, s0, s1, s2, cb, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
    }
}

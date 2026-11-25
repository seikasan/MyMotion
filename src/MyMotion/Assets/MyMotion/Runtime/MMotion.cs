using UnityEngine;

namespace MyMotion
{
    public static partial class MMotion
    {
        internal static Internal.MotionWorld World => MotionScheduler.GetWorld(PlayerLoopTiming.Update);

        public static int ActiveCount => MotionScheduler.GetActiveCount();

        public static long ReservedNativeBytes => MotionScheduler.GetReservedNativeBytes();

        public static MotionBuilder<float> Create(float start, float end, float duration) => new(null, start, end, duration, MotionFactories.Float);
        public static MotionBuilder<double> Create(double start, double end, float duration) => new(null, start, end, duration, MotionFactories.Double);
        public static MotionBuilder<int> Create(int start, int end, float duration) => new(null, start, end, duration, MotionFactories.Int);
        public static MotionBuilder<long> Create(long start, long end, float duration) => new(null, start, end, duration, MotionFactories.Long);
        public static MotionBuilder<Vector2> Create(Vector2 start, Vector2 end, float duration) => new(null, start, end, duration, MotionFactories.Vector2);
        public static MotionBuilder<Vector3> Create(Vector3 start, Vector3 end, float duration) => new(null, start, end, duration, MotionFactories.Vector3);
        public static MotionBuilder<Vector4> Create(Vector4 start, Vector4 end, float duration) => new(null, start, end, duration, MotionFactories.Vector4);
        public static MotionBuilder<Quaternion> Create(Quaternion start, Quaternion end, float duration) => new(null, start, end, duration, MotionFactories.Quaternion);
        public static MotionBuilder<Color> Create(Color start, Color end, float duration) => new(null, start, end, duration, MotionFactories.Color);
        public static MotionBuilder<Rect> Create(Rect start, Rect end, float duration) => new(null, start, end, duration, MotionFactories.Rect);
        public static MotionBuilder<TValue, TOptions, TAdapter> Create<TValue, TOptions, TAdapter>(
            in TValue start,
            in TValue end,
            float duration)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions> =>
            new(start, end, duration);

        public static void Update(double deltaTime) => MotionScheduler.UpdateDefaultWorld(deltaTime, deltaTime, deltaTime);

        public static void Update(double deltaTime, double unscaledDeltaTime, double realtimeDeltaTime) => MotionScheduler.UpdateDefaultWorld(deltaTime, unscaledDeltaTime, realtimeDeltaTime);

        internal static void Reset()
        {
            MotionScheduler.Reset();
            ManualMotionDispatcher.ResetDefault();
        }

        /// <summary>
        /// PlayerLoop の World と、すべての ManualMotionDispatcher が持つネイティブメモリを解放する。
        /// 終了時と assembly の再読み込み前に呼ぶ。次に motion を作ると World は作り直される。
        /// </summary>
        internal static void ReleaseNativeMemory()
        {
            MotionScheduler.Reset();
            ManualMotionDispatcher.ReleaseAllWorlds();
        }
    }
}

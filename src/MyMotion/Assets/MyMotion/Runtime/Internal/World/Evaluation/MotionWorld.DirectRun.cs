using Unity.Burst;

namespace MyMotion.Internal
{
    // 件数が少ないとき、IJobParallelForBatch.RunBatch は Job system の登録と引数の受け渡しを経由するため、
    // 計算そのものより固定費が目立つ。Burst の direct call で Execute を直接呼び、その固定費を省く。
    // direct call の引数は blittable でなければならない (bool を含む Job は ref で渡せない) ため、ポインタで渡す。
    [BurstCompile]
    internal static class MotionWorldDirectUpdate
    {
        [BurstCompile(
            CompileSynchronously = true,
            OptimizeFor = OptimizeFor.Performance,
            FloatPrecision = FloatPrecision.Standard,
            FloatMode = FloatMode.Strict)]
        public static unsafe void Run(MotionWorld.UpdateJob* job, int count) => job->Execute(0, count);
    }
}

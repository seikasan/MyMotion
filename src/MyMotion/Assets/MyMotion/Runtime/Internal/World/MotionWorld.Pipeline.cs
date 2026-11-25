using System;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace MyMotion.Internal
{
    /// <summary>
    /// Update Job と managed Binding の重ね合わせ。
    /// 値の計算を複数の chunk Job に分けて worker に流し、main thread は完了した chunk から順に Binding する。
    /// Job と Binding を直列に待つ従来方式では「Job 時間 + Binding 時間」かかるが、重ねると Binding の裏で計算が進む。
    ///
    /// 安全性: Binding や callback は利用者コードで、任意の handle 操作や motion 生成を行える。
    /// World の入口 (index 解決、生成、容量変更、Reset、Dispose) はすべて <see cref="CompletePipeline"/> を先に呼ぶので、
    /// 利用者コードが走っている Job と同じ要素に触れることはない。重ね合わせは早めに打ち切られるだけで、結果は変わらない。
    /// </summary>
    internal sealed partial class MotionWorld
    {
        private const int MaxPipelineChunkCount = 16;
#if MYMOTION_THRESHOLD_BENCHMARK
        private static readonly int PipelineChunkCount = GetEnvInt("MYMOTION_PIPELINE_CHUNKS", 8);
        // 1 で最初の chunk を小さくし、Binding を早く始める A/B 用。
        private static readonly bool PipelineRamp = GetEnvInt("MYMOTION_PIPELINE_RAMP", 0) != 0;
        private static readonly bool DirectTransformBinding = GetEnvInt("MYMOTION_DIRECT_TRANSFORM", 1) != 0;
        private static readonly int WidePipelineMinCount = GetEnvInt("MYMOTION_WIDE_PIPELINE_MIN", 4096);

        private static int GetEnvInt(string name, int fallback)
        {
            string value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? fallback : int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        // 0 で重ね合わせを無効にする A/B 用。
        private static readonly int PipelineMinCount = GetPipelineMinCount();

        private static int GetPipelineMinCount()
        {
            string value = Environment.GetEnvironmentVariable("MYMOTION_PIPELINE_MIN");
            if (string.IsNullOrEmpty(value)) return 8192;
            int parsed = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            return parsed <= 0 ? int.MaxValue : parsed;
        }
#else
        // 2026-11-23 の A/B（docs/measurements/update-binding-pipeline-20261123.md）を同一起動内の LitMotion 比で見ると、
        // 2,048〜6,144 件では重ね合わせが +13〜+40% 遅く、8,192 件以上では -2〜-19% 速かった。
        // 型による差は誤差の範囲を超えなかったため、値型によらず 8,192 件から使う。
        private const int PipelineMinCount = 8192;
        private const int PipelineChunkCount = 8;
        private const bool PipelineRamp = false;
        private const bool DirectTransformBinding = true;
        // 2026-11-24 の A/B で、Quaternion 4,096件は直列計算だと LitMotion 比 +43〜+55%、worker へ出すと +0〜+1% だった。
        private const int WidePipelineMinCount = 4096;
#endif

        private readonly JobHandle[] _pipelineHandles = new JobHandle[MaxPipelineChunkCount];
        private readonly int[] _pipelineChunkEnds = new int[MaxPipelineChunkCount];
        private int _pipelineChunkCount;
        private int _pipelineNextChunk;
        // 次に完了待ちが必要になる dense index。これ未満の要素は計算済み。
        private int _pipelineReadyEnd;
        private bool _pipelineActive;

        // Quaternion などの wide 値は1件の計算が重く、main thread で直列に計算すると 4,096 件付近で LitMotion の並列計算に負ける。
        // wide 値が過半の World は、より少ない件数から計算を worker へ出す。
        private bool CanPipeline(int count) =>
            count >= (_wideValueCount * 2 >= count ? WidePipelineMinCount : PipelineMinCount)
            && _adapterMotionCount == 0
            && !_transformJobMode
            && _transformEligibleCount < TransformJobEnterCount
            && JobsUtility.JobWorkerCount > 0;

        private void SchedulePipeline(UpdateJob job, int count)
        {
            int chunkCount = Math.Min(PipelineChunkCount, MaxPipelineChunkCount);
            int chunk = (count + chunkCount - 1) / chunkCount;
            chunk = (chunk + ParallelBatchSize - 1) / ParallelBatchSize * ParallelBatchSize;
            // 最初の chunk だけ小さくすると、Binding を早く始められる (A/B 用の切り替え)。
            int firstChunk = PipelineRamp ? Math.Max(ParallelBatchSize, chunk / 4 / ParallelBatchSize * ParallelBatchSize) : chunk;
            int chunks = 0;
            int start = 0;
            while (start < count)
            {
                int length = Math.Min(chunks == 0 ? firstChunk : chunk, count - start);
                if (chunks == MaxPipelineChunkCount - 1) length = count - start;
                job.IndexOffset = start;
                _pipelineHandles[chunks] = job.ScheduleBatch(length, ParallelBatchSize);
                _pipelineChunkEnds[chunks] = start + length;
                chunks++;
                start += length;
            }
            JobHandle.ScheduleBatchedJobs();
            _pipelineChunkCount = chunks;
            _pipelineNextChunk = 0;
            _pipelineReadyEnd = 0;
            _pipelineActive = true;
        }

        /// <summary>
        /// index までの計算が終わるのを待つ。Binding ループから要素ごとに呼ぶ。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WaitPipeline(int index)
        {
            if (index < _pipelineReadyEnd || !_pipelineActive) return;
            WaitPipelineSlow(index);
        }

        private void WaitPipelineSlow(int index)
        {
            while (_pipelineNextChunk < _pipelineChunkCount && index >= _pipelineReadyEnd)
            {
                _pipelineHandles[_pipelineNextChunk].Complete();
                _pipelineReadyEnd = _pipelineChunkEnds[_pipelineNextChunk];
                _pipelineNextChunk++;
            }
            if (_pipelineNextChunk >= _pipelineChunkCount) _pipelineActive = false;
        }

        /// <summary>
        /// 走っている chunk Job をすべて終わらせる。World の状態に触れる入口で必ず呼ぶ。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CompletePipeline()
        {
            if (_pipelineActive) CompletePipelineSlow();
            _bindRecordsValid = false;
        }

        private void CompletePipelineSlow()
        {
            for (int i = _pipelineNextChunk; i < _pipelineChunkCount; i++)
            {
                _pipelineHandles[i].Complete();
            }
            _pipelineNextChunk = _pipelineChunkCount;
            _pipelineReadyEnd = int.MaxValue;
            _pipelineActive = false;
        }
    }
}

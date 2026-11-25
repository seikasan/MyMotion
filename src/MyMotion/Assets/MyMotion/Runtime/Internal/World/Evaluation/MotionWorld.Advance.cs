using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        internal static void AdvanceItem(
            ref Data item,
            double deltaTime,
            double unscaledDeltaTime,
            double realDeltaTime,
            bool useOnlyFloatPath,
            bool useOnlyVector3Path,
            NativeArray<WideData> wideItems)
        {
            AdvanceItem(ref item, deltaTime, unscaledDeltaTime, realDeltaTime,
                useOnlyFloatPath, useOnlyVector3Path, wideItems, default, 0, false);
        }

        internal static void AdvanceItem(
            ref Data item,
            double deltaTime,
            double unscaledDeltaTime,
            double realDeltaTime,
            bool useOnlyFloatPath,
            bool useOnlyVector3Path,
            NativeArray<WideData> wideItems,
            NativeArray<CustomEaseCurveData> customEaseCurves,
            int index,
            bool evaluateCustomEase)
        {
            // Preserve された完了済みmotionはLitMotionと同様に時間を進め、巻き戻しで再生へ戻れるようにする。
            if (item.IsCanceled || item.IsInSequence || (item.IsCompletionNotified && !item.IsPreserved)) return;

            item.HasStarted = true;
            double selectedDeltaTime = item.TimeKind switch
            {
                MotionTimeKind.Time => deltaTime,
                MotionTimeKind.UnscaledTime => unscaledDeltaTime,
                MotionTimeKind.Realtime => realDeltaTime,
                _ => 0.0
            };
            item.Elapsed += selectedDeltaTime * item.PlaybackSpeed;
            Evaluate(ref item, useOnlyFloatPath, useOnlyVector3Path, wideItems,
                customEaseCurves, index, evaluateCustomEase);
        }

        internal static void AdvanceAdapterItem(
            ref Data item,
            double deltaTime,
            double unscaledDeltaTime,
            double realDeltaTime)
        {
            // Preserve された完了済みmotionはLitMotionと同様に時間を進め、巻き戻しで再生へ戻れるようにする。
            if (item.IsCanceled || item.IsInSequence || (item.IsCompletionNotified && !item.IsPreserved)) return;

            item.HasStarted = true;
            double selectedDeltaTime = item.TimeKind switch
            {
                MotionTimeKind.Time => deltaTime,
                MotionTimeKind.UnscaledTime => unscaledDeltaTime,
                MotionTimeKind.Realtime => realDeltaTime,
                _ => 0.0
            };
            item.Elapsed += selectedDeltaTime * item.PlaybackSpeed;
            EvaluateAdapterItem(ref item);
        }

        [BurstCompile(
            CompileSynchronously = true,
            OptimizeFor = OptimizeFor.Performance,
            FloatPrecision = FloatPrecision.Standard,
            FloatMode = FloatMode.Strict)]
        internal unsafe struct UpdateJob : IJobParallelForBatch
        {
            // 重ね合わせ中も main thread が完了済み chunk の要素を読み書きするため、container の安全性追跡を外す。
            // 同じ要素へ同時に触れないことは MotionWorld.Pipeline の barrier で保証する。
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Data> Items;
            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeArray<WideData> WideItems;
            [ReadOnly, NativeDisableContainerSafetyRestriction]
            public NativeArray<CustomEaseCurveData> CustomEaseCurves;
            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeArray<BindRecord> Records;
            public bool WriteRecords;
            public int IndexOffset;
            public double DeltaTime;
            public double UnscaledDeltaTime;
            public double RealDeltaTime;
            public bool UseOnlyFloatPath;
            public bool UseOnlyVector3Path;

            public void Execute(int startIndex, int count)
            {
                var items = (Data*)Items.GetUnsafePtr();
                startIndex += IndexOffset;
                int endIndex = startIndex + count;
                for (int index = startIndex; index < endIndex; index++)
                {
                    AdvanceItem(
                        ref items[index],
                        DeltaTime,
                        UnscaledDeltaTime,
                        RealDeltaTime,
                        UseOnlyFloatPath,
                        UseOnlyVector3Path,
                        WideItems,
                        CustomEaseCurves,
                        index,
                        true);
                }
                if (WriteRecords)
                {
                    var records = (BindRecord*)Records.GetUnsafePtr();
                    var wide = (WideData*)WideItems.GetUnsafeReadOnlyPtr();
                    for (int index = startIndex; index < endIndex; index++)
                    {
                        WriteBindRecord(ref items[index], records, wide, index);
                    }
                }
            }
        }

        [BurstCompile(
            CompileSynchronously = true,
            OptimizeFor = OptimizeFor.Performance,
            FloatPrecision = FloatPrecision.Standard,
            FloatMode = FloatMode.Strict)]
        private unsafe struct UpdateMixedJob : IJobParallelForBatch
        {
            public NativeArray<Data> Items;
            [NativeDisableParallelForRestriction]
            public NativeArray<WideData> WideItems;
            [ReadOnly]
            public NativeArray<CustomEaseCurveData> CustomEaseCurves;
            public double DeltaTime;
            public double UnscaledDeltaTime;
            public double RealDeltaTime;
            public bool UseOnlyFloatPath;
            public bool UseOnlyVector3Path;

            public void Execute(int startIndex, int count)
            {
                var items = (Data*)Items.GetUnsafePtr();
                int endIndex = startIndex + count;
                for (int index = startIndex; index < endIndex; index++)
                {
                    if (items[index].IsAdapter) continue;
                    AdvanceItem(
                        ref items[index],
                        DeltaTime,
                        UnscaledDeltaTime,
                        RealDeltaTime,
                        UseOnlyFloatPath,
                        UseOnlyVector3Path,
                        WideItems,
                        CustomEaseCurves,
                        index,
                        true);
                }
            }
        }
    }
}

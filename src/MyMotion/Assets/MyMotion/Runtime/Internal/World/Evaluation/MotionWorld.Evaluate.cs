using Unity.Collections;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        internal static void Evaluate(
            ref Data item,
            bool useOnlyFloatPath,
            bool useOnlyVector3Path,
            NativeArray<WideData> wideItems,
            NativeArray<CustomEaseCurveData> customEaseCurves,
            int index,
            bool evaluateCustomEase,
            bool forceValueDuringDelay = false)
        {
            if (item.Duration <= 0f)
            {
                bool isComplete;
                int completedLoops;
                if (!item.DelayEveryLoop || item.Delay == 0f)
                {
                    isComplete = item.Loops >= 0 && item.Elapsed - item.Delay > 0.0;
                    completedLoops = isComplete ? item.Loops : item.Elapsed - item.Delay < 0.0 ? -1 : 0;
                    item.IsDelayed = item.Elapsed - item.Delay < 0.0;
                }
                else
                {
                    completedLoops = (int)math.floor(item.Elapsed / item.Delay);
                    completedLoops = item.Loops < 0 ? math.max(0, completedLoops) : math.clamp(completedLoops, 0, item.Loops);
                    isComplete = item.Loops >= 0 && completedLoops > item.Loops - 1;
                    item.IsDelayed = !isComplete;
                }

                float progress = isComplete ? 1f : 0f;

                item.PreviousCompletedLoops = item.CompletedLoops;
                item.CompletedLoops = (ushort)(item.Loops < 0 ? math.max(0, completedLoops) : math.clamp(completedLoops, 0, item.Loops));
                item.IsComplete = isComplete;

                if (!item.HasBinding || (item.IsDelayed && item.SkipValuesDuringDelay && !forceValueDuringDelay)) return;

                if (item.Ease == MotionEase.CustomAnimationCurve)
                {
                    if (evaluateCustomEase)
                    {
                        float easedProgress = EvaluateCustomEaseProgress(item, customEaseCurves[index], progress);
                        SetValue(ref item, easedProgress, useOnlyFloatPath, useOnlyVector3Path, wideItems);
                    }
                    else
                    {
                        item.Value = new float3(progress, 0f, 0f);
                    }
                    return;
                }

                bool isOdd = ((item.CompletedLoops + (int)progress) & 1) != 0;

                switch (item.LoopType)
                {
                    case MotionLoopType.Yoyo when isOdd:
                    case MotionLoopType.Flip when isOdd:
                        progress = 1f - progress;
                        break;
                    case MotionLoopType.Incremental:
                        progress = item.CompletedLoops + (item.IsComplete ? 0f : progress);
                        break;
                }

                SetValue(ref item, progress, useOnlyFloatPath, useOnlyVector3Path, wideItems);
                return;
            }

            if (item.Loops == 1 && item.Elapsed >= item.Delay)
            {
                item.PreviousCompletedLoops = item.CompletedLoops;
                double timeSinceStart = item.Elapsed - item.Delay;
                item.IsComplete = timeSinceStart >= item.Duration;
                item.CompletedLoops = item.IsComplete ? (ushort)1 : (ushort)0;
                item.IsDelayed = false;

                if (!item.HasBinding) return;

                float progress = item.IsComplete
                    ? 1f
                    : (float)math.clamp(timeSinceStart / item.Duration, 0.0, 1.0);
                if (item.Ease == MotionEase.CustomAnimationCurve)
                {
                    if (evaluateCustomEase)
                    {
                        float easedProgress = EvaluateCustomEaseProgress(item, customEaseCurves[index], progress);
                        SetValue(ref item, easedProgress, useOnlyFloatPath, useOnlyVector3Path, wideItems);
                    }
                    else
                    {
                        item.Value = new float3(progress, 0f, 0f);
                    }
                    return;
                }
                SetValue(ref item, ApplyEase(item.Ease, progress), useOnlyFloatPath, useOnlyVector3Path, wideItems);
                return;
            }

            if (item.Elapsed < item.Delay)
            {
                item.PreviousCompletedLoops = item.CompletedLoops;
                item.CompletedLoops = 0;
                item.IsComplete = false;
                item.IsDelayed = true;

                if (!item.HasBinding || (item.SkipValuesDuringDelay && !forceValueDuringDelay)) return;

                if (item.Ease == MotionEase.CustomAnimationCurve && evaluateCustomEase)
                {
                    float easedProgress = EvaluateCustomEaseProgress(item, customEaseCurves[index], 0f);
                    SetValue(ref item, easedProgress, useOnlyFloatPath, useOnlyVector3Path, wideItems);
                }
                else
                {
                    SetValue(ref item, 0f, useOnlyFloatPath, useOnlyVector3Path, wideItems);
                }
                return;
            }

            double cycleDuration = item.Duration + (item.DelayEveryLoop ? item.Delay : 0.0);
            double loopTime = item.DelayEveryLoop
                ? math.fmod(item.Elapsed, cycleDuration) - item.Delay
                : item.Elapsed - item.Delay;

            int completed = (int)math.floor((item.DelayEveryLoop ? item.Elapsed : loopTime) / cycleDuration);

            completed = item.Loops < 0
                ? math.max(0, completed)
                : math.clamp(completed, 0, item.Loops);

            item.PreviousCompletedLoops = item.CompletedLoops;
            item.CompletedLoops = (ushort)completed;
            item.IsComplete = item.Loops >= 0 && completed >= item.Loops;
            item.IsDelayed = !item.IsComplete && loopTime < 0.0;

            if (!item.HasBinding || (item.IsDelayed && item.SkipValuesDuringDelay && !forceValueDuringDelay)) return;

            if (!item.DelayEveryLoop)
            {
                loopTime -= item.Duration * completed;
            }

            float t = item.IsComplete
                ? 1f
                : (float)math.clamp(loopTime / item.Duration, 0.0, 1.0);
            if (item.Ease == MotionEase.CustomAnimationCurve)
            {
                if (evaluateCustomEase)
                {
                    float easedProgress = EvaluateCustomEaseProgress(item, customEaseCurves[index], t);
                    SetValue(ref item, easedProgress, useOnlyFloatPath, useOnlyVector3Path, wideItems);
                }
                else
                {
                    item.Value = new float3(t, 0f, 0f);
                }
                return;
            }
            bool odd = ((completed + (int)t) & 1) != 0;

            if (item.LoopType == MotionLoopType.Yoyo && odd)
            {
                t = 1f - t;
            }

            t = ApplyEase(item.Ease, t);

            switch (item.LoopType)
            {
                case MotionLoopType.Flip when odd:
                    t = 1f - t;
                    break;
                case MotionLoopType.Incremental:
                    t = completed + (item.IsComplete ? 0f : t);
                    break;
            }

            SetValue(ref item, t, useOnlyFloatPath, useOnlyVector3Path, wideItems);
        }
    }
}

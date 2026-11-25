using System;
using System.Runtime.CompilerServices;

namespace MyMotion
{
    public partial struct MotionBuilder<TValue> : IDisposable
    {
        // With* は builder を複製し、複製した local の設定だけを書き換えて返す。
        // 新しい builder を constructor で組み直すと、IL2CPP は参照型 field の代入ごとに GC の write barrier を入れるが、
        // local の値型 field の書き換えには入らない。共有設定 (WithSharedConfiguration) を持つ builder は、共有側を書き換える。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref MotionBuilderOptions EditOptions(ref MotionBuilder<TValue> builder)
        {
            if (builder._sharedConfiguration != null) return ref builder._sharedConfiguration.Options;
            return ref builder._options;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly MotionBuilder<TValue> WithEase(Ease ease)
        {
            if (ease == Ease.CustomAnimationCurve)
            {
                throw new ArgumentException("Use WithEase(AnimationCurve) to specify a custom ease curve.", nameof(ease));
            }
            if (ease > Ease.InOutBounce)
            {
                throw new ArgumentOutOfRangeException(nameof(ease));
            }
            MotionBuilder<TValue> copy = this;
            ref MotionBuilderOptions options = ref EditOptions(ref copy);
            options.Ease = ease;
            if (options.CustomEaseCurve != null) options.CustomEaseCurve = null;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithEase(UnityEngine.AnimationCurve animationCurve)
        {
            if (animationCurve == null)
            {
                throw new ArgumentNullException(nameof(animationCurve));
            }
            MotionBuilder<TValue> copy = this;
            ref MotionBuilderOptions options = ref EditOptions(ref copy);
            options.Ease = Ease.CustomAnimationCurve;
            options.CustomEaseCurve = animationCurve;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithScheduler(IMotionScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }
            if (_sharedConfiguration != null)
            {
                _sharedConfiguration.Scheduler = scheduler;
                return this;
            }
            MotionBuilder<TValue> copy = this;
            copy._scheduler = scheduler;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithDelay(float delay, DelayType delayType = DelayType.FirstLoop, bool skipValuesDuringDelay = true)
        {
            MotionBuilder<TValue> copy = this;
            ref MotionBuilderOptions options = ref EditOptions(ref copy);
            options.Delay = delay;
            options.DelayEveryLoop = delayType == DelayType.EveryLoop;
            options.SkipValuesDuringDelay = skipValuesDuringDelay;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithLoops(int loops, LoopType loopType = LoopType.Restart)
        {
            MotionBuilder<TValue> copy = this;
            ref MotionBuilderOptions options = ref EditOptions(ref copy);
            options.Loops = loops;
            options.LoopType = loopType;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithPlaybackSpeed(float playbackSpeed)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).PlaybackSpeed = playbackSpeed;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithTimeKind(MotionTimeKind timeKind)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).TimeKind = timeKind;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithRoundingMode(IntegerRoundingMode roundingMode)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).RoundingMode = roundingMode;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithCancelOnError(bool cancelOnError = true)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).CancelOnError = cancelOnError;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithImmediateBind(bool immediateBind = true)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).ImmediateBind = immediateBind;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithDebugName(string debugName)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).DebugName = debugName;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithOnComplete(Action callback)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).OnComplete += callback;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithOnCancel(Action callback)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).OnCancel += callback;
            return copy;
        }

        public readonly MotionBuilder<TValue> WithOnLoopComplete(Action<int> callback)
        {
            MotionBuilder<TValue> copy = this;
            EditOptions(ref copy).OnLoopComplete += callback;
            return copy;
        }
    }
}

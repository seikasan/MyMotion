using System;
using NUnit.Framework;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void DelaySkipValues_PreservesValueAndStillReportsLoopCompletion()
        {
            float value = -1f;
            int completedLoops = 0;
            MotionHandle handle = MMotion.Create(0f, 10f, 0.5f)
                .WithDelay(0.2f, DelayType.EveryLoop)
                .WithLoops(2)
                .WithOnLoopComplete(loop => completedLoops = loop)
                .Bind(next => value = next);

            Assert.That(value, Is.EqualTo(0f));

            MMotion.Update(0.1d);
            Assert.That(value, Is.EqualTo(0f));

            MMotion.Update(0.4d);
            Assert.That(value, Is.EqualTo(6f).Within(0.0001f));

            MMotion.Update(0.21d);
            Assert.That(value, Is.EqualTo(6f).Within(0.0001f));
            Assert.That(completedLoops, Is.EqualTo(1));
            Assert.That(handle.IsActive(), Is.True);
            handle.TryCancel();
        }

        [Test]
        public void DelaySkipValuesFalse_BindsDuringInitialDelay()
        {
            float value = -1f;
            MotionHandle handle = MMotion.Create(3f, 7f, 1f)
                .WithDelay(0.5f, DelayType.FirstLoop, skipValuesDuringDelay: false)
                .Bind(next => value = next);

            MMotion.Update(0.25d);

            Assert.That(value, Is.EqualTo(3f));
            Assert.That(handle.IsActive(), Is.True);
            handle.TryCancel();
        }

        [Test]
        public void SingleLoopDelay_MatchesAllLoopTypesAndDelayModes()
        {
            foreach (LoopType loopType in Enum.GetValues(typeof(LoopType)))
            foreach (DelayType delayType in Enum.GetValues(typeof(DelayType)))
            foreach (bool skipValuesDuringDelay in new[] { false, true })
            {
                float value = float.NaN;
                int completedLoops = 0;
                MotionHandle handle = MMotion.Create(2f, 10f, 0.5f)
                    .WithImmediateBind(false)
                    .WithEase(Ease.Linear)
                    .WithDelay(0.25f, delayType, skipValuesDuringDelay)
                    .WithLoops(1, loopType)
                    .WithOnLoopComplete(loop => completedLoops = loop)
                    .Bind(next => value = next);

                MMotion.Update(0.2d);
                if (skipValuesDuringDelay)
                {
                    Assert.That(float.IsNaN(value), Is.True, $"loop={loopType}, delay={delayType}");
                }
                else
                {
                    Assert.That(value, Is.EqualTo(2f), $"loop={loopType}, delay={delayType}");
                }

                MMotion.Update(0.15d);
                Assert.That(value, Is.EqualTo(3.6f).Within(0.0001f), $"loop={loopType}, delay={delayType}");

                MMotion.Update(0.2d);
                Assert.That(value, Is.EqualTo(6.8f).Within(0.0001f), $"loop={loopType}, delay={delayType}");

                MMotion.Update(0.2d);
                Assert.That(value, Is.EqualTo(10f), $"loop={loopType}, delay={delayType}");
                Assert.That(completedLoops, Is.EqualTo(1), $"loop={loopType}, delay={delayType}");
                MMotion.Reset();
            }
        }

        [Test]
        public void InitialDelayBindsExactStartForEveryEaseLoopTypeAndDelayMode()
        {
            for (int easeValue = (int)Ease.Linear; easeValue <= (int)Ease.InOutBounce; easeValue++)
            foreach (LoopType loopType in Enum.GetValues(typeof(LoopType)))
            foreach (DelayType delayType in Enum.GetValues(typeof(DelayType)))
            {
                Ease ease = (Ease)easeValue;
                float value = float.NaN;
                MotionHandle handle = MMotion.Create(2f, 10f, 1f)
                    .WithImmediateBind(false)
                    .WithEase(ease)
                    .WithDelay(0.5f, delayType, skipValuesDuringDelay: false)
                    .WithLoops(2, loopType)
                    .Bind(next => value = next);

                MMotion.Update(0.25d);

                string scenario = $"ease={ease}, loop={loopType}, delay={delayType}";
                Assert.That(value, Is.EqualTo(2f), scenario);
                Assert.That(handle.IsActive(), Is.True, scenario);
                Assert.That(handle.TryCancel(), Is.True, scenario);
                MMotion.Update(0d);
            }
        }

        [Test]
        public void TrySetTime_RespectsDelayValueSkipping()
        {
            float value = float.NaN;
            MotionHandle handle = MMotion.Create(0f, 10f, 1f)
                .WithImmediateBind(false)
                .WithDelay(0.5f)
                .Bind(next => value = next);

            Assert.That(float.IsNaN(value), Is.True);

            handle.Time = 0.25d;
            Assert.That(float.IsNaN(value), Is.True);

            handle.Time = 0.75d;
            Assert.That(value, Is.EqualTo(2.5f).Within(0.0001f));
            Assert.That(handle.TryCancel(), Is.True);
        }

        [Test]
        public void ZeroDurationMotionsKeepExactEaseEndpoints()
        {
            for (int easeValue = (int)Ease.Linear; easeValue <= (int)Ease.InOutBounce; easeValue++)
            {
                Ease ease = (Ease)easeValue;
                using var dispatcher = new ManualMotionDispatcher();
                float value = float.NaN;
                MotionHandle handle = MMotion.Create(0f, 1f, 0f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithEase(ease)
                    .WithImmediateBind(false)
                    .Bind(next => value = next);

                dispatcher.Update(0d);
                Assert.That(value, Is.EqualTo(0f), ease.ToString() + " start endpoint");

                dispatcher.Update(1d);
                Assert.That(value, Is.EqualTo(1f), ease.ToString() + " end endpoint");
                Assert.That(handle.IsActive(), Is.False, ease.ToString() + " completion");
            }
        }

        [Test]
        public void TrySetTime_MatchesUpdateAcrossBuiltInEases()
        {
            for (int easeValue = (int)Ease.Linear; easeValue <= (int)Ease.InOutBounce; easeValue++)
            {
                Ease ease = (Ease)easeValue;
                using var seekDispatcher = new ManualMotionDispatcher();
                using var updateDispatcher = new ManualMotionDispatcher();
                float seekValue = -1f;
                float updateValue = -1f;

                MotionHandle seekHandle = MMotion.Create(-2f, 3f, 1f)
                    .WithScheduler(seekDispatcher.Scheduler)
                    .WithEase(ease)
                    .WithImmediateBind(false)
                    .Bind(value => seekValue = value);
                MMotion.Create(-2f, 3f, 1f)
                    .WithScheduler(updateDispatcher.Scheduler)
                    .WithEase(ease)
                    .WithImmediateBind(false)
                    .Bind(value => updateValue = value);

                seekHandle.Time = 0.375d;
                updateDispatcher.Update(0.375d);

                Assert.That(seekValue, Is.EqualTo(updateValue).Within(0.00001f), ease.ToString());
            }
        }

        [Test]
        public void CircOutBackAndInOutBounce_MatchReferenceAcrossProgressGrid()
        {
            Ease[] eases = { Ease.InCirc, Ease.OutCirc, Ease.InOutCirc, Ease.OutBack, Ease.InOutBounce };
            const int Steps = 4096;

            foreach (Ease ease in eases)
            {
                using var dispatcher = new ManualMotionDispatcher();
                float value = float.NaN;
                MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithEase(ease)
                    .Bind(next => value = next);

                for (int i = 0; i <= Steps; i++)
                {
                    float progress = i / (float)Steps;
                    handle.Time = progress;
                    Assert.That(value, Is.EqualTo(EvaluateEaseReference(ease, progress)).Within(0.000002f),
                        ease + " at progress " + progress.ToString("R"));
                }

                dispatcher.Update(0d);
                Assert.That(dispatcher.ActiveCount, Is.Zero);
            }
        }

        private static double EvaluateEaseReference(Ease ease, double progress)
        {
            switch (ease)
            {
                case Ease.InCirc:
                    return 1d - Math.Sqrt(1d - progress * progress);
                case Ease.OutCirc:
                    double shifted = progress - 1d;
                    return Math.Sqrt(1d - shifted * shifted);
                case Ease.InOutCirc:
                    if (progress < 0.5d)
                    {
                        double doubled = 2d * progress;
                        return (1d - Math.Sqrt(1d - doubled * doubled)) * 0.5d;
                    }
                    double inOutShifted = 2d * progress - 2d;
                    return (Math.Sqrt(1d - inOutShifted * inOutShifted) + 1d) * 0.5d;
                case Ease.OutBack:
                    const double c1 = 1.70158d;
                    double backShifted = progress - 1d;
                    return 1d + (c1 + 1d) * backShifted * backShifted * backShifted
                        + c1 * backShifted * backShifted;
                case Ease.InOutBounce:
                    double scaled = 2d * progress;
                    return scaled < 1d
                        ? (1d - EvaluateOutBounceReference(1d - scaled)) * 0.5d
                        : (1d + EvaluateOutBounceReference(scaled - 1d)) * 0.5d;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ease));
            }
        }

        private static double EvaluateOutBounceReference(double value)
        {
            const double n1 = 7.5625d;
            const double d1 = 2.75d;
            if (value < 1d / d1) return n1 * value * value;
            if (value < 2d / d1)
            {
                value -= 1.5d / d1;
                return n1 * value * value + 0.75d;
            }
            if (value < 2.5d / d1)
            {
                value -= 2.25d / d1;
                return n1 * value * value + 0.9375d;
            }
            value -= 2.625d / d1;
            return n1 * value * value + 0.984375d;
        }
    }
}

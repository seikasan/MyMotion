using System;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void CustomAnimationCurve_MatchesLoopModesAndSnapshotsCurveAtBind()
        {
            Keyframe[] keys =
            {
                new Keyframe(0f, 0.2f, 0f, 1.4f),
                new Keyframe(1f, 0.8f, 0.3f, 0f)
            };
            LoopType[] loopTypes = { LoopType.Restart, LoopType.Flip, LoopType.Incremental, LoopType.Yoyo };

            foreach (LoopType loopType in loopTypes)
            {
                using var dispatcher = new ManualMotionDispatcher();
                AnimationCurve sourceCurve = new AnimationCurve(keys)
                {
                    preWrapMode = WrapMode.PingPong,
                    postWrapMode = WrapMode.Loop
                };
                AnimationCurve expectedCurve = new AnimationCurve(keys)
                {
                    preWrapMode = WrapMode.PingPong,
                    postWrapMode = WrapMode.Loop
                };
                float value = float.NaN;

                MMotion.Create(0f, 10f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithLoops(2, loopType)
                    .WithEase(sourceCurve)
                    .Bind(result => value = result);

                Keyframe[] changedKeys = sourceCurve.keys;
                changedKeys[0].value = -3f;
                changedKeys[1].value = -3f;
                sourceCurve.keys = changedKeys;
                dispatcher.Update(1.25d);

                float curveValue = loopType switch
                {
                    LoopType.Restart => expectedCurve.Evaluate(0.25f),
                    LoopType.Flip => 1f - expectedCurve.Evaluate(0.25f),
                    LoopType.Incremental => expectedCurve.Evaluate(1f) + expectedCurve.Evaluate(0.25f),
                    LoopType.Yoyo => expectedCurve.Evaluate(0.75f),
                    _ => throw new ArgumentOutOfRangeException(nameof(loopType))
                };
                Assert.That(value, Is.EqualTo(curveValue * 10f).Within(1e-5f), loopType.ToString());
            }
        }

        [Test]
        public void CustomAnimationCurve_AppliesToImmediateBindSeekAndComplete()
        {
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, 0.2f, 0f, 1.4f),
                new Keyframe(1f, 0.8f, 0.3f, 0f));
            float value = float.NaN;
            int completeCount = 0;
            MotionHandle handle = MMotion.Create(0f, 10f, 1f)
                .WithEase(curve)
                .WithOnComplete(() => completeCount++)
                .Bind(result => value = result);

            Assert.That(value, Is.EqualTo(curve.Evaluate(0f) * 10f).Within(1e-5f));
            handle.Time = 0.5d;
            Assert.That(value, Is.EqualTo(curve.Evaluate(0.5f) * 10f).Within(1e-5f));
            Assert.That(handle.TryComplete(), Is.True);
            Assert.That(value, Is.EqualTo(curve.Evaluate(1f) * 10f).Within(1e-5f));
            Assert.That(completeCount, Is.EqualTo(1));
        }

        [TestCase(WrapMode.ClampForever, WrapMode.ClampForever)]
        [TestCase(WrapMode.Loop, WrapMode.PingPong)]
        public void CustomAnimationCurve_BurstEvaluationMatchesUnityForWeightedKeys(WrapMode preWrap, WrapMode postWrap)
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            Keyframe first = new Keyframe(0.2f, 0.3f, -0.4f, 1.1f)
            {
                weightedMode = WeightedMode.Out,
                outWeight = 0.72f
            };
            Keyframe middle = new Keyframe(0.51f, 0.66f, 0.8f, -0.2f)
            {
                weightedMode = WeightedMode.Both,
                inWeight = 0.18f,
                outWeight = 0.63f
            };
            Keyframe last = new Keyframe(0.8f, 0.4f, 0.1f, -0.5f)
            {
                weightedMode = WeightedMode.In,
                inWeight = 0.81f
            };
            AnimationCurve curve = new AnimationCurve(first, middle, last)
            {
                preWrapMode = preWrap,
                postWrapMode = postWrap
            };
            float value = float.NaN;

            MMotion.Create(0f, 10f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithLoops(3)
                .WithEase(curve)
                .Bind(result => value = result);

            for (int i = 1; i <= 100; i++)
            {
                dispatcher.Update(0.01d);
                float elapsed = i * 0.01f;
                float progress = elapsed >= 1f ? elapsed - 1f : elapsed;
                Assert.That(value, Is.EqualTo(curve.Evaluate(progress) * 10f).Within(2e-5f),
                    $"wrap={preWrap}/{postWrap}, progress={progress}");
            }
        }

        [Test]
        public void CustomAnimationCurve_BurstEvaluationMatchesUnityForUnweightedKeys()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, 0.2f, 0.8f, -0.4f),
                new Keyframe(0.13f, 0.6f, -1.2f, 0.7f),
                new Keyframe(0.57f, 0.3f, 1.1f, -0.3f),
                new Keyframe(1f, 0.9f, -0.5f, 0.2f));
            float value = float.NaN;

            MMotion.Create(0f, 10f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithLoops(-1)
                .WithEase(curve)
                .Bind(result => value = result);

            for (int i = 1; i < 100; i++)
            {
                dispatcher.Update(0.01d);
                float progress = i * 0.01f;
                Assert.That(value, Is.EqualTo(curve.Evaluate(progress) * 10f).Within(2e-5f),
                    $"progress={progress}");
            }
        }

        [Test]
        public void CustomAnimationCurve_EmptyAndSingleKeyCurvesKeepUnityEvaluation()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            float emptyValue = float.NaN;
            float singleKeyValue = float.NaN;

            MMotion.Create(0f, 10f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithEase(new AnimationCurve())
                .Bind(value => emptyValue = value);
            MMotion.Create(0f, 10f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithEase(new AnimationCurve(new Keyframe(0.4f, 0.7f)))
                .Bind(value => singleKeyValue = value);

            dispatcher.Update(0.35d);

            Assert.That(emptyValue, Is.EqualTo(0f));
            Assert.That(singleKeyValue, Is.EqualTo(7f).Within(1e-6f));
        }

        [Test]
        public void TryComplete_CustomIncrementalEaseMatchesLitMotionCurveAtLoopCount()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(1f, 0.8f));
            float completedValue = float.NaN;
            float updatedValue = float.NaN;

            MotionHandle completedHandle = MMotion.Create(0f, 10f, 0.1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithLoops(3, LoopType.Incremental)
                .WithEase(curve)
                .Bind(value => completedValue = value);
            MotionHandle updatedHandle = MMotion.Create(0f, 10f, 0.1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithLoops(3, LoopType.Incremental)
                .WithEase(curve)
                .Bind(value => updatedValue = value);

            Assert.That(completedHandle.TryComplete(), Is.True);
            dispatcher.Update(0.31d);

            Assert.That(completedValue, Is.EqualTo(curve.Evaluate(3f) * 10f).Within(1e-5f));
            Assert.That(updatedValue, Is.EqualTo((curve.Evaluate(1f) * 3f + curve.Evaluate(0f)) * 10f).Within(1e-5f));
            Assert.That(completedValue, Is.Not.EqualTo(updatedValue));
        }
    }
}

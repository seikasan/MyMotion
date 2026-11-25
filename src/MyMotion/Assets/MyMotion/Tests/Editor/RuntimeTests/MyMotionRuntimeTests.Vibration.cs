// LitMotion の実装と結果を突き合わせるテスト。LitMotion がプロジェクトにあるときだけコンパイルする。
#if MYMOTION_TEST_LITMOTION
using MyMotion.Adapters;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [TestCase(0f)]
        [TestCase(0.125f)]
        [TestCase(0.5f)]
        [TestCase(0.875f)]
        [TestCase(1f)]
        public void PunchAdapters_MatchLitMotion(float progress)
        {
            MotionEvaluationContext context = new() { Progress = progress, Time = progress };
            LitMotion.MotionEvaluationContext referenceContext = new() { Progress = progress, Time = progress };
            PunchOptions options = new() { Frequency = 13, DampingRatio = 0.65f };
            LitMotion.PunchOptions referenceOptions = new() { Frequency = 13, DampingRatio = 0.65f };

            float floatStart = 2.5f;
            float floatStrength = 4.25f;
            float actualFloat = new FloatPunchMotionAdapter().Evaluate(
                ref floatStart, ref floatStrength, ref options, in context);
            float expectedFloat = new LitMotion.Adapters.FloatPunchMotionAdapter().Evaluate(
                ref floatStart, ref floatStrength, ref referenceOptions, in referenceContext);
            Assert.That(actualFloat, Is.EqualTo(expectedFloat).Within(0.000001f));

            Vector2 vector2Start = new(1.5f, -3f);
            Vector2 vector2Strength = new(2f, 6f);
            options = new PunchOptions { Frequency = 13, DampingRatio = 0.65f };
            referenceOptions = new LitMotion.PunchOptions { Frequency = 13, DampingRatio = 0.65f };
            Vector2 actualVector2 = new Vector2PunchMotionAdapter().Evaluate(
                ref vector2Start, ref vector2Strength, ref options, in context);
            Vector2 expectedVector2 = new LitMotion.Adapters.Vector2PunchMotionAdapter().Evaluate(
                ref vector2Start, ref vector2Strength, ref referenceOptions, in referenceContext);
            Assert.That(actualVector2.x, Is.EqualTo(expectedVector2.x).Within(0.000001f));
            Assert.That(actualVector2.y, Is.EqualTo(expectedVector2.y).Within(0.000001f));

            Vector3 vector3Start = new(1.5f, -3f, 7f);
            Vector3 vector3Strength = new(2f, 6f, -4f);
            options = new PunchOptions { Frequency = 13, DampingRatio = 0.65f };
            referenceOptions = new LitMotion.PunchOptions { Frequency = 13, DampingRatio = 0.65f };
            Vector3 actualVector3 = new Vector3PunchMotionAdapter().Evaluate(
                ref vector3Start, ref vector3Strength, ref options, in context);
            Vector3 expectedVector3 = new LitMotion.Adapters.Vector3PunchMotionAdapter().Evaluate(
                ref vector3Start, ref vector3Strength, ref referenceOptions, in referenceContext);
            Assert.That(actualVector3.x, Is.EqualTo(expectedVector3.x).Within(0.000001f));
            Assert.That(actualVector3.y, Is.EqualTo(expectedVector3.y).Within(0.000001f));
            Assert.That(actualVector3.z, Is.EqualTo(expectedVector3.z).Within(0.000001f));
        }

        [TestCase(MyMotion.ShakeMode.Random, 0.11f, 0.37d)]
        [TestCase(MyMotion.ShakeMode.Random, 0.73f, 1.25d)]
        [TestCase(MyMotion.ShakeMode.PerlinNoise, 0.11f, 0.37d)]
        [TestCase(MyMotion.ShakeMode.PerlinNoise, 0.73f, 1.25d)]
        public void ShakeAdapters_MatchLitMotion(ShakeMode shakeMode, float progress, double time)
        {
            MotionEvaluationContext context = new() { Progress = progress, Time = time };
            ShakeOptions options = new()
            {
                ShakeMode = shakeMode,
                Frequency = 17,
                DampingRatio = 0.45f,
                RandomSeed = 1234567u
            };
            LitMotion.ShakeOptions referenceOptions = new()
            {
                ShakeMode = (LitMotion.ShakeMode)shakeMode,
                Frequency = 17,
                DampingRatio = 0.45f,
                RandomSeed = 1234567u
            };
            LitMotion.MotionEvaluationContext referenceContext = new() { Progress = progress, Time = time };

            float floatStart = 2.5f;
            float floatStrength = 4.25f;
            float actualFloat = new FloatShakeMotionAdapter().Evaluate(
                ref floatStart, ref floatStrength, ref options, in context);
            float expectedFloat = new LitMotion.Adapters.FloatShakeMotionAdapter().Evaluate(
                ref floatStart, ref floatStrength, ref referenceOptions, in referenceContext);
            Assert.That(actualFloat, Is.EqualTo(expectedFloat).Within(0.00001f));

            Vector2 vector2Start = new(1.5f, -3f);
            Vector2 vector2Strength = new(2f, 6f);
            options = new ShakeOptions
            {
                ShakeMode = shakeMode,
                Frequency = 17,
                DampingRatio = 0.45f,
                RandomSeed = 1234567u
            };
            referenceOptions = new LitMotion.ShakeOptions
            {
                ShakeMode = (LitMotion.ShakeMode)shakeMode,
                Frequency = 17,
                DampingRatio = 0.45f,
                RandomSeed = 1234567u
            };
            Vector2 actualVector2 = new Vector2ShakeMotionAdapter().Evaluate(
                ref vector2Start, ref vector2Strength, ref options, in context);
            Vector2 expectedVector2 = new LitMotion.Adapters.Vector2ShakeMotionAdapter().Evaluate(
                ref vector2Start, ref vector2Strength, ref referenceOptions, in referenceContext);
            Assert.That(actualVector2.x, Is.EqualTo(expectedVector2.x).Within(0.00001f));
            Assert.That(actualVector2.y, Is.EqualTo(expectedVector2.y).Within(0.00001f));

            Vector3 vector3Start = new(1.5f, -3f, 7f);
            Vector3 vector3Strength = new(2f, 6f, -4f);
            options = new ShakeOptions
            {
                ShakeMode = shakeMode,
                Frequency = 17,
                DampingRatio = 0.45f,
                RandomSeed = 1234567u
            };
            referenceOptions = new LitMotion.ShakeOptions
            {
                ShakeMode = (LitMotion.ShakeMode)shakeMode,
                Frequency = 17,
                DampingRatio = 0.45f,
                RandomSeed = 1234567u
            };
            Vector3 actualVector3 = new Vector3ShakeMotionAdapter().Evaluate(
                ref vector3Start, ref vector3Strength, ref options, in context);
            Vector3 expectedVector3 = new LitMotion.Adapters.Vector3ShakeMotionAdapter().Evaluate(
                ref vector3Start, ref vector3Strength, ref referenceOptions, in referenceContext);
            Assert.That(actualVector3.x, Is.EqualTo(expectedVector3.x).Within(0.00001f));
            Assert.That(actualVector3.y, Is.EqualTo(expectedVector3.y).Within(0.00001f));
            Assert.That(actualVector3.z, Is.EqualTo(expectedVector3.z).Within(0.00001f));
        }

        [Test]
        public void VibrationBuilders_ExposeLitMotionStyleOptionsAndBinding()
        {
            using ManualMotionDispatcher dispatcher = new();
            float punchValue = -1f;
            float shakeValue = -1f;

            MotionHandle punch = MMotion.Punch.Create(2f, 4f, 1f)
                .WithFrequency(12)
                .WithDampingRatio(0.5f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(value => punchValue = value);
            MotionHandle shake = MMotion.Shake.Create(3f, 5f, 1f)
                .WithFrequency(14)
                .WithDampingRatio(0.75f)
                .WithRandomSeed(789u)
                .WithShakeMode(ShakeMode.Random)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(value => shakeValue = value);

            dispatcher.Update(0.25d);

            Assert.That(punchValue, Is.Not.EqualTo(-1f));
            Assert.That(shakeValue, Is.Not.EqualTo(-1f));
            Assert.That(punch.IsActive(), Is.True);
            Assert.That(shake.IsActive(), Is.True);
        }

        [Test]
        public void ShakeBuilder_MatchesLitMotionAcrossDelayAndLoops()
        {
            using ManualMotionDispatcher myDispatcher = new();
            LitMotion.ManualMotionDispatcher litDispatcher = new();
            float myValue = -1f;
            float litValue = -1f;

            MotionHandle myHandle = MMotion.Shake.Create(2f, 4f, 0.5f)
                .WithDelay(0.1f, DelayType.EveryLoop, false)
                .WithLoops(3, LoopType.Restart)
                .WithEase(Ease.InOutQuad)
                .WithScheduler(myDispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(value => myValue = value);
            LitMotion.MotionHandle litHandle = LitMotion.LMotion.Shake.Create(2f, 4f, 0.5f)
                .WithDelay(0.1f, LitMotion.DelayType.EveryLoop, false)
                .WithLoops(3, LitMotion.LoopType.Restart)
                .WithEase(LitMotion.Ease.InOutQuad)
                .WithScheduler(litDispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(value => litValue = value);

            double[] deltaTimes = { 0.05d, 0.06d, 0.4d, 0.15d, 0.45d, 0.5d };
            foreach (double deltaTime in deltaTimes)
            {
                myDispatcher.Update(deltaTime);
                litDispatcher.Update(deltaTime);
                Assert.That(myValue, Is.EqualTo(litValue).Within(0.00001f));
                Assert.That(myHandle.IsActive(), Is.EqualTo(LitMotion.MotionHandleExtensions.IsActive(litHandle)));
            }

            myDispatcher.Update(1d);
            litDispatcher.Update(1d);
            Assert.That(myValue, Is.EqualTo(litValue).Within(0.00001f));
            Assert.That(myHandle.IsActive(), Is.False);
            Assert.That(LitMotion.MotionHandleExtensions.IsActive(litHandle), Is.False);
        }
    }
}
#endif

using System;
using NUnit.Framework;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void GenericAdapter_StorageRoutesSurviveGrowthAndSwapBackRemoval()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            const int count = 40;
            AdapterTarget[] targets = new AdapterTarget[count];
            MotionHandle[] handles = new MotionHandle[count];

            for (int i = 0; i < count; i++)
            {
                targets[i] = new AdapterTarget { Value = -1f };
                handles[i] = MMotion.Create<float, AdapterOptions, TestAdapter>(i, i + 4f, 1f)
                    .WithOptions(new AdapterOptions { ProgressScale = 1f })
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .Bind(targets[i], static (value, state) => state.Value = value);
            }

            for (int i = 0; i < count; i += 2)
            {
                Assert.That(handles[i].TryCancel(), Is.True);
            }

            dispatcher.Update(0.25d);

            Assert.That(dispatcher.ActiveCount, Is.EqualTo(count / 2));
            for (int i = 0; i < count; i++)
            {
                float expected = i % 2 == 0 ? -1f : i + 1.25f;
                Assert.That(targets[i].Value, Is.EqualTo(expected).Within(0.0001f));
            }
        }

        [Test]
        public void GenericAdapter_DifferentTypesShareOneMotionWorld()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            AdapterTarget first = new();
            AdapterTarget second = new();

            MotionHandle firstHandle = MMotion.Create<float, AdapterOptions, TestAdapter>(2f, 6f, 2f)
                .WithOptions(new AdapterOptions { ProgressScale = 1f })
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(first, static (value, state) => state.Value = value);
            MotionHandle secondHandle = MMotion.Create<float, OffsetOptions, OffsetAdapter>(10f, 20f, 2f)
                .WithOptions(new OffsetOptions { TimeScale = 2f })
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(second, static (value, state) => state.Value = value);

            dispatcher.Update(1d);

            Assert.That(first.Value, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(secondHandle.Time, Is.EqualTo(1d).Within(0.0001d));
            Assert.That(second.Value, Is.EqualTo(17f).Within(0.0001f));
            Assert.That(firstHandle.IsActive(), Is.True);
            Assert.That(secondHandle.IsActive(), Is.True);
        }

        [Test]
        public void GenericAdapter_RunWithoutBindingDoesNotAllocateAdapterStorage()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            using ManualMotionDispatcher referenceDispatcher = new ManualMotionDispatcher();

            MotionHandle handle = MMotion.Create<float, AdapterOptions, TestAdapter>(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .RunWithoutBinding();
            MMotion.Create(0f, 1f, 1f)
                .WithScheduler(referenceDispatcher.Scheduler)
                .WithImmediateBind(false)
                .RunWithoutBinding();

            dispatcher.Update(0.5d);

            Assert.That(dispatcher.ReservedNativeBytes, Is.EqualTo(referenceDispatcher.ReservedNativeBytes));
            Assert.That(handle.IsActive(), Is.True);
        }
        [Test]
        public void Update_DoesNotCreateWorldWhenNoMotionsExist()
        {
            Assert.That(MMotion.ReservedNativeBytes, Is.Zero);

            MMotion.Update(0d);
            MMotion.Update(0d, 0d, 0d);
            Assert.Throws<ArgumentOutOfRangeException>(() => MMotion.Update(double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => MMotion.Update(0d, double.PositiveInfinity, 0d));

            Assert.That(MMotion.ReservedNativeBytes, Is.Zero);
        }

        [Test]
        public void InSineMinimax_HasExactEndpointsAndMonotoneSamples()
        {
            float value = -1f;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithEase(Ease.InSine)
                .Bind(next => value = next);

            float previous = -1f;
            for (int sample = 0; sample <= 256; sample++)
            {
                float time = sample / 256f;
                handle.Time = time;

                double expected = 1d - Math.Cos(time * Math.PI * 0.5d);
                Assert.That(value, Is.EqualTo(expected).Within(0.000001f));
                Assert.That(value, Is.GreaterThanOrEqualTo(previous));
                previous = value;
            }

            Assert.That(value, Is.EqualTo(1f));
        }

        [Test]
        public void InOutSineMinimax_HasExactEndpointsAndMonotoneSamples()
        {
            float value = -1f;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithEase(Ease.InOutSine)
                .Bind(next => value = next);

            float previous = -1f;
            const int sampleCount = 100_000;
            for (int sample = 0; sample <= sampleCount; sample++)
            {
                float time = sample / (float)sampleCount;
                handle.Time = time;

                double expected = 0.5d - 0.5d * Math.Cos(time * Math.PI);
                Assert.That(value, Is.EqualTo(expected).Within(0.000001f), "time=" + time);
                Assert.That(value, Is.GreaterThanOrEqualTo(previous), "time=" + time);
                previous = value;
            }

            Assert.That(value, Is.EqualTo(1f));
        }

        [Test]
        public void ElasticEases_MatchMathReferenceAndPreserveEndpoints()
        {
            foreach (Ease ease in new[] { Ease.InElastic, Ease.OutElastic, Ease.InOutElastic })
            {
                float value = -1f;
                MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                    .WithEase(ease)
                    .Bind(next => value = next);

                for (int sample = 0; sample <= 1024; sample++)
                {
                    float time = sample / 1024f;
                    handle.Time = time;
                    Assert.That(value, Is.EqualTo(EvaluateElasticReference(ease, time)).Within(0.000001f), ease + " at " + time);
                }

                Assert.That(value, Is.EqualTo(1f));
                MMotion.Reset();
            }
        }
    }
}

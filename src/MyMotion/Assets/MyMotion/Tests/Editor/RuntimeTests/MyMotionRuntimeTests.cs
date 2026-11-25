using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        private static readonly Action<Color> s_wideBinding = RecordWideValue;
        private static readonly Action<float> s_narrowBinding = RecordNarrowValue;
        private static int s_wideCallbackCount;
        private static Color s_lastWideValue;
        private static float s_narrowValue;

        [SetUp]
        public void SetUp()
        {
            MMotion.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            MMotion.Reset();
        }

        [Test]
        public void FloatMotion_UpdatesAndCancels()
        {
            float value = -1f;
            MotionHandle handle = MMotion.Create(0f, 10f, 1f)
                .WithEase(Ease.Linear)
                .Bind(next => value = next);

            MMotion.Update(0.5d);

            Assert.That(value, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(handle.IsActive(), Is.True);
            Assert.That(handle.TryCancel(), Is.True);
            Assert.That(handle.IsActive(), Is.False);
        }

        [Test]
        public void PlayerLoopScheduler_DoesNotReportAnEmptyWorldAsActive()
        {
            PlayerLoopTiming timing = PlayerLoopTiming.Update;
            MotionScheduler.GetWorld(timing);
            Assert.That(MotionScheduler.HasWorld(timing), Is.False);

            FieldInfo previousRealtimeField = typeof(PlayerLoopMotionScheduler)
                .GetField("s_previousRealtime", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(previousRealtimeField, Is.Not.Null);
            double[] previousRealtime = (double[])previousRealtimeField.GetValue(null);
            previousRealtime[(int)timing] = -1d;

            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .Bind(static _ => { });
            Assert.That(MotionScheduler.HasWorld(timing), Is.True);
            Assert.That(previousRealtime[(int)timing], Is.EqualTo(Time.realtimeSinceStartupAsDouble).Within(0.01d));

            Assert.That(handle.TryCancel(), Is.True);
            MotionScheduler.UpdateWorld(timing, 0d, 0d, 0d);
            Assert.That(MotionScheduler.HasWorld(timing), Is.False);
        }

        [Test]
        public void GenericAdapter_UsesOptionsProgressAndElapsedTime()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            AdapterTarget target = new();

            MotionHandle handle = MMotion.Create<float, AdapterOptions, TestAdapter>(2f, 6f, 2f)
                .WithOptions(new AdapterOptions { ProgressScale = 2f })
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(target, static (value, state) => state.Value = value);

            dispatcher.Update(1d);

            Assert.That(target.Value, Is.EqualTo(7f).Within(0.0001f));
            Assert.That(handle.Time, Is.EqualTo(1d).Within(0.0001d));
            Assert.That(handle.IsActive(), Is.True);
        }

        [Test]
        public void GenericAdapterAdvance_MatchesFloatMotionAcrossLoopAndDelayModes()
        {
            LoopType[] loopTypes = { LoopType.Restart, LoopType.Flip, LoopType.Yoyo, LoopType.Incremental };
            DelayType[] delayTypes = { DelayType.FirstLoop, DelayType.EveryLoop };
            int[] loopCounts = { 1, 3, -1 };
            float[] durations = { 0f, 0.7f };
            double[] deltas = { 0d, 0.07d, 0.13d, 0.21d, 0.27d, 0.41d, 0.53d, 0.11d, 0.79d, 0.15d, 0.8d };

            foreach (LoopType loopType in loopTypes)
            foreach (DelayType delayType in delayTypes)
            foreach (int loopCount in loopCounts)
            foreach (float duration in durations)
            foreach (bool skipValuesDuringDelay in new[] { false, true })
            {
                using ManualMotionDispatcher floatDispatcher = new();
                using ManualMotionDispatcher adapterDispatcher = new();
                float floatValue = float.NaN;
                float adapterValue = float.NaN;

                MotionHandle floatHandle = MMotion.Create(0f, 1f, duration)
                    .WithScheduler(floatDispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithDelay(0.2f, delayType, skipValuesDuringDelay)
                    .WithLoops(loopCount, loopType)
                    .WithPlaybackSpeed(1.35f)
                    .WithEase(Ease.OutBounce)
                    .Bind(value => floatValue = value);
                MotionHandle adapterHandle = MMotion.Create<float, AdapterOptions, ProgressAdapter>(0f, 1f, duration)
                    .WithOptions(new AdapterOptions { ProgressScale = 1f })
                    .WithScheduler(adapterDispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithDelay(0.2f, delayType, skipValuesDuringDelay)
                    .WithLoops(loopCount, loopType)
                    .WithPlaybackSpeed(1.35f)
                    .WithEase(Ease.OutBounce)
                    .Bind(value => adapterValue = value);

                foreach (double delta in deltas)
                {
                    floatDispatcher.Update(delta);
                    adapterDispatcher.Update(delta);

                    Assert.That(adapterHandle.IsActive(), Is.EqualTo(floatHandle.IsActive()));
                    if (floatHandle.IsActive())
                    {
                        Assert.That(adapterHandle.Time, Is.EqualTo(floatHandle.Time).Within(1e-9d));
                    }
                    Assert.That(float.IsNaN(adapterValue), Is.EqualTo(float.IsNaN(floatValue)));
                    if (!float.IsNaN(floatValue))
                    {
                        Assert.That(adapterValue, Is.EqualTo(floatValue).Within(1e-5f));
                    }
                    if (!floatHandle.IsActive()) break;
                }
            }
        }

        [Test]
        public void GenericAdapter_ImmediateBindingUsesStartValueAtTimeZero()
        {
            float value = -1f;
            MotionHandle handle = MMotion.Create<float, AdapterOptions, TestAdapter>(3f, 7f, 1f)
                .WithOptions(new AdapterOptions { ProgressScale = 2f })
                .Bind(next => value = next);

            Assert.That(value, Is.EqualTo(3f));
            Assert.That(handle.IsActive(), Is.True);
        }
    }
}

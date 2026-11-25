using System;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void PartialCompletionCleanup_RemovesQueuedHandlesAfterCapacityGrowth()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            dispatcher.EnsureStorageCapacity(5000);
            MotionHandle[] handles = new MotionHandle[5000];
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i] = MMotion.Create(0f, 1f, 10f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .RunWithoutBinding();
            }

            Assert.That(handles[1].TryComplete(), Is.True);
            Assert.That(handles[10].TryCancel(), Is.True);
            Assert.That(handles[4999].TryComplete(), Is.True);
            dispatcher.EnsureStorageCapacity(10000);
            dispatcher.Update(0d);

            Assert.That(dispatcher.ActiveCount, Is.EqualTo(4997));
            Assert.That(handles[1].IsActive(), Is.False);
            Assert.That(handles[10].IsActive(), Is.False);
            Assert.That(handles[4999].IsActive(), Is.False);
            Assert.That(handles[0].IsActive(), Is.True);
            Assert.That(handles[4998].IsActive(), Is.True);
        }

        [Test]
        public void PartialCompletionCleanup_SortsOnlyWhenRemovalOrderIsUnsorted()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            dispatcher.EnsureStorageCapacity(5000);
            MotionHandle[] handles = new MotionHandle[5000];
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i] = MMotion.Create(0f, 1f, 10f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .RunWithoutBinding();
            }

            Assert.That(handles[4999].TryComplete(), Is.True);
            Assert.That(handles[1].TryCancel(), Is.True);
            Assert.That(handles[2500].TryComplete(), Is.True);
            Assert.That(handles[10].TryCancel(), Is.True);
            dispatcher.Update(0d);

            Assert.That(dispatcher.ActiveCount, Is.EqualTo(4996));
            Assert.That(handles[4999].IsActive(), Is.False);
            Assert.That(handles[1].IsActive(), Is.False);
            Assert.That(handles[2500].IsActive(), Is.False);
            Assert.That(handles[10].IsActive(), Is.False);
            Assert.That(handles[0].IsActive(), Is.True);
            Assert.That(handles[4998].IsActive(), Is.True);
        }

        [Test]
        public void PartialCompletionCleanup_OverflowUsesFullScanSafely()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            dispatcher.EnsureStorageCapacity(5000);
            MotionHandle[] handles = new MotionHandle[5000];
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i] = MMotion.Create(0f, 1f, 10f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .RunWithoutBinding();
            }
            for (int i = 0; i < 51; i++) Assert.That(handles[i].TryComplete(), Is.True);

            dispatcher.Update(0d);

            Assert.That(dispatcher.ActiveCount, Is.EqualTo(4949));
            for (int i = 0; i < 51; i++) Assert.That(handles[i].IsActive(), Is.False);
            for (int i = 51; i < handles.Length; i++) Assert.That(handles[i].IsActive(), Is.True);
        }

        [Test]
        public void CustomAnimationCurve_RequiresCurveOverloadAndPreservesPackedDataStride()
        {
            Assert.Throws<ArgumentException>(() => MMotion.Create(0f, 1f, 1f).WithEase(Ease.CustomAnimationCurve));
            Assert.Throws<ArgumentNullException>(() => MMotion.Create(0f, 1f, 1f).WithEase((AnimationCurve)null));

            using var world = new MyMotion.Internal.MotionWorld(4);
            Assert.That(MyMotion.Internal.MotionWorld.DataStrideBytes, Is.EqualTo(80));
            Assert.That(world.ReservedNativeBytes, Is.EqualTo(
                376L + MyMotion.Internal.MotionWorld.CustomEaseCurveDataStrideBytes));
        }

        private struct AdapterOptions : IMotionOptions
        {
            public float ProgressScale;
        }

        private struct OffsetOptions : IMotionOptions
        {
            public float TimeScale;
        }

        private struct TestAdapter : IMotionAdapter<float, AdapterOptions>
        {
            public float Evaluate(
                ref float startValue,
                ref float endValue,
                ref AdapterOptions options,
                in MotionEvaluationContext context)
            {
                return startValue
                    + (endValue - startValue) * context.Progress * options.ProgressScale
                    + (float)context.Time;
            }
        }

        private struct ProgressAdapter : IMotionAdapter<float, AdapterOptions>
        {
            public float Evaluate(
                ref float startValue,
                ref float endValue,
                ref AdapterOptions options,
                in MotionEvaluationContext context)
            {
                return startValue + (endValue - startValue) * context.Progress * options.ProgressScale;
            }
        }

        private struct OffsetAdapter : IMotionAdapter<float, OffsetOptions>
        {
            public float Evaluate(
                ref float startValue,
                ref float endValue,
                ref OffsetOptions options,
                in MotionEvaluationContext context) =>
                startValue + (endValue - startValue) * context.Progress + (float)context.Time * options.TimeScale;
        }

        private sealed class AdapterTarget
        {
            public float Value;
        }

        private sealed class RecordingLogHandler : ILogHandler
        {
            public string LastMessage;

            public void LogException(Exception exception, UnityEngine.Object context)
            {
            }

            public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                LastMessage = string.Format(format, args);
            }
        }

        private sealed class StringTarget64
        {
            public FixedString64Bytes Value;
        }

        private sealed class Target
        {
            public float Value;
        }

        private sealed class RecordingProgress<T> : IProgress<T>
        {
            public T Value;
            public void Report(T value) => Value = value;
        }

        private struct TargetState
        {
            public Target Target;
        }

        private static void RecordWideValue(Color value)
        {
            s_wideCallbackCount++;
            s_lastWideValue = value;
        }

        private static void RecordNarrowValue(float value) => s_narrowValue = value;

        private static double EvaluateElasticReference(Ease ease, double time)
        {
            if (time == 0d) return 0d;
            if (time == 1d) return 1d;
            if (ease == Ease.InElastic)
            {
                return -Math.Pow(2d, 10d * time - 10d) * Math.Sin((10d * time - 10.75d) * (2d * Math.PI / 3d));
            }
            if (ease == Ease.OutElastic)
            {
                return Math.Pow(2d, -10d * time) * Math.Sin((10d * time - 0.75d) * (2d * Math.PI / 3d)) + 1d;
            }

            double phase = (20d * time - 11.125d) * (2d * Math.PI / 4.5d);
            return time < 0.5d
                ? -0.5d * Math.Pow(2d, 20d * time - 10d) * Math.Sin(phase)
                : 0.5d * Math.Pow(2d, 10d - 20d * time) * Math.Sin(phase) + 1d;
        }
    }
}

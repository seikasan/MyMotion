using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void WideValues_ParallelWritesRespectUniqueWideIndices()
        {
            s_wideCallbackCount = 0;
            s_lastWideValue = default;
            s_narrowValue = -1f;

            MMotion.Create(0f, 1f, 1f).Bind(s_narrowBinding);
            const int WideCount = 14999;
            for (int i = 0; i < WideCount; i++)
            {
                MMotion.Create(Color.black, Color.white, 1f).Bind(s_wideBinding);
            }

            s_wideCallbackCount = 0;
            MMotion.Update(0.5d);

            Assert.That(MMotion.ActiveCount, Is.EqualTo(15000));
            Assert.That(s_narrowValue, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(s_wideCallbackCount, Is.EqualTo(WideCount));
            Assert.That(s_lastWideValue.r, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(s_lastWideValue.a, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void SparseCleanup_CancelAndWideSwapBackKeepRemainingMotionValues()
        {
            using ManualMotionDispatcher dispatcher = new();
            const int MotionCount = 4100;
            const int RemovedWideIndex = 3;
            const int RemainingWideIndex = 1000;
            const int TriggerIndex = 2000;
            const int RemovedLastIndex = MotionCount - 1;
            MotionHandle[] handles = new MotionHandle[MotionCount];
            Color remainingWideValue = Color.black;
            bool canceledLast = false;
            bool canceledWide = false;
            bool requestedCancellation = false;

            for (int i = 0; i < MotionCount; i++)
            {
                if (i == RemovedWideIndex)
                {
                    handles[i] = MMotion.Create(Color.black, Color.white, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .Bind(static _ => { });
                }
                else if (i == RemainingWideIndex)
                {
                    handles[i] = MMotion.Create(Color.black, Color.white, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .Bind(value => remainingWideValue = value);
                }
                else if (i == TriggerIndex)
                {
                    handles[i] = MMotion.Create(0f, 1f, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .Bind(_ =>
                        {
                            if (requestedCancellation) return;
                            requestedCancellation = true;
                            canceledLast = handles[RemovedLastIndex].TryCancel();
                            canceledWide = handles[RemovedWideIndex].TryCancel();
                        });
                }
                else
                {
                    handles[i] = MMotion.Create(0f, 1f, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .RunWithoutBinding();
                }
            }

            dispatcher.Update(1d);

            Assert.That(requestedCancellation, Is.True);
            Assert.That(canceledLast, Is.True);
            Assert.That(canceledWide, Is.True);
            Assert.That(handles[RemovedLastIndex].IsActive(), Is.False);
            Assert.That(handles[RemovedWideIndex].IsActive(), Is.False);
            Assert.That(handles[RemainingWideIndex].IsActive(), Is.True);
            Assert.That(dispatcher.ActiveCount, Is.EqualTo(MotionCount - 2));

            dispatcher.Update(0.5d);

            Assert.That(remainingWideValue.r, Is.EqualTo(0.15f).Within(0.0001f));
            Assert.That(handles[RemainingWideIndex].IsActive(), Is.True);
            Assert.That(dispatcher.ActiveCount, Is.EqualTo(MotionCount - 2));
        }

        [Test]
        public void SparseCleanup_WideSwapBackMovesAndClearsNotifications()
        {
            using ManualMotionDispatcher dispatcher = new();
            const int MotionCount = 4100;
            const int RemovedWideIndex = 3;
            const int TriggerIndex = 2000;
            const int MovedWideIndex = MotionCount - 1;
            MotionHandle[] handles = new MotionHandle[MotionCount];
            int movedMotionCancelCount = 0;
            bool requestedCancellation = false;
            bool canceledWide = false;

            for (int i = 0; i < MotionCount; i++)
            {
                if (i == RemovedWideIndex)
                {
                    handles[i] = MMotion.Create(Color.black, Color.white, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .Bind(static _ => { });
                }
                else if (i == TriggerIndex)
                {
                    handles[i] = MMotion.Create(0f, 1f, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .Bind(_ =>
                        {
                            if (requestedCancellation) return;
                            requestedCancellation = true;
                            canceledWide = handles[RemovedWideIndex].TryCancel();
                        });
                }
                else if (i == MovedWideIndex)
                {
                    handles[i] = MMotion.Create(Color.black, Color.white, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .WithOnCancel(() => movedMotionCancelCount++)
                        .Bind(static _ => { });
                }
                else
                {
                    handles[i] = MMotion.Create(0f, 1f, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .RunWithoutBinding();
                }
            }

            dispatcher.Update(1d);

            Assert.That(requestedCancellation, Is.True);
            Assert.That(canceledWide, Is.True);
            Assert.That(handles[RemovedWideIndex].IsActive(), Is.False);
            Assert.That(handles[MovedWideIndex].IsActive(), Is.True);
            Assert.That(movedMotionCancelCount, Is.Zero);

            MotionHandle replacement = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();
            Assert.That(replacement.TryCancel(), Is.True);
            dispatcher.Update(0d);
            Assert.That(movedMotionCancelCount, Is.Zero);

            Assert.That(handles[MovedWideIndex].TryCancel(), Is.True);
            dispatcher.Update(0d);
            Assert.That(movedMotionCancelCount, Is.EqualTo(1));
        }

        [Test]
        public void BuilderSettings_AreApplied()
        {
            float value = 0f;
            MotionHandle handle = MMotion.Create(0f, 10f, 1f)
                .WithEase(Ease.OutQuad)
                .WithDelay(0.25f, DelayType.FirstLoop)
                .WithLoops(2, LoopType.Restart)
                .Bind(next => value = next);

            MMotion.Update(0.5d);

            Assert.That(value, Is.EqualTo(4.375f).Within(0.0001f));
            Assert.That(handle.IsActive(), Is.True);
            Assert.That(handle.TryCancel(), Is.True);
        }

        [Test]
        public void Bind_IsImmediateByDefault_AndCanBeDisabled()
        {
            float value = -1f;
            MotionHandle immediate = MMotion.Create(3f, 7f, 1f).Bind(next => value = next);

            Assert.That(value, Is.EqualTo(3f));
            Assert.That(immediate.IsValid(), Is.True);
            Assert.That(immediate.IsActive(), Is.True);
            Assert.That(immediate.IsPlaying(), Is.True);
            immediate.Cancel();

            value = -1f;
            MotionHandle deferred = MMotion.Create(3f, 7f, 1f)
                .WithImmediateBind(false)
                .Bind(next => value = next);

            Assert.That(value, Is.EqualTo(-1f));
            Assert.That(deferred.IsValid(), Is.True);
            deferred.Cancel();
            Assert.That(deferred.IsValid(), Is.True);
            MMotion.Update(0d);
            Assert.That(deferred.IsValid(), Is.False);
            Assert.That(MotionHandle.None.IsValid(), Is.False);

            value = -1f;
            MotionHandle delayed = MMotion.Create(3f, 7f, 1f)
                .WithDelay(0.5f)
                .Bind(next => value = next);

            Assert.That(value, Is.EqualTo(3f));
            Assert.That(delayed.IsPlaying(), Is.False);
            MMotion.Update(0.25d);
            Assert.That(value, Is.EqualTo(3f));
            delayed.Cancel();
        }

        [Test]
        public void HandleProperties_TrackDurationLoopsAndDelayState()
        {
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithDelay(0.2f, DelayType.EveryLoop)
                .WithLoops(3)
                .RunWithoutBinding();

            Assert.That(handle.Delay, Is.EqualTo(0.2f));
            Assert.That(handle.Duration, Is.EqualTo(1f));
            Assert.That(handle.TotalDuration, Is.EqualTo(3.6d).Within(0.000001d));
            Assert.That(handle.Loops, Is.EqualTo(3));
            Assert.That(handle.CompletedLoops, Is.Zero);
            Assert.That(handle.Time, Is.Zero);
            Assert.That(handle.IsActive(), Is.True);
            Assert.That(handle.IsPlaying(), Is.False);

            MMotion.Update(0.25d);
            Assert.That(handle.IsPlaying(), Is.True);

            MMotion.Update(0.96d);
            Assert.That(handle.CompletedLoops, Is.EqualTo(1));
            Assert.That(handle.IsPlaying(), Is.False);
            handle.Cancel();
        }

        [Test]
        public void WideBuiltInTypes_InterpolateThroughTheSharedPayload()
        {
            double doubleValue = 0d;
            int intValue = 0;
            long longValue = 0;
            Vector2 vector2Value = Vector2.zero;
            Vector4 vector4Value = Vector4.zero;
            Quaternion quaternionValue = Quaternion.identity;
            Color colorValue = Color.black;
            Rect rectValue = default;

            MotionHandle doubleHandle = MMotion.Create(0d, 10d, 1f).Bind(value => doubleValue = value);
            MotionHandle intHandle = MMotion.Create(0, 10, 1f).Bind(value => intValue = value);
            MotionHandle longHandle = MMotion.Create(0L, 10L, 1f).Bind(value => longValue = value);
            MotionHandle vector2Handle = MMotion.Create(Vector2.zero, Vector2.one, 1f).Bind(value => vector2Value = value);
            MotionHandle vector4Handle = MMotion.Create(Vector4.zero, Vector4.one, 1f).Bind(value => vector4Value = value);
            MotionHandle quaternionHandle = MMotion.Create(Quaternion.identity, Quaternion.Euler(0f, 90f, 0f), 1f).Bind(value => quaternionValue = value);
            MotionHandle colorHandle = MMotion.Create(Color.black, Color.white, 1f).Bind(value => colorValue = value);
            MotionHandle rectHandle = MMotion.Create(new Rect(0f, 0f, 0f, 0f), new Rect(10f, 20f, 30f, 40f), 1f).Bind(value => rectValue = value);

            MMotion.Update(0.5d);

            Assert.That(doubleValue, Is.EqualTo(5d).Within(0.0001d));
            Assert.That(intValue, Is.EqualTo(5));
            Assert.That(longValue, Is.EqualTo(5L));
            Assert.That(vector2Value, Is.EqualTo(Vector2.one * 0.5f));
            Assert.That(vector4Value, Is.EqualTo(Vector4.one * 0.5f));
            Assert.That(Quaternion.Angle(quaternionValue, Quaternion.Euler(0f, 45f, 0f)), Is.LessThan(0.1f));
            Assert.That(colorValue, Is.EqualTo(new Color(0.5f, 0.5f, 0.5f, 1f)));
            Assert.That(rectValue, Is.EqualTo(new Rect(5f, 10f, 15f, 20f)));

            doubleHandle.TryCancel();
            intHandle.TryCancel();
            longHandle.TryCancel();
            vector2Handle.TryCancel();
            vector4Handle.TryCancel();
            quaternionHandle.TryCancel();
            colorHandle.TryCancel();
            rectHandle.TryCancel();
        }
    }
}

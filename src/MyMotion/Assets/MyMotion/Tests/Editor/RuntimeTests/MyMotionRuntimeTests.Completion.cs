using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void Vector3Motion_CompleteBindsAndNotifies()
        {
            Vector3 value = Vector3.zero;
            int completionCount = 0;
            MotionHandle handle = MMotion.Create(Vector3.zero, Vector3.one, 1f)
                .WithOnComplete(() => completionCount++)
                .Bind(next => value = next);

            Assert.That(handle.TryComplete(), Is.True);

            Assert.That(value, Is.EqualTo(Vector3.one));
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(handle.IsActive(), Is.False);
        }

        [Test]
        public void TryComplete_IncrementalOutQuadUsesFinalLoopValue()
        {
            float value = -1f;
            int completedLoops = 0;
            int completionCount = 0;
            MotionHandle handle = MMotion.Create(0f, 8f, 0.1f)
                .WithEase(Ease.OutQuad)
                .WithLoops(3, LoopType.Incremental)
                .WithOnLoopComplete(loop => completedLoops = loop)
                .WithOnComplete(() => completionCount++)
                .Bind(next => value = next);

            Assert.That(handle.TryComplete(), Is.True);

            Assert.That(value, Is.EqualTo(24f).Within(0.0001f));
            Assert.That(completedLoops, Is.EqualTo(3));
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(handle.IsActive(), Is.False);
        }

        [Test]
        public void TryComplete_WithoutBindingStillNotifiesCompletion()
        {
            int completionCount = 0;
            MotionHandle handle = MMotion.Create(0f, 8f, 0.1f)
                .WithLoops(3, LoopType.Incremental)
                .WithOnComplete(() => completionCount++)
                .RunWithoutBinding();

            Assert.That(handle.TryComplete(), Is.True);
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(handle.IsActive(), Is.False);
        }

        [Test]
        public void Preserve_KeepsCompletedMotionActiveUntilCanceled()
        {
            int bindingCount = 0;
            int completionCount = 0;
            int cancellationCount = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnComplete(() => completionCount++)
                .WithOnCancel(() => cancellationCount++)
                .Bind(_ => bindingCount++)
                .Preserve();

            MMotion.Update(1d);

            Assert.That(handle.IsValid(), Is.True);
            Assert.That(handle.IsActive(), Is.True);
            Assert.That(handle.IsPlaying(), Is.False);
            Assert.That(completionCount, Is.EqualTo(1));
            int bindingCountAtCompletion = bindingCount;

            MMotion.Update(0d);

            Assert.That(bindingCount, Is.EqualTo(bindingCountAtCompletion + 1));
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(handle.TryComplete(), Is.False);
            Assert.That(handle.TryCancel(), Is.True);
            Assert.That(cancellationCount, Is.EqualTo(1));
            Assert.That(handle.IsActive(), Is.False);
            Assert.That(handle.IsValid(), Is.True);

            MMotion.Update(0d);
            Assert.That(handle.IsValid(), Is.False);
        }

        [Test]
        public void Preserve_CanBeSetInsideCompletionCallback()
        {
            int completionCount = 0;
            MotionHandle handle = default;
            handle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnComplete(() =>
                {
                    completionCount++;
                    handle = handle.Preserve();
                })
                .RunWithoutBinding();

            MMotion.Update(1d);

            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(handle.IsValid(), Is.True);
            Assert.That(handle.IsActive(), Is.True);
            Assert.That(handle.TryCancel(), Is.True);
            MMotion.Update(0d);
            Assert.That(handle.IsValid(), Is.False);
        }

        [Test]
        public void Preserve_SequenceKeepsCompletedChildrenUntilCanceled()
        {
            using var dispatcher = new ManualMotionDispatcher();
            float value = -1f;
            int bindingCount = 0;
            int completeCount = 0;
            MotionHandle child = MMotion.Create(0f, 1f, 0.5f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithOnComplete(() => completeCount++)
                .Bind(next =>
                {
                    value = next;
                    bindingCount++;
                });
            MotionHandle sequence = MSequence.Create()
                .Append(child)
                .Run(builder => builder.WithScheduler(dispatcher.Scheduler).WithImmediateBind(false))
                .Preserve();

            dispatcher.Update(0.5d);

            Assert.That(sequence.IsActive(), Is.True);
            Assert.That(child.IsInSequence, Is.True);
            Assert.That(value, Is.EqualTo(1f));
            Assert.That(completeCount, Is.EqualTo(1));
            int bindingCountAtCompletion = bindingCount;

            dispatcher.Update(0d);

            Assert.That(bindingCount, Is.EqualTo(bindingCountAtCompletion + 1));
            Assert.That(completeCount, Is.EqualTo(1));
            Assert.That(sequence.TryCancel(), Is.True);
            dispatcher.Update(0d);
            Assert.That(sequence.IsValid(), Is.False);
            Assert.That(child.IsValid(), Is.False);
        }

        [Test]
        public void Preserve_CanBeSetWhileMotionBelongsToSequence()
        {
            using var dispatcher = new ManualMotionDispatcher();
            MotionHandle child = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();
            using (MotionSequenceBuilder builder = MSequence.Create())
            {
                builder.Append(child);
                Assert.DoesNotThrow(() => child.Preserve());
            }

            Assert.That(child.IsInSequence, Is.False);
            Assert.That(child.IsActive(), Is.True);
            child.TryCancel();
            dispatcher.Update(0d);
        }

        [Test]
        public void CancelOnError_DoesNotReportLoopOrCompletionAfterCancel()
        {
            int cancelCount = 0;
            int loopCount = 0;
            int completeCount = 0;
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("cancel-on-error sentinel"));
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithCancelOnError()
                .WithOnCancel(() => cancelCount++)
                .WithOnLoopComplete(_ => loopCount++)
                .WithOnComplete(() => completeCount++)
                .Bind(_ => throw new InvalidOperationException("cancel-on-error sentinel"));

            MMotion.Update(1d);

            Assert.That(cancelCount, Is.EqualTo(1));
            Assert.That(loopCount, Is.Zero);
            Assert.That(completeCount, Is.Zero);
            Assert.That(handle.IsValid(), Is.False);
        }
    }
}

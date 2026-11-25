using System;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void CompositeMotionHandle_CompletesAndCancelsAsAGroup()
        {
            int completeCount = 0;
            int cancelCount = 0;
            MotionHandle first = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnComplete(() => completeCount++)
                .RunWithoutBinding();
            MotionHandle second = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnCancel(() => cancelCount++)
                .RunWithoutBinding();
            var composite = new CompositeMotionHandle(2);

            Assert.That(first.AddTo(composite), Is.EqualTo(first));
            composite.Add(second);
            Assert.That(composite.Count, Is.EqualTo(2));
            Assert.That(composite.Contains(first), Is.True);

            composite.Complete();

            Assert.That(completeCount, Is.EqualTo(1));
            Assert.That(cancelCount, Is.Zero);
            Assert.That(first.IsActive(), Is.False);
            Assert.That(second.IsActive(), Is.False);
            Assert.That(composite.Count, Is.Zero);

            MotionHandle third = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnCancel(() => cancelCount++)
                .RunWithoutBinding();
            composite.Add(third);
            composite.Cancel();

            Assert.That(cancelCount, Is.EqualTo(1));
            Assert.That(third.IsActive(), Is.False);
            Assert.That(composite.Count, Is.Zero);
        }

        [Test]
        public void MotionHandleExtensions_DisposableAndCoroutineWait()
        {
            MotionHandle cancelHandle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .RunWithoutBinding();
            Assert.That(cancelHandle.GetDebugName(), Is.EqualTo(cancelHandle.ToString()));
            cancelHandle.ToDisposable().Dispose();
            Assert.That(cancelHandle.IsActive(), Is.False);

            int completeCount = 0;
            MotionHandle completeHandle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnComplete(() => completeCount++)
                .RunWithoutBinding();
            completeHandle.ToDisposable(DisposeBehavior.Complete).Dispose();
            Assert.That(completeCount, Is.EqualTo(1));
            Assert.That(completeHandle.IsActive(), Is.False);

            MotionHandle yieldingHandle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .RunWithoutBinding();
            System.Collections.IEnumerator wait = yieldingHandle.ToYieldInstruction();
            Assert.That(wait.MoveNext(), Is.True);
            yieldingHandle.TryComplete();
            Assert.That(wait.MoveNext(), Is.False);
        }

        [Test]
        public void MotionAwaiter_ContinuesOnCompletionAndCancellation()
        {
            int completeCount = 0;
            int completedContinuationCount = 0;
            MotionHandle completed = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnComplete(() => completeCount++)
                .RunWithoutBinding();
            MotionAwaiter completedAwaiter = completed.GetAwaiter();
            Assert.That(completedAwaiter.IsCompleted, Is.False);
            completedAwaiter.UnsafeOnCompleted(() => completedContinuationCount++);
            completed.OnComplete(() => completeCount++);

            Assert.That(completed.TryComplete(), Is.True);
            Assert.That(completedAwaiter.IsCompleted, Is.True);
            Assert.That(completeCount, Is.EqualTo(1));
            Assert.That(completedContinuationCount, Is.EqualTo(1));

            MotionHandle preserved = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .RunWithoutBinding()
                .Preserve();
            Assert.That(preserved.TryComplete(), Is.True);
            Assert.That(preserved.IsActive(), Is.True);
            Assert.That(preserved.GetAwaiter().IsCompleted, Is.True);

            int canceledContinuationCount = 0;
            MotionHandle canceled = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .RunWithoutBinding();
            canceled.GetAwaiter().UnsafeOnCompleted(() => canceledContinuationCount++);

            Assert.That(canceled.TryCancel(), Is.True);
            Assert.That(canceledContinuationCount, Is.EqualTo(1));
        }

        [Test]
        public void MotionHandleAddTo_RejectsNullTargets()
        {
            GameObject gameObject = null;
            BoxCollider component = null;
            Animator behaviour = null;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f).WithImmediateBind(false).RunWithoutBinding();

            Assert.Throws<ArgumentNullException>(() => handle.AddTo(gameObject));
            Assert.Throws<ArgumentNullException>(() => handle.AddTo(component));
            Assert.Throws<ArgumentNullException>(() => handle.AddTo(behaviour));
        }
    }
}

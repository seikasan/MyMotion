using System;
using NUnit.Framework;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void RunWithoutBinding_StillCompletesAndReportsLoops()
        {
            int completedLoops = 0;
            int completionCount = 0;
            MotionHandle handle = MMotion.Create(0f, 10f, 0.5f)
                .WithLoops(2)
                .WithOnLoopComplete(loop => completedLoops = loop)
                .WithOnComplete(() => completionCount++)
                .RunWithoutBinding();

            MMotion.Update(0.5d);
            Assert.That(completedLoops, Is.EqualTo(1));
            Assert.That(completionCount, Is.Zero);

            MMotion.Update(0.5d);
            Assert.That(completedLoops, Is.EqualTo(2));
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(handle.IsActive(), Is.False);
        }

        [Test]
        public void RunWithoutBinding_LoopCallbackCanGrowStorageDuringUpdate()
        {
            const int initialCount = 32;
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            dispatcher.EnsureStorageCapacity(initialCount);
            bool createdDuringLoopCallback = false;
            MotionHandle spawned = default;

            for (int i = 0; i < initialCount; i++)
            {
                MMotion.Create(0f, 1f, 0.5f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithLoops(2)
                    .WithOnLoopComplete(_ =>
                    {
                        if (createdDuringLoopCallback) return;
                        createdDuringLoopCallback = true;
                        spawned = MMotion.Create(0f, 1f, 1f)
                            .WithScheduler(dispatcher.Scheduler)
                            .WithImmediateBind(false)
                            .RunWithoutBinding();
                    })
                    .RunWithoutBinding();
            }

            dispatcher.Update(0.5d);

            Assert.That(createdDuringLoopCallback, Is.True);
            Assert.That(dispatcher.ActiveCount, Is.EqualTo(initialCount + 1));
            Assert.That(spawned.IsActive(), Is.True);
        }

        [Test]
        public void Sequence_AppendIntervalJoinAndInsertFollowTheTimeline()
        {
            using var dispatcher = new ManualMotionDispatcher();
            float first = -1f;
            float second = -1f;
            float joined = -1f;
            float inserted = -1f;
            int sequenceCompleted = 0;

            MotionHandle firstHandle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(value => first = value);
            MotionHandle secondHandle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(value => second = value);
            MotionHandle joinedHandle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(value => joined = value);
            MotionHandle insertedHandle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(value => inserted = value);

            MotionHandle sequence = MSequence.Create()
                .Append(firstHandle)
                .AppendInterval(0.25f)
                .Append(secondHandle)
                .Join(joinedHandle)
                .Insert(0.5f, insertedHandle)
                .Run(builder => builder
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnComplete(() => sequenceCompleted++));

            dispatcher.Update(0.5d);
            Assert.That(first, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(inserted, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(second, Is.EqualTo(0f));
            Assert.That(joined, Is.EqualTo(0f));

            dispatcher.Update(0.5d);
            Assert.That(first, Is.EqualTo(1f));
            Assert.That(inserted, Is.EqualTo(0.5f).Within(0.0001f));

            dispatcher.Update(0.5d);
            Assert.That(second, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(joined, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(inserted, Is.EqualTo(1f));

            dispatcher.Update(0.75d);
            Assert.That(second, Is.EqualTo(1f));
            Assert.That(joined, Is.EqualTo(1f));
            Assert.That(sequence.IsActive(), Is.False);
            Assert.That(sequenceCompleted, Is.EqualTo(1));
            Assert.That(dispatcher.ActiveCount, Is.Zero);
        }

        [Test]
        public void Sequence_NestedSequenceSeeksItsChildren()
        {
            using var dispatcher = new ManualMotionDispatcher();
            float first = -1f;
            float second = -1f;
            float final = -1f;

            MotionHandle firstHandle = MMotion.Create(0f, 1f, 0.5f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(value => first = value);
            MotionHandle secondHandle = MMotion.Create(1f, 0f, 0.5f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(value => second = value);
            MotionHandle nested = MSequence.Create()
                .Append(firstHandle)
                .Append(secondHandle)
                .Run(builder => builder.WithScheduler(dispatcher.Scheduler).WithImmediateBind(false));

            MotionHandle finalHandle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(value => final = value);
            MotionHandle sequence = MSequence.Create()
                .Append(nested)
                .Append(finalHandle)
                .Run(builder => builder.WithScheduler(dispatcher.Scheduler).WithImmediateBind(false));

            dispatcher.Update(0.25d);
            Assert.That(first, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(second, Is.EqualTo(1f));
            Assert.That(final, Is.EqualTo(0f));

            dispatcher.Update(0.5d);
            Assert.That(first, Is.EqualTo(1f));
            Assert.That(second, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(final, Is.EqualTo(0f));

            dispatcher.Update(0.5d);
            Assert.That(second, Is.EqualTo(0f));
            Assert.That(final, Is.EqualTo(0.25f).Within(0.0001f));

            dispatcher.Update(0.75d);
            Assert.That(final, Is.EqualTo(1f));
            Assert.That(sequence.IsActive(), Is.False);
            Assert.That(dispatcher.ActiveCount, Is.Zero);
        }

        [Test]
        public void Sequence_CompletedChildrenCompleteAndOnlyRemainingChildrenCancel()
        {
            using var dispatcher = new ManualMotionDispatcher();
            int completedChildComplete = 0;
            int completedChildCancel = 0;
            int remainingChildComplete = 0;
            int remainingChildCancel = 0;

            MotionHandle completedChild = MMotion.Create(0f, 1f, 0.25f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => completedChildComplete++)
                .WithOnCancel(() => completedChildCancel++)
                .RunWithoutBinding();
            MotionHandle remainingChild = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => remainingChildComplete++)
                .WithOnCancel(() => remainingChildCancel++)
                .RunWithoutBinding();
            MotionHandle sequence = MSequence.Create()
                .Append(completedChild)
                .Append(remainingChild)
                .Run(builder => builder.WithScheduler(dispatcher.Scheduler).WithImmediateBind(false));

            dispatcher.Update(0.5d);
            Assert.That(completedChildComplete, Is.EqualTo(1));
            Assert.That(completedChildCancel, Is.Zero);

            sequence.Cancel();
            dispatcher.Update(0d);

            Assert.That(completedChildComplete, Is.EqualTo(1));
            Assert.That(completedChildCancel, Is.Zero);
            Assert.That(remainingChildComplete, Is.Zero);
            Assert.That(remainingChildCancel, Is.EqualTo(1));
            Assert.That(dispatcher.ActiveCount, Is.Zero);
        }

        [Test]
        public void Sequence_CompleteEvaluatesEveryChildAndNotifiesThem()
        {
            using var dispatcher = new ManualMotionDispatcher();
            float firstValue = -1f;
            float secondValue = -1f;
            int firstCompleted = 0;
            int secondCompleted = 0;

            MotionHandle first = MMotion.Create(0f, 1f, 0.5f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => firstCompleted++)
                .Bind(value => firstValue = value);
            MotionHandle second = MMotion.Create(1f, 0f, 0.5f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => secondCompleted++)
                .Bind(value => secondValue = value);
            MotionHandle sequence = MSequence.Create()
                .Append(first)
                .Append(second)
                .Run(builder => builder.WithScheduler(dispatcher.Scheduler).WithImmediateBind(false));

            sequence.Complete();
            dispatcher.Update(0d);

            Assert.That(firstValue, Is.EqualTo(1f));
            Assert.That(secondValue, Is.EqualTo(0f));
            Assert.That(firstCompleted, Is.EqualTo(1));
            Assert.That(secondCompleted, Is.EqualTo(1));
            Assert.That(dispatcher.ActiveCount, Is.Zero);
        }

        [Test]
        public void Sequence_ReleasingCompletedNestedSequenceDoesNotCancelItsChildren()
        {
            using var dispatcher = new ManualMotionDispatcher();
            int nestedChildComplete = 0;
            int nestedChildCancel = 0;
            int nestedComplete = 0;
            int nestedCancel = 0;
            int remainingChildCancel = 0;

            MotionHandle nestedChild = MMotion.Create(0f, 1f, 0.25f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => nestedChildComplete++)
                .WithOnCancel(() => nestedChildCancel++)
                .RunWithoutBinding();
            MotionHandle nestedSequence = MSequence.Create()
                .Append(nestedChild)
                .Run(builder => builder
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnComplete(() => nestedComplete++)
                    .WithOnCancel(() => nestedCancel++));
            MotionHandle remainingChild = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnCancel(() => remainingChildCancel++)
                .RunWithoutBinding();
            MotionHandle sequence = MSequence.Create()
                .Append(nestedSequence)
                .Append(remainingChild)
                .Run(builder => builder.WithScheduler(dispatcher.Scheduler).WithImmediateBind(false));

            dispatcher.Update(0.5d);
            Assert.That(nestedChildComplete, Is.EqualTo(1));
            Assert.That(nestedComplete, Is.EqualTo(1));

            sequence.Cancel();
            dispatcher.Update(0d);

            Assert.That(nestedChildComplete, Is.EqualTo(1));
            Assert.That(nestedChildCancel, Is.Zero);
            Assert.That(nestedCancel, Is.Zero);
            Assert.That(remainingChildCancel, Is.EqualTo(1));
            Assert.That(dispatcher.ActiveCount, Is.Zero);
        }

        [Test]
        public void Sequence_ControlsChildrenAndCancelNotifiesThem()
        {
            using var dispatcher = new ManualMotionDispatcher();
            int firstCanceled = 0;
            int secondCanceled = 0;
            int sequenceCanceled = 0;
            MotionHandle first = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnCancel(() => firstCanceled++)
                .RunWithoutBinding();
            MotionHandle second = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnCancel(() => secondCanceled++)
                .RunWithoutBinding();
            MotionHandle sequence = MSequence.Create()
                .Append(first)
                .Append(second)
                .Run(builder => builder
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnCancel(() => sequenceCanceled++));

            Assert.Throws<InvalidOperationException>(() => first.Time = 0d);
            Assert.Throws<InvalidOperationException>(() => first.Cancel());
            Assert.Throws<InvalidOperationException>(() => first.Complete());
            Assert.That(first.TryCancel(), Is.False);
            Assert.That(first.TryComplete(), Is.False);

            sequence.Cancel();
            dispatcher.Update(0d);

            Assert.That(firstCanceled, Is.EqualTo(1));
            Assert.That(secondCanceled, Is.EqualTo(1));
            Assert.That(sequenceCanceled, Is.EqualTo(1));
            Assert.That(dispatcher.ActiveCount, Is.Zero);
        }

        [Test]
        public void Sequence_RejectsRunningAndInfiniteMotions()
        {
            using var dispatcher = new ManualMotionDispatcher();
            MotionHandle running = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();
            dispatcher.Update(0.1d);

            Assert.Throws<ArgumentException>(() => MSequence.Create().Append(running));

            MotionHandle infinite = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithLoops(-1)
                .RunWithoutBinding();
            Assert.Throws<ArgumentException>(() => MSequence.Create().Append(infinite));
        }

        [Test]
        public void Sequence_DisposingBuilderRestoresItsMotions()
        {
            using var dispatcher = new ManualMotionDispatcher();
            float value = -1f;
            MotionHandle child = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(next => value = next);

            using (MSequence.Create().Append(child))
            {
            }

            dispatcher.Update(0.5d);

            Assert.That(value, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(child.IsActive(), Is.True);
            child.Cancel();
        }
    }
}

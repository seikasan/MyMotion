using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public async Task MotionHandleToValueTask_CompletesAndPreservesBuilderCallbacks()
        {
            using ManualMotionDispatcher dispatcher = new();
            int completed = 0;
            int canceled = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithOnComplete(() => completed++)
                .WithOnCancel(() => canceled++)
                .RunWithoutBinding();

            ValueTask task = handle.ToValueTask();
            Assert.That(task.IsCompleted, Is.False);

            dispatcher.Update(1d);

            Assert.That(task.IsCompletedSuccessfully, Is.True);
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(canceled, Is.Zero);
            await task;
        }

        [Test]
        public async Task MotionHandleToValueTask_DefaultTokenBehaviorCancelsMotionAndWait()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            int canceled = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnCancel(() => canceled++)
                .RunWithoutBinding();

            ValueTask task = handle.ToValueTask(cancellation.Token);
            cancellation.Cancel();

            Assert.That(handle.IsActive(), Is.False);
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(canceled, Is.EqualTo(1));
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }

        [Test]
        public async Task MotionHandleToValueTask_NoneBehaviorCancelsWaitWithoutCancelingMotion()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();

            ValueTask task = handle.ToValueTask(CancelBehavior.None, cancellation.Token);
            cancellation.Cancel();

            Assert.That(handle.IsActive(), Is.True);
            Assert.That(task.IsCanceled, Is.True);
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
            Assert.That(handle.TryCancel(), Is.True);
        }

        [Test]
        public async Task MotionHandleToValueTask_CompleteBehaviorCompletesMotionAndCancelsWait()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            int completed = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => completed++)
                .RunWithoutBinding();

            ValueTask task = handle.ToValueTask(CancelBehavior.Complete, cancellation.Token);
            cancellation.Cancel();

            Assert.That(handle.IsActive(), Is.False);
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(task.IsCanceled, Is.True);
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }

        [Test]
        public async Task MotionHandleToValueTask_CanCompleteWaitWhenMotionIsCanceled()
        {
            using ManualMotionDispatcher dispatcher = new();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();

            ValueTask task = handle.ToValueTask(CancelBehavior.None, cancelAwaitOnMotionCanceled: false);
            Assert.That(handle.TryCancel(), Is.True);

            Assert.That(task.IsCompletedSuccessfully, Is.True);
            await task;
        }

        [Test]
        public async Task MotionHandleToValueTask_DirectMotionCancellationCancelsWaitByDefault()
        {
            using ManualMotionDispatcher dispatcher = new();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();

            ValueTask task = handle.ToValueTask();
            Assert.That(handle.TryCancel(), Is.True);

            Assert.That(task.IsCanceled, Is.True);
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }

        [Test]
        public async Task MotionHandleToValueTask_AlreadyCanceledTokenAppliesBehaviorBeforeReturningCanceledWait()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            int completed = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => completed++)
                .RunWithoutBinding();
            cancellation.Cancel();

            ValueTask task = handle.ToValueTask(CancelBehavior.Complete, cancellation.Token);

            Assert.That(handle.IsActive(), Is.False);
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(task.IsCanceled, Is.True);
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }

        [Test]
        public async Task MotionHandleToValueTask_InactiveHandleReturnsCompletedWait()
        {
            ValueTask task = default(MotionHandle).ToValueTask();

            Assert.That(task.IsCompletedSuccessfully, Is.True);
            await task;
        }

        [Test]
        public async Task MotionHandleToValueTask_InactiveHandleWithCanceledTokenReturnsCompletedWait()
        {
            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();

            ValueTask task = default(MotionHandle).ToValueTask(cancellation.Token);

            Assert.That(task.IsCompletedSuccessfully, Is.True);
            await task;
        }

#if UNITY_2023_1_OR_NEWER
        [Test]
        public async Task MotionHandleToAwaitable_CompletesAndPreservesBuilderCallbacks()
        {
            using ManualMotionDispatcher dispatcher = new();
            int completed = 0;
            int canceled = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => completed++)
                .WithOnCancel(() => canceled++)
                .RunWithoutBinding();

            Awaitable task = handle.ToAwaitable();
            dispatcher.Update(1d);

            Assert.That(completed, Is.EqualTo(1));
            Assert.That(canceled, Is.Zero);
            await task;
        }

        [Test]
        public async Task MotionHandleToAwaitable_DefaultTokenBehaviorCancelsMotionAndWait()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            int canceled = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnCancel(() => canceled++)
                .RunWithoutBinding();

            Awaitable task = handle.ToAwaitable(cancellation.Token);
            cancellation.Cancel();

            Assert.That(handle.IsActive(), Is.False);
            Assert.That(canceled, Is.EqualTo(1));
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }

        [Test]
        public async Task MotionHandleToAwaitable_NoneBehaviorCancelsWaitWithoutCancelingMotion()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();

            Awaitable task = handle.ToAwaitable(CancelBehavior.None, cancellation.Token);
            cancellation.Cancel();

            Assert.That(handle.IsActive(), Is.True);
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
            Assert.That(handle.TryCancel(), Is.True);
        }

        [Test]
        public async Task MotionHandleToAwaitable_MotionCancellationCanCompleteWaitSuccessfully()
        {
            using ManualMotionDispatcher dispatcher = new();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();

            Awaitable task = handle.ToAwaitable(CancelBehavior.None, cancelAwaitOnMotionCanceled: false);
            Assert.That(handle.TryCancel(), Is.True);

            await task;
        }

        [Test]
        public async Task MotionHandleToAwaitable_DirectMotionCancellationCancelsWaitByDefault()
        {
            using ManualMotionDispatcher dispatcher = new();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();

            Awaitable task = handle.ToAwaitable();
            Assert.That(handle.TryCancel(), Is.True);

            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }

        [Test]
        public async Task MotionHandleToAwaitable_AlreadyCanceledTokenAppliesBehaviorAndReturnsCanceledWait()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            int completed = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithOnComplete(() => completed++)
                .RunWithoutBinding();
            cancellation.Cancel();

            Awaitable task = handle.ToAwaitable(CancelBehavior.Complete, cancellation.Token);

            Assert.That(handle.IsActive(), Is.False);
            Assert.That(completed, Is.EqualTo(1));
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }

        [Test]
        public async Task MotionHandleToAwaitable_InactiveHandleWithCanceledTokenReturnsCompletedWait()
        {
            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();

            Awaitable task = default(MotionHandle).ToAwaitable(cancellation.Token);

            await task;
        }

        [Test]
        public async Task MotionHandleToAwaitable_CompletedAwaitsHaveIndependentSources()
        {
            Awaitable first = default(MotionHandle).ToAwaitable();
            Awaitable second = default(MotionHandle).ToAwaitable();

            await first;
            await second;
        }

        [Test]
        public async Task MotionHandleToAwaitable_PreCanceledAwaitsHaveIndependentSources()
        {
            using ManualMotionDispatcher dispatcher = new();
            using CancellationTokenSource cancellation = new();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .RunWithoutBinding();
            cancellation.Cancel();

            Awaitable first = handle.ToAwaitable(CancelBehavior.None, cancellation.Token);
            Awaitable second = handle.ToAwaitable(CancelBehavior.None, cancellation.Token);

            Assert.CatchAsync<OperationCanceledException>(async () => await first);
            Assert.CatchAsync<OperationCanceledException>(async () => await second);
            Assert.That(handle.TryCancel(), Is.True);
        }
#endif
    }
}

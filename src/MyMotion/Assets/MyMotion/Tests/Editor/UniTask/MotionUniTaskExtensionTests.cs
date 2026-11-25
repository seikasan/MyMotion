using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MyMotion.Extensions;
using NUnit.Framework;

namespace MyMotion.Tests
{
    public sealed class MotionUniTaskExtensionTests
    {
        [SetUp]
        public void SetUp() => MMotion.Reset();

        [TearDown]
        public void TearDown() => MMotion.Reset();

        [Test]
        public async Task ToUniTask_CompletesAndPreservesMotionCallbacks()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            int completed = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithOnComplete(() => completed++)
                .RunWithoutBinding();

            UniTask task = handle.ToUniTask();
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending));

            dispatcher.Update(1d);

            Assert.That(completed, Is.EqualTo(1));
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            await task;
        }

        [Test]
        public async Task ToUniTask_DefaultCancellationCancelsMotionAndAwait()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .RunWithoutBinding();

            UniTask task = handle.ToUniTask(cancellation.Token);
            cancellation.Cancel();

            Assert.That(handle.IsActive(), Is.False);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));

            bool wasCanceled = false;
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                wasCanceled = true;
            }

            Assert.That(wasCanceled, Is.True);
        }

        [Test]
        public async Task ToUniTask_NoneBehaviorCancelsAwaitWithoutCancelingMotion()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .RunWithoutBinding();

            UniTask task = handle.ToUniTask(CancelBehavior.None, cancellation.Token);
            cancellation.Cancel();

            Assert.That(handle.IsActive(), Is.True);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            handle.TryCancel();

            bool wasCanceled = false;
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                wasCanceled = true;
            }

            Assert.That(wasCanceled, Is.True);
        }

        [Test]
        public async Task ToUniTask_CanCompleteAwaitWhenMotionIsCanceled()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            MotionHandle handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .RunWithoutBinding();

            UniTask task = handle.ToUniTask(CancelBehavior.None, cancelAwaitOnMotionCanceled: false);
            Assert.That(handle.TryCancel(), Is.True);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            await task;
        }

        [Test]
        public void BindToAsyncReactiveProperty_WritesStandardAndAdapterValues()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            AsyncReactiveProperty<float> standardProperty = new AsyncReactiveProperty<float>(-1f);
            AsyncReactiveProperty<float> adapterProperty = new AsyncReactiveProperty<float>(-1f);

            MotionHandle standardHandle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToAsyncReactiveProperty(standardProperty);
            MotionHandle adapterHandle = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToAsyncReactiveProperty(adapterProperty);

            dispatcher.Update(0.5d);

            Assert.That(standardProperty.Value, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(adapterProperty.Value, Is.EqualTo(0.5f).Within(0.0001f));

            standardHandle.TryCancel();
            adapterHandle.TryCancel();
        }

        private readonly struct LinearFloatAdapter : IMotionAdapter<float, NoMotionOptions>
        {
            public float Evaluate(
                ref float startValue,
                ref float endValue,
                ref NoMotionOptions options,
                in MotionEvaluationContext context) =>
                startValue + (endValue - startValue) * context.Progress;
        }
    }
}


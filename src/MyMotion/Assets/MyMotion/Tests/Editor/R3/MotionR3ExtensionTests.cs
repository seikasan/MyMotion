using System;
using System.Collections.Generic;
using MyMotion.Extensions;
using NUnit.Framework;
using R3;

namespace MyMotion.Tests
{
    public sealed class MotionR3ExtensionTests
    {
        [SetUp]
        public void SetUp() => MMotion.Reset();

        [TearDown]
        public void TearDown() => MMotion.Reset();

        [Test]
        public void ToObservable_EmitsValuesCompletesAndPreservesBuilderCallback()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            int completedCallbackCount = 0;
            Observable<float> observable = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .WithOnComplete(() => completedCallbackCount++)
                .ToObservable();
            RecordingObserver<float> observer = new RecordingObserver<float>();
            observable.Subscribe(observer);

            dispatcher.Update(1d);

            Assert.That(observer.Values, Is.EqualTo(new[] { 1f }));
            Assert.That(observer.Completed, Is.True);
            Assert.That(completedCallbackCount, Is.EqualTo(1));
        }

        [Test]
        public void ToObservable_SupportsAdapterBuilder()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            Observable<float> observable = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .ToObservable();
            RecordingObserver<float> observer = new RecordingObserver<float>();
            observable.Subscribe(observer);

            dispatcher.Update(1d);

            Assert.That(observer.Values, Is.EqualTo(new[] { 1f }));
            Assert.That(observer.Completed, Is.True);
        }

        [Test]
        public void BindToReactiveProperty_SupportsStandardAndAdapterBuilders()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            ReactiveProperty<float> standardProperty = new ReactiveProperty<float>(-1f);
            ReactiveProperty<float> adapterProperty = new ReactiveProperty<float>(-1f);

            MotionHandle standardHandle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToReactiveProperty(standardProperty);
            MotionHandle adapterHandle = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToReactiveProperty(adapterProperty);

            dispatcher.Update(0.5d);

            Assert.That(standardProperty.Value, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(adapterProperty.Value, Is.EqualTo(0.5f).Within(0.0001f));

            standardHandle.TryCancel();
            adapterHandle.TryCancel();
            standardProperty.Dispose();
            adapterProperty.Dispose();
        }

        [Test]
        public void AddTo_SupportsCompositeSerialBagAndBuilder()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            CompositeDisposable composite = new CompositeDisposable();
            MotionHandle compositeHandle = CreateMotion(dispatcher).AddTo(composite);
            composite.Dispose();

            SerialDisposable serial = new SerialDisposable();
            MotionHandle serialHandle = CreateMotion(dispatcher).AddTo(serial, DisposeBehavior.Complete);
            serial.Dispose();

            DisposableBag bag = default;
            MotionHandle bagHandle = CreateMotion(dispatcher).AddTo(ref bag);
            bag.Dispose();

            DisposableBuilder builder = Disposable.CreateBuilder();
            MotionHandle builderHandle = CreateMotion(dispatcher).AddTo(ref builder);
            IDisposable builderDisposable = builder.Build();
            builderDisposable.Dispose();

            Assert.That(compositeHandle.IsActive(), Is.False);
            Assert.That(serialHandle.IsActive(), Is.False);
            Assert.That(bagHandle.IsActive(), Is.False);
            Assert.That(builderHandle.IsActive(), Is.False);
        }

        private static MotionHandle CreateMotion(ManualMotionDispatcher dispatcher) =>
            MMotion.Create(0f, 1f, 10f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .RunWithoutBinding();

        private sealed class RecordingObserver<T> : Observer<T>
        {
            public List<T> Values { get; } = new List<T>();
            public bool Completed { get; private set; }

            protected override void OnNextCore(T value) => Values.Add(value);
            protected override void OnErrorResumeCore(Exception error) => throw error;
            protected override void OnCompletedCore(Result result) => Completed = true;
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


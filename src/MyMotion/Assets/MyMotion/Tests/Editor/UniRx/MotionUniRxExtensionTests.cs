using System;
using System.Collections.Generic;
using MyMotion.Extensions;
using NUnit.Framework;
using UniRx;

namespace MyMotion.Tests
{
    public sealed class MotionUniRxExtensionTests
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
            IObservable<float> observable = MMotion.Create(0f, 1f, 1f)
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
            IObservable<float> observable = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
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

        private sealed class RecordingObserver<T> : IObserver<T>
        {
            public List<T> Values { get; } = new List<T>();
            public bool Completed { get; private set; }

            public void OnNext(T value) => Values.Add(value);

            public void OnError(Exception error) => throw error;

            public void OnCompleted() => Completed = true;
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


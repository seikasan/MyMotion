using NUnit.Framework;
using Unity.Collections;
using UnityEngine.UIElements;
using MyMotion.Extensions;

namespace MyMotion.Tests
{
    public sealed class MotionUIToolkitExtensionTests
    {
        [Test]
        public void DefaultFloatMotion_BindsToStyleOpacity()
        {
            using ManualMotionDispatcher dispatcher = new();
            var element = new VisualElement();
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToStyleOpacity(element);

            dispatcher.Update(0.5d);

            Assert.That(element.style.opacity.value, Is.EqualTo(0.5f).Within(0.0001f));
            handle.TryCancel();
        }

        [Test]
        public void GenericAdapterMotion_BindsToProgressBar()
        {
            using ManualMotionDispatcher dispatcher = new();
            var progressBar = new ProgressBar();
            MotionHandle handle = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToProgressBar(progressBar);

            dispatcher.Update(0.25d);

            Assert.That(progressBar.value, Is.EqualTo(0.25f).Within(0.0001f));
            handle.TryCancel();
        }

        [Test]
        public void FixedStringMotion_BindsToTextElement()
        {
            using ManualMotionDispatcher dispatcher = new();
            var label = new Label();
            var start = new FixedString32Bytes("start");
            var end = new FixedString32Bytes("end");
            MotionHandle handle = MMotion.String.Create32Bytes(start, end, 1f)
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToText(label);

            dispatcher.Update(1d);

            Assert.That(label.text, Is.EqualTo("end"));
            Assert.That(handle.IsActive(), Is.False);
        }

        [Test]
        public void IntegerMotion_FormatsTextElementValue()
        {
            using ManualMotionDispatcher dispatcher = new();
            var label = new Label();
            MotionHandle handle = MMotion.Create(0, 42, 1f)
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToText(label, "Score: {0}");

            dispatcher.Update(1d);

            Assert.That(label.text, Is.EqualTo("Score: 42"));
            Assert.That(handle.IsActive(), Is.False);
        }

        private readonly struct LinearFloatAdapter : IMotionAdapter<float, NoMotionOptions>
        {
            public float Evaluate(ref float startValue, ref float endValue, ref NoMotionOptions options, in MotionEvaluationContext context)
                => startValue + (endValue - startValue) * context.Progress;
        }
    }
}
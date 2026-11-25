using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyMotion.Extensions;

namespace MyMotion.Tests
{
    public sealed class MyMotionUGUIExtensionTests
    {
        [SetUp]
        public void SetUp() => MMotion.Reset();

        [TearDown]
        public void TearDown() => MMotion.Reset();

        [Test]
        public void BuiltInColorMotion_BindsToGraphic()
        {
            GameObject gameObject = new("MyMotion uGUI Graphic Test");
            Image image = gameObject.AddComponent<Image>();
            Color from = new(0.1f, 0.2f, 0.3f, 0.4f);
            Color to = new(0.9f, 0.8f, 0.7f, 0.6f);

            MMotion.Create(from, to, 1f)
                .WithImmediateBind()
                .BindToColor(image);

            Assert.That(image.color, Is.EqualTo(from));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void CustomAdapterMotion_BindsToCanvasGroupAlpha()
        {
            GameObject gameObject = new("MyMotion uGUI CanvasGroup Test");
            CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();

            MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0.25f, 0.75f, 1f)
                .WithImmediateBind()
                .BindToAlpha(canvasGroup);

            Assert.That(canvasGroup.alpha, Is.EqualTo(0.25f));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void FixedStringMotion_BindsToLegacyText()
        {
            GameObject gameObject = new("MyMotion uGUI Text Test");
            Text text = gameObject.AddComponent<Text>();
            FixedString32Bytes from = new("start");
            FixedString32Bytes to = new("finish");

            MMotion.String.Create32Bytes(in from, in to, 1f)
                .WithImmediateBind()
                .BindToText(text);

            Assert.That(text.text, Is.EqualTo("start"));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void IntegerMotion_BindsFormattedText()
        {
            GameObject gameObject = new("MyMotion uGUI Formatted Text Test");
            Text text = gameObject.AddComponent<Text>();

            MMotion.Create(7, 15, 1f)
                .WithImmediateBind()
                .BindToText(text, "value={0:D2}");

            Assert.That(text.text, Is.EqualTo("value=07"));
            Object.DestroyImmediate(gameObject);
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

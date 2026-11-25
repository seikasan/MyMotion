using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using MyMotion.Extensions;

namespace MyMotion.Tests
{
    public sealed class MotionVolumeExtensionTests
    {
        [Test]
        public void StandardFloatMotion_BindsToVolumeWeight()
        {
            using ManualMotionDispatcher dispatcher = new();
            GameObject gameObject = new(nameof(StandardFloatMotion_BindsToVolumeWeight));
            try
            {
                Volume volume = gameObject.AddComponent<Volume>();
                MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                    .WithEase(Ease.Linear)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .BindToWeight(volume);

                dispatcher.Update(0.4d);

                Assert.That(volume.weight, Is.EqualTo(0.4f).Within(0.0001f));
                handle.TryCancel();
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void GenericAdapterFloatMotion_BindsToVolumeWeight()
        {
            using ManualMotionDispatcher dispatcher = new();
            GameObject gameObject = new(nameof(GenericAdapterFloatMotion_BindsToVolumeWeight));
            try
            {
                Volume volume = gameObject.AddComponent<Volume>();
                MotionHandle handle = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                    .WithEase(Ease.Linear)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .BindToWeight(volume);

                dispatcher.Update(0.6d);

                Assert.That(volume.weight, Is.EqualTo(0.6f).Within(0.0001f));
                handle.TryCancel();
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private readonly struct LinearFloatAdapter : IMotionAdapter<float, NoMotionOptions>
        {
            public float Evaluate(ref float startValue, ref float endValue, ref NoMotionOptions options, in MotionEvaluationContext context)
                => startValue + (endValue - startValue) * context.Progress;
        }
    }
}

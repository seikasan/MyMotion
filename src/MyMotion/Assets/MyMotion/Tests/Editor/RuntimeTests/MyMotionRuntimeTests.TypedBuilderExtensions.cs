using MyMotion.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        // LitMotion と同じく、Punch / Shake の型付き builder を Transform などの拡張で Bind できる。
        [Test]
        public void TypedBuilders_BindWithGeneralExtensions()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            var gameObject = new GameObject("TypedBuilderTarget");
            try
            {
                Transform transform = gameObject.transform;

                MotionHandle punch = MMotion.Punch.Create(Vector3.zero, Vector3.one, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .BindToLocalPosition(transform);
                MotionHandle shake = MMotion.Shake.Create(1f, 0.5f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .BindToLocalScaleX(transform);
                MotionHandle angle = MMotion.Punch.Create(0f, 30f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .BindToEulerAnglesZ(transform);

                // Shake は乱数で揺れるため、数フレームのうちに一度でも初期値から動けばよい。
                bool scaleMoved = false;
                for (int i = 0; i < 10; i++)
                {
                    dispatcher.Update(0.05d);
                    scaleMoved |= transform.localScale.x != 1f;
                }

                Assert.That(transform.localPosition, Is.Not.EqualTo(Vector3.zero));
                Assert.That(scaleMoved, Is.True);
                Assert.That(punch.IsActive() && shake.IsActive() && angle.IsActive(), Is.True);

                Vector3 expected = transform.localPosition;
                dispatcher.Update(0d);
                Assert.That(transform.localPosition, Is.EqualTo(expected));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void TypedBuilders_RejectNullTransform()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            Assert.Throws<System.ArgumentNullException>(() => MMotion.Punch.Create(Vector3.zero, Vector3.one, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .BindToPosition((Transform)null));
        }
    }
}

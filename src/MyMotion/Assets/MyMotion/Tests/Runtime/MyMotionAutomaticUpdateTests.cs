using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyMotion.Tests
{
    public sealed class MyMotionAutomaticUpdateTests
    {
        [UnityTest]
        public IEnumerator DefaultRunner_UpdatesBoundMotion()
        {
            Assert.That(MMotion.ReservedNativeBytes, Is.Zero);
            yield return null;
            Assert.That(MMotion.ReservedNativeBytes, Is.Zero);

            float value = 0f;
            MotionHandle handle = MMotion.Create(0f, 1f, 10f).Bind(next => value = next);

            yield return null;

            Assert.That(value, Is.GreaterThan(0f));
            Assert.That(handle.IsActive(), Is.True);
            handle.TryCancel();
        }
    }
}

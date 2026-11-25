using System;
using System.Collections;
using MyMotion;
using MyMotion.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace MyMotion.Tests.Upstream
{
    public class PlaybackSpeedTest
    {
        [UnityTest]
        public IEnumerator Test_PlaybackSpeed()
        {
            var endValue = 10f;
            var handle = MMotion.Create(0f, endValue, 1f)
                .BindToUnityLogger();
            handle.PlaybackSpeed = 0.5f;

            var time = Time.timeAsDouble;
            yield return handle.ToYieldInstruction();
            Assert.That(Time.timeAsDouble - time, Is.GreaterThan(2.0));
        }

        [UnityTest]
        public IEnumerator Test_PlaybackSpeed_Pause()
        {
            var endValue = 10f;
            var value = 0f;
            var handle = MMotion.Create(0f, endValue, 1f)
                .Bind(x => value = x);

            handle.PlaybackSpeed = 0f;
            yield return new WaitForSeconds(0.5f);
            Assert.That(value, Is.EqualTo(0f));

            handle.Cancel();
        }

        [UnityTest]
        public IEnumerator Test_PlaybackSpeed_2x_Speed()
        {
            var endValue = 10f;
            var value = 0f;
            var handle = MMotion.Create(0f, endValue, 1f)
                .Bind(x => value = x);

            handle.PlaybackSpeed = 2f;
            var time = Time.time;
            yield return handle.ToYieldInstruction();
            Assert.That(Time.time - time, Is.EqualTo(0.5f).Using(new FloatEqualityComparer(0.05f)));
        }
    }
}
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyMotion.Tests
{
    public sealed class MyMotionPlayerLoopSchedulerTests
    {
        private MotionHandle _handle;
        private MotionHandle _secondHandle;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            _handle.TryCancel();
            _secondHandle.TryCancel();
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator FixedUpdateScheduler_BindsInFixedTimeStep()
        {
            bool boundDuringFixedUpdate = false;
            _handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(MotionScheduler.FixedUpdate)
                .Bind(_ => boundDuringFixedUpdate |= Time.inFixedTimeStep);

            yield return new WaitForFixedUpdate();

            Assert.That(boundDuringFixedUpdate, Is.True);
            _handle.TryCancel();
        }

        [UnityTest]
        public IEnumerator PlayerLoopSchedulers_AdvanceInAllEightPhases()
        {
            IMotionScheduler[] schedulers =
            {
                MotionScheduler.Initialization,
                MotionScheduler.EarlyUpdate,
                MotionScheduler.FixedUpdate,
                MotionScheduler.PreUpdate,
                MotionScheduler.Update,
                MotionScheduler.PreLateUpdate,
                MotionScheduler.PostLateUpdate,
                MotionScheduler.TimeUpdate
            };

            foreach (IMotionScheduler scheduler in schedulers)
            {
                float value = 0f;
                _handle = MMotion.Create(0f, 1f, 10f)
                    .WithScheduler(scheduler)
                    .Bind(next => value = next);

                if (ReferenceEquals(scheduler, MotionScheduler.FixedUpdate))
                {
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    yield return null;
                }

                Assert.That(value, Is.GreaterThan(0f), scheduler.ToString());
                _handle.TryCancel();
            }
        }

        [UnityTest]
        public IEnumerator RealtimeScheduler_AdvancesWhenTimeScaleIsZero()
        {
            float initialTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            float value = 0f;
            _handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(MotionScheduler.PreUpdateRealtime)
                .Bind(next => value = next);
            try
            {
                yield return null;
                Assert.That(value, Is.GreaterThan(0f));
            }
            finally
            {
                _handle.TryCancel();
                Time.timeScale = initialTimeScale;
            }
        }

        [UnityTest]
        public IEnumerator RealtimeScheduler_NewWorldStartsFromMotionCreationTime()
        {
            MethodInfo reset = typeof(MotionScheduler).GetMethod("Reset", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(reset, Is.Not.Null);
            reset.Invoke(null, null);

            yield return new WaitForSecondsRealtime(0.75f);

            float value = 0f;
            _handle = MMotion.Create(0f, 1f, 0.5f)
                .WithScheduler(MotionScheduler.TimeUpdateRealtime)
                .WithImmediateBind(false)
                .Bind(next => value = next);

            yield return null;

            Assert.That(_handle.IsActive(), Is.True);
            Assert.That(value, Is.LessThan(0.5f));
        }

        [UnityTest]
        public IEnumerator PlayerLoopScheduler_BindsWithTwoAndThreeReferenceStates()
        {
            var twoStateOutput = new float[1];
            var twoStateOffset = new float[] { 2f };
            var threeStateOutput = new float[1];
            var threeStateScale = new float[] { 3f };
            var threeStateOffset = new float[] { 4f };

            _handle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(MotionScheduler.Update)
                .WithImmediateBind(false)
                .Bind(twoStateOutput, twoStateOffset, static (value, output, offset) => output[0] = value + offset[0]);
            _secondHandle = MMotion.Create(0f, 1f, 10f)
                .WithScheduler(MotionScheduler.Update)
                .WithImmediateBind(false)
                .Bind(threeStateOutput, threeStateScale, threeStateOffset, static (value, output, scale, offset) => output[0] = value * scale[0] + offset[0]);

            yield return null;

            Assert.That(twoStateOutput[0], Is.GreaterThan(2f));
            Assert.That(threeStateOutput[0], Is.GreaterThan(4f));
            Assert.That(_handle.IsActive(), Is.True);
            Assert.That(_secondHandle.IsActive(), Is.True);
        }
    }
}

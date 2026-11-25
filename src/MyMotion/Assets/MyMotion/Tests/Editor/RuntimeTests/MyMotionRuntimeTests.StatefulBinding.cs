using NUnit.Framework;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void StatefulBinding_UsesTypedState()
        {
            var target = new Target();
            MotionHandle handle = MMotion.Create(4f, 8f, 1f)
                .Bind(target, static (value, state) => state.Value = value);

            MMotion.Update(0.25d);

            Assert.That(target.Value, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(handle.TryCancel(), Is.True);
        }

        [Test]
        public void StatefulBinding_StructValueStateRemainsSupported()
        {
            var target = new Target();
            var state = new TargetState { Target = target };
            MotionHandle handle = MMotion.Create(0f, 8f, 1f)
                .Bind(state, static (value, callbackState) => callbackState.Target.Value = value);

            MMotion.Update(0.25d);

            Assert.That(target.Value, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(handle.TryCancel(), Is.True);
        }

        [Test]
        public void StatefulBinding_StructValueStateSurvivesGrowthSwapBackAndSlotReuse()
        {
            using var dispatcher = new ManualMotionDispatcher();
            dispatcher.EnsureStorageCapacity(2);
            var targets = new Target[40];
            var handles = new MotionHandle[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = new Target();
                handles[i] = MMotion.Create(0f, 8f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .Bind(new TargetState { Target = targets[i] }, static (value, state) => state.Target.Value = value);
            }

            Assert.That(handles[0].TryCancel(), Is.True);
            dispatcher.Update(0d);

            Assert.That(handles[0].IsActive(), Is.False);
            Assert.That(targets[1].Value, Is.Zero);
            Assert.That(targets[39].Value, Is.Zero);

            var reusedTarget = new Target();
            MotionHandle reused = MMotion.Create(0f, 8f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(new TargetState { Target = reusedTarget }, static (value, state) => state.Target.Value = value);

            Assert.That(reused.Index, Is.EqualTo(handles[0].Index));
            dispatcher.Update(0.25d);

            Assert.That(targets[1].Value, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(targets[39].Value, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(reusedTarget.Value, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void StatefulBinding_TwoAndThreeReferenceStatesSurviveGrowthAndSwapBack()
        {
            using var dispatcher = new ManualMotionDispatcher();
            var twoStateOutput = new float[1];
            var twoStateOffset = new float[] { 2f };
            var canceledOutput = new float[1];
            var canceledMultiplier = new float[] { 7f };
            var canceledOffset = new float[] { 9f };
            var threeStateOutput = new float[1];
            var threeStateMultiplier = new float[] { 2f };
            var threeStateOffset = new float[] { 3f };

            MotionHandle first = MMotion.Create(0f, 10f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(twoStateOutput, twoStateOffset, static (value, output, offset) => output[0] = value + offset[0]);
            MotionHandle canceled = MMotion.Create(0f, 10f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(canceledOutput, canceledMultiplier, canceledOffset, static (value, output, multiplier, offset) => output[0] = value * multiplier[0] + offset[0]);

            dispatcher.EnsureStorageCapacity(64);

            MotionHandle moved = MMotion.Create(0f, 10f, 1f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .Bind(threeStateOutput, threeStateMultiplier, threeStateOffset, static (value, output, multiplier, offset) => output[0] = value * multiplier[0] + offset[0]);

            Assert.That(canceled.TryCancel(), Is.True);
            dispatcher.Update(0.5d);

            Assert.That(twoStateOutput[0], Is.EqualTo(7f).Within(0.0001f));
            Assert.That(threeStateOutput[0], Is.EqualTo(13f).Within(0.0001f));
            Assert.That(canceledOutput[0], Is.Zero);
            Assert.That(first.IsActive(), Is.True);
            Assert.That(moved.IsActive(), Is.True);
        }

        [Test]
        public void BindingCanCreateMotionWhileUpdateCrossesCapacityGrowth()
        {
            const int initialCount = 32;
            var values = new float[initialCount];
            bool createDuringBinding = false;
            bool created = false;
            float spawnedValue = -1f;
            MotionHandle spawned = default;

            for (int i = 0; i < initialCount; i++)
            {
                int index = i;
                MMotion.Create(0f, 10f, 1f)
                    .WithEase(Ease.Linear)
                    .WithImmediateBind(false)
                    .Bind(value =>
                    {
                        values[index] = value;
                        if (createDuringBinding && !created)
                        {
                            created = true;
                            spawned = MMotion.Create(0f, 10f, 1f)
                                .WithEase(Ease.Linear)
                                .WithImmediateBind(false)
                                .Bind(next => spawnedValue = next);
                        }
                    });
            }

            createDuringBinding = true;
            MMotion.Update(0.5d);

            Assert.That(created, Is.True);
            Assert.That(MMotion.ActiveCount, Is.EqualTo(initialCount + 1));
            Assert.That(values, Is.All.EqualTo(5f).Within(0.0001f));
            Assert.That(spawnedValue, Is.EqualTo(-1f));

            MMotion.Update(0.5d);

            Assert.That(spawnedValue, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(spawned.IsValid(), Is.True);
        }

        [Test]
        public void EnsureStorageCapacity_UsesCompactDataAndPreservesActiveMotions()
        {
            const int requestedCapacity = 100_000;
            var dispatcher = new ManualMotionDispatcher();
            var target = new Target();
            MotionHandle handle = MMotion.Create(0f, 10f, 1f)
                .WithImmediateBind(false)
                .WithScheduler(dispatcher.Scheduler)
                .Bind(target, static (value, state) => state.Value = value);

            try
            {
                dispatcher.EnsureStorageCapacity(requestedCapacity);

                Assert.That(dispatcher.ReservedNativeBytes, Is.EqualTo(
                    requestedCapacity * 80L + 56L + MyMotion.Internal.MotionWorld.CustomEaseCurveDataStrideBytes));

                dispatcher.Update(0.5d);

                Assert.That(target.Value, Is.EqualTo(5f).Within(0.0001f));
                Assert.That(handle.IsActive(), Is.True);
            }
            finally
            {
                handle.TryCancel();
                dispatcher.Dispose();
            }
        }
    }
}

using NUnit.Framework;

namespace MyMotion.Tests
{
    public sealed class MotionBuilderCallbackTests
    {
        [SetUp]
        public void SetUp() => MMotion.Reset();

        [TearDown]
        public void TearDown() => MMotion.Reset();

        [Test]
        public void WithOnComplete_AppendsCallbacksInOrder()
        {
            int invocationOrder = 0;
            int firstCallbackOrder = 0;
            int secondCallbackOrder = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnComplete(() => firstCallbackOrder = ++invocationOrder)
                .WithOnComplete(() => secondCallbackOrder = ++invocationOrder)
                .RunWithoutBinding();

            Assert.That(handle.TryComplete(), Is.True);

            Assert.That(firstCallbackOrder, Is.EqualTo(1));
            Assert.That(secondCallbackOrder, Is.EqualTo(2));
        }

        [Test]
        public void WithOnCancel_AppendsCallbacksInOrder()
        {
            int invocationOrder = 0;
            int firstCallbackOrder = 0;
            int secondCallbackOrder = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnCancel(() => firstCallbackOrder = ++invocationOrder)
                .WithOnCancel(() => secondCallbackOrder = ++invocationOrder)
                .RunWithoutBinding();

            Assert.That(handle.TryCancel(), Is.True);

            Assert.That(firstCallbackOrder, Is.EqualTo(1));
            Assert.That(secondCallbackOrder, Is.EqualTo(2));
        }

        [Test]
        public void WithOnLoopComplete_AppendsCallbacksInOrder()
        {
            int invocationOrder = 0;
            int firstCallbackOrder = 0;
            int firstCompletedLoops = 0;
            int secondCallbackOrder = 0;
            int secondCompletedLoops = 0;
            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithLoops(3)
                .WithOnLoopComplete(loop =>
                {
                    firstCallbackOrder = ++invocationOrder;
                    firstCompletedLoops = loop;
                })
                .WithOnLoopComplete(loop =>
                {
                    secondCallbackOrder = ++invocationOrder;
                    secondCompletedLoops = loop;
                })
                .RunWithoutBinding();

            Assert.That(handle.TryComplete(), Is.True);

            Assert.That(firstCallbackOrder, Is.EqualTo(1));
            Assert.That(firstCompletedLoops, Is.EqualTo(3));
            Assert.That(secondCallbackOrder, Is.EqualTo(2));
            Assert.That(secondCompletedLoops, Is.EqualTo(3));
        }

        [Test]
        public void AdapterBuilder_WithOnComplete_AppendsCallbacksInOrder()
        {
            int invocationOrder = 0;
            int firstCallbackOrder = 0;
            int secondCallbackOrder = 0;
            MotionHandle handle = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithOnComplete(() => firstCallbackOrder = ++invocationOrder)
                .WithOnComplete(() => secondCallbackOrder = ++invocationOrder)
                .RunWithoutBinding();

            Assert.That(handle.TryComplete(), Is.True);

            Assert.That(firstCallbackOrder, Is.EqualTo(1));
            Assert.That(secondCallbackOrder, Is.EqualTo(2));
        }

        [Test]
        public void AdapterBuilder_BindsStructValueState()
        {
            var target = new AdapterTarget();
            MotionHandle handle = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                .WithImmediateBind(false)
                .Bind(new AdapterState { Target = target }, static (value, state) => state.Target.Value = value);

            MMotion.Update(0.25d);

            Assert.That(target.Value, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(handle.TryCancel(), Is.True);
        }

        [Test]
        public void WithDebugName_IsReturnedForStandardAndAdapterBuilders()
        {
            MotionHandle standardHandle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithDebugName("standard motion")
                .RunWithoutBinding();
            MotionHandle adapterHandle = MMotion.Create<float, NoMotionOptions, LinearFloatAdapter>(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithDebugName("adapter motion")
                .RunWithoutBinding();

            Assert.That(standardHandle.GetDebugName(), Is.EqualTo("standard motion"));
            Assert.That(adapterHandle.GetDebugName(), Is.EqualTo("adapter motion"));
        }

        [Test]
        public void WithDebugName_IsClearedWhenSlotIsReused()
        {
            MotionHandle previousHandle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithDebugName("previous motion")
                .RunWithoutBinding();
            previousHandle.TryCancel();
            MMotion.Update(0d);

            MotionHandle nextHandle = MMotion.Create(0f, 1f, 1f)
                .WithImmediateBind(false)
                .WithDebugName("next motion")
                .RunWithoutBinding();

            Assert.That(nextHandle.Index, Is.EqualTo(previousHandle.Index));
            Assert.That(previousHandle.GetDebugName(), Is.EqualTo(previousHandle.ToString()));
            Assert.That(nextHandle.GetDebugName(), Is.EqualTo("next motion"));
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

        private sealed class AdapterTarget
        {
            public float Value;
        }

        private struct AdapterState
        {
            public AdapterTarget Target;
        }
    }
}

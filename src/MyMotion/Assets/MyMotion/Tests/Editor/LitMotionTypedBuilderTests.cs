using NUnit.Framework;
using UnityEngine;
using MyMotion.Adapters;

namespace MyMotion.Tests
{
    // LitMotion の利用コードでよく書かれる、型付き builder を受け取る拡張メソッドの例。
    internal static class LitMotionStyleUserExtensions
    {
        public static MotionHandle BindToCounter(this MotionBuilder<int, IntegerOptions, IntMotionAdapter> builder, int[] target) =>
            builder.Bind(target, static (value, t) => t[0] = value);
    }

    /// <summary>
    /// LitMotion と同じ型名 (MotionBuilder&lt;float, NoOptions, FloatMotionAdapter&gt; など) で書いたコードが動くことを確かめる。
    /// </summary>
    public sealed class LitMotionTypedBuilderTests
    {
        [SetUp]
        public void SetUp() => MMotion.Reset();

        [TearDown]
        public void TearDown() => MMotion.Reset();

        [Test]
        public void TypedVariable_AcceptsCreateResult_AndUsesBuiltInPath()
        {
            float value = -1f;
            MotionBuilder<float, NoOptions, FloatMotionAdapter> builder = MMotion.Create(0f, 10f, 1f);
            builder.WithEase(Ease.Linear).WithImmediateBind(false).Bind(x => value = x);

            Assert.That(MMotion.World.AdapterMotionCount, Is.EqualTo(0), "built-in adapters must not use adapter storage");
            MMotion.Update(0.5);
            Assert.That(value, Is.EqualTo(5f).Within(1e-5f));
        }

        [Test]
        public void ExplicitGenericCreate_WithBuiltInAdapter_Works()
        {
            Vector3 value = default;
            MMotion.Create<Vector3, NoOptions, Vector3MotionAdapter>(Vector3.zero, new Vector3(2f, 4f, 6f), 1f)
                .WithDelay(0.5f)
                .WithOnComplete(() => value *= 10f)
                .Bind(x => value = x);

            MMotion.Update(1.0);
            Assert.That(Vector3.Distance(value, new Vector3(1f, 2f, 3f)), Is.LessThan(1e-4f));
            MMotion.Update(0.5);
            Assert.That(Vector3.Distance(value, new Vector3(20f, 40f, 60f)), Is.LessThan(1e-3f));
        }

        [Test]
        public void IntegerOptions_RoundingMode_IsCarriedBothWays()
        {
            var target = new int[1];
            MotionBuilder<int, IntegerOptions, IntMotionAdapter> typed = MMotion.Create(0, 10, 1f);
            typed = typed.WithOptions(new IntegerOptions { RoundingMode = RoundingMode.ToNegativeInfinity });
            typed.WithImmediateBind(false).BindToCounter(target);

            MMotion.Update(0.19);
            Assert.That(target[0], Is.EqualTo(1), "floor(1.9)");

            MotionBuilder<int> back = typed;
            Assert.That(back.RoundingMode, Is.EqualTo(IntegerRoundingMode.ToNegativeInfinity));
        }

        [Test]
        public void StateBindings_AndRunWithoutBinding_RouteToBuiltInPath()
        {
            var output = new float[1];
            MotionBuilder<float, NoOptions, FloatMotionAdapter> builder = MMotion.Create(0f, 4f, 1f).WithImmediateBind(false);
            builder.Bind(output, static (v, o) => o[0] = v);
            MotionHandle handle = builder.RunWithoutBinding();

            MMotion.Update(0.25);
            Assert.That(output[0], Is.EqualTo(1f).Within(1e-5f));
            Assert.That(handle.IsActive(), Is.True);
            Assert.That(MMotion.World.AdapterMotionCount, Is.EqualTo(0));
        }

        [Test]
        public void EnsureStorageCapacity_WithBuiltInAdapter_ReservesSharedStorage()
        {
            using var dispatcher = new ManualMotionDispatcher();
            long before = dispatcher.ReservedNativeBytes;
            dispatcher.EnsureStorageCapacity<float, NoOptions, FloatMotionAdapter>(4096);
            long after = dispatcher.ReservedNativeBytes;
            Assert.That(after, Is.GreaterThan(before));
            Assert.That(dispatcher.World.AdapterMotionCount, Is.EqualTo(0));

            // 共有 storage を確保済みなので、同じ容量の組み込み値型 builder では増えない。
            dispatcher.EnsureStorageCapacity(4096);
            Assert.That(dispatcher.ReservedNativeBytes, Is.EqualTo(after));
        }

        private readonly struct NotBuiltInAdapter : IMotionAdapter<float, NoOptions>
        {
            public float Evaluate(ref float startValue, ref float endValue, ref NoOptions options, in MotionEvaluationContext context) => startValue;
        }

        [Test]
        public void ConversionToCustomAdapter_Throws()
        {
            Assert.Throws<System.InvalidCastException>(() =>
            {
                MotionBuilder<float, NoOptions, NotBuiltInAdapter> unused = MMotion.Create(0f, 1f, 1f);
            });
        }
    }
}

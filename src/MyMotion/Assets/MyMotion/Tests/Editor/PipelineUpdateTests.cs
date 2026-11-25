using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    /// <summary>
    /// Update Job と Binding を重ねる経路で、利用者コードが World を操作しても結果が直列実行と一致することを確かめる。
    /// </summary>
    public sealed class PipelineUpdateTests
    {
        private const int Count = 9000;

        [SetUp]
        public void SetUp() => MMotion.Reset();

        [TearDown]
        public void TearDown() => MMotion.Reset();

        [Test]
        public void ManyFloatMotions_MatchExpectedValues()
        {
            var values = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                int index = i;
                MMotion.Create(0f, i, 2f).WithImmediateBind(false).WithEase(Ease.OutQuad).Bind(values, (v, a) => a[index] = v);
            }
            MMotion.Update(0.5);
            float t = 1f - (1f - 0.25f) * (1f - 0.25f);
            for (int i = 0; i < Count; i += 97)
            {
                Assert.That(values[i], Is.EqualTo(i * t).Within(1e-3f * (i + 1)), "index " + i);
            }
        }

        [Test]
        public void ManyQuaternionMotions_UseWidePipelineAndMatchExpectedValues()
        {
            // wide 値が過半の World は 4,096 件から計算を worker へ出す。
            const int count = 5000;
            var values = new Quaternion[count];
            Quaternion to = Quaternion.Euler(0f, 90f, 0f);
            for (int i = 0; i < count; i++)
            {
                int index = i;
                MMotion.Create(Quaternion.identity, to, 2f).WithImmediateBind(false).Bind(values, (v, a) => a[index] = v);
            }
            MMotion.Update(0.5);
            Quaternion expected = Quaternion.LerpUnclamped(Quaternion.identity, to, 0.25f);
            for (int i = 0; i < count; i += 97)
            {
                Assert.That(Quaternion.Angle(values[i], expected), Is.LessThan(0.01f), "index " + i);
            }
        }

        [Test]
        public void BindingThatCancelsLaterMotionsAndCreatesNewOnes_IsSafe()
        {
            var handles = new MotionHandle[Count];
            var values = new float[Count];
            int created = 0;
            for (int i = 0; i < Count; i++)
            {
                int index = i;
                handles[i] = MMotion.Create(0f, 1f, 1f).WithImmediateBind(false).Bind(values, (v, a) =>
                {
                    a[index] = v;
                    // 先頭の Binding から、まだ計算中かもしれない後方の motion を操作する。
                    if (index == 0 && created == 0)
                    {
                        handles[Count - 1].Cancel();
                        handles[Count / 2].Time = 0.75;
                        MMotion.Create(0f, 1f, 1f).RunWithoutBinding();
                        created++;
                    }
                });
            }

            MMotion.Update(0.25);
            Assert.That(created, Is.EqualTo(1));
            Assert.That(values[1], Is.EqualTo(0.25f).Within(1e-5f));
            Assert.That(values[Count - 2], Is.EqualTo(0.25f).Within(1e-5f));
            Assert.That(values[Count / 2], Is.EqualTo(0.75f).Within(1e-5f), "Time setter binds synchronously and the job result is not applied afterwards");
            Assert.That(handles[Count - 1].IsActive(), Is.False);
            Assert.That(values[Count - 1], Is.EqualTo(0f), "canceled before its binding ran");

            MMotion.Update(0.25);
            Assert.That(values[Count / 2], Is.EqualTo(1f).Within(1e-5f));
            Assert.That(MMotion.ActiveCount, Is.EqualTo(Count - 1), "one canceled, one completed, one created");
        }

        [Test]
        public void OnCompleteCreatingMotions_GrowsStorageSafely()
        {
            int completed = 0;
            for (int i = 0; i < Count; i++)
            {
                MMotion.Create(0f, 1f, 0.1f).WithImmediateBind(false).WithOnComplete(() =>
                {
                    completed++;
                    MMotion.Create(Vector3.zero, Vector3.one, 1f).RunWithoutBinding();
                }).Bind(static _ => { });
            }
            MMotion.Update(0.2);
            Assert.That(completed, Is.EqualTo(Count));
            Assert.That(MMotion.ActiveCount, Is.EqualTo(Count));
        }
    }
}

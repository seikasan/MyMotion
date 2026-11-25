using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    /// <summary>
    /// 終了時・assembly 再読み込み前の解放で、PlayerLoop の World と既定 dispatcher のネイティブメモリが残らないこと。
    /// </summary>
    public sealed class NativeMemoryReleaseTests
    {
        [TearDown]
        public void TearDown() => MMotion.Reset();

        [Test]
        public void ReleaseNativeMemory_ReleasesUndisposedDispatchers_AndTheyStayUsable()
        {
            // LitMotion と同じく Dispose を呼ばない使い方でも、解放時にネイティブメモリが残らない。
            var dispatcher = new ManualMotionDispatcher();
            float value = 0f;
            MMotion.Create(0f, 1f, 1f).WithScheduler(dispatcher.Scheduler).WithImmediateBind(false).Bind(x => value = x);
            dispatcher.Update(0.25);
            Assert.That(dispatcher.ReservedNativeBytes, Is.GreaterThan(0));

            MMotion.ReleaseNativeMemory();
            Assert.That(dispatcher.ReservedNativeBytes, Is.EqualTo(0));
            Assert.That(dispatcher.ActiveCount, Is.EqualTo(0));

            MMotion.Create(0f, 4f, 1f).WithScheduler(dispatcher.Scheduler).WithImmediateBind(false).Bind(x => value = x);
            dispatcher.Update(0.5);
            Assert.That(value, Is.EqualTo(2f).Within(1e-5f));
            dispatcher.Dispose();
        }

        [Test]
        public void ReleaseNativeMemory_DisposesAllWorlds_AndWorldsAreRecreatedOnNextUse()
        {
            MMotion.Create(0f, 1f, 1f).RunWithoutBinding();
            MMotion.Create(Quaternion.identity, Quaternion.Euler(0f, 90f, 0f), 1f).WithScheduler(MotionScheduler.FixedUpdate).RunWithoutBinding();
            MMotion.Create(0f, 1f, 1f).WithScheduler(MotionScheduler.Manual).RunWithoutBinding();
            Assert.That(MMotion.ReservedNativeBytes, Is.GreaterThan(0));
            Assert.That(ManualMotionDispatcher.Default.ReservedNativeBytes, Is.GreaterThan(0));

            MMotion.ReleaseNativeMemory();

            Assert.That(MMotion.ReservedNativeBytes, Is.EqualTo(0));
            Assert.That(ManualMotionDispatcher.Default.ReservedNativeBytes, Is.EqualTo(0));
            Assert.That(MMotion.ActiveCount, Is.EqualTo(0));

            float value = 0f;
            MMotion.Create(0f, 1f, 1f).WithImmediateBind(false).Bind(x => value = x);
            MMotion.Update(0.5);
            Assert.That(value, Is.EqualTo(0.5f).Within(1e-5f));
        }
    }
}

using System;
using NUnit.Framework;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void ManualDispatcher_UpdatesOnlyWhenItsDispatcherAdvances()
        {
            Assert.That(MMotion.ReservedNativeBytes, Is.Zero);
            var dispatcher = new ManualMotionDispatcher();
            try
            {
                float value = 0f;
                bool completed = false;
                var builder = MMotion.Create(0f, 10f, 1f).WithScheduler(dispatcher.Scheduler);
                Assert.That(MMotion.ReservedNativeBytes, Is.Zero);
                MotionHandle handle = builder
                    .WithOnComplete(() => completed = true)
                    .Bind(next => value = next);

                dispatcher.Update(0.5d);
                Assert.That(value, Is.EqualTo(5f).Within(0.0001f));
                Assert.That(completed, Is.False);

                MMotion.Update(0.25d);
                Assert.That(value, Is.EqualTo(5f).Within(0.0001f));

                dispatcher.Update(0.5d);
                Assert.That(value, Is.EqualTo(10f).Within(0.0001f));
                Assert.That(completed, Is.True);
                Assert.That(handle.IsActive(), Is.False);
            }
            finally
            {
                dispatcher.Dispose();
            }
        }

        [Test]
        public void Update_CallbacksCanGrowMotionStorageDuringBindingAndNotifications()
        {
            using var dispatcher = new ManualMotionDispatcher();
            const int OriginalCount = 32;
            const int BindingSpawnCount = 40;
            const int LoopSpawnCount = 40;
            const int CompletionSpawnCount = 80;
            var originalHandles = new MotionHandle[OriginalCount];
            var spawnedHandles = new MotionHandle[BindingSpawnCount + LoopSpawnCount + CompletionSpawnCount];
            int spawnedHandleCount = 0;
            int originalBindingCount = 0;
            int spawnedBindingCount = 0;
            bool bindingSpawned = false;
            bool loopSpawned = false;
            bool completionSpawned = false;
            Action<float> spawnedBinding = _ => spawnedBindingCount++;

            void Spawn(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    spawnedHandles[spawnedHandleCount++] = MMotion.Create(0f, 1f, 10f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .Bind(spawnedBinding);
                }
            }

            for (int i = 0; i < OriginalCount; i++)
            {
                int index = i;
                var builder = MMotion.Create(0f, 1f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithLoops(2, LoopType.Restart)
                    .WithImmediateBind(false);
                if (index == 1) builder = builder.WithOnLoopComplete(_ =>
                {
                    if (loopSpawned) return;
                    loopSpawned = true;
                    Spawn(LoopSpawnCount);
                });
                if (index == 2) builder = builder.WithOnComplete(() =>
                {
                    if (completionSpawned) return;
                    completionSpawned = true;
                    Spawn(CompletionSpawnCount);
                });
                originalHandles[index] = builder.Bind(_ =>
                {
                    originalBindingCount++;
                    if (index == 0 && !bindingSpawned)
                    {
                        bindingSpawned = true;
                        Spawn(BindingSpawnCount);
                    }
                });
            }

            long initialReservedBytes = dispatcher.ReservedNativeBytes;
            dispatcher.Update(1d);

            Assert.That(spawnedHandleCount, Is.EqualTo(BindingSpawnCount + LoopSpawnCount));
            Assert.That(originalBindingCount, Is.EqualTo(OriginalCount));
            Assert.That(spawnedBindingCount, Is.Zero);
            Assert.That(dispatcher.ActiveCount, Is.EqualTo(OriginalCount + BindingSpawnCount + LoopSpawnCount));
            Assert.That(dispatcher.ReservedNativeBytes, Is.GreaterThan(initialReservedBytes));

            dispatcher.Update(1d);

            Assert.That(spawnedHandleCount, Is.EqualTo(spawnedHandles.Length));
            Assert.That(originalBindingCount, Is.EqualTo(OriginalCount * 2));
            Assert.That(spawnedBindingCount, Is.EqualTo(BindingSpawnCount + LoopSpawnCount));
            Assert.That(dispatcher.ActiveCount, Is.EqualTo(spawnedHandles.Length));
            for (int i = 0; i < originalHandles.Length; i++) Assert.That(originalHandles[i].IsValid(), Is.False);

            dispatcher.Update(0.5d);
            Assert.That(spawnedBindingCount, Is.EqualTo(BindingSpawnCount * 2 + LoopSpawnCount * 2 + CompletionSpawnCount));

            for (int i = 0; i < spawnedHandles.Length; i++) Assert.That(spawnedHandles[i].TryCancel(), Is.True);
            dispatcher.Update(0d);
            Assert.That(dispatcher.ActiveCount, Is.Zero);
        }

        [Test]
        public void SchedulerMetrics_IncludeAllPlayerLoopWorldsWithoutForcingAllocation()
        {
            Assert.That(MMotion.ActiveCount, Is.Zero);
            Assert.That(MMotion.ReservedNativeBytes, Is.Zero);

            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithScheduler(MotionScheduler.PreUpdate)
                .Bind(_ => { });

            Assert.That(MMotion.ActiveCount, Is.EqualTo(1));
            Assert.That(MMotion.ReservedNativeBytes, Is.GreaterThan(0));

            handle.TryCancel();
            Assert.That(MMotion.ActiveCount, Is.EqualTo(1));

            MMotion.Reset();
            Assert.That(MMotion.ActiveCount, Is.Zero);
            Assert.That(MMotion.ReservedNativeBytes, Is.Zero);
        }

        [Test]
        public void ManualDispatcher_ResetInvalidatesHandlesAndRetainsCapacity()
        {
            var dispatcher = new ManualMotionDispatcher();
            try
            {
                dispatcher.EnsureStorageCapacity(128);
                long reservedBytes = dispatcher.ReservedNativeBytes;
                float value = 0f;
                MotionHandle handle = MMotion.Create(0f, 10f, 1f)
                    .WithLoops(-1)
                    .WithScheduler(dispatcher.Scheduler)
                    .Bind(next => value = next);

                dispatcher.Update(0.5d);
                Assert.That(value, Is.EqualTo(5f).Within(0.0001f));
                Assert.That(handle.IsValid(), Is.True);

                dispatcher.Time = 0.75d;
                Assert.That(value, Is.EqualTo(7.5f).Within(0.0001f));
                dispatcher.Time = 0.25d;
                Assert.That(value, Is.EqualTo(2.5f).Within(0.0001f));

                dispatcher.Reset();

                Assert.That(dispatcher.Time, Is.Zero);
                Assert.That(dispatcher.ActiveCount, Is.Zero);
                Assert.That(handle.IsValid(), Is.False);
                Assert.That(dispatcher.ReservedNativeBytes, Is.EqualTo(reservedBytes));
            }
            finally
            {
                dispatcher.Dispose();
            }
        }
    }
}

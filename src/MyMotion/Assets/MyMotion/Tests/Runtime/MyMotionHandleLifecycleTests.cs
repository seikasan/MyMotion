using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyMotion.Tests
{
    public sealed class MyMotionHandleLifecycleTests
    {
        [UnityTest]
        public IEnumerator AddTo_TracksDestroyDisableAndComponentLifetime()
        {
            using (var dispatcher = new ManualMotionDispatcher())
            {
                var destroyTarget = new GameObject("MyMotion destroy link test");
                var disableTarget = new GameObject("MyMotion disable link test");
                var completeTarget = new GameObject("MyMotion complete link test");
                var componentTarget = new GameObject("MyMotion component destroy link test");

                int destroyCancelCount = 0;
                MotionHandle destroyLinked = MMotion.Create(0f, 1f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnCancel(() => destroyCancelCount++)
                    .RunWithoutBinding()
                    .AddTo(destroyTarget);

                destroyTarget.SetActive(false);
                Assert.That(destroyLinked.IsActive(), Is.True);
                destroyTarget.SetActive(true);

                int disableCancelCount = 0;
                var boxCollider = disableTarget.AddComponent<BoxCollider>();
                MotionHandle disableLinked = MMotion.Create(0f, 1f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnCancel(() => disableCancelCount++)
                    .RunWithoutBinding()
                    .AddTo(boxCollider, LinkBehavior.CancelOnDisable);
                disableTarget.SetActive(false);

                Assert.That(disableLinked.IsActive(), Is.False);
                Assert.That(disableCancelCount, Is.EqualTo(1));

                float value = -1f;
                int completeCount = 0;
                MotionHandle completeLinked = MMotion.Create(0f, 1f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnComplete(() => completeCount++)
                    .Bind(next => value = next)
                    .AddTo(completeTarget, LinkBehavior.CompleteOnDisable);
                completeTarget.SetActive(false);

                Assert.That(completeLinked.IsActive(), Is.False);
                Assert.That(value, Is.EqualTo(1f));
                Assert.That(completeCount, Is.EqualTo(1));

                int componentCancelCount = 0;
                Animator animator = componentTarget.AddComponent<Animator>();
                MotionHandle componentLinked = MMotion.Create(0f, 1f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnCancel(() => componentCancelCount++)
                    .RunWithoutBinding()
                    .AddTo(animator);
                Object.Destroy(componentTarget);
                yield return null;

                Assert.That(componentLinked.IsActive(), Is.False);
                Assert.That(componentCancelCount, Is.EqualTo(1));

                Object.Destroy(destroyTarget);
                yield return null;
                Assert.That(destroyLinked.IsActive(), Is.False);
                Assert.That(destroyCancelCount, Is.EqualTo(1));

                Object.Destroy(disableTarget);
                Object.Destroy(completeTarget);
            }
        }

        [UnityTest]
        public IEnumerator AddTo_CompactsInactiveHandlesWhenFullAndKeepsActiveHandles()
        {
            using (var dispatcher = new ManualMotionDispatcher())
            {
                var target = new GameObject("MyMotion linker compaction test");
                int activeCancelCount = 0;
                MotionHandle active = MMotion.Create(0f, 1f, 1f)
                    .WithScheduler(dispatcher.Scheduler)
                    .WithImmediateBind(false)
                    .WithOnCancel(() => activeCancelCount++)
                    .RunWithoutBinding()
                    .AddTo(target);

                for (int i = 0; i < 32; i++)
                {
                    MotionHandle completed = MMotion.Create(0f, 1f, 1f)
                        .WithScheduler(dispatcher.Scheduler)
                        .WithImmediateBind(false)
                        .RunWithoutBinding()
                        .AddTo(target);
                    Assert.That(completed.TryCancel(), Is.True);
                    dispatcher.Update(0d);
                }

                System.Type linkerType = typeof(MotionHandle).Assembly.GetType("MyMotion.Internal.MotionHandleLinker");
                Assert.That(linkerType, Is.Not.Null);
                Component linker = target.GetComponent(linkerType);
                FieldInfo entriesField = linkerType.GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(entriesField, Is.Not.Null);
                IList entries = (IList)entriesField.GetValue(linker);
                Assert.That(entries.Count, Is.LessThanOrEqualTo(4));

                Object.Destroy(target);
                yield return null;

                Assert.That(active.IsActive(), Is.False);
                Assert.That(activeCancelCount, Is.EqualTo(1));
            }
        }
    }
}

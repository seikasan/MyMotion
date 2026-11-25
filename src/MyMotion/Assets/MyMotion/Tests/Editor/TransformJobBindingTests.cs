using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MyMotion.Extensions;
using MyMotion.Internal;

namespace MyMotion.Tests
{
    /// <summary>
    /// Transform への Binding を Burst Job で書き込む経路が、managed Binding と同じ結果になることを確かめる。
    /// </summary>
    public sealed class TransformJobBindingTests
    {
        private readonly List<GameObject> _objects = new();
        private ManualMotionDispatcher _dispatcher;

        [SetUp]
        public void SetUp()
        {
            MMotion.Reset();
            _dispatcher = new ManualMotionDispatcher();
            // Editor テストでは少数で Job モードへ入れ、寿命判定も外して Job 経路そのものを検証する。
            MotionWorld world = _dispatcher.World;
            world.TransformJobEnterCount = 16;
            world.TransformJobExitCount = 8;
            world.TransformMinRemainingFrames = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _dispatcher.Dispose();
            foreach (GameObject go in _objects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _objects.Clear();
            MMotion.Reset();
        }

        private Transform[] CreateTransforms(int count, Transform parent = null)
        {
            var result = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("t" + i);
                if (parent != null) go.transform.SetParent(parent, false);
                _objects.Add(go);
                result[i] = go.transform;
            }
            return result;
        }

        private int JobThreshold => _dispatcher.World.TransformJobEnterCount;

        [Test]
        public void ManyPositionMotions_UseJobAndMatchExpectedValues()
        {
            int count = JobThreshold * 2;
            Transform[] targets = CreateTransforms(count);
            for (int i = 0; i < count; i++)
            {
                MMotion.Create(Vector3.zero, new Vector3(i, 2f * i, -i), 1f)
                    .WithScheduler(_dispatcher.Scheduler)
                    .BindToPosition(targets[i]);
            }

            Assert.That(_dispatcher.World.TransformEligibleCount, Is.EqualTo(count));
            _dispatcher.Update(0.5);
            Assert.That(_dispatcher.World.TransformTargetCount, Is.EqualTo(count));
            for (int i = 0; i < count; i++)
            {
                Assert.That(Vector3.Distance(targets[i].position, new Vector3(i, 2f * i, -i) * 0.5f), Is.LessThan(1e-4f), "index " + i);
            }

            _dispatcher.Update(0.5);
            for (int i = 0; i < count; i++)
            {
                Assert.That(Vector3.Distance(targets[i].position, new Vector3(i, 2f * i, -i)), Is.LessThan(1e-4f));
            }
            _dispatcher.Update(0.1);
            Assert.That(_dispatcher.World.TransformTargetCount, Is.EqualTo(0), "completed lanes are released");
        }

        [Test]
        public void AllKinds_MatchManagedBinding()
        {
            int count = JobThreshold;
            var parent = new GameObject("parent").transform;
            _objects.Add(parent.gameObject);
            parent.position = new Vector3(1f, 2f, 3f);
            parent.rotation = Quaternion.Euler(10f, 20f, 30f);
            parent.localScale = new Vector3(2f, 2f, 2f);

            Transform[] job = CreateTransforms(count * 6, parent);
            Transform[] managed = CreateTransforms(count * 6, parent);
            var to = new Vector3(30f, 60f, 90f);
            var rot = Quaternion.Euler(40f, 50f, 60f);

            void Schedule(Transform[] t, bool viaJob)
            {
                for (int i = 0; i < count; i++)
                {
                    int o = i * 6;
                    if (viaJob)
                    {
                        Build(Vector3.zero, to).BindToPosition(t[o]);
                        Build(Vector3.zero, to).BindToLocalEulerAngles(t[o + 1]);
                        Build(Quaternion.identity, rot).BindToRotation(t[o + 2]);
                        Build(0f, 5f).BindToLocalPositionY(t[o + 3]);
                        Build(1f, 3f).BindToLocalScaleXYZ(t[o + 4]);
                        Build(Vector3.zero, to).BindToEulerAngles(t[o + 5]);
                    }
                    else
                    {
                        // 同じ設定を managed delegate で書く参照実装。
                        Build(Vector3.zero, to).Bind(t[o], static (v, x) => x.position = v);
                        Build(Vector3.zero, to).Bind(t[o + 1], static (v, x) => x.localEulerAngles = v);
                        Build(Quaternion.identity, rot).Bind(t[o + 2], static (v, x) => x.rotation = v);
                        Build(0f, 5f).Bind(t[o + 3], static (v, x) => { var p = x.localPosition; p.y = v; x.localPosition = p; });
                        Build(1f, 3f).Bind(t[o + 4], static (v, x) => x.localScale = new Vector3(v, v, v));
                        Build(Vector3.zero, to).Bind(t[o + 5], static (v, x) => x.eulerAngles = v);
                    }
                }
            }

            Schedule(job, true);
            Schedule(managed, false);
            _dispatcher.Update(0.37);

            for (int i = 0; i < job.Length; i++)
            {
                Assert.That(Vector3.Distance(job[i].position, managed[i].position), Is.LessThan(1e-3f), "position " + i);
                Assert.That(Quaternion.Angle(job[i].rotation, managed[i].rotation), Is.LessThan(0.01f), "rotation " + i);
                Assert.That(Vector3.Distance(job[i].localScale, managed[i].localScale), Is.LessThan(1e-4f), "scale " + i);
            }
        }

        private MotionBuilder<Vector3> Build(Vector3 a, Vector3 b) => MMotion.Create(a, b, 1f).WithEase(Ease.OutQuad).WithScheduler(_dispatcher.Scheduler);
        private MotionBuilder<Quaternion> Build(Quaternion a, Quaternion b) => MMotion.Create(a, b, 1f).WithEase(Ease.OutQuad).WithScheduler(_dispatcher.Scheduler);
        private MotionBuilder<float> Build(float a, float b) => MMotion.Create(a, b, 1f).WithEase(Ease.OutQuad).WithScheduler(_dispatcher.Scheduler);

        [Test]
        public void CancelAndSwapRemoval_KeepLanesConsistent()
        {
            int count = JobThreshold * 2;
            Transform[] targets = CreateTransforms(count);
            var handles = new MotionHandle[count];
            for (int i = 0; i < count; i++)
            {
                handles[i] = MMotion.Create(Vector3.zero, Vector3.one * (i + 1), 1f)
                    .WithScheduler(_dispatcher.Scheduler)
                    .BindToLocalPosition(targets[i]);
            }

            // 偶数番を cancel し、swap-back で lane と dense index が入れ替わる状況を作る。
            for (int i = 0; i < count; i += 2) handles[i].Cancel();
            _dispatcher.Update(0.25);
            Assert.That(_dispatcher.World.TransformTargetCount, Is.EqualTo(count / 2));

            Vector3[] canceled = new Vector3[count];
            for (int i = 0; i < count; i += 2) canceled[i] = targets[i].localPosition;
            _dispatcher.Update(0.25);
            for (int i = 0; i < count; i++)
            {
                if ((i & 1) == 0)
                {
                    Assert.That(targets[i].localPosition, Is.EqualTo(canceled[i]), "canceled motion must not move " + i);
                }
                else
                {
                    Assert.That(Vector3.Distance(targets[i].localPosition, Vector3.one * (i + 1) * 0.5f), Is.LessThan(1e-4f), "index " + i);
                }
            }
        }

        [Test]
        public void DestroyedTransform_ReportsExceptionLikeManagedBinding()
        {
            int count = JobThreshold;
            Transform[] targets = CreateTransforms(count);
            bool canceled = false;
            for (int i = 0; i < count; i++)
            {
                var builder = MMotion.Create(Vector3.zero, Vector3.one, 1f).WithScheduler(_dispatcher.Scheduler);
                if (i == 3) builder = builder.WithCancelOnError().WithOnCancel(() => canceled = true);
                builder.BindToPosition(targets[i]);
            }

            Object.DestroyImmediate(targets[3].gameObject);
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("MissingReferenceException"));
            _dispatcher.Update(0.5);

            Assert.That(canceled, Is.True);
            Assert.That(Vector3.Distance(targets[4].position, Vector3.one * 0.5f), Is.LessThan(1e-4f));
        }

        [Test]
        public void ShortLivedMotions_AreNotMigrated()
        {
            _dispatcher.World.TransformMinRemainingFrames = 32;
            int count = JobThreshold * 2;
            Transform[] targets = CreateTransforms(count);
            for (int i = 0; i < count; i++)
            {
                // 残り 1 秒 / 0.1 秒 = 10 フレームで終わるため、登録費用を払わず managed のまま書く。
                MMotion.Create(0f, 1f, 1f).WithScheduler(_dispatcher.Scheduler).BindToLocalPositionX(targets[i]);
            }
            _dispatcher.Update(0.1);
            Assert.That(_dispatcher.World.TransformTargetCount, Is.EqualTo(0));
            Assert.That(targets[count - 1].localPosition.x, Is.EqualTo(0.1f).Within(1e-5f));
        }

        [Test]
        public void LeavingAndReenteringJobMode_KeepsValuesCorrect()
        {
            int count = JobThreshold * 2;
            Transform[] targets = CreateTransforms(count);
            var handles = new MotionHandle[count];
            for (int i = 0; i < count; i++)
            {
                handles[i] = MMotion.Create(0f, 10f, 10f).WithScheduler(_dispatcher.Scheduler).BindToLocalScaleY(targets[i]);
            }
            _dispatcher.Update(1.0);
            Assert.That(_dispatcher.World.TransformTargetCount, Is.EqualTo(count));

            // exit 未満まで減らすと managed に戻る。
            for (int i = 4; i < count; i++) handles[i].Cancel();
            _dispatcher.Update(1.0);
            _dispatcher.Update(1.0);
            Assert.That(_dispatcher.World.TransformTargetCount, Is.EqualTo(0));
            for (int i = 0; i < 4; i++) Assert.That(targets[i].localScale.y, Is.EqualTo(3f).Within(1e-4f));

            // 再び増えると Job へ戻る。残っていた 4 件も再登録される。
            var more = CreateTransforms(count);
            for (int i = 0; i < count; i++)
            {
                MMotion.Create(0f, 10f, 10f).WithScheduler(_dispatcher.Scheduler).BindToLocalScaleY(more[i]);
            }
            _dispatcher.Update(1.0);
            Assert.That(_dispatcher.World.TransformTargetCount, Is.EqualTo(count + 4));
            for (int i = 0; i < 4; i++) Assert.That(targets[i].localScale.y, Is.EqualTo(4f).Within(1e-4f));
            Assert.That(more[count - 1].localScale.y, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void FewMotions_StayOnManagedPath()
        {
            Transform[] targets = CreateTransforms(3);
            for (int i = 0; i < 3; i++)
            {
                MMotion.Create(0f, 4f, 1f).WithScheduler(_dispatcher.Scheduler).BindToPositionX(targets[i]);
            }
            _dispatcher.Update(0.5);
            for (int i = 0; i < 3; i++) Assert.That(targets[i].position.x, Is.EqualTo(2f).Within(1e-5f));
        }

        [Test]
        public void CompleteAndSetTime_UseManagedBindingSynchronously()
        {
            int count = JobThreshold;
            Transform[] targets = CreateTransforms(count);
            var handles = new MotionHandle[count];
            for (int i = 0; i < count; i++)
            {
                handles[i] = MMotion.Create(Vector3.zero, Vector3.one * 4f, 1f)
                    .WithScheduler(_dispatcher.Scheduler)
                    .BindToLocalScale(targets[i]);
            }
            handles[0].Time = 0.25;
            Assert.That(Vector3.Distance(targets[0].localScale, Vector3.one), Is.LessThan(1e-4f));
            handles[1].Complete();
            Assert.That(Vector3.Distance(targets[1].localScale, Vector3.one * 4f), Is.LessThan(1e-4f));
            _dispatcher.Update(0.5);
            Assert.That(Vector3.Distance(targets[2].localScale, Vector3.one * 2f), Is.LessThan(1e-4f));
        }
    }
}

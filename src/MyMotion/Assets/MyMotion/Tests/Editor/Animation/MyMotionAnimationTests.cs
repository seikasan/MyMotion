using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using MyMotion.Animation;
using MyMotion.Animation.Components;

namespace MyMotion.Tests
{
    /// <summary>
    /// LitMotion.Animation 相当のコンポーネント再生を、既定 World を手動で進めて確かめる。
    /// </summary>
    public sealed class MyMotionAnimationTests
    {
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            MMotion.Reset();
            _host = new GameObject("animation-host");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            MMotion.Reset();
        }

        private static void SetField(object target, string name, object value)
        {
            System.Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
                type = type.BaseType;
            }
            Assert.Fail("field not found: " + name);
        }

        private TransformPositionAnimation Position(Transform target, Vector3 to, float duration)
        {
            var component = new TransformPositionAnimation();
            SetField(component, "_target", target);
            SetField(component, "_settings", new SerializableMotionSettings<Vector3, NoOptions> { StartValue = Vector3.zero, EndValue = to, Duration = duration });
            return component;
        }

        private MyMotionAnimation CreateAnimation(bool sequential, params MyMotionAnimationComponent[] components)
        {
            var animation = _host.AddComponent<MyMotionAnimation>();
            SetField(animation, "_components", components);
            if (sequential)
            {
                FieldInfo mode = typeof(MyMotionAnimation).GetField("_animationMode", BindingFlags.Instance | BindingFlags.NonPublic);
                mode.SetValue(animation, System.Enum.ToObject(mode.FieldType, 1));
            }
            return animation;
        }

        [Test]
        public void Parallel_PlaysAllComponents_AndStopRestoresStartValues()
        {
            var a = new GameObject("a").transform;
            var b = new GameObject("b").transform;
            a.SetParent(_host.transform);
            b.SetParent(_host.transform);
            b.localPosition = new Vector3(5f, 0f, 0f);
            MyMotionAnimation animation = CreateAnimation(false, Position(a, Vector3.one, 1f), Position(b, Vector3.one * 2f, 1f));

            animation.Play();
            MMotion.Update(0.5);
            Assert.That(Vector3.Distance(a.localPosition, Vector3.one * 0.5f), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(b.localPosition, Vector3.one), Is.LessThan(1e-4f));
            Assert.That(animation.IsPlaying, Is.True);

            animation.Stop();
            Assert.That(a.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(b.localPosition, Is.EqualTo(new Vector3(5f, 0f, 0f)), "OnStop restores the value captured at Play");
            Assert.That(animation.IsActive, Is.False);
        }

        [Test]
        public void Sequential_PlaysNextAfterCompletion_PauseAndResume()
        {
            var a = new GameObject("a").transform;
            var b = new GameObject("b").transform;
            a.SetParent(_host.transform);
            b.SetParent(_host.transform);
            MyMotionAnimation animation = CreateAnimation(true, Position(a, Vector3.one, 0.5f), Position(b, Vector3.one, 0.5f));

            animation.Play();
            MMotion.Update(0.25);
            Assert.That(b.localPosition, Is.EqualTo(Vector3.zero), "second component waits");
            MMotion.Update(0.3);
            Assert.That(Vector3.Distance(a.localPosition, Vector3.one), Is.LessThan(1e-4f));

            MMotion.Update(0.25);
            Assert.That(Vector3.Distance(b.localPosition, Vector3.one * 0.5f), Is.LessThan(1e-4f));

            animation.Pause();
            MMotion.Update(1.0);
            Assert.That(Vector3.Distance(b.localPosition, Vector3.one * 0.5f), Is.LessThan(1e-4f), "paused");
            animation.Play();
            MMotion.Update(0.25);
            Assert.That(Vector3.Distance(b.localPosition, Vector3.one), Is.LessThan(1e-4f));
        }

        [Test]
        public void Sequential_StopDoesNotStartQueuedComponents()
        {
            var a = new GameObject("a").transform;
            var b = new GameObject("b").transform;
            a.SetParent(_host.transform);
            b.SetParent(_host.transform);
            MyMotionAnimation animation = CreateAnimation(true, Position(a, Vector3.one, 1f), Position(b, Vector3.one, 1f));

            animation.Play();
            MMotion.Update(0.5);
            animation.Stop();
            MMotion.Update(1.0);
            Assert.That(b.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(MMotion.ActiveCount, Is.EqualTo(0));
        }
    }
}

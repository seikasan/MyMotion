using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using MyMotion.Editor;
using Unity.Collections;
using UnityEngine.UIElements;

namespace MyMotion.Tests
{
    internal sealed class MotionSettingsHolder : ScriptableObject
    {
        public SerializableMotionSettings<float, NoOptions> floatSettings = new();
        public SerializableMotionSettings<FixedString32Bytes, StringOptions> stringSettings = new();
    }

    /// <summary>
    /// EditorMotionScheduler と MotionDebugger の動作確認。
    /// </summary>
    public sealed class EditorToolingTests
    {
        [SetUp]
        public void SetUp()
        {
            MMotion.Reset();
            MotionDebugger.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            MotionDebugger.Enabled = false;
            MotionDebugger.EnableStackTrace = false;
            MotionDebugger.Clear();
            MMotion.Reset();
        }

        [UnityTest]
        public IEnumerator EditorUpdateScheduler_AdvancesWithEditorTime()
        {
            float value = 0f;
            bool completed = false;
            MMotion.Create(0f, 1f, 0.2f)
                .WithScheduler(EditorMotionScheduler.Update)
                .WithOnComplete(() => completed = true)
                .Bind(x => value = x);

            double start = EditorApplication.timeSinceStartup;
            while (!completed && EditorApplication.timeSinceStartup - start < 5d)
            {
                yield return null;
            }
            Assert.That(completed, Is.True);
            Assert.That(value, Is.EqualTo(1f));
        }

        [Test]
        public void SettingsDrawer_BuildsFieldsForPrimitiveAndFixedString()
        {
            var holder = ScriptableObject.CreateInstance<MotionSettingsHolder>();
            try
            {
                var serialized = new SerializedObject(holder);
                foreach (string name in new[] { nameof(MotionSettingsHolder.floatSettings), nameof(MotionSettingsHolder.stringSettings) })
                {
                    var drawer = new SerializableMotionSettingsDrawer();
                    typeof(PropertyDrawer)
                        .GetField("m_FieldInfo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        .SetValue(drawer, typeof(MotionSettingsHolder).GetField(name));
                    VisualElement root = drawer.CreatePropertyGUI(serialized.FindProperty(name));
                    Assert.That(root, Is.Not.Null);
                    Assert.That(root.childCount, Is.GreaterThanOrEqualTo(5), name);
                }

                var stringDrawer = new StringOptionsDrawer();
                VisualElement options = stringDrawer.CreatePropertyGUI(serialized.FindProperty("stringSettings").FindPropertyRelative("_options"));
                Assert.That(options.childCount, Is.EqualTo(4));
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void MotionDebugger_TracksOnlyWhenEnabled_AndDropsEndedMotions()
        {
            MMotion.Create(0f, 1f, 1f).RunWithoutBinding();
            Assert.That(MotionDebugger.Items.Count, Is.EqualTo(0));

            MotionDebugger.Enabled = true;
            MotionDebugger.EnableStackTrace = true;
            MotionHandle handle = MMotion.Create(Vector3.zero, Vector3.one, 1f).WithDebugName("debug-target").Bind(_ => { });
            Assert.That(MotionDebugger.Items.Count, Is.EqualTo(1));
            MotionDebugger.TrackingState state = MotionDebugger.Items[0];
            Assert.That(state.DebugName, Is.EqualTo("debug-target"));
            Assert.That(state.ValueType, Is.EqualTo(typeof(Vector3)));
            Assert.That(state.StackTrace, Is.Not.Null);
            StringAssert.Contains(nameof(MotionDebugger_TracksOnlyWhenEnabled_AndDropsEndedMotions), state.StackTrace.ToString());

            handle.Cancel();
            MotionDebugger.RemoveInactive();
            Assert.That(MotionDebugger.Items.Count, Is.EqualTo(0));
        }
    }
}

using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MyMotion.Editor
{
    [CustomPropertyDrawer(typeof(SerializableMotionSettings<,>))]
    public class SerializableMotionSettingsDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldout = new Foldout()
            {
                text = property.displayName,
            };

            foldout.BindProperty(property);

            Group(foldout, group =>
            {
                var valueType = fieldInfo.FieldType.GenericTypeArguments[0];
                if (valueType == typeof(FixedString32Bytes))
                {
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_startValue"), FixedString32Bytes.UTF8MaxLengthInBytes));
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_endValue"), FixedString32Bytes.UTF8MaxLengthInBytes));
                }
                else if (valueType == typeof(FixedString64Bytes))
                {
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_startValue"), FixedString64Bytes.UTF8MaxLengthInBytes));
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_endValue"), FixedString64Bytes.UTF8MaxLengthInBytes));
                }
                else if (valueType == typeof(FixedString128Bytes))
                {
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_startValue"), FixedString128Bytes.UTF8MaxLengthInBytes));
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_endValue"), FixedString128Bytes.UTF8MaxLengthInBytes));
                }
                else if (valueType == typeof(FixedString512Bytes))
                {
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_startValue"), FixedString512Bytes.UTF8MaxLengthInBytes));
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_endValue"), FixedString512Bytes.UTF8MaxLengthInBytes));
                }
                else if (valueType == typeof(FixedString4096Bytes))
                {
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_startValue"), FixedString4096Bytes.UTF8MaxLengthInBytes));
                    group.Add(PropertyFieldHelper.CreateFixedStringField(property.FindPropertyRelative("_endValue"), FixedString4096Bytes.UTF8MaxLengthInBytes));
                }
                else
                {
                    AddPropertyField(group, property, "_startValue");
                    AddPropertyField(group, property, "_endValue");
                }

                AddPropertyField(group, property, "_duration");
            });

            Group(foldout, group =>
            {
                var ease = property.FindPropertyRelative("_ease");
                group.Add(new PropertyField(ease));

                var customEaseCurve = new PropertyField(property.FindPropertyRelative("_customEaseCurve"))
                {
                    style = { display = ease.enumValueIndex == (int)Ease.CustomAnimationCurve ? DisplayStyle.Flex : DisplayStyle.None }
                };

                group.Add(customEaseCurve);
                customEaseCurve.schedule
                    .Execute(() =>
                    {
                        try
                        {
                            customEaseCurve.style.display = ease.enumValueIndex == (int)Ease.CustomAnimationCurve ? DisplayStyle.Flex : DisplayStyle.None;
                        }
                        catch (InvalidOperationException)
                        {
                            // 何もしない
                        }
                    })
                    .Every(10);
            });

            Group(foldout, group =>
            {
                AddPropertyField(group, property, "_delay");
                AddPropertyField(group, property, "_delayType");
            });

            Group(foldout, group =>
            {
                AddPropertyField(group, property, "_loops");
                AddPropertyField(group, property, "_loopType");
            });

            var options = property.FindPropertyRelative("_options");
            if (options.hasChildren)
            {
                Group(foldout, group =>
                {
                    group.style.paddingLeft = 15f;
                    group.Add(new PropertyField(options));
                });
            }

            Group(foldout, group =>
            {
                FoldoutGroup(group, "Additional Settings", property.FindPropertyRelative("additionalSettings"), group =>
                {
                    group.style.marginLeft = 15f;
                    AddPropertyField(group, property, "_cancelOnError");
                    AddPropertyField(group, property, "_skipValuesDuringDelay");
                    AddPropertyField(group, property, "_immediateBind");
                    AddPropertyField(group, property, "_playbackSpeed");
                    AddPropertyField(group, property, "_timeKind");
                    var scheduler = property.FindPropertyRelative("_schedulerType");
                    if (scheduler != null) group.Add(new PropertyField(scheduler, "Scheduler"));
                });
            });

            return foldout;
        }

        private void Group(VisualElement root, Action<VisualElement> configure)
        {
            var group = new Box { style = { marginBottom = 3f } };
            configure(group);
            root.Add(group);
        }

        private void FoldoutGroup(VisualElement root, string label, SerializedProperty property, Action<VisualElement> configure)
        {
            var group = new Foldout
            {
                text = label,
                style = { marginBottom = 3f },
            };
            // MyMotion は開閉状態を保存しないため、保存先が無ければ閉じた状態で表示する。
            if (property != null) group.BindProperty(property);
            else group.value = false;
            configure(group);
            root.Add(group);
        }

        private void AddPropertyField(VisualElement root, SerializedProperty property, string name)
        {
            SerializedProperty child = property.FindPropertyRelative(name);
            if (child != null) root.Add(new PropertyField(child));
        }

        private IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
        {
            SerializedProperty currentProperty = property.Copy();
            SerializedProperty nextProperty = property.Copy();
            nextProperty.Next(false);

            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextProperty)) break;
                    yield return currentProperty;
                }
                while (currentProperty.Next(false));
            }
        }
    }
}

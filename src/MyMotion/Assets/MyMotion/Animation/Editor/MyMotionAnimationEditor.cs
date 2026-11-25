using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace MyMotion.Animation.Editor
{
    [CustomEditor(typeof(MyMotionAnimation))]
    public sealed class MyMotionAnimationEditor : UnityEditor.Editor
    {
        private SerializedProperty _componentsProperty;
        private int _prevArraySize;

        private AddAnimationComponentDropdown _dropdown;
        private VisualElement _componentRoot;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            _componentRoot = new VisualElement();

            _componentsProperty = serializedObject.FindProperty("_components");
            _prevArraySize = _componentsProperty.arraySize;

            _dropdown = new AddAnimationComponentDropdown(new());
            _dropdown.OnTypeSelected += type =>
            {
                int last = _componentsProperty.arraySize;
                _componentsProperty.InsertArrayElementAtIndex(_componentsProperty.arraySize);
                SerializedProperty property = _componentsProperty.GetArrayElementAtIndex(last);
                property.managedReferenceValue = ReflectionHelper.CreateDefaultInstance(type);
                serializedObject.ApplyModifiedProperties();
            };

            root.Add(CreateSettingsPanel());
            _componentRoot.Add(CreateComponentsPanel());
            root.Add(_componentRoot);
            root.Add(CreateDebugPanel());

            return root;
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && target != null)
            {
                ((MyMotionAnimation)target).Stop();
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                ((MyMotionAnimation)target).Stop();
            }
        }

        private VisualElement CreateBox(string label)
        {
            var box = new Box
            {
                style = {
                    marginTop = 6f,
                    marginBottom = 2f,
                    paddingLeft = 4f,
                    alignItems = Align.Stretch,
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1f,
                }
            };

            box.Add(new Label(label)
            {
                style = {
                    marginTop = 5f,
                    marginBottom = 3f,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            });

            return box;
        }

        private VisualElement CreateSettingsPanel()
        {
            VisualElement box = CreateBox("Settings");
            box.Add(new PropertyField(serializedObject.FindProperty("_autoPlayMode")));
            box.Add(new PropertyField(serializedObject.FindProperty("_animationMode")));
            return box;
        }

        private VisualElement CreateComponentsPanel()
        {
            VisualElement box = CreateBox("Components");
            var views = new List<AnimationComponentView>();

            for (int i = 0; i < _componentsProperty.arraySize; i++)
            {
                SerializedProperty property = _componentsProperty.GetArrayElementAtIndex(i);
                AnimationComponentView view = CreateComponentGUI(property.Copy());
                CreateContextMenuManipulator(_componentsProperty, i, false).target = view.Foldout.Q<Toggle>();

                SerializedProperty enabledProperty = property.FindPropertyRelative("_enabled");
                if (enabledProperty != null)
                {
                    view.EnabledToggle.BindProperty(enabledProperty);
                }

                box.Add(view);
                views.Add(view);
                CreateContextMenuManipulator(_componentsProperty, i, true).target = view.ContextMenuButton;
            }

            var addButton = new Button()
            {
                text = "Add...",
                style = {
                    width = 200f,
                    alignSelf = Align.Center
                }
            };
            addButton.clicked += () => _dropdown.Show(addButton.worldBound);
            box.Add(addButton);

            box.schedule.Execute(() =>
            {
                bool enabled = IsActive();
                foreach (AnimationComponentView view in views)
                {
                    view.SetEnabled(enabled);
                }
                addButton.SetEnabled(enabled);
            }).Every(10);

            box.schedule.Execute(() =>
            {
                if (_componentsProperty.arraySize != _prevArraySize)
                {
                    if (_prevArraySize < _componentsProperty.arraySize)
                    {
                        var seen = new HashSet<object>();
                        bool dirty = false;
                        for (int i = 0; i < _componentsProperty.arraySize; ++i)
                        {
                            SerializedProperty element = _componentsProperty.GetArrayElementAtIndex(i);
                            object value = element.managedReferenceValue;
                            if (value != null && !seen.Add(value))
                            {
                                object cloned = JsonUtility.FromJson(JsonUtility.ToJson(value), value.GetType());
                                element.managedReferenceValue = cloned;
                                dirty = true;
                            }
                        }
                        if (dirty)
                            serializedObject.ApplyModifiedProperties();
                    }
                    RefleshComponentsView(true);
                    _prevArraySize = _componentsProperty.arraySize;
                }

                IReadOnlyList<MyMotionAnimationComponent> components = ((MyMotionAnimation)target).Components;
                for (int i = 0; i < views.Count; i++)
                {
                    if (components.Count <= i)
                    {
                        views[i].Progress = 0f;
                        continue;
                    }

                    MyMotionAnimationComponent component = components[i];
                    if (component == null)
                    {
                        views[i].Progress = 0f;
                        continue;
                    }

                    MotionHandle handle = component.TrackedHandle;

                    if (handle.IsActive() && !double.IsInfinity(handle.TotalDuration))
                    {
                        views[i].Progress = Mathf.InverseLerp(0f, (float)handle.TotalDuration, (float)handle.Time);
                    }
                    else
                    {
                        views[i].Progress = 0f;
                    }
                }
            })
            .Every(10);

            return box;
        }

        private VisualElement CreateDebugPanel()
        {
            VisualElement box = CreateBox("Debug");
            var buttonGroup = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1f,
                }
            };
            var playButton = new Button(() => {
                ((MyMotionAnimation)target).Play();
                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
                    PrefabStage.prefabStageClosing += OnPrefabStageClosing;
                }
            })
            {
                text = "Play",
                style = {
                    flexGrow = 1f,
                }
            };
            var restartButton = new Button(() => ((MyMotionAnimation)target).Restart())
            {
                text = "Restart",
                style = {
                    flexGrow = 1f,
                }
            };
            var stopButton = new Button(() => ((MyMotionAnimation)target).Pause())
            {
                text = "Pause",
                style = {
                    flexGrow = 1f,
                }
            };
            var resetButton = new Button(() => ((MyMotionAnimation)target).Stop())
            {
                text = "Stop",
                style = {
                    flexGrow = 1f,
                }
            };

            buttonGroup.Add(playButton);
            buttonGroup.Add(restartButton);
            buttonGroup.Add(stopButton);
            buttonGroup.Add(resetButton);

            buttonGroup.schedule.Execute(() =>
            {
                bool enabled = !IsActive();
                restartButton.SetEnabled(enabled);
                stopButton.SetEnabled(enabled);
                resetButton.SetEnabled(enabled);
            })
            .Every(10);

            box.Add(buttonGroup);

            return box;
        }

        private void RefleshComponentsView(bool applyModifiedProperties)
        {
            if (applyModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }

            _componentRoot.Clear();
            _componentRoot.Add(CreateComponentsPanel());
        }

        private AnimationComponentView CreateComponentGUI(SerializedProperty property)
        {
            var view = new AnimationComponentView();

            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                view.Text = "(Missing)";
                view.Icon = (Texture2D)EditorGUIUtility.IconContent("Error").image;
                view.EnabledToggle.value = true;
                view.SetEnabled(true);
                view.EnabledToggle.Q("unity-checkmark").style.visibility = Visibility.Hidden;
                view.Add(new HelpBox("The type referenced in SerializeReference is missing. You may have renamed the type or moved it to a different namespace or assembly.", HelpBoxMessageType.Error));
            }
            else
            {
                view.Text = property.FindPropertyRelative("_displayName").stringValue;

                SerializedProperty targetProperty = property.FindPropertyRelative("_target");
                if (targetProperty != null)
                {
                    view.Icon = GUIHelper.GetComponentIcon(targetProperty.GetPropertyType());
                }

                view.TrackPropertyValue(property.FindPropertyRelative("_displayName"), x =>
                {
                    view.Text = x.stringValue;
                });

                view.Foldout.BindProperty(property);

                SerializedProperty endProperty = property.GetEndProperty();
                bool isFirst = true;
                while (property.NextVisible(isFirst))
                {
                    if (SerializedProperty.EqualContents(property, endProperty)) break;
                    if (property.name == "_enabled") continue;
                    isFirst = false;

                    view.Add(new PropertyField(property));
                }
            }

            return view;
        }

        private ContextualMenuManipulator CreateContextMenuManipulator(SerializedProperty property, int arrayIndex, bool activeLeftClick)
        {
            var manipulator = new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Reset", _ =>
                {
                    Undo.RecordObject(serializedObject.targetObject, "Reset MyMotionAnimation component");
                    SerializedProperty elementProperty = property.GetArrayElementAtIndex(arrayIndex);
                    elementProperty.managedReferenceValue = ReflectionHelper.CreateDefaultInstance(elementProperty.managedReferenceValue.GetType());
                    RefleshComponentsView(true);
                }, string.IsNullOrEmpty(property.GetArrayElementAtIndex(arrayIndex).managedReferenceFullTypename) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

                evt.menu.AppendSeparator();

                evt.menu.AppendAction("Remove Component", _ =>
                {
                    property.DeleteArrayElementAtIndex(arrayIndex);
                    RefleshComponentsView(true);
                });

                evt.menu.AppendAction("Move Up", _ =>
                {
                    property.MoveArrayElement(arrayIndex, arrayIndex - 1);
                    RefleshComponentsView(true);
                }, arrayIndex == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);


                evt.menu.AppendAction("Move Down", _ =>
                {
                    property.MoveArrayElement(arrayIndex, arrayIndex + 1);
                    RefleshComponentsView(true);
                }, arrayIndex == property.arraySize - 1 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
            });

            if (activeLeftClick)
            {
                manipulator.activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse,
                });
            }

            return manipulator;
        }

        private bool IsActive()
        {
            return !((MyMotionAnimation)target).IsActive;
        }

        private void OnPrefabStageClosing(PrefabStage stage)
        {
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            foreach (MyMotionAnimation  i in stage.prefabContentsRoot.GetComponentsInChildren<MyMotionAnimation>(true))
            {
                i.Stop();
            }
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyMotion.Animation.Editor
{
    public class AnimationComponentView : VisualElement
    {
        private VisualElement _container;
        private readonly VisualElement _contextMenuButton;
        private readonly Foldout _foldout;
        private readonly VisualElement _icon;
        private readonly Toggle _enabledToggle;
        private readonly ProgressBar _progressBar;

        public Foldout Foldout => _foldout;
        public Toggle EnabledToggle => _enabledToggle;
        public VisualElement ContextMenuButton => _contextMenuButton;

        public string Text
        {
            get => _enabledToggle.text;
            set => _enabledToggle.text = value;
        }

        public float Progress
        {
            get => _progressBar.value;
            set => _progressBar.value = value;
        }

        public StyleBackground Icon
        {
            get => _icon.style.backgroundImage;
            set => _icon.style.backgroundImage = value;
        }

        public override VisualElement contentContainer => _container;

        public AnimationComponentView()
        {
            _container = this;

            var root = new HelpBox
            {
                style = {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1f,
                }
            };
            root.Clear();
            Add(root);

            _foldout = new Foldout
            {
                style = {
                    marginLeft = 15f,
                    paddingRight = 3f,
                    alignSelf = Align.Stretch,
                }
            };
            root.Add(_foldout);
            _foldout.Add(new VisualElement { style = { height = 5f } });
            VisualElement foldoutCheck = _foldout.Q(className: Foldout.checkmarkUssClassName);
            _icon = new VisualElement
            {
                style = {
                    width = 16f,
                    height = 16f,
                    marginTop = 0.5f,
                    marginRight = 2f,
                    backgroundImage = (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image,
                }
            };
            foldoutCheck.parent.Add(_icon);
            _enabledToggle = new Toggle
            {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                }
            };
            _enabledToggle.Q(className: Toggle.checkmarkUssClassName).style.marginRight = 6f;
            _enabledToggle.schedule.Execute(() =>
            {
                _enabledToggle.pickingMode = PickingMode.Ignore;
                _enabledToggle.Q(className: Toggle.inputUssClassName).pickingMode = PickingMode.Ignore;
                _enabledToggle.Q(className: Toggle.textUssClassName).pickingMode = PickingMode.Ignore;
                _enabledToggle.Q(className: Toggle.checkmarkUssClassName).pickingMode = PickingMode.Position;
            });
            foldoutCheck.parent.Add(_enabledToggle);

            _progressBar = new ProgressBar
            {
                lowValue = 0f,
                highValue = 1f,
                value = 0f,
                style = {
                    height = 2.5f,
                    position = Position.Absolute,
                    top = 22f,
                    left = 24f,
                    right = 2f,
                    alignSelf = Align.Stretch,
                }
            };
            VisualElement background = _progressBar.Q(className: AbstractProgressBar.backgroundUssClassName);
            background.style.borderTopWidth = 0f;
            background.style.borderBottomWidth = 0f;
            background.style.borderLeftWidth = 0f;
            background.style.borderRightWidth = 0f;
            VisualElement progress = _progressBar.Q(className: AbstractProgressBar.progressUssClassName);
            progress.style.backgroundColor = Color.white;
            progress.style.minWidth = 0f;
            _progressBar.schedule.Execute(() => progress.style.display = _progressBar.value > _progressBar.lowValue ? DisplayStyle.Flex : DisplayStyle.None).Every(10);
            root.Add(_progressBar);

            _contextMenuButton = new VisualElement
            {
                style = {
                    height = 15f,
                    width = 15f,
                    top = 4f,
                    right = 4f,
                    position = Position.Absolute,
                    backgroundImage = (Texture2D)EditorGUIUtility.IconContent("_Menu").image,
                }
            };
            root.Add(_contextMenuButton);

            _container = _foldout.contentContainer;
        }

        public new void SetEnabled(bool enabled)
        {
            Foldout.contentContainer.SetEnabled(enabled && EnabledToggle.value);
            EnabledToggle.SetEnabled(enabled);
            _icon.SetEnabled(enabled);
            _contextMenuButton.SetEnabled(enabled);
            _progressBar.SetEnabled(enabled);
        }
    }
}

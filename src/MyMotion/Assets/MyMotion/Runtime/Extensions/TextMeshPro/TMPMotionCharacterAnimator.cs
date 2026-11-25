#if MYMOTION_SUPPORT_TMP
using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace MyMotion.Extensions
{
    internal sealed class TMPMotionCharacterAnimator
    {
        private static readonly ConditionalWeakTable<TMP_Text, TMPMotionCharacterAnimator> s_animators = new();

        private readonly TMP_Text _text;
        private TMPMotionCharacter[] _characters = new TMPMotionCharacter[1];
        private bool[] _registeredCharacters = new bool[1];
        private int[] _characterIndices = new int[1];
        private MotionHandle[] _handles = new MotionHandle[1];
        private int _registeredCharacterCount;
        private int _handleCount;
        private bool _verticesDirty;

        private TMPMotionCharacterAnimator(TMP_Text text)
        {
            _text = text;
            _text.OnPreRenderText += ApplyToMesh;
        }

        public static TMPMotionCharacterAnimator Get(TMP_Text text) =>
            s_animators.GetValue(text, static target => new TMPMotionCharacterAnimator(target));

        public void Register(MotionHandle handle)
        {
            if (_handleCount == _handles.Length)
            {
                Array.Resize(ref _handles, _handles.Length * 2);
            }
            _handles[_handleCount++] = handle;
        }

        public void UpdateCharacter<TValue>(
            TValue value,
            int charIndex,
            TMPCharacterMotionUpdateAction<TValue> action)
        {
            EnsureCharacter(charIndex);
            action(value, charIndex, ref _characters[charIndex]);
            if (!_verticesDirty)
            {
                _verticesDirty = true;
                _text.SetVerticesDirty();
            }
        }

        private void EnsureCharacter(int charIndex)
        {
            if (charIndex >= _characters.Length)
            {
                int capacity = _characters.Length;
                while (capacity <= charIndex) capacity *= 2;
                Array.Resize(ref _characters, capacity);
                Array.Resize(ref _registeredCharacters, capacity);
                Array.Resize(ref _characterIndices, capacity);
            }

            if (_registeredCharacters[charIndex]) return;

            _registeredCharacters[charIndex] = true;
            _characterIndices[_registeredCharacterCount++] = charIndex;
            _characters[charIndex] = new TMPMotionCharacter
            {
                Position = Vector3.zero,
                Scale = Vector3.one,
                Rotation = Quaternion.identity,
                Color = _text.color
            };
        }

        private void ApplyToMesh(TMP_TextInfo textInfo)
        {
            _verticesDirty = false;
            TMP_CharacterInfo[] characters = textInfo.characterInfo;
            TMP_MeshInfo[] meshes = textInfo.meshInfo;

            for (int i = 0; i < _registeredCharacterCount; i++)
            {
                int charIndex = _characterIndices[i];
                if ((uint)charIndex >= (uint)textInfo.characterCount) continue;

                ref TMP_CharacterInfo characterInfo = ref characters[charIndex];
                if (!characterInfo.isVisible) continue;

                TMPMotionCharacter motionCharacter = _characters[charIndex];
                TMP_MeshInfo meshInfo = meshes[characterInfo.materialReferenceIndex];
                int vertexIndex = characterInfo.vertexIndex;
                Vector3[] vertices = meshInfo.vertices;
                Color32[] colors = meshInfo.colors32;
                Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) * 0.5f;

                for (int vertex = 0; vertex < 4; vertex++)
                {
                    Vector3 direction = vertices[vertexIndex + vertex] - center;
                    vertices[vertexIndex + vertex] =
                        center + motionCharacter.Rotation * Vector3.Scale(direction, motionCharacter.Scale) + motionCharacter.Position;
                    colors[vertexIndex + vertex] = motionCharacter.Color;
                }
            }

            int activeHandleCount = 0;
            for (int i = 0; i < _handleCount; i++)
            {
                MotionHandle handle = _handles[i];
                if (handle.IsActive()) _handles[activeHandleCount++] = handle;
            }
            _handleCount = activeHandleCount;

            if (_handleCount == 0)
            {
                _text.OnPreRenderText -= ApplyToMesh;
                s_animators.Remove(_text);
                _characters = Array.Empty<TMPMotionCharacter>();
                _registeredCharacters = Array.Empty<bool>();
                _characterIndices = Array.Empty<int>();
                _handles = Array.Empty<MotionHandle>();
                _registeredCharacterCount = 0;
            }
        }
    }
}
#endif

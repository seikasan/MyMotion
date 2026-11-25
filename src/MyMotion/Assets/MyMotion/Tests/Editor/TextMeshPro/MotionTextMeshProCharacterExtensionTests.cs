#if MYMOTION_SUPPORT_TMP
using NUnit.Framework;
using TMPro;
using UnityEngine;
using MyMotion.Extensions;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionTextMeshProExtensionTests
    {
        private static int s_observedCharacterIndex;
        private static float s_observedCharacterValue;

        [Test]
        public void CharacterActionMotion_UpdatesTheRequestedCharacterState()
        {
            using ManualMotionDispatcher dispatcher = new();
            s_observedCharacterIndex = -1;
            s_observedCharacterValue = -1f;

            MotionHandle handle = MMotion.Create(0f, 1f, 1f)
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToTMPChar(_text, 3, ObserveCharacterValue);

            dispatcher.Update(0.5d);

            Assert.That(s_observedCharacterIndex, Is.EqualTo(3));
            Assert.That(s_observedCharacterValue, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(handle.IsActive(), Is.True);
            handle.TryCancel();
        }

        [Test]
        public void CharacterPositionMotion_UsesTheCharacterBinding()
        {
            using ManualMotionDispatcher dispatcher = new();
            MotionHandle handle = MMotion.Create(Vector3.zero, Vector3.one, 1f)
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToTMPCharPosition(_text, 0);

            dispatcher.Update(0.5d);

            Assert.That(handle.IsActive(), Is.True);
            handle.TryCancel();
        }

        [Test]
        public void CharacterPositionMotion_ChangesMeshBeforeUpload()
        {
            _tmpMobileSdfShaderField = typeof(ShaderUtilities).GetField("k_ShaderRef_MobileSDF", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.That(_tmpMobileSdfShaderField, Is.Not.Null);
            _previousTmpMobileSdfShader = _tmpMobileSdfShaderField.GetValue(null) as Shader;
            Shader meshTestShader = Shader.Find("UI/Default");
            Assert.That(meshTestShader, Is.Not.Null);
            _tmpMobileSdfShaderField.SetValue(null, meshTestShader);

            _testFont = TMP_FontAsset.CreateFontAsset("Arial", "Regular", 48);
            Assert.That(_testFont, Is.Not.Null);
            _text.font = _testFont;
            _text.text = "A";
            _text.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
            Assert.That(_text.textInfo.characterCount, Is.EqualTo(1));

            Vector3 originalVertex = _text.textInfo.meshInfo[0].vertices[0];
            using ManualMotionDispatcher dispatcher = new();
            MotionHandle handle = MMotion.Create(Vector3.zero, new Vector3(0f, 2f, 0f), 1f)
                .WithEase(Ease.Linear)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToTMPCharPosition(_text, 0);

            dispatcher.Update(1d);
            _text.ForceMeshUpdate(ignoreActiveState: true);

            Vector3 transformedVertex = _text.textInfo.meshInfo[0].vertices[0];
            Assert.That(transformedVertex.y - originalVertex.y, Is.EqualTo(2f).Within(0.01f));
            Assert.That(handle.IsActive(), Is.False);

            _text.text = "A";
            _text.ForceMeshUpdate(ignoreActiveState: true);
            Vector3 rebuiltVertex = _text.textInfo.meshInfo[0].vertices[0];
            Assert.That(rebuiltVertex.y, Is.EqualTo(originalVertex.y).Within(0.01f));
        }

        private static void ObserveCharacterValue(float value, int index, ref TMPMotionCharacter character)
        {
            s_observedCharacterIndex = index;
            s_observedCharacterValue = value;
            character.Position.x = value;
        }
    }
}
#endif

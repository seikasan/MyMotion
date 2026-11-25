using System.Reflection;
using NUnit.Framework;
using TMPro;
using Unity.Collections;
using UnityEngine;
using MyMotion.Extensions;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionTextMeshProExtensionTests
    {
        private GameObject _gameObject;
        private TextMeshPro _text;
        private FieldInfo _settingsInstanceField;
        private TMP_Settings _previousSettings;
        private TMP_Settings _testSettings;
        private TMP_FontAsset _testFont;
        private FieldInfo _tmpMobileSdfShaderField;
        private Shader _previousTmpMobileSdfShader;

        [SetUp]
        public void SetUp()
        {
            MMotion.Reset();
            _settingsInstanceField = typeof(TMP_Settings).GetField("s_Instance", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(_settingsInstanceField, Is.Not.Null);
            _previousSettings = _settingsInstanceField.GetValue(null) as TMP_Settings;
            if (_previousSettings == null)
            {
                _testSettings = ScriptableObject.CreateInstance<TMP_Settings>();
                _settingsInstanceField.SetValue(null, _testSettings);
            }

            _gameObject = new GameObject("MyMotion TextMeshPro Test");
            _text = _gameObject.AddComponent<TextMeshPro>();
        }

        [TearDown]
        public void TearDown()
        {
            MMotion.Reset();
            if (_gameObject != null) Object.DestroyImmediate(_gameObject);
            if (_testFont != null) Object.DestroyImmediate(_testFont);
            if (_tmpMobileSdfShaderField != null) _tmpMobileSdfShaderField.SetValue(null, _previousTmpMobileSdfShader);
            if (_testSettings != null)
            {
                _settingsInstanceField.SetValue(null, _previousSettings);
                Object.DestroyImmediate(_testSettings);
            }
        }

        [Test]
        public void BuiltInMotion_BindsFontSizeAndColor()
        {
            Color from = new(0.1f, 0.2f, 0.3f, 0.4f);
            Color to = new(0.9f, 0.8f, 0.7f, 0.6f);

            MMotion.Create(12f, 24f, 1f).WithImmediateBind().BindToFontSize(_text);
            MMotion.Create(from, to, 1f).WithImmediateBind().BindToColor(_text);

            Assert.That(_text.fontSize, Is.EqualTo(12f));
            Assert.That(_text.color, Is.EqualTo(from));
        }

        [Test]
        public void FixedStringMotion_UsesTmpTextBufferBinding()
        {
            FixedString32Bytes from = new("開始");
            FixedString32Bytes to = new("完了");

            MMotion.String.Create32Bytes(in from, in to, 1f)
                .WithImmediateBind()
                .BindToText(_text);

            Assert.That(_text.text, Is.EqualTo("開始"));
        }

        [Test]
        public void IntegerMotion_FormatsLongMinimumWithoutOverflow()
        {
            MMotion.Create(long.MinValue, 0L, 1f)
                .WithImmediateBind()
                .BindToText(_text);

            Assert.That(_text.text, Is.EqualTo(long.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        [Test]
        public void FormattedIntegerMotion_UsesRequestedFormat()
        {
            MMotion.Create(7, 15, 1f)
                .WithImmediateBind()
                .BindToText(_text, "value={0:D2}");

            Assert.That(_text.text, Is.EqualTo("value=07"));
        }

        [Test]
        public void FloatMotion_UsesTmpNumericFormatting()
        {
            MMotion.Create(1.5f, 3.5f, 1f)
                .WithImmediateBind()
                .BindToText(_text);

            Assert.That(_text.text, Is.EqualTo("1.5"));
        }

        [Test]
        public void IntegerMotion_BindsVisibleCharacterCount()
        {
            MMotion.Create(2, 5, 1f)
                .WithImmediateBind()
                .BindToMaxVisibleCharacters(_text);

            Assert.That(_text.maxVisibleCharacters, Is.EqualTo(2));
        }
    }
}

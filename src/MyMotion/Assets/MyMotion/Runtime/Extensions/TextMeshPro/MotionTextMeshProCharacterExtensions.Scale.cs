#if MYMOTION_SUPPORT_TMP
using System;
using TMPro;
using UnityEngine;

namespace MyMotion.Extensions
{
    public static partial class MotionTextMeshProCharacterExtensions
    {
        public static MotionHandle BindToTMPCharScale(this MotionBuilder<Vector3> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector3 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale = value;
            });

        public static MotionHandle BindToTMPCharScale<TOptions, TAdapter>(
            this MotionBuilder<Vector3, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector3 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale = value;
            });

        public static MotionHandle BindToTMPCharScaleX(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.x = value;
            });

        public static MotionHandle BindToTMPCharScaleX<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.x = value;
            });

        public static MotionHandle BindToTMPCharScaleY(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.y = value;
            });

        public static MotionHandle BindToTMPCharScaleY<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.y = value;
            });

        public static MotionHandle BindToTMPCharScaleZ(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.z = value;
            });

        public static MotionHandle BindToTMPCharScaleZ<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.z = value;
            });

        public static MotionHandle BindToTMPCharScaleXY(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.x = value.x;
                                character.Scale.y = value.y;
            });

        public static MotionHandle BindToTMPCharScaleXY<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.x = value.x;
                                character.Scale.y = value.y;
            });

        public static MotionHandle BindToTMPCharScaleYZ(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.y = value.x;
                                character.Scale.z = value.y;
            });

        public static MotionHandle BindToTMPCharScaleYZ<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.y = value.x;
                                character.Scale.z = value.y;
            });

        public static MotionHandle BindToTMPCharScaleXZ(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.x = value.x;
                                character.Scale.z = value.y;
            });

        public static MotionHandle BindToTMPCharScaleXZ<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale.x = value.x;
                                character.Scale.z = value.y;
            });

        public static MotionHandle BindToTMPCharScaleXYZ(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale = new Vector3(value, value, value);
            });

        public static MotionHandle BindToTMPCharScaleXYZ<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Scale = new Vector3(value, value, value);
            });

        private static void Validate<TValue>(
            TMP_Text text,
            int charIndex,
            TMPCharacterMotionUpdateAction<TValue> action)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (charIndex < 0) throw new ArgumentOutOfRangeException(nameof(charIndex));
            if (action == null) throw new ArgumentNullException(nameof(action));
        }
    }
}
#endif

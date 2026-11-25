#if MYMOTION_SUPPORT_TMP
using TMPro;
using UnityEngine;

namespace MyMotion.Extensions
{
    public static partial class MotionTextMeshProCharacterExtensions
    {
        public static MotionHandle BindToTMPCharPosition(this MotionBuilder<Vector3> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector3 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position = value;
            });

        public static MotionHandle BindToTMPCharPosition<TOptions, TAdapter>(
            this MotionBuilder<Vector3, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector3 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position = value;
            });

        public static MotionHandle BindToTMPCharPositionX(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.x = value;
            });

        public static MotionHandle BindToTMPCharPositionX<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.x = value;
            });

        public static MotionHandle BindToTMPCharPositionY(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.y = value;
            });

        public static MotionHandle BindToTMPCharPositionY<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.y = value;
            });

        public static MotionHandle BindToTMPCharPositionZ(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.z = value;
            });

        public static MotionHandle BindToTMPCharPositionZ<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.z = value;
            });

        public static MotionHandle BindToTMPCharPositionXY(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.x = value.x;
                                character.Position.y = value.y;
            });

        public static MotionHandle BindToTMPCharPositionXY<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.x = value.x;
                                character.Position.y = value.y;
            });

        public static MotionHandle BindToTMPCharPositionYZ(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.y = value.x;
                                character.Position.z = value.y;
            });

        public static MotionHandle BindToTMPCharPositionYZ<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.y = value.x;
                                character.Position.z = value.y;
            });

        public static MotionHandle BindToTMPCharPositionXZ(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.x = value.x;
                                character.Position.z = value.y;
            });

        public static MotionHandle BindToTMPCharPositionXZ<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Position.x = value.x;
                                character.Position.z = value.y;
            });
    }
}
#endif

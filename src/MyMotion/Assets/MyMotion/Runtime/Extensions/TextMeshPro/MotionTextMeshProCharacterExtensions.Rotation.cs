#if MYMOTION_SUPPORT_TMP
using TMPro;
using UnityEngine;

namespace MyMotion.Extensions
{
    public static partial class MotionTextMeshProCharacterExtensions
    {
        public static MotionHandle BindToTMPCharRotation(this MotionBuilder<Quaternion> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Quaternion value, int _, ref TMPMotionCharacter character) =>
            {
                character.Rotation = value;
            });

        public static MotionHandle BindToTMPCharRotation<TOptions, TAdapter>(
            this MotionBuilder<Quaternion, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Quaternion, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Quaternion value, int _, ref TMPMotionCharacter character) =>
            {
                character.Rotation = value;
            });

        public static MotionHandle BindToTMPCharEulerAngles(this MotionBuilder<Vector3> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector3 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Rotation = Quaternion.Euler(value);
            });

        public static MotionHandle BindToTMPCharEulerAngles<TOptions, TAdapter>(
            this MotionBuilder<Vector3, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector3 value, int _, ref TMPMotionCharacter character) =>
            {
                character.Rotation = Quaternion.Euler(value);
            });

        public static MotionHandle BindToTMPCharEulerAnglesX(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.x = value;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesX<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.x = value;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesY(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.y = value;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesY<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.y = value;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesZ(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.z = value;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesZ<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.z = value;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesXY(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.x = value.x;
                                eulerAngles.y = value.y;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesXY<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.x = value.x;
                                eulerAngles.y = value.y;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesYZ(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.y = value.x;
                                eulerAngles.z = value.y;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesYZ<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.y = value.x;
                                eulerAngles.z = value.y;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesXZ(this MotionBuilder<Vector2> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.x = value.x;
                                eulerAngles.z = value.y;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });

        public static MotionHandle BindToTMPCharEulerAnglesXZ<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Vector2 value, int _, ref TMPMotionCharacter character) =>
            {
                Vector3 eulerAngles = character.Rotation.eulerAngles;
                                eulerAngles.x = value.x;
                                eulerAngles.z = value.y;
                                character.Rotation = Quaternion.Euler(eulerAngles);
            });
    }
}
#endif

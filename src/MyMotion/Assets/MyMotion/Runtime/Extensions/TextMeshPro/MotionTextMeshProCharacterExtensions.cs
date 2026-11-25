#if MYMOTION_SUPPORT_TMP
using TMPro;
using UnityEngine;

namespace MyMotion.Extensions
{
    /// <summary>
    /// TextMeshPro の1文字ごとにアニメーションさせる bind 拡張。
    /// </summary>
    public static partial class MotionTextMeshProCharacterExtensions
    {
        public static MotionHandle BindToTMPChar<TValue>(
            this MotionBuilder<TValue> builder,
            TMP_Text text,
            int charIndex,
            TMPCharacterMotionUpdateAction<TValue> action)
            where TValue : unmanaged
        {
            Validate(text, charIndex, action);
            TMPMotionCharacterAnimator animator = TMPMotionCharacterAnimator.Get(text);
            var state = new TMPCharacterBindingState<TValue>(
                animator,
                charIndex,
                action);
            MotionHandle handle = builder.Bind(state, static (value, binding) => binding.Apply(value));
            animator.Register(handle);
            return handle;
        }

        public static MotionHandle BindToTMPChar<TValue, TOptions, TAdapter>(
            this MotionBuilder<TValue, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex,
            TMPCharacterMotionUpdateAction<TValue> action)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            Validate(text, charIndex, action);
            TMPMotionCharacterAnimator animator = TMPMotionCharacterAnimator.Get(text);
            var state = new TMPCharacterBindingState<TValue>(
                animator,
                charIndex,
                action);
            MotionHandle handle = builder.Bind(state, static (value, binding) => binding.Apply(value));
            animator.Register(handle);
            return handle;
        }

        public static MotionHandle BindToTMPCharColor(this MotionBuilder<Color> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (Color value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color = value;
            });

        public static MotionHandle BindToTMPCharColor<TOptions, TAdapter>(
            this MotionBuilder<Color, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (Color value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color = value;
            });

        public static MotionHandle BindToTMPCharColorR(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.r = value;
            });

        public static MotionHandle BindToTMPCharColorR<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.r = value;
            });

        public static MotionHandle BindToTMPCharColorG(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.g = value;
            });

        public static MotionHandle BindToTMPCharColorG<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.g = value;
            });

        public static MotionHandle BindToTMPCharColorB(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.b = value;
            });

        public static MotionHandle BindToTMPCharColorB<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.b = value;
            });

        public static MotionHandle BindToTMPCharColorA(this MotionBuilder<float> builder, TMP_Text text, int charIndex) =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.a = value;
            });

        public static MotionHandle BindToTMPCharColorA<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            TMP_Text text,
            int charIndex)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions> =>
            builder.BindToTMPChar(text, charIndex, static (float value, int _, ref TMPMotionCharacter character) =>
            {
                character.Color.a = value;
            });
    }
}
#endif

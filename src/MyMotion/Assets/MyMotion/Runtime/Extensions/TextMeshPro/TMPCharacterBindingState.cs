#if MYMOTION_SUPPORT_TMP
namespace MyMotion.Extensions
{
    internal readonly struct TMPCharacterBindingState<TValue>
    {
        private readonly TMPMotionCharacterAnimator _animator;
        private readonly int _charIndex;
        private readonly TMPCharacterMotionUpdateAction<TValue> _action;

        public TMPCharacterBindingState(
            TMPMotionCharacterAnimator animator,
            int charIndex,
            TMPCharacterMotionUpdateAction<TValue> action)
        {
            _animator = animator;
            _charIndex = charIndex;
            _action = action;
        }

        public void Apply(TValue value) =>
            _animator.UpdateCharacter(value, _charIndex, _action);
    }
}
#endif

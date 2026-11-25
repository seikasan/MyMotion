#if MYMOTION_ANIMATION_PHYSICS

using System;
using MyMotion.Adapters;
using UnityEngine;

namespace MyMotion.Animation.Components
{
    public abstract class RigidbodyPositionAnimationBase<TOptions, TAdapter> : PropertyAnimationComponent<Rigidbody, Vector3, TOptions, TAdapter>
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
    {
        [SerializeField] private bool _useMovePosition = true;

        protected override Vector3 GetValue(Rigidbody target)
        {
            return target.position;
        }

        protected override void SetValue(Rigidbody target, in Vector3 value)
        {
            if (_useMovePosition) target.MovePosition(value);
            else target.position = value;
        }

        protected override Vector3 GetRelativeValue(in Vector3 startValue, in Vector3 relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rigidbody/Position")]
    public sealed class RigidbodyPositionAnimation : RigidbodyPositionAnimationBase<NoOptions, Vector3MotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rigidbody/Position (Punch)")]
    public sealed class RigidbodyPositionPunchAnimation : RigidbodyPositionAnimationBase<PunchOptions, Vector3PunchMotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rigidbody/Position (Shake)")]
    public sealed class RigidbodyPositionShakeAnimation : RigidbodyPositionAnimationBase<ShakeOptions, Vector3ShakeMotionAdapter> { }

    public abstract class RigidbodyRotationAnimationBase<TOptions, TAdapter> : PropertyAnimationComponent<Rigidbody, Vector3, TOptions, TAdapter>
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
    {
        [SerializeField] private bool _useMoveRotation;

        protected override Vector3 GetValue(Rigidbody target)
        {
            return target.rotation.eulerAngles;
        }

        protected override void SetValue(Rigidbody target, in Vector3 value)
        {
            if (_useMoveRotation) target.MoveRotation(Quaternion.Euler(value));
            else target.rotation = Quaternion.Euler(value);
        }

        protected override Vector3 GetRelativeValue(in Vector3 startValue, in Vector3 relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rigidbody/Rotation")]
    public sealed class RigidbodyRotationAnimation : RigidbodyRotationAnimationBase<NoOptions, Vector3MotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rigidbody/Rotation (Punch)")]
    public sealed class RigidbodyRotationPunchAnimation : RigidbodyRotationAnimationBase<PunchOptions, Vector3PunchMotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rigidbody/Rotation (Shake)")]
    public sealed class RigidbodyRotationShakeAnimation : RigidbodyRotationAnimationBase<ShakeOptions, Vector3ShakeMotionAdapter> { }
}

#endif

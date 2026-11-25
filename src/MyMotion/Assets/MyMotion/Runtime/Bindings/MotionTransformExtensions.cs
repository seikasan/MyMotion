using System;
using UnityEngine;
using MyMotion.Internal;

namespace MyMotion.Extensions
{
    /// <summary>
    /// Transform 拡張が登録する delegate。Update の Binding ループはこの参照と一致したとき、
    /// delegate を介さずに Transform の setter を直接呼ぶ。
    /// </summary>
    internal static class TransformBindings
    {
        public static readonly Action<Vector3, Transform> Position = static (value, target) => target.position = value;
        public static readonly Action<Vector3, Transform> LocalPosition = static (value, target) => target.localPosition = value;
        public static readonly Action<Vector3, Transform> LocalScale = static (value, target) => target.localScale = value;
        public static readonly Action<Quaternion, Transform> Rotation = static (value, target) => target.rotation = value;
        public static readonly Action<Quaternion, Transform> LocalRotation = static (value, target) => target.localRotation = value;
    }

    /// <summary>MyMotion の標準値型向け Transform bind 拡張。</summary>
    public static partial class MotionTransformExtensions
    {
        public static MotionHandle BindToPosition(this MotionBuilder<Vector3> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.Position, TransformBindings.Position);
        public static MotionHandle BindToPositionX(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.PositionX, static (value, target) => { Vector3 position = target.position; position.x = value; target.position = position; });
        public static MotionHandle BindToPositionY(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.PositionY, static (value, target) => { Vector3 position = target.position; position.y = value; target.position = position; });
        public static MotionHandle BindToPositionZ(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.PositionZ, static (value, target) => { Vector3 position = target.position; position.z = value; target.position = position; });
        public static MotionHandle BindToPositionXY(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.y = value.y; target.position = position; });
        public static MotionHandle BindToPositionXZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.z = value.y; target.position = position; });
        public static MotionHandle BindToPositionYZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 position = target.position; position.y = value.x; position.z = value.y; target.position = position; });

        public static MotionHandle BindToLocalPosition(this MotionBuilder<Vector3> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalPosition, TransformBindings.LocalPosition);
        public static MotionHandle BindToLocalPositionX(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalPositionX, static (value, target) => { Vector3 position = target.localPosition; position.x = value; target.localPosition = position; });
        public static MotionHandle BindToLocalPositionY(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalPositionY, static (value, target) => { Vector3 position = target.localPosition; position.y = value; target.localPosition = position; });
        public static MotionHandle BindToLocalPositionZ(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalPositionZ, static (value, target) => { Vector3 position = target.localPosition; position.z = value; target.localPosition = position; });
        public static MotionHandle BindToLocalPositionXY(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 position = target.localPosition; position.x = value.x; position.y = value.y; target.localPosition = position; });
        public static MotionHandle BindToLocalPositionXZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 position = target.localPosition; position.x = value.x; position.z = value.y; target.localPosition = position; });
        public static MotionHandle BindToLocalPositionYZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 position = target.localPosition; position.y = value.x; position.z = value.y; target.localPosition = position; });

        public static MotionHandle BindToRotation(this MotionBuilder<Quaternion> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.Rotation, TransformBindings.Rotation);
        public static MotionHandle BindToLocalRotation(this MotionBuilder<Quaternion> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalRotation, TransformBindings.LocalRotation);

        public static MotionHandle BindToEulerAngles(this MotionBuilder<Vector3> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.EulerAngles, static (value, target) => target.eulerAngles = value);
        public static MotionHandle BindToEulerAnglesX(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.x = value; target.eulerAngles = angles; });
        public static MotionHandle BindToEulerAnglesY(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.y = value; target.eulerAngles = angles; });
        public static MotionHandle BindToEulerAnglesZ(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.z = value; target.eulerAngles = angles; });
        public static MotionHandle BindToEulerAnglesXY(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.x = value.x; angles.y = value.y; target.eulerAngles = angles; });
        public static MotionHandle BindToEulerAnglesXZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.x = value.x; angles.z = value.y; target.eulerAngles = angles; });
        public static MotionHandle BindToEulerAnglesYZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.y = value.x; angles.z = value.y; target.eulerAngles = angles; });

        public static MotionHandle BindToLocalEulerAngles(this MotionBuilder<Vector3> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalEulerAngles, static (value, target) => target.localEulerAngles = value);
        public static MotionHandle BindToLocalEulerAnglesX(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.x = value; target.localEulerAngles = angles; });
        public static MotionHandle BindToLocalEulerAnglesY(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.y = value; target.localEulerAngles = angles; });
        public static MotionHandle BindToLocalEulerAnglesZ(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.z = value; target.localEulerAngles = angles; });
        public static MotionHandle BindToLocalEulerAnglesXY(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.x = value.x; angles.y = value.y; target.localEulerAngles = angles; });
        public static MotionHandle BindToLocalEulerAnglesXZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.x = value.x; angles.z = value.y; target.localEulerAngles = angles; });
        public static MotionHandle BindToLocalEulerAnglesYZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.y = value.x; angles.z = value.y; target.localEulerAngles = angles; });

        public static MotionHandle BindToLocalScale(this MotionBuilder<Vector3> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalScale, TransformBindings.LocalScale);
        public static MotionHandle BindToLocalScaleX(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalScaleX, static (value, target) => { Vector3 scale = target.localScale; scale.x = value; target.localScale = scale; });
        public static MotionHandle BindToLocalScaleY(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalScaleY, static (value, target) => { Vector3 scale = target.localScale; scale.y = value; target.localScale = scale; });
        public static MotionHandle BindToLocalScaleZ(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalScaleZ, static (value, target) => { Vector3 scale = target.localScale; scale.z = value; target.localScale = scale; });
        public static MotionHandle BindToLocalScaleXY(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 scale = target.localScale; scale.x = value.x; scale.y = value.y; target.localScale = scale; });
        public static MotionHandle BindToLocalScaleXZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 scale = target.localScale; scale.x = value.x; scale.z = value.y; target.localScale = scale; });
        public static MotionHandle BindToLocalScaleYZ(this MotionBuilder<Vector2> builder, Transform transform) => Bind(builder, transform, static (value, target) => { Vector3 scale = target.localScale; scale.y = value.x; scale.z = value.y; target.localScale = scale; });
        public static MotionHandle BindToLocalScaleXYZ(this MotionBuilder<float> builder, Transform transform) => Bind(builder, transform, TransformTargetKind.LocalScaleXYZ, static (value, target) => target.localScale = new Vector3(value, value, value));

        public static MotionHandle BindToAnchoredPosition(this MotionBuilder<Vector2> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => target.anchoredPosition = value);
        public static MotionHandle BindToAnchoredPositionX(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector2 position = target.anchoredPosition; position.x = value; target.anchoredPosition = position; });
        public static MotionHandle BindToAnchoredPositionY(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector2 position = target.anchoredPosition; position.y = value; target.anchoredPosition = position; });
        public static MotionHandle BindToAnchoredPosition3D(this MotionBuilder<Vector3> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => target.anchoredPosition3D = value);
        public static MotionHandle BindToAnchoredPosition3DX(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector3 position = target.anchoredPosition3D; position.x = value; target.anchoredPosition3D = position; });
        public static MotionHandle BindToAnchoredPosition3DY(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector3 position = target.anchoredPosition3D; position.y = value; target.anchoredPosition3D = position; });
        public static MotionHandle BindToAnchoredPosition3DZ(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector3 position = target.anchoredPosition3D; position.z = value; target.anchoredPosition3D = position; });
        public static MotionHandle BindToAnchorMin(this MotionBuilder<Vector2> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => target.anchorMin = value);
        public static MotionHandle BindToAnchorMax(this MotionBuilder<Vector2> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => target.anchorMax = value);
        public static MotionHandle BindToSizeDelta(this MotionBuilder<Vector2> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => target.sizeDelta = value);
        public static MotionHandle BindToSizeDeltaX(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector2 size = target.sizeDelta; size.x = value; target.sizeDelta = size; });
        public static MotionHandle BindToSizeDeltaY(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector2 size = target.sizeDelta; size.y = value; target.sizeDelta = size; });
        public static MotionHandle BindToPivot(this MotionBuilder<Vector2> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => target.pivot = value);
        public static MotionHandle BindToPivotX(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector2 pivot = target.pivot; pivot.x = value; target.pivot = pivot; });
        public static MotionHandle BindToPivotY(this MotionBuilder<float> builder, RectTransform rectTransform) => Bind(builder, rectTransform, static (value, target) => { Vector2 pivot = target.pivot; pivot.y = value; target.pivot = pivot; });

        private static MotionHandle Bind<TValue>(MotionBuilder<TValue> builder, Transform transform, TransformTargetKind kind, Action<TValue, Transform> binding)
        {
            MotionHandle handle = Bind(builder, transform, binding);
            // managed Binding は残したまま、Update の書き込みだけを Burst の Transform Job に移す。
            handle.AttachTransformTarget(transform, kind);
            return handle;
        }

        private static MotionHandle Bind<TValue>(MotionBuilder<TValue> builder, Transform transform, Action<TValue, Transform> binding)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            return builder.Bind(transform, binding);
        }

        private static MotionHandle Bind<TValue>(MotionBuilder<TValue> builder, RectTransform rectTransform, Action<TValue, RectTransform> binding)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }

            return builder.Bind(rectTransform, binding);
        }
    }
}

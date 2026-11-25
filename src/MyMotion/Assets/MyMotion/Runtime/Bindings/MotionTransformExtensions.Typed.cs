using System;
using UnityEngine;
using MyMotion.Internal;

namespace MyMotion.Extensions
{
    // LitMotion と同じく、型付き builder (Punch / Shake / 独自 Adapter の MotionBuilder<TValue, TOptions, TAdapter>) も受け取る。
    public static partial class MotionTransformExtensions
    {
        public static MotionHandle BindToPosition<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, transform, TransformTargetKind.Position, TransformBindings.Position);

        public static MotionHandle BindToPositionX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.PositionX, static (value, target) => { Vector3 position = target.position; position.x = value; target.position = position; });

        public static MotionHandle BindToPositionY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.PositionY, static (value, target) => { Vector3 position = target.position; position.y = value; target.position = position; });

        public static MotionHandle BindToPositionZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.PositionZ, static (value, target) => { Vector3 position = target.position; position.z = value; target.position = position; });

        public static MotionHandle BindToPositionXY<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.y = value.y; target.position = position; });

        public static MotionHandle BindToPositionXZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.z = value.y; target.position = position; });

        public static MotionHandle BindToPositionYZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 position = target.position; position.y = value.x; position.z = value.y; target.position = position; });

        public static MotionHandle BindToLocalPosition<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalPosition, TransformBindings.LocalPosition);

        public static MotionHandle BindToLocalPositionX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalPositionX, static (value, target) => { Vector3 position = target.localPosition; position.x = value; target.localPosition = position; });

        public static MotionHandle BindToLocalPositionY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalPositionY, static (value, target) => { Vector3 position = target.localPosition; position.y = value; target.localPosition = position; });

        public static MotionHandle BindToLocalPositionZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalPositionZ, static (value, target) => { Vector3 position = target.localPosition; position.z = value; target.localPosition = position; });

        public static MotionHandle BindToLocalPositionXY<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 position = target.localPosition; position.x = value.x; position.y = value.y; target.localPosition = position; });

        public static MotionHandle BindToLocalPositionXZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 position = target.localPosition; position.x = value.x; position.z = value.y; target.localPosition = position; });

        public static MotionHandle BindToLocalPositionYZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 position = target.localPosition; position.y = value.x; position.z = value.y; target.localPosition = position; });

        public static MotionHandle BindToRotation<TOptions, TAdapter>(this MotionBuilder<Quaternion, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Quaternion, TOptions>
            => Bind(builder, transform, TransformTargetKind.Rotation, TransformBindings.Rotation);

        public static MotionHandle BindToLocalRotation<TOptions, TAdapter>(this MotionBuilder<Quaternion, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Quaternion, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalRotation, TransformBindings.LocalRotation);

        public static MotionHandle BindToEulerAngles<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, transform, TransformTargetKind.EulerAngles, static (value, target) => target.eulerAngles = value);

        public static MotionHandle BindToEulerAnglesX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.x = value; target.eulerAngles = angles; });

        public static MotionHandle BindToEulerAnglesY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.y = value; target.eulerAngles = angles; });

        public static MotionHandle BindToEulerAnglesZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.z = value; target.eulerAngles = angles; });

        public static MotionHandle BindToEulerAnglesXY<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.x = value.x; angles.y = value.y; target.eulerAngles = angles; });

        public static MotionHandle BindToEulerAnglesXZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.x = value.x; angles.z = value.y; target.eulerAngles = angles; });

        public static MotionHandle BindToEulerAnglesYZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.eulerAngles; angles.y = value.x; angles.z = value.y; target.eulerAngles = angles; });

        public static MotionHandle BindToLocalEulerAngles<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalEulerAngles, static (value, target) => target.localEulerAngles = value);

        public static MotionHandle BindToLocalEulerAnglesX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.x = value; target.localEulerAngles = angles; });

        public static MotionHandle BindToLocalEulerAnglesY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.y = value; target.localEulerAngles = angles; });

        public static MotionHandle BindToLocalEulerAnglesZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.z = value; target.localEulerAngles = angles; });

        public static MotionHandle BindToLocalEulerAnglesXY<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.x = value.x; angles.y = value.y; target.localEulerAngles = angles; });

        public static MotionHandle BindToLocalEulerAnglesXZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.x = value.x; angles.z = value.y; target.localEulerAngles = angles; });

        public static MotionHandle BindToLocalEulerAnglesYZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 angles = target.localEulerAngles; angles.y = value.x; angles.z = value.y; target.localEulerAngles = angles; });

        public static MotionHandle BindToLocalScale<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalScale, TransformBindings.LocalScale);

        public static MotionHandle BindToLocalScaleX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalScaleX, static (value, target) => { Vector3 scale = target.localScale; scale.x = value; target.localScale = scale; });

        public static MotionHandle BindToLocalScaleY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalScaleY, static (value, target) => { Vector3 scale = target.localScale; scale.y = value; target.localScale = scale; });

        public static MotionHandle BindToLocalScaleZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalScaleZ, static (value, target) => { Vector3 scale = target.localScale; scale.z = value; target.localScale = scale; });

        public static MotionHandle BindToLocalScaleXY<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 scale = target.localScale; scale.x = value.x; scale.y = value.y; target.localScale = scale; });

        public static MotionHandle BindToLocalScaleXZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 scale = target.localScale; scale.x = value.x; scale.z = value.y; target.localScale = scale; });

        public static MotionHandle BindToLocalScaleYZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, transform, static (value, target) => { Vector3 scale = target.localScale; scale.y = value.x; scale.z = value.y; target.localScale = scale; });

        public static MotionHandle BindToLocalScaleXYZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Transform transform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, transform, TransformTargetKind.LocalScaleXYZ, static (value, target) => target.localScale = new Vector3(value, value, value));

        public static MotionHandle BindToAnchoredPosition<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, rectTransform, static (value, target) => target.anchoredPosition = value);

        public static MotionHandle BindToAnchoredPositionX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector2 position = target.anchoredPosition; position.x = value; target.anchoredPosition = position; });

        public static MotionHandle BindToAnchoredPositionY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector2 position = target.anchoredPosition; position.y = value; target.anchoredPosition = position; });

        public static MotionHandle BindToAnchoredPosition3D<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
            => Bind(builder, rectTransform, static (value, target) => target.anchoredPosition3D = value);

        public static MotionHandle BindToAnchoredPosition3DX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector3 position = target.anchoredPosition3D; position.x = value; target.anchoredPosition3D = position; });

        public static MotionHandle BindToAnchoredPosition3DY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector3 position = target.anchoredPosition3D; position.y = value; target.anchoredPosition3D = position; });

        public static MotionHandle BindToAnchoredPosition3DZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector3 position = target.anchoredPosition3D; position.z = value; target.anchoredPosition3D = position; });

        public static MotionHandle BindToAnchorMin<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, rectTransform, static (value, target) => target.anchorMin = value);

        public static MotionHandle BindToAnchorMax<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, rectTransform, static (value, target) => target.anchorMax = value);

        public static MotionHandle BindToSizeDelta<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, rectTransform, static (value, target) => target.sizeDelta = value);

        public static MotionHandle BindToSizeDeltaX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector2 size = target.sizeDelta; size.x = value; target.sizeDelta = size; });

        public static MotionHandle BindToSizeDeltaY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector2 size = target.sizeDelta; size.y = value; target.sizeDelta = size; });

        public static MotionHandle BindToPivot<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
            => Bind(builder, rectTransform, static (value, target) => target.pivot = value);

        public static MotionHandle BindToPivotX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector2 pivot = target.pivot; pivot.x = value; target.pivot = pivot; });

        public static MotionHandle BindToPivotY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, RectTransform rectTransform)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, rectTransform, static (value, target) => { Vector2 pivot = target.pivot; pivot.y = value; target.pivot = pivot; });

        private static MotionHandle Bind<TValue, TOptions, TAdapter>(MotionBuilder<TValue, TOptions, TAdapter> builder, Transform transform, TransformTargetKind kind, Action<TValue, Transform> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions> => Bind(builder, transform, binding);

        private static MotionHandle Bind<TValue, TOptions, TAdapter>(MotionBuilder<TValue, TOptions, TAdapter> builder, Transform transform, Action<TValue, Transform> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            return builder.Bind(transform, binding);
        }

        private static MotionHandle Bind<TValue, TOptions, TAdapter>(MotionBuilder<TValue, TOptions, TAdapter> builder, RectTransform rectTransform, Action<TValue, RectTransform> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (rectTransform == null) throw new ArgumentNullException(nameof(rectTransform));
            return builder.Bind(rectTransform, binding);
        }
    }
}

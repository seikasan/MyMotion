using System;
using UnityEngine;

namespace MyMotion.Animation.Components
{
    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Aspect")]
    public sealed class CameraAspectAnimation : FloatPropertyAnimationComponent<Camera>
    {
        protected override float GetValue(Camera target) => target.aspect;
        protected override void SetValue(Camera target, in float value) => target.aspect = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Near Clip Plane")]
    public sealed class CameraNearClipPlaneAnimation : FloatPropertyAnimationComponent<Camera>
    {
        protected override float GetValue(Camera target) => target.nearClipPlane;
        protected override void SetValue(Camera target, in float value) => target.nearClipPlane = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Far Clip Plane")]
    public sealed class CameraFarClipPlaneAnimation : FloatPropertyAnimationComponent<Camera>
    {
        protected override float GetValue(Camera target) => target.farClipPlane;
        protected override void SetValue(Camera target, in float value) => target.farClipPlane = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Field Of View")]
    public sealed class CameraFieldOfViewAnimation : FloatPropertyAnimationComponent<Camera>
    {
        protected override float GetValue(Camera target) => target.fieldOfView;
        protected override void SetValue(Camera target, in float value) => target.fieldOfView = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Orthographic Size")]
    public sealed class CameraOrthographicSizeAnimation : FloatPropertyAnimationComponent<Camera>
    {
        protected override float GetValue(Camera target) => target.orthographicSize;
        protected override void SetValue(Camera target, in float value) => target.orthographicSize = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Rect")]
    public sealed class CameraRectAnimation : RectPropertyAnimationComponent<Camera>
    {
        protected override Rect GetValue(Camera target) => target.rect;
        protected override void SetValue(Camera target, in Rect value) => target.rect = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Pixel Rect")]
    public sealed class CameraPixelRectAnimation : RectPropertyAnimationComponent<Camera>
    {
        protected override Rect GetValue(Camera target) => target.pixelRect;
        protected override void SetValue(Camera target, in Rect value) => target.pixelRect = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Camera/Background Color")]
    public sealed class CameraBackgroundColorAnimation : ColorPropertyAnimationComponent<Camera>
    {
        protected override Color GetValue(Camera target) => target.backgroundColor;
        protected override void SetValue(Camera target, in Color value) => target.backgroundColor = value;
    }
}

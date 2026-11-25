using System;
using UnityEngine;

namespace MyMotion.Animation.Components
{
    [Serializable]
    [MyMotionAnimationComponentMenu("UI/Rect Transform/Size Delta")]
    public sealed class RectTransformSizeDeltaAnimation : Vector2PropertyAnimationComponent<RectTransform>
    {
        protected override Vector2 GetValue(RectTransform target) => target.sizeDelta;
        protected override void SetValue(RectTransform target, in Vector2 value) => target.sizeDelta = value;
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("UI/Rect Transform/Pivot")]
    public sealed class RectTransformPivotAnimation : Vector2PropertyAnimationComponent<RectTransform>
    {
        protected override Vector2 GetValue(RectTransform target) => target.pivot;
        protected override void SetValue(RectTransform target, in Vector2 value) => target.pivot = value;
    }
}

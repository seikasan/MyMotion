using System;
using UnityEngine;
#if MYMOTION_ANIMATION_RENDER_PIPELINES
using UnityEngine.Rendering;
#endif

namespace MyMotion.Animation.Components
{
    [Serializable]
    [MyMotionAnimationComponentMenu("Rendering/Material/Property (Float)")]
    public sealed class MaterialFloatAnimation : FloatPropertyAnimationComponent<Material>
    {
        [SerializeField] private string _propertyName = "";

        protected override float GetValue(Material target)
        {
            return target.GetFloat(_propertyName);
        }

        protected override void SetValue(Material target, in float value)
        {
            target.SetFloat(_propertyName, value);
        }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rendering/Material/Property (Int)")]
    public sealed class MaterialIntAnimation : IntPropertyAnimationComponent<Material>
    {
        [SerializeField] private string _propertyName = "";

        protected override int GetValue(Material target)
        {
            return target.GetInteger(_propertyName);
        }

        protected override void SetValue(Material target, in int value)
        {
            target.SetInteger(_propertyName, value);
        }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rendering/Material/Property (Vector)")]
    public sealed class MaterialVectorAnimation : Vector4PropertyAnimationComponent<Material>
    {
        [SerializeField] private string _propertyName = "";

        protected override Vector4 GetValue(Material target)
        {
            return target.GetVector(_propertyName);
        }

        protected override void SetValue(Material target, in Vector4 value)
        {
            target.SetVector(_propertyName, value);
        }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rendering/Material/Property (Color)")]
    public sealed class MaterialColorAnimation : ColorPropertyAnimationComponent<Material>
    {
        [SerializeField] private string _propertyName = "_Color";

        protected override Color GetValue(Material target)
        {
            return target.GetColor(_propertyName);
        }

        protected override void SetValue(Material target, in Color value)
        {
            target.SetColor(_propertyName, value);
        }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Rendering/Sprite Renderer/Color")]
    public sealed class SpriteRendererColorAnimation : ColorPropertyAnimationComponent<SpriteRenderer>
    {
        protected override Color GetValue(SpriteRenderer target)
        {
            return target.color;
        }

        protected override void SetValue(SpriteRenderer target, in Color value)
        {
            target.color = value;
        }
    }

#if MYMOTION_ANIMATION_RENDER_PIPELINES

    [Serializable]
    [MyMotionAnimationComponentMenu("Rendering/Volume/Weight")]
    public sealed class VolumeWeightAnimation : FloatPropertyAnimationComponent<Volume>
    {
        protected override float GetValue(Volume target) => target.weight;
        protected override void SetValue(Volume target, in float value) => target.weight = value;
    }

#endif
}

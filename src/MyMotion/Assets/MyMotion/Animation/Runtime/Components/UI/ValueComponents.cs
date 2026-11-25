using System;
using MyMotion.Adapters;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MyMotion.Animation.Components
{
    public abstract class ValueAnimationComponent<TValue, TOptions, TAdapter> : MyMotionAnimationComponent
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        [SerializeField] private SerializableMotionSettings<TValue, TOptions> _settings;
        [SerializeField] private UnityEvent<TValue> _onValueChanged;

        public override MotionHandle Play()
        {
            return MMotion.Create<TValue, TOptions, TAdapter>(_settings)
                .Bind(this, (x, state) =>
                {
                    state._onValueChanged.Invoke(x);
                });
        }

        public override void OnStop() { }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Float")]
    public sealed class FloatValueAnimation : ValueAnimationComponent<float, NoOptions, FloatMotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Double")]
    public sealed class DoubleValueAnimation : ValueAnimationComponent<double, NoOptions, DoubleMotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Int")]
    public sealed class IntValueAnimation : ValueAnimationComponent<int, IntegerOptions, IntMotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Long")]
    public sealed class LongValueAnimation : ValueAnimationComponent<long, IntegerOptions, LongMotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Vector2")]
    public sealed class Vector2ValueAnimation : ValueAnimationComponent<Vector2, NoOptions, Vector2MotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Vector3")]
    public sealed class Vector3ValueAnimation : ValueAnimationComponent<Vector3, NoOptions, Vector3MotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Vector4")]
    public sealed class Vector4ValueAnimation : ValueAnimationComponent<Vector4, NoOptions, Vector4MotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/Color")]
    public sealed class ColorValueAnimation : ValueAnimationComponent<Color, NoOptions, ColorMotionAdapter> { }

    [Serializable]
    [MyMotionAnimationComponentMenu("Value/String")]
    public sealed class StringValueAnimation : MyMotionAnimationComponent
    {
        [SerializeField] private SerializableMotionSettings<FixedString512Bytes, StringOptions> _settings;
        [SerializeField] private UnityEvent<string> _onValueChanged;

        public override MotionHandle Play()
        {
            return MMotion.Create<FixedString512Bytes, StringOptions, FixedString512BytesMotionAdapter>(_settings)
                .Bind(this, static (x, state) =>
                {
                    // TODO: アロケーションを避ける
                    state._onValueChanged.Invoke(x.ConvertToString());
                });
        }

        public override void OnStop() { }
    }
}

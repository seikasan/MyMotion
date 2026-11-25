using System;
using MyMotion.Adapters;
using Unity.Collections;
using UnityEngine;

namespace MyMotion.Animation
{
    [Serializable]
    public abstract class PropertyAnimationComponent<TObject, TValue, TOptions, TAdapter> : MyMotionAnimationComponent
        where TObject : UnityEngine.Object
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        [SerializeField] private TObject _target;
        [SerializeField] private SerializableMotionSettings<TValue, TOptions> _settings;
        [SerializeField] private bool _relative;

        private TValue _startValue;

        public override void OnStop()
        {
            if (_target == null) return;
            SetValue(_target, _startValue);
        }

        public override MotionHandle Play()
        {
            _startValue = GetValue(_target);

            MotionHandle handle;

            if (_relative)
            {
                handle = MMotion.Create<TValue, TOptions, TAdapter>(_settings)
                    .Bind(this, (x, state) =>
                    {
                        state.SetValue(_target, state.GetRelativeValue(state._startValue, x));
                    });
            }
            else
            {
                handle = MMotion.Create<TValue, TOptions, TAdapter>(_settings)
                    .Bind(this, (x, state) =>
                    {
                        state.SetValue(_target, x);
                    });
            }

            return handle;
        }

        protected abstract TValue GetValue(TObject target);
        protected abstract void SetValue(TObject target, in TValue value);
        protected abstract TValue GetRelativeValue(in TValue startValue, in TValue relativeValue);
    }

    // Unity で Generic Math が使えれば良いのだが…… (

    [Serializable]
    public abstract class FloatPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, float, NoOptions, FloatMotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override float GetRelativeValue(in float startValue, in float relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class DoublePropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, double, NoOptions, DoubleMotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override double GetRelativeValue(in double startValue, in double relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class IntPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, int, IntegerOptions, IntMotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override int GetRelativeValue(in int startValue, in int relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class LongPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, long, IntegerOptions, LongMotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override long GetRelativeValue(in long startValue, in long relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class Vector2PropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Vector2, NoOptions, Vector2MotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override Vector2 GetRelativeValue(in Vector2 startValue, in Vector2 relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class Vector3PropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Vector3, NoOptions, Vector3MotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override Vector3 GetRelativeValue(in Vector3 startValue, in Vector3 relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class Vector4PropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Vector4, NoOptions, Vector4MotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override Vector4 GetRelativeValue(in Vector4 startValue, in Vector4 relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class ColorPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Color, NoOptions, ColorMotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override Color GetRelativeValue(in Color startValue, in Color relativeValue)
        {
            return startValue + relativeValue;
        }
    }

    [Serializable]
    public abstract class RectPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Rect, NoOptions, RectMotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override Rect GetRelativeValue(in Rect startValue, in Rect relativeValue)
        {
            return new Rect(startValue.position + relativeValue.position, startValue.size + relativeValue.size);
        }
    }

    [Serializable]
    public abstract class FixedString512BytesPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, FixedString512Bytes, StringOptions, FixedString512BytesMotionAdapter>
        where TObject : UnityEngine.Object
    {
        protected sealed override FixedString512Bytes GetRelativeValue(in FixedString512Bytes startValue, in FixedString512Bytes relativeValue)
        {
            FixedString512Bytes value = startValue;
            value.Append(relativeValue);
            return value;
        }
    }
}

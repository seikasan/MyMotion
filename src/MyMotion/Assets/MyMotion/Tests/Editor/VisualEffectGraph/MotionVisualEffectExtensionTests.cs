using System;
using MyMotion.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed class MotionVisualEffectExtensionTests
    {
        [Test]
        public void StandardBuilderOverloadsRejectNullTarget()
        {
            MotionBuilder<float> floatBuilder = default;
            MotionBuilder<int> intBuilder = default;
            MotionBuilder<Vector2> vector2Builder = default;
            MotionBuilder<Vector3> vector3Builder = default;
            MotionBuilder<Vector4> vector4Builder = default;

            Assert.Throws<ArgumentNullException>(() => floatBuilder.BindToVisualEffectFloat(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => floatBuilder.BindToVisualEffectFloat(null, 1));
            Assert.Throws<ArgumentNullException>(() => intBuilder.BindToVisualEffectInt(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => intBuilder.BindToVisualEffectInt(null, 1));
            Assert.Throws<ArgumentNullException>(() => vector2Builder.BindToVisualEffectVector2(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => vector2Builder.BindToVisualEffectVector2(null, 1));
            Assert.Throws<ArgumentNullException>(() => vector3Builder.BindToVisualEffectVector3(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => vector3Builder.BindToVisualEffectVector3(null, 1));
            Assert.Throws<ArgumentNullException>(() => vector4Builder.BindToVisualEffectVector4(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => vector4Builder.BindToVisualEffectVector4(null, 1));
        }

        [Test]
        public void AdapterBuilderOverloadsRejectNullTarget()
        {
            MotionBuilder<float, NoMotionOptions, TestAdapter<float>> floatBuilder = default;
            MotionBuilder<int, NoMotionOptions, TestAdapter<int>> intBuilder = default;
            MotionBuilder<Vector2, NoMotionOptions, TestAdapter<Vector2>> vector2Builder = default;
            MotionBuilder<Vector3, NoMotionOptions, TestAdapter<Vector3>> vector3Builder = default;
            MotionBuilder<Vector4, NoMotionOptions, TestAdapter<Vector4>> vector4Builder = default;

            Assert.Throws<ArgumentNullException>(() => floatBuilder.BindToVisualEffectFloat(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => floatBuilder.BindToVisualEffectFloat(null, 1));
            Assert.Throws<ArgumentNullException>(() => intBuilder.BindToVisualEffectInt(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => intBuilder.BindToVisualEffectInt(null, 1));
            Assert.Throws<ArgumentNullException>(() => vector2Builder.BindToVisualEffectVector2(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => vector2Builder.BindToVisualEffectVector2(null, 1));
            Assert.Throws<ArgumentNullException>(() => vector3Builder.BindToVisualEffectVector3(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => vector3Builder.BindToVisualEffectVector3(null, 1));
            Assert.Throws<ArgumentNullException>(() => vector4Builder.BindToVisualEffectVector4(null, "_Value"));
            Assert.Throws<ArgumentNullException>(() => vector4Builder.BindToVisualEffectVector4(null, 1));
        }

        private readonly struct TestAdapter<TValue> : IMotionAdapter<TValue, NoMotionOptions>
            where TValue : unmanaged
        {
            public TValue Evaluate(
                ref TValue startValue,
                ref TValue endValue,
                ref NoMotionOptions options,
                in MotionEvaluationContext context) => startValue;
        }
    }
}

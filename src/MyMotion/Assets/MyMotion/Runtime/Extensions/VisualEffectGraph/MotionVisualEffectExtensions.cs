using System;
using UnityEngine;
using UnityEngine.VFX;

namespace MyMotion.Extensions
{
    /// <summary>
    /// Visual Effect Graph の公開プロパティ向け bind 拡張。
    /// </summary>
    public static class MotionVisualEffectExtensions
    {
        public static MotionHandle BindToVisualEffectFloat(
            this MotionBuilder<float> builder,
            VisualEffect visualEffect,
            string name)
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetFloat(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectFloat<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetFloat(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectFloat(
            this MotionBuilder<float> builder,
            VisualEffect visualEffect,
            int nameID)
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetFloat(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectFloat<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetFloat(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectInt(
            this MotionBuilder<int> builder,
            VisualEffect visualEffect,
            string name)
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetInt(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectInt<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetInt(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectInt(
            this MotionBuilder<int> builder,
            VisualEffect visualEffect,
            int nameID)
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetInt(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectInt<TOptions, TAdapter>(
            this MotionBuilder<int, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetInt(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectVector2(
            this MotionBuilder<Vector2> builder,
            VisualEffect visualEffect,
            string name)
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetVector2(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectVector2<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetVector2(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectVector2(
            this MotionBuilder<Vector2> builder,
            VisualEffect visualEffect,
            int nameID)
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetVector2(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectVector2<TOptions, TAdapter>(
            this MotionBuilder<Vector2, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetVector2(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectVector3(
            this MotionBuilder<Vector3> builder,
            VisualEffect visualEffect,
            string name)
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetVector3(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectVector3<TOptions, TAdapter>(
            this MotionBuilder<Vector3, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetVector3(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectVector3(
            this MotionBuilder<Vector3> builder,
            VisualEffect visualEffect,
            int nameID)
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetVector3(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectVector3<TOptions, TAdapter>(
            this MotionBuilder<Vector3, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetVector3(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectVector4(
            this MotionBuilder<Vector4> builder,
            VisualEffect visualEffect,
            string name)
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetVector4(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectVector4<TOptions, TAdapter>(
            this MotionBuilder<Vector4, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector4, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            return builder.Bind(visualEffect, name, static (value, target, propertyName) => target.SetVector4(propertyName, value));
        }

        public static MotionHandle BindToVisualEffectVector4(
            this MotionBuilder<Vector4> builder,
            VisualEffect visualEffect,
            int nameID)
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetVector4(state.PropertyId, value));
        }

        public static MotionHandle BindToVisualEffectVector4<TOptions, TAdapter>(
            this MotionBuilder<Vector4, TOptions, TAdapter> builder,
            VisualEffect visualEffect,
            int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector4, TOptions>
        {
            ValidateVisualEffect(visualEffect);
            var state = new VisualEffectPropertyId(visualEffect, nameID);
            return builder.Bind(state, static (value, state) => state.Target.SetVector4(state.PropertyId, value));
        }

        private static void ValidateVisualEffect(VisualEffect visualEffect)
        {
            if (visualEffect == null)
            {
                throw new ArgumentNullException(nameof(visualEffect));
            }
        }

        private readonly struct VisualEffectPropertyId
        {
            public readonly VisualEffect Target;
            public readonly int PropertyId;

            public VisualEffectPropertyId(VisualEffect target, int propertyId)
            {
                Target = target;
                PropertyId = propertyId;
            }
        }
    }
}

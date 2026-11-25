using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MyMotion.Extensions
{
    public static partial class MotionGeneralExtensions
    {
        public static MotionHandle BindToUnityLogger<TValue>(this MotionBuilder<TValue> builder) =>
            builder.Bind(static value => Debug.unityLogger.Log(value));

        public static MotionHandle BindToUnityLogger<TValue>(this MotionBuilder<TValue> builder, string format) =>
            builder.Bind(format, static (value, messageFormat) => Debug.unityLogger.Log(string.Format(messageFormat, value)));

        public static MotionHandle BindToUnityLogger<TValue>(this MotionBuilder<TValue> builder, ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            return builder.Bind(logger, static (value, target) => target.Log(value));
        }

        public static MotionHandle BindToUnityLogger<TValue>(this MotionBuilder<TValue> builder, ILogger logger, string format)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            return builder.Bind(logger, format, static (value, targetLogger, messageFormat) => targetLogger.Log(string.Format(messageFormat, value)));
        }

        public static MotionHandle BindToMaterialFloat(this MotionBuilder<float> builder, Material material, string name)
        {
            EnsureMaterial(material);
            if (name == null) return builder.Bind(material, static (value, target) => target.SetFloat(null, value));
            return builder.Bind(new MaterialPropertyTarget(material, Shader.PropertyToID(name)), static (value, target) => target.Material.SetFloat(target.NameID, value));
        }

        public static MotionHandle BindToMaterialFloat(this MotionBuilder<float> builder, Material material, int nameID)
        {
            EnsureMaterial(material);
            return builder.Bind(new MaterialPropertyTarget(material, nameID), static (value, target) => target.Material.SetFloat(target.NameID, value));
        }

        public static MotionHandle BindToMaterialInt(this MotionBuilder<int> builder, Material material, string name)
        {
            EnsureMaterial(material);
            if (name == null) return builder.Bind(material, static (value, target) => target.SetInteger(null, value));
            return builder.Bind(new MaterialPropertyTarget(material, Shader.PropertyToID(name)), static (value, target) => target.Material.SetInteger(target.NameID, value));
        }

        public static MotionHandle BindToMaterialInt(this MotionBuilder<int> builder, Material material, int nameID)
        {
            EnsureMaterial(material);
            return builder.Bind(new MaterialPropertyTarget(material, nameID), static (value, target) => target.Material.SetInteger(target.NameID, value));
        }

        public static MotionHandle BindToMaterialColor(this MotionBuilder<Color> builder, Material material, string name)
        {
            EnsureMaterial(material);
            if (name == null) return builder.Bind(material, static (value, target) => target.SetColor(null, value));
            return builder.Bind(new MaterialPropertyTarget(material, Shader.PropertyToID(name)), static (value, target) => target.Material.SetColor(target.NameID, value));
        }

        public static MotionHandle BindToMaterialColor(this MotionBuilder<Color> builder, Material material, int nameID)
        {
            EnsureMaterial(material);
            return builder.Bind(new MaterialPropertyTarget(material, nameID), static (value, target) => target.Material.SetColor(target.NameID, value));
        }

        public static MotionHandle BindToAudioMixerFloat(this MotionBuilder<float> builder, AudioMixer audioMixer, string name)
        {
            if (audioMixer == null) throw new ArgumentNullException(nameof(audioMixer));
            return builder.Bind(audioMixer, name, static (value, targetMixer, propertyName) => targetMixer.SetFloat(propertyName, value));
        }

        private static void EnsureMaterial(Material material)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));
        }

        private readonly struct MaterialPropertyTarget
        {
            public readonly Material Material;
            public readonly int NameID;

            public MaterialPropertyTarget(Material material, int nameID)
            {
                Material = material;
                NameID = nameID;
            }
        }

    }
}

using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MyMotion.Extensions
{
    // LitMotion と同じく、型付き builder (Punch / Shake / 独自 Adapter の MotionBuilder<TValue, TOptions, TAdapter>) も受け取る。
    public static partial class MotionGeneralExtensions
    {
        public static MotionHandle BindToUnityLogger<TValue, TOptions, TAdapter>(this MotionBuilder<TValue, TOptions, TAdapter> builder)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
            => builder.Bind(static value => Debug.unityLogger.Log(value));

        public static MotionHandle BindToUnityLogger<TValue, TOptions, TAdapter>(this MotionBuilder<TValue, TOptions, TAdapter> builder, string format)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
            => builder.Bind(format, static (value, messageFormat) => Debug.unityLogger.Log(string.Format(messageFormat, value)));

        public static MotionHandle BindToUnityLogger<TValue, TOptions, TAdapter>(this MotionBuilder<TValue, TOptions, TAdapter> builder, ILogger logger)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            return builder.Bind(logger, static (value, target) => target.Log(value));
        }

        public static MotionHandle BindToUnityLogger<TValue, TOptions, TAdapter>(this MotionBuilder<TValue, TOptions, TAdapter> builder, ILogger logger, string format)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            return builder.Bind(logger, format, static (value, targetLogger, messageFormat) => targetLogger.Log(string.Format(messageFormat, value)));
        }

        public static MotionHandle BindToMaterialFloat<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Material material, string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            EnsureMaterial(material);
            if (name == null) return builder.Bind(material, static (value, target) => target.SetFloat(null, value));
            return builder.Bind(new MaterialPropertyTarget(material, Shader.PropertyToID(name)), static (value, target) => target.Material.SetFloat(target.NameID, value));
        }

        public static MotionHandle BindToMaterialFloat<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Material material, int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            EnsureMaterial(material);
            return builder.Bind(new MaterialPropertyTarget(material, nameID), static (value, target) => target.Material.SetFloat(target.NameID, value));
        }

        public static MotionHandle BindToMaterialInt<TOptions, TAdapter>(this MotionBuilder<int, TOptions, TAdapter> builder, Material material, string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions>
        {
            EnsureMaterial(material);
            if (name == null) return builder.Bind(material, static (value, target) => target.SetInteger(null, value));
            return builder.Bind(new MaterialPropertyTarget(material, Shader.PropertyToID(name)), static (value, target) => target.Material.SetInteger(target.NameID, value));
        }

        public static MotionHandle BindToMaterialInt<TOptions, TAdapter>(this MotionBuilder<int, TOptions, TAdapter> builder, Material material, int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<int, TOptions>
        {
            EnsureMaterial(material);
            return builder.Bind(new MaterialPropertyTarget(material, nameID), static (value, target) => target.Material.SetInteger(target.NameID, value));
        }

        public static MotionHandle BindToMaterialColor<TOptions, TAdapter>(this MotionBuilder<Color, TOptions, TAdapter> builder, Material material, string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions>
        {
            EnsureMaterial(material);
            if (name == null) return builder.Bind(material, static (value, target) => target.SetColor(null, value));
            return builder.Bind(new MaterialPropertyTarget(material, Shader.PropertyToID(name)), static (value, target) => target.Material.SetColor(target.NameID, value));
        }

        public static MotionHandle BindToMaterialColor<TOptions, TAdapter>(this MotionBuilder<Color, TOptions, TAdapter> builder, Material material, int nameID)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions>
        {
            EnsureMaterial(material);
            return builder.Bind(new MaterialPropertyTarget(material, nameID), static (value, target) => target.Material.SetColor(target.NameID, value));
        }

        public static MotionHandle BindToAudioMixerFloat<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, AudioMixer audioMixer, string name)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (audioMixer == null) throw new ArgumentNullException(nameof(audioMixer));
            return builder.Bind(audioMixer, name, static (value, targetMixer, propertyName) => targetMixer.SetFloat(propertyName, value));
        }
    }
}

using System;
using UnityEngine.Rendering;

namespace MyMotion.Extensions
{
    /// <summary>
    /// Scriptable Render Pipeline の Volume コンポーネント向け bind 拡張。
    /// </summary>
    public static class MotionVolumeExtensions
    {
        public static MotionHandle BindToWeight(this MotionBuilder<float> builder, Volume volume)
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            return builder.Bind(volume, static (value, target) => target.weight = value);
        }

        public static MotionHandle BindToWeight<TOptions, TAdapter>(
            this MotionBuilder<float, TOptions, TAdapter> builder,
            Volume volume)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            return builder.Bind(volume, static (value, target) => target.weight = value);
        }
    }
}

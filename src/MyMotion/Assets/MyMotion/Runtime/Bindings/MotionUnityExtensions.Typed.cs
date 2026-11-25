using System;
using UnityEngine;

namespace MyMotion.Extensions
{
    // LitMotion と同じく、型付き builder (Punch / Shake / 独自 Adapter の MotionBuilder<TValue, TOptions, TAdapter>) も受け取る。
    public static partial class MotionUnityExtensions
    {
        public static MotionHandle BindToPosition<TOptions, TAdapter>(this MotionBuilder<Vector3, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector3, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => target.MovePosition(value))
                : builder.Bind(rigidbody, static (value, target) => target.position = value);
        }

        public static MotionHandle BindToPositionX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value; target.position = position; });
        }

        public static MotionHandle BindToPositionY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value; target.position = position; });
        }

        public static MotionHandle BindToPositionZ<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.z = value; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.z = value; target.position = position; });
        }

        public static MotionHandle BindToPositionXY<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.y = value.y; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.y = value.y; target.position = position; });
        }

        public static MotionHandle BindToPositionYZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value.x; position.z = value.y; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value.x; position.z = value.y; target.position = position; });
        }

        public static MotionHandle BindToPositionXZ<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.z = value.y; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.z = value.y; target.position = position; });
        }

        public static MotionHandle BindToRotation<TOptions, TAdapter>(this MotionBuilder<Quaternion, TOptions, TAdapter> builder, Rigidbody rigidbody, bool useMoveRotation = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Quaternion, TOptions>
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMoveRotation
                ? builder.Bind(rigidbody, static (value, target) => target.MoveRotation(value))
                : builder.Bind(rigidbody, static (value, target) => target.rotation = value);
        }

        public static MotionHandle BindToPosition<TOptions, TAdapter>(this MotionBuilder<Vector2, TOptions, TAdapter> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => target.MovePosition(value))
                : builder.Bind(rigidbody2D, static (value, target) => target.position = value);
        }

        public static MotionHandle BindToPositionX<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.x = value; target.MovePosition(position); })
                : builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.x = value; target.position = position; });
        }

        public static MotionHandle BindToPositionY<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.y = value; target.MovePosition(position); })
                : builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.y = value; target.position = position; });
        }

        public static MotionHandle BindToRotation<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => target.MoveRotation(value))
                : builder.Bind(rigidbody2D, static (value, target) => target.rotation = value);
        }

        public static MotionHandle BindToAspect<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, camera, static (value, target) => target.aspect = value, nameof(camera));

        public static MotionHandle BindToNearClipPlane<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, camera, static (value, target) => target.nearClipPlane = value, nameof(camera));

        public static MotionHandle BindToFarClipPlane<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, camera, static (value, target) => target.farClipPlane = value, nameof(camera));

        public static MotionHandle BindToFieldOfView<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, camera, static (value, target) => target.fieldOfView = value, nameof(camera));

        public static MotionHandle BindToOrthographicSize<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, camera, static (value, target) => target.orthographicSize = value, nameof(camera));

        public static MotionHandle BindToRect<TOptions, TAdapter>(this MotionBuilder<Rect, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Rect, TOptions>
            => Bind(builder, camera, static (value, target) => target.rect = value, nameof(camera));

        public static MotionHandle BindToPixelRect<TOptions, TAdapter>(this MotionBuilder<Rect, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Rect, TOptions>
            => Bind(builder, camera, static (value, target) => target.pixelRect = value, nameof(camera));

        public static MotionHandle BindToBackgroundColor<TOptions, TAdapter>(this MotionBuilder<Color, TOptions, TAdapter> builder, Camera camera)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions>
            => Bind(builder, camera, static (value, target) => target.backgroundColor = value, nameof(camera));

        public static MotionHandle BindToVolume<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, AudioSource audioSource)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, audioSource, static (value, target) => target.volume = value, nameof(audioSource));

        public static MotionHandle BindToPitch<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, AudioSource audioSource)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, audioSource, static (value, target) => target.pitch = value, nameof(audioSource));

        public static MotionHandle BindToColor<TOptions, TAdapter>(this MotionBuilder<Color, TOptions, TAdapter> builder, SpriteRenderer spriteRenderer)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<Color, TOptions>
            => Bind(builder, spriteRenderer, static (value, target) => target.color = value, nameof(spriteRenderer));

        public static MotionHandle BindToColorR<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, SpriteRenderer spriteRenderer)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.r = value; target.color = color; }, nameof(spriteRenderer));

        public static MotionHandle BindToColorG<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, SpriteRenderer spriteRenderer)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.g = value; target.color = color; }, nameof(spriteRenderer));

        public static MotionHandle BindToColorB<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, SpriteRenderer spriteRenderer)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.b = value; target.color = color; }, nameof(spriteRenderer));

        public static MotionHandle BindToColorA<TOptions, TAdapter>(this MotionBuilder<float, TOptions, TAdapter> builder, SpriteRenderer spriteRenderer)
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
            => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.a = value; target.color = color; }, nameof(spriteRenderer));

        public static MotionHandle BindToProgress<TValue, TOptions, TAdapter>(this MotionBuilder<TValue, TOptions, TAdapter> builder, IProgress<TValue> progress)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            return builder.Bind(progress, static (value, target) => target.Report(value));
        }

        private static MotionHandle Bind<TValue, TOptions, TAdapter, TTarget>(MotionBuilder<TValue, TOptions, TAdapter> builder, TTarget target, Action<TValue, TTarget> binding, string targetName)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
            where TTarget : UnityEngine.Object
        {
            if (target == null) throw new ArgumentNullException(targetName);
            return builder.Bind(target, binding);
        }
    }
}

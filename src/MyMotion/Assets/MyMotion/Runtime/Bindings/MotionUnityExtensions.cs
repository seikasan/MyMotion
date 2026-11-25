using System;
using UnityEngine;

namespace MyMotion.Extensions
{
    public static partial class MotionUnityExtensions
    {
        public static MotionHandle BindToPosition(this MotionBuilder<Vector3> builder, Rigidbody rigidbody, bool useMovePosition = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => target.MovePosition(value))
                : builder.Bind(rigidbody, static (value, target) => target.position = value);
        }

        public static MotionHandle BindToPositionX(this MotionBuilder<float> builder, Rigidbody rigidbody, bool useMovePosition = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value; target.position = position; });
        }

        public static MotionHandle BindToPositionY(this MotionBuilder<float> builder, Rigidbody rigidbody, bool useMovePosition = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value; target.position = position; });
        }

        public static MotionHandle BindToPositionZ(this MotionBuilder<float> builder, Rigidbody rigidbody, bool useMovePosition = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.z = value; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.z = value; target.position = position; });
        }

        public static MotionHandle BindToPositionXY(this MotionBuilder<Vector2> builder, Rigidbody rigidbody, bool useMovePosition = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.y = value.y; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.y = value.y; target.position = position; });
        }

        public static MotionHandle BindToPositionYZ(this MotionBuilder<Vector2> builder, Rigidbody rigidbody, bool useMovePosition = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value.x; position.z = value.y; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.y = value.x; position.z = value.y; target.position = position; });
        }

        public static MotionHandle BindToPositionXZ(this MotionBuilder<Vector2> builder, Rigidbody rigidbody, bool useMovePosition = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMovePosition
                ? builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.z = value.y; target.MovePosition(position); })
                : builder.Bind(rigidbody, static (value, target) => { Vector3 position = target.position; position.x = value.x; position.z = value.y; target.position = position; });
        }

        public static MotionHandle BindToRotation(this MotionBuilder<Quaternion> builder, Rigidbody rigidbody, bool useMoveRotation = true)
        {
            if (rigidbody == null) throw new ArgumentNullException(nameof(rigidbody));
            return useMoveRotation
                ? builder.Bind(rigidbody, static (value, target) => target.MoveRotation(value))
                : builder.Bind(rigidbody, static (value, target) => target.rotation = value);
        }

        public static MotionHandle BindToPosition(this MotionBuilder<Vector2> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => target.MovePosition(value))
                : builder.Bind(rigidbody2D, static (value, target) => target.position = value);
        }

        public static MotionHandle BindToPositionX(this MotionBuilder<float> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.x = value; target.MovePosition(position); })
                : builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.x = value; target.position = position; });
        }

        public static MotionHandle BindToPositionY(this MotionBuilder<float> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.y = value; target.MovePosition(position); })
                : builder.Bind(rigidbody2D, static (value, target) => { Vector2 position = target.position; position.y = value; target.position = position; });
        }

        public static MotionHandle BindToRotation(this MotionBuilder<float> builder, Rigidbody2D rigidbody2D, bool useMovePosition = true)
        {
            if (rigidbody2D == null) throw new ArgumentNullException(nameof(rigidbody2D));
            return useMovePosition
                ? builder.Bind(rigidbody2D, static (value, target) => target.MoveRotation(value))
                : builder.Bind(rigidbody2D, static (value, target) => target.rotation = value);
        }

        public static MotionHandle BindToAspect(this MotionBuilder<float> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.aspect = value, nameof(camera));
        public static MotionHandle BindToNearClipPlane(this MotionBuilder<float> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.nearClipPlane = value, nameof(camera));
        public static MotionHandle BindToFarClipPlane(this MotionBuilder<float> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.farClipPlane = value, nameof(camera));
        public static MotionHandle BindToFieldOfView(this MotionBuilder<float> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.fieldOfView = value, nameof(camera));
        public static MotionHandle BindToOrthographicSize(this MotionBuilder<float> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.orthographicSize = value, nameof(camera));
        public static MotionHandle BindToRect(this MotionBuilder<Rect> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.rect = value, nameof(camera));
        public static MotionHandle BindToPixelRect(this MotionBuilder<Rect> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.pixelRect = value, nameof(camera));
        public static MotionHandle BindToBackgroundColor(this MotionBuilder<Color> builder, Camera camera) => Bind(builder, camera, static (value, target) => target.backgroundColor = value, nameof(camera));

        public static MotionHandle BindToVolume(this MotionBuilder<float> builder, AudioSource audioSource) => Bind(builder, audioSource, static (value, target) => target.volume = value, nameof(audioSource));
        public static MotionHandle BindToPitch(this MotionBuilder<float> builder, AudioSource audioSource) => Bind(builder, audioSource, static (value, target) => target.pitch = value, nameof(audioSource));

        public static MotionHandle BindToColor(this MotionBuilder<Color> builder, SpriteRenderer spriteRenderer) => Bind(builder, spriteRenderer, static (value, target) => target.color = value, nameof(spriteRenderer));
        public static MotionHandle BindToColorR(this MotionBuilder<float> builder, SpriteRenderer spriteRenderer) => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.r = value; target.color = color; }, nameof(spriteRenderer));
        public static MotionHandle BindToColorG(this MotionBuilder<float> builder, SpriteRenderer spriteRenderer) => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.g = value; target.color = color; }, nameof(spriteRenderer));
        public static MotionHandle BindToColorB(this MotionBuilder<float> builder, SpriteRenderer spriteRenderer) => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.b = value; target.color = color; }, nameof(spriteRenderer));
        public static MotionHandle BindToColorA(this MotionBuilder<float> builder, SpriteRenderer spriteRenderer) => Bind(builder, spriteRenderer, static (value, target) => { Color color = target.color; color.a = value; target.color = color; }, nameof(spriteRenderer));

        public static MotionHandle BindToProgress<TValue>(this MotionBuilder<TValue> builder, IProgress<TValue> progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            return builder.Bind(progress, static (value, target) => target.Report(value));
        }

        private static MotionHandle Bind<TValue, TTarget>(MotionBuilder<TValue> builder, TTarget target, Action<TValue, TTarget> binding, string targetName)
            where TTarget : UnityEngine.Object
        {
            if (target == null) throw new ArgumentNullException(targetName);
            return builder.Bind(target, binding);
        }
    }
}

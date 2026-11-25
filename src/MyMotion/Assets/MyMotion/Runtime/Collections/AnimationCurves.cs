using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using MyMotion.Internal;

namespace MyMotion.Collections
{
    internal struct KeyframeTimeComparer : IComparer<Keyframe>
    {
        public int Compare(Keyframe x, Keyframe y) => x.time.CompareTo(y.time);
    }

    internal static unsafe class AnimationCurveCopy
    {
        // AnimationCurve.keys は呼ぶたびに managed 配列を確保するので一度だけ読む。
        public static void Copy(AnimationCurve source, ref UnsafeList<Keyframe> destination)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            int length = source.length;
            destination.Resize(length, NativeArrayOptions.UninitializedMemory);
            if (length == 0) return;
            Keyframe[] keys = source.keys;
            fixed (Keyframe* src = keys)
            {
                UnsafeUtility.MemCpy(destination.Ptr, src, length * sizeof(Keyframe));
            }
            destination.Sort(default(KeyframeTimeComparer));
        }

        public static float Evaluate(Keyframe* keys, int length, WrapMode pre, WrapMode post, float time)
        {
            var data = new MotionWorld.CustomEaseCurveData
            {
                Keys = keys,
                Length = length,
                PreWrapMode = pre,
                PostWrapMode = post,
                IsAssigned = true
            };
            return MotionWorld.CustomEaseCurveEvaluator.Evaluate(data, time);
        }
    }

    /// <summary>
    /// Burst から評価できる AnimationCurve のネイティブコピー（安全性チェック付き）。
    /// </summary>
    public unsafe struct NativeAnimationCurve : IDisposable
    {
        private NativeList<Keyframe> _keys;
        private WrapMode _preWrapMode;
        private WrapMode _postWrapMode;

        public NativeAnimationCurve(AllocatorManager.AllocatorHandle allocator)
        {
            _keys = new NativeList<Keyframe>(0, allocator);
            _preWrapMode = default;
            _postWrapMode = default;
        }

        public NativeAnimationCurve(AnimationCurve animationCurve, AllocatorManager.AllocatorHandle allocator)
        {
            _keys = new NativeList<Keyframe>(animationCurve?.length ?? 0, allocator);
            _preWrapMode = default;
            _postWrapMode = default;
            CopyFrom(animationCurve);
        }

        public void CopyFrom(AnimationCurve animationCurve)
        {
            AnimationCurveCopy.Copy(animationCurve, ref *_keys.GetUnsafeList());
            _preWrapMode = animationCurve.preWrapMode;
            _postWrapMode = animationCurve.postWrapMode;
        }

        public void CopyFrom(in NativeAnimationCurve animationCurve)
        {
            _keys.CopyFrom(animationCurve._keys);
            _preWrapMode = animationCurve._preWrapMode;
            _postWrapMode = animationCurve._postWrapMode;
        }

        public void Dispose() => _keys.Dispose();

        public readonly bool IsCreated => _keys.IsCreated;

        public float Evaluate(float time) =>
            AnimationCurveCopy.Evaluate((Keyframe*)_keys.GetUnsafeReadOnlyPtr(), _keys.Length, _preWrapMode, _postWrapMode, time);
    }

    /// <summary>
    /// 安全性チェックを持たない AnimationCurve のネイティブコピー。
    /// </summary>
    public unsafe struct UnsafeAnimationCurve : IDisposable
    {
        internal UnsafeList<Keyframe> _keys;
        internal WrapMode _preWrapMode;
        internal WrapMode _postWrapMode;

        public UnsafeAnimationCurve(AllocatorManager.AllocatorHandle allocator)
        {
            _keys = new UnsafeList<Keyframe>(0, allocator);
            _preWrapMode = default;
            _postWrapMode = default;
        }

        public UnsafeAnimationCurve(AnimationCurve animationCurve, AllocatorManager.AllocatorHandle allocator)
        {
            _keys = new UnsafeList<Keyframe>(animationCurve?.length ?? 0, allocator);
            _preWrapMode = default;
            _postWrapMode = default;
            CopyFrom(animationCurve);
        }

        public void CopyFrom(AnimationCurve animationCurve)
        {
            AnimationCurveCopy.Copy(animationCurve, ref _keys);
            _preWrapMode = animationCurve.preWrapMode;
            _postWrapMode = animationCurve.postWrapMode;
        }

        public void CopyFrom(in UnsafeAnimationCurve animationCurve)
        {
            _keys.CopyFrom(animationCurve._keys);
            _preWrapMode = animationCurve._preWrapMode;
            _postWrapMode = animationCurve._postWrapMode;
        }

        public void Dispose() => _keys.Dispose();

        public readonly bool IsCreated => _keys.IsCreated;

        public float Evaluate(float time) =>
            AnimationCurveCopy.Evaluate(_keys.Ptr, _keys.Length, _preWrapMode, _postWrapMode, time);
    }
}

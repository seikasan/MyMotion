using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        public unsafe bool SetCustomEaseCurve(CoreMotionHandle handle, AnimationCurve animationCurve)
        {
            if (animationCurve == null)
            {
                throw new ArgumentNullException(nameof(animationCurve));
            }
            if (!TryGetIndex(handle, out int index) || _items[index].Ease != MotionEase.CustomAnimationCurve)
            {
                return false;
            }

            bool isAdapter = _items[index].IsAdapter;
            bool needsEndValue = _items[index].LoopType == MotionLoopType.Incremental;
            Keyframe[] keys;
            float endValue = 0f;
            if (isAdapter)
            {
                _customEaseCurves ??= new AnimationCurve[_items.Length];
                keys = animationCurve.keys;
                var snapshot = new AnimationCurve(keys)
                {
                    preWrapMode = animationCurve.preWrapMode,
                    postWrapMode = animationCurve.postWrapMode
                };
                _customEaseCurves[index] = snapshot;
                if (needsEndValue) endValue = snapshot.Evaluate(1f);
            }
            else
            {
                keys = animationCurve.keys;
                WrapMode preWrapMode = animationCurve.preWrapMode;
                WrapMode postWrapMode = animationCurve.postWrapMode;

                EnsureNativeCustomEaseCurveCapacity(_items.Length);
                CustomEaseCurveData nativeCurve = _nativeCustomEaseCurves[index];
                bool wasAssigned = nativeCurve.IsAssigned;
                if (nativeCurve.Keys != null)
                {
                    UnsafeUtility.Free(nativeCurve.Keys, Allocator.Persistent);
                }

                nativeCurve = new CustomEaseCurveData
                {
                    Length = keys.Length,
                    PreWrapMode = preWrapMode,
                    PostWrapMode = postWrapMode,
                    IsAssigned = true
                };
                if (keys.Length > 0)
                {
                    nativeCurve.Keys = (Keyframe*)UnsafeUtility.Malloc(
                        UnsafeUtility.SizeOf<Keyframe>() * keys.Length,
                        UnsafeUtility.AlignOf<Keyframe>(),
                        Allocator.Persistent);
                    fixed (Keyframe* source = keys)
                    {
                        UnsafeUtility.MemCpy(nativeCurve.Keys, source, UnsafeUtility.SizeOf<Keyframe>() * keys.Length);
                    }
                }
                nativeCurve.EndValue = CustomEaseCurveEvaluator.Evaluate(in nativeCurve, 1f);
                _nativeCustomEaseCurves[index] = nativeCurve;
                _hasNativeCustomEaseCurves = true;
                if (!wasAssigned) _nativeCustomEaseCurveCount++;
            }
            if (isAdapter && needsEndValue)
            {
                _customEaseEndValues ??= new float[_items.Length];
                _customEaseEndValues[index] = endValue;
            }
            return true;
        }

        public bool SetOnComplete(CoreMotionHandle handle, Action callback)
        {
            if (!TryGetIndex(handle, out int index)) return false;
            Notifications notifications = _notifications[index];
            notifications.OnComplete = callback;
            _notifications[index] = notifications;
            return true;
        }

        public bool SetOnCancel(CoreMotionHandle handle, Action callback)
        {
            if (!TryGetIndex(handle, out int index)) return false;
            Notifications notifications = _notifications[index];
            notifications.OnCancel = callback;
            _notifications[index] = notifications;
            return true;
        }

        public bool SetOnLoopComplete(CoreMotionHandle handle, Action<int> callback)
        {
            if (!TryGetIndex(handle, out int index)) return false;
            Notifications notifications = _notifications[index];
            notifications.OnLoopComplete = callback;
            _notifications[index] = notifications;
            return true;
        }

        public bool AddContinuation(CoreMotionHandle handle, Action continuation)
        {
            if (continuation == null) return false;
            if (!TryGetIndex(handle, out int index))
            {
                continuation();
                return true;
            }
            Data item = _items[index];
            if (item.IsCompletionNotified)
            {
                continuation();
                return true;
            }
            Notifications notifications = _notifications[index];
            notifications.Continuation += continuation;
            _notifications[index] = notifications;
            return true;
        }

        public bool RemoveContinuation(CoreMotionHandle handle, Action continuation)
        {
            if (continuation == null || !TryGetIndexRegardlessOfCancellation(handle, out int index)) return false;
            Notifications notifications = _notifications[index];
            notifications.Continuation -= continuation;
            _notifications[index] = notifications;
            return true;
        }
    }
}

using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        // 配列の範囲は World 自身が管理しており、hot path の null / 範囲検査は生成コードから外す。
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        private void Bind(int index, in Data item)
        {
            if (item.IsAdapter)
            {
                AdapterRoute route = _adapterRoutes[item.Slot];
                MotionEvaluationContext context = new() { Progress = item.Value.x, Time = item.Elapsed };
                MotionAdapterStorageBase storage = _adapterStorages[route.StorageIndex];
                if (item.Ease == MotionEase.CustomAnimationCurve)
                {
                    storage.EvaluateAt(route.Index, in context);
                }
                int stateIndex = item.IsValueStateBinding ? _slots[item.Slot].ValueStateIndex : -1;
                storage.Bind(
                    route.Index,
                    in context,
                    _callbacks[index],
                    _states[index],
                    _states1?[index],
                    _states2?[index],
                    item.StateCount,
                    stateIndex,
                    item.IsValueStateBinding);
                return;
            }
            if (item.ValueType == MotionValueType.Float)
            {
                InvokeManagedBinding(index, item, item.Value.x);
                return;
            }
            if (item.ValueType == MotionValueType.Vector3)
            {
                var value = new Vector3(item.Value.x, item.Value.y, item.Value.z);
                // Transform 拡張の delegate なら、Update の記録経路と同じく setter を直接呼ぶ (Bind 時の初期値の書き込みもここを通る)。
                if (item.StateCount == 1 && !item.IsValueStateBinding)
                {
                    object callback = _callbacks[index];
                    object state = _states[index];
                    // これらの delegate は Transform の state でしか登録されないので、型検査なしで読み替える。
                    if (callback == (object)MyMotion.Extensions.TransformBindings.Position) { UnsafeUtility.As<object, Transform>(ref state).position = value; return; }
                    if (callback == (object)MyMotion.Extensions.TransformBindings.LocalPosition) { UnsafeUtility.As<object, Transform>(ref state).localPosition = value; return; }
                    if (callback == (object)MyMotion.Extensions.TransformBindings.LocalScale) { UnsafeUtility.As<object, Transform>(ref state).localScale = value; return; }
                }
                InvokeManagedBinding(index, item, value);
                return;
            }

            MotionValue wideValue = _wideItems[item.WideIndex].Value;
            switch (item.ValueType)
            {
                case MotionValueType.Double:
                    InvokeManagedBinding(index, item, wideValue.Double);
                    break;
                case MotionValueType.Int:
                    InvokeManagedBinding(index, item, (int)wideValue.Integer);
                    break;
                case MotionValueType.Long:
                    InvokeManagedBinding(index, item, wideValue.Integer);
                    break;
                case MotionValueType.Vector2:
                    var vector2 = new Vector2(wideValue.Floats.x, wideValue.Floats.y);
                    InvokeManagedBinding(index, item, vector2);
                    break;
                case MotionValueType.Vector4:
                    var vector4 = new Vector4(wideValue.Floats.x, wideValue.Floats.y, wideValue.Floats.z, wideValue.Floats.w);
                    InvokeManagedBinding(index, item, vector4);
                    break;
                case MotionValueType.Quaternion:
                    var quaternion = new Quaternion(wideValue.Floats.x, wideValue.Floats.y, wideValue.Floats.z, wideValue.Floats.w);
                    InvokeManagedBinding(index, item, quaternion);
                    break;
                case MotionValueType.Color:
                    var color = new Color(wideValue.Floats.x, wideValue.Floats.y, wideValue.Floats.z, wideValue.Floats.w);
                    InvokeManagedBinding(index, item, color);
                    break;
                case MotionValueType.Rect:
                    var rect = new Rect(wideValue.Floats.x, wideValue.Floats.y, wideValue.Floats.z, wideValue.Floats.w);
                    InvokeManagedBinding(index, item, rect);
                    break;
            }
        }

        // 配列の範囲は World 自身が管理しており、hot path の null / 範囲検査は生成コードから外す。
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        private void InvokeManagedBinding<TValue>(int index, in Data item, TValue value)
        {
            switch (item.StateCount)
            {
                case 0:
                    InvokeBinding(_callbacks[index], value);
                    break;
                case 1:
                    if (item.IsValueStateBinding)
                    {
                        ((ValueStateBindingStorageBase<TValue>)_states[index]).Invoke(value, _slots[item.Slot].ValueStateIndex);
                    }
                    else
                    {
                        InvokeStateful(_callbacks[index], value, _states[index]);
                    }
                    break;
                case 2:
                    InvokeStateful(_callbacks[index], value, _states[index], _states1[index]);
                    break;
                case 3:
                    InvokeStateful(_callbacks[index], value, _states[index], _states1[index], _states2[index]);
                    break;
            }
        }

        private static void InvokeBinding<TValue>(object callback, TValue value)
        {
            Action<TValue> action = UnsafeUtility.As<object, Action<TValue>>(ref callback);
            action.Invoke(value);
        }

        private static void InvokeStateful<TValue>(object callback, TValue value, object state)
        {
            Action<TValue, object> action = UnsafeUtility.As<object, Action<TValue, object>>(ref callback);
            action.Invoke(value, state);
        }

        private static void InvokeStateful<TValue>(object callback, TValue value, object state0, object state1)
        {
            Action<TValue, object, object> action = UnsafeUtility.As<object, Action<TValue, object, object>>(ref callback);
            action.Invoke(value, state0, state1);
        }

        private static void InvokeStateful<TValue>(object callback, TValue value, object state0, object state1, object state2)
        {
            Action<TValue, object, object, object> action = UnsafeUtility.As<object, Action<TValue, object, object, object>>(ref callback);
            action.Invoke(value, state0, state1, state2);
        }
    }
}

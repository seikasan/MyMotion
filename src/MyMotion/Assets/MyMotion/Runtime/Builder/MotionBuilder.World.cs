using System;
using MyMotion.Internal;
using Unity.Collections.LowLevel.Unsafe;

namespace MyMotion
{
    public partial struct MotionBuilder<TValue> : IDisposable
    {
        // float と Vector3 (組み込み factory) は、factory の delegate と中継関数を通さず World の生成を直接呼ぶ。
        // typeof(TValue) の比較は IL2CPP が値型ごとの実体で定数に畳むので、ほかの型では分岐が消える。
        private readonly CoreMotionHandle CreateWithState(MotionWorld world, in MotionBuilderOptions o, byte stateCount, object state0, object state1, object state2, object callback)
        {
            if (_factory == null)
            {
                TValue start = _start;
                TValue end = _end;
                if (typeof(TValue) == typeof(float))
                {
                    return world.CreateWithStates(UnsafeUtility.As<TValue, float>(ref start), UnsafeUtility.As<TValue, float>(ref end), o.Duration, (MotionEase)o.Ease, stateCount, state0, state1, state2, callback, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
                }
                if (typeof(TValue) == typeof(UnityEngine.Vector3))
                {
                    return world.CreateWithStates(UnsafeUtility.As<TValue, UnityEngine.Vector3>(ref start), UnsafeUtility.As<TValue, UnityEngine.Vector3>(ref end), o.Duration, (MotionEase)o.Ease, stateCount, state0, state1, state2, callback, o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
                }
            }
            return Factory.CreateWithState(world, _start, _end, o, stateCount, state0, state1, state2, callback);
        }

        private readonly CoreMotionHandle CreateDirect(MotionWorld world, in MotionBuilderOptions o, Action<TValue> binding)
        {
            if (_factory == null)
            {
                TValue start = _start;
                TValue end = _end;
                object untyped = binding;
                if (typeof(TValue) == typeof(float))
                {
                    return world.Create(UnsafeUtility.As<TValue, float>(ref start), UnsafeUtility.As<TValue, float>(ref end), o.Duration, (MotionEase)o.Ease, UnsafeUtility.As<object, Action<float>>(ref untyped), o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
                }
                if (typeof(TValue) == typeof(UnityEngine.Vector3))
                {
                    return world.Create(UnsafeUtility.As<TValue, UnityEngine.Vector3>(ref start), UnsafeUtility.As<TValue, UnityEngine.Vector3>(ref end), o.Duration, (MotionEase)o.Ease, UnsafeUtility.As<object, Action<UnityEngine.Vector3>>(ref untyped), o.Delay, o.SkipValuesDuringDelay, o.Loops, (MotionLoopType)o.LoopType, o.DelayEveryLoop, o.PlaybackSpeed, (Internal.MotionTimeKind)o.TimeKind, o.CancelOnError);
                }
            }
            return Factory.Create(world, _start, _end, o, binding);
        }

        internal readonly MotionHandle BindToWorld(MotionWorld world, Action<TValue> binding)
        {
            ref readonly MotionBuilderOptions options = ref OptionsOf(in this);
            CoreMotionHandle coreHandle = CreateDirect(world, in options, binding);
            ConfigureCustomEase(world, coreHandle, in options);
            if (options.OnComplete != null || options.OnCancel != null || options.OnLoopComplete != null)
            {
                world.SetCallbacks(coreHandle, options.OnComplete, options.OnCancel, options.OnLoopComplete);
            }
            if (options.ImmediateBind)
            {
                world.ApplyImmediateBind(coreHandle);
            }
            return new MotionHandle(world, coreHandle);
        }

        internal readonly MotionHandle BindToWorld<TState>(MotionWorld world, TState state, Action<TValue, TState> binding)
        {
            ref readonly MotionBuilderOptions options = ref OptionsOf(in this);
            object callbackState;
            object callback;
            ValueStateBindingStorage<TValue, TState> valueStateStorage = null;
            if (typeof(TState).IsValueType)
            {
                valueStateStorage = world.GetOrCreateValueStateBindingStorage<TValue, TState>();
                callbackState = valueStateStorage;
                callback = valueStateStorage;
            }
            else
            {
                callbackState = state;
                callback = binding;
            }
            CoreMotionHandle coreHandle = CreateWithState(world, in options, 1, callbackState, null, null, callback);
            if (valueStateStorage != null)
            {
                world.SetValueStateBinding(coreHandle, valueStateStorage, state, binding);
            }
            ConfigureCustomEase(world, coreHandle, in options);
            if (options.OnComplete != null || options.OnCancel != null || options.OnLoopComplete != null)
            {
                world.SetCallbacks(coreHandle, options.OnComplete, options.OnCancel, options.OnLoopComplete);
            }
            if (options.ImmediateBind)
            {
                world.ApplyImmediateBind(coreHandle);
            }
            return new MotionHandle(world, coreHandle);
        }

        internal readonly MotionHandle BindToWorld<TState0, TState1>(MotionWorld world, TState0 state0, TState1 state1, Action<TValue, TState0, TState1> binding)
            where TState0 : class
            where TState1 : class
        {
            ref readonly MotionBuilderOptions options = ref OptionsOf(in this);
            object untypedCallback = binding;
            Action<TValue, object, object> callback = UnsafeUtility.As<object, Action<TValue, object, object>>(ref untypedCallback);
            CoreMotionHandle coreHandle = CreateWithState(world, in options, 2, state0, state1, null, callback);
            ConfigureCustomEase(world, coreHandle, in options);
            if (options.OnComplete != null || options.OnCancel != null || options.OnLoopComplete != null)
            {
                world.SetCallbacks(coreHandle, options.OnComplete, options.OnCancel, options.OnLoopComplete);
            }
            if (options.ImmediateBind)
            {
                world.ApplyImmediateBind(coreHandle);
            }
            return new MotionHandle(world, coreHandle);
        }

        internal readonly MotionHandle BindToWorld<TState0, TState1, TState2>(MotionWorld world, TState0 state0, TState1 state1, TState2 state2, Action<TValue, TState0, TState1, TState2> binding)
            where TState0 : class
            where TState1 : class
            where TState2 : class
        {
            ref readonly MotionBuilderOptions options = ref OptionsOf(in this);
            object untypedCallback = binding;
            Action<TValue, object, object, object> callback = UnsafeUtility.As<object, Action<TValue, object, object, object>>(ref untypedCallback);
            CoreMotionHandle coreHandle = CreateWithState(world, in options, 3, state0, state1, state2, callback);
            ConfigureCustomEase(world, coreHandle, in options);
            if (options.OnComplete != null || options.OnCancel != null || options.OnLoopComplete != null)
            {
                world.SetCallbacks(coreHandle, options.OnComplete, options.OnCancel, options.OnLoopComplete);
            }
            if (options.ImmediateBind)
            {
                world.ApplyImmediateBind(coreHandle);
            }
            return new MotionHandle(world, coreHandle);
        }

        internal readonly MotionHandle RunWithoutBindingInWorld(MotionWorld world)
        {
            ref readonly MotionBuilderOptions options = ref OptionsOf(in this);
            CoreMotionHandle coreHandle = Factory.Create(world, _start, _end, options, null);
            ConfigureDebugName(world, coreHandle, in options);
            if (options.OnComplete != null || options.OnCancel != null || options.OnLoopComplete != null)
            {
                world.SetCallbacks(coreHandle, options.OnComplete, options.OnCancel, options.OnLoopComplete);
            }
            return new MotionHandle(world, coreHandle);
        }
    }
}

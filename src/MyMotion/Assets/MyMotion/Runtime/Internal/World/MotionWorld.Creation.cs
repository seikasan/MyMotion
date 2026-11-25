using System;
using Unity.Mathematics;
using UnityEngine;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        public CoreMotionHandle CreateAdapter<TValue, TOptions, TAdapter>(
            in TValue start,
            in TValue end,
            in TOptions adapterOptions,
            MotionBuilderOptions options,
            object binding,
            object state0,
            object state1,
            object state2,
            byte stateCount)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            CompletePipeline();
            CoreMotionHandle handle = CreateCore(
                FloatValue(0f, 0f, 0f, 0f),
                FloatValue(1f, 0f, 0f, 0f),
                MotionValueType.Float,
                MotionRoundingMode.ToEven,
                options.Duration,
                (MotionEase)options.Ease,
                binding,
                state0,
                stateCount > 0,
                options.Delay,
                options.SkipValuesDuringDelay,
                options.Loops,
                (MotionLoopType)options.LoopType,
                options.DelayEveryLoop,
                options.PlaybackSpeed,
                (MotionTimeKind)options.TimeKind,
                options.CancelOnError,
                state1,
                state2,
                stateCount);

            int denseIndex = _slots[handle.Slot].DenseIndex;
            try
            {
                MotionAdapterStorage<TValue, TOptions, TAdapter> storage = GetOrCreateAdapterStorage<TValue, TOptions, TAdapter>();
                _adapterRoutes ??= new AdapterRoute[_slots.Length];
                int adapterIndex = storage.Add(handle.Slot, denseIndex, in start, in end, in adapterOptions);
                _adapterRoutes[handle.Slot] = new AdapterRoute { StorageIndex = storage.StorageIndex, Index = adapterIndex };
                Data item = _items[denseIndex];
                item.IsAdapter = true;
                _items[denseIndex] = item;
                _adapterMotionCount++;
            }
            catch
            {
                RemoveAt(denseIndex);
                throw;
            }

            return handle;
        }

        public CoreMotionHandle Create(float start, float end, float duration, MotionEase ease, Action<float> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start, 0f, 0f, 0f), FloatValue(end, 0f, 0f, 0f), MotionValueType.Float, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Vector3 start, Vector3 end, float duration, MotionEase ease, Action<Vector3> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, 0f), FloatValue(end.x, end.y, end.z, 0f), MotionValueType.Vector3, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(float start, float end, float duration, MotionEase ease, object state, Action<float, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start, 0f, 0f, 0f), FloatValue(end, 0f, 0f, 0f), MotionValueType.Float, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Vector3 start, Vector3 end, float duration, MotionEase ease, object state, Action<Vector3, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, 0f), FloatValue(end.x, end.y, end.z, 0f), MotionValueType.Vector3, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(double start, double end, float duration, MotionEase ease, Action<double> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(DoubleValue(start), DoubleValue(end), MotionValueType.Double, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(double start, double end, float duration, MotionEase ease, object state, Action<double, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(DoubleValue(start), DoubleValue(end), MotionValueType.Double, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(int start, int end, float duration, MotionEase ease, Action<int> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false, MotionRoundingMode roundingMode = MotionRoundingMode.ToEven)
        {
            return CreateCore(IntegerValue(start), IntegerValue(end), MotionValueType.Int, roundingMode, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(int start, int end, float duration, MotionEase ease, object state, Action<int, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false, MotionRoundingMode roundingMode = MotionRoundingMode.ToEven)
        {
            return CreateCore(IntegerValue(start), IntegerValue(end), MotionValueType.Int, roundingMode, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(long start, long end, float duration, MotionEase ease, Action<long> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false, MotionRoundingMode roundingMode = MotionRoundingMode.ToEven)
        {
            return CreateCore(IntegerValue(start), IntegerValue(end), MotionValueType.Long, roundingMode, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(long start, long end, float duration, MotionEase ease, object state, Action<long, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false, MotionRoundingMode roundingMode = MotionRoundingMode.ToEven)
        {
            return CreateCore(IntegerValue(start), IntegerValue(end), MotionValueType.Long, roundingMode, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Vector2 start, Vector2 end, float duration, MotionEase ease, Action<Vector2> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, 0f, 0f), FloatValue(end.x, end.y, 0f, 0f), MotionValueType.Vector2, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Vector2 start, Vector2 end, float duration, MotionEase ease, object state, Action<Vector2, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, 0f, 0f), FloatValue(end.x, end.y, 0f, 0f), MotionValueType.Vector2, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Vector4 start, Vector4 end, float duration, MotionEase ease, Action<Vector4> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, start.w), FloatValue(end.x, end.y, end.z, end.w), MotionValueType.Vector4, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Vector4 start, Vector4 end, float duration, MotionEase ease, object state, Action<Vector4, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, start.w), FloatValue(end.x, end.y, end.z, end.w), MotionValueType.Vector4, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Quaternion start, Quaternion end, float duration, MotionEase ease, Action<Quaternion> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, start.w), FloatValue(end.x, end.y, end.z, end.w), MotionValueType.Quaternion, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Quaternion start, Quaternion end, float duration, MotionEase ease, object state, Action<Quaternion, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, start.w), FloatValue(end.x, end.y, end.z, end.w), MotionValueType.Quaternion, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Color start, Color end, float duration, MotionEase ease, Action<Color> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.r, start.g, start.b, start.a), FloatValue(end.r, end.g, end.b, end.a), MotionValueType.Color, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Color start, Color end, float duration, MotionEase ease, object state, Action<Color, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.r, start.g, start.b, start.a), FloatValue(end.r, end.g, end.b, end.a), MotionValueType.Color, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Rect start, Rect end, float duration, MotionEase ease, Action<Rect> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.width, start.height), FloatValue(end.x, end.y, end.width, end.height), MotionValueType.Rect, MotionRoundingMode.ToEven, duration, ease, binding, null, false, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle Create(Rect start, Rect end, float duration, MotionEase ease, object state, Action<Rect, object> binding, float delay = 0f, bool skipValuesDuringDelay = true, int loops = 1, MotionLoopType loopType = MotionLoopType.Restart, bool delayEveryLoop = false, float playbackSpeed = 1f, MotionTimeKind timeKind = MotionTimeKind.Time, bool cancelOnError = false)
        {
            return CreateCore(FloatValue(start.x, start.y, start.width, start.height), FloatValue(end.x, end.y, end.width, end.height), MotionValueType.Rect, MotionRoundingMode.ToEven, duration, ease, binding, state, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError);
        }

        public CoreMotionHandle CreateWithStates(float start, float end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(FloatValue(start, 0f, 0f, 0f), FloatValue(end, 0f, 0f, 0f), MotionValueType.Float, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(double start, double end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(DoubleValue(start), DoubleValue(end), MotionValueType.Double, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(int start, int end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError, MotionRoundingMode roundingMode)
        {
            return CreateCore(IntegerValue(start), IntegerValue(end), MotionValueType.Int, roundingMode, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(long start, long end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError, MotionRoundingMode roundingMode)
        {
            return CreateCore(IntegerValue(start), IntegerValue(end), MotionValueType.Long, roundingMode, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(Vector2 start, Vector2 end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(FloatValue(start.x, start.y, 0f, 0f), FloatValue(end.x, end.y, 0f, 0f), MotionValueType.Vector2, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(Vector3 start, Vector3 end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, 0f), FloatValue(end.x, end.y, end.z, 0f), MotionValueType.Vector3, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(Vector4 start, Vector4 end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, start.w), FloatValue(end.x, end.y, end.z, end.w), MotionValueType.Vector4, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(Quaternion start, Quaternion end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(FloatValue(start.x, start.y, start.z, start.w), FloatValue(end.x, end.y, end.z, end.w), MotionValueType.Quaternion, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(Color start, Color end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(FloatValue(start.r, start.g, start.b, start.a), FloatValue(end.r, end.g, end.b, end.a), MotionValueType.Color, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        public CoreMotionHandle CreateWithStates(Rect start, Rect end, float duration, MotionEase ease, byte stateCount, object state0, object state1, object state2, object binding, float delay, bool skipValuesDuringDelay, int loops, MotionLoopType loopType, bool delayEveryLoop, float playbackSpeed, MotionTimeKind timeKind, bool cancelOnError)
        {
            return CreateCore(FloatValue(start.x, start.y, start.width, start.height), FloatValue(end.x, end.y, end.width, end.height), MotionValueType.Rect, MotionRoundingMode.ToEven, duration, ease, binding, state0, true, delay, skipValuesDuringDelay, loops, loopType, delayEveryLoop, playbackSpeed, timeKind, cancelOnError, state1, state2, stateCount);
        }

        private static MotionValue FloatValue(float x, float y, float z, float w)
        {
            return new MotionValue { Floats = new float4(x, y, z, w) };
        }

        private static MotionValue DoubleValue(double value)
        {
            return new MotionValue { Double = value };
        }

        private static MotionValue IntegerValue(long value)
        {
            return new MotionValue { Integer = value };
        }
    }
}

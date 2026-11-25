using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MyMotion
{
    /// <summary>
    /// 値型を custom motion adapter のオプションとして示す。
    /// </summary>
    public interface IMotionOptions
    {
    }

    /// <summary>
    /// custom motion 型の値を計算する。
    /// </summary>
    public interface IMotionAdapter<TValue, TOptions>
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
    {
        TValue Evaluate(ref TValue startValue, ref TValue endValue, ref TOptions options, in MotionEvaluationContext context);
    }

    /// <summary>
    /// custom adapter が motion を評価するときに使える値。
    /// </summary>
    public struct MotionEvaluationContext
    {
        /// <summary>
        /// ループを考慮した、Ease 適用後の進行度。
        /// </summary>
        public float Progress;

        /// <summary>
        /// motion の経過時間 (秒)。
        /// </summary>
        public double Time;
    }

    /// <summary>
    /// 追加パラメータを必要としない adapter 用のオプション。
    /// </summary>
    public readonly struct NoMotionOptions : IMotionOptions
    {
    }

    /// <summary>
    /// unmanaged 値の custom adapter 用 builder。
    /// </summary>
    public readonly struct MotionBuilder<TValue, TOptions, TAdapter> : IDisposable
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        private readonly TValue _start;
        private readonly TValue _end;
        private readonly TOptions _options;
        private readonly MotionBuilder<float> _driver;

        internal MotionBuilder(TValue start, TValue end, float duration)
        {
            _start = start;
            _end = end;
            _options = default;
            _driver = MMotion.Create(0f, 1f, duration);
        }

        private MotionBuilder(TValue start, TValue end, TOptions options, MotionBuilder<float> driver)
        {
            _start = start;
            _end = end;
            _options = options;
            _driver = driver;
        }

        internal TOptions Options => _options;

        // 組み込み Adapter (FloatMotionAdapter など) のときは、組み込み値型の高速経路に乗せ替える。
        private MotionBuilder<TValue> ToBuiltIn()
        {
            IntegerRoundingMode? rounding = null;
            if (typeof(TOptions) == typeof(IntegerOptions))
            {
                TOptions options = _options;
                rounding = (IntegerRoundingMode)UnsafeUtility.As<TOptions, IntegerOptions>(ref options).RoundingMode;
            }
            return _driver.Retarget(_start, _end, BuiltInMotionAdapter<TValue, TAdapter>.Factory, rounding);
        }

        /// <summary>
        /// LitMotion 形式の型付き builder から、MyMotion の組み込み値型 builder へ変換する。
        /// </summary>
        public static implicit operator MotionBuilder<TValue>(MotionBuilder<TValue, TOptions, TAdapter> builder)
        {
            if (!BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn)
            {
                throw new InvalidCastException(typeof(TAdapter).Name + " is not a built-in motion adapter.");
            }
            return builder.ToBuiltIn();
        }

        /// <summary>
        /// MMotion.Create の結果を、LitMotion と同じ型付き builder として受け取れるようにする。
        /// </summary>
        public static implicit operator MotionBuilder<TValue, TOptions, TAdapter>(MotionBuilder<TValue> builder)
        {
            if (!BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn)
            {
                throw new InvalidCastException(typeof(TAdapter).Name + " is not a built-in motion adapter.");
            }
            TOptions options = default;
            if (typeof(TOptions) == typeof(IntegerOptions))
            {
                var integerOptions = new IntegerOptions { RoundingMode = (RoundingMode)builder.RoundingMode };
                options = UnsafeUtility.As<IntegerOptions, TOptions>(ref integerOptions);
            }
            return new MotionBuilder<TValue, TOptions, TAdapter>(
                builder.StartValue,
                builder.EndValue,
                options,
                builder.Retarget(0f, 1f, MotionFactories.Float));
        }

        /// <summary>
        /// adapter 固有のオプションを設定する。
        /// </summary>
        public MotionBuilder<TValue, TOptions, TAdapter> WithOptions(TOptions options) =>
            new(_start, _end, options, _driver);

        public MotionBuilder<TValue, TOptions, TAdapter> WithEase(Ease ease) =>
            new(_start, _end, _options, _driver.WithEase(ease));

        public MotionBuilder<TValue, TOptions, TAdapter> WithEase(AnimationCurve animationCurve) =>
            new(_start, _end, _options, _driver.WithEase(animationCurve));

        public MotionBuilder<TValue, TOptions, TAdapter> WithScheduler(IMotionScheduler scheduler) =>
            new(_start, _end, _options, _driver.WithScheduler(scheduler));

        public MotionBuilder<TValue, TOptions, TAdapter> WithDelay(
            float delay,
            DelayType delayType = DelayType.FirstLoop,
            bool skipValuesDuringDelay = true) =>
            new(_start, _end, _options, _driver.WithDelay(delay, delayType, skipValuesDuringDelay));

        public MotionBuilder<TValue, TOptions, TAdapter> WithLoops(int loops, LoopType loopType = LoopType.Restart) =>
            new(_start, _end, _options, _driver.WithLoops(loops, loopType));

        public MotionBuilder<TValue, TOptions, TAdapter> WithPlaybackSpeed(float playbackSpeed) =>
            new(_start, _end, _options, _driver.WithPlaybackSpeed(playbackSpeed));

        public MotionBuilder<TValue, TOptions, TAdapter> WithTimeKind(MotionTimeKind timeKind) =>
            new(_start, _end, _options, _driver.WithTimeKind(timeKind));

        public MotionBuilder<TValue, TOptions, TAdapter> WithCancelOnError(bool cancelOnError = true) =>
            new(_start, _end, _options, _driver.WithCancelOnError(cancelOnError));

        public MotionBuilder<TValue, TOptions, TAdapter> WithImmediateBind(bool immediateBind = true) =>
            new(_start, _end, _options, _driver.WithImmediateBind(immediateBind));

        public MotionBuilder<TValue, TOptions, TAdapter> WithDebugName(string debugName) =>
            new(_start, _end, _options, _driver.WithDebugName(debugName));

        public MotionBuilder<TValue, TOptions, TAdapter> WithOnComplete(Action callback) =>
            new(_start, _end, _options, _driver.WithOnComplete(callback));

        public MotionBuilder<TValue, TOptions, TAdapter> WithOnCancel(Action callback) =>
            new(_start, _end, _options, _driver.WithOnCancel(callback));

        public MotionBuilder<TValue, TOptions, TAdapter> WithOnLoopComplete(Action<int> callback) =>
            new(_start, _end, _options, _driver.WithOnLoopComplete(callback));

        public MotionHandle Bind(Action<TValue> binding)
        {
            if (BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn) return ToBuiltIn().Bind(binding);
            if (_driver.TryScheduleAdapter<TValue, TOptions, TAdapter>(
                in _start, in _end, in _options, binding, null, null, null, 0, out MotionHandle handle))
            {
                return handle;
            }
            var state = new MotionAdapterBinding<TValue, TOptions, TAdapter>(_start, _end, _options, binding);
            return BindDriver(_driver, state);
        }

        public MotionHandle Bind<TState>(TState state, Action<TValue, TState> binding)
        {
            if (BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn) return ToBuiltIn().Bind(state, binding);
            if (typeof(TState).IsValueType)
            {
                if (_driver.TryScheduleAdapterWithValueState<TValue, TOptions, TAdapter, TState>(
                    in _start, in _end, in _options, state, binding, out MotionHandle valueStateHandle))
                {
                    return valueStateHandle;
                }
            }
            else
            {
                object callbackState = state;
                object untypedCallback = binding;
                Action<TValue, object> callback = UnsafeUtility.As<object, Action<TValue, object>>(ref untypedCallback);
                if (_driver.TryScheduleAdapter<TValue, TOptions, TAdapter>(
                    in _start, in _end, in _options, callback, callbackState, null, null, 1, out MotionHandle handle))
                {
                    return handle;
                }
            }
            var adapterBinding = new MotionAdapterBinding<TValue, TOptions, TAdapter, TState>(
                _start, _end, _options, state, binding);
            return BindDriver(_driver, adapterBinding);
        }

        public MotionHandle Bind<TState0, TState1>(
            TState0 state0,
            TState1 state1,
            Action<TValue, TState0, TState1> binding)
            where TState0 : class
            where TState1 : class
        {
            if (BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn) return ToBuiltIn().Bind(state0, state1, binding);
            object untypedCallback = binding;
            Action<TValue, object, object> callback = UnsafeUtility.As<object, Action<TValue, object, object>>(ref untypedCallback);
            if (_driver.TryScheduleAdapter<TValue, TOptions, TAdapter>(
                in _start, in _end, in _options, callback, state0, state1, null, 2, out MotionHandle handle))
            {
                return handle;
            }
            var adapterBinding = new MotionAdapterBinding<TValue, TOptions, TAdapter, TState0, TState1>(
                _start, _end, _options, state0, state1, binding);
            return BindDriver(_driver, adapterBinding);
        }

        public MotionHandle Bind<TState0, TState1, TState2>(
            TState0 state0,
            TState1 state1,
            TState2 state2,
            Action<TValue, TState0, TState1, TState2> binding)
            where TState0 : class
            where TState1 : class
            where TState2 : class
        {
            if (BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn) return ToBuiltIn().Bind(state0, state1, state2, binding);
            object untypedCallback = binding;
            Action<TValue, object, object, object> callback = UnsafeUtility.As<object, Action<TValue, object, object, object>>(ref untypedCallback);
            if (_driver.TryScheduleAdapter<TValue, TOptions, TAdapter>(
                in _start, in _end, in _options, callback, state0, state1, state2, 3, out MotionHandle handle))
            {
                return handle;
            }
            var adapterBinding = new MotionAdapterBinding<TValue, TOptions, TAdapter, TState0, TState1, TState2>(
                _start, _end, _options, state0, state1, state2, binding);
            return BindDriver(_driver, adapterBinding);
        }

        /// <summary>
        /// 評価した値を binding callback へ渡さずに motion を実行する。
        /// </summary>
        public MotionHandle RunWithoutBinding() =>
            BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn ? ToBuiltIn().RunWithoutBinding() : _driver.RunWithoutBinding();

        /// <summary>
        /// この builder に設定された値から、再利用できる設定を作る。
        /// </summary>
        public MotionSettings<TValue, TOptions> ToMotionSettings()
        {
            _driver.GetSettings(
                out _,
                out _,
                out MotionBuilderOptions options,
                out IMotionScheduler scheduler);
            return MotionBuilderSettingsExtensions.FromBuilder(_start, _end, _options, options, scheduler);
        }

        public void Dispose()
        {
        }

        private static MotionHandle BindDriver<TBinding>(MotionBuilder<float> driver, TBinding binding)
            where TBinding : MotionAdapterBindingBase<TValue, TOptions, TAdapter>
        {
            MotionHandle handle = driver.Bind(
                binding,
                static (progress, state) => state.EvaluateAndBind(progress));
            binding.SetHandle(handle);
            return handle;
        }
    }
}

using System;

namespace MyMotion
{
    internal abstract class MotionAdapterBindingBase<TValue, TOptions, TAdapter>
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        private TValue _start;
        private TValue _end;
        private TOptions _options;
        private MotionHandle _handle;
        private bool _hasHandle;

        protected MotionAdapterBindingBase(TValue start, TValue end, TOptions options)
        {
            _start = start;
            _end = end;
            _options = options;
        }

        public void SetHandle(MotionHandle handle)
        {
            _handle = handle;
            _hasHandle = true;
        }

        public void EvaluateAndBind(float progress)
        {
            MotionEvaluationContext context = new()
            {
                Progress = progress,
                Time = _hasHandle ? _handle.Time : 0d
            };
            TValue value = default(TAdapter).Evaluate(ref _start, ref _end, ref _options, in context);
            Bind(value);
        }

        protected abstract void Bind(TValue value);
    }

    internal sealed class MotionAdapterBinding<TValue, TOptions, TAdapter>
        : MotionAdapterBindingBase<TValue, TOptions, TAdapter>
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        private readonly Action<TValue> _binding;

        public MotionAdapterBinding(TValue start, TValue end, TOptions options, Action<TValue> binding)
            : base(start, end, options)
        {
            _binding = binding;
        }

        protected override void Bind(TValue value) => _binding(value);
    }

    internal sealed class MotionAdapterBinding<TValue, TOptions, TAdapter, TState>
        : MotionAdapterBindingBase<TValue, TOptions, TAdapter>
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        private readonly TState _state;
        private readonly Action<TValue, TState> _binding;

        public MotionAdapterBinding(
            TValue start, TValue end, TOptions options, TState state, Action<TValue, TState> binding)
            : base(start, end, options)
        {
            _state = state;
            _binding = binding;
        }

        protected override void Bind(TValue value) => _binding(value, _state);
    }

    internal sealed class MotionAdapterBinding<TValue, TOptions, TAdapter, TState0, TState1>
        : MotionAdapterBindingBase<TValue, TOptions, TAdapter>
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        private readonly TState0 _state0;
        private readonly TState1 _state1;
        private readonly Action<TValue, TState0, TState1> _binding;

        public MotionAdapterBinding(
            TValue start, TValue end, TOptions options,
            TState0 state0, TState1 state1,
            Action<TValue, TState0, TState1> binding)
            : base(start, end, options)
        {
            _state0 = state0;
            _state1 = state1;
            _binding = binding;
        }

        protected override void Bind(TValue value) => _binding(value, _state0, _state1);
    }

    internal sealed class MotionAdapterBinding<TValue, TOptions, TAdapter, TState0, TState1, TState2>
        : MotionAdapterBindingBase<TValue, TOptions, TAdapter>
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        private readonly TState0 _state0;
        private readonly TState1 _state1;
        private readonly TState2 _state2;
        private readonly Action<TValue, TState0, TState1, TState2> _binding;

        public MotionAdapterBinding(
            TValue start, TValue end, TOptions options,
            TState0 state0, TState1 state1, TState2 state2,
            Action<TValue, TState0, TState1, TState2> binding)
            : base(start, end, options)
        {
            _state0 = state0;
            _state1 = state1;
            _state2 = state2;
            _binding = binding;
        }

        protected override void Bind(TValue value) => _binding(value, _state0, _state1, _state2);
    }
}

using System;

namespace MyMotion
{
    internal interface IAdapterMotionScheduler
    {
        MotionHandle ScheduleAdapter<TBuilderValue, TValue, TOptions, TAdapter>(
            MotionBuilder<TBuilderValue> builder,
            in TValue start,
            in TValue end,
            in TOptions adapterOptions,
            object binding,
            object state0,
            object state1,
            object state2,
            byte stateCount)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>;

        MotionHandle ScheduleAdapterWithValueState<TBuilderValue, TValue, TOptions, TAdapter, TState>(
            MotionBuilder<TBuilderValue> builder,
            in TValue start,
            in TValue end,
            in TOptions adapterOptions,
            TState state,
            Action<TValue, TState> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>;
    }

    /// <summary>
    /// motion を、更新を担当する world へ振り分ける。
    /// </summary>
    public interface IMotionScheduler
    {
        MotionHandle Schedule<TValue>(MotionBuilder<TValue> builder, Action<TValue> binding);

        MotionHandle Schedule<TValue, TState>(MotionBuilder<TValue> builder, TState state, Action<TValue, TState> binding);

        MotionHandle Schedule<TValue, TState0, TState1>(MotionBuilder<TValue> builder, TState0 state0, TState1 state1, Action<TValue, TState0, TState1> binding)
            where TState0 : class
            where TState1 : class;

        MotionHandle Schedule<TValue, TState0, TState1, TState2>(MotionBuilder<TValue> builder, TState0 state0, TState1 state1, TState2 state2, Action<TValue, TState0, TState1, TState2> binding)
            where TState0 : class
            where TState1 : class
            where TState2 : class;

        MotionHandle RunWithoutBinding<TValue>(MotionBuilder<TValue> builder);
    }
}

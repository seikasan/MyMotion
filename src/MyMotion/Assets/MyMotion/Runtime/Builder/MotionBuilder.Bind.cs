using System;
using MyMotion.Internal;

namespace MyMotion
{
    public partial struct MotionBuilder<TValue> : IDisposable
    {
        // IMotionScheduler.Schedule<...> は interface の generic method で、IL2CPP では呼び出しごとに実体を探す
        // (generic virtual method) ため 1 回あたり数百 ns かかる。組み込みの scheduler は sealed 型なので、
        // 型判定だけで World を直接解決し、その呼び出しを省く。自作の scheduler は従来どおり interface を通す。
        // 組み込み scheduler の World を返す。自作の scheduler なら null を返す。
        // 時間の種類が builder と違うときだけ retime を立て、呼び出し側で WithTimeKind する (同じなら builder を複製しない)。
        private readonly MotionWorld ResolveBuiltInWorld(IMotionScheduler scheduler, out bool retime, out MotionTimeKind timeKind)
        {
            if (scheduler is PlayerLoopMotionScheduler playerLoop)
            {
                timeKind = playerLoop.TimeKind;
                retime = TimeKind != timeKind;
                return MotionScheduler.GetWorld(playerLoop.Timing);
            }
            retime = false;
            timeKind = default;
            if (scheduler is ManualMotionScheduler manual) return manual.World;
            return null;
        }

        private readonly bool TryResolveBuiltInWorld(IMotionScheduler scheduler, out MotionWorld world, out MotionBuilder<TValue> builder)
        {
            world = ResolveBuiltInWorld(scheduler, out bool retime, out MotionTimeKind timeKind);
            builder = retime ? WithTimeKind(timeKind) : this;
            return world != null;
        }

        public readonly MotionHandle Bind(Action<TValue> binding)
        {
            IMotionScheduler scheduler = _sharedConfiguration?.Scheduler ?? _scheduler ?? MotionScheduler.DefaultScheduler;
            MotionWorld world = ResolveBuiltInWorld(scheduler, out bool retime, out MotionTimeKind timeKind);
            if (world != null) return Track(retime ? WithTimeKind(timeKind).BindToWorld(world, binding) : BindToWorld(world, binding), scheduler);
            return Track(scheduler.Schedule(this, binding), scheduler);
        }

        public readonly MotionHandle Bind<TState>(TState state, Action<TValue, TState> binding)
        {
            IMotionScheduler scheduler = _sharedConfiguration?.Scheduler ?? _scheduler ?? MotionScheduler.DefaultScheduler;
            MotionWorld world = ResolveBuiltInWorld(scheduler, out bool retime, out MotionTimeKind timeKind);
            if (world != null) return Track(retime ? WithTimeKind(timeKind).BindToWorld(world, state, binding) : BindToWorld(world, state, binding), scheduler);
            return Track(scheduler.Schedule(this, state, binding), scheduler);
        }

        public readonly MotionHandle Bind<TState0, TState1>(TState0 state0, TState1 state1, Action<TValue, TState0, TState1> binding)
            where TState0 : class
            where TState1 : class
        {
            IMotionScheduler scheduler = _sharedConfiguration?.Scheduler ?? _scheduler ?? MotionScheduler.DefaultScheduler;
            if (TryResolveBuiltInWorld(scheduler, out MotionWorld world, out MotionBuilder<TValue> builder)) return Track(builder.BindToWorld(world, state0, state1, binding), scheduler);
            return Track(scheduler.Schedule(this, state0, state1, binding), scheduler);
        }

        public readonly MotionHandle Bind<TState0, TState1, TState2>(TState0 state0, TState1 state1, TState2 state2, Action<TValue, TState0, TState1, TState2> binding)
            where TState0 : class
            where TState1 : class
            where TState2 : class
        {
            IMotionScheduler scheduler = _sharedConfiguration?.Scheduler ?? _scheduler ?? MotionScheduler.DefaultScheduler;
            if (TryResolveBuiltInWorld(scheduler, out MotionWorld world, out MotionBuilder<TValue> builder)) return Track(builder.BindToWorld(world, state0, state1, state2, binding), scheduler);
            return Track(scheduler.Schedule(this, state0, state1, state2, binding), scheduler);
        }

        public readonly MotionHandle RunWithoutBinding()
        {
            IMotionScheduler scheduler = _sharedConfiguration?.Scheduler ?? _scheduler ?? MotionScheduler.DefaultScheduler;
            if (TryResolveBuiltInWorld(scheduler, out MotionWorld world, out MotionBuilder<TValue> builder)) return Track(builder.RunWithoutBindingInWorld(world), scheduler);
            return Track(scheduler.RunWithoutBinding(this), scheduler);
        }

        internal readonly bool TryScheduleAdapter<TAdapterValue, TOptions, TAdapter>(
            in TAdapterValue start,
            in TAdapterValue end,
            in TOptions adapterOptions,
            object binding,
            object state0,
            object state1,
            object state2,
            byte stateCount,
            out MotionHandle handle)
            where TAdapterValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TAdapterValue, TOptions>
        {
            IMotionScheduler scheduler = _sharedConfiguration?.Scheduler ?? _scheduler ?? MotionScheduler.DefaultScheduler;
            if (TryResolveBuiltInWorld(scheduler, out MotionWorld world, out MotionBuilder<TValue> builder))
            {
                handle = builder.BindAdapterToWorld<TAdapterValue, TOptions, TAdapter>(
                    world, in start, in end, in adapterOptions, binding, state0, state1, state2, stateCount);
                return true;
            }
            if (scheduler is IAdapterMotionScheduler adapterScheduler)
            {
                handle = adapterScheduler.ScheduleAdapter<TValue, TAdapterValue, TOptions, TAdapter>(
                    this, in start, in end, in adapterOptions, binding, state0, state1, state2, stateCount);
                return true;
            }

            handle = default;
            return false;
        }

        internal readonly bool TryScheduleAdapterWithValueState<TAdapterValue, TOptions, TAdapter, TState>(
            in TAdapterValue start,
            in TAdapterValue end,
            in TOptions adapterOptions,
            TState state,
            Action<TAdapterValue, TState> binding,
            out MotionHandle handle)
            where TAdapterValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TAdapterValue, TOptions>
        {
            IMotionScheduler scheduler = _sharedConfiguration?.Scheduler ?? _scheduler ?? MotionScheduler.DefaultScheduler;
            if (TryResolveBuiltInWorld(scheduler, out MotionWorld world, out MotionBuilder<TValue> builder))
            {
                handle = builder.BindAdapterToWorldWithValueState<TAdapterValue, TOptions, TAdapter, TState>(
                    world, in start, in end, in adapterOptions, state, binding);
                return true;
            }
            if (scheduler is IAdapterMotionScheduler adapterScheduler)
            {
                handle = adapterScheduler.ScheduleAdapterWithValueState<TValue, TAdapterValue, TOptions, TAdapter, TState>(
                    this, in start, in end, in adapterOptions, state, binding);
                return true;
            }

            handle = default;
            return false;
        }

        internal readonly MotionHandle BindAdapterToWorld<TAdapterValue, TOptions, TAdapter>(
            MotionWorld world,
            in TAdapterValue start,
            in TAdapterValue end,
            in TOptions adapterOptions,
            object binding,
            object state0,
            object state1,
            object state2,
            byte stateCount)
            where TAdapterValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TAdapterValue, TOptions>
        {
            ref readonly MotionBuilderOptions options = ref OptionsOf(in this);
            CoreMotionHandle coreHandle = world.CreateAdapter<TAdapterValue, TOptions, TAdapter>(
                in start, in end, in adapterOptions, options, binding, state0, state1, state2, stateCount);
            ConfigureCustomEase(world, coreHandle, options);
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

        internal readonly MotionHandle BindAdapterToWorldWithValueState<TAdapterValue, TOptions, TAdapter, TState>(
            MotionWorld world,
            in TAdapterValue start,
            in TAdapterValue end,
            in TOptions adapterOptions,
            TState state,
            Action<TAdapterValue, TState> binding)
            where TAdapterValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TAdapterValue, TOptions>
        {
            ref readonly MotionBuilderOptions options = ref OptionsOf(in this);
            ValueStateBindingStorage<TAdapterValue, TState> valueStateStorage =
                world.GetOrCreateValueStateBindingStorage<TAdapterValue, TState>();
            CoreMotionHandle coreHandle = world.CreateAdapter<TAdapterValue, TOptions, TAdapter>(
                in start, in end, in adapterOptions, options, valueStateStorage, valueStateStorage, null, null, 1);
            world.SetValueStateBinding(coreHandle, valueStateStorage, state, binding);
            ConfigureCustomEase(world, coreHandle, options);
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
    }
}

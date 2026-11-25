using System;
using MyMotion.Internal;
using UnityEngine;
using UnityEngine.LowLevel;
using PlayerLoopTypes = UnityEngine.PlayerLoop;

namespace MyMotion
{
    /// <summary>
    /// Runtime で使える scheduler。
    /// </summary>
    public static class MotionScheduler
    {
        private static readonly MotionWorld[] s_worlds = new MotionWorld[8];
        private static IMotionScheduler s_defaultScheduler;

        static MotionScheduler()
        {
            s_defaultScheduler = Update;
        }

        /// <summary>
        /// builder が scheduler を明示しなかったときに使う scheduler。
        /// </summary>
        public static IMotionScheduler DefaultScheduler
        {
            get => s_defaultScheduler;
            set => s_defaultScheduler = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static readonly IMotionScheduler Initialization = new PlayerLoopMotionScheduler(PlayerLoopTiming.Initialization, MotionTimeKind.Time);
        public static readonly IMotionScheduler InitializationIgnoreTimeScale = new PlayerLoopMotionScheduler(PlayerLoopTiming.Initialization, MotionTimeKind.UnscaledTime);
        public static readonly IMotionScheduler InitializationRealtime = new PlayerLoopMotionScheduler(PlayerLoopTiming.Initialization, MotionTimeKind.Realtime);
        public static readonly IMotionScheduler EarlyUpdate = new PlayerLoopMotionScheduler(PlayerLoopTiming.EarlyUpdate, MotionTimeKind.Time);
        public static readonly IMotionScheduler EarlyUpdateIgnoreTimeScale = new PlayerLoopMotionScheduler(PlayerLoopTiming.EarlyUpdate, MotionTimeKind.UnscaledTime);
        public static readonly IMotionScheduler EarlyUpdateRealtime = new PlayerLoopMotionScheduler(PlayerLoopTiming.EarlyUpdate, MotionTimeKind.Realtime);
        public static readonly IMotionScheduler FixedUpdate = new PlayerLoopMotionScheduler(PlayerLoopTiming.FixedUpdate, MotionTimeKind.Time);
        public static readonly IMotionScheduler PreUpdate = new PlayerLoopMotionScheduler(PlayerLoopTiming.PreUpdate, MotionTimeKind.Time);
        public static readonly IMotionScheduler PreUpdateIgnoreTimeScale = new PlayerLoopMotionScheduler(PlayerLoopTiming.PreUpdate, MotionTimeKind.UnscaledTime);
        public static readonly IMotionScheduler PreUpdateRealtime = new PlayerLoopMotionScheduler(PlayerLoopTiming.PreUpdate, MotionTimeKind.Realtime);
        public static readonly IMotionScheduler Update = new PlayerLoopMotionScheduler(PlayerLoopTiming.Update, MotionTimeKind.Time);
        public static readonly IMotionScheduler UpdateIgnoreTimeScale = new PlayerLoopMotionScheduler(PlayerLoopTiming.Update, MotionTimeKind.UnscaledTime);
        public static readonly IMotionScheduler UpdateRealtime = new PlayerLoopMotionScheduler(PlayerLoopTiming.Update, MotionTimeKind.Realtime);
        public static readonly IMotionScheduler PreLateUpdate = new PlayerLoopMotionScheduler(PlayerLoopTiming.PreLateUpdate, MotionTimeKind.Time);
        public static readonly IMotionScheduler PreLateUpdateIgnoreTimeScale = new PlayerLoopMotionScheduler(PlayerLoopTiming.PreLateUpdate, MotionTimeKind.UnscaledTime);
        public static readonly IMotionScheduler PreLateUpdateRealtime = new PlayerLoopMotionScheduler(PlayerLoopTiming.PreLateUpdate, MotionTimeKind.Realtime);
        public static readonly IMotionScheduler PostLateUpdate = new PlayerLoopMotionScheduler(PlayerLoopTiming.PostLateUpdate, MotionTimeKind.Time);
        public static readonly IMotionScheduler PostLateUpdateIgnoreTimeScale = new PlayerLoopMotionScheduler(PlayerLoopTiming.PostLateUpdate, MotionTimeKind.UnscaledTime);
        public static readonly IMotionScheduler PostLateUpdateRealtime = new PlayerLoopMotionScheduler(PlayerLoopTiming.PostLateUpdate, MotionTimeKind.Realtime);
        public static readonly IMotionScheduler TimeUpdate = new PlayerLoopMotionScheduler(PlayerLoopTiming.TimeUpdate, MotionTimeKind.Time);
        public static readonly IMotionScheduler TimeUpdateIgnoreTimeScale = new PlayerLoopMotionScheduler(PlayerLoopTiming.TimeUpdate, MotionTimeKind.UnscaledTime);
        public static readonly IMotionScheduler TimeUpdateRealtime = new PlayerLoopMotionScheduler(PlayerLoopTiming.TimeUpdate, MotionTimeKind.Realtime);

        /// <summary>
        /// <see cref="ManualMotionDispatcher.Default"/> を通じて進める scheduler。
        /// </summary>
        public static ManualMotionScheduler Manual => ManualMotionDispatcher.Default.Scheduler;

        internal static MotionWorld GetWorld(PlayerLoopTiming timing)
        {
            int index = (int)timing;
            MotionWorld world = s_worlds[index];
            if (world != null)
            {
                if (world.Count == 0) PlayerLoopMotionScheduler.ResetClock(timing);
                return world;
            }

            PlayerLoopMotionScheduler.ResetClock(timing);
            world = new MotionWorld();
            s_worlds[index] = world;
            return world;
        }

        internal static bool HasWorld(PlayerLoopTiming timing)
        {
            MotionWorld world = s_worlds[(int)timing];
            return world != null && world.Count > 0;
        }

        internal static int GetActiveCount()
        {
            int count = 0;
            for (int i = 0; i < s_worlds.Length; i++)
            {
                if (s_worlds[i] != null) count += s_worlds[i].Count;
            }
            return count;
        }

        internal static long GetReservedNativeBytes()
        {
            long bytes = 0L;
            for (int i = 0; i < s_worlds.Length; i++)
            {
                if (s_worlds[i] != null) bytes += s_worlds[i].ReservedNativeBytes;
            }
            return bytes;
        }

        internal static void UpdateWorld(PlayerLoopTiming timing, double deltaTime, double unscaledDeltaTime, double realtimeDeltaTime)
        {
            s_worlds[(int)timing]?.Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime);
        }

        internal static void UpdateDefaultWorld(double deltaTime, double unscaledDeltaTime, double realtimeDeltaTime)
        {
            MotionWorld world = s_worlds[(int)PlayerLoopTiming.Update];
            if (world != null)
            {
                world.Update(deltaTime, unscaledDeltaTime, realtimeDeltaTime);
                return;
            }
            if (!IsFinite(deltaTime) || !IsFinite(unscaledDeltaTime) || !IsFinite(realtimeDeltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
        }

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        // LitMotionのMotionDispatcher.Clearに相当する。Worldは残し、登録済みmotionだけを外す。
        internal static void ClearAll()
        {
            for (int i = 0; i < s_worlds.Length; i++)
            {
                MotionWorld world = s_worlds[i];
                if (world == null) continue;
                world.Reset();
                PlayerLoopMotionScheduler.ResetClock((PlayerLoopTiming)i);
            }
        }

        internal static void EnsureCapacity(int capacity)
        {
            for (int i = 0; i < s_worlds.Length; i++) s_worlds[i]?.EnsureCapacity(capacity);
            GetWorld(PlayerLoopTiming.Update).EnsureCapacity(capacity);
        }

        internal static void EnsureAdapterCapacity<TValue, TOptions, TAdapter>(int capacity)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            for (int i = 0; i < s_worlds.Length; i++) s_worlds[i]?.EnsureAdapterStorageCapacity<TValue, TOptions, TAdapter>(capacity);
            GetWorld(PlayerLoopTiming.Update).EnsureAdapterStorageCapacity<TValue, TOptions, TAdapter>(capacity);
        }

        internal static void Reset()
        {
            for (int i = 0; i < s_worlds.Length; i++)
            {
                s_worlds[i]?.Dispose();
                s_worlds[i] = null;
            }
            s_defaultScheduler = Update;
            PlayerLoopMotionScheduler.ResetClock();
        }

        internal static void InstallPlayerLoop()
        {
            PlayerLoopMotionScheduler.Install();
        }
    }

    internal sealed class PlayerLoopMotionScheduler : IMotionScheduler, IAdapterMotionScheduler
    {
        private readonly PlayerLoopTiming _timing;
        private readonly MotionTimeKind _timeKind;

        internal PlayerLoopTiming Timing => _timing;
        internal MotionTimeKind TimeKind => _timeKind;

        internal PlayerLoopMotionScheduler(PlayerLoopTiming timing, MotionTimeKind timeKind)
        {
            _timing = timing;
            _timeKind = timeKind;
        }

        public MotionHandle Schedule<TValue>(MotionBuilder<TValue> builder, Action<TValue> binding)
        {
            if (builder.TimeKind == _timeKind) return builder.BindToWorld(MotionScheduler.GetWorld(_timing), binding);
            return builder.WithTimeKind(_timeKind).BindToWorld(MotionScheduler.GetWorld(_timing), binding);
        }

        public MotionHandle Schedule<TValue, TState>(MotionBuilder<TValue> builder, TState state, Action<TValue, TState> binding)
        {
            if (builder.TimeKind == _timeKind) return builder.BindToWorld(MotionScheduler.GetWorld(_timing), state, binding);
            return builder.WithTimeKind(_timeKind).BindToWorld(MotionScheduler.GetWorld(_timing), state, binding);
        }

        public MotionHandle Schedule<TValue, TState0, TState1>(MotionBuilder<TValue> builder, TState0 state0, TState1 state1, Action<TValue, TState0, TState1> binding)
            where TState0 : class
            where TState1 : class
        {
            if (builder.TimeKind == _timeKind) return builder.BindToWorld(MotionScheduler.GetWorld(_timing), state0, state1, binding);
            return builder.WithTimeKind(_timeKind).BindToWorld(MotionScheduler.GetWorld(_timing), state0, state1, binding);
        }

        public MotionHandle Schedule<TValue, TState0, TState1, TState2>(MotionBuilder<TValue> builder, TState0 state0, TState1 state1, TState2 state2, Action<TValue, TState0, TState1, TState2> binding)
            where TState0 : class
            where TState1 : class
            where TState2 : class
        {
            if (builder.TimeKind == _timeKind) return builder.BindToWorld(MotionScheduler.GetWorld(_timing), state0, state1, state2, binding);
            return builder.WithTimeKind(_timeKind).BindToWorld(MotionScheduler.GetWorld(_timing), state0, state1, state2, binding);
        }

        public MotionHandle RunWithoutBinding<TValue>(MotionBuilder<TValue> builder)
        {
            if (builder.TimeKind == _timeKind) return builder.RunWithoutBindingInWorld(MotionScheduler.GetWorld(_timing));
            return builder.WithTimeKind(_timeKind).RunWithoutBindingInWorld(MotionScheduler.GetWorld(_timing));
        }

        public MotionHandle ScheduleAdapter<TBuilderValue, TValue, TOptions, TAdapter>(
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
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (builder.TimeKind == _timeKind)
            {
                return builder.BindAdapterToWorld<TValue, TOptions, TAdapter>(
                    MotionScheduler.GetWorld(_timing), in start, in end, in adapterOptions, binding, state0, state1, state2, stateCount);
            }
            return builder.WithTimeKind(_timeKind).BindAdapterToWorld<TValue, TOptions, TAdapter>(
                MotionScheduler.GetWorld(_timing), in start, in end, in adapterOptions, binding, state0, state1, state2, stateCount);
        }

        public MotionHandle ScheduleAdapterWithValueState<TBuilderValue, TValue, TOptions, TAdapter, TState>(
            MotionBuilder<TBuilderValue> builder,
            in TValue start,
            in TValue end,
            in TOptions adapterOptions,
            TState state,
            Action<TValue, TState> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (builder.TimeKind == _timeKind)
            {
                return builder.BindAdapterToWorldWithValueState<TValue, TOptions, TAdapter, TState>(
                    MotionScheduler.GetWorld(_timing), in start, in end, in adapterOptions, state, binding);
            }
            return builder.WithTimeKind(_timeKind).BindAdapterToWorldWithValueState<TValue, TOptions, TAdapter, TState>(
                MotionScheduler.GetWorld(_timing), in start, in end, in adapterOptions, state, binding);
        }

        internal static void Install()
        {
            ResetClock();
            PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
            if (!Insert(ref loop, typeof(PlayerLoopTypes.Initialization), typeof(MotionLoopRunners.Initialization), UpdateInitialization)
                || !Insert(ref loop, typeof(PlayerLoopTypes.EarlyUpdate), typeof(MotionLoopRunners.EarlyUpdate), UpdateEarlyUpdate)
                || !Insert(ref loop, typeof(PlayerLoopTypes.FixedUpdate), typeof(MotionLoopRunners.FixedUpdate), UpdateFixedUpdate)
                || !Insert(ref loop, typeof(PlayerLoopTypes.PreUpdate), typeof(MotionLoopRunners.PreUpdate), UpdatePreUpdate)
                || !Insert(ref loop, typeof(PlayerLoopTypes.Update), typeof(MotionLoopRunners.Update), UpdateUpdate)
                || !Insert(ref loop, typeof(PlayerLoopTypes.PreLateUpdate), typeof(MotionLoopRunners.PreLateUpdate), UpdatePreLateUpdate)
                || !Insert(ref loop, typeof(PlayerLoopTypes.PostLateUpdate), typeof(MotionLoopRunners.PostLateUpdate), UpdatePostLateUpdate)
                || !Insert(ref loop, typeof(PlayerLoopTypes.TimeUpdate), typeof(MotionLoopRunners.TimeUpdate), UpdateTimeUpdate))
            {
                throw new InvalidOperationException("A required Unity PlayerLoop phase was not found.");
            }
            PlayerLoop.SetPlayerLoop(loop);
        }

        private static readonly double[] s_previousRealtime = new double[8];
        private static readonly bool[] s_hasPreviousRealtime = new bool[8];

        internal static void ResetClock()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            for (int i = 0; i < s_previousRealtime.Length; i++)
            {
                s_previousRealtime[i] = now;
                s_hasPreviousRealtime[i] = true;
            }
        }

        internal static void ResetClock(PlayerLoopTiming timing)
        {
            int index = (int)timing;
            s_previousRealtime[index] = Time.realtimeSinceStartupAsDouble;
            s_hasPreviousRealtime[index] = true;
        }

        private static void UpdateInitialization() => UpdateWorld(PlayerLoopTiming.Initialization);
        private static void UpdateEarlyUpdate() => UpdateWorld(PlayerLoopTiming.EarlyUpdate);
        private static void UpdateFixedUpdate() => UpdateWorld(PlayerLoopTiming.FixedUpdate);
        private static void UpdatePreUpdate() => UpdateWorld(PlayerLoopTiming.PreUpdate);
        private static void UpdateUpdate() => UpdateWorld(PlayerLoopTiming.Update);
        private static void UpdatePreLateUpdate() => UpdateWorld(PlayerLoopTiming.PreLateUpdate);
        private static void UpdatePostLateUpdate() => UpdateWorld(PlayerLoopTiming.PostLateUpdate);
        private static void UpdateTimeUpdate() => UpdateWorld(PlayerLoopTiming.TimeUpdate);

        private static void UpdateWorld(PlayerLoopTiming timing)
        {
            if (!MotionScheduler.HasWorld(timing)) return;

            int index = (int)timing;
            double realtime = Time.realtimeSinceStartupAsDouble;
            double realtimeDelta = s_hasPreviousRealtime[index] ? realtime - s_previousRealtime[index] : 0d;
            s_previousRealtime[index] = realtime;
            s_hasPreviousRealtime[index] = true;
            MotionScheduler.UpdateWorld(timing, Time.deltaTime, Time.unscaledDeltaTime, realtimeDelta);
        }

        private static bool Insert(ref PlayerLoopSystem parent, Type groupType, Type runnerType, PlayerLoopSystem.UpdateFunction update)
        {
            if (parent.type == groupType)
            {
                PlayerLoopSystem[] source = parent.subSystemList ?? Array.Empty<PlayerLoopSystem>();
                int existingCount = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i].type != runnerType) existingCount++;
                }
                var children = new PlayerLoopSystem[existingCount + 1];
                children[0] = new PlayerLoopSystem { type = runnerType, updateDelegate = update };
                int destination = 1;
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i].type != runnerType) children[destination++] = source[i];
                }
                parent.subSystemList = children;
                return true;
            }

            PlayerLoopSystem[] systems = parent.subSystemList;
            if (systems == null) return false;
            for (int i = 0; i < systems.Length; i++)
            {
                if (Insert(ref systems[i], groupType, runnerType, update))
                {
                    parent.subSystemList = systems;
                    return true;
                }
            }
            return false;
        }

        private static class MotionLoopRunners
        {
            internal struct Initialization { }
            internal struct EarlyUpdate { }
            internal struct FixedUpdate { }
            internal struct PreUpdate { }
            internal struct Update { }
            internal struct PreLateUpdate { }
            internal struct PostLateUpdate { }
            internal struct TimeUpdate { }
        }
    }
}

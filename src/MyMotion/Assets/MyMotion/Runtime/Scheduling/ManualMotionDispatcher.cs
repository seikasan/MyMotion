using System;
using System.Collections.Generic;
using MyMotion.Internal;

namespace MyMotion
{
    /// <summary>
    /// この dispatcher を手動で進めたときだけ motion を更新する。
    /// </summary>
    public sealed class ManualMotionDispatcher : IDisposable
    {
        private static ManualMotionDispatcher s_default;
        // World を持っている dispatcher。LitMotion の ManualMotionDispatcher は Dispose を要求しないため、
        // Dispose されなかった dispatcher のネイティブメモリも終了時・assembly 再読み込み前にまとめて解放する。
        private static readonly HashSet<ManualMotionDispatcher> s_withWorld = new();

        private readonly bool _isDefault;
        private readonly ManualMotionScheduler _scheduler;
        private MotionWorld _world;
        private double _time;
        private bool _isDisposed;

        /// <summary>
        /// 共有の手動 dispatcher。
        /// </summary>
        public static ManualMotionDispatcher Default => s_default ??= new ManualMotionDispatcher(true);

        /// <summary>
        /// この dispatcher に motion を割り当てるための scheduler。
        /// </summary>
        public ManualMotionScheduler Scheduler => _scheduler;

        /// <summary>
        /// <see cref="Update(double)"/> で進めた累積時間。
        /// この値を設定すると、差分だけ dispatcher を進める (または戻す)。
        /// </summary>
        public double Time
        {
            get => _time;
            set => Update(value - _time);
        }

        /// <summary>
        /// この dispatcher が保持している motion の数。
        /// </summary>
        public int ActiveCount => _world?.Count ?? 0;

        /// <summary>
        /// 確保済み NativeArray の実データのバイト数。allocator のオーバーヘッドとマネージド配列は含まない。
        /// </summary>
        public long ReservedNativeBytes => _world?.ReservedNativeBytes ?? 0L;

        /// <summary>
        /// 独立した手動 dispatcher を作る。使い終わったら Dispose すること。
        /// </summary>
        public ManualMotionDispatcher() : this(false)
        {
        }

        private ManualMotionDispatcher(bool isDefault)
        {
            _isDefault = isDefault;
            _scheduler = new ManualMotionScheduler(this);
        }

        /// <summary>
        /// motion ストレージの容量を確保する。必要なときは容量を等比で拡張する。
        /// </summary>
        public void EnsureStorageCapacity(int capacity)
        {
            World.EnsureCapacity(capacity);
        }

        /// <summary>
        /// custom motion adapter 用の型付きストレージを確保する。
        /// </summary>
        public void EnsureStorageCapacity<TValue, TOptions, TAdapter>(int capacity)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            // FloatMotionAdapter などの組み込み Adapter は共有 storage で動くため、そちらを確保する。
            if (BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn) World.EnsureCapacity(capacity);
            else World.EnsureAdapterStorageCapacity<TValue, TOptions, TAdapter>(capacity);
        }

        /// <summary>
        /// この dispatcher に割り当てられたすべての motion を進める。
        /// </summary>
        public void Update(double deltaTime)
        {
            ThrowIfDisposed();
            if (double.IsNaN(deltaTime) || double.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
            _time += deltaTime;
            _world?.Update(deltaTime);
        }

        /// <summary>
        /// dispatcher の現在の motion を callback を呼ばずにキャンセルし、時間をリセットする。
        /// 確保した容量は保持される。
        /// </summary>
        public void Reset()
        {
            ThrowIfDisposed();
            _world?.Reset();
            _time = 0d;
        }

        /// <summary>
        /// この dispatcher のネイティブストレージを解放する。共有の既定 dispatcher は代わりに Reset すること。
        /// </summary>
        public void Dispose()
        {
            if (_isDefault)
            {
                throw new InvalidOperationException("The default dispatcher cannot be disposed. Use Reset instead.");
            }
            if (_isDisposed)
            {
                return;
            }
            ReleaseWorld();
            _isDisposed = true;
        }

        internal MotionWorld World
        {
            get
            {
                ThrowIfDisposed();
                if (_world == null)
                {
                    _world = new MotionWorld();
                    s_withWorld.Add(this);
                }
                return _world;
            }
        }

        internal static void ResetDefault()
        {
            s_default?.Reset();
        }

        // すべての dispatcher の World を解放する。dispatcher 自体は使い続けられ、次に使うと World を作り直す。
        internal static void ReleaseAllWorlds()
        {
            if (s_withWorld.Count == 0) return;
            var dispatchers = new ManualMotionDispatcher[s_withWorld.Count];
            s_withWorld.CopyTo(dispatchers);
            foreach (ManualMotionDispatcher dispatcher in dispatchers)
            {
                dispatcher.ReleaseWorld();
                dispatcher._time = 0d;
            }
        }

        private void ReleaseWorld()
        {
            if (_world == null) return;
            _world.Dispose();
            _world = null;
            s_withWorld.Remove(this);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ManualMotionDispatcher));
            }
        }
    }

    /// <summary>
    /// motion を <see cref="ManualMotionDispatcher"/> へ振り分ける scheduler。
    /// </summary>
    public sealed class ManualMotionScheduler : IMotionScheduler, IAdapterMotionScheduler
    {
        private readonly ManualMotionDispatcher _dispatcher;

        internal ManualMotionScheduler(ManualMotionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        internal MotionWorld World => _dispatcher.World;

        public MotionHandle Schedule<TValue>(MotionBuilder<TValue> builder, Action<TValue> binding) => builder.BindToWorld(World, binding);

        public MotionHandle Schedule<TValue, TState>(MotionBuilder<TValue> builder, TState state, Action<TValue, TState> binding) => builder.BindToWorld(World, state, binding);

        public MotionHandle Schedule<TValue, TState0, TState1>(MotionBuilder<TValue> builder, TState0 state0, TState1 state1, Action<TValue, TState0, TState1> binding)
            where TState0 : class
            where TState1 : class => builder.BindToWorld(World, state0, state1, binding);

        public MotionHandle Schedule<TValue, TState0, TState1, TState2>(MotionBuilder<TValue> builder, TState0 state0, TState1 state1, TState2 state2, Action<TValue, TState0, TState1, TState2> binding)
            where TState0 : class
            where TState1 : class
            where TState2 : class => builder.BindToWorld(World, state0, state1, state2, binding);

        public MotionHandle RunWithoutBinding<TValue>(MotionBuilder<TValue> builder) => builder.RunWithoutBindingInWorld(World);

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
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions> =>
            builder.BindAdapterToWorld<TValue, TOptions, TAdapter>(
                World, in start, in end, in adapterOptions, binding, state0, state1, state2, stateCount);

        public MotionHandle ScheduleAdapterWithValueState<TBuilderValue, TValue, TOptions, TAdapter, TState>(
            MotionBuilder<TBuilderValue> builder,
            in TValue start,
            in TValue end,
            in TOptions adapterOptions,
            TState state,
            Action<TValue, TState> binding)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions> =>
            builder.BindAdapterToWorldWithValueState<TValue, TOptions, TAdapter, TState>(
                World, in start, in end, in adapterOptions, state, binding);
    }
}

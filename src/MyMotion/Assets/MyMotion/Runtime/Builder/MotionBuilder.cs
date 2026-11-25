using System;

namespace MyMotion
{
    /// <remarks>
    /// LitMotion の builder は pool から借りた buffer を持つため再利用を例外にするが、
    /// MyMotion の builder は不変な値なので同じ設定から何度でも Bind できる。
    /// <see cref="Dispose"/> は <c>using</c> 構文との互換のためにあり、何もしない。
    /// </remarks>
    public partial struct MotionBuilder<TValue> : IDisposable
    {
        public readonly void Dispose() { }

        private IMotionScheduler _scheduler;
        private readonly TValue _start;
        private readonly TValue _end;
        private MotionBuilderOptions _options;
        private readonly MotionFactory<TValue> _factory;
        private readonly MotionFactory<TValue> Factory => _factory ?? MotionFactory<TValue>.Default;
        private readonly SharedMotionBuilderConfiguration _sharedConfiguration;

        internal readonly MotionTimeKind TimeKind => _sharedConfiguration != null ? _sharedConfiguration.Options.TimeKind : _options.TimeKind;

        internal MotionBuilder(
            IMotionScheduler scheduler,
            TValue start,
            TValue end,
            float duration,
            MotionFactory<TValue> factory)
        {
            // builder は With* のたびに複製される。IL2CPP は参照型 field への代入ごとに GC の write barrier を入れるため、
            // 0 で埋めてから null でない参照だけを書く。
            this = default;
            if (scheduler != null) _scheduler = scheduler;
            _start = start;
            _end = end;
            // options を struct ごと代入すると参照型 field の barrier が入るため、0 埋めの上に値型 field だけを書く。
            MotionBuilderOptions.InitializeZeroed(ref _options, duration);
            // 組み込み型の factory は MotionFactory<TValue>.Default から引けるので、field には書かない (null のまま)。
            if (factory != MotionFactory<TValue>.Default) _factory = factory;
        }

        private MotionBuilder(
            IMotionScheduler scheduler,
            TValue start,
            TValue end,
            MotionBuilderOptions options,
            MotionFactory<TValue> factory)
        {
            this = default;
            if (scheduler != null) _scheduler = scheduler;
            _start = start;
            _end = end;
            _options = options;
            if (factory != null && factory != MotionFactory<TValue>.Default) _factory = factory;
        }

        internal MotionBuilder(
            IMotionScheduler scheduler,
            TValue start,
            TValue end,
            MotionBuilderOptions options,
            MotionFactory<TValue> factory,
            SharedMotionBuilderConfiguration sharedConfiguration)
        {
            this = default;
            if (scheduler != null) _scheduler = scheduler;
            _start = start;
            _end = end;
            _options = options;
            if (factory != null && factory != MotionFactory<TValue>.Default) _factory = factory;
            if (sharedConfiguration != null) _sharedConfiguration = sharedConfiguration;
        }

        private static MotionHandle Track(MotionHandle handle, IMotionScheduler scheduler)
        {
            if (MotionDebugger.Enabled) MotionDebugger.AddTracking(handle, scheduler, typeof(TValue), 3);
            return handle;
        }

        internal readonly TValue StartValue => _start;
        internal readonly TValue EndValue => _end;

        /// <summary>
        /// 同じ設定 (時間・Ease・callback など) のまま、値の型だけ別の builder を作る。
        /// LitMotion 形式の型付き builder と相互変換するために使う。
        /// </summary>
        internal readonly MotionBuilder<TOther> Retarget<TOther>(TOther start, TOther end, MotionFactory<TOther> factory, IntegerRoundingMode? roundingMode = null)
        {
            MotionBuilderOptions options = _options;
            SharedMotionBuilderConfiguration shared = _sharedConfiguration;
            if (roundingMode.HasValue)
            {
                if (shared != null)
                {
                    options = shared.Options;
                    shared = null;
                }
                options.RoundingMode = roundingMode.Value;
            }
            return new MotionBuilder<TOther>(_scheduler, start, end, options, factory, shared);
        }

        internal readonly IntegerRoundingMode RoundingMode => (_sharedConfiguration?.Options ?? _options).RoundingMode;
    }
}

using System;
using UnityEngine;

namespace MyMotion
{
    /// <summary>
    /// motion の作成に使う設定を保持する。
    /// </summary>
    [Serializable]
    public record MotionSettings<TValue, TOptions>
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
    {
        public TValue StartValue
        {
            get => _startValue;
            init => _startValue = value;
        }

        public TValue EndValue
        {
            get => _endValue;
            init => _endValue = value;
        }

        public float Duration
        {
            get => _duration;
            init => _duration = value;
        }

        public TOptions Options
        {
            get => _options;
            init => _options = value;
        }

        public Ease Ease
        {
            get => _ease;
            init => _ease = value;
        }

        public AnimationCurve CustomEaseCurve
        {
            get => _customEaseCurve;
            init => _customEaseCurve = value;
        }

        public float Delay
        {
            get => _delay;
            init => _delay = value;
        }

        public DelayType DelayType
        {
            get => _delayType;
            init => _delayType = value;
        }

        public int Loops
        {
            get => _loops;
            init => _loops = value;
        }

        public LoopType LoopType
        {
            get => _loopType;
            init => _loopType = value;
        }

        public bool CancelOnError
        {
            get => _cancelOnError;
            init => _cancelOnError = value;
        }

        public bool SkipValuesDuringDelay
        {
            get => _skipValuesDuringDelay;
            init => _skipValuesDuringDelay = value;
        }

        public bool ImmediateBind
        {
            get => _immediateBind;
            init => _immediateBind = value;
        }

        public IMotionScheduler Scheduler
        {
            get => _scheduler;
            init => _scheduler = value;
        }

        /// <summary>
        /// MyMotion 独自の再生速度。既定値は 1。
        /// </summary>
        public float PlaybackSpeed
        {
            get => _playbackSpeed;
            init => _playbackSpeed = value;
        }

        /// <summary>
        /// MyMotion 独自の時間源。scheduler が上書きしない場合に使われる。
        /// </summary>
        public MotionTimeKind TimeKind
        {
            get => _timeKind;
            init => _timeKind = value;
        }

        [SerializeField] private TValue _startValue;
        [SerializeField] private TValue _endValue;
        [SerializeField] private float _duration;
        [SerializeField] private TOptions _options;
        [SerializeField] private Ease _ease;
        [SerializeField] private AnimationCurve _customEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _delay;
        [SerializeField] private DelayType _delayType;
        [SerializeField] private int _loops = 1;
        [SerializeField] private LoopType _loopType;
        [SerializeField] private bool _cancelOnError;
        [SerializeField] private bool _skipValuesDuringDelay;
        [SerializeField] private bool _immediateBind;
        [SerializeField] private float _playbackSpeed = 1f;
        [SerializeField] private MotionTimeKind _timeKind;

        internal IMotionScheduler _scheduler;
    }

    /// <summary>
    /// 追加パラメータを必要としない adapter motion 用のオプション。
    /// </summary>
    [Serializable]
    public readonly struct NoOptions : IMotionOptions, IEquatable<NoOptions>
    {
        public bool Equals(NoOptions other) => true;

        public override bool Equals(object obj) => obj is NoOptions;

        public override int GetHashCode() => 0;
    }

    /// <summary>
    /// 整数 motion が使う丸めモード。
    /// </summary>
    public enum RoundingMode : byte
    {
        ToEven,
        AwayFromZero,
        ToZero,
        ToPositiveInfinity,
        ToNegativeInfinity
    }

    /// <summary>
    /// 整数 motion のオプション。
    /// </summary>
    [Serializable]
    public struct IntegerOptions : IMotionOptions, IEquatable<IntegerOptions>
    {
        public RoundingMode RoundingMode;

        public readonly bool Equals(IntegerOptions other) => other.RoundingMode == RoundingMode;

        public override readonly bool Equals(object obj) => obj is IntegerOptions other && Equals(other);

        public override readonly int GetHashCode() => (int)RoundingMode;
    }
}

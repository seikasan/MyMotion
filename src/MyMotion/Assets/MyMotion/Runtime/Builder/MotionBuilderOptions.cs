namespace MyMotion
{
    internal struct MotionBuilderOptions
    {
        public float Duration;
        public float Delay;
        public float PlaybackSpeed;
        public int Loops;
        public Ease Ease;
        public UnityEngine.AnimationCurve CustomEaseCurve;
        public LoopType LoopType;
        public MotionTimeKind TimeKind;
        public IntegerRoundingMode RoundingMode;
        public bool DelayEveryLoop;
        public bool SkipValuesDuringDelay;
        public bool CancelOnError;
        public bool ImmediateBind;
        public string DebugName;
        public System.Action OnComplete;
        public System.Action OnCancel;
        public System.Action<int> OnLoopComplete;

        public MotionBuilderOptions(float duration)
        {
            // 参照型 field へ null を個別に代入すると、IL2CPP はそれぞれに GC の write barrier を入れる。
            // まとめて 0 で埋めてから値型 field だけを設定する。
            this = default;
            InitializeZeroed(ref this, duration);
        }

        /// <summary>0 で埋まった options に、既定値の値型 field だけを書く。</summary>
        public static void InitializeZeroed(ref MotionBuilderOptions options, float duration)
        {
            options.Duration = duration;
            options.PlaybackSpeed = 1f;
            options.Loops = 1;
            options.Ease = Ease.Linear;
            options.LoopType = LoopType.Restart;
            options.TimeKind = MotionTimeKind.Time;
            options.RoundingMode = IntegerRoundingMode.ToEven;
            options.SkipValuesDuringDelay = true;
            options.ImmediateBind = true;
        }
    }
}

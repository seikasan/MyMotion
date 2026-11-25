namespace MyMotion
{
    public enum Ease : byte
    {
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        CustomAnimationCurve
    }

    public enum LoopType : byte
    {
        Restart,
        Flip,
        Incremental,
        Yoyo
    }

    public enum DelayType : byte
    {
        FirstLoop,
        EveryLoop
    }

    public enum MotionTimeKind : byte
    {
        Time,
        UnscaledTime,
        Realtime
    }

    /// <summary>
    /// motion scheduler が使う Unity PlayerLoop の実行タイミング。
    /// </summary>
    public enum PlayerLoopTiming : byte
    {
        Initialization,
        EarlyUpdate,
        FixedUpdate,
        PreUpdate,
        Update,
        PreLateUpdate,
        PostLateUpdate,
        TimeUpdate
    }

    public enum IntegerRoundingMode : byte
    {
        ToEven,
        AwayFromZero,
        ToZero,
        ToPositiveInfinity,
        ToNegativeInfinity
    }

    /// <summary>
    /// ValueTask のキャンセルトークンがキャンセルされたとき、motion をどう扱うかを指定する。
    /// </summary>
    public enum CancelBehavior : byte
    {
        None,
        Cancel,
        Complete
    }
}

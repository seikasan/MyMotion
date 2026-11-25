using System;
using UnityEngine;

namespace MyMotion
{
    internal enum MotionSettingsSchedulerType : byte
    {
        Default,
        Initialization,
        InitializationIgnoreTimeScale,
        InitializationRealtime,
        EarlyUpdate,
        EarlyUpdateIgnoreTimeScale,
        EarlyUpdateRealtime,
        FixedUpdate,
        PreUpdate,
        PreUpdateIgnoreTimeScale,
        PreUpdateRealtime,
        Update,
        UpdateIgnoreTimeScale,
        UpdateRealtime,
        PreLateUpdate,
        PreLateUpdateIgnoreTimeScale,
        PreLateUpdateRealtime,
        PostLateUpdate,
        PostLateUpdateIgnoreTimeScale,
        PostLateUpdateRealtime,
        TimeUpdate,
        TimeUpdateIgnoreTimeScale,
        TimeUpdateRealtime,
        Manual
    }

    /// <summary>
    /// Inspector で編集できる、シリアライズ可能な motion 設定。
    /// </summary>
    [Serializable]
    public sealed record SerializableMotionSettings<TValue, TOptions>
        : MotionSettings<TValue, TOptions>, ISerializationCallbackReceiver
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
    {
        [SerializeField] private MotionSettingsSchedulerType _schedulerType;

        public void OnBeforeSerialize()
        {
            SetScheduler(Scheduler);
        }

        public void OnAfterDeserialize()
        {
            _scheduler = _schedulerType switch
            {
                MotionSettingsSchedulerType.Initialization => MotionScheduler.Initialization,
                MotionSettingsSchedulerType.InitializationIgnoreTimeScale => MotionScheduler.InitializationIgnoreTimeScale,
                MotionSettingsSchedulerType.InitializationRealtime => MotionScheduler.InitializationRealtime,
                MotionSettingsSchedulerType.EarlyUpdate => MotionScheduler.EarlyUpdate,
                MotionSettingsSchedulerType.EarlyUpdateIgnoreTimeScale => MotionScheduler.EarlyUpdateIgnoreTimeScale,
                MotionSettingsSchedulerType.EarlyUpdateRealtime => MotionScheduler.EarlyUpdateRealtime,
                MotionSettingsSchedulerType.FixedUpdate => MotionScheduler.FixedUpdate,
                MotionSettingsSchedulerType.PreUpdate => MotionScheduler.PreUpdate,
                MotionSettingsSchedulerType.PreUpdateIgnoreTimeScale => MotionScheduler.PreUpdateIgnoreTimeScale,
                MotionSettingsSchedulerType.PreUpdateRealtime => MotionScheduler.PreUpdateRealtime,
                MotionSettingsSchedulerType.Update => MotionScheduler.Update,
                MotionSettingsSchedulerType.UpdateIgnoreTimeScale => MotionScheduler.UpdateIgnoreTimeScale,
                MotionSettingsSchedulerType.UpdateRealtime => MotionScheduler.UpdateRealtime,
                MotionSettingsSchedulerType.PreLateUpdate => MotionScheduler.PreLateUpdate,
                MotionSettingsSchedulerType.PreLateUpdateIgnoreTimeScale => MotionScheduler.PreLateUpdateIgnoreTimeScale,
                MotionSettingsSchedulerType.PreLateUpdateRealtime => MotionScheduler.PreLateUpdateRealtime,
                MotionSettingsSchedulerType.PostLateUpdate => MotionScheduler.PostLateUpdate,
                MotionSettingsSchedulerType.PostLateUpdateIgnoreTimeScale => MotionScheduler.PostLateUpdateIgnoreTimeScale,
                MotionSettingsSchedulerType.PostLateUpdateRealtime => MotionScheduler.PostLateUpdateRealtime,
                MotionSettingsSchedulerType.TimeUpdate => MotionScheduler.TimeUpdate,
                MotionSettingsSchedulerType.TimeUpdateIgnoreTimeScale => MotionScheduler.TimeUpdateIgnoreTimeScale,
                MotionSettingsSchedulerType.TimeUpdateRealtime => MotionScheduler.TimeUpdateRealtime,
                MotionSettingsSchedulerType.Manual => MotionScheduler.Manual,
                _ => null
            };
        }

        private void SetScheduler(IMotionScheduler value)
        {
            _schedulerType = MotionSettingsSchedulerType.Default;
            if (value == MotionScheduler.Initialization) _schedulerType = MotionSettingsSchedulerType.Initialization;
            else if (value == MotionScheduler.InitializationIgnoreTimeScale) _schedulerType = MotionSettingsSchedulerType.InitializationIgnoreTimeScale;
            else if (value == MotionScheduler.InitializationRealtime) _schedulerType = MotionSettingsSchedulerType.InitializationRealtime;
            else if (value == MotionScheduler.EarlyUpdate) _schedulerType = MotionSettingsSchedulerType.EarlyUpdate;
            else if (value == MotionScheduler.EarlyUpdateIgnoreTimeScale) _schedulerType = MotionSettingsSchedulerType.EarlyUpdateIgnoreTimeScale;
            else if (value == MotionScheduler.EarlyUpdateRealtime) _schedulerType = MotionSettingsSchedulerType.EarlyUpdateRealtime;
            else if (value == MotionScheduler.FixedUpdate) _schedulerType = MotionSettingsSchedulerType.FixedUpdate;
            else if (value == MotionScheduler.PreUpdate) _schedulerType = MotionSettingsSchedulerType.PreUpdate;
            else if (value == MotionScheduler.PreUpdateIgnoreTimeScale) _schedulerType = MotionSettingsSchedulerType.PreUpdateIgnoreTimeScale;
            else if (value == MotionScheduler.PreUpdateRealtime) _schedulerType = MotionSettingsSchedulerType.PreUpdateRealtime;
            else if (value == MotionScheduler.Update) _schedulerType = MotionSettingsSchedulerType.Update;
            else if (value == MotionScheduler.UpdateIgnoreTimeScale) _schedulerType = MotionSettingsSchedulerType.UpdateIgnoreTimeScale;
            else if (value == MotionScheduler.UpdateRealtime) _schedulerType = MotionSettingsSchedulerType.UpdateRealtime;
            else if (value == MotionScheduler.PreLateUpdate) _schedulerType = MotionSettingsSchedulerType.PreLateUpdate;
            else if (value == MotionScheduler.PreLateUpdateIgnoreTimeScale) _schedulerType = MotionSettingsSchedulerType.PreLateUpdateIgnoreTimeScale;
            else if (value == MotionScheduler.PreLateUpdateRealtime) _schedulerType = MotionSettingsSchedulerType.PreLateUpdateRealtime;
            else if (value == MotionScheduler.PostLateUpdate) _schedulerType = MotionSettingsSchedulerType.PostLateUpdate;
            else if (value == MotionScheduler.PostLateUpdateIgnoreTimeScale) _schedulerType = MotionSettingsSchedulerType.PostLateUpdateIgnoreTimeScale;
            else if (value == MotionScheduler.PostLateUpdateRealtime) _schedulerType = MotionSettingsSchedulerType.PostLateUpdateRealtime;
            else if (value == MotionScheduler.TimeUpdate) _schedulerType = MotionSettingsSchedulerType.TimeUpdate;
            else if (value == MotionScheduler.TimeUpdateIgnoreTimeScale) _schedulerType = MotionSettingsSchedulerType.TimeUpdateIgnoreTimeScale;
            else if (value == MotionScheduler.TimeUpdateRealtime) _schedulerType = MotionSettingsSchedulerType.TimeUpdateRealtime;
            else if (value == MotionScheduler.Manual) _schedulerType = MotionSettingsSchedulerType.Manual;
        }
    }
}

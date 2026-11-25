using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyMotion.Animation.Components
{
    [Serializable]
    [MyMotionAnimationComponentMenu("Control/Delay")]
    public sealed class DelayComponent : MyMotionAnimationComponent
    {
        [SerializeField] private float _delay;

        public override MotionHandle Play()
        {
            return MMotion.Create(0f, 1f, _delay)
                .RunWithoutBinding();
        }

        public override void OnStop() { }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Control/Event")]
    public sealed class EventComponent : MyMotionAnimationComponent
    {
        [Space(5f)]
        [SerializeField]
        private UnityEvent _onPlay;
        [SerializeField] private UnityEvent _onStop;

        public override MotionHandle Play()
        {
            _onPlay.Invoke();
            return MMotion.Create(0f, 1f, 0f).RunWithoutBinding();
        }

        public override void OnStop()
        {
            _onStop.Invoke();
        }
    }

    [Serializable]
    [MyMotionAnimationComponentMenu("Control/Play MyMotion Animation")]
    public sealed class PlayMyMotionAnimationComponent : MyMotionAnimationComponent
    {
        [SerializeField] private MyMotionAnimation _target;

        public override MotionHandle Play()
        {
            _target.Play();
            return MMotion.Create(0f, 1f, float.MaxValue)
                .Bind(this, (_, _) =>
                {
                    if (_target == null) TrackedHandle.TryComplete();
                    if (!_target.IsPlaying) TrackedHandle.TryComplete();
                });
        }

        public override void OnResume()
        {
            _target.Play();
        }

        public override void OnPause()
        {
            _target.Pause();
        }

        public override void OnStop()
        {
            _target.Stop();
        }
    }
}

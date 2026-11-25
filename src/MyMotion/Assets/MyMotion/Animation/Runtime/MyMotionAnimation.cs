using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyMotion.Animation
{
    [AddComponentMenu("MyMotion Animation")]
    public sealed class MyMotionAnimation : MonoBehaviour, ISerializationCallbackReceiver
    {
        private enum AutoPlayMode
        {
            None,
            OnStart,
            OnEnable
        }

        private enum AnimationMode
        {
            Parallel,
            Sequential
        }

        [SerializeField] private AutoPlayMode _autoPlayMode = AutoPlayMode.OnStart;
        [SerializeField] private AnimationMode _animationMode;

        [SerializeReference] private MyMotionAnimationComponent[] _components;

        private readonly Queue<MyMotionAnimationComponent> _queue = new();
        private readonly List<MyMotionAnimationComponent> _playingComponents = new();
        private Action _moveNextMotion;
        private bool _stopping;

        [HideInInspector, SerializeField] private bool _playOnAwake = true;
        [HideInInspector, SerializeField] private int _version;

        public IReadOnlyList<MyMotionAnimationComponent> Components => _components;

        private void OnEnable()
        {
            if (_autoPlayMode == AutoPlayMode.OnEnable)
                Play();
        }

        private void Start()
        {
            if (_autoPlayMode == AutoPlayMode.OnStart)
                Play();
        }

        private void MoveNextMotion()
        {
            // Stop 中の cancel からも continuation が呼ばれるため、次の motion を始めない。
            if (_stopping) return;
            if (!_queue.TryDequeue(out MyMotionAnimationComponent queuedComponent)) return;

            try
            {
                MotionHandle handle = queuedComponent.Play();
                bool isActive = handle.IsActive();

                if (isActive)
                {
                    handle.Preserve();
                    // 完了時に次へ進む。OnComplete は利用者の callback を上書きしないよう continuation に積む。
                    handle.AddContinuation(_moveNextMotion ??= MoveNextMotion);
                }

                queuedComponent.TrackedHandle = handle;
                _playingComponents.Add(queuedComponent);

                if (!isActive)
                {
                    MoveNextMotion();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, context: this);
            }
        }

        public void Play()
        {
            bool isPlaying = false;

            foreach (MyMotionAnimationComponent component in _playingComponents)
            {
                MotionHandle handle = component.TrackedHandle;
                if (!handle.IsActive()) continue;

                handle.PlaybackSpeed = 1f;
                isPlaying = true;

                component.OnResume();
            }

            if (isPlaying) return;

            _playingComponents.Clear();

            switch (_animationMode)
            {
                case AnimationMode.Sequential:
                    foreach (MyMotionAnimationComponent component in _components)
                    {
                        if (component == null) continue;
                        if (!component.Enabled) continue;
                        _queue.Enqueue(component);
                    }

                    MoveNextMotion();
                    break;
                case AnimationMode.Parallel:
                    foreach (MyMotionAnimationComponent component in _components)
                    {
                        if (component == null) continue;
                        if (!component.Enabled) continue;

                        try
                        {
                            MotionHandle handle = component.Play();
                            component.TrackedHandle = handle;

                            if (handle.IsActive())
                            {
                                handle.Preserve();
                            }

                            _playingComponents.Add(component);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex, context: this);
                        }
                    }
                    break;
            }
        }

        public void Pause()
        {
            foreach (MyMotionAnimationComponent component in _playingComponents)
            {
                MotionHandle handle = component.TrackedHandle;
                if (!handle.IsActive()) continue;

                handle.PlaybackSpeed = 0f;
                component.OnPause();
            }
        }

        public void Stop()
        {
            _stopping = true;
            try
            {
                _queue.Clear();
                for (int i = _playingComponents.Count - 1; i >= 0; i--)
                {
                    MyMotionAnimationComponent component = _playingComponents[i];
                    MotionHandle handle = component.TrackedHandle;
                    handle.TryCancel();
                    component.OnStop();
                    component.TrackedHandle = handle;
                }

                _playingComponents.Clear();
            }
            finally
            {
                _stopping = false;
            }
        }

        public void Restart()
        {
            Stop();
            Play();
        }

        public bool IsActive
        {
            get
            {
                if (_queue.Count > 0) return true;

                foreach (MyMotionAnimationComponent component in _playingComponents)
                {
                    MotionHandle handle = component.TrackedHandle;
                    if (handle.IsActive()) return true;
                }

                return false;
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (_queue.Count > 0) return true;

                foreach (MyMotionAnimationComponent component in _playingComponents)
                {
                    MotionHandle handle = component.TrackedHandle;
                    if (handle.IsPlaying()) return true;
                }

                return false;
            }
        }

        private void OnDisable()
        {
            if (_autoPlayMode == AutoPlayMode.OnEnable)
                Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_version < 1)
            {
                _autoPlayMode = _playOnAwake ? AutoPlayMode.OnStart : AutoPlayMode.None;
                _version = 1;
            }
        }
    }
}

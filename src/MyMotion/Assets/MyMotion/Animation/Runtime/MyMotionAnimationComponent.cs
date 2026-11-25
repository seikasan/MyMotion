using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MyMotion.Animation
{
    [Serializable]
    public abstract class MyMotionAnimationComponent
    {
        public MyMotionAnimationComponent()
        {
#if UNITY_EDITOR
            Type type = GetType();
            var attribute = type.GetCustomAttribute<MyMotionAnimationComponentMenuAttribute>();
            _displayName = attribute != null
                ? attribute.MenuName.Split('/').Last()
                : type.Name;
#endif
        }

        [SerializeField] private string _displayName;
        [SerializeField] private bool _enabled = true;

        public bool Enabled => _enabled;
        public string DisplayName => _displayName;

        public abstract MotionHandle Play();

        public virtual void OnResume() { }
        public virtual void OnPause() { }
        public virtual void OnStop() { }

        public MotionHandle TrackedHandle { get; set; }
    }
}

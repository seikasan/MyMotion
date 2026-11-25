using System;

namespace MyMotion.Animation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MyMotionAnimationComponentMenuAttribute : Attribute
    {
        public MyMotionAnimationComponentMenuAttribute(string menuName)
        {
            MenuName = menuName;
        }

        public string MenuName { get; }
    }
}

using System;
using System.Runtime.CompilerServices;

namespace MyMotion
{
    public readonly struct MotionAwaiter : ICriticalNotifyCompletion
    {
        private readonly MotionHandle _handle;

        internal MotionAwaiter(MotionHandle handle)
        {
            _handle = handle;
        }

        public bool IsCompleted => _handle.IsAwaitCompleted;

        public MotionAwaiter GetAwaiter() => this;

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation != null) _handle.AddContinuation(continuation);
        }
    }
}

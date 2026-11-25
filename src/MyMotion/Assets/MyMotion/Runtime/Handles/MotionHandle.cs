using System;
using MyMotion.Internal;

namespace MyMotion
{
    public readonly struct MotionHandle : IEquatable<MotionHandle>
    {
        public static readonly MotionHandle None = default;

        public int StorageId => _coreHandle.Owner;

        public int Index => _coreHandle.Slot;

        public int Version => (int)_coreHandle.Version;

        // LitMotion の MotionHandle と同じく、参照型の field を持たない (unmanaged)。
        // World は CoreMotionHandle.Owner から引く。参照を持つと、handle を配列や field に保存するたびに
        // IL2CPP が GC の write barrier を呼ぶため。
        private readonly CoreMotionHandle _coreHandle;

        internal MotionHandle(MotionWorld world, CoreMotionHandle coreHandle)
        {
            _coreHandle = coreHandle;
        }

        // Dispose された World の handle は null を返し、無効な handle として扱う。
        private MotionWorld OwnerWorld => MotionWorld.FromOwner(_coreHandle.Owner);

        private MotionWorld World => OwnerWorld ?? throw new InvalidOperationException("Motion handle is not valid.");

        public bool IsValid() => OwnerWorld is { } world && world.IsValid(_coreHandle);

        public bool IsActive() => OwnerWorld is { } world && world.IsActive(_coreHandle);

        public bool IsPlaying() => OwnerWorld is { } world && world.IsPlaying(_coreHandle);

        internal bool IsAwaitCompleted => !(OwnerWorld is { } world) || world.IsAwaitCompleted(_coreHandle);

        internal bool WasCanceled => OwnerWorld is { } world && world.WasCanceled(_coreHandle);

        internal string GetDebugNameInternal() => OwnerWorld is { } world ? world.GetDebugName(_coreHandle) : ToString();

        public float Delay => World.GetDelay(_coreHandle);

        public float Duration => World.GetDuration(_coreHandle);

        public double TotalDuration => World.GetTotalDuration(_coreHandle);

        public int Loops => World.GetLoops(_coreHandle);

        public int CompletedLoops
        {
            get
            {
                ThrowIfInSequence();
                return World.GetCompletedLoops(_coreHandle);
            }
        }

        public float PlaybackSpeed
        {
            get
            {
                ThrowIfInSequence();
                return World.GetPlaybackSpeed(_coreHandle);
            }
            set
            {
                ThrowIfInSequence();
                if (!World.TrySetPlaybackSpeed(_coreHandle, value))
                {
                    throw new InvalidOperationException("Motion handle is not active.");
                }
            }
        }

        public double Time
        {
            get => World.GetTime(_coreHandle);
            set
            {
                ThrowIfInSequence();
                if (!World.TrySetTime(_coreHandle, value))
                {
                    throw new InvalidOperationException("Motion handle is not active or the time value is invalid.");
                }
            }
        }

        public bool TryCancel() => OwnerWorld is { } world && world.Cancel(_coreHandle);

        public void Cancel()
        {
            ThrowIfInSequence();
            if (!TryCancel()) ThrowControlFailure(false);
        }

        public bool TryComplete() => OwnerWorld is { } world && world.TryComplete(_coreHandle);

        public void Complete()
        {
            ThrowIfInSequence();
            if (!TryComplete()) ThrowControlFailure(true);
        }

        // LitMotion と同じ例外メッセージで失敗理由を伝える。成功時の経路には追加費用を置かない。
        private void ThrowControlFailure(bool complete)
        {
            MotionWorld world = OwnerWorld;
            if (world == null || !world.IsValid(_coreHandle))
            {
                throw new InvalidOperationException("Motion has been destroyed or no longer exists.");
            }
            if (complete && world.IsInfiniteLoop(_coreHandle))
            {
                throw new InvalidOperationException("Complete was ignored because it is not possible to complete a motion that loops infinitely. If you want to end the motion, call Cancel() instead.");
            }
            throw new InvalidOperationException("Motion has already been canceled or completed.");
        }

        internal bool IsInSequence => OwnerWorld is { } world && world.IsInSequence(_coreHandle);

        internal bool IsPreserved => OwnerWorld is { } world && world.IsPreserved(_coreHandle);

        internal double AddToSequence()
        {
            MotionWorld world = OwnerWorld ?? throw new ArgumentException("Cannot add an invalid motion to a sequence.");
            return world.AddToSequence(_coreHandle);
        }

        internal void ReleaseSequence()
        {
            OwnerWorld?.ReleaseSequence(_coreHandle);
        }

        internal bool TrySetTimeFromSequence(double time)
        {
            return OwnerWorld is { } world && world.TrySetTime(_coreHandle, time, false, true, false);
        }

        internal bool FinishFromSequence()
        {
            return OwnerWorld is { } world && world.FinishFromSequence(_coreHandle);
        }

        public MotionHandle OnComplete(Action callback)
        {
            OwnerWorld?.SetOnComplete(_coreHandle, callback);
            return this;
        }

        public MotionHandle OnCancel(Action callback)
        {
            OwnerWorld?.SetOnCancel(_coreHandle, callback);
            return this;
        }

        public MotionHandle OnLoopComplete(Action<int> callback)
        {
            OwnerWorld?.SetOnLoopComplete(_coreHandle, callback);
            return this;
        }

        internal void AddContinuation(Action continuation) => OwnerWorld?.AddContinuation(_coreHandle, continuation);

        internal void RemoveContinuation(Action continuation) => OwnerWorld?.RemoveContinuation(_coreHandle, continuation);

        internal void AttachTransformTarget(UnityEngine.Transform transform, TransformTargetKind kind) =>
            OwnerWorld?.AttachTransformTarget(_coreHandle, transform, kind);

        internal void PreserveCore()
        {
            World.Preserve(_coreHandle);
        }

        private void ThrowIfInSequence()
        {
            if (IsInSequence)
            {
                throw new InvalidOperationException("Cannot access the motion in sequence.");
            }
        }

        public bool Equals(MotionHandle other) => _coreHandle.Equals(other._coreHandle);

        public override bool Equals(object obj) => obj is MotionHandle other && Equals(other);

        public override int GetHashCode() => _coreHandle.GetHashCode();

        public static bool operator ==(MotionHandle left, MotionHandle right) => left.Equals(right);

        public static bool operator !=(MotionHandle left, MotionHandle right) => !left.Equals(right);

        public override string ToString() => $"MotionHandle`{_coreHandle.Owner} ({_coreHandle.Slot}:{_coreHandle.Version})";
    }
}

using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        internal struct Data
        {
            private const ushort DelayEveryLoopFlag = 1 << 0;
            internal const ushort IsValueStateBindingFlag = 1 << 1;
            internal const ushort StateCountMask = 3 << 12;
            private const ushort HasBindingFlag = 1 << 2;
            private const ushort IsCanceledFlag = 1 << 3;
            private const ushort IsCompleteFlag = 1 << 4;
            private const ushort IsDelayedFlag = 1 << 5;
            private const ushort SkipValuesDuringDelayFlag = 1 << 6;
            private const ushort CancelOnErrorFlag = 1 << 7;
            private const ushort IsCompletionNotifiedFlag = 1 << 8;
            private const ushort HasStartedFlag = 1 << 9;
            private const ushort IsInSequenceFlag = 1 << 10;
            private const ushort IsPreservedFlag = 1 << 11;
            private const ushort IsRemovalQueuedFlag = 1 << 14;
            private const ushort IsAdapterFlag = 1 << 15;

            public double Elapsed;
            public float3 Start;
            public float3 End;
            public float3 Value;
            public float Duration;
            public float Delay;
            public float PlaybackSpeed;
            public int Slot;
            public int Loops;
            public ushort CompletedLoops;
            public ushort PreviousCompletedLoops;
            public int WideIndex;
            public MotionEase Ease;
            public MotionLoopType LoopType;
            public MotionTimeKind TimeKind;
            public MotionValueType ValueType;
            public MotionRoundingMode RoundingMode;
            public ushort Flags;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private readonly bool GetFlag(ushort flag) => (Flags & flag) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SetFlag(ushort flag, bool value) =>
                Flags = value
                ? (ushort)(Flags | flag)
                : (ushort)(Flags & ~flag);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ushort CreateFlags(
                bool delayEveryLoop,
                byte stateCount,
                bool hasBinding,
                bool isDelayed,
                bool skipValuesDuringDelay,
                bool cancelOnError)
            {
                ushort flags = 0;
                if (delayEveryLoop) flags |= DelayEveryLoopFlag;
                flags |= (ushort)(stateCount << 12);
                if (hasBinding) flags |= HasBindingFlag;
                if (isDelayed) flags |= IsDelayedFlag;
                if (skipValuesDuringDelay) flags |= SkipValuesDuringDelayFlag;
                if (cancelOnError) flags |= CancelOnErrorFlag;
                return flags;
            }

            public bool DelayEveryLoop
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(DelayEveryLoopFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(DelayEveryLoopFlag, value);
            }
            public bool IsValueStateBinding
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsValueStateBindingFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsValueStateBindingFlag, value);
            }
            public readonly byte StateCount
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (byte)((Flags & StateCountMask) >> 12);
            }

            public bool HasBinding
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(HasBindingFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(HasBindingFlag, value);
            }

            public bool IsCanceled
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsCanceledFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsCanceledFlag, value);
            }

            public bool IsComplete
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsCompleteFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsCompleteFlag, value);
            }

            public bool IsDelayed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsDelayedFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsDelayedFlag, value);
            }
            public bool SkipValuesDuringDelay
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(SkipValuesDuringDelayFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(SkipValuesDuringDelayFlag, value);
            }

            public bool CancelOnError
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(CancelOnErrorFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(CancelOnErrorFlag, value);
            }

            public bool IsCompletionNotified
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsCompletionNotifiedFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsCompletionNotifiedFlag, value);
            }

            public bool HasStarted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(HasStartedFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(HasStartedFlag, value);
            }

            public bool IsInSequence
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsInSequenceFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsInSequenceFlag, value);
            }
            public bool IsPreserved
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsPreservedFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsPreservedFlag, value);
            }

            public bool IsAdapter
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsAdapterFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsAdapterFlag, value);
            }
            public bool IsRemovalQueued
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                readonly get => GetFlag(IsRemovalQueuedFlag);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetFlag(IsRemovalQueuedFlag, value);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyMotion
{
    /// <summary>
    /// 実行中の motion を追跡する。LitMotion の MotionDebugger と同じ使い方で、Motion Debugger ウィンドウから使う。
    /// 無効な間は Bind ごとに bool を1回読むだけで、割り当ても callback の差し替えもしない。
    /// </summary>
    public static class MotionDebugger
    {
        public static bool Enabled = false;
        public static bool EnableStackTrace = false;

        private static readonly List<TrackingState> s_trackings = new(16);
        private static readonly Stack<TrackingState> s_pool = new(16);

        /// <summary>
        /// 追跡中の motion。終了した motion は <see cref="RemoveInactive"/> か次の追加時に取り除かれる。
        /// </summary>
        public static IReadOnlyList<TrackingState> Items => s_trackings;

        public static void AddTracking(MotionHandle motionHandle, IMotionScheduler scheduler, int skipFrames = 3) =>
            AddTracking(motionHandle, scheduler, null, skipFrames + 1);

        internal static void AddTracking(MotionHandle motionHandle, IMotionScheduler scheduler, Type valueType, int skipFrames)
        {
            // callback を包まずに済むよう、終了済みの要素は追加時にまとめて回収する。
            if (s_trackings.Count >= 64 && (s_trackings.Count & 63) == 0) RemoveInactive();

            TrackingState state = s_pool.Count > 0 ? s_pool.Pop() : new TrackingState();
            state.ValueType = valueType;
            state.Scheduler = scheduler ?? MotionScheduler.DefaultScheduler;
            state.Handle = motionHandle;
            state.DebugName = motionHandle.GetDebugName();
#if UNITY_EDITOR
            state.CreatedOnEditor = !UnityEditor.EditorApplication.isPlaying;
#endif
            state.StackTrace = EnableStackTrace ? new StackTrace(skipFrames, true) : null;
            s_trackings.Add(state);
        }

        /// <summary>
        /// 終了 (完了・キャンセル・破棄) した motion を一覧から外す。Preserve 中の motion は残す。
        /// </summary>
        public static void RemoveInactive()
        {
            int write = 0;
            for (int read = 0; read < s_trackings.Count; read++)
            {
                TrackingState state = s_trackings[read];
                if (state.Handle.IsActive())
                {
                    s_trackings[write++] = state;
                }
                else
                {
                    state.Reset();
                    s_pool.Push(state);
                }
            }
            s_trackings.RemoveRange(write, s_trackings.Count - write);
        }

        public static void Clear()
        {
            for (int i = 0; i < s_trackings.Count; i++)
            {
                s_trackings[i].Reset();
                s_pool.Push(s_trackings[i]);
            }
            s_trackings.Clear();
        }

        public sealed class TrackingState
        {
            public Type ValueType;
            public IMotionScheduler Scheduler;
            public MotionHandle Handle;
            public string DebugName;
            public StackTrace StackTrace;
            public bool CreatedOnEditor;

            internal void Reset()
            {
                ValueType = null;
                Scheduler = null;
                Handle = default;
                DebugName = null;
                StackTrace = null;
                CreatedOnEditor = false;
            }
        }
    }
}

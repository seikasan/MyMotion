using System;
using UnityEngine;

namespace MyMotion
{
    /// <summary>
    /// LitMotion の MotionDispatcher と同じ使い方で、全 World 共通の設定と操作を提供する。
    /// </summary>
    public static class MotionDispatcher
    {
        private static Action<Exception> s_unhandledException = DefaultUnhandledExceptionHandler;

        /// <summary>
        /// callback や Binding から投げられた未処理例外の受け取り先を設定する。
        /// </summary>
        public static void RegisterUnhandledExceptionHandler(Action<Exception> unhandledExceptionHandler)
        {
            s_unhandledException = unhandledExceptionHandler;
        }

        /// <summary>
        /// 現在の未処理例外 handler を取得する。
        /// </summary>
        public static Action<Exception> GetUnhandledExceptionHandler() => s_unhandledException;

        /// <summary>
        /// PlayerLoop 上の全 motion を取り除く。
        /// </summary>
        public static void Clear() => MotionScheduler.ClearAll();

        /// <summary>
        /// 組み込み値型 (float、Vector3、Quaternion など) の storage 容量を最低 <paramref name="capacity"/> まで確保する。
        /// MyMotion の組み込み値型は1つの storage を共有するため、型を指定する必要はない。
        /// </summary>
        public static void EnsureStorageCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            MotionScheduler.EnsureCapacity(capacity);
        }

        /// <summary>
        /// Adapter 型の storage 容量を最低 <paramref name="capacity"/> まで確保する。
        /// </summary>
        public static void EnsureStorageCapacity<TValue, TOptions, TAdapter>(int capacity)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            // FloatMotionAdapter などの組み込み Adapter は共有 storage で動くため、そちらを確保する。
            if (BuiltInMotionAdapter<TValue, TAdapter>.IsBuiltIn) MotionScheduler.EnsureCapacity(capacity);
            else MotionScheduler.EnsureAdapterCapacity<TValue, TOptions, TAdapter>(capacity);
        }

        private static void DefaultUnhandledExceptionHandler(Exception exception) => Debug.LogException(exception);
    }
}

using UnityEditor;

namespace MyMotion.Editor
{
    /// <summary>
    /// Editor で使える scheduler。LitMotion の EditorMotionScheduler と同じ名前で使う。
    /// </summary>
    public static class EditorMotionScheduler
    {
        /// <summary>
        /// EditorApplication.update で motion を進める scheduler。
        /// </summary>
        public static IMotionScheduler Update => EditorMotionDispatcher.Scheduler;
    }

    /// <summary>
    /// 専用の ManualMotionDispatcher を EditorApplication.update の経過時間で進める。
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorMotionDispatcher
    {
        private static ManualMotionDispatcher s_dispatcher;
        private static double s_lastTime;

        static EditorMotionDispatcher()
        {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
            EditorApplication.quitting += Dispose;
        }

        public static IMotionScheduler Scheduler
        {
            get
            {
                if (s_dispatcher == null)
                {
                    s_dispatcher = new ManualMotionDispatcher();
                    s_lastTime = EditorApplication.timeSinceStartup;
                }
                return s_dispatcher.Scheduler;
            }
        }

        internal static ManualMotionDispatcher Dispatcher => s_dispatcher;

        private static void Update()
        {
            if (s_dispatcher == null) return;
            double now = EditorApplication.timeSinceStartup;
            double delta = now - s_lastTime;
            s_lastTime = now;
            if (s_dispatcher.ActiveCount == 0 || !(delta > 0d)) return;
            s_dispatcher.Update(delta);
        }

        private static void Dispose()
        {
            if (s_dispatcher == null) return;
            s_dispatcher.Dispose();
            s_dispatcher = null;
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace MyMotion.Animation.Editor
{
    /// <summary>
    /// Inspector の Play で、Play Mode に入らずにアニメーションを再生する。
    /// PlayerLoop の World は Play Mode 中しか進まないため、プレビュー中だけ EditorApplication.update で進める。
    /// </summary>
    internal static class MyMotionAnimationPreview
    {
        private static bool s_running;
        private static double s_lastTime;

        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            s_lastTime = EditorApplication.timeSinceStartup;
            if (s_running) return;
            s_running = true;
            EditorApplication.update += Update;
        }

        public static void End()
        {
            if (!s_running) return;
            s_running = false;
            EditorApplication.update -= Update;
        }

        private static void Update()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                End();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            double delta = now - s_lastTime;
            s_lastTime = now;

            bool any = false;
            for (int i = 0; i <= (int)PlayerLoopTiming.TimeUpdate; i++)
            {
                var timing = (PlayerLoopTiming)i;
                if (!MotionScheduler.HasWorld(timing)) continue;
                any = true;
                if (delta > 0d) MotionScheduler.UpdateWorld(timing, delta, delta, delta);
            }

            if (!any)
            {
                End();
                return;
            }

            // Play Mode 外では Scene と Game ビューが自動で再描画されないため、変化を見せるために要求する。
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
    }
}

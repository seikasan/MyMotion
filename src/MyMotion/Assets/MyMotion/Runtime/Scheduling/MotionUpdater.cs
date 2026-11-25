using UnityEngine;

namespace MyMotion
{
    internal static class MotionUpdater
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetWorld()
        {
            MMotion.Reset();
            // World は NativeArray を Allocator.Persistent で持つ。PlayMode 終了・アプリ終了・assembly の再読み込みで
            // 解放しないと、Unity の leak 検出に残る。登録の重複を避けるため、一度外してから登録する。
            Application.quitting -= ReleaseNativeMemory;
            Application.quitting += ReleaseNativeMemory;
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= ReleaseNativeMemory;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ReleaseNativeMemory;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (Application.isPlaying)
            {
                MotionScheduler.InstallPlayerLoop();
            }
        }

        private static void ReleaseNativeMemory() => MMotion.ReleaseNativeMemory();

#if UNITY_EDITOR
        // Edit Mode で作った motion (テストなど) も、assembly の再読み込み前に解放する。
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditorRelease()
        {
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= ReleaseNativeMemory;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ReleaseNativeMemory;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange change)
        {
            if (change == UnityEditor.PlayModeStateChange.EnteredEditMode) ReleaseNativeMemory();
        }
#endif
    }
}

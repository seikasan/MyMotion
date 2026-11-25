using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MyMotion.Editor
{
    /// <summary>
    /// 追跡中の motion を一覧するウィンドウ。Window > MyMotion > Motion Debugger。
    /// </summary>
    public sealed class MotionDebuggerWindow : EditorWindow
    {
        private const string EnabledKey = "MyMotion.MotionDebugger.Enabled";
        private const string StackTraceKey = "MyMotion.MotionDebugger.EnableStackTrace";

        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private int _selected = -1;
        private string _search = string.Empty;
        private readonly List<int> _visible = new();

        [MenuItem("Window/MyMotion/Motion Debugger")]
        public static void Open()
        {
            var window = GetWindow<MotionDebuggerWindow>();
            window.titleContent = new GUIContent("Motion Debugger");
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void RestorePreferences()
        {
            MotionDebugger.Enabled = EditorPrefs.GetBool(EnabledKey, false);
            MotionDebugger.EnableStackTrace = EditorPrefs.GetBool(StackTraceKey, false);
        }

        private void OnEnable() => EditorApplication.update += Repaint;
        private void OnDisable() => EditorApplication.update -= Repaint;

        private void OnGUI()
        {
            MotionDebugger.RemoveInactive();
            DrawToolbar();

            IReadOnlyList<MotionDebugger.TrackingState> items = MotionDebugger.Items;
            _visible.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(_search) || Describe(items[i]).IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _visible.Add(i);
                }
            }

            float listHeight = position.height * 0.6f;
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.Height(listHeight));
            DrawHeader();
            foreach (int index in _visible)
            {
                DrawRow(index, items[index]);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            if (_selected >= 0 && _selected < items.Count)
            {
                DrawDetail(items[_selected]);
            }
            else if (!MotionDebugger.Enabled)
            {
                EditorGUILayout.HelpBox("Enable Tracking をオンにすると、以降に Bind した motion を表示します。", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                bool enabled = GUILayout.Toggle(MotionDebugger.Enabled, "Enable Tracking", EditorStyles.toolbarButton);
                if (enabled != MotionDebugger.Enabled)
                {
                    MotionDebugger.Enabled = enabled;
                    EditorPrefs.SetBool(EnabledKey, enabled);
                }
                bool stackTrace = GUILayout.Toggle(MotionDebugger.EnableStackTrace, "Enable Stack Trace", EditorStyles.toolbarButton);
                if (stackTrace != MotionDebugger.EnableStackTrace)
                {
                    MotionDebugger.EnableStackTrace = stackTrace;
                    EditorPrefs.SetBool(StackTraceKey, stackTrace);
                }
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                {
                    MotionDebugger.Clear();
                    _selected = -1;
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label("Active " + MMotion.ActiveCount + " / Tracked " + MotionDebugger.Items.Count, EditorStyles.miniLabel);
                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.Width(180f));
            }
        }

        private static void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(220f));
                GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(90f));
                GUILayout.Label("State", EditorStyles.boldLabel, GUILayout.Width(70f));
                GUILayout.Label("Time", EditorStyles.boldLabel, GUILayout.Width(120f));
                GUILayout.Label("Loops", EditorStyles.boldLabel, GUILayout.Width(60f));
            }
        }

        private void DrawRow(int index, MotionDebugger.TrackingState state)
        {
            MotionHandle handle = state.Handle;
            bool active = handle.IsActive();
            Rect rect = EditorGUILayout.BeginHorizontal();
            if (index == _selected) EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.9f, 0.35f));
            GUILayout.Label(state.DebugName ?? handle.ToString(), GUILayout.Width(220f));
            GUILayout.Label(state.ValueType?.Name ?? "-", GUILayout.Width(90f));
            GUILayout.Label(!active ? "Ended" : handle.IsPlaying() ? "Playing" : "Waiting", GUILayout.Width(70f));
            GUILayout.Label(active ? handle.Time.ToString("F2") + " / " + handle.TotalDuration.ToString("F2") : "-", GUILayout.Width(120f));
            GUILayout.Label(active ? handle.CompletedLoops + " / " + (handle.Loops < 0 ? "∞" : handle.Loops.ToString()) : "-", GUILayout.Width(60f));
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                _selected = index;
                Event.current.Use();
            }
        }

        private static void DrawDetail(MotionDebugger.TrackingState state)
        {
            EditorGUILayout.LabelField("Name", state.DebugName ?? state.Handle.ToString());
            EditorGUILayout.LabelField("Value Type", state.ValueType?.FullName ?? "-");
            EditorGUILayout.LabelField("Scheduler", state.Scheduler?.GetType().Name ?? "-");
            EditorGUILayout.LabelField("Created", state.CreatedOnEditor ? "Edit Mode" : "Play Mode");
            if (state.Handle.IsActive())
            {
                EditorGUILayout.LabelField("Delay / Duration", state.Handle.Delay.ToString("F3") + " / " + state.Handle.Duration.ToString("F3"));
                EditorGUILayout.LabelField("Playback Speed", state.Handle.PlaybackSpeed.ToString("F2"));
            }
            if (state.StackTrace != null)
            {
                EditorGUILayout.LabelField("Stack Trace", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(FormatStackTrace(state.StackTrace), EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static string FormatStackTrace(StackTrace stackTrace)
        {
            var builder = new StringBuilder();
            foreach (StackFrame frame in stackTrace.GetFrames() ?? System.Array.Empty<StackFrame>())
            {
                MethodBase method = frame.GetMethod();
                if (method == null) continue;
                builder.Append(method.DeclaringType?.FullName).Append('.').Append(method.Name);
                string file = frame.GetFileName();
                if (!string.IsNullOrEmpty(file)) builder.Append(" (").Append(file).Append(':').Append(frame.GetFileLineNumber()).Append(')');
                builder.AppendLine();
            }
            return builder.ToString();
        }

        private static string Describe(MotionDebugger.TrackingState state) =>
            (state.DebugName ?? string.Empty) + " " + (state.ValueType?.Name ?? string.Empty);
    }
}

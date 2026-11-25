// Reproduces the measurements of AnnulusGames/TweenPerformance (the benchmark published in the
// LitMotion README) in a Release IL2CPP player, one library and one case per process.
//
// Command line: -tpLibrary <name> -tpCase <startup_float|update_float|startup_position|update_position|gc_position> -tpOut <csv path>
//
// The per-library benchmark bodies follow TweenPerformance (Benchmarks/FloatProperty and Benchmarks/Position);
// MyMotion uses the same calls as LitMotion with MMotion in place of LMotion.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DG.Tweening;
using Kovnir.FastTweener;
using Kovnir.FastTweener.Extension;
using LitMotion;
using LitMotion.Extensions;
using PrimeTween;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace TweenPerformanceBench
{
    public sealed class TestClass
    {
        public float Value { get; set; }
    }

    public sealed class TweenPerformanceRunner : MonoBehaviour
    {
        private const int FloatCount = 64000;
        private const int PositionCount = 50000;
        private const int WarmupFrames = 3;
        private const int MeasureFrames = 600;

        private string _library;
        private string _case;
        private string _out;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string[] args = Environment.GetCommandLineArgs();
            string library = Arg(args, "-tpLibrary");
            if (library == null) return;
            var go = new GameObject("TweenPerformanceRunner");
            DontDestroyOnLoad(go);
            var runner = go.AddComponent<TweenPerformanceRunner>();
            runner._library = library;
            runner._case = Arg(args, "-tpCase");
            runner._out = Arg(args, "-tpOut");
        }

        private static string Arg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }

        private IEnumerator Start()
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            Application.runInBackground = true;
            yield return null;

            bool position = _case.EndsWith("_position", StringComparison.Ordinal);
            int count = position ? PositionCount : FloatCount;
            var targets = new TestClass[count];
            Transform[] transforms = null;
            if (position)
            {
                transforms = new Transform[count];
                for (int i = 0; i < count; i++) transforms[i] = new GameObject().transform;
            }
            else
            {
                for (int i = 0; i < count; i++) targets[i] = new TestClass();
            }
            yield return null;

            Setup(count);
            yield return null;
            GC.Collect();
            yield return null;

            double value;
            string unit;
            if (_case.StartsWith("startup", StringComparison.Ordinal))
            {
                long start = Stopwatch.GetTimestamp();
                Run(targets, transforms, position);
                long end = Stopwatch.GetTimestamp();
                value = (end - start) * 1000.0 / Stopwatch.Frequency;
                unit = "ms";
            }
            else if (_case.StartsWith("update", StringComparison.Ordinal))
            {
                Run(targets, transforms, position);
                for (int i = 0; i < WarmupFrames; i++) yield return null;
                var samples = new List<double>(MeasureFrames);
                for (int i = 0; i < MeasureFrames; i++)
                {
                    yield return null;
                    samples.Add(Time.unscaledDeltaTime * 1000.0);
                }
                samples.Sort();
                value = samples[samples.Count / 2];
                unit = "ms";
            }
            else
            {
                long prev = GC.GetTotalMemory(true);
                Run(targets, transforms, position);
                long current = GC.GetTotalMemory(true);
                value = (double)(current - prev) / count;
                unit = "B";
            }

            bool valid = position
                ? transforms[count - 1].position != Vector3.zero || _case.StartsWith("startup", StringComparison.Ordinal) || _case.StartsWith("gc", StringComparison.Ordinal)
                : true;
            string line = string.Join(",", _library, _case, count.ToString(CultureInfo.InvariantCulture),
                value.ToString("F4", CultureInfo.InvariantCulture), unit, valid ? "True" : "False");
            File.AppendAllText(_out, line + "\n");
            Debug.Log("TWEEN_PERFORMANCE " + line);
            Application.Quit(0);
        }

        private void Setup(int count)
        {
            switch (_library)
            {
                case "MyMotion":
                    MyMotion.MotionDispatcher.EnsureStorageCapacity(count);
                    break;
                case "LitMotion":
                    LitMotion.MotionDispatcher.EnsureStorageCapacity<float, NoOptions, LitMotion.Adapters.FloatMotionAdapter>(count);
                    LitMotion.MotionDispatcher.EnsureStorageCapacity<Vector3, NoOptions, LitMotion.Adapters.Vector3MotionAdapter>(count);
                    break;
                case "PrimeTween":
                    PrimeTweenConfig.SetTweensCapacity(count);
                    break;
                case "DOTween":
                    DOTween.Init().SetCapacity(count + 1, 0);
                    break;
                case "LeanTween":
                    LeanTween.init(count);
                    break;
                case "FastTweener":
                    FastTweener.Init(new FastTweenerSettings
                    {
                        DefaultEase = Kovnir.FastTweener.Ease.Linear,
                        TaskPoolSize = count,
                        TransformExtensionsPoolSize = count,
                    });
                    break;
                default:
                    throw new ArgumentException(_library);
            }
        }

        private void Run(TestClass[] targets, Transform[] transforms, bool position)
        {
            if (!position)
            {
                switch (_library)
                {
                    case "MyMotion":
                        foreach (var target in targets) MyMotion.MMotion.Create(0f, 10f, 100f).Bind(target, (x, target) => target.Value = x);
                        break;
                    case "LitMotion":
                        foreach (var target in targets) LMotion.Create(0f, 10f, 100f).Bind(target, (x, target) => target.Value = x);
                        break;
                    case "PrimeTween":
                        foreach (var target in targets) PrimeTween.Tween.Custom(target, 0f, 10f, 100f, (obj, x) => obj.Value = x);
                        break;
                    case "DOTween":
                        foreach (var target in targets) DOTween.To(() => target.Value, x => target.Value = x, 10f, 100f);
                        break;
                    case "LeanTween":
                        foreach (var target in targets) LeanTween.value(0f, 10f, 100f).setOnUpdate(x => target.Value = x);
                        break;
                    case "FastTweener":
                        foreach (var target in targets) FastTweener.Float(0f, 10f, 100f, x => target.Value = x);
                        break;
                }
                return;
            }

            switch (_library)
            {
                case "MyMotion":
                    foreach (var transform in transforms) MyMotion.Extensions.MotionTransformExtensions.BindToPosition(MyMotion.MMotion.Create(Vector3.zero, Vector3.one, 100f), transform);
                    break;
                case "LitMotion":
                    foreach (var transform in transforms) LMotion.Create(Vector3.zero, Vector3.one, 100f).BindToPosition(transform);
                    break;
                case "PrimeTween":
                    foreach (var transform in transforms) PrimeTween.Tween.Position(transform, Vector3.zero, Vector3.one, 100f);
                    break;
                case "DOTween":
                    foreach (var transform in transforms) transform.DOMove(Vector3.one, 100f);
                    break;
                case "LeanTween":
                    foreach (var transform in transforms) LeanTween.move(transform.gameObject, Vector3.positiveInfinity, 100f);
                    break;
                case "FastTweener":
                    foreach (var transform in transforms) transform.TweenMove(Vector3.one, 100f);
                    break;
            }
        }
    }
}

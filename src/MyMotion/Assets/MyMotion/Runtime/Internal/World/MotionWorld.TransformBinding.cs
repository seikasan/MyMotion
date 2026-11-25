using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace MyMotion.Internal
{
    /// <summary>
    /// Burst の TransformAccess で直接書き込める Transform のプロパティ。
    /// </summary>
    internal enum TransformTargetKind : byte
    {
        Position,
        LocalPosition,
        LocalScale,
        EulerAngles,
        LocalEulerAngles,
        Rotation,
        LocalRotation,
        PositionX,
        PositionY,
        PositionZ,
        LocalPositionX,
        LocalPositionY,
        LocalPositionZ,
        LocalScaleX,
        LocalScaleY,
        LocalScaleZ,
        LocalScaleXYZ,
    }

    internal sealed partial class MotionWorld
    {
        // 実測 (docs/measurements/transform-job-binding-20261123.md) では、TransformAccessArray への登録が
        // 1件約0.2µs、Job の固定費が数µsかかる。managed Binding が速い少数件では Job を使わず、
        // 対象が多い間だけ「Job モード」に入り、十分長く生きる motion だけを少しずつ Job へ移す。
        // 2026-11-24: Binding ループの改善後は managed Binding のほうが 50,000 件まで LitMotion に対して安定して速く、
        // Job への移行費用で lifecycle が負けるため、既定では Job モードに入らない (docs/measurements/binding-loop-cost-20261124.md)。
        // 経路と検証は残し、閾値を下げれば有効になる。
#if MYMOTION_THRESHOLD_BENCHMARK
        // 閾値測定用ビルドでは環境変数で変えられる (0 以下で無効)。
        internal int TransformJobEnterCount = GetEnvInt("MYMOTION_TRANSFORM_JOB_ENTER", 0) is int enter && enter > 0 ? enter : int.MaxValue;
#else
        internal int TransformJobEnterCount = int.MaxValue;
#endif
        internal int TransformJobExitCount = 4096;
        internal int TransformMigrationBudget = 4096;
        // 登録費用（約0.2µs/件）を Job の節約（15,000件で約14ns/件/フレーム、50,000件で約5ns）で
        // 十分回収できる残りフレーム数。これより早く終わる motion は managed のままにする。
        internal double TransformMinRemainingFrames = 120.0;

        private struct TransformLane
        {
            public int DenseIndex;
            public TransformTargetKind Kind;
        }

        private struct PendingTransformTarget
        {
            public int Slot;
            public uint Version;
        }

        private TransformAccessArray _transformTargets;
        private NativeList<TransformLane> _transformLanes;
        private NativeArray<byte> _transformInvalid;
        private PendingTransformTarget[] _transformPending;
        private int _transformPendingCount;
        private int _transformEligibleCount;
        private bool _transformJobMode;
        // 直近の Update で Job が書き込みを担当したか。Binding ループで managed 呼び出しを省く判定に使う。
        private bool _transformJobBound;

        internal int TransformTargetCount => _transformLanes.IsCreated ? _transformLanes.Length : 0;
        internal int TransformEligibleCount => _transformEligibleCount;

        /// <summary>
        /// managed Binding はそのまま残し、Job モード中の Update では書き込みを Burst の Transform Job に任せる。
        /// Complete、TrySetTime、Sequence など同期評価の経路は常に managed Binding を使う。
        /// </summary>
        public void AttachTransformTarget(CoreMotionHandle handle, Transform transform, TransformTargetKind kind)
        {
            if (TransformJobEnterCount == int.MaxValue) return;
            if (transform == null || !TryGetIndex(handle, out int index)) return;
            ref Slot slot = ref _slots[handle.Slot];
            if (slot.TransformKindPlusOne != 0) return;
            if (!IsCompatible(_items[index].ValueType, kind)) return;

            slot.TransformKindPlusOne = (byte)(kind + 1);
            _transformEligibleCount++;
            _transformPending ??= new PendingTransformTarget[64];
            if (_transformPendingCount == _transformPending.Length) CompactOrGrowTransformPending();
            _transformPending[_transformPendingCount++] = new PendingTransformTarget { Slot = handle.Slot, Version = handle.Version };
        }

        private static bool IsCompatible(MotionValueType valueType, TransformTargetKind kind) => kind switch
        {
            TransformTargetKind.Position or TransformTargetKind.LocalPosition or TransformTargetKind.LocalScale
                or TransformTargetKind.EulerAngles or TransformTargetKind.LocalEulerAngles => valueType == MotionValueType.Vector3,
            TransformTargetKind.Rotation or TransformTargetKind.LocalRotation => valueType == MotionValueType.Quaternion,
            _ => valueType == MotionValueType.Float,
        };

        private void CompactOrGrowTransformPending()
        {
            int write = 0;
            for (int i = 0; i < _transformPendingCount; i++)
            {
                PendingTransformTarget pending = _transformPending[i];
                if (IsPendingAlive(pending)) _transformPending[write++] = pending;
            }
            _transformPendingCount = write;
            if (write * 2 >= _transformPending.Length) System.Array.Resize(ref _transformPending, _transformPending.Length * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPendingAlive(in PendingTransformTarget pending)
        {
            Slot slot = _slots[pending.Slot];
            return slot.Version == pending.Version && slot.DenseIndex >= 0 && slot.TransformKindPlusOne != 0 && slot.TransformLanePlusOne == 0;
        }

        // RemoveAt から呼ぶ。dense 要素が動く前に lane を外す。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseTransformTarget(int slotIndex)
        {
            ref Slot slot = ref _slots[slotIndex];
            if (slot.TransformKindPlusOne == 0) return;
            slot.TransformKindPlusOne = 0;
            _transformEligibleCount--;
            int lanePlusOne = slot.TransformLanePlusOne;
            if (lanePlusOne == 0) return;
            int lane = lanePlusOne - 1;
            int last = _transformLanes.Length - 1;
            if (lane != last)
            {
                int movedSlot = _items[_transformLanes[last].DenseIndex].Slot;
                _slots[movedSlot].TransformLanePlusOne = lane + 1;
            }
            _transformTargets.RemoveAtSwapBack(lane);
            _transformLanes.RemoveAtSwapBack(lane);
            slot.TransformLanePlusOne = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveTransformLaneDenseIndex(int slotIndex, int newDenseIndex)
        {
            int lanePlusOne = _slots[slotIndex].TransformLanePlusOne;
            if (lanePlusOne == 0) return;
            ref TransformLane lane = ref _transformLanes.ElementAt(lanePlusOne - 1);
            lane.DenseIndex = newDenseIndex;
        }

        private void RunTransformBindingJob(double deltaTime, double unscaledDeltaTime, double realDeltaTime, bool recordsWritten)
        {
            _transformJobBound = false;
            if (_transformEligibleCount == 0 && !_transformJobMode) return;

            if (!_transformJobMode)
            {
                if (_transformEligibleCount < TransformJobEnterCount) return;
                _transformJobMode = true;
                if (!_transformLanes.IsCreated)
                {
                    _transformTargets = new TransformAccessArray(TransformJobEnterCount);
                    _transformLanes = new NativeList<TransformLane>(TransformJobEnterCount, Allocator.Persistent);
                }
            }
            else if (_transformEligibleCount < TransformJobExitCount)
            {
                LeaveTransformJobMode();
                return;
            }

            MigrateTransformTargets(deltaTime, unscaledDeltaTime, realDeltaTime);
            int count = _transformLanes.Length;
            if (count == 0) return;

            if (!_transformInvalid.IsCreated || _transformInvalid.Length < count)
            {
                if (_transformInvalid.IsCreated) _transformInvalid.Dispose();
                _transformInvalid = new NativeArray<byte>(math.ceilpow2(count), Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }

            // 破棄済み Transform の要素では Execute が呼ばれないため、既定を「未書き込み」にしておく。
            unsafe { UnsafeUtility.MemSet(_transformInvalid.GetUnsafePtr(), 1, count); }

            var job = new TransformBindingJob
            {
                Items = _items,
                WideItems = _wideItems,
                Lanes = _transformLanes.AsArray(),
                Invalid = _transformInvalid,
                Records = _bindRecords,
                UseRecords = recordsWritten,
            };
            job.Schedule(_transformTargets).Complete();
            _transformJobBound = true;
        }

        private void MigrateTransformTargets(double deltaTime, double unscaledDeltaTime, double realDeltaTime)
        {
            int budget = TransformMigrationBudget;
            int read = _transformPendingCount;
            while (read > 0 && budget > 0)
            {
                PendingTransformTarget pending = _transformPending[--read];
                budget--;
                if (!IsPendingAlive(pending)) continue;
                Slot slot = _slots[pending.Slot];
                Data item = _items[slot.DenseIndex];
                if (!LivesLongEnough(item, deltaTime, unscaledDeltaTime, realDeltaTime)) continue;
                if (!(_states[slot.DenseIndex] is Transform transform) || transform == null) continue;

                _transformTargets.Add(transform);
                _transformLanes.Add(new TransformLane { DenseIndex = slot.DenseIndex, Kind = (TransformTargetKind)(slot.TransformKindPlusOne - 1) });
                _slots[pending.Slot].TransformLanePlusOne = _transformLanes.Length;
            }
            _transformPendingCount = read;
        }

        private bool LivesLongEnough(in Data item, double deltaTime, double unscaledDeltaTime, double realDeltaTime)
        {
            if (item.Loops < 0) return true;
            double delta = item.TimeKind switch
            {
                MotionTimeKind.UnscaledTime => unscaledDeltaTime,
                MotionTimeKind.Realtime => realDeltaTime,
                _ => deltaTime
            } * item.PlaybackSpeed;
            // 停止中や巻き戻し中も毎フレーム書き込むため、Job に載せる価値がある。
            if (!(delta > 0.0)) return true;
            double total = (double)item.Delay * (item.DelayEveryLoop ? item.Loops : 1) + (double)item.Duration * item.Loops;
            return (total - item.Elapsed) / delta >= TransformMinRemainingFrames;
        }

        private void LeaveTransformJobMode()
        {
            _transformJobMode = false;
            if (!_transformLanes.IsCreated) return;
            for (int lane = 0; lane < _transformLanes.Length; lane++)
            {
                int slotIndex = _items[_transformLanes[lane].DenseIndex].Slot;
                _slots[slotIndex].TransformLanePlusOne = 0;
                if (_transformPendingCount == _transformPending.Length) CompactOrGrowTransformPending();
                _transformPending[_transformPendingCount++] = new PendingTransformTarget { Slot = slotIndex, Version = _slots[slotIndex].Version };
            }
            _transformTargets.Dispose();
            _transformLanes.Dispose();
            if (_transformInvalid.IsCreated) _transformInvalid.Dispose();
        }

        // Job が書き込めなかった (Transform 破棄済み) lane は managed Binding に戻し、LitMotion と同じ例外経路を通す。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBoundByTransformJob(int slotIndex)
        {
            int lanePlusOne = _slots[slotIndex].TransformLanePlusOne;
            return lanePlusOne != 0 && _transformInvalid[lanePlusOne - 1] == 0;
        }

        private void DisposeTransformTargets()
        {
            if (_transformTargets.isCreated) _transformTargets.Dispose();
            if (_transformLanes.IsCreated) _transformLanes.Dispose();
            if (_transformInvalid.IsCreated) _transformInvalid.Dispose();
            _transformPending = null;
            _transformPendingCount = 0;
            _transformEligibleCount = 0;
            _transformJobMode = false;
            _transformJobBound = false;
        }

        [BurstCompile(
            CompileSynchronously = true,
            OptimizeFor = OptimizeFor.Performance,
            FloatPrecision = FloatPrecision.Standard,
            FloatMode = FloatMode.Strict)]
        private unsafe struct TransformBindingJob : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<Data> Items;
            [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<WideData> WideItems;
            [ReadOnly] public NativeArray<TransformLane> Lanes;
            [NativeDisableParallelForRestriction] public NativeArray<byte> Invalid;
            // Update Job が書いた Binding 用の記録。書いた要素は Binding の印を外し、main thread の記録経路で飛ばさせる。
            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] public NativeArray<BindRecord> Records;
            public bool UseRecords;

            public void Execute(int index, TransformAccess transform)
            {
                TransformLane lane = Lanes[index];
                if (UseRecords)
                {
                    // 記録 (20 byte) だけを読む。Binding するかの判定は Update Job が記録に書いた結果と同じ。
                    ref BindRecord record = ref ((BindRecord*)Records.GetUnsafePtr())[lane.DenseIndex];
                    if ((record.Bits & RecordShouldBind) == 0 || !transform.isValid)
                    {
                        // 書かない。破棄済み Transform は main thread の managed Binding に任せ、LitMotion と同じ例外経路を通す。
                        Invalid[index] = 1;
                        return;
                    }
                    Write(transform, lane.Kind, record.Value.Floats);
                    record.Bits = (byte)(record.Bits & ~RecordShouldBind);
                    Invalid[index] = 0;
                    return;
                }
                Data item = ((Data*)Items.GetUnsafeReadOnlyPtr())[lane.DenseIndex];
                // Update の Binding ループと同じ条件でだけ書く。
                bool skip = item.IsCanceled || item.IsInSequence || (item.IsCompletionNotified && !item.IsPreserved)
                    || !item.HasBinding || (!item.IsComplete && item.IsDelayed && item.SkipValuesDuringDelay);
                if (skip)
                {
                    Invalid[index] = 0;
                    return;
                }
                if (!transform.isValid)
                {
                    Invalid[index] = 1;
                    return;
                }
                Invalid[index] = 0;

                float4 value = lane.Kind == TransformTargetKind.Rotation || lane.Kind == TransformTargetKind.LocalRotation
                    ? WideItems[item.WideIndex].Value.Floats
                    : new float4(item.Value, 0f);
                Write(transform, lane.Kind, value);
            }

            private static void Write(TransformAccess transform, TransformTargetKind kind, float4 value)
            {
                float3 v = value.xyz;
                switch (kind)
                {
                    case TransformTargetKind.Position: transform.position = v; break;
                    case TransformTargetKind.LocalPosition: transform.localPosition = v; break;
                    case TransformTargetKind.LocalScale: transform.localScale = v; break;
                    case TransformTargetKind.EulerAngles: transform.rotation = quaternion.EulerZXY(math.radians(v)); break;
                    case TransformTargetKind.LocalEulerAngles: transform.localRotation = quaternion.EulerZXY(math.radians(v)); break;
                    case TransformTargetKind.Rotation: transform.rotation = new quaternion(value); break;
                    case TransformTargetKind.LocalRotation: transform.localRotation = new quaternion(value); break;
                    case TransformTargetKind.PositionX: { Vector3 p = transform.position; p.x = v.x; transform.position = p; break; }
                    case TransformTargetKind.PositionY: { Vector3 p = transform.position; p.y = v.x; transform.position = p; break; }
                    case TransformTargetKind.PositionZ: { Vector3 p = transform.position; p.z = v.x; transform.position = p; break; }
                    case TransformTargetKind.LocalPositionX: { Vector3 p = transform.localPosition; p.x = v.x; transform.localPosition = p; break; }
                    case TransformTargetKind.LocalPositionY: { Vector3 p = transform.localPosition; p.y = v.x; transform.localPosition = p; break; }
                    case TransformTargetKind.LocalPositionZ: { Vector3 p = transform.localPosition; p.z = v.x; transform.localPosition = p; break; }
                    case TransformTargetKind.LocalScaleX: { Vector3 s = transform.localScale; s.x = v.x; transform.localScale = s; break; }
                    case TransformTargetKind.LocalScaleY: { Vector3 s = transform.localScale; s.y = v.x; transform.localScale = s; break; }
                    case TransformTargetKind.LocalScaleZ: { Vector3 s = transform.localScale; s.z = v.x; transform.localScale = s; break; }
                    case TransformTargetKind.LocalScaleXYZ: transform.localScale = new Vector3(v.x, v.x, v.x); break;
                }
            }
        }
    }
}

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MyMotion.Internal
{
    /// <summary>
    /// Binding ループ用の小さな記録 (hot/cold 分割)。
    /// Update Job が要素ごとに「Binding するか・通知が要るか・値」を 20 バイトにまとめて書き、
    /// main thread はこれだけを順に読む。88 バイトの Data と 56 バイトの WideData を main thread に運ばずに済み、
    /// 並列 Job が書いた cache line を main thread が取りに行く量も減る。
    ///
    /// 記録は Job 実行時点の状態の写しなので、Binding や callback の利用者コードが World に触れた時点
    /// (MotionWorld の入口が呼ぶ <see cref="CompletePipeline"/>) で無効にし、以降の要素は Data から読む。
    /// </summary>
    internal sealed partial class MotionWorld
    {
        internal struct BindRecord
        {
            public MotionValue Value;
            public ushort Flags;
            public MotionValueType ValueType;
            public byte Bits;
        }

        internal const byte RecordShouldBind = 1;
        internal const byte RecordNeedsEvent = 2;

        private NativeArray<BindRecord> _bindRecords;
        private bool _bindRecordsValid;

        private void EnsureBindRecords(int count)
        {
            if (_bindRecords.IsCreated && _bindRecords.Length >= count) return;
            if (_bindRecords.IsCreated) _bindRecords.Dispose();
            _bindRecords = new NativeArray<BindRecord>(math.max(count, _items.Length), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        private void DisposeBindRecords()
        {
            if (_bindRecords.IsCreated) _bindRecords.Dispose();
            _bindRecordsValid = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void WriteBindRecord(ref Data item, BindRecord* records, WideData* wideItems, int index)
        {
            bool skip = item.IsCanceled || item.IsInSequence || (item.IsCompletionNotified && !item.IsPreserved);
            byte bits = 0;
            if (!skip)
            {
                if (item.HasBinding && (item.IsComplete || !item.IsDelayed || !item.SkipValuesDuringDelay)) bits |= RecordShouldBind;
                if ((!item.IsCompletionNotified && item.CompletedLoops > item.PreviousCompletedLoops)
                    || item.IsComplete != item.IsCompletionNotified)
                {
                    bits |= RecordNeedsEvent;
                }
            }
            ref BindRecord record = ref records[index];
            record.Bits = bits;
            if ((bits & RecordShouldBind) == 0) return;
            record.Flags = item.Flags;
            record.ValueType = item.ValueType;
            if (item.WideIndex >= 0) record.Value = wideItems[item.WideIndex].Value;
            else record.Value.Floats = new float4(item.Value, 0f);
        }

        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        private void BindFromRecord(int index, in BindRecord record)
        {
            MotionValue v = record.Value;
            ushort flags = record.Flags;
            switch (record.ValueType)
            {
                case MotionValueType.Float: InvokeFromFlags(index, flags, v.Floats.x); break;
                case MotionValueType.Vector3: InvokeFromFlags(index, flags, new Vector3(v.Floats.x, v.Floats.y, v.Floats.z)); break;
                case MotionValueType.Double: InvokeFromFlags(index, flags, v.Double); break;
                case MotionValueType.Int: InvokeFromFlags(index, flags, (int)v.Integer); break;
                case MotionValueType.Long: InvokeFromFlags(index, flags, v.Integer); break;
                case MotionValueType.Vector2: InvokeFromFlags(index, flags, new Vector2(v.Floats.x, v.Floats.y)); break;
                case MotionValueType.Vector4: InvokeFromFlags(index, flags, new Vector4(v.Floats.x, v.Floats.y, v.Floats.z, v.Floats.w)); break;
                case MotionValueType.Quaternion: InvokeFromFlags(index, flags, new Quaternion(v.Floats.x, v.Floats.y, v.Floats.z, v.Floats.w)); break;
                case MotionValueType.Color: InvokeFromFlags(index, flags, new Color(v.Floats.x, v.Floats.y, v.Floats.z, v.Floats.w)); break;
                case MotionValueType.Rect: InvokeFromFlags(index, flags, new Rect(v.Floats.x, v.Floats.y, v.Floats.z, v.Floats.w)); break;
            }
        }

        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        private void InvokeFromFlags<TValue>(int index, ushort flags, TValue value)
        {
            switch ((flags & Data.StateCountMask) >> 12)
            {
                case 0:
                    InvokeBinding(_callbacks[index], value);
                    break;
                case 1:
                    if ((flags & Data.IsValueStateBindingFlag) != 0)
                    {
                        ((ValueStateBindingStorageBase<TValue>)_states[index]).Invoke(value, _slots[_items[index].Slot].ValueStateIndex);
                    }
                    else
                    {
                        InvokeStateful(_callbacks[index], value, _states[index]);
                    }
                    break;
                case 2:
                    InvokeStateful(_callbacks[index], value, _states[index], _states1[index]);
                    break;
                case 3:
                    InvokeStateful(_callbacks[index], value, _states[index], _states1[index], _states2[index]);
                    break;
            }
        }
    }
}

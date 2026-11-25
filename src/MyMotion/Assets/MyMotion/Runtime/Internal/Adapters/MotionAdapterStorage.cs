using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace MyMotion.Internal
{
    internal abstract class MotionAdapterStorageBase : IDisposable
    {
        public ushort StorageIndex { get; set; }
        public abstract long ReservedBytes { get; }
        public abstract void RemoveAt(int index, MotionWorld world);
        public abstract void SetCoreDenseIndex(int index, int denseIndex);
        public abstract void Update(
            NativeArray<MotionWorld.Data> coreItems,
            double deltaTime,
            double unscaledDeltaTime,
            double realDeltaTime);
        public abstract void EvaluateAt(int index, in MotionEvaluationContext context);
        public abstract void Bind(
            int index,
            in MotionEvaluationContext context,
            object callback,
            object state0,
            object state1,
            object state2,
            byte stateCount,
            int slot,
            bool isValueStateBinding);
        public abstract void Dispose();
    }

    internal sealed class MotionAdapterStorage<TValue, TOptions, TAdapter> : MotionAdapterStorageBase
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
#if !MYMOTION_THRESHOLD_BENCHMARK
        private const int AdapterPayloadSizeThresholdBytes = 16;
        private const int LargePayloadParallelThreshold = 256;
        private const int VeryLargePayloadSizeThresholdBytes = 4096;
        private const int VeryLargePayloadParallelThreshold = 128;
#endif

        private struct Item
        {
            public TValue Start;
            public TValue End;
            public TOptions Options;
            public TValue Output;
            public MotionEvaluationContext Context;
            public int Slot;
            public int CoreDenseIndex;
            public bool HasOutput;
        }

        [BurstCompile(
            CompileSynchronously = true,
            OptimizeFor = OptimizeFor.Performance,
            FloatPrecision = FloatPrecision.Standard,
            FloatMode = FloatMode.Strict)]
        private struct EvaluateJob : IJobParallelForBatch
        {
            public NativeArray<Item> Items;
            [NativeDisableParallelForRestriction]
            public NativeArray<MotionWorld.Data> CoreItems;
            public double DeltaTime;
            public double UnscaledDeltaTime;
            public double RealDeltaTime;

            public unsafe void Execute(int startIndex, int count)
            {
                // TValue は大きな FixedString になり得る。Start・End・Options・Output を毎フレーム
                // ローカルの Item 経由でコピーせず、ネイティブ配列の要素をその場で更新する。
                Item* items = (Item*)Items.GetUnsafePtr();
                // adapter の駆動 motion は常に float なので、float 経路で Data をその場で更新する。
                MotionWorld.Data* coreItems = (MotionWorld.Data*)CoreItems.GetUnsafePtr();
                int endIndex = startIndex + count;
                for (int i = startIndex; i < endIndex; i++)
                {
                    ref Item item = ref items[i];
                    ref MotionWorld.Data driver = ref coreItems[item.CoreDenseIndex];
                    MotionWorld.AdvanceAdapterItem(
                        ref driver,
                        DeltaTime,
                        UnscaledDeltaTime,
                        RealDeltaTime);

                    bool shouldEvaluate = !driver.IsCanceled
                        && !driver.IsInSequence
                        && !(driver.IsCompletionNotified && !driver.IsPreserved)
                        && driver.HasBinding
                        && (driver.IsComplete || !driver.IsDelayed || !driver.SkipValuesDuringDelay);
                    if (shouldEvaluate && driver.Ease != MotionEase.CustomAnimationCurve)
                    {
                        item.Context = new MotionEvaluationContext { Progress = driver.Value.x, Time = driver.Elapsed };
                        item.Output = default(TAdapter).Evaluate(ref item.Start, ref item.End, ref item.Options, in item.Context);
                        item.HasOutput = true;
                    }

                }
            }
        }

        private NativeArray<Item> _items;
        private int _count;

        public MotionAdapterStorage(int capacity)
        {
            _items = new NativeArray<Item>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        public override long ReservedBytes => (long)_items.Length * UnsafeUtility.SizeOf<Item>();

        public int Add(int slot, int coreDenseIndex, in TValue start, in TValue end, in TOptions options)
        {
            EnsureCapacity(_count + 1);
            int index = _count++;
            _items[index] = new Item
            {
                Start = start,
                End = end,
                Options = options,
                Slot = slot,
                CoreDenseIndex = coreDenseIndex
            };
            return index;
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > _items.Length) EnsureCapacityForOneMore(capacity);
        }

        public override void RemoveAt(int index, MotionWorld world)
        {
            int last = --_count;
            if (index == last) return;

            Item moved = _items[last];
            _items[index] = moved;
            world.SetAdapterRoute(moved.Slot, this, index);
        }

        public override unsafe void SetCoreDenseIndex(int index, int denseIndex)
        {
            Item* items = (Item*)_items.GetUnsafePtr();
            items[index].CoreDenseIndex = denseIndex;
        }

        public override void Update(
            NativeArray<MotionWorld.Data> coreItems,
            double deltaTime,
            double unscaledDeltaTime,
            double realDeltaTime)
        {
            int count = _count;
            if (count == 0) return;

            var job = new EvaluateJob
            {
                Items = _items,
                CoreItems = coreItems,
                DeltaTime = deltaTime,
                UnscaledDeltaTime = unscaledDeltaTime,
                RealDeltaTime = realDeltaTime
            };
#if MYMOTION_THRESHOLD_BENCHMARK
            int parallelThreshold = MotionWorld.AdapterParallelThreshold;
#else
            // FixedString 値はより少ない件数から scheduling する利点があるが、切り替えの分岐点は
            // ペイロードサイズによって変わる。軽い値は既存の閾値のままにする。
            int valueSize = UnsafeUtility.SizeOf<TValue>();
            int parallelThreshold = valueSize >= VeryLargePayloadSizeThresholdBytes
                ? VeryLargePayloadParallelThreshold
                : valueSize > AdapterPayloadSizeThresholdBytes
                    ? LargePayloadParallelThreshold
                    : MotionWorld.AdapterParallelThreshold;
#endif
            if (count < parallelThreshold)
            {
                job.RunBatch(count);
            }
            else
            {
                job.ScheduleBatch(count, MotionWorld.AdapterParallelBatchSize).Complete();
            }
        }

        public override unsafe void EvaluateAt(int index, in MotionEvaluationContext context)
        {
            Item* items = (Item*)_items.GetUnsafePtr();
            ref Item item = ref items[index];
            item.Context = context;
            item.Output = default(TAdapter).Evaluate(ref item.Start, ref item.End, ref item.Options, in item.Context);
            item.HasOutput = true;
        }

        public override unsafe void Bind(
            int index,
            in MotionEvaluationContext context,
            object callback,
            object state0,
            object state1,
            object state2,
            byte stateCount,
            int stateIndex,
            bool isValueStateBinding)
        {
            Item* items = (Item*)_items.GetUnsafePtr();
            ref Item item = ref items[index];
            if (!item.HasOutput || item.Context.Progress != context.Progress || item.Context.Time != context.Time)
            {
                EvaluateAt(index, in context);
                items = (Item*)_items.GetUnsafePtr();
                item = ref items[index];
            }

            // callback に渡す値だけをコピーする。callback は motion を作成し、このストレージを拡張し得る。
            TValue output = item.Output;

            if (isValueStateBinding)
            {
                ((ValueStateBindingStorageBase<TValue>)state0).Invoke(output, stateIndex);
                return;
            }

            switch (stateCount)
            {
                case 0:
                    Invoke(callback, output);
                    break;
                case 1:
                    Invoke(callback, output, state0);
                    break;
                case 2:
                    Invoke(callback, output, state0, state1);
                    break;
                case 3:
                    Invoke(callback, output, state0, state1, state2);
                    break;
            }
        }

        public override void Dispose()
        {
            if (_items.IsCreated) _items.Dispose();
            _count = 0;
        }

        private void EnsureCapacityForOneMore(int required)
        {
            if (required <= _items.Length) return;

            int capacity = Math.Max(4, _items.Length);
            while (capacity < required) capacity = checked(capacity * 2);
            var next = new NativeArray<Item>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            NativeArray<Item>.Copy(_items, next, _count);
            _items.Dispose();
            _items = next;
        }

        private static void Invoke(object callback, TValue value)
        {
            Action<TValue> action = UnsafeUtility.As<object, Action<TValue>>(ref callback);
            action.Invoke(value);
        }

        private static void Invoke(object callback, TValue value, object state)
        {
            Action<TValue, object> action = UnsafeUtility.As<object, Action<TValue, object>>(ref callback);
            action.Invoke(value, state);
        }

        private static void Invoke(object callback, TValue value, object state0, object state1)
        {
            Action<TValue, object, object> action = UnsafeUtility.As<object, Action<TValue, object, object>>(ref callback);
            action.Invoke(value, state0, state1);
        }

        private static void Invoke(object callback, TValue value, object state0, object state1, object state2)
        {
            Action<TValue, object, object, object> action = UnsafeUtility.As<object, Action<TValue, object, object, object>>(ref callback);
            action.Invoke(value, state0, state1, state2);
        }
    }
}

using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
#if MYMOTION_PROFILE_MARKERS
using Unity.Profiling;
#endif

namespace MyMotion.Internal
{
    internal enum MotionEase : byte
    {
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        CustomAnimationCurve
    }

    internal enum MotionTimeKind : byte
    {
        Time,
        UnscaledTime,
        Realtime
    }

    internal enum MotionLoopType : byte
    {
        Restart,
        Flip,
        Incremental,
        Yoyo
    }

    internal enum MotionRoundingMode : byte
    {
        ToEven,
        AwayFromZero,
        ToZero,
        ToPositiveInfinity,
        ToNegativeInfinity
    }

    internal readonly struct CoreMotionHandle : IEquatable<CoreMotionHandle>
    {
        internal CoreMotionHandle(int owner, int slot, uint version)
        {
            Owner = owner;
            Slot = slot;
            Version = version;
        }

        internal int Owner { get; }
        internal int Slot { get; }
        internal uint Version { get; }

        public bool Equals(CoreMotionHandle other) =>
            Owner == other.Owner &&
            Slot == other.Slot &&
            Version == other.Version;

        public override bool Equals(object obj) =>
            obj is CoreMotionHandle other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Owner;
                hash = (hash * 397) ^ Slot;
                return (hash * 397) ^ (int)Version;
            }
        }
    }

    // 実験的な手動更新エンジン。callback はメインスレッドで呼ばれる。
    internal sealed partial class MotionWorld : IDisposable
    {
#if MYMOTION_PROFILE_MARKERS
        private static readonly ProfilerMarker UpdateJobMarker = new(ProfilerCategory.Scripts, "MyMotion.Update.Job");
        private static readonly ProfilerMarker UpdateBindingMarker = new(ProfilerCategory.Scripts, "MyMotion.Update.Binding");
#endif

        private struct Slot
        {
            public int DenseIndex;
            public int Next;
            public uint Version;
            public int ValueStateIndex;
            // Transform Job の lane 番号 + 1。0 は lane なし。
            public int TransformLanePlusOne;
            // Transform Job で書ける Binding の種類 + 1。0 は対象外。
            public byte TransformKindPlusOne;
        }

        private struct AdapterRoute
        {
            public ushort StorageIndex;
            public int Index;
        }

        private struct Notifications
        {
            public Action OnComplete;
            public Action OnCancel;
            public Action<int> OnLoopComplete;
            public Action Continuation;
        }

        private static int s_nextOwner;
        private readonly int _owner = Interlocked.Increment(ref s_nextOwner);

        // MotionHandle は World の参照を持たず、owner id からここで引く。owner は World ごとに増える一意の値で、再利用しない
        // (再利用すると、Dispose された World の古い handle が新しい World の motion を指しうる)。
        private static MotionWorld[] s_registry = new MotionWorld[64];
        private static readonly object s_registryLock = new();

        internal static MotionWorld FromOwner(int owner)
        {
            MotionWorld[] registry = s_registry;
            return (uint)owner < (uint)registry.Length ? registry[owner] : null;
        }

        private void Register()
        {
            lock (s_registryLock)
            {
                MotionWorld[] registry = s_registry;
                if (_owner >= registry.Length)
                {
                    var grown = new MotionWorld[Math.Max(registry.Length * 2, _owner + 1)];
                    Array.Copy(registry, grown, registry.Length);
                    registry = grown;
                }
                registry[_owner] = this;
                s_registry = registry;
            }
        }

        private void Unregister()
        {
            lock (s_registryLock)
            {
                if (FromOwner(_owner) == this) s_registry[_owner] = null;
            }
        }
#if MYMOTION_THRESHOLD_BENCHMARK
        private static readonly int ParallelThreshold = GetParallelThreshold();

        private static int GetParallelThreshold()
        {
            string value = Environment.GetEnvironmentVariable("MYMOTION_THRESHOLD");
            if (!int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int threshold) || threshold <= 0)
            {
                throw new InvalidOperationException("MYMOTION_THRESHOLD must be a positive integer in a threshold benchmark build.");
            }
            return threshold;
        }
#else
        private const int ParallelThreshold = 8192;
#endif
        private const int CustomEaseParallelThreshold = 4096;
#if MYMOTION_BATCH_SIZE_BENCHMARK
        private static readonly int ParallelBatchSize = GetParallelBatchSize();

        private static int GetParallelBatchSize()
        {
            string value = Environment.GetEnvironmentVariable("MYMOTION_BATCH_SIZE");
            if (!int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int batchSize) || batchSize <= 0)
            {
                throw new InvalidOperationException("MYMOTION_BATCH_SIZE must be a positive integer in a batch size benchmark build.");
            }
            return batchSize;
        }
#else
        private const int ParallelBatchSize = 16;
#endif
        private const int SparseCleanupMinMotionCount = 4096;
        private readonly Action<Exception> _unhandledException;
        private NativeArray<Data> _items;
        private NativeArray<WideData> _wideItems;
        private Slot[] _slots;
        private object[] _callbacks;
        private object[] _states;
        private object[] _states1;
        private object[] _states2;
        private Notifications[] _notifications;
        private AdapterRoute[] _adapterRoutes;
        private MotionAdapterStorageBase[] _adapterStorages;
        private ValueStateBindingStorageBase[] _valueStateBindingStorages;
        private string[] _debugNames;
        private int _adapterStorageCount;
        private int _valueStateBindingStorageCount;
        private AnimationCurve[] _customEaseCurves;
        private NativeArray<CustomEaseCurveData> _nativeCustomEaseCurves;
        private float[] _customEaseEndValues;
        private bool _hasNativeCustomEaseCurves;
        private int[] _pendingRemovalIndices;
        private int _freeSlot;
        private int _count;
        private int _adapterMotionCount;
        internal int AdapterMotionCount => _adapterMotionCount;
        private int _floatValueCount;
        private int _vector3ValueCount;
        private int _wideValueCount;
        private int _pendingRemovalCount;
        private bool _pendingRemovalOverflow;
        private bool _pendingRemovalNeedsSort;
        private bool _hasPendingRemovals;
        private bool _isUpdating;
        private bool _isDisposed;
        private int _nativeCustomEaseCurveCount;

#if MYMOTION_THRESHOLD_BENCHMARK
        internal static readonly int AdapterParallelThreshold = GetAdapterParallelThreshold();

        private static int GetAdapterParallelThreshold()
        {
            string value = Environment.GetEnvironmentVariable("MYMOTION_ADAPTER_THRESHOLD");
            if (!int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int threshold) || threshold <= 0)
            {
                throw new InvalidOperationException("MYMOTION_ADAPTER_THRESHOLD must be a positive integer in a threshold benchmark build.");
            }
            return threshold;
        }
#else
        internal const int AdapterParallelThreshold = 11000;
#endif
        internal const int AdapterParallelBatchSize = 16;

        public MotionWorld(int capacity = 32, Action<Exception> unhandledException = null)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            _unhandledException = unhandledException;
            _items = new NativeArray<Data>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            // Job はすべての NativeArray フィールドが作成済みであることを要求するため、wide 値がなくても作成する。
            _wideItems = new NativeArray<WideData>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _nativeCustomEaseCurves = new NativeArray<CustomEaseCurveData>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            _slots = new Slot[capacity];
            _callbacks = new object[capacity];
            _states = new object[capacity];
            _notifications = new Notifications[capacity];
            _pendingRemovalIndices = new int[GetPendingRemovalCapacity(capacity)];
            InitializeSlots(0, capacity, -1);
            Register();
        }

        public int Count => _count;

        // World固有のhandlerが無ければ、MotionDispatcherに登録された共通handlerを使う。
        private void ReportUnhandledException(Exception exception)
        {
            Action<Exception> handler = _unhandledException ?? MotionDispatcher.GetUnhandledExceptionHandler();
            if (handler != null) handler(exception);
            else Debug.LogException(exception);
        }

        internal static int DataStrideBytes => UnsafeUtility.SizeOf<Data>();
        internal static int CustomEaseCurveDataStrideBytes => UnsafeUtility.SizeOf<CustomEaseCurveData>();

        public void EnsureCapacity(int minimumCapacity)
        {
            CompletePipeline();
            ThrowIfDisposed();
            if (minimumCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumCapacity));
            }
            if (_items.Length < minimumCapacity)
            {
                Grow(minimumCapacity);
            }
        }

        public void EnsureAdapterStorageCapacity<TValue, TOptions, TAdapter>(int minimumCapacity)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            CompletePipeline();
            ThrowIfDisposed();
            if (minimumCapacity < 0) throw new ArgumentOutOfRangeException(nameof(minimumCapacity));
            GetOrCreateAdapterStorage<TValue, TOptions, TAdapter>().EnsureCapacity(minimumCapacity);
        }

        public void Reset()
        {
            CompletePipeline();
            ThrowIfDisposed();
            if (_isUpdating)
            {
                throw new InvalidOperationException("Cannot reset during Update.");
            }
            for (int i = _count - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
            _pendingRemovalCount = 0;
            _pendingRemovalOverflow = false;
            _pendingRemovalNeedsSort = false;
            _hasPendingRemovals = false;
        }

        // 実データのみ。allocator のオーバーヘッドとマネージド配列は含まない。
        public unsafe long ReservedNativeBytes
        {
            get
            {
                if (_isDisposed) return 0;
                long bytes = (long)_items.Length * UnsafeUtility.SizeOf<Data>()
                    + (_wideItems.IsCreated ? (long)_wideItems.Length * UnsafeUtility.SizeOf<WideData>() : 0L)
                    + (_nativeCustomEaseCurves.IsCreated
                        ? (long)_nativeCustomEaseCurves.Length * UnsafeUtility.SizeOf<CustomEaseCurveData>()
                        : 0L);
                if (_nativeCustomEaseCurves.IsCreated)
                {
                    for (int i = 0; i < _nativeCustomEaseCurves.Length; i++)
                    {
                        CustomEaseCurveData curve = _nativeCustomEaseCurves[i];
                        if (curve.Keys != null)
                        {
                            bytes += (long)curve.Length * UnsafeUtility.SizeOf<Keyframe>();
                        }
                    }
                }
                for (int i = 0; i < _adapterStorageCount; i++) bytes += _adapterStorages[i].ReservedBytes;
                return bytes;
            }
        }
    }
}

using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MyMotion.Internal
{
    internal sealed partial class MotionWorld
    {
        public bool WasCanceled(CoreMotionHandle handle)
        {
            return TryGetIndexRegardlessOfCancellation(handle, out int index) && _items[index].IsCanceled;
        }

        public bool IsAwaitCompleted(CoreMotionHandle handle)
        {
            return !TryGetIndex(handle, out int index) || _items[index].IsCompletionNotified;
        }

        // handle の検査は1回だけにし、Data (約 100 byte) は複製せずにその場で書き換える。
        // 条件は IsInSequence と IsActive を合わせたもの (取り消し済み・Sequence 内・完了済みで保持されていないものは対象外)。
        public unsafe bool Cancel(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) return false;
            ref Data item = ref ((Data*)_items.GetUnsafePtr())[index];
            if (item.IsInSequence || (item.IsComplete && !item.IsPreserved)) return false;
            item.IsCanceled = true;
            QueueRemoval(index);
            NotifyCancel(index);
            return true;
        }

        public bool FinishFromSequence(CoreMotionHandle handle)
        {
            if (!TryGetIndex(handle, out int index)) return false;
            Data item = _items[index];
            if (!item.IsInSequence) return false;

            if (item.IsComplete)
            {
                item.IsInSequence = false;
                _items[index] = item;
                if (!item.IsPreserved) QueueRemoval(index);
                if (_states[index] is ISequenceReleaseHandler sequenceReleaseHandler)
                {
                    sequenceReleaseHandler.OnSequenceReleased();
                }
                return true;
            }

            item.IsCanceled = true;
            _items[index] = item;
            QueueRemoval(index);
            NotifyCancel(index);
            return true;
        }

        public bool TryComplete(CoreMotionHandle handle)
        {
            if (IsInSequence(handle) || !IsActive(handle))
            {
                return false;
            }
            int index = _slots[handle.Slot].DenseIndex;
            Data item = _items[index];
            if (item.Loops < 0 || (item.IsComplete && item.IsCompletionNotified))
            {
                return false;
            }
            item.Elapsed = (double)item.Delay * (item.DelayEveryLoop ? item.Loops : 1) + (double)item.Duration * item.Loops;
            item.PreviousCompletedLoops = item.CompletedLoops;
            item.CompletedLoops = (ushort)item.Loops;
            item.IsComplete = true;
            item.IsDelayed = false;
            item.IsCompletionNotified = true;
            if (!item.IsPreserved) QueueRemoval(index);
            float progress = item.LoopType switch
            {
                MotionLoopType.Restart => 1f,
                MotionLoopType.Flip or MotionLoopType.Yoyo => (item.Loops & 1) == 0 ? 0f : 1f,
                MotionLoopType.Incremental => item.Loops,
                _ => 1f
            };
            if (item.HasBinding)
            {
                bool isCustomEase = item.Ease == MotionEase.CustomAnimationCurve;
                if (isCustomEase)
                {
                    if (item.IsAdapter)
                    {
                        AnimationCurve curve = _customEaseCurves?[index];
                        if (curve == null) throw new InvalidOperationException("The custom ease curve has not been assigned.");
                        progress = curve.Evaluate(progress);
                    }
                    else
                    {
                        CustomEaseCurveData curve = _nativeCustomEaseCurves[index];
                        if (!curve.IsAssigned) throw new InvalidOperationException("The custom ease curve has not been assigned.");
                        progress = CustomEaseCurveEvaluator.Evaluate(in curve, progress);
                    }
                }
                SetValue(ref item, progress, IsOnlyFloatValueType(), IsOnlyVector3ValueType(), _wideItems);
            }
            _items[index] = item;
            if (item.HasBinding)
            {
                try
                {
                    Bind(index, item);
                }
                catch (Exception exception)
                {
                    ReportUnhandledException(exception);
                }
            }
            if (item.CompletedLoops > item.PreviousCompletedLoops)
            {
                try
                {
                    _notifications[index].OnLoopComplete?.Invoke(item.CompletedLoops);
                }
                catch (Exception exception)
                {
                    ReportUnhandledException(exception);
                }
            }
            NotifyComplete(index);
            return true;
        }

        private void NotifyCancel(int index)
        {
            // callback がない motion がほとんどなので、null の書き込み (と GC の write barrier) と try は要るときだけにする。
            ref Notifications notifications = ref _notifications[index];
            Action continuation = notifications.Continuation;
            Action onCancel = notifications.OnCancel;
            if (continuation != null) notifications.Continuation = null;
            if (onCancel != null)
            {
                try { onCancel.Invoke(); }
                catch (Exception exception) { ReportUnhandledException(exception); }
            }
            if (continuation != null)
            {
                try { continuation.Invoke(); }
                catch (Exception exception) { ReportUnhandledException(exception); }
            }
        }

        private void NotifyLoopComplete(int index, int loops)
        {
            try { _notifications[index].OnLoopComplete?.Invoke(loops); }
            catch (Exception exception) { ReportUnhandledException(exception); }
        }

        private void NotifyComplete(int index)
        {
            // NotifyCancel と同じく、callback がなければ書き込みも try も行わない。
            ref Notifications notifications = ref _notifications[index];
            Action continuation = notifications.Continuation;
            Action onComplete = notifications.OnComplete;
            if (continuation != null) notifications.Continuation = null;
            if (onComplete != null)
            {
                try { onComplete.Invoke(); }
                catch (Exception exception) { ReportUnhandledException(exception); }
            }
            if (continuation != null)
            {
                try { continuation.Invoke(); }
                catch (Exception exception) { ReportUnhandledException(exception); }
            }
        }
    }
}

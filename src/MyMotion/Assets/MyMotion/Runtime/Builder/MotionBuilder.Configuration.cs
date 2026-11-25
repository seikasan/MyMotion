using System;
using MyMotion.Internal;

namespace MyMotion
{
    public partial struct MotionBuilder<TValue> : IDisposable
    {
        internal readonly MotionBuilder<TValue> WithSharedConfiguration()
        {
            var sharedConfiguration = new SharedMotionBuilderConfiguration(_scheduler, _options);
            return new MotionBuilder<TValue>(_scheduler, _start, _end, _options, _factory, sharedConfiguration);
        }

        internal readonly void GetSettings(
            out TValue startValue,
            out TValue endValue,
            out MotionBuilderOptions options,
            out IMotionScheduler scheduler)
        {
            startValue = _start;
            endValue = _end;
            options = _sharedConfiguration?.Options ?? _options;
            scheduler = _sharedConfiguration?.Scheduler ?? _scheduler;
        }

        internal readonly MotionBuilder<TValue> WithSequenceCallbacks(Action onComplete, Action onCancel)
        {
            MotionBuilderOptions options = _sharedConfiguration?.Options ?? _options;
            Action userOnComplete = options.OnComplete;
            Action userOnCancel = options.OnCancel;
            options.OnComplete = () =>
            {
                try
                {
                    userOnComplete?.Invoke();
                }
                finally
                {
                    onComplete?.Invoke();
                }
            };
            options.OnCancel = () =>
            {
                try
                {
                    userOnCancel?.Invoke();
                }
                finally
                {
                    onCancel?.Invoke();
                }
            };
            return WithOptions(options);
        }

        // Bind のたびに options (約 80 byte) を複製しないよう、参照で読む。
        private static ref readonly MotionBuilderOptions OptionsOf(in MotionBuilder<TValue> builder)
        {
            if (builder._sharedConfiguration != null) return ref builder._sharedConfiguration.Options;
            return ref builder._options;
        }

        private readonly MotionBuilder<TValue> WithOptions(MotionBuilderOptions options)
        {
            if (_sharedConfiguration != null)
            {
                _sharedConfiguration.Options = options;
                return this;
            }
            return new MotionBuilder<TValue>(_scheduler, _start, _end, options, _factory);
        }

        private static void ConfigureCustomEase(MotionWorld world, CoreMotionHandle coreHandle, in MotionBuilderOptions options)
        {
            if (options.Ease == Ease.CustomAnimationCurve)
            {
                world.SetCustomEaseCurve(coreHandle, options.CustomEaseCurve);
            }
            ConfigureDebugName(world, coreHandle, in options);
        }

        private static void ConfigureDebugName(MotionWorld world, CoreMotionHandle coreHandle, in MotionBuilderOptions options)
        {
            if (options.DebugName != null)
            {
                world.SetDebugName(coreHandle, options.DebugName);
            }
        }
    }
}

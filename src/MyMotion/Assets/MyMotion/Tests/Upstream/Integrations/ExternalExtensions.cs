using MyMotion.Extensions;
namespace MyMotion.Tests.Upstream
{
    static class ExternalExtensions
    {
#if MYMOTION_TEST_R3
        public static R3.Observable<TValue> ToR3Observable<TValue>(this MotionBuilder<TValue> builder) where TValue : unmanaged
        {
            return MotionR3Extensions.ToObservable(builder);
        }
#endif

#if MYMOTION_TEST_UNIRX
        public static System.IObservable<TValue> ToRxObservable<TValue>(this MotionBuilder<TValue> builder) where TValue : unmanaged
        {
            return MotionUniRxExtensions.ToObservable(builder);
        }
#endif
    }
}
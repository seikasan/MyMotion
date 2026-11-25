using Unity.Burst;
using Unity.Collections;

namespace MyMotion.Internal
{
    internal static partial class FixedStringHelper
    {
        [BurstCompile]
        public static unsafe int GetUtf8CharCount(ref FixedString512Bytes runes)
        {
            int length = 0;
            byte* bytes = runes.GetUnsafePtr();
            for (int i = 0; i < runes.Length; i++)
            {
                if ((bytes[i] & 0xC0) != 0x80) length++;
            }
            return length;
        }

    }
}

// LitMotion 2.0.2 (b4d57e2614bf8209b425d166fcb864fa82efdef2) を基にしている。Copyright (c) 2023 Yusuke Nakada; MIT License。ThirdPartyNotices.md を参照。
using Unity.Burst;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    [BurstCompile]
    internal static partial class FixedStringHelper
    {
        private static readonly char[] LowercaseChars = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        private static readonly char[] UppercaseChars = {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        private static readonly char[] NumeralsChars = {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        private static readonly char[] AllChars = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        private static char GetScrambleChar(ScrambleMode scrambleMode, ref Random random)
        {
            return scrambleMode switch
            {
                ScrambleMode.None => '\0',
                ScrambleMode.Uppercase => UppercaseChars[random.NextInt(0, UppercaseChars.Length)],
                ScrambleMode.Lowercase => LowercaseChars[random.NextInt(0, LowercaseChars.Length)],
                ScrambleMode.Numerals => NumeralsChars[random.NextInt(0, NumeralsChars.Length)],
                ScrambleMode.All => AllChars[random.NextInt(0, AllChars.Length)],
                _ => '\0'
            };
        }
    }
}

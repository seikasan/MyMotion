using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal static partial class FixedStringHelper
    {
        [BurstCompile]
        public static unsafe int GetUtf8CharCount(ref FixedString32Bytes runes)
        {
            int length = 0;
            byte* bytes = runes.GetUnsafePtr();
            for (int i = 0; i < runes.Length; i++)
            {
                if ((bytes[i] & 0xC0) != 0x80) length++;
            }
            return length;
        }

        private static unsafe int GetUtf8ByteIndex<TFixedString>(ref TFixedString text, int characterCount, int totalCharacterCount)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            if (characterCount <= 0) return 0;
            if (text.Length == totalCharacterCount) return math.min(characterCount, text.Length);

            byte* bytes = text.GetUnsafePtr();
            int currentCharacter = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if ((bytes[i] & 0xC0) == 0x80) continue;
                if (currentCharacter == characterCount) return i;
                currentCharacter++;
            }

            return text.Length;
        }

        [BurstCompile]
        public static void Interpolate(ref FixedString32Bytes start, ref FixedString32Bytes end, float t, ScrambleMode scrambleMode, bool richTextEnabled, ref Random randomState, ref FixedString64Bytes customScrambleChars, out FixedString32Bytes result)
        {
            if (richTextEnabled)
            {
                RichTextParser.GetSymbols(ref start, Allocator.Temp, out var startTextSymbols, out var startTextUtf8Length);
                RichTextParser.GetSymbols(ref end, Allocator.Temp, out var endTextSymbols, out var endTextUtf8Length);

                FillRichText(ref startTextSymbols, ref endTextSymbols, startTextUtf8Length, endTextUtf8Length, t, scrambleMode, ref randomState, ref customScrambleChars, out result);

                startTextSymbols.Dispose();
                endTextSymbols.Dispose();
            }
            else
            {
                FillText(ref start, ref end, t, scrambleMode, ref randomState, ref customScrambleChars, out result);
            }
        }

        private static unsafe void FillText(
            ref FixedString32Bytes start,
            ref FixedString32Bytes end,
            float t,
            ScrambleMode scrambleMode,
            ref Random randomState,
            ref FixedString64Bytes customScrambleChars,
            out FixedString32Bytes result)
        {
            var startTextUtf8Length = GetUtf8CharCount(ref start);
            var endTextUtf8Length = GetUtf8CharCount(ref end);
            var length = math.max(startTextUtf8Length, endTextUtf8Length);
            var currentTextLength = (int)math.round(length * t);

            if (scrambleMode == ScrambleMode.None && length >= 8)
            {
                int endByteLength = GetUtf8ByteIndex(ref end, currentTextLength, endTextUtf8Length);
                int startByteOffset = GetUtf8ByteIndex(ref start, currentTextLength, startTextUtf8Length);
                int outputByteLength = endByteLength + start.Length - startByteOffset;
                if (outputByteLength <= start.Capacity)
                {
                    Unsafe.SkipInit(out result);
                    byte* resultBytes = result.GetUnsafePtr();
                    if (endByteLength > 0) UnsafeUtility.MemCpy(resultBytes, end.GetUnsafePtr(), endByteLength);
                    int startByteLength = start.Length - startByteOffset;
                    if (startByteLength > 0) UnsafeUtility.MemCpy(resultBytes + endByteLength, start.GetUnsafePtr() + startByteOffset, startByteLength);
                    result.Length = outputByteLength;
                    return;
                }
            }

            var enumeratorStart = start.GetEnumerator();
            var enumeratorEnd = end.GetEnumerator();
            result = new();

            for (int i = 0; i < length; i++)
            {
                var startMoveNext = enumeratorStart.MoveNext();
                var endMoveNext = enumeratorEnd.MoveNext();

                if (i < currentTextLength)
                {
                    if (endMoveNext)
                    {
                        result.Append(enumeratorEnd.Current);
                    }
                }
                else
                {
                    if (startMoveNext)
                    {
                        result.Append(enumeratorStart.Current);
                    }
                }
            }

            FillScrambleChars(ref result, scrambleMode, ref randomState, ref customScrambleChars, length - currentTextLength);
        }

        private static void FillRichText(
            ref UnsafeList<RichTextSymbol32Bytes> startSymbols,
            ref UnsafeList<RichTextSymbol32Bytes> endSymbols,
            int startTextUtf8Length,
            int endTextUtf8Length,
            float t,
            ScrambleMode scrambleMode,
            ref Random randomState,
            ref FixedString64Bytes customScrambleChars,
            out FixedString32Bytes result)
        {
            var length = math.max(startTextUtf8Length, endTextUtf8Length);
            var currentTextLength = (int)math.round(length * t);

            var slicedText1 = SliceSymbols(ref endSymbols, 0, currentTextLength, out var length1);
            var slicedText2 = SliceSymbols(ref startSymbols, currentTextLength + 1, length - 1, out var length2);

            result = new FixedString32Bytes();
            result.Append(slicedText1);
            result.Append(slicedText2);

            FillScrambleChars(ref result, scrambleMode, ref randomState, ref customScrambleChars, length - (length1 + length2));
        }

        private static void FillScrambleChars(
            ref FixedString32Bytes target,
            ScrambleMode scrambleMode,
            ref Random randomState,
            ref FixedString64Bytes customScrambleChars,
            int count)
        {
            if (scrambleMode == ScrambleMode.None) return;
            if (randomState.state == 0) randomState.InitState();

            if (scrambleMode == ScrambleMode.Custom)
            {
                var customScrambleCharsUtf8Length = GetUtf8CharCount(ref customScrambleChars);
                for (int i = 0; i < count; i++)
                {
                    target.Append(GetRuneOf(ref customScrambleChars, randomState.NextInt(0, customScrambleCharsUtf8Length)));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    target.Append(GetScrambleChar(scrambleMode, ref randomState));
                }
            }
        }

        private static unsafe FixedString32Bytes SliceSymbols(ref UnsafeList<RichTextSymbol32Bytes> symbols, int from, int to, out int resultRichTextLength)
        {
            var text = new FixedString32Bytes();
            RichTextSymbol32Bytes* symbolsPtr = symbols.Ptr;
            var offset = 0;
            var tagIndent = 0;
            resultRichTextLength = 0;

            for (int i = 0; i < symbols.Length; i++)
            {
                RichTextSymbol32Bytes* symbol = symbolsPtr + i;
                switch (symbol->Type)
                {
                    case RichTextSymbolType.Text:
                        var enumerator = symbol->Text.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            if (from <= offset && offset < to)
                            {
                                text.Append(current);
                                resultRichTextLength++;
                            }
                            offset++;

                            if (offset >= to && tagIndent == 0) goto LOOP_END;
                        }
                        break;
                    case RichTextSymbolType.TagStart:
                        text.Append(symbol->Text);
                        tagIndent++;
                        break;
                    case RichTextSymbolType.TagEnd:
                        text.Append(symbol->Text);
                        tagIndent--;
                        if (offset >= to && tagIndent == 0) goto LOOP_END;
                        break;
                }
            }

        LOOP_END:
            return text;
        }
    }
}

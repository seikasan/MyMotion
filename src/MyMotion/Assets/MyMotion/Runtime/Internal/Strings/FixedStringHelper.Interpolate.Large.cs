// LitMotion 2.0.2 (b4d57e2614bf8209b425d166fcb864fa82efdef2) を基にしている。Copyright (c) 2023 Yusuke Nakada; MIT License。ThirdPartyNotices.md を参照。
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MyMotion.Internal
{
    internal static partial class FixedStringHelper
    {
        public static void Interpolate(ref FixedString512Bytes start, ref FixedString512Bytes end, float t, ScrambleMode scrambleMode, bool richTextEnabled, ref Random randomState, ref FixedString64Bytes customScrambleChars, out FixedString512Bytes result)
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
            ref FixedString512Bytes start,
            ref FixedString512Bytes end,
            float t,
            ScrambleMode scrambleMode,
            ref Random randomState,
            ref FixedString64Bytes customScrambleChars,
            out FixedString512Bytes result)
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
            ref UnsafeList<RichTextSymbol512Bytes> startSymbols,
            ref UnsafeList<RichTextSymbol512Bytes> endSymbols,
            int startTextUtf8Length,
            int endTextUtf8Length,
            float t,
            ScrambleMode scrambleMode,
            ref Random randomState,
            ref FixedString64Bytes customScrambleChars,
            out FixedString512Bytes result)
        {
            var length = math.max(startTextUtf8Length, endTextUtf8Length);
            var currentTextLength = (int)math.round(length * t);

            var slicedText1 = SliceSymbols(ref endSymbols, 0, currentTextLength, out var length1);
            var slicedText2 = SliceSymbols(ref startSymbols, currentTextLength + 1, length - 1, out var length2);

            result = new FixedString512Bytes();
            result.Append(slicedText1);
            result.Append(slicedText2);

            FillScrambleChars(ref result, scrambleMode, ref randomState, ref customScrambleChars, length - (length1 + length2));
        }

        private static void FillScrambleChars(
            ref FixedString512Bytes target,
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

        private static unsafe FixedString512Bytes SliceSymbols(ref UnsafeList<RichTextSymbol512Bytes> symbols, int from, int to, out int resultRichTextLength)
        {
            var text = new FixedString512Bytes();
            RichTextSymbol512Bytes* symbolsPtr = symbols.Ptr;
            var offset = 0;
            var tagIndent = 0;
            resultRichTextLength = 0;

            for (int i = 0; i < symbols.Length; i++)
            {
                RichTextSymbol512Bytes* symbol = symbolsPtr + i;
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

        [BurstCompile]
        public static unsafe int GetUtf8CharCount(ref FixedString4096Bytes runes)
        {
            int length = 0;
            byte* bytes = runes.GetUnsafePtr();
            for (int i = 0; i < runes.Length; i++)
            {
                if ((bytes[i] & 0xC0) != 0x80) length++;
            }
            return length;
        }

        private static Unicode.Rune GetRuneOf(ref FixedString4096Bytes text, int charIndex)
        {
            int index = 0;
            var enumerator = text.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (index == charIndex) return enumerator.Current;
                index++;
            }
            return Unicode.BadRune;
        }

        [BurstCompile]
        public static void Interpolate(ref FixedString4096Bytes start, ref FixedString4096Bytes end, float t, ScrambleMode scrambleMode, bool richTextEnabled, ref Random randomState, ref FixedString64Bytes customScrambleChars, out FixedString4096Bytes result)
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
            ref FixedString4096Bytes start,
            ref FixedString4096Bytes end,
            float t,
            ScrambleMode scrambleMode,
            ref Random randomState,
            ref FixedString64Bytes customScrambleChars,
            out FixedString4096Bytes result)
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
            ref UnsafeList<RichTextSymbol4096Bytes> startSymbols,
            ref UnsafeList<RichTextSymbol4096Bytes> endSymbols,
            int startTextUtf8Length,
            int endTextUtf8Length,
            float t,
            ScrambleMode scrambleMode,
            ref Random randomState,
            ref FixedString64Bytes customScrambleChars,
            out FixedString4096Bytes result)
        {
            var length = math.max(startTextUtf8Length, endTextUtf8Length);
            var currentTextLength = (int)math.round(length * t);

            var slicedText1 = SliceSymbols(ref endSymbols, 0, currentTextLength, out var length1);
            var slicedText2 = SliceSymbols(ref startSymbols, currentTextLength + 1, length - 1, out var length2);

            result = new FixedString4096Bytes();
            result.Append(slicedText1);
            result.Append(slicedText2);

            FillScrambleChars(ref result, scrambleMode, ref randomState, ref customScrambleChars, length - (length1 + length2));
        }

        private static void FillScrambleChars(
            ref FixedString4096Bytes target,
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

        private static unsafe FixedString4096Bytes SliceSymbols(ref UnsafeList<RichTextSymbol4096Bytes> symbols, int from, int to, out int resultRichTextLength)
        {
            var text = new FixedString4096Bytes();
            RichTextSymbol4096Bytes* symbolsPtr = symbols.Ptr;
            var offset = 0;
            var tagIndent = 0;
            resultRichTextLength = 0;

            for (int i = 0; i < symbols.Length; i++)
            {
                RichTextSymbol4096Bytes* symbol = symbolsPtr + i;
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

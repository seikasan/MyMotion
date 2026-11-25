// LitMotion の実装と結果を突き合わせるテスト。LitMotion がプロジェクトにあるときだけコンパイルする。
#if MYMOTION_TEST_LITMOTION
using System;
using MyMotion.Adapters;
using NUnit.Framework;
using Unity.Collections;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [TestCase(MyMotion.ScrambleMode.None)]
        [TestCase(MyMotion.ScrambleMode.Uppercase)]
        [TestCase(MyMotion.ScrambleMode.Lowercase)]
        [TestCase(MyMotion.ScrambleMode.Numerals)]
        [TestCase(MyMotion.ScrambleMode.All)]
        [TestCase(MyMotion.ScrambleMode.Custom)]
        public void FixedString32Adapter_MatchesLitMotionForScrambleModes(MyMotion.ScrambleMode scrambleMode)
        {
            FixedString32Bytes start = new("生命値 0123");
            FixedString32Bytes end = new("生命値 9876");
            string originalStart = start.ToString();
            string originalEnd = end.ToString();
            FixedString64Bytes customScrambleChars = new("xyz123");
            StringOptions options = new()
            {
                ScrambleMode = scrambleMode,
                CustomScrambleChars = customScrambleChars,
                RandomSeed = 78431u
            };
            MotionEvaluationContext context = new() { Progress = 0.37f, Time = 0.625d };

            FixedString32Bytes actual = new FixedString32BytesMotionAdapter().Evaluate(
                ref start, ref end, ref options, in context);

            FixedString32Bytes referenceStart = new(originalStart);
            FixedString32Bytes referenceEnd = new(originalEnd);
            LitMotion.StringOptions referenceOptions = new()
            {
                ScrambleMode = (LitMotion.ScrambleMode)scrambleMode,
                CustomScrambleChars = customScrambleChars,
                RandomSeed = 78431u
            };
            LitMotion.MotionEvaluationContext referenceContext = new() { Progress = 0.37f, Time = 0.625d };
            FixedString32Bytes expected = new LitMotion.Adapters.FixedString32BytesMotionAdapter().Evaluate(
                ref referenceStart, ref referenceEnd, ref referenceOptions, in referenceContext);

            Assert.That(actual.ToString(), Is.EqualTo(expected.ToString()));
            Assert.That(start.ToString(), Is.EqualTo(originalStart));
            Assert.That(end.ToString(), Is.EqualTo(originalEnd));
        }

        [Test]
        public void FixedStringAdapters_MatchLitMotionForAllCapacities()
        {
            MotionEvaluationContext context = new() { Progress = 0.53f, Time = 0.29d };
            LitMotion.MotionEvaluationContext referenceContext = new() { Progress = 0.53f, Time = 0.29d };
            StringOptions options = new() { ScrambleMode = MyMotion.ScrambleMode.None, RandomSeed = 927u };
            LitMotion.StringOptions referenceOptions = new()
            {
                ScrambleMode = LitMotion.ScrambleMode.None,
                RandomSeed = 927u
            };

            FixedString32Bytes start32 = new("abcdefghijklあいうえ");
            FixedString32Bytes end32 = new("mnopqrstuvwxかきくけ");
            FixedString32Bytes result32 = new FixedString32BytesMotionAdapter().Evaluate(ref start32, ref end32, ref options, in context);
            FixedString32Bytes expected32 = new LitMotion.Adapters.FixedString32BytesMotionAdapter().Evaluate(ref start32, ref end32, ref referenceOptions, in referenceContext);
            Assert.That(result32.ToString(), Is.EqualTo(expected32.ToString()));

            FixedString64Bytes start64 = new("abcdefghijklあいうえ");
            FixedString64Bytes end64 = new("mnopqrstuvwxかきくけ");
            FixedString64Bytes result64 = new FixedString64BytesMotionAdapter().Evaluate(ref start64, ref end64, ref options, in context);
            FixedString64Bytes expected64 = new LitMotion.Adapters.FixedString64BytesMotionAdapter().Evaluate(ref start64, ref end64, ref referenceOptions, in referenceContext);
            Assert.That(result64.ToString(), Is.EqualTo(expected64.ToString()));

            FixedString128Bytes start128 = new("abcdefghijklあいうえ");
            FixedString128Bytes end128 = new("mnopqrstuvwxかきくけ");
            FixedString128Bytes result128 = new FixedString128BytesMotionAdapter().Evaluate(ref start128, ref end128, ref options, in context);
            FixedString128Bytes expected128 = new LitMotion.Adapters.FixedString128BytesMotionAdapter().Evaluate(ref start128, ref end128, ref referenceOptions, in referenceContext);
            Assert.That(result128.ToString(), Is.EqualTo(expected128.ToString()));

            FixedString512Bytes start512 = new("abcdefghijklあいうえ");
            FixedString512Bytes end512 = new("mnopqrstuvwxかきくけ");
            FixedString512Bytes result512 = new FixedString512BytesMotionAdapter().Evaluate(ref start512, ref end512, ref options, in context);
            FixedString512Bytes expected512 = new LitMotion.Adapters.FixedString512BytesMotionAdapter().Evaluate(ref start512, ref end512, ref referenceOptions, in referenceContext);
            Assert.That(result512.ToString(), Is.EqualTo(expected512.ToString()));

            FixedString4096Bytes start4096 = new("abcdefghijklあいうえ");
            FixedString4096Bytes end4096 = new("mnopqrstuvwxかきくけ");
            FixedString4096Bytes result4096 = new FixedString4096BytesMotionAdapter().Evaluate(ref start4096, ref end4096, ref options, in context);
            FixedString4096Bytes expected4096 = new LitMotion.Adapters.FixedString4096BytesMotionAdapter().Evaluate(ref start4096, ref end4096, ref referenceOptions, in referenceContext);
            Assert.That(result4096.ToString(), Is.EqualTo(expected4096.ToString()));
        }

        [TestCase(0f)]
        [TestCase(0.37f)]
        [TestCase(1f)]
        public void FixedStringAdapters_MatchLitMotionForAsciiFastPathAtEveryCapacity(float progress)
        {
            MotionEvaluationContext context = new() { Progress = progress, Time = 0.5d };
            LitMotion.MotionEvaluationContext referenceContext = new() { Progress = progress, Time = 0.5d };
            StringOptions options = new() { ScrambleMode = MyMotion.ScrambleMode.None };
            LitMotion.StringOptions referenceOptions = new() { ScrambleMode = LitMotion.ScrambleMode.None };

            FixedString32Bytes start32 = new("abcdefghijkl");
            FixedString32Bytes end32 = new("12345678");
            FixedString32Bytes result32 = new FixedString32BytesMotionAdapter().Evaluate(ref start32, ref end32, ref options, in context);
            FixedString32Bytes expected32 = new LitMotion.Adapters.FixedString32BytesMotionAdapter().Evaluate(ref start32, ref end32, ref referenceOptions, in referenceContext);
            Assert.That(result32.ToString(), Is.EqualTo(expected32.ToString()));

            FixedString64Bytes start64 = new("abcdefghijkl");
            FixedString64Bytes end64 = new("12345678");
            FixedString64Bytes result64 = new FixedString64BytesMotionAdapter().Evaluate(ref start64, ref end64, ref options, in context);
            FixedString64Bytes expected64 = new LitMotion.Adapters.FixedString64BytesMotionAdapter().Evaluate(ref start64, ref end64, ref referenceOptions, in referenceContext);
            Assert.That(result64.ToString(), Is.EqualTo(expected64.ToString()));

            FixedString128Bytes start128 = new("abcdefghijkl");
            FixedString128Bytes end128 = new("12345678");
            FixedString128Bytes result128 = new FixedString128BytesMotionAdapter().Evaluate(ref start128, ref end128, ref options, in context);
            FixedString128Bytes expected128 = new LitMotion.Adapters.FixedString128BytesMotionAdapter().Evaluate(ref start128, ref end128, ref referenceOptions, in referenceContext);
            Assert.That(result128.ToString(), Is.EqualTo(expected128.ToString()));

            FixedString512Bytes start512 = new("abcdefghijkl");
            FixedString512Bytes end512 = new("12345678");
            FixedString512Bytes result512 = new FixedString512BytesMotionAdapter().Evaluate(ref start512, ref end512, ref options, in context);
            FixedString512Bytes expected512 = new LitMotion.Adapters.FixedString512BytesMotionAdapter().Evaluate(ref start512, ref end512, ref referenceOptions, in referenceContext);
            Assert.That(result512.ToString(), Is.EqualTo(expected512.ToString()));

            FixedString4096Bytes start4096 = new("abcdefghijkl");
            FixedString4096Bytes end4096 = new("12345678");
            FixedString4096Bytes result4096 = new FixedString4096BytesMotionAdapter().Evaluate(ref start4096, ref end4096, ref options, in context);
            FixedString4096Bytes expected4096 = new LitMotion.Adapters.FixedString4096BytesMotionAdapter().Evaluate(ref start4096, ref end4096, ref referenceOptions, in referenceContext);
            Assert.That(result4096.ToString(), Is.EqualTo(expected4096.ToString()));
        }

        [Test]
        public void FixedString32Adapter_NoScramble_MatchesLitMotionWhenJoinedTextOverflowsCapacity()
        {
            FixedString32Bytes start = new("aaaaaaaaaaああああbbbbbb");
            FixedString32Bytes end = new("ああああccccccdddddddddd");
            StringOptions options = new() { ScrambleMode = MyMotion.ScrambleMode.None };
            LitMotion.StringOptions referenceOptions = new() { ScrambleMode = LitMotion.ScrambleMode.None };
            MotionEvaluationContext context = new() { Progress = 0.5f, Time = 0.5d };
            LitMotion.MotionEvaluationContext referenceContext = new() { Progress = 0.5f, Time = 0.5d };

            FixedString32Bytes actual = new FixedString32BytesMotionAdapter().Evaluate(
                ref start, ref end, ref options, in context);
            FixedString32Bytes expected = new LitMotion.Adapters.FixedString32BytesMotionAdapter().Evaluate(
                ref start, ref end, ref referenceOptions, in referenceContext);

            Assert.That(actual.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void FixedString64Adapter_MatchesLitMotionWithRichTextAndBuilderOptions()
        {
            using ManualMotionDispatcher myDispatcher = new();
            LitMotion.ManualMotionDispatcher litDispatcher = new();
            StringTarget64 myTarget = new();
            StringTarget64 litTarget = new();
            FixedString64Bytes start = new("<color=#ff0000>生命 01</color>");
            FixedString64Bytes end = new("<color=#00ff00>生命 99</color>");
            FixedString64Bytes scrambleChars = new("xyz012");

            MyMotion.MotionHandle myHandle = MMotion.String.Create64Bytes(start, end, 1f)
                .WithScheduler(myDispatcher.Scheduler)
                .WithEase(Ease.Linear)
                .WithImmediateBind(false)
                .WithRichText()
                .WithScrambleChars(scrambleChars)
                .WithRandomSeed(312u)
                .Bind(myTarget, static (value, target) => target.Value = value);

            var litBuilder = LitMotion.LMotion.String.Create64Bytes(start, end, 1f)
                .WithScheduler(litDispatcher.Scheduler)
                .WithEase(LitMotion.Ease.Linear)
                .WithImmediateBind(false);
            litBuilder = LitMotion.MotionBuilderExtensions.WithRichText(litBuilder);
            litBuilder = LitMotion.MotionBuilderExtensions.WithScrambleChars(litBuilder, scrambleChars);
            litBuilder = LitMotion.MotionBuilderExtensions.WithRandomSeed(litBuilder, 312u);
            LitMotion.MotionHandle litHandle = litBuilder.Bind(
                litTarget,
                static (FixedString64Bytes value, StringTarget64 target) => target.Value = value);

            myDispatcher.Update(0.37d);
            litDispatcher.Update(0.37d);

            Assert.That(myTarget.Value.ToString(), Is.EqualTo(litTarget.Value.ToString()));
            Assert.That(myHandle.IsActive(), Is.True);
            Assert.That(LitMotion.MotionHandleExtensions.IsActive(litHandle), Is.True);
            Assert.Throws<ArgumentException>(() => MMotion.String.Create32Bytes(
                    new FixedString32Bytes("a"), new FixedString32Bytes("b"), 1f)
                .WithScrambleChars(MyMotion.ScrambleMode.Custom));
        }
    }
}
#endif

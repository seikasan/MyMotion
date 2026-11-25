using System;
using NUnit.Framework;
using UnityEngine;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void MotionSettings_IsSerializable()
        {
            Assert.That(
                Attribute.IsDefined(typeof(MotionSettings<float, NoOptions>), typeof(SerializableAttribute)),
                Is.True);
        }

        [Test]
        public void MotionSettings_CreateAndToMotionSettingsPreserveBuilderConfiguration()
        {
            using ManualMotionDispatcher dispatcher = new();
            MotionSettings<float, NoOptions> settings = new()
            {
                StartValue = 2f,
                EndValue = 10f,
                Duration = 2f,
                Ease = Ease.InQuad,
                Delay = 0.5f,
                DelayType = DelayType.EveryLoop,
                Loops = 3,
                LoopType = LoopType.Flip,
                CancelOnError = true,
                SkipValuesDuringDelay = false,
                ImmediateBind = false,
                Scheduler = dispatcher.Scheduler,
                PlaybackSpeed = 1.5f,
                TimeKind = MotionTimeKind.Realtime
            };

            MotionBuilder<float> builder = MMotion.Create(settings);
            MotionSettings<float, NoOptions> roundTrip = builder.ToMotionSettings();

            Assert.That(roundTrip.StartValue, Is.EqualTo(settings.StartValue));
            Assert.That(roundTrip.EndValue, Is.EqualTo(settings.EndValue));
            Assert.That(roundTrip.Duration, Is.EqualTo(settings.Duration));
            Assert.That(roundTrip.Ease, Is.EqualTo(settings.Ease));
            Assert.That(roundTrip.Delay, Is.EqualTo(settings.Delay));
            Assert.That(roundTrip.DelayType, Is.EqualTo(settings.DelayType));
            Assert.That(roundTrip.Loops, Is.EqualTo(settings.Loops));
            Assert.That(roundTrip.LoopType, Is.EqualTo(settings.LoopType));
            Assert.That(roundTrip.CancelOnError, Is.EqualTo(settings.CancelOnError));
            Assert.That(roundTrip.SkipValuesDuringDelay, Is.EqualTo(settings.SkipValuesDuringDelay));
            Assert.That(roundTrip.ImmediateBind, Is.EqualTo(settings.ImmediateBind));
            Assert.That(roundTrip.PlaybackSpeed, Is.EqualTo(settings.PlaybackSpeed));
            Assert.That(roundTrip.TimeKind, Is.EqualTo(settings.TimeKind));
            Assert.That(roundTrip.Scheduler, Is.SameAs(dispatcher.Scheduler));

            float value = -1f;
            MotionHandle handle = builder.Bind(next => value = next);
            dispatcher.Update(1d);

            Assert.That(value, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(handle.IsActive(), Is.True);
        }

        [Test]
        public void MotionSettings_IntegerRoundingModeMapsToBuilderAndBack()
        {
            MotionSettings<int, IntegerOptions> settings = new()
            {
                StartValue = 0,
                EndValue = 1,
                Duration = 1f,
                Options = new IntegerOptions { RoundingMode = RoundingMode.AwayFromZero },
                ImmediateBind = false
            };

            MotionBuilder<int> builder = MMotion.Create(settings);
            Assert.That(builder.ToMotionSettings().Options.RoundingMode, Is.EqualTo(RoundingMode.AwayFromZero));

            int value = -1;
            MotionHandle handle = builder.Bind(next => value = next);
            MMotion.Update(0.5d);

            Assert.That(value, Is.EqualTo(1));
            Assert.That(handle.IsActive(), Is.True);
        }

        [Test]
        public void MotionSettings_GenericAdapterCreateAndRoundTripPreserveAdapterOptions()
        {
            using ManualMotionDispatcher dispatcher = new();
            MotionSettings<float, AdapterOptions> settings = MMotion
                .Create<float, AdapterOptions, TestAdapter>(2f, 6f, 2f)
                .WithOptions(new AdapterOptions { ProgressScale = 2f })
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .ToMotionSettings();

            MotionBuilder<float, AdapterOptions, TestAdapter> builder =
                MMotion.Create<float, AdapterOptions, TestAdapter>(settings);
            MotionSettings<float, AdapterOptions> roundTrip = builder.ToMotionSettings();

            Assert.That(roundTrip.StartValue, Is.EqualTo(2f));
            Assert.That(roundTrip.EndValue, Is.EqualTo(6f));
            Assert.That(roundTrip.Duration, Is.EqualTo(2f));
            Assert.That(roundTrip.Options.ProgressScale, Is.EqualTo(2f));
            Assert.That(roundTrip.ImmediateBind, Is.False);
            Assert.That(roundTrip.Scheduler, Is.SameAs(dispatcher.Scheduler));

            float value = -1f;
            MotionHandle handle = builder.Bind(next => value = next);
            dispatcher.Update(0.5d);

            Assert.That(value, Is.EqualTo(4.5f).Within(0.0001f));
            Assert.That(handle.IsActive(), Is.True);
        }

        [Test]
        public void SerializableMotionSettings_RestoresBuiltInScheduler()
        {
            SerializableMotionSettings<float, NoOptions> settings = new()
            {
                StartValue = 0f,
                EndValue = 1f,
                Duration = 1f,
                Scheduler = MotionScheduler.UpdateRealtime
            };
            settings.OnBeforeSerialize();

            string json = JsonUtility.ToJson(settings);
            SerializableMotionSettings<float, NoOptions> deserialized =
                JsonUtility.FromJson<SerializableMotionSettings<float, NoOptions>>(json);
            deserialized.OnAfterDeserialize();

            Assert.That(deserialized.Scheduler, Is.SameAs(MotionScheduler.UpdateRealtime));
            Assert.That(deserialized.StartValue, Is.EqualTo(0f));
            Assert.That(deserialized.EndValue, Is.EqualTo(1f));
        }
    }
}

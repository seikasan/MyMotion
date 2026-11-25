using System;
using MyMotion.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;

namespace MyMotion.Tests
{
    public sealed partial class MyMotionRuntimeTests
    {
        [Test]
        public void TransformExtensions_BindPropertiesAndComponents()
        {
            var gameObject = new GameObject("MyMotion Transform Extension Test");
            Transform target = gameObject.transform;
            try
            {
                target.position = new Vector3(1f, 2f, 3f);
                MotionHandle positionX = MMotion.Create(0f, 10f, 1f)
                    .WithImmediateBind(false)
                    .BindToPositionX(target);
                MMotion.Update(0.5d);
                Assert.That(target.position, Is.EqualTo(new Vector3(5f, 2f, 3f)));
                positionX.TryCancel();

                MotionHandle positionXY = MMotion.Create(Vector2.zero, new Vector2(10f, 20f), 1f)
                    .WithImmediateBind(false)
                    .BindToPositionXY(target);
                MMotion.Update(0.5d);
                Assert.That(target.position, Is.EqualTo(new Vector3(5f, 10f, 3f)));
                positionXY.TryCancel();

                MotionHandle scaleXYZ = MMotion.Create(1f, 3f, 1f)
                    .WithImmediateBind(false)
                    .BindToLocalScaleXYZ(target);
                MMotion.Update(0.5d);
                Assert.That(target.localScale, Is.EqualTo(new Vector3(2f, 2f, 2f)));
                scaleXYZ.TryCancel();

                MotionHandle localRotation = MMotion.Create(Quaternion.identity, Quaternion.Euler(0f, 90f, 0f), 1f)
                    .WithImmediateBind(false)
                    .BindToLocalRotation(target);
                MMotion.Update(0.5d);
                Assert.That(Quaternion.Angle(target.localRotation, Quaternion.Euler(0f, 45f, 0f)), Is.LessThan(0.1f));
                localRotation.TryCancel();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void TransformExtensions_ExposeLitMotionBindingSurface()
        {
            var gameObject = new GameObject("MyMotion Transform Extension API Test");
            Transform target = gameObject.transform;
            var vector3 = MMotion.Create(Vector3.zero, Vector3.one, 1f).WithImmediateBind(false);
            var vector2 = MMotion.Create(Vector2.zero, Vector2.one, 1f).WithImmediateBind(false);
            var scalar = MMotion.Create(0f, 1f, 1f).WithImmediateBind(false);
            var rotation = MMotion.Create(Quaternion.identity, Quaternion.identity, 1f).WithImmediateBind(false);
            MotionHandle[] handles =
            {
                vector3.BindToPosition(target), scalar.BindToPositionX(target), scalar.BindToPositionY(target), scalar.BindToPositionZ(target),
                vector2.BindToPositionXY(target), vector2.BindToPositionXZ(target), vector2.BindToPositionYZ(target),
                vector3.BindToLocalPosition(target), scalar.BindToLocalPositionX(target), scalar.BindToLocalPositionY(target), scalar.BindToLocalPositionZ(target),
                vector2.BindToLocalPositionXY(target), vector2.BindToLocalPositionXZ(target), vector2.BindToLocalPositionYZ(target),
                rotation.BindToRotation(target), rotation.BindToLocalRotation(target),
                vector3.BindToEulerAngles(target), scalar.BindToEulerAnglesX(target), scalar.BindToEulerAnglesY(target), scalar.BindToEulerAnglesZ(target),
                vector2.BindToEulerAnglesXY(target), vector2.BindToEulerAnglesXZ(target), vector2.BindToEulerAnglesYZ(target),
                vector3.BindToLocalEulerAngles(target), scalar.BindToLocalEulerAnglesX(target), scalar.BindToLocalEulerAnglesY(target), scalar.BindToLocalEulerAnglesZ(target),
                vector2.BindToLocalEulerAnglesXY(target), vector2.BindToLocalEulerAnglesXZ(target), vector2.BindToLocalEulerAnglesYZ(target),
                vector3.BindToLocalScale(target), scalar.BindToLocalScaleX(target), scalar.BindToLocalScaleY(target), scalar.BindToLocalScaleZ(target),
                vector2.BindToLocalScaleXY(target), vector2.BindToLocalScaleXZ(target), vector2.BindToLocalScaleYZ(target), scalar.BindToLocalScaleXYZ(target)
            };

            try
            {
                Assert.That(handles.Length, Is.EqualTo(38));
                foreach (MotionHandle handle in handles)
                {
                    Assert.That(handle.IsValid(), Is.True);
                    handle.TryCancel();
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void TransformExtensions_RejectNullTarget()
        {
            Transform target = null;
            Assert.Throws<ArgumentNullException>(() => MMotion.Create(Vector3.zero, Vector3.one, 1f).BindToPosition(target));
        }

        [Test]
        public void UnityObjectExtensions_BindPropertiesAndComponents()
        {
            var cameraObject = new GameObject("MyMotion Camera Extension Test");
            var audioObject = new GameObject("MyMotion Audio Extension Test");
            var spriteObject = new GameObject("MyMotion Sprite Extension Test");
            var rigidbodyObject = new GameObject("MyMotion Rigidbody Extension Test");
            var rigidbody2DObject = new GameObject("MyMotion Rigidbody2D Extension Test");
            Camera camera = cameraObject.AddComponent<Camera>();
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            Rigidbody rigidbody = rigidbodyObject.AddComponent<Rigidbody>();
            Rigidbody2D rigidbody2D = rigidbody2DObject.AddComponent<Rigidbody2D>();
            rigidbody.useGravity = false;

            try
            {
                MotionHandle cameraHandle = MMotion.Create(60f, 90f, 1f).WithImmediateBind(false).BindToFieldOfView(camera);
                MMotion.Update(0.5d);
                Assert.That(camera.fieldOfView, Is.EqualTo(75f).Within(0.0001f));
                cameraHandle.TryCancel();

                spriteRenderer.color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
                MotionHandle spriteHandle = MMotion.Create(0f, 1f, 1f).WithImmediateBind(false).BindToColorA(spriteRenderer);
                MMotion.Update(0.5d);
                Assert.That(spriteRenderer.color, Is.EqualTo(new Color(0.1f, 0.2f, 0.3f, 0.5f)));
                spriteHandle.TryCancel();

                rigidbody.position = new Vector3(1f, 2f, 3f);
                MotionHandle rigidbodyHandle = MMotion.Create(0f, 10f, 1f).WithImmediateBind(false).BindToPositionY(rigidbody, false);
                MMotion.Update(0.5d);
                Assert.That(rigidbody.position, Is.EqualTo(new Vector3(1f, 5f, 3f)));
                rigidbodyHandle.TryCancel();

                rigidbody2D.position = new Vector2(1f, 2f);
                MotionHandle rigidbody2DHandle = MMotion.Create(0f, 90f, 1f).WithImmediateBind(false).BindToRotation(rigidbody2D, false);
                MMotion.Update(0.5d);
                Assert.That(rigidbody2D.rotation, Is.EqualTo(45f).Within(0.0001f));
                rigidbody2DHandle.TryCancel();

                var progress = new RecordingProgress<float>();
                MotionHandle progressHandle = MMotion.Create(0f, 1f, 1f).WithImmediateBind(false).BindToProgress(progress);
                MMotion.Update(0.5d);
                Assert.That(progress.Value, Is.EqualTo(0.5f).Within(0.0001f));
                progressHandle.TryCancel();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(audioObject);
                UnityEngine.Object.DestroyImmediate(spriteObject);
                UnityEngine.Object.DestroyImmediate(rigidbodyObject);
                UnityEngine.Object.DestroyImmediate(rigidbody2DObject);
            }
        }

        [Test]
        public void UnityObjectExtensions_ExposeCommonLitMotionSurface()
        {
            var cameraObject = new GameObject("MyMotion Camera Extension API Test");
            var audioObject = new GameObject("MyMotion Audio Extension API Test");
            var spriteObject = new GameObject("MyMotion Sprite Extension API Test");
            var rigidbodyObject = new GameObject("MyMotion Rigidbody Extension API Test");
            var rigidbody2DObject = new GameObject("MyMotion Rigidbody2D Extension API Test");
            Camera camera = cameraObject.AddComponent<Camera>();
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            Rigidbody rigidbody = rigidbodyObject.AddComponent<Rigidbody>();
            Rigidbody2D rigidbody2D = rigidbody2DObject.AddComponent<Rigidbody2D>();
            var scalar = MMotion.Create(0f, 1f, 1f).WithImmediateBind(false);
            var vector2 = MMotion.Create(Vector2.zero, Vector2.one, 1f).WithImmediateBind(false);
            var vector3 = MMotion.Create(Vector3.zero, Vector3.one, 1f).WithImmediateBind(false);
            var rotation = MMotion.Create(Quaternion.identity, Quaternion.identity, 1f).WithImmediateBind(false);
            var rect = MMotion.Create(new Rect(0f, 0f, 0f, 0f), new Rect(1f, 1f, 1f, 1f), 1f).WithImmediateBind(false);
            var color = MMotion.Create(Color.black, Color.white, 1f).WithImmediateBind(false);
            var progress = new RecordingProgress<float>();
            MotionHandle[] handles =
            {
                vector3.BindToPosition(rigidbody), scalar.BindToPositionX(rigidbody), scalar.BindToPositionY(rigidbody), scalar.BindToPositionZ(rigidbody),
                vector2.BindToPositionXY(rigidbody), vector2.BindToPositionYZ(rigidbody), vector2.BindToPositionXZ(rigidbody), rotation.BindToRotation(rigidbody),
                vector2.BindToPosition(rigidbody2D), scalar.BindToPositionX(rigidbody2D), scalar.BindToPositionY(rigidbody2D), scalar.BindToRotation(rigidbody2D),
                scalar.BindToAspect(camera), scalar.BindToNearClipPlane(camera), scalar.BindToFarClipPlane(camera), scalar.BindToFieldOfView(camera),
                scalar.BindToOrthographicSize(camera), rect.BindToRect(camera), rect.BindToPixelRect(camera), color.BindToBackgroundColor(camera),
                scalar.BindToVolume(audioSource), scalar.BindToPitch(audioSource),
                color.BindToColor(spriteRenderer), scalar.BindToColorR(spriteRenderer), scalar.BindToColorG(spriteRenderer), scalar.BindToColorB(spriteRenderer), scalar.BindToColorA(spriteRenderer),
                scalar.BindToProgress(progress)
            };

            try
            {
                Assert.That(handles.Length, Is.EqualTo(28));
                foreach (MotionHandle handle in handles)
                {
                    Assert.That(handle.IsValid(), Is.True);
                    handle.TryCancel();
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(audioObject);
                UnityEngine.Object.DestroyImmediate(spriteObject);
                UnityEngine.Object.DestroyImmediate(rigidbodyObject);
                UnityEngine.Object.DestroyImmediate(rigidbody2DObject);
            }
        }

        [Test]
        public void UnityObjectExtensions_RejectNullTargets()
        {
            Camera camera = null;
            Assert.Throws<ArgumentNullException>(() => MMotion.Create(0f, 1f, 1f).BindToAspect(camera));
        }

        [Test]
        public void GeneralExtensions_ExposeRemainingLitMotionSurface()
        {
            Shader shader = Shader.Find("Hidden/InternalErrorShader");
            Assert.That(shader, Is.Not.Null);
            var material = new Material(shader);
            var scalar = MMotion.Create(0f, 1f, 1f).WithImmediateBind(false);
            var integer = MMotion.Create(0, 1, 1f).WithImmediateBind(false);
            var color = MMotion.Create(Color.black, Color.white, 1f).WithImmediateBind(false);
            MotionHandle[] handles =
            {
                scalar.BindToUnityLogger(), scalar.BindToUnityLogger("{0}"),
                scalar.BindToUnityLogger(Debug.unityLogger), scalar.BindToUnityLogger(Debug.unityLogger, "{0}"),
                scalar.BindToMaterialFloat(material, "_Value"), scalar.BindToMaterialFloat(material, Shader.PropertyToID("_Value")),
                integer.BindToMaterialInt(material, "_Value"), integer.BindToMaterialInt(material, Shader.PropertyToID("_Value")),
                color.BindToMaterialColor(material, "_Color"), color.BindToMaterialColor(material, Shader.PropertyToID("_Color"))
            };

            try
            {
                Assert.That(handles.Length, Is.EqualTo(10));
                foreach (MotionHandle handle in handles)
                {
                    Assert.That(handle.IsValid(), Is.True);
                    handle.TryCancel();
                }

                AudioMixer audioMixer = null;
                Assert.Throws<ArgumentNullException>(() => scalar.BindToAudioMixerFloat(audioMixer, "Volume"));
                Material missingMaterial = null;
                Assert.Throws<ArgumentNullException>(() => scalar.BindToMaterialFloat(missingMaterial, "_Value"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void UnityLoggerExtension_FormatsValueWithTwoStateBinding()
        {
            using ManualMotionDispatcher dispatcher = new ManualMotionDispatcher();
            RecordingLogHandler logHandler = new RecordingLogHandler();
            ILogger logger = new Logger(logHandler);

            MotionHandle handle = MMotion.Create(2f, 6f, 2f)
                .WithScheduler(dispatcher.Scheduler)
                .WithImmediateBind(false)
                .BindToUnityLogger(logger, "value={0}");

            dispatcher.Update(1d);

            Assert.That(logHandler.LastMessage, Is.EqualTo("value=4"));
            Assert.That(handle.IsActive(), Is.True);
            handle.TryCancel();
        }

        [Test]
        public void MaterialExtensions_StringNamesMatchPropertyIds()
        {
            Shader shader = Resources.Load<Shader>("MaterialPropertyIdBenchmark");
            Assert.That(shader, Is.Not.Null);
            var stringMaterial = new Material(shader);
            var idMaterial = new Material(shader);
            int floatId = Shader.PropertyToID("_Value");
            int integerId = Shader.PropertyToID("_IntegerValue");
            int colorId = Shader.PropertyToID("_Color");

            try
            {
                Assert.That(stringMaterial.HasProperty(floatId), Is.True);
                Assert.That(stringMaterial.HasProperty(integerId), Is.True);
                Assert.That(stringMaterial.HasProperty(colorId), Is.True);
                MotionHandle[] handles =
                {
                    MMotion.Create(0f, 10f, 1f).WithImmediateBind(false).BindToMaterialFloat(stringMaterial, "_Value"),
                    MMotion.Create(0f, 10f, 1f).WithImmediateBind(false).BindToMaterialFloat(idMaterial, floatId),
                    MMotion.Create(0, 10, 1f).WithImmediateBind(false).BindToMaterialInt(stringMaterial, "_IntegerValue"),
                    MMotion.Create(0, 10, 1f).WithImmediateBind(false).BindToMaterialInt(idMaterial, integerId),
                    MMotion.Create(Color.black, Color.white, 1f).WithImmediateBind(false).BindToMaterialColor(stringMaterial, "_Color"),
                    MMotion.Create(Color.black, Color.white, 1f).WithImmediateBind(false).BindToMaterialColor(idMaterial, colorId)
                };

                MMotion.Update(0.5d);

                Assert.That(stringMaterial.GetFloat(floatId), Is.EqualTo(idMaterial.GetFloat(floatId)));
                Assert.That(stringMaterial.GetInteger(integerId), Is.EqualTo(idMaterial.GetInteger(integerId)));
                Assert.That(stringMaterial.GetColor(colorId), Is.EqualTo(idMaterial.GetColor(colorId)));

                foreach (MotionHandle handle in handles) handle.TryCancel();
                MMotion.Update(0d);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(stringMaterial);
                UnityEngine.Object.DestroyImmediate(idMaterial);
            }
        }
    }
}

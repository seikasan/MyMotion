using MyMotion.Extensions;
#if MYMOTION_TEST_UNIRX
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;
using NUnit.Framework;

namespace MyMotion.Tests.Upstream
{
    public class UniRxTest
    {
        readonly CompositeDisposable disposables = new();

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            disposables.Dispose();
        }

        [UnityTest]
        public IEnumerator Test_ToObservable()
        {
            bool completed = false;
            MMotion.Create(0f, 10f, 2f)
                .WithOnComplete(() => completed = true)
                .ToRxObservable()
                .Subscribe(x => Debug.Log(x))
                .AddTo(disposables);
            while (!completed) yield return null;
        }
        
        [UnityTest]
        public IEnumerator Test_BindToReactiveProperty()
        {
            var reactiveProperty = new ReactiveProperty<float>();
            reactiveProperty.AddTo(disposables);

            bool completed = false;
            MMotion.Create(0f, 10f, 2f)
                .WithOnComplete(() => completed = true)
                .BindToReactiveProperty(reactiveProperty)
                .ToDisposable()
                .AddTo(disposables);

            reactiveProperty.Subscribe(x =>
            {
                Debug.Log(x);
            })
            .AddTo(disposables);

            while (!completed) yield return null;
        }
    }
}
#endif
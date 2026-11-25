using MyMotion.Extensions;
#if MYMOTION_TEST_R3
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using R3;
using NUnit.Framework;

namespace MyMotion.Tests.Upstream
{
    public class R3Test
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
                .ToR3Observable()
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
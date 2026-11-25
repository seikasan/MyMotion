using MyMotion.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyMotion.Tests.Upstream
{
    public class EaseTest
    {
        [UnityTest]
        public IEnumerator Test_AnimationCurve()
        {
            var curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            for (int i = 0; i < 10; i++)
            {
                yield return MMotion.Create(0f, 10f, 0.2f)
                    .WithEase(curve)
                    .RunWithoutBinding()
                    .ToYieldInstruction();
            }
        }
    }
}

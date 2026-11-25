using System.Collections;
using MyMotion;
using MyMotion.Extensions;
using UnityEngine.TestTools;

namespace MyMotion.Tests.Upstream
{
    public class CoroutineTest
    {
        [UnityTest]
        public IEnumerator Test_ToYieldInteraction()
        {
            yield return MMotion.Create(0f, 10f, 3f)
                .BindToUnityLogger()
                .ToYieldInstruction();
        }
    }
}
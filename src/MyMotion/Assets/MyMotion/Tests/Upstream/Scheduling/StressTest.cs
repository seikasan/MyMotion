using MyMotion.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyMotion.Tests.Upstream
{
    public class StressTest
    {
        [UnityTest]
        public IEnumerator StressTest_64000_Float()
        {
            for (int i = 0; i < 64000; i++)
            {
                MMotion.Create(0, 1f, 1f)
                    .Bind(x => { });
            }

            yield return new WaitForSeconds(1.1f);
        }
    }
}

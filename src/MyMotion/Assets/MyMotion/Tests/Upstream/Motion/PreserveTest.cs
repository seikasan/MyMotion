using MyMotion.Extensions;
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MyMotion.Tests.Upstream
{
    public class PreserveTest
    {
        // MyMotion の builder は不変な値型で、再利用を仕様として許可している（MotionBuilder の remarks 参照）。
        [Test, Ignore("MyMotion allows builder reuse by design")]
        public void Test_Error_ReuseBuiler()
        {
            using var builder = MMotion.Create(0f, 1f, 0.1f);

            Assert.Throws<InvalidOperationException>(() =>
            {
                builder.RunWithoutBinding();
                builder.RunWithoutBinding();
            });
        }
    }
}
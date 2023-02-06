using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Unity.Formats.USD.Tests
{
    public class PerformanceTests : BaseFixture
    {
        [Test, Performance]
        public void TestTest()
        {
            var a = Vector2.one;
            var b = Vector2.zero;

            Measure.Method(() =>
            {
                Vector2.MoveTowards(a, b, 0.5f);
                Vector2.ClampMagnitude(a, 0.5f);
                Vector2.Reflect(a, b);
                Vector2.SignedAngle(a, b);
            })
                .MeasurementCount(20)
                .Run();
        }
    }
}

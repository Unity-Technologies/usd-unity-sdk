using NUnit.Framework;
using UnityEngine;
using USD.NET.Tests;

namespace USD.NET.Unity.Tests
{
    class XformTests : UsdTests
    {
        [Test]
        public static void XformTest()
        {
            var sample = new USD.NET.Unity.XformSample();
            var sample2 = new USD.NET.Unity.XformSample();

            sample.transform = Matrix4x4.identity;
            WriteAndRead(ref sample, ref sample2);

            AssertEqual(sample.transform, sample2.transform);
            AssertEqual(sample.xformOpOrder, sample2.xformOpOrder);

            // Matrix4x4.TRS cannot be used outside of of the actual Unity runtime.
            // Using GfMatrix here instead.
            var m = new pxr.GfMatrix4d();
            m.SetScale(new pxr.GfVec3d(8, 9, 10));
            m.SetTranslateOnly(new pxr.GfVec3d(1, 2, 3));
            m.SetRotateOnly(new pxr.GfQuatd(4, 5, 6, 7));
            sample.transform = USD.NET.Unity.UnityTypeConverter.FromMatrix(m);

            sample2 = new USD.NET.Unity.XformSample();
            WriteAndRead(ref sample, ref sample2);
            AssertEqual(sample.transform, sample2.transform);
            AssertEqual(sample.xformOpOrder, sample2.xformOpOrder);

            sample.ConvertTransform();
            sample2 = new USD.NET.Unity.XformSample();
            WriteAndRead(ref sample, ref sample2);
            AssertEqual(sample.transform, sample2.transform);
            AssertEqual(sample.xformOpOrder, sample2.xformOpOrder);

            sample.ConvertTransform();
            sample2.ConvertTransform();
            AssertEqual(sample.transform, sample2.transform);
            AssertEqual(sample.xformOpOrder, sample2.xformOpOrder);
        }

        public static void Xform2Test()
        {
            var sample = new USD.NET.Unity.XformSample();
            var sample2 = new USD.NET.Unity.XformSample();

            var mat = new Matrix4x4();
            for (int i = 0; i < 16; i++)
            {
                mat[i] = i;
            }

            sample.transform = mat;

            WriteAndRead(ref sample, ref sample2);

            AssertEqual(sample2.transform, sample.transform);
            AssertEqual(sample2.xformOpOrder, sample.xformOpOrder);
        }
    }
}

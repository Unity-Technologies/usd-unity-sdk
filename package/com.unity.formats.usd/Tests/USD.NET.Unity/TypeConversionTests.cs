using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using USD.NET.Tests;

namespace USD.NET.Unity.Tests
{
    class TypeConversionTests : UsdTests
    {
        class RectBoundsSample : SampleBase
        {
            public Rect rect;
            public Bounds bounds;
        }

        class ColorSample : SampleBase
        {
            public Color32 color32;
            // public List<Color32> color32List;
            // public Color32[] color32Array;

            public Color color;
            // public List<Color> colorList;
            // public Color[] colorArray;
        }

        class VectorSample : SampleBase
        {
            public Vector2 v2;
            public Vector2[] v2Array;
            public List<Vector2> v2List;

            public Vector3 v3;
            public Vector3[] v3Array;
            public List<Vector3> v3List;

            public Vector4 v4;
            public Vector4[] v4Array;
            public List<Vector4> v4List;
        }

        class MatrixSample : SampleBase
        {
            public Matrix4x4 m44;
            public Matrix4x4[] m44Array;
            public List<Matrix4x4> m44List;
        }

        class QuaternionSample : SampleBase
        {
            public Quaternion quat;
            public Quaternion[] quatArray;
            public List<Quaternion> quatList;
        }

        [Test]
        public void QuaternionTest()
        {
            var sampleW = new QuaternionSample();
            sampleW.quatArray = new[]
            {
                new Quaternion(1, 2, 3, 4),
                new Quaternion(5, 6, 7, 8)
            };
            sampleW.quat = sampleW.quatArray[0];
            sampleW.quatList = sampleW.quatArray.ToList();

            var sampleR = new QuaternionSample();
            WriteAndRead(ref sampleW, ref sampleR);
            Assert.AreEqual(sampleW.quat, sampleR.quat);
            AssertEqual(sampleW.quatArray, sampleR.quatArray);
            AssertEqual(sampleW.quatList, sampleR.quatList);
        }

        [Test]
        public void VectorTest()
        {
            var sampleW = new VectorSample();
            sampleW.v2Array = new[]
            {
                new Vector2(0, 1),
                new Vector2(2, 3),
            };
            sampleW.v2 = sampleW.v2Array[0];
            sampleW.v2List = sampleW.v2Array.ToList();

            sampleW.v3Array = new[]
            {
                new Vector3(0, 1, 2),
                new Vector3(3, 4, 5),
            };
            sampleW.v3 = sampleW.v3Array[0];
            sampleW.v3List = sampleW.v3Array.ToList();

            sampleW.v4Array = new[]
            {
                new Vector4(0, 1, 2, 3),
                new Vector4(4, 5, 6, 7),
            };
            sampleW.v4 = sampleW.v4Array[0];
            sampleW.v4List = sampleW.v4Array.ToList();

            var sampleR = new VectorSample();
            WriteAndRead(ref sampleW, ref sampleR);

            Assert.AreEqual(sampleW.v2, sampleR.v2);
            AssertEqual(sampleW.v2Array, sampleR.v2Array);
            AssertEqual(sampleW.v2List, sampleR.v2List);

            Assert.AreEqual(sampleW.v3, sampleR.v3);
            AssertEqual(sampleW.v3Array, sampleR.v3Array);
            AssertEqual(sampleW.v3List, sampleR.v3List);

            Assert.AreEqual(sampleW.v4, sampleR.v4);
            AssertEqual(sampleW.v4Array, sampleR.v4Array);
            AssertEqual(sampleW.v4List, sampleR.v4List);
        }

        [Test]
        public void ColorTest()
        {
            var sampleW = new ColorSample();
            sampleW.color = Color.green;
            // sampleW.colorArray = Enumerable.Repeat(Color.blue, 2).ToArray(),
            // sampleW.colorList = sampleW.colorArray.ToList(),

            sampleW.color32 = new Color32(32, 64, 128, 255);
            // sampleW.color32Array = Enumerable.Repeat(new Color32(32, 64, 128, 255), 2).ToArray(),
            // sampleW.color32List = sampleW.color32Array.ToList(),

            var sampleR = new ColorSample();
            WriteAndRead(ref sampleW, ref sampleR);

            Assert.AreEqual(sampleW.color, sampleR.color);
            // AssertEqual(sampleW.colorArray, sampleR.colorArray);
            // AssertEqual(sampleW.colorList, sampleR.colorList);

            Assert.AreEqual(sampleW.color32, sampleR.color32);
            // AssertEqual(sampleW.color32Array, sampleR.color32Array);
            // AssertEqual(sampleW.color32List, sampleR.color32List);
        }

        [Test]
        public void RectBoundsTest()
        {
            var sampleW = new RectBoundsSample();
            sampleW.rect = Rect.MinMaxRect(-5, -5, 5, 5);
            sampleW.bounds = new Bounds(new Vector3(1, 2, 3), new Vector3(1, 2, 3));

            var sampleR = new RectBoundsSample();
            WriteAndRead(ref sampleW, ref sampleR);

            Assert.AreEqual(sampleW.bounds, sampleR.bounds);
            Assert.AreEqual(sampleW.rect, sampleR.rect);
        }

        [Test]
        public void MatrixTest()
        {
            var sampleW = new MatrixSample();
            sampleW.m44 = Matrix4x4.identity;
            sampleW.m44Array = Enumerable.Repeat(Matrix4x4.identity, 2).ToArray();
            sampleW.m44List = sampleW.m44Array.ToList();

            var sampleR = new MatrixSample();
            WriteAndRead(ref sampleW, ref sampleR);

            Assert.AreEqual(sampleW.m44, sampleR.m44);
            AssertEqual(sampleW.m44Array, sampleR.m44Array);
            AssertEqual(sampleW.m44List, sampleR.m44List);
        }

        [Test]
        public void GetPathTests()
        {
            var a = new GameObject("A");
            var b = new GameObject("B");
            b.transform.SetParent(a.transform);
            var c = new GameObject("C");
            c.transform.SetParent(b.transform);

            Assert.AreEqual("/A/B/C", UnityTypeConverter.GetPath(c.transform));
            Assert.AreEqual("/A", UnityTypeConverter.GetPath(a.transform));
            Assert.AreEqual("/B/C", UnityTypeConverter.GetPath(c.transform, a.transform));
            Assert.AreEqual("", UnityTypeConverter.GetPath(c.transform, c.transform));
            Assert.AreEqual("", UnityTypeConverter.GetPath(null));
            Assert.AreEqual("", UnityTypeConverter.GetPath(null, null));
            Assert.Throws<System.Exception>(delegate () { UnityTypeConverter.GetPath(null, a.transform); });
        }

        [Test]
        public void HasPreserveAttribute()
        {
            foreach (var method in typeof(UnityTypeConverter).GetMethods())
            {
                var name = method.Name;
                if (name.Contains("ToVt") || name.Contains("FromVt") ||
                    name.Contains("Gf") || name.Contains("FromMatrix") ||
                    name.Contains("ToVec4f") || name.Contains("Vec4fTo"))
                {
                    var attrs = method.GetCustomAttributes(true);
                    Assert.IsTrue(attrs.Any(attr => attr.GetType().Name == "PreserveAttribute"));
                }
            }
        }
    }
}

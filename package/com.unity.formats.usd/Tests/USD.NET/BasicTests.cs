// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace USD.NET.Tests
{
    class BasicTests : VariabilityTests
    {
        class IntrinsicsSample : USD.NET.SampleBase
        {
            public bool bool_;
            public byte byte_;
            public int int_;
            public uint uint_;
            public long long_;
            public ulong ulong_;
            public string string_;
            public float float_;
            public double double_;

            public bool[] boolArray_;
            public byte[] byteArray_;
            public int[] intArray_;
            public uint[] uintArray_;
            public long[] longArray_;
            public ulong[] ulongArray_;
            public string[] stringArray_;
            public float[] floatArray_;
            public double[] doubleArray_;

            public List<bool> boolList_;
            public List<byte> byteList_;
            public List<int> intList_;
            public List<uint> uintList_;
            public List<long> longList_;
            public List<ulong> ulongList_;
            public List<string> stringList_;
            public List<float> floatList_;
            public List<double> doubleList_;

            [USD.NET.UsdNamespace("customNamespace")]
            public Dictionary<string, object> dict;

            [USD.NET.VertexData] public Dictionary<string, float[]> dictVertexData;

            public Dictionary<string, USD.NET.Primvar<float[]>> dictPrimvar;

            public Dictionary<string, string> dictTyped;
            public Dictionary<string, IntPtr> dictUnknown;
        }

        class AssetPathSample : USD.NET.SampleBase
        {
            public pxr.SdfAssetPath assetPath;
            public pxr.SdfAssetPath[] assetPathArray;
            public List<pxr.SdfAssetPath> assetPathList;

            static private readonly string m_path = "C:/foo/bar/baz.usd";
            static private readonly string m_path2 = "C:/garply/quz.usd";

            public static AssetPathSample GetTestSample()
            {
                var sample = new AssetPathSample();
                sample.assetPath = new pxr.SdfAssetPath(m_path);
                sample.assetPathArray = new pxr.SdfAssetPath[]
                {
                    new pxr.SdfAssetPath(m_path),
                    new pxr.SdfAssetPath(m_path2)
                };
                sample.assetPathList = sample.assetPathArray.ToList();
                return sample;
            }

            public void Verify()
            {
                var baselineSample = GetTestSample();
                AssertEqual(baselineSample.assetPath, assetPath);
                AssertEqual(baselineSample.assetPathArray, assetPathArray);
                AssertEqual(baselineSample.assetPathList, assetPathList);
            }
        }

        class PrimvarSample : USD.NET.SampleBase
        {
            public class NestedSample : USD.NET.SampleBase
            {
                // Because an outer namespace was declared, this results in the namespace
                // "primvars:nested:foo:bar:baz"
                [USD.NET.UsdNamespace("foo:bar")]
                [USD.NET.VertexData(4)]
                public int[] baz;

                // Not a primvar, so the resulting namespace is:
                // "nested:foo:bar:garply"
                [USD.NET.UsdNamespace("foo:bar")] public int[] garply;

                [USD.NET.UsdNamespace("internal:dict")]
                public Dictionary<string, USD.NET.Primvar<float[]>> namespacedDict =
                    new Dictionary<string, USD.NET.Primvar<float[]>>();

                public Dictionary<string, USD.NET.Primvar<float[]>> vanillaDict =
                    new Dictionary<string, USD.NET.Primvar<float[]>>();
            }

            [USD.NET.VertexData()] public int[] somePrimvar;

            [USD.NET.VertexData(1)] public int[] somePrimvar1;

            [USD.NET.VertexData(2)] public int[] somePrimvar2;

            [USD.NET.UsdNamespace("skel")]
            [USD.NET.VertexData(3)]
            public int[] jointIndices;

            public Primvar<Color[]> colors;

            [USD.NET.UsdNamespace("nested")] public NestedSample nestedSample;

            public static PrimvarSample GetTestSample()
            {
                var sample = new PrimvarSample();
                sample.somePrimvar = new int[] { 1, 2, 3, 4 };
                sample.somePrimvar1 = new int[] { 2, 4, 6, 8 };
                sample.somePrimvar2 = new int[] { 9, 8, 7, 6 };
                sample.jointIndices = new int[] { 9, 8, 7, 6, 5, 3 };
                sample.nestedSample = new NestedSample();
                sample.nestedSample.baz = new int[] { 9, 8, 7, 1 };
                sample.nestedSample.garply = new int[] { 99, 88, 77 };

                var pv = new USD.NET.Primvar<float[]>();
                pv.value = new float[] { 123f };
                sample.nestedSample.namespacedDict["Foo"] = pv;

                pv = new USD.NET.Primvar<float[]>();
                pv.value = new float[] { 3245f };
                sample.nestedSample.vanillaDict["Bar"] = pv;

                sample.colors = new Primvar<Color[]>();
                sample.colors.SetValue(new Color[1] { Color.red });

                return sample;
            }
        }

        class ColorPrimvarSample : USD.NET.SampleBase
        {
            [USD.NET.VertexData(), FusedDisplayColor]
            public Color[] colorVD;

            public Primvar<Color[]> colorPV;

            public Primvar<object> colorObjPV;

            public Primvar<int[]> intPV;

            public static ColorPrimvarSample GetTestSample()
            {
                var sample = new ColorPrimvarSample();
                sample.colorVD = new[] { Color.green };

                sample.colorPV = new Primvar<Color[]>();
                sample.colorPV.SetValue(new Color[1] { Color.red });

                sample.colorObjPV = new Primvar<object>();
                sample.colorPV.SetValue(new Color[1] { Color.blue });

                sample.intPV = new Primvar<int[]>();
                sample.intPV.SetValue(new int[3] { 1, 2, 3 });

                return sample;
            }
        }

        class InheritNonSampleBaseSample : USD.NET.SampleBase
        {
            [USD.NET.UsdNamespace("foo")] public NonSampleBaseSample nonSampleBase;

            public class NonSampleBaseSample
            {
                public int foo = 0;
            }
        }

        class LateBoundSample : USD.NET.SampleBase
        {
            public object intValue;
            public object doubleValue;

            [USD.NET.UsdNamespace("nested")] public NestedLate nestedLate;

            public class NestedLate : USD.NET.SampleBase
            {
                public object floatValue = 0.0f;

                public NestedLate()
                {
                }

                public NestedLate(float v)
                {
                    floatValue = v;
                }
            }
        }

        void InitIntrinsicSample(ref IntrinsicsSample sample)
        {
            sample.boolArray_ = new bool[] { false, true };
            sample.boolList_ = sample.boolArray_.ToList();
            sample.bool_ = true;

            sample.byteArray_ = new byte[] { 1, 2, 3 };
            sample.byteList_ = sample.byteArray_.ToList();
            sample.byte_ = 42;

            sample.doubleArray_ = new double[] { -1.1, 2.2, double.MaxValue, double.MinValue };
            sample.doubleList_ = sample.doubleArray_.ToList();
            sample.double_ = double.MaxValue;

            sample.floatArray_ = new float[] { -1.1f, 2.2f, float.MaxValue, float.MinValue };
            sample.floatList_ = sample.floatArray_.ToList();
            sample.float_ = float.MaxValue;

            sample.intArray_ = new int[] { -1, 0, 1, 2, int.MaxValue, int.MinValue };
            sample.intList_ = sample.intArray_.ToList();
            sample.int_ = int.MaxValue;

            sample.longArray_ = new long[] { -1, 0, 2, long.MaxValue, long.MinValue };
            sample.longList_ = sample.longArray_.ToList();
            sample.long_ = long.MinValue;

            sample.stringArray_ = new string[] { "hello", "world" };
            sample.stringList_ = sample.stringArray_.ToList();
            sample.string_ = "foobar";

            sample.uintArray_ = new uint[] { 0, 1, 2, uint.MaxValue, uint.MinValue };
            sample.uintList_ = sample.uintArray_.ToList();
            sample.uint_ = uint.MaxValue;

            sample.ulongArray_ = new ulong[] { 0, 2, ulong.MaxValue, ulong.MinValue };
            sample.ulongList_ = sample.ulongArray_.ToList();
            sample.ulong_ = ulong.MaxValue;

            sample.dict = new Dictionary<string, object>();
            sample.dict["Foo"] = 1.2;

            sample.dictTyped = new Dictionary<string, string>();
            sample.dictTyped["Bar"] = "baz";

            sample.dictVertexData = new Dictionary<string, float[]>();
            sample.dictVertexData["VertexData"] = new float[] { 42.3f };

            sample.dictPrimvar = new Dictionary<string, USD.NET.Primvar<float[]>>();
            var pv = new USD.NET.Primvar<float[]>();
            pv.value = new float[] { 423.2f };
            pv.interpolation = USD.NET.PrimvarInterpolation.FaceVarying;
            sample.dictPrimvar["PrimvarValue"] = pv;
        }

        [Test]
        public void IntrinsicTypesTest()
        {
            // The sample to be serialized
            var sampleToWrite = new IntrinsicsSample();
            InitIntrinsicSample(ref sampleToWrite);

            // Sample to deserialize to. Dicts need to be initialized
            var sampleToRead = new IntrinsicsSample();
            sampleToRead.dict = new Dictionary<string, object>();
            sampleToRead.dictTyped = new Dictionary<string, string>();
            sampleToRead.dictVertexData = new Dictionary<string, float[]>();
            sampleToRead.dictPrimvar = new Dictionary<string, USD.NET.Primvar<float[]>>();

            // We don't know how to serialize an IntPtr, should throw an exception.
            sampleToWrite.dictUnknown = new Dictionary<string, IntPtr>();
            sampleToWrite.dictUnknown["Quz"] = new IntPtr(34292);
            sampleToRead.dictUnknown = new Dictionary<string, IntPtr>();
            Assert.Throws<ArgumentException>(delegate { WriteAndRead(ref sampleToWrite, ref sampleToRead); });
            sampleToRead.dictUnknown = null;
            sampleToWrite.dictUnknown = null;

            WriteAndRead(ref sampleToWrite, ref sampleToRead);

            // Check that everything has been properly serialized/deserialized
            AssertEqual(sampleToWrite.dict, sampleToRead.dict);
            AssertEqual(sampleToWrite.dict["Foo"], sampleToRead.dict["Foo"]);

            AssertEqual(sampleToWrite.dictTyped, sampleToRead.dictTyped);
            AssertEqual(sampleToWrite.dictTyped["Bar"], sampleToRead.dictTyped["Bar"]);

            AssertEqual(sampleToWrite.dictVertexData, sampleToRead.dictVertexData);
            AssertEqual(sampleToWrite.dictVertexData["VertexData"], sampleToRead.dictVertexData["VertexData"]);

            AssertEqual(sampleToWrite.dictPrimvar["PrimvarValue"].value,
                sampleToRead.dictPrimvar["PrimvarValue"].value);
            AssertEqual(sampleToWrite.dictPrimvar["PrimvarValue"].interpolation,
                sampleToRead.dictPrimvar["PrimvarValue"].interpolation);
            AssertEqual(sampleToWrite.dictPrimvar["PrimvarValue"].indices,
                sampleToRead.dictPrimvar["PrimvarValue"].indices);

            AssertEqual(sampleToWrite.boolArray_, sampleToRead.boolArray_);
            AssertEqual(sampleToWrite.byteArray_, sampleToRead.byteArray_);
            AssertEqual(sampleToWrite.doubleArray_, sampleToRead.doubleArray_);
            AssertEqual(sampleToWrite.floatArray_, sampleToRead.floatArray_);
            AssertEqual(sampleToWrite.intArray_, sampleToRead.intArray_);
            AssertEqual(sampleToWrite.longArray_, sampleToRead.longArray_);
            AssertEqual(sampleToWrite.stringArray_, sampleToRead.stringArray_);
            AssertEqual(sampleToWrite.uintArray_, sampleToRead.uintArray_);
            AssertEqual(sampleToWrite.ulongArray_, sampleToRead.ulongArray_);

            AssertEqual(sampleToRead.boolList_, sampleToRead.boolList_);
            AssertEqual(sampleToWrite.byteList_, sampleToRead.byteList_);
            AssertEqual(sampleToWrite.doubleList_, sampleToRead.doubleList_);
            AssertEqual(sampleToWrite.floatList_, sampleToRead.floatList_);
            AssertEqual(sampleToWrite.intList_, sampleToRead.intList_);
            AssertEqual(sampleToWrite.longList_, sampleToRead.longList_);
            AssertEqual(sampleToWrite.stringList_, sampleToRead.stringList_);
            AssertEqual(sampleToWrite.uintList_, sampleToRead.uintList_);
            AssertEqual(sampleToWrite.ulongList_, sampleToRead.ulongList_);

            AssertEqual(sampleToRead.bool_, sampleToRead.bool_);
            AssertEqual(sampleToWrite.byte_, sampleToRead.byte_);
            AssertEqual(sampleToWrite.double_, sampleToRead.double_);
            AssertEqual(sampleToWrite.float_, sampleToRead.float_);
            AssertEqual(sampleToWrite.int_, sampleToRead.int_);
            AssertEqual(sampleToWrite.long_, sampleToRead.long_);
            AssertEqual(sampleToWrite.string_, sampleToRead.string_);
            AssertEqual(sampleToWrite.uint_, sampleToRead.uint_);
            AssertEqual(sampleToWrite.ulong_, sampleToRead.ulong_);
        }

        [Test]
        public void IntrinsicTypesVariabilityTest()
        {
            // The first sample to be serialized
            var sampleToWrite = new IntrinsicsSample();
            InitIntrinsicSample(ref sampleToWrite);

            // Create a 2nd sample and make it slightly different so the sample gets written (serialization dedupe indentical values)
            var sampleToWrite2 = new IntrinsicsSample();
            InitIntrinsicSample(ref sampleToWrite2);
            sampleToWrite2.bool_ = false;

            //Check animated data
            TestDefaultTime_NotVarying(sampleToWrite);
            TestTimeOne_NotVarying(sampleToWrite);
            TestTimeOneTwo_SameSample_NotVarying(sampleToWrite);
            TestTimeOneTwo_DifferentSample_Varying(sampleToWrite, sampleToWrite2);
            TestTimeOneTwo_SameSample_RefNotPopulated(sampleToWrite, sampleToWrite2);
        }

        [Test]
        public static void SdfPathEqualityTest()
        {
            var A = new pxr.SdfPath("/Foo");
            var B = new pxr.SdfPath("/Foo");
            var C = new pxr.SdfPath("/Foo/Bar");

            AssertEqual(A, A);
            AssertEqual(B, B);
            AssertEqual(C, C);
            AssertEqual(A, B);
            AssertEqual(A, C.GetParentPath());
            AssertEqual(C.GetParentPath(), A);

            AssertEqual(A.GetHashCode(), A.GetHashCode());
            AssertEqual(B.GetHashCode(), B.GetHashCode());
            AssertEqual(C.GetHashCode(), C.GetHashCode());
            AssertEqual(A.GetHashCode(), B.GetHashCode());
            AssertEqual(A.GetHashCode(), C.GetParentPath().GetHashCode());
            AssertEqual(C.GetParentPath().GetHashCode(), A.GetHashCode());

            AssertEqual(A.GetHash(), A.GetHash());
            AssertEqual(B.GetHash(), B.GetHash());
            AssertEqual(C.GetHash(), C.GetHash());
            AssertEqual(A.GetHash(), B.GetHash());
            AssertEqual(A.GetHash(), C.GetParentPath().GetHash());
            AssertEqual(C.GetParentPath().GetHash(), A.GetHash());

            AssertEqual(A.ToString(), A.ToString());
            AssertEqual(B.ToString(), B.ToString());
            AssertEqual(C.ToString(), C.ToString());
            AssertEqual(A.ToString(), B.ToString());
            AssertEqual(A.ToString(), C.GetParentPath().ToString());
            AssertEqual(C.GetParentPath().ToString(), A.ToString());

            var hashSet = new HashSet<pxr.SdfPath>();
            hashSet.Add(A);
            Assert.True(hashSet.Contains(A));
            Assert.True(hashSet.Contains(B));
            Assert.True(hashSet.Contains(C.GetParentPath()));
            Assert.True(hashSet.Remove(B));

            hashSet.Add(B);
            Assert.True(hashSet.Contains(A));
            Assert.True(hashSet.Contains(B));
            Assert.True(hashSet.Contains(C.GetParentPath()));
            Assert.True(hashSet.Remove(C.GetParentPath()));

            var dict = new Dictionary<pxr.SdfPath, string>();
            dict.Add(A, A.GetString());
            Assert.True(dict.ContainsKey(A));
            Assert.True(dict.ContainsKey(B));
            Assert.True(dict.ContainsKey(C.GetParentPath()));
            Assert.True(dict.Remove(B));

            dict.Add(B, B.GetString());
            Assert.True(dict.ContainsKey(A));
            Assert.True(dict.ContainsKey(B));
            Assert.True(dict.ContainsKey(C.GetParentPath()));
            Assert.True(dict.Remove(C.GetParentPath()));
        }

        [Test]
        public static void VtValueEqualityTest()
        {
            var A = new pxr.VtValue(1);
            var B = new pxr.VtValue(1);
            var C = new pxr.VtValue(2);
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(1.1f);
            B = new pxr.VtValue(1.1f);
            C = new pxr.VtValue(1.3f);
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(1.1);
            B = new pxr.VtValue(1.1);
            C = new pxr.VtValue(1.2);
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(9L);
            B = new pxr.VtValue(9L);
            C = new pxr.VtValue(0L);
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue("string");
            B = new pxr.VtValue("string");
            C = new pxr.VtValue("foo");
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(new pxr.TfToken("token"));
            B = new pxr.VtValue(new pxr.TfToken("token"));
            C = new pxr.VtValue(new pxr.TfToken("foo"));
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(new pxr.GfVec2f(1.1f, 2.2f));
            B = new pxr.VtValue(new pxr.GfVec2f(1.1f, 2.2f));
            C = new pxr.VtValue(new pxr.GfVec2f(1.1f, 3.3f));
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(new pxr.GfVec3f(1.1f, 2.2f, 3.3f));
            B = new pxr.VtValue(new pxr.GfVec3f(1.1f, 2.2f, 3.3f));
            C = new pxr.VtValue(new pxr.GfVec3f(1.1f, 2.2f, 3.1f));
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f));
            B = new pxr.VtValue(new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f));
            C = new pxr.VtValue(new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.1f));
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(new pxr.VtVec2fArray(2, new pxr.GfVec2f(1.1f, 2.2f)));
            B = new pxr.VtValue(new pxr.VtVec2fArray(2, new pxr.GfVec2f(1.1f, 2.2f)));
            C = new pxr.VtValue(new pxr.VtVec2fArray(2, new pxr.GfVec2f(1.1f, 2.1f)));
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(new pxr.VtVec3fArray(2, new pxr.GfVec3f(1.1f, 2.2f, 3.3f)));
            B = new pxr.VtValue(new pxr.VtVec3fArray(2, new pxr.GfVec3f(1.1f, 2.2f, 3.3f)));
            C = new pxr.VtValue(new pxr.VtVec3fArray(1, new pxr.GfVec3f(1.1f, 2.2f, 3.3f)));
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);

            A = new pxr.VtValue(new pxr.VtVec4fArray(2, new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f)));
            B = new pxr.VtValue(new pxr.VtVec4fArray(2, new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f)));
            C = new pxr.VtValue(new pxr.VtVec4fArray(2, new pxr.GfVec4f(1.1f, 2.1f, 3.1f, 4.4f)));
            AssertEqual(A, B);
            Assert.AreNotEqual(A, C);
        }

        [Test]
        public static void AssetPathTest()
        {
            var sample = AssetPathSample.GetTestSample();
            var sample2 = new AssetPathSample();
            WriteAndRead(ref sample, ref sample2);
            sample2.Verify();
        }

        [Test]
        public static void PrimvarsTest()
        {
            var sampleToWrite = PrimvarSample.GetTestSample();
            var sampleToRead = new PrimvarSample();
            sampleToRead.nestedSample = new PrimvarSample.NestedSample();
            var sampleToWrite2 = PrimvarSample.GetTestSample();
            sampleToWrite2.somePrimvar = new int[] { 3, 4, 5, 6 };

            var scene = USD.NET.Scene.Create();
            scene.Write("/Foo", sampleToWrite);
            scene.Read("/Foo", sampleToRead);

            var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath("/Foo"));

            var garply = prim.GetAttribute(new pxr.TfToken("nested:foo:bar:garply"));
            Assert.True(garply.GetNamespace() == new pxr.TfToken("nested:foo:bar"));

            var primvar = new pxr.UsdGeomPrimvar(
                prim.GetAttribute(new pxr.TfToken("primvars:somePrimvar")));
            AssertEqual(primvar.GetElementSize(), 1);

            primvar = new pxr.UsdGeomPrimvar(
                prim.GetAttribute(new pxr.TfToken("primvars:somePrimvar1")));
            AssertEqual(primvar.GetElementSize(), 1);

            primvar = new pxr.UsdGeomPrimvar(
                prim.GetAttribute(new pxr.TfToken("primvars:somePrimvar2")));
            AssertEqual(primvar.GetElementSize(), 2);

            primvar = new pxr.UsdGeomPrimvar(
                prim.GetAttribute(new pxr.TfToken("primvars:skel:jointIndices")));
            AssertEqual(primvar.GetElementSize(), 3);

            primvar = new pxr.UsdGeomPrimvar(
                prim.GetAttribute(new pxr.TfToken("primvars:nested:foo:bar:baz")));
            AssertEqual(primvar.GetElementSize(), 4);

            primvar = new pxr.UsdGeomPrimvar(
                prim.GetAttribute(new pxr.TfToken("primvars:colors")));
            AssertEqual(primvar.GetElementSize(), 1);

            AssertEqual(sampleToWrite.somePrimvar, sampleToRead.somePrimvar);
            AssertEqual(sampleToWrite.somePrimvar1, sampleToRead.somePrimvar1);
            AssertEqual(sampleToWrite.somePrimvar2, sampleToRead.somePrimvar2);
            AssertEqual(sampleToWrite.jointIndices, sampleToRead.jointIndices);
            AssertEqual(sampleToWrite.colors, sampleToRead.colors);
            AssertEqual(sampleToWrite.nestedSample.baz, sampleToRead.nestedSample.baz);
            AssertEqual(sampleToWrite.nestedSample.garply, sampleToRead.nestedSample.garply);
            AssertEqual(sampleToWrite.nestedSample.namespacedDict["Foo"].value,
                sampleToRead.nestedSample.namespacedDict["Foo"].value);
            AssertEqual(sampleToWrite.nestedSample.vanillaDict["Bar"].value, sampleToRead.nestedSample.vanillaDict["Bar"].value);

            //
            // Test deserialization without nested object instantiated.
            //
            sampleToRead = new PrimvarSample();
            sampleToRead.nestedSample = null;
            scene.Read("/Foo", sampleToRead);

            AssertEqual(sampleToWrite.somePrimvar, sampleToRead.somePrimvar);
            AssertEqual(sampleToWrite.somePrimvar1, sampleToRead.somePrimvar1);
            AssertEqual(sampleToWrite.somePrimvar2, sampleToRead.somePrimvar2);
            AssertEqual(sampleToWrite.jointIndices, sampleToRead.jointIndices);
            AssertEqual(null, sampleToRead.nestedSample);
        }

        [Test]
        public void ColorPrimvarTest()
        {
            var sampleToWrite = ColorPrimvarSample.GetTestSample();
            var sampleToRead = new ColorPrimvarSample();

            var scene = USD.NET.Scene.Create();
            scene.Write("/Foo", sampleToWrite);
            scene.Read("/Foo", sampleToRead);


            AssertEqual(sampleToWrite.colorVD, sampleToRead.colorVD);
            AssertEqual(sampleToWrite.colorPV, sampleToRead.colorPV);
            AssertEqual(sampleToWrite.colorObjPV, sampleToRead.colorObjPV);
        }

        [Test]
        public static void PrimvarsVariabilityTest()
        {
            var sampleToWrite = PrimvarSample.GetTestSample();
            var sampleToWrite2 = PrimvarSample.GetTestSample();
            sampleToWrite2.somePrimvar = new int[] { 3, 4, 5, 6 };

            //Check animated data
            TestDefaultTime_NotVarying(sampleToWrite);
            TestTimeOne_NotVarying(sampleToWrite);
            TestTimeOneTwo_SameSample_NotVarying(sampleToWrite);
            TestTimeOneTwo_DifferentSample_Varying(sampleToWrite, sampleToWrite2);
            TestTimeOneTwo_SameSample_RefNotPopulated(sampleToWrite, sampleToWrite2);
        }

        [Test]
        public static void SampleBaseTest()
        {
            var scene = USD.NET.Scene.Create();
            var s1 = new InheritNonSampleBaseSample();
            var s2 = new InheritNonSampleBaseSample();

            try
            {
                s1.nonSampleBase = new InheritNonSampleBaseSample.NonSampleBaseSample();
                scene.Write("/Foo", s1);
                throw new Exception("Expected exception");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Non-SampleBase sample successfully threw exception on Write");
            }

            try
            {
                s2.nonSampleBase = new InheritNonSampleBaseSample.NonSampleBaseSample();
                scene.Read("/Foo", s2);
                throw new Exception("Expected exception");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Non-SampleBase sample successfully threw exception on Read\n");
            }

            var late1 = new LateBoundSample();
            var late2 = new LateBoundSample();
            late1.intValue = 42;
            late1.doubleValue = 99.44;
            late1.nestedLate = new LateBoundSample.NestedLate(.1f);

            late2.nestedLate = new LateBoundSample.NestedLate();
            WriteAndRead(ref late1, ref late2);

            scene.Close();
        }

        [Test]
        public static void ReverseBindingForAliasedType_NotFoundByDefault()
        {
            UsdTypeBinding binding;
            UsdIo.Bindings.GetReverseBinding(SdfValueTypeNames.Normal3f, out binding);
            Assert.Null(binding.sdfTypeName);
        }

        [Test]
        public static void ReverseBindingForAliasedType_FoundAfterAliasing()
        {
            UsdIo.Bindings.AddTypeAlias(SdfValueTypeNames.Normal3f, SdfValueTypeNames.Float3);
            UsdTypeBinding binding;
            UsdIo.Bindings.GetReverseBinding(SdfValueTypeNames.Normal3f, out binding);
            Assert.AreEqual(SdfValueTypeNames.Float3, binding.sdfTypeName);
            UsdIo.Bindings.RemoveTypeAlias(SdfValueTypeNames.Normal3f);
        }
    }
}

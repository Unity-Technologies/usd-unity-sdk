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

namespace Tests.Cases {
  class BasicTests : UnitTest {

    class MinimalSample : USD.NET.SampleBase {
      public int number;
      public int? number2;
      public USD.NET.Relationship rel;
    }

    class IntrinsicsSample : USD.NET.SampleBase {
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

      [USD.NET.VertexData]
      public Dictionary<string, float[]> dictVertexData;

      public Dictionary<string, USD.NET.Primvar<float[]>> dictPrimvar;

      public Dictionary<string, string> dictTyped;
      public Dictionary<string, IntPtr> dictUnknown;
    }

    class AssetPathSample : USD.NET.SampleBase {

      public pxr.SdfAssetPath assetPath;
      public pxr.SdfAssetPath[] assetPathArray;
      public List<pxr.SdfAssetPath> assetPathList;

      static private readonly string m_path = "C:/foo/bar/baz.usd";
      static private readonly string m_path2 = "C:/garply/quz.usd";

      public static AssetPathSample GetTestSample() {
        var sample = new AssetPathSample();
        sample.assetPath = new pxr.SdfAssetPath(m_path);
        sample.assetPathArray = new pxr.SdfAssetPath[] {
          new pxr.SdfAssetPath(m_path),
          new pxr.SdfAssetPath(m_path2)
        };
        sample.assetPathList = sample.assetPathArray.ToList();
        return sample;
      }

      public void Verify() {
        var baselineSample = GetTestSample();
        AssertEqual(baselineSample.assetPath, assetPath);
        AssertEqual(baselineSample.assetPathArray, assetPathArray);
        AssertEqual(baselineSample.assetPathList, assetPathList);
      }
    }

    class PrimvarSample : USD.NET.SampleBase {

      public class NestedSample : USD.NET.SampleBase {
        // Because an outer namespace was declared, this results in the namespace
        // "primvars:nested:foo:bar:baz"
        [USD.NET.UsdNamespace("foo:bar")]
        [USD.NET.VertexData(4)]
        public int[] baz;

        // Not a primvar, so the resulting namespace is:
        // "nested:foo:bar:garply"
        [USD.NET.UsdNamespace("foo:bar")]
        public int[] garply;

        [USD.NET.UsdNamespace("internal:dict")]
        public Dictionary<string, USD.NET.Primvar<float[]>> namespacedDict = new Dictionary<string, USD.NET.Primvar<float[]>>();

        public Dictionary<string, USD.NET.Primvar<float[]>> vanillaDict = new Dictionary<string, USD.NET.Primvar<float[]>>();
      }

      [USD.NET.VertexData()]
      public int[] somePrimvar;

      [USD.NET.VertexData(1)]
      public int[] somePrimvar1;

      [USD.NET.VertexData(2)]
      public int[] somePrimvar2;

      [USD.NET.UsdNamespace("skel")]
      [USD.NET.VertexData(3)]
      public int[] jointIndices;

      [USD.NET.UsdNamespace("nested")]
      public NestedSample nestedSample;

      public static PrimvarSample GetTestSample() {
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
        return sample;
      }
    }

    class InheritNonSampleBaseSample : USD.NET.SampleBase {
      [USD.NET.UsdNamespace("foo")]
      public NonSampleBaseSample nonSampleBase;

      public class NonSampleBaseSample {
        public int foo = 0;
      }
    }

    class LateBoundSample : USD.NET.SampleBase {
      public object intValue;
      public object doubleValue;

      [USD.NET.UsdNamespace("nested")]
      public NestedLate nestedLate;

      public class NestedLate : USD.NET.SampleBase {
        public object floatValue = 0.0f;
        public NestedLate() {
        }
        public NestedLate(float v) {
          floatValue = v;
        }
      }
    }

    public static void SmokeTest() {
      var sample = new MinimalSample();
      var sample2 = new MinimalSample();

      sample.number = 42;
      sample.number2 = null;
      WriteAndRead(ref sample, ref sample2, true);

      AssertEqual(sample2.number, sample.number);
    }

    public static void IntrinsicTypesTest() {
      var sample = new IntrinsicsSample();
      var sample2 = new IntrinsicsSample();

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

      sample2.dict = new Dictionary<string, object>();
      sample.dict = new Dictionary<string, object>();
      sample.dict["Foo"] = 1.2;

      sample2.dictTyped = new Dictionary<string, string>();
      sample.dictTyped = new Dictionary<string, string>();
      sample.dictTyped["Bar"] = "baz";

      sample2.dictVertexData = new Dictionary<string, float[]>();
      sample.dictVertexData = new Dictionary<string, float[]>();
      sample.dictVertexData["VertexData"] = new float[] { 42.3f };

      sample2.dictPrimvar = new Dictionary<string, USD.NET.Primvar<float[]>>();
      sample.dictPrimvar = new Dictionary<string, USD.NET.Primvar<float[]>>();
      var pv = new USD.NET.Primvar<float[]>();
      pv.value = new float[] { 423.2f };
      pv.interpolation = USD.NET.PrimvarInterpolation.FaceVarying;
      sample.dictPrimvar["PrimvarValue"] = pv;

      try {
        // We don't know how to serialize an IntPtr, should throw an exception.
        sample2.dictUnknown = new Dictionary<string, IntPtr>();
        sample.dictUnknown = new Dictionary<string, IntPtr>();
        sample.dictUnknown["Quz"] = new IntPtr(34292);
        WriteAndRead(ref sample, ref sample2, true);
        throw new Exception("Expected exception");
      } catch (ArgumentException) {
        Console.WriteLine("Caught expected exception");
      }

      sample2.dictUnknown = null;
      sample.dictUnknown = null;

      WriteAndRead(ref sample, ref sample2, true);
      TestVariability(sample);

      AssertEqual(sample.dict, sample2.dict);
      AssertEqual(sample.dict["Foo"], sample2.dict["Foo"]);

      AssertEqual(sample.dictTyped, sample2.dictTyped);
      AssertEqual(sample.dictTyped["Bar"], sample2.dictTyped["Bar"]);

      AssertEqual(sample.dictVertexData, sample2.dictVertexData);
      AssertEqual(sample.dictVertexData["VertexData"], sample2.dictVertexData["VertexData"]);

      AssertEqual(sample.dictPrimvar["PrimvarValue"].value, sample2.dictPrimvar["PrimvarValue"].value);
      AssertEqual(sample.dictPrimvar["PrimvarValue"].interpolation, sample2.dictPrimvar["PrimvarValue"].interpolation);
      AssertEqual(sample.dictPrimvar["PrimvarValue"].indices, sample2.dictPrimvar["PrimvarValue"].indices);

      AssertEqual(sample.boolArray_, sample2.boolArray_);
      AssertEqual(sample.byteArray_, sample2.byteArray_);
      AssertEqual(sample.doubleArray_, sample2.doubleArray_);
      AssertEqual(sample.floatArray_, sample2.floatArray_);
      AssertEqual(sample.intArray_, sample2.intArray_);
      AssertEqual(sample.longArray_, sample2.longArray_);
      AssertEqual(sample.stringArray_, sample2.stringArray_);
      AssertEqual(sample.uintArray_, sample2.uintArray_);
      AssertEqual(sample.ulongArray_, sample2.ulongArray_);

      AssertEqual(sample2.boolList_, sample2.boolList_);
      AssertEqual(sample.byteList_, sample2.byteList_);
      AssertEqual(sample.doubleList_, sample2.doubleList_);
      AssertEqual(sample.floatList_, sample2.floatList_);
      AssertEqual(sample.intList_, sample2.intList_);
      AssertEqual(sample.longList_, sample2.longList_);
      AssertEqual(sample.stringList_, sample2.stringList_);
      AssertEqual(sample.uintList_, sample2.uintList_);
      AssertEqual(sample.ulongList_, sample2.ulongList_);

      AssertEqual(sample2.bool_, sample2.bool_);
      AssertEqual(sample.byte_, sample2.byte_);
      AssertEqual(sample.double_, sample2.double_);
      AssertEqual(sample.float_, sample2.float_);
      AssertEqual(sample.int_, sample2.int_);
      AssertEqual(sample.long_, sample2.long_);
      AssertEqual(sample.string_, sample2.string_);
      AssertEqual(sample.uint_, sample2.uint_);
      AssertEqual(sample.ulong_, sample2.ulong_);
    }

    public static void SdfPathEqualityTest() {
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
      AssertTrue(hashSet.Contains(A));
      AssertTrue(hashSet.Contains(B));
      AssertTrue(hashSet.Contains(C.GetParentPath()));
      AssertTrue(hashSet.Remove(B));

      hashSet.Add(B);
      AssertTrue(hashSet.Contains(A));
      AssertTrue(hashSet.Contains(B));
      AssertTrue(hashSet.Contains(C.GetParentPath()));
      AssertTrue(hashSet.Remove(C.GetParentPath()));

      var dict = new Dictionary<pxr.SdfPath, string>();
      dict.Add(A, A.GetString());
      AssertTrue(dict.ContainsKey(A));
      AssertTrue(dict.ContainsKey(B));
      AssertTrue(dict.ContainsKey(C.GetParentPath()));
      AssertTrue(dict.Remove(B));

      dict.Add(B, B.GetString());
      AssertTrue(dict.ContainsKey(A));
      AssertTrue(dict.ContainsKey(B));
      AssertTrue(dict.ContainsKey(C.GetParentPath()));
      AssertTrue(dict.Remove(C.GetParentPath()));
    }

    public static void VtValueEqualityTest() {
      var A = new pxr.VtValue(1);
      var B = new pxr.VtValue(1);
      var C = new pxr.VtValue(2);
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(1.1f);
      B = new pxr.VtValue(1.1f);
      C = new pxr.VtValue(1.3f);
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(1.1);
      B = new pxr.VtValue(1.1);
      C = new pxr.VtValue(1.2);
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(9L);
      B = new pxr.VtValue(9L);
      C = new pxr.VtValue(0L);
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue("string");
      B = new pxr.VtValue("string");
      C = new pxr.VtValue("foo");
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(new pxr.TfToken("token"));
      B = new pxr.VtValue(new pxr.TfToken("token"));
      C = new pxr.VtValue(new pxr.TfToken("foo"));
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(new pxr.GfVec2f(1.1f, 2.2f));
      B = new pxr.VtValue(new pxr.GfVec2f(1.1f, 2.2f));
      C = new pxr.VtValue(new pxr.GfVec2f(1.1f, 3.3f));
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(new pxr.GfVec3f(1.1f, 2.2f, 3.3f));
      B = new pxr.VtValue(new pxr.GfVec3f(1.1f, 2.2f, 3.3f));
      C = new pxr.VtValue(new pxr.GfVec3f(1.1f, 2.2f, 3.1f));
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f));
      B = new pxr.VtValue(new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f));
      C = new pxr.VtValue(new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.1f));
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(new pxr.VtVec2fArray(2, new pxr.GfVec2f(1.1f, 2.2f)));
      B = new pxr.VtValue(new pxr.VtVec2fArray(2, new pxr.GfVec2f(1.1f, 2.2f)));
      C = new pxr.VtValue(new pxr.VtVec2fArray(2, new pxr.GfVec2f(1.1f, 2.1f)));
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(new pxr.VtVec3fArray(2, new pxr.GfVec3f(1.1f, 2.2f, 3.3f)));
      B = new pxr.VtValue(new pxr.VtVec3fArray(2, new pxr.GfVec3f(1.1f, 2.2f, 3.3f)));
      C = new pxr.VtValue(new pxr.VtVec3fArray(1, new pxr.GfVec3f(1.1f, 2.2f, 3.3f)));
      AssertEqual(A, B);
      AssertNotEqual(A, C);

      A = new pxr.VtValue(new pxr.VtVec4fArray(2, new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f)));
      B = new pxr.VtValue(new pxr.VtVec4fArray(2, new pxr.GfVec4f(1.1f, 2.2f, 3.3f, 4.4f)));
      C = new pxr.VtValue(new pxr.VtVec4fArray(2, new pxr.GfVec4f(1.1f, 2.1f, 3.1f, 4.4f)));
      AssertEqual(A, B);
      AssertNotEqual(A, C);
    }

    public static void AssetPathTest() {
      var sample = AssetPathSample.GetTestSample();
      var sample2 = new AssetPathSample();
      WriteAndRead(ref sample, ref sample2, true);
      sample2.Verify();
    }

    public static void PrimvarsTest() {
      var sample = PrimvarSample.GetTestSample();
      var sample2 = new PrimvarSample();
      var scene = USD.NET.Scene.Create();

      scene.Write("/Foo", sample);
      TestVariability(sample);

      PrintScene(scene);

      var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath("/Foo"));

      var garply = prim.GetAttribute(new pxr.TfToken("nested:foo:bar:garply"));
      AssertTrue(garply.GetNamespace() == new pxr.TfToken("nested:foo:bar"));

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


      sample2.nestedSample = new PrimvarSample.NestedSample();
      scene.Read("/Foo", sample2);

      TestVariability(sample2);

      AssertEqual(sample.somePrimvar, sample2.somePrimvar);
      AssertEqual(sample.somePrimvar1, sample2.somePrimvar1);
      AssertEqual(sample.somePrimvar2, sample2.somePrimvar2);
      AssertEqual(sample.jointIndices, sample2.jointIndices);
      AssertEqual(sample.nestedSample.baz, sample2.nestedSample.baz);
      AssertEqual(sample.nestedSample.garply, sample2.nestedSample.garply);
      AssertEqual(sample.nestedSample.namespacedDict["Foo"].value, sample2.nestedSample.namespacedDict["Foo"].value);
      AssertEqual(sample.nestedSample.vanillaDict["Bar"].value, sample2.nestedSample.vanillaDict["Bar"].value);

      //
      // Test deserialization without nested object instantiated.
      //
      sample2 = new PrimvarSample();
      sample2.nestedSample = null;
      scene.Read("/Foo", sample2);

      AssertEqual(sample.somePrimvar, sample2.somePrimvar);
      AssertEqual(sample.somePrimvar1, sample2.somePrimvar1);
      AssertEqual(sample.somePrimvar2, sample2.somePrimvar2);
      AssertEqual(sample.jointIndices, sample2.jointIndices);
      AssertEqual(null, sample2.nestedSample);

    }

    public static void SampleBaseTest() {
      var scene = USD.NET.Scene.Create();
      var s1 = new InheritNonSampleBaseSample();
      var s2 = new InheritNonSampleBaseSample();

      try {
        s1.nonSampleBase = new InheritNonSampleBaseSample.NonSampleBaseSample();
        scene.Write("/Foo", s1);
        throw new Exception("Expected exception");
      } catch (ArgumentException) {
        Console.WriteLine("Non-SampleBase sample successfully threw exception on Write");
      }

      try {
        s2.nonSampleBase = new InheritNonSampleBaseSample.NonSampleBaseSample();
        scene.Read("/Foo", s2);
        throw new Exception("Expected exception");
      } catch (ArgumentException) {
        Console.WriteLine("Non-SampleBase sample successfully threw exception on Read\n");
      }

      var late1 = new LateBoundSample();
      var late2 = new LateBoundSample();
      late1.intValue = 42;
      late1.doubleValue = 99.44;
      late1.nestedLate = new LateBoundSample.NestedLate(.1f);
      
      late2.nestedLate = new LateBoundSample.NestedLate();
      WriteAndRead(ref late1, ref late2, true);

      scene.Close();
    }

    public static void GetUsdObjectsTest() {
      var scene = USD.NET.Scene.Create();
      var sample = new MinimalSample();
      sample.number = 45;
      sample.rel = new USD.NET.Relationship("/Foo");
      
      scene.Write("/Foo", sample);
      PrintScene(scene);

      AssertTrue(scene.GetPrimAtPath("/Foo") != null);
      AssertTrue(scene.GetAttributeAtPath("/Foo.number") != null);
      AssertTrue(scene.GetRelationshipAtPath("/Foo.rel") != null);

      AssertTrue(scene.GetPrimAtPath("/Prim/Does/Not/Exist") == null);
      AssertTrue(scene.GetRelationshipAtPath("/Foo.number") == null);
      AssertTrue(scene.GetAttributeAtPath("/Foo.rel") == null);
    }
  }
}

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
  class Basic : UnitTest {

    class MinimalSample : USD.NET.SampleBase {
      public int number;
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
        return sample;
      }
    }

    public static void SmokeTest() {
      var sample = new MinimalSample();
      var sample2 = new MinimalSample();

      sample.number = 42;
      WriteAndRead(ref sample, ref sample2, true);

      if (sample2.number != sample.number) { throw new Exception("Values do not match"); }
    }

    public static void IntrinsicTypes() {
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

      WriteAndRead(ref sample, ref sample2, true);

      AssertEqual(sample2.boolArray_, sample2.boolArray_);
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

    public static void EqualityTest() {
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

    public static void TestAssetPath() {
      var sample = AssetPathSample.GetTestSample();
      var sample2 = new AssetPathSample();
      WriteAndRead(ref sample, ref sample2, true);
      sample2.Verify();
    }

    public static void TestPrimvars() {
      var sample = PrimvarSample.GetTestSample();
      var sample2 = new PrimvarSample();
      var scene = USD.NET.Scene.Create();

      scene.Write("/Foo", sample);

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

      AssertEqual(sample.somePrimvar, sample2.somePrimvar);
      AssertEqual(sample.somePrimvar1, sample2.somePrimvar1);
      AssertEqual(sample.somePrimvar2, sample2.somePrimvar2);
      AssertEqual(sample.jointIndices, sample2.jointIndices);
      AssertEqual(sample.nestedSample.baz, sample2.nestedSample.baz);
      AssertEqual(sample.nestedSample.garply, sample2.nestedSample.garply);

    }
  }
}

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
using UnityEngine;

namespace Tests.Cases {
  class UnityIoTests : UnitTest {

    class QuaternionSample : USD.NET.SampleBase {
      public Quaternion quaternion;
      public List<Quaternion> quaternionList;
      public Quaternion[] quaternionArray;

      public static QuaternionSample GetTestSample() {
        var quat = new QuaternionSample();
        quat.quaternion = new Quaternion(1, 2, 3, 4);
        quat.quaternionList = new List<Quaternion>();
        quat.quaternionList.Add(quat.quaternion);
        quat.quaternionList.Add(new Quaternion(5, 6, 7, 8));
        quat.quaternionArray = quat.quaternionList.ToArray();
        return quat;
      }

      public void Verify() {
        var q0 = new Quaternion(1, 2, 3, 4);
        var q1 = new Quaternion(5, 6, 7, 8);
        AssertEqual(q0, quaternion);
        AssertEqual(q0, quaternionList[0]);
        AssertEqual(q1, quaternionList[1]);
        AssertEqual(q0, quaternionArray[0]);
        AssertEqual(q1, quaternionArray[1]);
      }
    }

    class VectorSample : USD.NET.SampleBase {
      //
      // Vector2
      //
      public Vector2 vec2;
      public List<Vector2> vec2List;
      public Vector2[] vec2Array;

      //
      // Vector3
      //
      public Vector3 vec3;
      public List<Vector3> vec3List;
      public Vector3[] vec3Array;

      //
      // Vector4
      //
      public Vector4 vec4;
      public List<Vector4> vec4List;
      public Vector4[] vec4Array;

      static private Vector2[] GetVec2Array() {
        return new Vector2[] { new Vector2(1, 2), new Vector2(3, 4) };
      }

      static private Vector3[] GetVec3Array() {
        return new Vector3[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6) };
      }

      static private Vector4[] GetVec4Array() {
        return new Vector4[] { new Vector4(1, 2, 3, 4), new Vector4(5, 6, 7, 8) };
      }

      public static VectorSample GetTestSample() {
        var sample = new VectorSample();

        sample.vec2Array = GetVec2Array();
        sample.vec2 = sample.vec2Array[0];
        sample.vec2List = sample.vec2Array.ToList();

        sample.vec3Array = GetVec3Array();
        sample.vec3 = sample.vec3Array[0];
        sample.vec3List = sample.vec3Array.ToList();

        sample.vec4Array = GetVec4Array();
        sample.vec4 = sample.vec4Array[0];
        sample.vec4List = sample.vec4Array.ToList();
        return sample;
      }

      public void Verify() {
        var v2a = GetVec2Array();
        AssertEqual(v2a[0], vec2);
        AssertEqual(v2a, vec2Array);
        AssertEqual(v2a.ToList(), vec2List);

        var v3a = GetVec3Array();
        AssertEqual(v3a[0], vec3);
        AssertEqual(v3a, vec3Array);
        AssertEqual(v3a.ToList(), vec3List);

        var v4a = GetVec4Array();
        AssertEqual(v4a[0], vec4);
        AssertEqual(v4a, vec4Array);
        AssertEqual(v4a.ToList(), vec4List);
      }
    }

    public static void QuaternionTest() {
      var sample = QuaternionSample.GetTestSample();
      var sample2 = new QuaternionSample();
      WriteAndRead(ref sample, ref sample2, true);
      sample2.Verify();
    }

    public static void VectorsTest() {
      var sample = VectorSample.GetTestSample();
      var sample2 = new VectorSample();
      WriteAndRead(ref sample, ref sample2, true);
      sample2.Verify();
    }

    public static void XformTest() {
      var sample = new USD.NET.Unity.XformSample();
      var sample2 = new USD.NET.Unity.XformSample();

      sample.transform = Matrix4x4.identity;
      WriteAndRead(ref sample, ref sample2, true);

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
      WriteAndRead(ref sample, ref sample2, true);
      AssertEqual(sample.transform, sample2.transform);
      AssertEqual(sample.xformOpOrder, sample2.xformOpOrder);

      sample.ConvertTransform();
      sample2 = new USD.NET.Unity.XformSample();
      WriteAndRead(ref sample, ref sample2, true);
      AssertEqual(sample.transform, sample2.transform);
      AssertEqual(sample.xformOpOrder, sample2.xformOpOrder);

      sample.ConvertTransform();
      sample2.ConvertTransform();
      AssertEqual(sample.transform, sample2.transform);
      AssertEqual(sample.xformOpOrder, sample2.xformOpOrder);
    }

    public static void Xform2Test() {
      var sample = new USD.NET.Unity.XformSample();
      var sample2 = new USD.NET.Unity.XformSample();

      var mat = new Matrix4x4();
      for (int i = 0; i < 16; i++) {
        mat[i] = i;
      }
      sample.transform = mat;

      WriteAndRead(ref sample, ref sample2, true);

      AssertEqual(sample2.transform, sample.transform);
      AssertEqual(sample2.xformOpOrder, sample.xformOpOrder);
    }
  }
}

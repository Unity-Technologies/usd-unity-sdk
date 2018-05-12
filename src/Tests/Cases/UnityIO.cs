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

namespace Tests.Cases {
  class UnityIO : UnitTest {

    class QuaternionSample : USD.NET.SampleBase {
      public UnityEngine.Quaternion quaternion;
      public List<UnityEngine.Quaternion> quaternionList;
      public UnityEngine.Quaternion[] quaternionArray;

      public static QuaternionSample GetTestSample() {
        var quat = new QuaternionSample();
        quat.quaternion = new UnityEngine.Quaternion(1, 2, 3, 4);
        quat.quaternionList = new List<UnityEngine.Quaternion>();
        quat.quaternionList.Add(quat.quaternion);
        quat.quaternionList.Add(new UnityEngine.Quaternion(5, 6, 7, 8));
        quat.quaternionArray = quat.quaternionList.ToArray();
        return quat;
      }

      public void Verify() {
        var q0 = new UnityEngine.Quaternion(1, 2, 3, 4);
        var q1 = new UnityEngine.Quaternion(5, 6, 7, 8);
        AssertEqual(q0, quaternion);
        AssertEqual(q0, quaternionList[0]);
        AssertEqual(q1, quaternionList[1]);
        AssertEqual(q0, quaternionArray[0]);
        AssertEqual(q1, quaternionArray[1]);
      }
    }

    public static void TestQuaternion() {
      var sample = QuaternionSample.GetTestSample();
      var sample2 = new QuaternionSample();
      WriteAndRead(ref sample, ref sample2, true);
    }

    public static void TestXform() {
      var sample = new USD.NET.Unity.XformSample();
      var sample2 = new USD.NET.Unity.XformSample();

      sample.transform = UnityEngine.Matrix4x4.identity;

      WriteAndRead(ref sample, ref sample2, true);

      if (sample2.transform != sample.transform) { throw new Exception("Values do not match"); }
      if (sample2.xformOpOrder[0] != sample.xformOpOrder[0]) { throw new Exception("XformOpOrder do not match"); }
    }

    public static void TestXform2() {
      var sample = new USD.NET.Unity.XformSample();
      var sample2 = new USD.NET.Unity.XformSample();

      var mat = new UnityEngine.Matrix4x4();
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

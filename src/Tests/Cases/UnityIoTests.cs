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

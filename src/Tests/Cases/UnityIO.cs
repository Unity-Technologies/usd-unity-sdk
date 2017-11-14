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

namespace Tests.Cases {
  class UnityIO : UnitTest {

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

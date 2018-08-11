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
using pxr;

namespace Tests.Cases {
  class MeshTests : UnitTest {
    
    public static void VisibilityTest() {
      var sample = new USD.NET.Unity.MeshSample();
      var outSample = new USD.NET.Unity.MeshSample();
      sample.visibility = USD.NET.Visibility.Invisible;

      WriteAndRead(ref sample, ref outSample, true);

      AssertEqual(sample.visibility, outSample.visibility);
    }

    public static void TriangulationTest() {
      VtIntArray indices = new VtIntArray();
      VtIntArray faceCounts = new VtIntArray();

      faceCounts.push_back(5);
      indices.push_back(0);
      indices.push_back(1);
      indices.push_back(2);
      indices.push_back(3);
      indices.push_back(4);

      faceCounts.push_back(4);
      indices.push_back(5);
      indices.push_back(6);
      indices.push_back(7);
      indices.push_back(8);

      faceCounts.push_back(3);
      indices.push_back(9);
      indices.push_back(10);
      indices.push_back(11);

      // Degenerate face.
      faceCounts.push_back(2);
      indices.push_back(12);
      indices.push_back(13);

      UsdGeomMesh.Triangulate(indices, faceCounts);

      AssertEqual((int)faceCounts.size(), 6);

      for (int i = 0; i < faceCounts.size(); i++) {
        AssertEqual((int)faceCounts[i], 3);
      }

      AssertEqual((int)indices.size(), 18);

      AssertEqual(indices[0], 0);
      AssertEqual(indices[1], 1);
      AssertEqual(indices[2], 2);

      AssertEqual(indices[3], 0);
      AssertEqual(indices[4], 2);
      AssertEqual(indices[5], 3);

      AssertEqual(indices[6], 0);
      AssertEqual(indices[7], 3);
      AssertEqual(indices[8], 4);

      AssertEqual(indices[9], 5);
      AssertEqual(indices[10], 6);
      AssertEqual(indices[11], 7);

      AssertEqual(indices[12], 5);
      AssertEqual(indices[13], 7);
      AssertEqual(indices[14], 8);

      AssertEqual(indices[15], 9);
      AssertEqual(indices[16], 10);
      AssertEqual(indices[17], 11);
    }
  }
}

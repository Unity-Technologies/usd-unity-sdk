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

%module UsdGeomMesh

%{
#include "pxr/usd/usdGeom/mesh.h"
%}

%include "pxr/usd/usdGeom/mesh.h"

%extend UsdGeomMesh {
  static void Triangulate(VtIntArray& faceVertexIndices, VtIntArray& faceVertexCounts) {
    VtIntArray newIndices;
    VtIntArray newCounts;
    
    newIndices.reserve(faceVertexIndices.size());
    newCounts.reserve(faceVertexCounts.size());

    int last = 0;
    int next = 1;
    for (int i = 0; i < faceVertexCounts.size(); i++) {
      next = last + 1;
      for (int t = 0; t < faceVertexCounts[i] - 2; t++) {
        newCounts.push_back(3);
        newIndices.push_back(faceVertexIndices[last]);
        newIndices.push_back(faceVertexIndices[next++]);
        newIndices.push_back(faceVertexIndices[next]);
      }
      last += faceVertexCounts[i];
    }

    faceVertexIndices.swap(newIndices);
    faceVertexCounts.swap(newCounts);
  }
}
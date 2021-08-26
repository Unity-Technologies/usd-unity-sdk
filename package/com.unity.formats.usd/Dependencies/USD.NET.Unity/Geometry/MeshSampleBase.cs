// Copyright 2021 Unity Technologies. All rights reserved.
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

using UnityEngine;

namespace USD.NET.Unity
{
    [System.Serializable]
    [UsdSchema("Mesh")]
    public class MeshSampleBase : PointBasedSample
    {
        public int[] faceVertexIndices;
        public Vector3[] points;
        public Vector3[] normals;
        [VertexData] public Vector4[] tangents;

        // Regarding UVs: this feels like a very specific solution for "default primvar data", which
        // is fine, but this type of data may be specific to a given pipeline, though here it is
        // declared as universal. In general, primvar data such as UVS, normals, and tangents should
        // be driven by the need of the shader or because it was explicitly requested by the user,
        // and not always included by default.

        /// <summary>
        /// When not explicitly specified by the shader, "st" should be considered the default uv set.
        /// </summary>
        /// <remarks>
        /// UV object types should be Vector{2,3}[], List of Vector{2,3}, or null.
        /// </remarks>
        public Primvar<object> st = new Primvar<object>();
        public Primvar<object> uv = new Primvar<object>();
        public Primvar<object> uv2 = new Primvar<object>();
        public Primvar<object> uv3 = new Primvar<object>();
        public Primvar<object> uv4 = new Primvar<object>();
    }
}

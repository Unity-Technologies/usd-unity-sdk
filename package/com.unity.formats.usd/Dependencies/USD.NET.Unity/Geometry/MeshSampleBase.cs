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

using System.Collections.Generic;
using UnityEngine;

namespace USD.NET.Unity
{
    [System.Serializable]
    [UsdSchema("Mesh")]
    public class MeshSampleBase : PointBasedSample, IArbitraryPrimvars
    {
        public int[] faceVertexIndices;
        public Vector3[] points;
        public Vector3[] normals;
        public Primvar<Vector4[]> tangents = new Primvar<Vector4[]>() {interpolation = PrimvarInterpolation.Varying};

        /// <summary>
        /// Container to hold extra primvars properties
        /// </summary>
        /// <details>
        /// This is typically used to only deserialize the specific uv properties used by a mesh material and can only
        /// be discovered when materials have been processed.
        /// This could be extended to contains user defined primvars.
        /// Note the ForceNoNamespace attribute that instruct the deserialization code to keep the attributes in the "primvars"
        /// namespace.
        /// In unity mesh attributes are the same size as the vertex array so let's set the default interpolation to varying.
        /// When reading from USD the actual interpolation mode will be set.

        /// </details>
        [ForceNoNamespace] public Dictionary<string, Primvar<object>> ArbitraryPrimvars;

        public Dictionary<string, Primvar<object>> GetArbitraryPrimvars() => ArbitraryPrimvars;

        public MeshSampleBase()
        {
            ArbitraryPrimvars = new Dictionary<string, Primvar<object>>();
        }

        public void AddPrimvars(List<string> primvars)
        {
            if (primvars == null)
                return;
            if (ArbitraryPrimvars == null)
                ArbitraryPrimvars = new Dictionary<string, Primvar<object>>();
            foreach (var primvar in primvars)
                ArbitraryPrimvars[primvar] = new Primvar<object> { interpolation = PrimvarInterpolation.Varying };
        }
    }
}

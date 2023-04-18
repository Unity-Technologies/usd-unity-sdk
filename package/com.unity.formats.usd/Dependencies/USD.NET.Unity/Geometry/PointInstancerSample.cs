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

using System;
using UnityEngine;

namespace USD.NET.Unity
{
    [Serializable]
    [UsdSchema("PointInstancer")]
    public class PointInstancerPrototypesSample : BoundableSample
    {
        public Relationship prototypes = new Relationship();
    }

    [Serializable]
    [UsdSchema("PointInstancer")]
    public class PointInstancerSample : PointInstancerPrototypesSample
    {
        // TODO(jcowles): This data type cannot currently be serialized due to this bug:
        // https://github.com/PixarAnimationStudios/USD/issues/639
        //[MetaData]
        //public pxr.SdfInt64ListOp inactiveIds = new pxr.SdfInt64ListOp();

        public int[] protoIndices;
        public long[] ids;
        public long[] invisibleIds;
        public Vector3[] positions;
        public Quaternion[] rotations;
        public Vector3[] scales;
        public Vector3[] velocities;
        public Vector3[] angularVelocities;

        public Matrix4x4[] ComputeInstanceMatrices(Scene scene, string primPath)
        {
            var prim = scene.GetPrimAtPath(primPath);
            var pi = new pxr.UsdGeomPointInstancer(prim);
            var xforms = new pxr.VtMatrix4dArray();
            var timeCode = scene.Time == null ? pxr.UsdTimeCode.Default() : scene.Time;

            // For the base time from Pixar docs:
            // "If your application does not care about off-sample interpolation, it can supply the same value for baseTime that it does for time."
            pi.ComputeInstanceTransformsAtTime(xforms, timeCode, timeCode);

            // Slow, but works.
            var matrices = new Matrix4x4[xforms.size()];
            for (int i = 0; i < xforms.size(); i++)
            {
                matrices[i] = UnityTypeConverter.FromMatrix(xforms[i]);
                matrices[i] = UnityTypeConverter.ChangeBasis(matrices[i]);
            }
            return matrices;
        }
    }
}

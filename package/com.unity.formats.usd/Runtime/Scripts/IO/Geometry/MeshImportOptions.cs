// Copyright 2018 Jeremy Cowles. All rights reserved.
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

namespace Unity.Formats.USD
{
    /// <summary>
    /// Indicates how an attribute should be imported.
    /// </summary>
    public enum ImportMode
    {
        /// <summary>
        /// The value is imported if authored, otherwise null/default.
        /// </summary>
        Import,

        /// <summary>
        /// The value is imported if authored, else the value is computed (if possible).
        /// </summary>
        ImportOrCompute,

        /// <summary>
        /// The value is only computed (if possible), authored values are ignored.
        /// </summary>
        Compute,

        /// <summary>
        /// The authored value is ignored and no value is computed.
        /// </summary>
        Ignore,
    }

    /// <summary>
    /// Indicates how values are imported from the given scene into a UnityEngine.Mesh object.
    /// </summary>
    [System.Serializable]
    public class MeshImportOptions
    {
        /// <summary>
        /// When true, instance cubes onto each joint of the rest pose.
        /// </summary>
        public bool debugShowSkeletonRestPose = false;

        /// <summary>
        /// When true, instance cubes onto each joint of the bind pose.
        /// These cubes will be presented in world space, the inverse of the actual bind pose.
        /// </summary>
        public bool debugShowSkeletonBindPose = false;

        /// <summary>
        /// If true, the vertex positions of the mesh will be imported.
        /// </summary>
        public ImportMode points = ImportMode.Import;

        /// <summary>
        /// If true, the topology (triangles/faces) of the mesh will be imported.
        /// </summary>
        public ImportMode topology = ImportMode.Import;

        /// <summary>
        /// If true, triangulates the mesh. Should only be set to false if the mesh is guaranteed
        /// to be a valid triangle mesh before import.
        /// </summary>
        public bool triangulateMesh = true;

        /// <summary>
        /// If true, a secondary UV set will be generated automatically for lightmapping.
        /// </summary>
        public bool generateLightmapUVs = false;

        #region "Advanced Unwrapping Options"

        /// <summary>
        /// Maximum allowed angle distortion (0..1).
        /// </summary>
        public float unwrapAngleError = .08f;

        /// <summary>
        /// Maximum allowed area distortion (0..1).
        /// </summary>
        public float unwrapAreaError = .15f;

        /// <summary>
        /// This angle (in degrees) or greater between triangles will cause seam to be created.
        /// </summary>
        public float unwrapHardAngle = 88;

        /// <summary>
        /// How much uv-islands will be padded, in pixels.
        /// </summary>
        /// <remarks>
        /// Note that the Unity UnwrapParams API is actually specified in a unitless number. To convert
        /// pixels to this unitless value, this number is divided by 1024.
        /// </remarks>
        public int unwrapPackMargin = 4;

        #endregion

        public ImportMode color = ImportMode.Import;
        public ImportMode normals = ImportMode.ImportOrCompute;
        public ImportMode tangents = ImportMode.ImportOrCompute;
        public ImportMode boundingBox = ImportMode.ImportOrCompute;
    }
}

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

using UnityEngine;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    /// <summary>
    /// A collection of methods used for importing USD Cube data into Unity.
    /// </summary>
    public static class CubeImporter
    {
        /// <summary>
        /// Copy cube data from USD to Unity with the given import options.
        /// </summary>
        /// <param name="skinnedMesh">
        /// Whether the Cube to build is skinned or not. This will allow to determine which Renderer to create
        /// on the GameObject (MeshRenderer or SkinnedMeshRenderer). Default value is false (not skinned).
        /// </param>
        public static void BuildCube(CubeSample usdCube,
            GameObject go,
            SceneImportOptions options,
            bool skinnedMesh = false)
        {
            Material mat = null;

            var cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var unityMesh = cubeGo.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(cubeGo);

            // Because Unity only handle a cube with a default size, the custom size of it is define by the localScale
            // transform. This also need to be taken into account while computing the Unity extent of the mesh (see bellow).
            // This is doable because xformable data are always handled before mesh data, so go.transform already
            // contains any transform of the geometry.
            float size = (float)usdCube.size;
            go.transform.localScale = go.transform.localScale * size;

            bool changeHandedness = options.changeHandedness == BasisTransformation.SlowAndSafe;
            bool hasBounds = usdCube.extent.size.x > 0
                || usdCube.extent.size.y > 0
                || usdCube.extent.size.z > 0;

            if (ShouldImport(options.meshOptions.boundingBox) && hasBounds)
            {
                if (changeHandedness)
                {
                    usdCube.extent.center = UnityTypeConverter.ChangeBasis(usdCube.extent.center);

                    // Divide the extent by the size of the cube. A custom size of the extent is define by
                    // the localScale transform (see above).
                    usdCube.extent.extents = UnityTypeConverter.ChangeBasis(usdCube.extent.extents) / size;
                }

                unityMesh.bounds = usdCube.extent;
            }
            else if (ShouldCompute(options.meshOptions.boundingBox))
            {
                unityMesh.RecalculateBounds();
            }

            if (usdCube.colors != null && ShouldImport(options.meshOptions.color))
            {
                // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
                // For best performance, convert color space to linear off-line and skip conversion.

                if (usdCube.colors.Length == 1)
                {
                    // Constant color can just be set on the material.
                    mat = options.materialMap.InstantiateSolidColor(usdCube.colors.value[0].gamma);
                }
                else if (usdCube.colors.Length == 6)
                {
                    // Uniform colors to verts.
                    // Note that USD cubes have 6 uniform colors and Unity cube mesh has 24 (6*4)
                    // TODO: move the conversion to C++ and use the color management API.
                    Debug.Log(unityMesh.vertexCount);
                    for (int i = 0; i < usdCube.colors.Length; i++)
                    {
                        usdCube.colors.value[i] = usdCube.colors.value[i];
                    }

                    var unityColors = new Color[24];
                    // Front:0, Back:1, Top:2, Bottom:3, Right:4, Left:5
                    unityColors[0] = usdCube.colors.value[0]; // front bottom right
                    unityColors[1] = usdCube.colors.value[0]; // front bottom left
                    unityColors[2] = usdCube.colors.value[0]; // front top right
                    unityColors[3] = usdCube.colors.value[0]; // front top left

                    unityColors[4] = usdCube.colors.value[2]; // top back right
                    unityColors[5] = usdCube.colors.value[2]; // top back left
                    unityColors[6] = usdCube.colors.value[1]; // back bottom right
                    unityColors[7] = usdCube.colors.value[1]; // back bottom left

                    unityColors[8] = usdCube.colors.value[2]; // top front right
                    unityColors[9] = usdCube.colors.value[2]; // top front left
                    unityColors[10] = usdCube.colors.value[1]; // back top right
                    unityColors[11] = usdCube.colors.value[1]; // back top left

                    unityColors[12] = usdCube.colors.value[3]; // Bottom back right
                    unityColors[13] = usdCube.colors.value[3]; // Bottom front right
                    unityColors[14] = usdCube.colors.value[3]; // Bottom front left
                    unityColors[15] = usdCube.colors.value[3]; // Bottom back left

                    unityColors[16] = usdCube.colors.value[5]; // left front bottom
                    unityColors[17] = usdCube.colors.value[5]; // left front top
                    unityColors[18] = usdCube.colors.value[5]; // left back top
                    unityColors[19] = usdCube.colors.value[5]; // left back bottom

                    unityColors[20] = usdCube.colors.value[4]; // right back bottom
                    unityColors[21] = usdCube.colors.value[4]; // right back top
                    unityColors[22] = usdCube.colors.value[4]; // right front top
                    unityColors[23] = usdCube.colors.value[4]; // right front bottom

                    unityMesh.colors = unityColors;
                }
                else if (usdCube.colors.Length == 24)
                {
                    // Face varying colors to verts.
                    // Note that USD cubes have 24 face varying colors and Unity cube mesh has 24 (6*4)
                    // TODO: move the conversion to C++ and use the color management API.
                    Debug.Log(unityMesh.vertexCount);
                    for (int i = 0; i < usdCube.colors.Length; i++)
                    {
                        usdCube.colors.value[i] = usdCube.colors.value[i];
                    }

                    // USD order: front, back, top, bottom, right, left
                    var unityColors = new Color[24];
                    unityColors[0] = usdCube.colors.value[3]; // front bottom right
                    unityColors[1] = usdCube.colors.value[2]; // front bottom left
                    unityColors[2] = usdCube.colors.value[0]; // front top right
                    unityColors[3] = usdCube.colors.value[1]; // front top left

                    unityColors[4] = usdCube.colors.value[8 + 1]; // top back right
                    unityColors[5] = usdCube.colors.value[8 + 2]; // top back left
                    unityColors[6] = usdCube.colors.value[4 + 3]; // back bottom right
                    unityColors[7] = usdCube.colors.value[4 + 0]; // back bottom left

                    unityColors[8] = usdCube.colors.value[8 + 0]; // top front right
                    unityColors[9] = usdCube.colors.value[8 + 3]; // top front left
                    unityColors[10] = usdCube.colors.value[4 + 2]; // back top right
                    unityColors[11] = usdCube.colors.value[4 + 1]; // back top left

                    unityColors[12] = usdCube.colors.value[12 + 1]; // Bottom back right
                    unityColors[13] = usdCube.colors.value[12 + 2]; // Bottom front right
                    unityColors[14] = usdCube.colors.value[12 + 3]; // Bottom front left
                    unityColors[15] = usdCube.colors.value[12 + 0]; // Bottom back left

                    unityColors[16] = usdCube.colors.value[20 + 1]; // left front bottom
                    unityColors[17] = usdCube.colors.value[20 + 2]; // left front top
                    unityColors[18] = usdCube.colors.value[20 + 3]; // left back top
                    unityColors[19] = usdCube.colors.value[20 + 0]; // left back bottom

                    unityColors[20] = usdCube.colors.value[16 + 2]; // right back bottom
                    unityColors[21] = usdCube.colors.value[16 + 3]; // right back top
                    unityColors[22] = usdCube.colors.value[16 + 0]; // right front top
                    unityColors[23] = usdCube.colors.value[16 + 1]; // right front bottom

                    unityMesh.colors = unityColors;
                }
                else if (usdCube.colors.Length == 8)
                {
                    // Vertex colors map on to verts.
                    // Note that USD cubes have 8 verts but Unity cube mesh has 24 (6*4)
                    // TODO: move the conversion to C++ and use the color management API.
                    Debug.Log(unityMesh.vertexCount);
                    for (int i = 0; i < usdCube.colors.Length; i++)
                    {
                        usdCube.colors.value[i] = usdCube.colors.value[i];
                    }

                    // USD order: front (top-right -> ccw)
                    //            back (bottom-left -> ccw (from back perspective))
                    var unityColors = new Color[24];
                    unityColors[0] = usdCube.colors.value[3]; // front bottom right
                    unityColors[1] = usdCube.colors.value[2]; // front bottom left
                    unityColors[2] = usdCube.colors.value[0]; // front top right
                    unityColors[3] = usdCube.colors.value[1]; // front top left

                    unityColors[4] = usdCube.colors.value[6]; // top back right
                    unityColors[5] = usdCube.colors.value[5]; // top back left
                    unityColors[6] = usdCube.colors.value[7]; // back bottom right
                    unityColors[7] = usdCube.colors.value[4]; // back bottom left

                    unityColors[8] = usdCube.colors.value[0]; // top front right
                    unityColors[9] = usdCube.colors.value[1]; // top front left
                    unityColors[10] = usdCube.colors.value[6]; // back top right
                    unityColors[11] = usdCube.colors.value[5]; // back top left

                    unityColors[12] = usdCube.colors.value[7]; // Bottom back right
                    unityColors[13] = usdCube.colors.value[3]; // Bottom front right
                    unityColors[14] = usdCube.colors.value[2]; // Bottom front left
                    unityColors[15] = usdCube.colors.value[4]; // Bottom back left

                    unityColors[16] = usdCube.colors.value[2]; // left front bottom
                    unityColors[17] = usdCube.colors.value[1]; // left front top
                    unityColors[18] = usdCube.colors.value[5]; // left back top
                    unityColors[19] = usdCube.colors.value[4]; // left back bottom

                    unityColors[20] = usdCube.colors.value[7]; // right back bottom
                    unityColors[21] = usdCube.colors.value[6]; // right back top
                    unityColors[22] = usdCube.colors.value[0]; // right front top
                    unityColors[23] = usdCube.colors.value[3]; // right front bottom

                    unityMesh.colors = unityColors;
                    Debug.Log("vertex colors assigned");
                }
                else
                {
                    // FaceVarying and uniform both require breaking up the mesh and are not yet handled in
                    // this example.
                    Debug.LogWarning("Uniform (color per face) and FaceVarying (color per vert per face) "
                        + "display color not supported in this example");
                }
            }

            if (mat == null)
            {
                mat = options.materialMap.InstantiateSolidColor(Color.white);
            }

            // Create Unity mesh.
            Renderer renderer;
            if (skinnedMesh)
            {
                SkinnedMeshRenderer skinnedRenderer = ImporterBase.GetOrAddComponent<SkinnedMeshRenderer>(go);

                if (skinnedRenderer.sharedMesh == null)
                {
                    skinnedRenderer.sharedMesh = Mesh.Instantiate(unityMesh);
                }

                renderer = skinnedRenderer;
            }
            else
            {
                renderer = ImporterBase.GetOrAddComponent<MeshRenderer>(go);
                MeshFilter meshFilter = ImporterBase.GetOrAddComponent<MeshFilter>(go);

                if (meshFilter.sharedMesh == null)
                {
                    meshFilter.sharedMesh = Mesh.Instantiate(unityMesh);
                }
            }

            if (unityMesh.subMeshCount == 1)
            {
                renderer.sharedMaterial = mat;
            }
            else
            {
                var mats = new Material[unityMesh.subMeshCount];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = mat;
                }

                renderer.sharedMaterials = mats;
            }
        }

        /// <summary>
        /// Returns true if the mode is Import or ImportOrCompute.
        /// </summary>
        private static bool ShouldImport(ImportMode mode)
        {
            return mode == ImportMode.Import || mode == ImportMode.ImportOrCompute;
        }

        /// <summary>
        /// Returns true if the mode is Compute or ImportOrCompute.
        /// </summary>
        private static bool ShouldCompute(ImportMode mode)
        {
            return mode == ImportMode.Compute || mode == ImportMode.ImportOrCompute;
        }
    }
}

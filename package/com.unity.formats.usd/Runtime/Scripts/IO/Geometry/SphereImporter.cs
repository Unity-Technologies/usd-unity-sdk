// Copyright 2019 Unity Technologies. All rights reserved.
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
    /// A collection of methods used for importing USD Sphere data into Unity.
    /// </summary>
    public static class SphereImporter
    {
        /// <summary>
        /// Copy sphere data from USD to Unity with the given import options.
        /// </summary>
        /// <param name="skinnedMesh">
        /// Whether the Cube to build is skinned or not. This will allow to determine which Renderer to create
        /// on the GameObject (MeshRenderer or SkinnedMeshRenderer). Default value is false (not skinned).
        /// </param>
        public static void BuildSphere(SphereSample usdSphere,
            GameObject go,
            SceneImportOptions options,
            bool skinnedMesh = false)
        {
            Material mat = null;

            var sphereGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var unityMesh = sphereGo.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(sphereGo);

            // Because Unity only handle a sphere with a default size, the custom size of it is define by the localScale
            // transform. This also need to be taken into account while computing the Unity extent of the mesh (see bellow).
            // This is doable because xformable data are always handled before mesh data, so go.transform already
            // contains any transform of the geometry.
            float size = (float)usdSphere.radius * 2;
            go.transform.localScale = go.transform.localScale * size;

            bool changeHandedness = options.changeHandedness == BasisTransformation.SlowAndSafe;
            bool hasBounds = usdSphere.extent.size.x > 0
                || usdSphere.extent.size.y > 0
                || usdSphere.extent.size.z > 0;

            if (ShouldImport(options.meshOptions.boundingBox) && hasBounds)
            {
                if (changeHandedness)
                {
                    usdSphere.extent.center = UnityTypeConverter.ChangeBasis(usdSphere.extent.center);

                    // Divide the extent by the size of the cube. A custom size of the extent is define by
                    // the localScale transform (see above).
                    usdSphere.extent.extents = UnityTypeConverter.ChangeBasis(usdSphere.extent.extents) / size;
                }

                unityMesh.bounds = usdSphere.extent;
            }
            else if (ShouldCompute(options.meshOptions.boundingBox))
            {
                unityMesh.RecalculateBounds();
            }

            if (usdSphere.colors != null && ShouldImport(options.meshOptions.color))
            {
                // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
                // For best performance, convert color space to linear off-line and skip conversion.

                if (usdSphere.colors.Length == 1)
                {
                    // Constant color can just be set on the material.
                    mat = options.materialMap.InstantiateSolidColor(usdSphere.colors.value[0].gamma);
                }
                else
                {
                    // TODO: Improve logging by adding the path to the sphere prim. This would require that SphereSample
                    //       (and SampleBase class in general) allow to get the UsdPrim  back and it's path in the stage.
                    Debug.LogWarning(
                        "Only constant color are supported for sphere: (can't handle "
                        + usdSphere.colors.Length
                        + " color values)"
                    );
                }
            }

            if (mat == null)
            {
                mat = options.materialMap.InstantiateSolidColor(Color.white);
            }

            // Create Unity mesh.
            // TODO: This code is a duplicate of the CubeImporter code. It requires refactoring.
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

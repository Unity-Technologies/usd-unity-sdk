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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using USD.NET;
using USD.NET.Unity;
using Unity.Jobs;
using pxr;
using Unity.Collections;

namespace Unity.Formats.USD
{
    /// <summary>
    /// A callback function wich integrates the given sample into the given GameObject.
    /// </summary>
    public delegate void MeshImportFunction<T>(string path,
        T sample,
        MeshImporter.GeometrySubsets subsets,
        GameObject go,
        SceneImportOptions option,
        bool isDynamic,
        UsdSkelSkinningQuery query = null) where T : SampleBase, new();

    /// <summary>
    /// This class is responsible for importing mesh samples into Unity. By swapping out the
    /// MeshImportFunctions, the import behavior can be customized.
    /// </summary>
    public class MeshImportStrategy : IImporter
    {
        MeshImportFunction<SanitizedMeshSample> m_meshImporter;
        MeshImportFunction<SanitizedMeshSample> m_skinnedMeshImporter;

        public MeshImportStrategy(MeshImportFunction<SanitizedMeshSample> meshImporter,
                                  MeshImportFunction<SanitizedMeshSample> skinnedMeshImporter)
        {
            m_meshImporter = meshImporter;
            m_skinnedMeshImporter = skinnedMeshImporter;
        }

        ReadAllJob<SanitizedMeshSample> m_readMeshesJob;

        public void BeginReading(Scene scene, PrimMap primMap, SceneImportOptions importOptions)
        {
            m_readMeshesJob = new ReadAllJob<SanitizedMeshSample>(scene, primMap.Meshes, importOptions);
            m_readMeshesJob.Schedule(primMap.Meshes.Length, 2);
        }

        public System.Collections.IEnumerator Import(Scene scene,
            PrimMap primMap,
            SceneImportOptions importOptions)
        {
            System.Reflection.MemberInfo faceVertexCounts = null;
            System.Reflection.MemberInfo faceVertexIndices = null;
            System.Reflection.MemberInfo orientation = null;
            System.Reflection.MemberInfo points = null;
            System.Reflection.MemberInfo normals = null;
            System.Reflection.MemberInfo colors = null;
            System.Reflection.MemberInfo purpose = null;
            System.Reflection.MemberInfo visibility = null;
            var isDynamic = false;

            if (scene.AccessMask != null && scene.IsPopulatingAccessMask)
            {
                var meshType = typeof(SanitizedMeshSample);
                faceVertexCounts = meshType.GetMember("faceVertexCounts")[0];
                faceVertexIndices = meshType.GetMember("faceVertexIndices")[0];
                orientation = meshType.GetMember("orientation")[0];
                points = meshType.GetMember("points")[0];
                normals = meshType.GetMember("normals")[0];
                colors = meshType.GetMember("colors")[0];
                purpose = meshType.GetMember("purpose")[0];
                visibility = meshType.GetMember("visibility")[0];
            }

            foreach (var pathAndSample in m_readMeshesJob)
            {
                if (scene.AccessMask != null && scene.IsPopulatingAccessMask)
                {
                    DeserializationContext deserializationContext;
                    if (scene.AccessMask.Included.TryGetValue(pathAndSample.path, out deserializationContext))
                    {
                        if (deserializationContext.dynamicMembers.Contains(faceVertexCounts)
                            || deserializationContext.dynamicMembers.Contains(orientation)
                            || deserializationContext.dynamicMembers.Contains(faceVertexIndices)
                            || deserializationContext.dynamicMembers.Contains(points)
                            || deserializationContext.dynamicMembers.Contains(normals)
                            || deserializationContext.dynamicMembers.Contains(colors)
                        )
                        {
                            deserializationContext.dynamicMembers.Add(faceVertexCounts);
                            deserializationContext.dynamicMembers.Add(faceVertexIndices);
                            deserializationContext.dynamicMembers.Add(orientation);
                            deserializationContext.dynamicMembers.Add(points);
                            deserializationContext.dynamicMembers.Add(normals);
                            deserializationContext.dynamicMembers.Add(colors);
                        }

                        if (pathAndSample.sample.purpose != Purpose.Default && !deserializationContext.dynamicMembers.Contains(purpose))
                        {
                            deserializationContext.dynamicMembers.Add(purpose);
                        }

                        if (pathAndSample.sample.visibility != Visibility.Inherited && !deserializationContext.dynamicMembers.Contains(visibility))
                        {
                            deserializationContext.dynamicMembers.Add(visibility);
                        }

                        isDynamic = true;
                    }
                }

                Profiler.BeginSample("USD: Build Meshes");
                try
                {
                    GameObject go = primMap[pathAndSample.path];
                    NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path), importOptions);

                    if (importOptions.importTransforms)
                    {
                        Profiler.BeginSample("Build Mesh Xform");
                        XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions, scene);
                        Profiler.EndSample();
                    }

                    Profiler.BeginSample("Read Mesh Subsets");
                    MeshImporter.GeometrySubsets subsets = null;
                    if (primMap == null || !primMap.MeshSubsets.TryGetValue(pathAndSample.path, out subsets))
                    {
                        subsets = MeshImporter.ReadGeomSubsets(scene, pathAndSample.path);
                    }

                    Profiler.EndSample();

                    UsdSkelSkinningQuery skinningQuery;
                    if (importOptions.importHierarchy)
                    {
                        if (importOptions.importSkinning && primMap.SkelCache != null)
                        {
                            // This is pre-cached as part of calling skelCache.Populate and IsValid indicates if we
                            // have the data required to setup a skinned mesh.
                            Profiler.BeginSample("Get Skinning Query");
                            skinningQuery = new UsdSkelSkinningQuery();
                            primMap.SkinningQueries[pathAndSample.path] =
                                primMap.SkelCache.GetSkinningQuery(scene.GetPrimAtPath(pathAndSample.path));
                            Profiler.EndSample();
                        }

                        if (importOptions.importMeshes)
                        {
                            primMap.MeshSubsets[pathAndSample.path] =
                                MeshImporter.ReadGeomSubsets(scene, pathAndSample.path);
                        }
                    }

                    if (importOptions.importSkinning
                        && primMap.SkelCache != null
                        && primMap.SkinningQueries.TryGetValue(pathAndSample.path, out skinningQuery)
                        && skinningQuery.IsValid())
                    {
                        Profiler.BeginSample("USD: Build Skinned Mesh");
                        m_skinnedMeshImporter(pathAndSample.path,
                            pathAndSample.sample,
                            subsets, go, importOptions, isDynamic, skinningQuery);
                        Profiler.EndSample();
                    }
                    else
                    {
                        Profiler.BeginSample("USD: Build Mesh");
                        m_meshImporter(pathAndSample.path,
                            pathAndSample.sample,
                            subsets, go, importOptions, isDynamic);
                        Profiler.EndSample();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(
                        new SceneImporter.ImportException(
                            "Error processing mesh <" + pathAndSample.path + ">", ex));
                    primMap.HasErrors = true;
                }

                Profiler.EndSample();
                yield return null;
            } // foreach mesh
        }
    }

    /// <summary>
    /// A collection of methods used for importing USD Mesh data into Unity.
    /// </summary>
    public static class MeshImporter
    {
        public class GeometrySubsets
        {
            public Dictionary<string, int[]> Subsets { get; set; }

            public GeometrySubsets()
            {
                Subsets = new Dictionary<string, int[]>();
            }
        }

        /// <summary>
        /// Reads geometry subsets if authored. If not authored, returns an empty dictionary.
        /// </summary>
        public static GeometrySubsets ReadGeomSubsets(Scene scene, string path)
        {
            var result = new GeometrySubsets();

            var prim = scene.GetPrimAtPath(path);
            if (prim == null || prim.IsValid() == false)
            {
                return result;
            }

            var im = new pxr.UsdGeomImageable(prim);
            if (im._IsValid() == false)
            {
                return result;
            }

            var subsets =
                UsdGeomSubset.GetGeomSubsets(im, UsdGeomTokens.face,
                    new TfToken("materialBind"));

            // Cache these values to minimize garbage collector churn.
            var value = new VtValue();
            var defaultTime = UsdTimeCode.Default();

            foreach (var subset in subsets)
            {
                if (!subset._IsValid())
                {
                    continue;
                }

                var indices = subset.GetIndicesAttr();
                if (!indices.IsValid())
                {
                    continue;
                }

                if (!indices.Get(value, defaultTime))
                {
                    continue;
                }

                var intValue = new int[0];
                IntrinsicTypeConverter.FromVtArray(value, ref intValue);
                result.Subsets.Add(subset.GetPath(), intValue);
            }

            return result;
        }

        /// <summary>
        /// Copy mesh data from USD to Unity with the given import options, setup for skinning.
        /// </summary>
        public static void BuildSkinnedMesh(string path,
            SanitizedMeshSample usdMesh,
            GeometrySubsets geomSubsets,
            GameObject go,
            SceneImportOptions options,
            bool isDynamic,
            UsdSkelSkinningQuery skinningQuery = null)
        {
            if (usdMesh.points == null)
                return;

            var smr = ImporterBase.GetOrAddComponent<SkinnedMeshRenderer>(go);
            if (smr.sharedMesh == null)
            {
                smr.sharedMesh = new Mesh { name = UniqueMeshName(go.name) };
            }

            // We only check if a mesh is dynamic when scene.IsPopulatingAccessMask is True. It only happens when a playable is
            // created, potentially way after mesh creation.
            if (isDynamic)
            {
                smr.sharedMesh.MarkDynamic();
            }

            BuildMesh_(path, usdMesh, smr.sharedMesh, geomSubsets, go, smr, options);
            if (options.importSkinWeights)
            {
                ImportSkinning(path, usdMesh, smr.sharedMesh, skinningQuery);
            }
        }

        /// <summary>
        /// Import skin weights and joint indices from USD.
        /// </summary>
        /// <remarks>
        /// JointWeights and JointIndices attribute should probably go into the MeshSample but we might not want to pay
        /// the deserialization cost if we don't care about skinning info. Although sanitizing the data would be easier
        /// if done by SanitizedMeshSample.Sanitize
        /// </remarks>
        public static void ImportSkinning(string path, SanitizedMeshSample usdMesh, Mesh unityMesh, UsdSkelSkinningQuery skinningQuery)
        {
            // Get and validate the joint weights and indices informations.
            UsdGeomPrimvar jointWeights = skinningQuery.GetJointWeightsPrimvar();
            UsdGeomPrimvar jointIndices = skinningQuery.GetJointIndicesPrimvar();

            if (!jointWeights.IsDefined() || !jointIndices.IsDefined())
            {
                throw new Exception("Joints information (indices and/or weights) are missing for: " + path);
            }

            // TODO: Both indices and weights attributes can be animated. It's not handled yet.
            // TODO: Having something that convert a UsdGeomPrimvar into a PrimvarSample could help simplify this code.
            int[] indices = IntrinsicTypeConverter.FromVtArray((VtIntArray)jointIndices.GetAttr().Get());
            int indicesElementSize = jointIndices.GetElementSize();
            TfToken indicesInterpolation = jointIndices.GetInterpolation();

            if (indices.Length == 0
                || indicesElementSize == 0
                || indices.Length % indicesElementSize != 0
                || !UsdGeomPrimvar.IsValidInterpolation(indicesInterpolation))
            {
                throw new Exception("Joint indices information are invalid or empty for: " + path);
            }

            float[] weights = IntrinsicTypeConverter.FromVtArray((VtFloatArray)jointWeights.GetAttr().Get());
            int weightsElementSize = jointWeights.GetElementSize();
            TfToken weightsInterpolation = jointWeights.GetInterpolation();

            if (weights.Length == 0
                || weightsElementSize == 0
                || weights.Length % weightsElementSize != 0
                || !UsdGeomPrimvar.IsValidInterpolation(weightsInterpolation))
            {
                throw new Exception("Joints weights information are invalid or empty for: " + path);
            }

            if (weightsElementSize != indicesElementSize)
            {
                throw new Exception("Mismatch in joints weight and indices element sizes for: " + path);
            }

            int vertexCount = unityMesh.vertexCount;
            var bonesPerVertexArray = new byte[vertexCount];
            var boneWeightsArray = new BoneWeight1[vertexCount * weightsElementSize];

            // Remap the vertex indices if mesh attribute have been converted to faceVarying
            var remapIndices = weightsInterpolation.GetString() == UsdGeomTokens.vertex
                && usdMesh.arePrimvarsFaceVarying;

            int isNotConstant = weightsInterpolation.GetString() == UsdGeomTokens.constant ? 0 : 1;

            // Convert the USD joint weights and indices into a Unity BoneWeights array, including remapping to the new triangulated vertices where required
            for (var vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
            {
                // For every new vertex, get the original vertex if it has been triangulated
                var originalVertexIndex = remapIndices ? usdMesh.triangulatedFaceVertexIndices[vertexIndex] * weightsElementSize : vertexIndex * weightsElementSize;
                var vertexLookupIndex = isNotConstant * originalVertexIndex;

                // This intermediary array is faster than getting/setting to a NativeArray in a loop
                bonesPerVertexArray[vertexIndex] = (byte)weightsElementSize;

                // Create a BoneWeight for every element at every vertex
                for (var weightElementIndex = 0; weightElementIndex < weightsElementSize; weightElementIndex++)
                {
                    var boneWeight = new BoneWeight1();
                    boneWeight.boneIndex = indices[vertexLookupIndex + weightElementIndex];
                    boneWeight.weight = weights[vertexLookupIndex + weightElementIndex];
                    boneWeightsArray[vertexIndex * weightsElementSize + weightElementIndex] = boneWeight;
                }
            }
            // TODO: Investigate if bone weights should be normalized before this line.
            var bonesPerVertex = new NativeArray<byte>(bonesPerVertexArray, Allocator.Temp);
            var boneWeights1 = new NativeArray<BoneWeight1>(boneWeightsArray, Allocator.Temp);
            unityMesh.SetBoneWeights(bonesPerVertex, boneWeights1);
            bonesPerVertex.Dispose();
            boneWeights1.Dispose();
        }

        /// <summary>
        /// Copy mesh data from USD to Unity with the given import options.
        /// </summary>
        public static void BuildMesh(string path,
            SanitizedMeshSample usdMesh,
            GeometrySubsets geomSubsets,
            GameObject go,
            SceneImportOptions options,
            bool isDynamic,
            UsdSkelSkinningQuery skinQuery = null)
        {
            if (usdMesh.points == null)
                return;

            var mf = ImporterBase.GetOrAddComponent<MeshFilter>(go);
            var mr = ImporterBase.GetOrAddComponent<MeshRenderer>(go);
            if (mf.sharedMesh == null)
            {
                mf.sharedMesh = new Mesh { name = UniqueMeshName(go.name) };
            }

            // We only check if a mesh is dynamic when scene.IsPopulatingAccessMask is True. It only happens when a playable is
            // created, potentially way after mesh creation.
            if (isDynamic)
            {
                mf.sharedMesh.MarkDynamic();
            }

            BuildMesh_(path, usdMesh, mf.sharedMesh, geomSubsets, go, mr, options);
        }

        static void BuildMesh_(string path,
            SanitizedMeshSample usdMesh,
            Mesh unityMesh,
            GeometrySubsets geomSubsets,
            GameObject go,
            Renderer renderer,
            SceneImportOptions options)
        {
            //
            // Purpose.
            //
            // Deactivate non-geometry prims (e.g. guides, render, etc).
            if (usdMesh.purpose != Purpose.Default)
            {
                go.SetActive(false);
            }

            //
            // Points.
            //
            if (ShouldImport(options.meshOptions.points) && usdMesh.points != null)
            {
                if (usdMesh.faceVertexIndices != null)
                {
                    // Annoyingly, there is a circular dependency between vertices and triangles, which makes
                    // it impossible to have a fixed update order in this function. As a result, we must clear
                    // the triangles before setting the points, to break that dependency.
                    unityMesh.SetTriangles(new int[0] { }, 0);
                }

                unityMesh.vertices = usdMesh.points;
            }

            //
            // Mesh Topology.
            //
            if (ShouldImport(options.meshOptions.topology) && usdMesh.faceVertexIndices != null)
            {
                // Remap subsets
                var newSubsets = new GeometrySubsets();
                foreach (var nameAndSubset in geomSubsets.Subsets)
                {
                    var newFaceIndices = new List<int>();
                    foreach (var faceIndex in nameAndSubset.Value)
                    {
                        int firstFaceIndex = usdMesh.faceMapping[faceIndex];
                        // Number of triangles generated by triangulation is number of verts in original face - 2
                        int triCount = usdMesh.originalFaceVertexCounts[faceIndex] - 2;
                        for (int mappedFaceIndex = 0; mappedFaceIndex < triCount; mappedFaceIndex++)
                        {
                            newFaceIndices.Add(firstFaceIndex + mappedFaceIndex);
                        }
                    }

                    newSubsets.Subsets.Add(nameAndSubset.Key, newFaceIndices.ToArray());
                }
                geomSubsets = newSubsets;

                if (usdMesh.faceVertexIndices.Length > 65535)
                    unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                Profiler.BeginSample("Breakdown triangles for Mesh Subsets");
                if (geomSubsets.Subsets.Count == 0)
                {
                    unityMesh.triangles = usdMesh.faceVertexIndices;
                }
                else
                {
                    var usdIndices = usdMesh.faceVertexIndices;
                    unityMesh.subMeshCount = geomSubsets.Subsets.Count;
                    var subsetIndex = 0;
                    foreach (var kvp in geomSubsets.Subsets)
                    {
                        int[] faceIndices = kvp.Value;
                        var triangleIndices = new int[faceIndices.Length * 3];

                        for (var i = 0; i < faceIndices.Length; i++)
                        {
                            triangleIndices[i * 3 + 0] = usdIndices[faceIndices[i] * 3 + 0];
                            triangleIndices[i * 3 + 1] = usdIndices[faceIndices[i] * 3 + 1];
                            triangleIndices[i * 3 + 2] = usdIndices[faceIndices[i] * 3 + 2];
                        }

                        unityMesh.SetTriangles(triangleIndices, subsetIndex);
                        subsetIndex++;
                    }
                }

                Profiler.EndSample(); // Breakdown triangles for Mesh Subsets
            }

            //
            // Extent / Bounds.
            // TODO: move the bounds calculation to SanitizedMeshSample
            var hasBounds = usdMesh.extent.size.x > 0
                || usdMesh.extent.size.y > 0
                || usdMesh.extent.size.z > 0;

            if (ShouldImport(options.meshOptions.boundingBox) && hasBounds)
            {
                Profiler.BeginSample("Import Bounds");
                unityMesh.bounds = usdMesh.extent;
                Profiler.EndSample();
            }
            else if (!usdMesh.IsRestoredFromCachedData() && ShouldCompute(options.meshOptions.boundingBox))
            {
                Profiler.BeginSample("Calculate Bounds");
                unityMesh.RecalculateBounds();
                Profiler.EndSample();
            }

            //
            // Normals.
            // TODO: move the normals calculation to SanitizedMeshSample
            if (usdMesh.normals != null && ShouldImport(options.meshOptions.normals))
            {
                Profiler.BeginSample("Import Normals");
                unityMesh.normals = usdMesh.normals;
                Profiler.EndSample(); // Import Normals
            }
            // If the mesh has normals that have already been sanitized and restored from cache don't compute again
            else if (!usdMesh.IsRestoredFromCachedData() && ShouldCompute(options.meshOptions.normals))
            {
                Profiler.BeginSample("Calculate Normals");
                unityMesh.RecalculateNormals();
                Profiler.EndSample(); // Calculate Normals
            }

            //
            // Tangents.
            // TODO: move the tangents calculation to SanitizedMeshSample
            if (usdMesh.tangents != null && usdMesh.tangents.value != null && ShouldImport(options.meshOptions.tangents))
            {
                Profiler.BeginSample("Import Tangents");
                // TODO: We should check the interpolation of tangents and treat them accordingly (for now, we assume they
                //       are always face varying).
                unityMesh.tangents = usdMesh.tangents.value;
                Profiler.EndSample(); // Import Tangents
            }
            else if (!usdMesh.IsRestoredFromCachedData() && ShouldCompute(options.meshOptions.tangents))
            {
                Profiler.BeginSample("Calculate Tangents");
                unityMesh.RecalculateTangents();
                Profiler.EndSample(); // Calculate Tangents
            }

            //
            // Display Color.
            //

            var mat = renderer.sharedMaterial;
            if (ShouldImport(options.meshOptions.color) && usdMesh.colors.value != null)
            {
                Profiler.BeginSample("Import Display Color");
                // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
                // For best performance, convert color space to linear off-line and skip conversion.
                if (usdMesh.colors.Length == 1) // Constant
                {
                    // Constant color can just be set on the material.
                    if (options.useDisplayColorAsFallbackMaterial &&
                        options.materialImportMode != MaterialImportMode.None)
                        mat = options.materialMap.InstantiateSolidColor(usdMesh.colors.value[0].gamma);
                }
                else
                {
                    unityMesh.colors = usdMesh.colors.value;
                }

                Profiler.EndSample(); // Import Display Color
            } // should import color

            //
            // UVs / Texture Coordinates.
            //
            Profiler.BeginSample("Request Material Bindings");

            //
            // Materials.
            //

            if (options.materialImportMode != MaterialImportMode.None)
            {
                if (mat == null) mat = options.materialMap.InstantiateSolidColor(Color.white);

                if (unityMesh.subMeshCount == 1)
                {
                    renderer.sharedMaterial = mat;
                    if (options.ShouldBindMaterials)
                        options.materialMap.RequestBinding(
                            path,
                            (scene, boundMat, primvars) => BindMat(
                                unityMesh, boundMat, renderer, path, primvars, usdMesh));
                }
                else
                {
                    var mats = new Material[unityMesh.subMeshCount];
                    for (var i = 0; i < mats.Length; i++) mats[i] = mat;
                    renderer.sharedMaterials = mats;
                    if (options.ShouldBindMaterials)
                    {
                        Debug.Assert(geomSubsets.Subsets.Count == unityMesh.subMeshCount);
                        var subIndex = 0;
                        foreach (var kvp in geomSubsets.Subsets)
                        {
                            var idx = subIndex++;
                            options.materialMap.RequestBinding(
                                kvp.Key,
                                (scene, boundMat, primvars) => BindMat(
                                    unityMesh, boundMat, renderer, idx, path, primvars, usdMesh));
                        }
                    }
                }
            }

            Profiler.EndSample();

            //
            // Lightmap UV Unwrapping.
            //

#if UNITY_EDITOR
            if (options.meshOptions.generateLightmapUVs)
            {
                Profiler.BeginSample("Unwrap Lightmap UVs");
                var unwrapSettings = new UnityEditor.UnwrapParam();

                unwrapSettings.angleError = options.meshOptions.unwrapAngleError;
                unwrapSettings.areaError = options.meshOptions.unwrapAngleError;
                unwrapSettings.hardAngle = options.meshOptions.unwrapHardAngle;

                // Convert pixels to unitless UV space, which is what unwrapSettings uses internally.
                unwrapSettings.packMargin = options.meshOptions.unwrapPackMargin / 1024.0f;

                UnityEditor.Unwrapping.GenerateSecondaryUVSet(unityMesh, unwrapSettings);
                Profiler.EndSample();
            }
#else
            if (options.meshOptions.generateLightmapUVs)
            {
                Debug.LogWarning(
                    "Lightmap UVs were requested to be generated, but cannot be generated outside of the editor");
            }
#endif
        }

        static void LoadPrimvars(
            Mesh unityMesh,
            string usdMeshPath,
            List<string> primvars,
            MeshSample sample)
        {
            if (primvars == null || primvars.Count == 0) return;

            for (var i = 0; i < primvars.Count; i++)
            {
                try
                {
                    ImportUv(unityMesh, i, sample.ArbitraryPrimvars?[primvars[i]]);
                }
                catch (Exception ex)
                {
                    Debug.LogError(new Exception($"Error reading UVs at {usdMeshPath} > uv attribute: {primvars[i]}. ", ex));
                }
            }
        }

        static void BindMat(Mesh unityMesh,
            Material mat,
            Renderer renderer,
            string usdMeshPath,
            List<string> primvars,
            MeshSample sample)
        {
            renderer.sharedMaterial = mat;
            LoadPrimvars(unityMesh, usdMeshPath, primvars, sample);
        }

        // Pass in Unity Mesh from registration.
        // Pass in scene, meshPath from MaterialImporter.
        // Lookup material UV primvars by material path.
        // Read primvars from USD mesh.
        // Assign to UnityMesh sequentially.
        // Material must assign both primvar and Unity Mesh texcoord slots.
        static void BindMat(Mesh unityMesh,
            Material mat,
            Renderer renderer,
            int index,
            string usdMeshPath,
            List<string> primvars,
            MeshSample sample)
        {
            var sharedMats = renderer.sharedMaterials;
            sharedMats[index] = mat;
            renderer.sharedMaterials = sharedMats;
            LoadPrimvars(unityMesh, usdMeshPath, primvars, sample);
        }

        /// <summary>
        /// Attempts to build a valid per-vertex UV set from the given object. If the type T is not
        /// the held type of the object "uv" argument, null is returned. If there is an error such
        /// that the type is correct, but the uv values are somehow incompatible, error messages
        /// will be generated and an empty array will be returned.
        /// </summary>
        /// <returns>
        /// An array of size > 0 on succes, an array of size 0 on failure, or null if the given object
        /// is not of the desired type T.
        /// </returns>
        static T[] TryGetPrimvarValue<T>(Primvar<object> primvar)
        {
            // We can't use uv.GetValueType() as it return "typeof(T)" and so would return "object" in this case instead of
            // the actual type of value.
            if (primvar.value.GetType() != typeof(T[]))
                return null;

            return (T[])primvar.GetValue();
        }

        /// <summary>
        /// Imports UV data from USD into the unityMesh at the given index with the given import rules.
        /// </summary>
        static void ImportUv(
            Mesh unityMesh,
            int uvSetIndex,
            Primvar<object> uv)
        {
            // As in Unity, UVs are a dynamic type which can be vec2, vec3, or vec4.
            if (uv.GetValue() == null)
                return;

            var uv2 = TryGetPrimvarValue<Vector2>(uv);
            if (uv2 != null)
            {
                if (uv2.Length > 0) unityMesh.SetUVs(uvSetIndex, uv2);
                return;
            }

            var uv3 = TryGetPrimvarValue<Vector3>(uv);
            if (uv3 != null)
            {
                if (uv3.Length > 0) unityMesh.SetUVs(uvSetIndex, uv3);
                return;
            }

            var uv4 = TryGetPrimvarValue<Vector4>(uv);
            if (uv4 != null)
            {
                if (uv4.Length > 0) unityMesh.SetUVs(uvSetIndex, uv4);
                return;
            }

            throw new Exception("Unexpected uv type: " + uv.GetType());
        }

        /// <summary>
        /// Returns true if the mode is Import or ImportOrCompute.
        /// </summary>
        public static bool ShouldImport(ImportMode mode)
        {
            return mode == ImportMode.Import || mode == ImportMode.ImportOrCompute;
        }

        /// <summary>
        /// Returns true if the mode is Compute or ImportOrCompute.
        /// </summary>
        public static bool ShouldCompute(ImportMode mode)
        {
            return mode == ImportMode.Compute || mode == ImportMode.ImportOrCompute;
        }

        /// <summary>
        /// Returns a unique mesh name by appending a short guid to the given string
        /// </summary>
        static string UniqueMeshName(string meshName)
        {
            var shortGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return meshName + "_" + shortGuid.Substring(0, shortGuid.Length - 2);
        }
    }
}

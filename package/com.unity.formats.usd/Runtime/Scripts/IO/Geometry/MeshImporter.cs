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
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using USD.NET;
using USD.NET.Unity;
using Unity.Jobs;
using pxr;

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
        bool isDynamic) where T : SampleBase, new();

    /// <summary>
    /// This class is responsible for importing mesh samples into Unity. By swapping out the
    /// MeshImportFunctions, the import behavior can be customized.
    /// </summary>
    public class MeshImportStrategy : IImporter
    {
        private MeshImportFunction<MeshSample> m_meshImporter;
        private MeshImportFunction<MeshSample> m_skinnedMeshImporter;

        public MeshImportStrategy(MeshImportFunction<MeshSample> meshImporter,
            MeshImportFunction<MeshSample> skinnedMeshImporter)
        {
            m_meshImporter = meshImporter;
            m_skinnedMeshImporter = skinnedMeshImporter;
        }

        ReadAllJob<MeshSample> m_readMeshesJob;

        public void BeginReading(Scene scene, PrimMap primMap)
        {
            m_readMeshesJob = new ReadAllJob<MeshSample>(scene, primMap.Meshes);
            m_readMeshesJob.Schedule(primMap.Meshes.Length, 2);
        }

        public System.Collections.IEnumerator Import(Scene scene,
            PrimMap primMap,
            SceneImportOptions importOptions)
        {
            System.Reflection.MemberInfo faceVertexCounts = null;
            System.Reflection.MemberInfo faceVertexIndices = null;
            System.Reflection.MemberInfo orientation = null;
            System.Reflection.MemberInfo purpose = null;
            System.Reflection.MemberInfo visibility = null;
            bool isDynamic = false;

            if (scene.AccessMask != null && scene.IsPopulatingAccessMask)
            {
                var meshType = typeof(MeshSample);
                faceVertexCounts = meshType.GetMember("faceVertexCounts")[0];
                faceVertexIndices = meshType.GetMember("faceVertexIndices")[0];
                orientation = meshType.GetMember("orientation")[0];
                purpose = meshType.GetMember("purpose")[0];
                visibility = meshType.GetMember("visibility")[0];
            }

            foreach (var pathAndSample in m_readMeshesJob)
            {
                if (scene.AccessMask != null && scene.IsPopulatingAccessMask)
                {
                    HashSet<System.Reflection.MemberInfo> members;
                    if (scene.AccessMask.Included.TryGetValue(pathAndSample.path, out members))
                    {
                        if (members.Contains(faceVertexCounts)
                            || members.Contains(orientation)
                            || members.Contains(faceVertexIndices))
                        {
                            members.Add(faceVertexCounts);
                            members.Add(faceVertexIndices);
                            members.Add(orientation);
                        }

                        if (pathAndSample.sample.purpose != Purpose.Default && !members.Contains(purpose))
                        {
                            members.Add(purpose);
                        }

                        if (pathAndSample.sample.visibility != Visibility.Inherited && !members.Contains(visibility))
                        {
                            members.Add(visibility);
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
                            subsets, go, importOptions, isDynamic);
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

            pxr.UsdGeomSubsetVector subsets =
                pxr.UsdGeomSubset.GetGeomSubsets(im, pxr.UsdGeomTokens.face,
                    new pxr.TfToken("materialBind"));

      // Cache these values to minimize garbage collector churn.
      var value = new pxr.VtValue();
      var defaultTime = pxr.UsdTimeCode.Default();

            foreach (var subset in subsets)
            {
                if (!subset._IsValid())
                {
                    continue;
                }

        var indices = subset.GetIndicesAttr();
        if (!indices.IsValid()) { continue; }

        if (!indices.Get(value, defaultTime)) {
          continue;
        }

        int[] intValue = new int[0];
        UnityTypeConverter.FromVtArray(value, ref intValue);
        result.Subsets.Add(subset.GetPath(), intValue);
      }

            return result;
        }

        /// <summary>
        /// Copy mesh data from USD to Unity with the given import options, setup for skinning.
        /// </summary>
        public static void BuildSkinnedMesh(string path,
            MeshSample usdMesh,
            GeometrySubsets geomSubsets,
            GameObject go,
            SceneImportOptions options,
            bool isDynamic)
        {
            var smr = ImporterBase.GetOrAddComponent<SkinnedMeshRenderer>(go);
            if (smr.sharedMesh == null)
            {
                smr.sharedMesh = new Mesh {name = UniqueMeshName(go.name)};
            }

            // We only check if a mesh is dynamic when scene.IsPopulatingAccessMask is True. It only happens when a playable is
            // created, potentially way after mesh creation.
            if (isDynamic)
            {
                smr.sharedMesh.MarkDynamic();
            }

            BuildMesh_(path, usdMesh, smr.sharedMesh, geomSubsets, go, smr, options);
        }

        /// <summary>
        /// Copy mesh data from USD to Unity with the given import options.
        /// </summary>
        public static void BuildMesh(string path,
            MeshSample usdMesh,
            GeometrySubsets geomSubsets,
            GameObject go,
            SceneImportOptions options,
            bool isDynamic)
        {
            var mf = ImporterBase.GetOrAddComponent<MeshFilter>(go);
            var mr = ImporterBase.GetOrAddComponent<MeshRenderer>(go);
            if (mf.sharedMesh == null)
            {
                mf.sharedMesh = new Mesh {name = UniqueMeshName(go.name)};
            }

            // We only check if a mesh is dynamic when scene.IsPopulatingAccessMask is True. It only happens when a playable is
            // created, potentially way after mesh creation.
            if (isDynamic)
            {
                mf.sharedMesh.MarkDynamic();
            }

            BuildMesh_(path, usdMesh, mf.sharedMesh, geomSubsets, go, mr, options);
        }

        private static void BuildMesh_(string path,
            MeshSample usdMesh,
            Mesh unityMesh,
            GeometrySubsets geomSubsets,
            GameObject go,
            Renderer renderer,
            SceneImportOptions options)
        {
            // TODO: Because this method operates on a GameObject, it must be single threaded. For this
            // reason, it should be extremely light weight. All computation should be completed prior to
            // this step, allowing heavy computations to happen in parallel. This is not currently the
            // case, triangulation and change of basis are non-trivial operations. Computing the mesh
            // bounds, normals and tangents should similarly be moved out of this function and should not
            // rely on the UnityEngine.Mesh API.

      bool changeHandedness = options.changeHandedness == BasisTransformation.SlowAndSafe ||
                              options.changeHandedness == BasisTransformation.SlowAndSafeAsFBX;


      //
      // Purpose.
      //

            // Deactivate non-geometry prims (e.g. guides, render, etc).
            if (usdMesh.purpose != Purpose.Default) {
                go.SetActive(false);
            }

            //
            // Points.
            //

      // TODO: indices should not be accessed if topology is not requested, however it may be
      // needed for facevarying primvars; that special case should throw a warning, rather than
      // reading the value.
      int[] newIndices = null;
      bool areVerticesUnrolled = false;

      if (options.meshOptions.points == ImportMode.Import && usdMesh.points != null)
      {
        if (usdMesh.faceVertexIndices != null) {
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

      if (options.meshOptions.topology == ImportMode.Import && usdMesh.faceVertexIndices != null) {
        Profiler.BeginSample("Triangulate Mesh");
        if (options.meshOptions.triangulateMesh)
        {
          // TODO: I think that vertex indices and counts are mandatory attributes for a prim with a Mesh schema,
          //       we could have a generic validation phase for that (generic = uniform checks of mandatory
          //       attributes for any schema).
          if (usdMesh.faceVertexIndices == null) {
            Debug.LogWarning("Mesh had no face indices: " + UnityTypeConverter.GetPath(go.transform));
            return;
          }
          if (usdMesh.faceVertexCounts == null) {
            Debug.LogWarning("Mesh had no face counts: " + UnityTypeConverter.GetPath(go.transform));
            return;
          }

          // Triangulate n-gons.
          // For best performance, triangulate off-line and skip conversion.
          // var indices = UnityTypeConverter.ToVtArray(newIndices ?? usdMesh.faceVertexIndices);
          // var counts = UnityTypeConverter.ToVtArray(usdMesh.faceVertexCounts);
          // UsdGeomMesh.Triangulate(indices, counts);
          // newIndices = newIndices ?? new int[indices.size()];
          // UnityTypeConverter.FromVtArray(indices, ref newIndices);

          // UsdGeomSubsets contain a list of face indices, but once the mesh is triangulated, these
          // indices are no longer correct. The number of new faces generated is a fixed function of
          // the original face indices, but the offset requires an accumulation of all triangulated
          // face offsets, this is "offsetMapping". The index into offsetMapping is the original
          // face index and the value at that index is the new face offset.
          //
          // TODO: this should be moved to C++
          int[] offsetMapping = new int[usdMesh.faceVertexCounts.Length];
          int curOffset = 0;
          for (int i = 0; i < usdMesh.faceVertexCounts.Length; i++) {
            offsetMapping[i] = curOffset;
            curOffset += Math.Max(0, usdMesh.faceVertexCounts[i] - 3);
          }

          var newSubsets = new GeometrySubsets();
          foreach (var nameAndSubset in geomSubsets.Subsets) {
            var newFaceIndices = new List<int>();
            foreach (var faceIndex in nameAndSubset.Value) {
              newFaceIndices.Add(faceIndex + offsetMapping[faceIndex]);
              for (int i = 1; i < usdMesh.faceVertexCounts[faceIndex] - 2; i++) {
                newFaceIndices.Add(faceIndex + offsetMapping[faceIndex] + i);
              }
            }
            newSubsets.Subsets.Add(nameAndSubset.Key, newFaceIndices.ToArray());
          }
          geomSubsets = newSubsets;
        }
        Profiler.EndSample();  // Triangulate Mesh

        // Profiler.BeginSample("Convert LeftHanded");
        // bool isLeftHanded = usdMesh.orientation == Orientation.LeftHanded;
        // if (changeHandedness && !isLeftHanded || !changeHandedness && isLeftHanded) {
        //   // USD is right-handed, so the mesh needs to be flipped.
        //   // Unity is left-handed, but that doesn't matter here.
        //   newIndices = newIndices ?? (int[]) usdMesh.faceVertexIndices.Clone();
        //   for (int i = 0; i < newIndices.Length; i += 3)
        //   {
        //     int tmp = newIndices[i];
        //     newIndices[i] = newIndices[i + 1];
        //     newIndices[i + 1] = tmp;
        //   }
        // }
        // Profiler.EndSample();  // Convert LeftHanded

        if (usdMesh.faceVertexIndices.Length > 65535) {
          unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        Profiler.BeginSample("Breakdown triangles for Mesh Subsets");
        if (geomSubsets.Subsets.Count == 0) {
          unityMesh.triangles = newIndices ?? usdMesh.faceVertexIndices;
        }
        else {
          int[] usdIndices = newIndices ?? usdMesh.faceVertexIndices;
          unityMesh.subMeshCount = geomSubsets.Subsets.Count;
          int subsetIndex = 0;
          foreach (var kvp in geomSubsets.Subsets) {
            int[] faceIndices = kvp.Value;
            int[] triangleIndices = new int[faceIndices.Length * 3];

                        for (int i = 0; i < faceIndices.Length; i++)
                        {
                            triangleIndices[i * 3 + 0] = usdIndices[faceIndices[i] * 3 + 0];
                            triangleIndices[i * 3 + 1] = usdIndices[faceIndices[i] * 3 + 1];
                            triangleIndices[i * 3 + 2] = usdIndices[faceIndices[i] * 3 + 2];
                        }

                        unityMesh.SetTriangles(triangleIndices, subsetIndex);
                        subsetIndex++;
                    }
                }
               Profiler.EndSample();  // Breakdown triangles for Mesh Subsets
            }

            //
            // Extent / Bounds.
            //

            bool hasBounds = usdMesh.extent.size.x > 0
                             || usdMesh.extent.size.y > 0
                             || usdMesh.extent.size.z > 0;

            if (ShouldImport(options.meshOptions.boundingBox) && hasBounds)
            {
                Profiler.BeginSample("Import Bounds");
                if (changeHandedness)
                {
                    usdMesh.extent.center = UnityTypeConverter.ChangeBasis(usdMesh.extent.center);
                }

                unityMesh.bounds = usdMesh.extent;
                Profiler.EndSample();
            }
            else if (ShouldCompute(options.meshOptions.boundingBox))
            {
                Profiler.BeginSample("Calculate Bounds");
                unityMesh.RecalculateBounds();
                Profiler.EndSample();
            }

            //
            // Normals.
            //

      if (usdMesh.normals != null) {
        Profiler.BeginSample("Import Normals");
        Vector3[] newNormals = null;
        unityMesh.normals = usdMesh.normals;
        Profiler.EndSample();  // Import Normals
      }

            //
            // Tangents.
            //

      if (usdMesh.tangents != null && ShouldImport(options.meshOptions.tangents)) {
        Profiler.BeginSample("Import Tangents");
        Vector4[] newTangents = null;
        if (changeHandedness)
        {
          newTangents = new Vector4[usdMesh.tangents.Length];
          for (int i = 0; i < usdMesh.tangents.Length; i++)
          {
            var w = usdMesh.tangents[i].w;
            var t = UnityTypeConverter.ChangeBasis(usdMesh.tangents[i]);
            newTangents[i] = new Vector4(t.x, t.y, t.z, w);
          }
        }

        // TODO: We should check the interpolation of tangents and treat them accordingly (for now, we assume they
        //       are always face varying).
        unityMesh.tangents = newTangents ?? usdMesh.tangents;
        Profiler.EndSample();  // Import Tangents
      } else if (ShouldCompute(options.meshOptions.tangents)) {
        Profiler.BeginSample("Calculate Tangents");
        unityMesh.RecalculateTangents();
        Profiler.EndSample();  // Calculate Tangents
      }

            //
            // Display Color.
            //

      Material mat = renderer.sharedMaterial;
      if (ShouldImport(options.meshOptions.color) && usdMesh.colors != null && usdMesh.colors.Length > 0) {
        Profiler.BeginSample("Import Display Color");
        // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
        // For best performance, convert color space to linear off-line and skip conversion.

        if (usdMesh.colors.Length == 1)  // Constant
        {
          // Constant color can just be set on the material.
          if (options.useDisplayColorAsFallbackMaterial && options.materialImportMode != MaterialImportMode.None) {
            mat = options.materialMap.InstantiateSolidColor(usdMesh.colors[0].gamma);
          }
        }
        else
        {
          // Color[] newColors;
          // if (usdMesh.colors.Length == usdMesh.points.Length && areVerticesUnrolled) // Vertex
          // {
          //   // If there were as much colors as vertices, then colors interpolation is "per vertex" and
          //   // there is nothing to do except if vertices were sanitized to handle some other attributes
          //   // that were face varying.
          //
          //   // Assume colors indices are the same as the original points indices.
          //   // TODO: The right thing to do would be to get the DisplayColor primvars indices if there is some,
          //   //       otherwise fallback to the points indices.
          //   newColors = new Color[unityMesh.vertexCount];
          //   for (int i = 0; i < usdMesh.faceVertexIndices.Length; i++)
          //   {
          //     int colorIndex = usdMesh.faceVertexIndices[i];
          //     newColors[i] = usdMesh.colors[colorIndex];
          //   }
          // }
          // else if (usdMesh.colors.Length == usdMesh.faceVertexCounts.Length) // Uniform
          // {
          //   // Uniform colors, one per face: unroll face colors into vertex varying colors.
          //   newColors = new Color[usdMesh.faceVertexIndices.Length];
          //   int idx = 0;
          //   try
          //   {
          //     for (int faceIndex = 0; faceIndex < usdMesh.faceVertexCounts.Length; faceIndex++)
          //     {
          //       var faceColor = usdMesh.colors[faceIndex];
          //       for (int f = 0; f < usdMesh.faceVertexCounts[faceIndex]; f++)
          //       {
          //         newColors[idx++] = faceColor;
          //       }
          //     }
          //   }
          //   catch (Exception ex)
          //   {
          //     Debug.LogException(new Exception("Failed loading uniform/per-face colors at " + path, ex));
          //   }
          // }
          // else  // Face Varying
          // {
          //   // Because points were sanitized, colors are ready to go as is.
          //   newColors = usdMesh.colors;
          // }

          unityMesh.colors = usdMesh.colors;
        }

                Profiler.EndSample();  // Import Display Color
            } // should import color

            //
            // UVs / Texture Coordinates.
            //

      // TODO: these should also be driven by the UV primvars required by the bound shader.
      Profiler.BeginSample("Import UV Sets");
      ImportUv(path, unityMesh, 0, usdMesh.st, options.meshOptions.texcoord0, areVerticesUnrolled, usdMesh.faceVertexIndices);
      ImportUv(path, unityMesh, 0, usdMesh.uv, options.meshOptions.texcoord0, areVerticesUnrolled, usdMesh.faceVertexIndices);
      ImportUv(path, unityMesh, 1, usdMesh.uv2, options.meshOptions.texcoord1, areVerticesUnrolled, usdMesh.faceVertexIndices);
      ImportUv(path, unityMesh, 2, usdMesh.uv3, options.meshOptions.texcoord2, areVerticesUnrolled, usdMesh.faceVertexIndices);
      ImportUv(path, unityMesh, 3, usdMesh.uv4, options.meshOptions.texcoord3, areVerticesUnrolled, usdMesh.faceVertexIndices);
      Profiler.EndSample();

            Profiler.BeginSample("Request Material Bindings");

            //
            // Materials.
            //

            if (options.materialImportMode != MaterialImportMode.None)
            {
                if (mat == null)
                {
                    mat = options.materialMap.InstantiateSolidColor(Color.white);
                }

        if (unityMesh.subMeshCount == 1) {
          renderer.sharedMaterial = mat;
          if (options.ShouldBindMaterials) {
            options.materialMap.RequestBinding(
              path,
              (scene, boundMat, primvars) => BindMat(
                scene, unityMesh, boundMat, renderer, path, primvars,
                usdMesh.faceVertexCounts, usdMesh.faceVertexIndices));
          }
        } else {
          var mats = new Material[unityMesh.subMeshCount];
          for (int i = 0; i < mats.Length; i++) {
            mats[i] = mat;
          }
          renderer.sharedMaterials = mats;
          if (options.ShouldBindMaterials) {
            Debug.Assert(geomSubsets.Subsets.Count == unityMesh.subMeshCount);
            var subIndex = 0;
            foreach (var kvp in geomSubsets.Subsets) {
              int idx = subIndex++;
              options.materialMap.RequestBinding(
                kvp.Key,
                (scene, boundMat, primvars) => BindMat(
                  scene, unityMesh, boundMat, renderer, idx, path, primvars,
                  usdMesh.faceVertexCounts, usdMesh.faceVertexIndices));
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
            if (options.meshOptions.generateLightmapUVs) {
                Debug.LogWarning("Lightmap UVs were requested to be generated, but cannot be generated outside of the editor");
            }
#endif
        }

    /// <summary>
    /// Unroll primvar to ensure that vertex can be unique not simply regarding their position but also regarding
    /// all their attributes. In other word, a Unity vertex can be shared between two faces if position, normals,
    /// uv, tangents... are the same.
    /// </summary>
    /// <param name="vertices">The primvar to unroll.</param>
    /// <param name="indices">The indices that describe the faces used to unroll the vertices.</param>
    /// <returns>The new list of unrolled vertices.</returns>
    /// <remarks>
    /// Vertices should be unrolled only where attributes has seams. For now  we just unroll "brutaly"
    /// so we produce as much vertices as they are indices.
    /// </remarks>
    public static Vector3[] UnrollPrimvars(Vector3[] vertices, int[] indices)
    {
      int indicesCount = indices.Length;
      Vector3[] newPoints = new Vector3[indicesCount];

      for (int i = 0; i < indicesCount; i++)
      {
        int vertexIndex = indices[i];
        newPoints[i] = vertices[vertexIndex];
      }

      return newPoints;
    }

    private static void ComputeIfUnrollNeeded(MeshSample usdMesh, SceneImportOptions options)
    {
      // If an attribute is face varying, its number of element will be usdMesh.indices.Length.
      int pointCount = usdMesh.points.Length;
      int faceCount = usdMesh.faceVertexCounts.Length;

      // Potential face varying attributes are: normals, tangents, color and uv (st, uv, uv2. uv3. uv4) + all the
      // uv primvars related to materials.
      bool isFaceVarying = (IsFaceVaryingAttribute(options.meshOptions.normals, usdMesh.normals, pointCount, faceCount) ||
                            IsFaceVaryingAttribute(options.meshOptions.tangents, usdMesh.tangents, pointCount, faceCount) ||
                            IsFaceVaryingAttribute(options.meshOptions.color, usdMesh.colors, pointCount, faceCount) ||
                            IsFaceVaryingAttribute(options.meshOptions.texcoord0, usdMesh.st) ||
                            IsFaceVaryingAttribute(options.meshOptions.texcoord0, usdMesh.uv) ||
                            IsFaceVaryingAttribute(options.meshOptions.texcoord1, usdMesh.uv2) ||
                            IsFaceVaryingAttribute(options.meshOptions.texcoord2, usdMesh.uv3) ||
                            IsFaceVaryingAttribute(options.meshOptions.texcoord3, usdMesh.uv4) ||
                            // TODO: We should actually check the uv primvars related to bind material here, cf. LoadPrimvars(...)
                            options.ShouldBindMaterials);

      options.attrUnrollNeeded = isFaceVarying ? FaceVaryingOption.Unroll : FaceVaryingOption.DontUnroll;
    }

    private static bool IsFaceVaryingAttribute<T>(ImportMode mode, T[] attribute, int pointCount, int faceCount)
    {
      // TODO: Attribute interpolation should be retrieve via the USD API, not assumed from the data length.
      return (
        ShouldImport(mode) &&
        attribute != null &&
        (attribute.Length > pointCount || attribute.Length == faceCount));
    }

    private static bool IsFaceVaryingAttribute(ImportMode mode, PrimvarBase attribute)
    {
      return (
        ShouldImport(mode) &&
        ((Primvar<object>) attribute)?.GetValue() != null &&
        (
          attribute.interpolation == PrimvarInterpolation.FaceVarying ||
          attribute.interpolation == PrimvarInterpolation.Uniform));
    }

    static void LoadPrimvars(
      Scene scene,
      Mesh unityMesh,
      string usdMeshPath,
      List<string> primvars,
      int[] faceVertexCounts,
      int[] faceVertexIndices)
    {
      if (primvars == null || primvars.Count == 0) {
        return;
      }

      var prim = scene.GetPrimAtPath(usdMeshPath);
      for (int i = 0; i < primvars.Count; i++) {
        var attr = prim.GetAttribute(new TfToken("primvars:" + primvars[i]));
        if (!attr) continue;

        // Read the raw values.
        VtValue val = attr.Get(0.0);
        if (val.IsEmpty()) { continue; }

        // TODO: We shouldn't assume it is Vector2 Array (can be Vec2, Vec3 or Vec4).
        VtVec2fArray vec2fArray = UsdCs.VtValueToVtVec2fArray(val);
        Vector2[] values = UnityTypeConverter.FromVtArray(vec2fArray);
        //
        // // Unroll indexed primvars.
        // var pv = new UsdGeomPrimvar(attr);
        // VtIntArray vtIndices = new VtIntArray();
        // if (pv.GetIndices(vtIndices, 0.0)) {
        //   int[] indices = UnityTypeConverter.FromVtArray(vtIndices);
        //   values = indices.Select(idx => values[idx]).ToArray();
        // }
        //
        // // Handle primvar interpolation modes.
        // TfToken interp = pv.GetInterpolation();
        // if (interp == UsdGeomTokens.constant) {
        //   Debug.Assert(values.Length == 1);
        //   var newValues = new Vector2[unityMesh.vertexCount];
        //   for (int idx = 0; idx < values.Length; idx++) {
        //     newValues[idx] = values[0];
        //   }
        //   values = newValues;
        // } else if (interp == UsdGeomTokens.uniform) {
        //   Debug.Assert(values.Length == faceVertexCounts.Length);
        //   for (int faceIndex = 0; faceIndex < values.Length; faceIndex++) {
        //     var faceColor = values[faceIndex];
        //     int idx = 0;
        //     var newValues = new Vector2[unityMesh.vertexCount];
        //     for (int f = 0; f < faceVertexCounts[faceIndex]; f++) {
        //       int vertexInFaceIdx = faceVertexIndices[idx++];
        //       newValues[vertexInFaceIdx] = faceColor;
        //     }
        //     values = newValues;
        //   }
        // }
        // else if (interp == UsdGeomTokens.vertex)
        // {
        //   // "Unroll indexed primvars" bellow should have put the uv values in the same order as the original
        //   // position values (vertices), so now we can unroll them the same way to get face varying uv values.
        //   var newValues = new Vector2[unityMesh.vertexCount];
        //   for (int idx = 0; idx < faceVertexIndices.Length; idx++)
        //   {
        //     int valueIndex = faceVertexIndices[idx];
        //     newValues[idx] = values[valueIndex];
        //   }
        //   values = newValues;
        // }

                // Send them to Unity.
                unityMesh.SetUVs(i, values.ToList());
            }
        }

        static void BindMat(Scene scene,
            Mesh unityMesh,
            Material mat,
            Renderer renderer,
            string usdMeshPath,
            List<string> primvars,
            int[] faceVertexCounts,
            int[] faceVertexIndices)
        {
            renderer.sharedMaterial = mat;
            LoadPrimvars(scene, unityMesh, usdMeshPath, primvars, faceVertexCounts, faceVertexIndices);
        }

        // Pass in Unity Mesh from registration.
        // Pass in scene, meshPath from MaterialImporter.
        // Lookup material UV primvars by material path.
        // Read primvars from USD mesh.
        // Assign to UnityMesh sequentially.
        // Material must assign both primvar and Unity Mesh texcoord slots.
        static void BindMat(Scene scene,
            Mesh unityMesh,
            Material mat,
            Renderer renderer,
            int index,
            string usdMeshPath,
            List<string> primvars,
            int[] faceVertexCounts,
            int[] faceVertexIndices)
        {
            var sharedMats = renderer.sharedMaterials;
            sharedMats[index] = mat;
            renderer.sharedMaterials = sharedMats;
            LoadPrimvars(scene, unityMesh, usdMeshPath, primvars, faceVertexCounts, faceVertexIndices);
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
    private static T[] TryGetUVSet<T>(Primvar<object> uv, bool areVerticesUnrolled, int[] faceVertexIndices)
    {
      // We can't use uv.GetValueType() as it return "typeof(T)" and so would return "object" in this case instead of
      // the actual type of value.
      if (uv.value.GetType() != typeof(T[]))
      {
        return null;
      }

      var uvVec = (T[]) uv.GetValue();

      // if (uvVec.Length == 0 || (uv.interpolation == PrimvarInterpolation.Vertex && !areVerticesUnrolled))
      // {
      //   return uvVec;
      // }
      //
      // // Unroll UV values if indices are specified.
      // if (uv.indices != null && uv.indices.Length > 0)
      // {
      //   var newUvs = new T[uv.indices.Length];
      //   for (int i = 0; i < uv.indices.Length; i++)
      //   {
      //     newUvs[i] = uvVec[uv.indices[i]];
      //   }
      //   uvVec = newUvs;
      // }
      //
      // if ((uv.interpolation == PrimvarInterpolation.Vertex || uv.interpolation == PrimvarInterpolation.FaceVarying) &&
      //     areVerticesUnrolled)
      // {
      //   // UV values are already unrolled as either no indices was provided or it was unrolled according to the
      //   // provided indices above, but they need to be reordered to match the way vertices was unrolled.
      //   // TODO: if uv were face varying and indices were provided, this step is useless.
      //   var newUvs = new T[faceVertexIndices.Length];
      //   for (int i = 0; i < faceVertexIndices.Length; i++)
      //   {
      //     newUvs[i] = uvVec[faceVertexIndices[i]];
      //   }
      //   uvVec = newUvs;
      // }

      return uvVec;
    }

    /// <summary>
    /// Imports UV data from USD into the unityMesh at the given index with the given import rules.
    /// </summary>
    private static void ImportUv(string path,
                                 Mesh unityMesh,
                                 int uvSetIndex,
                                 Primvar<object> uv,
                                 ImportMode texcoordImportMode,
                                 bool areVerticesUnrolled,
                                 int[] faceVertexIndices) {
      // As in Unity, UVs are a dynamic type which can be vec2, vec3, or vec4.
      if (!ShouldImport(texcoordImportMode) || uv.GetValue() == null)
      {
        return;
      }

      try {
        var uv2 = TryGetUVSet<Vector2>(uv, areVerticesUnrolled, faceVertexIndices);
        if (uv2 != null) {
          if (uv2.Length > 0) {
            unityMesh.SetUVs(uvSetIndex, uv2);
          }
          return;
        }

        var uv3 = TryGetUVSet<Vector3>(uv, areVerticesUnrolled, faceVertexIndices);
        if (uv3 != null) {
          if (uv3.Length > 0) {
            unityMesh.SetUVs(uvSetIndex, uv3);
          }
          return;
        }

        var uv4 = TryGetUVSet<Vector4>(uv, areVerticesUnrolled, faceVertexIndices);
        if (uv4 != null) {
          if (uv4.Length > 0) {
            unityMesh.SetUVs(uvSetIndex, uv4);
          }
          return;
        }
        throw new Exception("Unexpected uv type: " + uv.GetType());
      } catch (Exception ex) {
        Debug.LogError(new Exception("Error reading UVs at <" + path + "> uv-index: " + uvSetIndex, ex));
      }
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
    private static string UniqueMeshName(string meshName)
    {
        var shortGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return meshName + "_" +  shortGuid.Substring(0, shortGuid.Length-2);
    }
  }
}

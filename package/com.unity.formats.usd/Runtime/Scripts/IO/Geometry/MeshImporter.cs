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

#if UNITY_2018_1_OR_NEWER
using Unity.Jobs;
#endif
using pxr;

namespace Unity.Formats.USD {

  /// <summary>
  /// A callback function wich integrates the given sample into the given GameObject.
  /// </summary>
  public delegate void MeshImportFunction<T>(string path,
                                             T sample,
                                             MeshImporter.GeometrySubsets subsets,
                                             GameObject go,
                                             SceneImportOptions option) where T : SampleBase, new();

  /// <summary>
  /// This class is responsible for importing mesh samples into Unity. By swapping out the
  /// MeshImportFunctions, the import behavior can be customized.
  /// </summary>
  public class MeshImportStrategy : IImporter {
    private MeshImportFunction<MeshSample> m_meshImporter;
    private MeshImportFunction<MeshSample> m_skinnedMeshImporter;

    public MeshImportStrategy(MeshImportFunction<MeshSample> meshImporter,
                            MeshImportFunction<MeshSample> skinnedMeshImporter) {
      m_meshImporter = meshImporter;
      m_skinnedMeshImporter = skinnedMeshImporter;
    }

    ReadAllJob<MeshSample> m_readMeshesJob;
#if UNITY_2018_1_OR_NEWER
    public void BeginReading(Scene scene, PrimMap primMap) {
      m_readMeshesJob = new ReadAllJob<MeshSample>(scene, primMap.Meshes);
      m_readMeshesJob.Schedule(primMap.Meshes.Length, 2);
    }
#else
    public void BeginReading(Scene scene, PrimMap primMap) {
      m_readMeshesJob = new ReadAllJob<MeshSample>(scene, primMap.Meshes);
      m_readMeshesJob.Run();
    }
#endif

    public System.Collections.IEnumerator Import(Scene scene,
                             PrimMap primMap,
                             SceneImportOptions importOptions) {
      if (importOptions.importSkinning) {
        Profiler.BeginSample("USD: Populate SkelCache");
        foreach (var path in primMap.SkelRoots) {
          var prim = scene.GetPrimAtPath(path);
          if (!prim) { continue; }

          var skelRoot = new UsdSkelRoot(prim);
          if (!skelRoot) { continue; }
        }
        Profiler.EndSample();
      }

      System.Reflection.MemberInfo faceVertexCounts = null;
      System.Reflection.MemberInfo faceVertexIndices = null;
      System.Reflection.MemberInfo orientation = null;
      System.Reflection.MemberInfo purpose = null;
      System.Reflection.MemberInfo visibility = null;

      if (scene.AccessMask != null && scene.IsPopulatingAccessMask) {
        var meshType = typeof(MeshSample);
        faceVertexCounts = meshType.GetMember("faceVertexCounts")[0];
        faceVertexIndices = meshType.GetMember("faceVertexIndices")[0];
        orientation = meshType.GetMember("orientation")[0];
        purpose = meshType.GetMember("purpose")[0];
        visibility = meshType.GetMember("visibility")[0];
      }

      foreach (var pathAndSample in m_readMeshesJob) {
        if (scene.AccessMask != null && scene.IsPopulatingAccessMask) {
          HashSet<System.Reflection.MemberInfo> members;
          if (scene.AccessMask.Included.TryGetValue(pathAndSample.path, out members)) {
            if (members.Contains(faceVertexCounts)
              || members.Contains(orientation)
              || members.Contains(faceVertexIndices)) {
              members.Add(faceVertexCounts);
              members.Add(faceVertexIndices);
              members.Add(orientation);
            }

            if (pathAndSample.sample.purpose != Purpose.Default && !members.Contains(purpose)) {
              members.Add(purpose);
            }

            if (pathAndSample.sample.visibility != Visibility.Inherited && !members.Contains(visibility)) {
              members.Add(visibility);
            }
          }
        }

        Profiler.BeginSample("USD: Build Meshes");
        try {
          GameObject go = primMap[pathAndSample.path];
          NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path), importOptions);

          if (importOptions.importTransforms) {
            Profiler.BeginSample("Build Mesh Xform");
            XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions, scene);
            Profiler.EndSample();
          }

          Profiler.BeginSample("Read Mesh Subsets");
          MeshImporter.GeometrySubsets subsets = null;
          if (primMap == null || !primMap.MeshSubsets.TryGetValue(pathAndSample.path, out subsets)) {
            subsets = MeshImporter.ReadGeomSubsets(scene, pathAndSample.path);
          }
          Profiler.EndSample();

          UsdSkelSkinningQuery skinningQuery;
          if (importOptions.importHierarchy) {
            if (importOptions.importSkinning && primMap.SkelCache != null) {
              // This is pre-cached as part of calling skelCache.Populate and IsValid indicates if we
              // have the data required to setup a skinned mesh.
              Profiler.BeginSample("Get Skinning Query");
              skinningQuery = new UsdSkelSkinningQuery();
              primMap.SkinningQueries[pathAndSample.path] = primMap.SkelCache.GetSkinningQuery(scene.GetPrimAtPath(pathAndSample.path));
              Profiler.EndSample();
            }
            if (importOptions.importMeshes) {
              primMap.MeshSubsets[pathAndSample.path] = MeshImporter.ReadGeomSubsets(scene, pathAndSample.path);
            }
          }

          if (importOptions.importSkinning) {
            primMap.SkinningQueries.TryGetValue(pathAndSample.path, out skinningQuery);
            /*
            Profiler.BeginSample("Get Skinning Query");
            skinningQuery = primMap.SkelCache.GetSkinningQuery(scene.GetPrimAtPath(pathAndSample.path));
            Profiler.EndSample();
            */
          }

          if (importOptions.importSkinning
              && primMap.SkelCache != null
              && primMap.SkinningQueries.TryGetValue(pathAndSample.path, out skinningQuery)
              && skinningQuery.IsValid()) {
            Profiler.BeginSample("USD: Build Skinned Mesh");
            m_skinnedMeshImporter(pathAndSample.path,
                                  pathAndSample.sample,
                                  subsets, go, importOptions);
            Profiler.EndSample();
          } else {
            Profiler.BeginSample("USD: Build Mesh");
            m_meshImporter(pathAndSample.path,
                           pathAndSample.sample,
                           subsets, go, importOptions);
            Profiler.EndSample();
          }
        } catch (Exception ex) {
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
  public static class MeshImporter {

    public class GeometrySubsets {
      public Dictionary<string, int[]> Subsets { get; set; }
      public GeometrySubsets() {
        Subsets = new Dictionary<string, int[]>();
      }
    }

    /// <summary>
    /// Reads geometry subsets if authored. If not authored, returns an empty dictionary.
    /// </summary>
    public static GeometrySubsets ReadGeomSubsets(Scene scene, string path) {
      var result = new GeometrySubsets();

      var prim = scene.GetPrimAtPath(path);
      if (prim == null || prim.IsValid() == false) { return result; }

      var im = new pxr.UsdGeomImageable(prim);
      if (im._IsValid() == false) { return result; }

      pxr.UsdGeomSubsetVector subsets =
          pxr.UsdGeomSubset.GetGeomSubsets(im, pxr.UsdGeomTokens.face,
                                           new pxr.TfToken("materialBind"));

      // Cache these values to minimize garbage collector churn.
      var value = new pxr.VtValue();
      int[] intValue = new int[0];
      var defaultTime = pxr.UsdTimeCode.Default();

      foreach (var subset in subsets) {
        if (!subset._IsValid()) { continue; }

        var indices = subset.GetIndicesAttr();
        if (!indices.IsValid()) { continue; }

        if (!indices.Get(value, defaultTime)) {
          continue;
        }

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
                             SceneImportOptions options) {
      var smr = ImporterBase.GetOrAddComponent<SkinnedMeshRenderer>(go);
      if (smr.sharedMesh == null) {
        smr.sharedMesh = new Mesh();
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
                             SceneImportOptions options) {
      var mf = ImporterBase.GetOrAddComponent<MeshFilter>(go);
      var mr = ImporterBase.GetOrAddComponent<MeshRenderer>(go);
      if (mf.sharedMesh == null) {
        mf.sharedMesh = new Mesh();
      }

      BuildMesh_(path, usdMesh, mf.sharedMesh, geomSubsets, go, mr, options);
    }

    private static void BuildMesh_(string path,
                                   MeshSample usdMesh,
                                   Mesh unityMesh,
                                   GeometrySubsets geomSubsets,
                                   GameObject go,
                                   Renderer renderer,
                                   SceneImportOptions options) {

      // TODO: Because this method operates on a GameObject, it must be single threaded. For this
      // reason, it should be extremely light weight. All computation should be completed prior to
      // this step, allowing heavy computations to happen in parallel. This is not currently the
      // case, triangulation and change of basis are non-trivial operations. Computing the mesh
      // bounds, normals and tangents should similarly be moved out of this function and should not
      // rely on the UnityEngine.Mesh API.

      Material mat = renderer.sharedMaterial;
      bool changeHandedness = options.changeHandedness == BasisTransformation.SlowAndSafe;

      //
      // Points.
      //

      if (options.meshOptions.points == ImportMode.Import && usdMesh.points != null) {
        if (changeHandedness) {
          for (int i = 0; i < usdMesh.points.Length; i++) {
            usdMesh.points[i] = UnityTypeConverter.ChangeBasis(usdMesh.points[i]);
          }
        }

        if (usdMesh.faceVertexIndices != null) {
          // Annoyingly, there is a circular dependency between vertices and triangles, which makes
          // it impossible to have a fixed update order in this function. As a result, we must clear
          // the triangles before setting the points, to break that dependency.
          unityMesh.SetTriangles(new int[0] { }, 0);
        }
        unityMesh.vertices = usdMesh.points;
      }

      //
      // Purpose.
      //

      // Deactivate non-geometry prims (e.g. guides, render, etc).
      if (usdMesh.purpose != Purpose.Default) {
        go.SetActive(false);
      }

      //
      // Mesh Topology.
      //

      // TODO: indices should not be accessed if topology is not requested, however it may be
      // needed for facevarying primvars; that special case should throw a warning, rather than
      // reading the value.
      int[] originalIndices = new int[usdMesh.faceVertexIndices == null
                                      ? 0
                                      : usdMesh.faceVertexIndices.Length];
      // Optimization: only do this when there are face varying primvars.
      if (usdMesh.faceVertexIndices != null) {
        Array.Copy(usdMesh.faceVertexIndices, originalIndices, originalIndices.Length);
      }

      if (options.meshOptions.topology == ImportMode.Import && usdMesh.faceVertexIndices != null) {
        Profiler.BeginSample("Triangulate Mesh");
        if (options.meshOptions.triangulateMesh) {
          // Triangulate n-gons.
          // For best performance, triangulate off-line and skip conversion.
          if (usdMesh.faceVertexIndices == null) {
            Debug.LogWarning("Mesh had no face indices: " + UnityTypeConverter.GetPath(go.transform));
            return;
          }
          if (usdMesh.faceVertexCounts == null) {
            Debug.LogWarning("Mesh had no face counts: " + UnityTypeConverter.GetPath(go.transform));
            return;
          }
          var indices = UnityTypeConverter.ToVtArray(usdMesh.faceVertexIndices);
          var counts = UnityTypeConverter.ToVtArray(usdMesh.faceVertexCounts);
          UsdGeomMesh.Triangulate(indices, counts);
          UnityTypeConverter.FromVtArray(indices, ref usdMesh.faceVertexIndices);
        }
        Profiler.EndSample();

        Profiler.BeginSample("Convert LeftHanded");
        bool isLeftHanded = usdMesh.orientation == Orientation.LeftHanded;
        if (changeHandedness && !isLeftHanded || !changeHandedness && isLeftHanded) {
          // USD is right-handed, so the mesh needs to be flipped.
          // Unity is left-handed, but that doesn't matter here.
          for (int i = 0; i < usdMesh.faceVertexIndices.Length; i += 3) {
            int tmp = usdMesh.faceVertexIndices[i];
            usdMesh.faceVertexIndices[i] = usdMesh.faceVertexIndices[i + 1];
            usdMesh.faceVertexIndices[i + 1] = tmp;
          }
        }
        Profiler.EndSample();

        if (usdMesh.faceVertexIndices.Length > 65535) {
          unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        Profiler.BeginSample("Breakdown triangles for Mesh Subsets");
        if (geomSubsets.Subsets.Count == 0) {
          unityMesh.triangles = usdMesh.faceVertexIndices;
        } else {
          unityMesh.subMeshCount = geomSubsets.Subsets.Count;
          int subsetIndex = 0;
          foreach (var kvp in geomSubsets.Subsets) {
            int[] faceIndices = kvp.Value;
            int[] triangleIndices = new int[faceIndices.Length * 3];

            for (int i = 0; i < faceIndices.Length; i++) {
              triangleIndices[i * 3 + 0] = usdMesh.faceVertexIndices[faceIndices[i] * 3 + 0];
              triangleIndices[i * 3 + 1] = usdMesh.faceVertexIndices[faceIndices[i] * 3 + 1];
              triangleIndices[i * 3 + 2] = usdMesh.faceVertexIndices[faceIndices[i] * 3 + 2];
            }

            unityMesh.SetTriangles(triangleIndices, subsetIndex);
            subsetIndex++;
          }
        }
        Profiler.EndSample();
      }

      //
      // Extent / Bounds.
      //

      bool hasBounds = usdMesh.extent.size.x > 0
                    || usdMesh.extent.size.y > 0
                    || usdMesh.extent.size.z > 0;

      if (ShouldImport(options.meshOptions.boundingBox) && hasBounds) {
        Profiler.BeginSample("Import Bounds");
        if (changeHandedness) {
          usdMesh.extent.center = UnityTypeConverter.ChangeBasis(usdMesh.extent.center);
          usdMesh.extent.extents = UnityTypeConverter.ChangeBasis(usdMesh.extent.extents);
        }
        unityMesh.bounds = usdMesh.extent;
        Profiler.EndSample();
      } else if (ShouldCompute(options.meshOptions.boundingBox)) {
        Profiler.BeginSample("Calculate Bounds");
        unityMesh.RecalculateBounds();
        Profiler.EndSample();
      }

      //
      // Normals.
      //

      if (usdMesh.normals != null && ShouldImport(options.meshOptions.normals)) {
        Profiler.BeginSample("Import Normals");
        if (changeHandedness) {
          for (int i = 0; i < usdMesh.points.Length; i++) {
            usdMesh.normals[i] = UnityTypeConverter.ChangeBasis(usdMesh.normals[i]);
          }
        }
        // If more normals than verts, assume face-varying.
        if (usdMesh.normals.Length > usdMesh.points.Length) {
          usdMesh.normals = UnrollFaceVarying(usdMesh.points.Length, usdMesh.normals, usdMesh.faceVertexCounts, originalIndices);
        }
        unityMesh.normals = usdMesh.normals;
        Profiler.EndSample();
      } else if (ShouldCompute(options.meshOptions.normals)) {
        Profiler.BeginSample("Calculate Normals");
        unityMesh.RecalculateNormals();
        Profiler.EndSample();
      }

      //
      // Tangents.
      //

      if (usdMesh.tangents != null && ShouldImport(options.meshOptions.tangents)) {
        Profiler.BeginSample("Import Tangents");
        if (changeHandedness) {
          for (int i = 0; i < usdMesh.points.Length; i++) {
            var w = usdMesh.tangents[i].w;
            var t = UnityTypeConverter.ChangeBasis(usdMesh.tangents[i]);
            usdMesh.tangents[i] = new Vector4(t.x, t.y, t.z, w);
          }
        }
        unityMesh.tangents = usdMesh.tangents;
        Profiler.EndSample();
      } else if (ShouldCompute(options.meshOptions.tangents)) {
        Profiler.BeginSample("Calculate Tangents");
        unityMesh.RecalculateTangents();
        Profiler.EndSample();
      }

      //
      // Display Color.
      //

      if (ShouldImport(options.meshOptions.color) && usdMesh.colors != null && usdMesh.colors.Length > 0) {
        Profiler.BeginSample("Import Display Color");
        // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
        // For best performance, convert color space to linear off-line and skip conversion.

        if (usdMesh.colors.Length == 1) {
          // Constant color can just be set on the material.
          if (options.useDisplayColorAsFallbackMaterial && options.materialImportMode != MaterialImportMode.None) {
            mat = options.materialMap.InstantiateSolidColor(usdMesh.colors[0].gamma);
          }
        } else if (usdMesh.colors.Length == usdMesh.points.Length) {
          // Vertex colors map on to verts.
          // TODO: move the conversion to C++ and use the color management API.
          for (int i = 0; i < usdMesh.colors.Length; i++) {
            usdMesh.colors[i] = usdMesh.colors[i];
          }
          unityMesh.colors = usdMesh.colors;
        } else if (usdMesh.colors.Length == usdMesh.faceVertexCounts.Length) {
          // Uniform colors, one per face.
          // Unroll face colors into vertex colors. This is not strictly correct, but it's much faster
          // than the fully correct solution.
          var colors = new Color[unityMesh.vertexCount];
          int idx = 0;
          try {
            for (int faceIndex = 0; faceIndex < usdMesh.colors.Length; faceIndex++) {
              var faceColor = usdMesh.colors[faceIndex];
              for (int f = 0; f < usdMesh.faceVertexCounts[faceIndex]; f++) {
                int vertexInFaceIdx = originalIndices[idx++];
                colors[vertexInFaceIdx] = faceColor;
              }
            }
            unityMesh.colors = colors;
          } catch (Exception ex) {
            Debug.LogException(new Exception("Failed loading uniform/per-face colors at " + path, ex));
          }
        } else if (usdMesh.colors.Length > usdMesh.points.Length) {
          try {
            usdMesh.colors = UnrollFaceVarying(unityMesh.vertexCount,
                                   usdMesh.colors,
                                   usdMesh.faceVertexCounts,
                                   originalIndices);
            for (int i = 0; i < usdMesh.colors.Length; i++) {
              usdMesh.colors[i] = usdMesh.colors[i];
            }
            unityMesh.colors = usdMesh.colors;
          } catch (Exception ex) {
            Debug.LogException(
                new Exception("Error unrolling Face-Varying colors at <" + path + ">", ex));
          }
        } else {
          Debug.LogWarning("Uniform (color per face) display color not supported");
        }
        Profiler.EndSample();
      } // should import color

      //
      // UVs / Texture Coordinates.
      //

      // TODO: these should also be driven by the UV privmars required by the bound shader.
      Profiler.BeginSample("Import UV Sets");
      ImportUv(path, unityMesh, 0, usdMesh.st, usdMesh.indices, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord0, go);
      ImportUv(path, unityMesh, 0, usdMesh.uv, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord0, go);
      ImportUv(path, unityMesh, 1, usdMesh.uv2, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord1, go);
      ImportUv(path, unityMesh, 2, usdMesh.uv3, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord2, go);
      ImportUv(path, unityMesh, 3, usdMesh.uv4, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord3, go);
      Profiler.EndSample();

      Profiler.BeginSample("Request Material Bindings");

      //
      // Materials.
      //

      if (options.materialImportMode != MaterialImportMode.None) {
        if (mat == null) {
          mat = options.materialMap.InstantiateSolidColor(Color.white);
        }

        if (unityMesh.subMeshCount == 1) {
          renderer.sharedMaterial = mat;
          if (options.ShouldBindMaterials) {
            options.materialMap.RequestBinding(path,
              (scene, boundMat, primvars) => BindMat(scene, unityMesh, boundMat, renderer, path, primvars, usdMesh.faceVertexCounts, originalIndices));
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
              options.materialMap.RequestBinding(kvp.Key,
                  (scene, boundMat, primvars) => BindMat(scene, unityMesh, boundMat, renderer, idx, path, primvars, usdMesh.faceVertexCounts, originalIndices));
            }
          }
        }

      }
      Profiler.EndSample();

      //
      // Lightmap UV Unwrapping.
      //

#if UNITY_EDITOR
      if (options.meshOptions.generateLightmapUVs) {
#if !UNITY_2018_3_OR_NEWER
        if (unityMesh.indexFormat == UnityEngine.Rendering.IndexFormat.UInt32) {
          Debug.LogWarning("Skipping prim " + path + " due to large IndexFormat (UInt32) bug in older vesrsions of Unity");
          return;
        }
#endif
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

    static void LoadPrimvars(Scene scene,
                             Mesh unityMesh,
                             string usdMeshPath,
                             List<string> primvars,
                             int[] faceVertexCounts,
                             int[] faceVertexIndices) {
      if (primvars == null || primvars.Count == 0) { return; }
      var prim = scene.GetPrimAtPath(usdMeshPath);
      for (int i = 0; i < primvars.Count; i++) {
        var attr = prim.GetAttribute(new TfToken("primvars:" + primvars[i]));
        if (!attr) continue;

        // Read the raw values.
        VtValue val = attr.Get(0.0);
        if (val.IsEmpty()) { continue; }
        VtVec2fArray vec2fArray = UsdCs.VtValueToVtVec2fArray(val);
        Vector2[] values = UnityTypeConverter.FromVtArray(vec2fArray);

        // Unroll indexed primvars.
        var pv = new UsdGeomPrimvar(attr);
        VtIntArray vtIndices = new VtIntArray();
        if (pv.GetIndices(vtIndices, 0.0)) {
          int[] indices = UnityTypeConverter.FromVtArray(vtIndices);
          values = indices.Select(idx => values[idx]).ToArray();
        }

        // Handle primvar interpolation modes.
        TfToken interp = pv.GetInterpolation();
        if (interp == UsdGeomTokens.constant) {
          Debug.Assert(values.Length == 1);
          var newValues = new Vector2[unityMesh.vertexCount];
          for (int idx = 0; idx < values.Length; idx++) {
            newValues[idx] = values[0];
          }
          values = newValues;
        } else if (interp == UsdGeomTokens.uniform) {
          Debug.Assert(values.Length == faceVertexCounts.Length);
          for (int faceIndex = 0; faceIndex < values.Length; faceIndex++) {
            var faceColor = values[faceIndex];
            int idx = 0;
            var newValues = new Vector2[unityMesh.vertexCount];
            for (int f = 0; f < faceVertexCounts[faceIndex]; f++) {
              int vertexInFaceIdx = faceVertexIndices[idx++];
              newValues[vertexInFaceIdx] = faceColor;
            }
            values = newValues;
          }
        } else if (interp == UsdGeomTokens.faceVarying) {
          values = UnrollFaceVarying(unityMesh.vertexCount, values, faceVertexCounts, faceVertexIndices);
        }

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
                        int[] faceVertexIndices) {
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
                        int[] faceVertexIndices) {
      var sharedMats = renderer.sharedMaterials;
      sharedMats[index] = mat;
      renderer.sharedMaterials = sharedMats;
      LoadPrimvars(scene, unityMesh, usdMeshPath, primvars, faceVertexCounts, faceVertexIndices);
    }

    /// <summary>
    /// Fast approximate unrolling of face varying UVs into vertices.
    /// </summary>
    /// <remarks>
    /// Strictly speaking, this is not correct because bordering faces with different UV values
    /// (i.e. seams) will share UV values. This artifact will appear as seams on the object that
    /// would otherwise be seamless. The correct solution is to compare UV values at every vertex
    /// and un-weld vertices which do not share a common value.
    /// 
    /// Still, this approximation is useful since often values are only incorrect at the seam and
    /// for fast iteration loops, such a seam may be preferred over a long load time.
    /// </remarks>
    static T[] UnrollFaceVarying<T>(int vertCount,
                                    T[] uvs,
                                    int[] faceVertexCounts,
                                    int[] faceVertexIndices) {
      var newUvs = new T[vertCount];
      int faceVaryingIndex = 0;
      int vertexVaryingIndex = 0;

      // Since face-varying UVs have one value per vertex, per face, this is the same number of
      // values as the mesh indices.
      if (faceVertexIndices.Length != uvs.Length) {
        throw new Exception("Expected " + faceVertexIndices.Length + " UVs but found " + uvs.Length);
      }

      foreach (var count in faceVertexCounts) {
        for (int i = 0; i < count; i++) {
          // Find the associated mesh vertex for each vertex of the face.
          vertexVaryingIndex = faceVertexIndices[faceVaryingIndex];
          //Debug.Assert(vertexVaryingIndex < vertCount);
          // Set the UV value into the same vertex as the position.
          newUvs[vertexVaryingIndex] = uvs[faceVaryingIndex];
          faceVaryingIndex++;
        }
      }
      return newUvs;
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
    private static T[] TryGetUVSet<T>(object uv,
                                      int[] uvIndices,
                                      int[] faceVertexCounts,
                                      int[] faceVertexIndices,
                                      int vertexCount,
                                      GameObject go) {
      if (uv.GetType() != typeof(T[])) {
        return null;
      }

      var uvVec = (T[])uv;

      if (uvVec.Length == 0) {
        return uvVec;
      }

      // Unroll UV indices if specified.
      if (uvIndices != null && uvIndices.Length > 0) {
        var newUvs = new T[uvIndices.Length];
        for (int i = 0; i < uvIndices.Length; i++) {
          newUvs[i] = uvVec[uvIndices[i]];
        }
        uvVec = newUvs;
      }

      // If there are more UVs than verts, the UVs must be face varying, e.g. each vertex for
      // each face has a unique UV value. These values must be collapsed such that verts shared
      // between faces also share a single UV value.
      if (uvVec.Length > vertexCount) {
        uvVec = UnrollFaceVarying(vertexCount, uvVec, faceVertexCounts, faceVertexIndices);
        if (uvVec == null) {
          return new T[0];
        }
        Debug.Assert(uvVec.Length == vertexCount);
      }

      // If there are fewer values, these must be "varying" / one value per face.
      // This is not yet supported.
      if (uvVec.Length < vertexCount) {
        Debug.LogWarning("Mesh UVs are constant or uniform, ignored "
          + UnityTypeConverter.GetPath(go.transform));
        return new T[0];
      }

      return uvVec;
    }

    /// <summary>
    /// Imports UV data from USD into the unityMesh at the given index with the given import rules.
    /// </summary>
    private static void ImportUv(string path,
                                 Mesh unityMesh,
                                 int uvSetIndex,
                                 object uv,
                                 int[] uvIndices,
                                 int[] faceVertexCounts,
                                 int[] faceVertexIndices,
                                 ImportMode texcoordImportMode,
                                 GameObject go) {
      // As in Unity, UVs are a dynamic type which can be vec2, vec3, or vec4.
      if (uv == null || !ShouldImport(texcoordImportMode)) {
        return;
      }

      try {
        int vertCount = unityMesh.vertexCount;

        var uv2 = TryGetUVSet<Vector2>(uv, uvIndices, faceVertexCounts, faceVertexIndices, vertCount, go);
        if (uv2 != null) {
          if (uv2.Length > 0) {
            unityMesh.SetUVs(uvSetIndex, uv2.ToList());
          }
          return;
        }

        var uv3 = TryGetUVSet<Vector3>(uv, uvIndices, faceVertexCounts, faceVertexIndices, vertCount, go);
        if (uv3 != null) {
          if (uv3.Length > 0) {
            unityMesh.SetUVs(uvSetIndex, uv3.ToList());
          }
          return;
        }

        var uv4 = TryGetUVSet<Vector4>(uv, uvIndices, faceVertexCounts, faceVertexIndices, vertCount, go);
        if (uv4 != null) {
          if (uv4.Length > 0) {
            unityMesh.SetUVs(uvSetIndex, uv4.ToList());
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
    private static bool ShouldImport(ImportMode mode) {
      return mode == ImportMode.Import || mode == ImportMode.ImportOrCompute;
    }

    /// <summary>
    /// Returns true if the mode is Compute or ImportOrCompute.
    /// </summary>
    private static bool ShouldCompute(ImportMode mode) {
      return mode == ImportMode.Compute || mode == ImportMode.ImportOrCompute;
    }
  }
}

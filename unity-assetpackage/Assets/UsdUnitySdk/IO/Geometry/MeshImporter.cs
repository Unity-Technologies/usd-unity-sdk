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
using pxr;

namespace USD.NET.Unity {

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

      if (options.meshOptions.points == ImportMode.Import) {
        if (changeHandedness) {
          for (int i = 0; i < usdMesh.points.Length; i++) {
            usdMesh.points[i] = UnityTypeConverter.ChangeBasis(usdMesh.points[i]);
          }
        }

        unityMesh.vertices = usdMesh.points;
      }

      int[] originalIndices = new int[usdMesh.faceVertexIndices == null ? 0 : usdMesh.faceVertexIndices.Length];
      
      if (options.meshOptions.topology == ImportMode.Import) {
        // Optimization: only do this when there are face varying primvars.
        Array.Copy(usdMesh.faceVertexIndices, originalIndices, originalIndices.Length);

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

      if (ShouldImport(options.meshOptions.color) && usdMesh.colors != null && usdMesh.colors.Length > 0) {
        Profiler.BeginSample("Import Display Color");
        // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
        // For best performance, convert color space to linear off-line and skip conversion.

        if (usdMesh.colors.Length == 1) {
          // Constant color can just be set on the material.
          if (options.useDisplayColorAsFallbackMaterial) {
            mat = options.materialMap.InstantiateSolidColor(usdMesh.colors[0].gamma);
          }
        } else if (usdMesh.colors.Length == usdMesh.points.Length) {
          // Vertex colors map on to verts.
          // TODO: move the conversion to C++ and use the color management API.
          for (int i = 0; i < usdMesh.colors.Length; i++) {
            usdMesh.colors[i] = usdMesh.colors[i].gamma;
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
              var faceColor = usdMesh.colors[faceIndex].gamma;
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
              usdMesh.colors[i] = usdMesh.colors[i].gamma;
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

      Profiler.BeginSample("Import UV Sets");
      ImportUv(path, unityMesh, 0, usdMesh.st, usdMesh.indices, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord0, go);
      ImportUv(path, unityMesh, 0, usdMesh.uv, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord0, go);
      ImportUv(path, unityMesh, 1, usdMesh.uv2, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord1, go);
      ImportUv(path, unityMesh, 2, usdMesh.uv3, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord2, go);
      ImportUv(path, unityMesh, 3, usdMesh.uv4, null, usdMesh.faceVertexCounts, originalIndices, options.meshOptions.texcoord3, go);
      Profiler.EndSample();

      Profiler.BeginSample("Request Material Bindings");
      if (mat == null) {
        mat = options.materialMap.InstantiateSolidColor(Color.white);
      }

      if (unityMesh.subMeshCount == 1) {
        renderer.sharedMaterial = mat;
        if (options.ShouldBindMaterials) {
          options.materialMap.RequestBinding(path, boundMat => renderer.sharedMaterial = boundMat);
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
            int idx = subIndex;
            options.materialMap.RequestBinding(kvp.Key, boundMat => BindMat(boundMat, renderer, idx, path));
          }
          subIndex++;
        }
      }
      Profiler.EndSample();

#if UNITY_EDITOR
      if (options.meshOptions.generateLightmapUVs) {
        Profiler.BeginSample("Unwrap Lightmap UVs");
        ShowCrashWarning();
        var unwrapSettings = new UnityEditor.UnwrapParam();
        unwrapSettings.angleError = options.meshOptions.unwrapAngleError;
        unwrapSettings.areaError = options.meshOptions.unwrapAngleError;
        unwrapSettings.hardAngle = options.meshOptions.unwrapHardAngle;
        unwrapSettings.packMargin = options.meshOptions.unwrapPackMargin;

        var vertCount = unityMesh.vertexCount;
        UnityEditor.Unwrapping.GenerateSecondaryUVSet(unityMesh, unwrapSettings);

        // Work around a bug where the unity Mesh can be corrupted by the unwrapping logic.
        if (vertCount != unityMesh.vertexCount) {
          Debug.LogError("Light map unwrapping failed, disabling game object for prim: " + path);
          go.SetActive(false);
        }
        Profiler.EndSample();
      }
#else
      if (options.meshOptions.generateLightmapUVs) {
        Debug.LogWarning("Lightmap UVs were requested to be generated, but cannot be generated outside of the editor");
      }
#endif
    }

    static void ShowCrashWarning() {
#if UNITY_2018_2
      Debug.LogWarning(
          "Unity 2018.2.x may generate light maps which result in a crash during light baking. " +
          "If you experience a crash, it is recommended to upgrade. For details see: " +
          "https://issuetracker.unity3d.com/issues/crash-in-calculatesurfacearea-after-opening-the-project-with-certain-mesh");
#endif
    }

    static void BindMat(Material mat, Renderer renderer, int index, string path) {
      Debug.Log(path + " -> " + index + " -> C: " + mat.color.ToString());
      var sharedMats = renderer.sharedMaterials;
      sharedMats[index] = mat;
      renderer.sharedMaterials = sharedMats;
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

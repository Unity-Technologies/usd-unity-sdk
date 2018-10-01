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
using System.Linq;
using UnityEngine;
using pxr;

namespace USD.NET.Unity {

  /// <summary>
  /// A collection of methods used for importing USD Mesh data into Unity.
  /// </summary>
  public static class MeshImporter {

    /// <summary>
    /// Copy mesh data from USD to Unity with the given import options.
    /// </summary>
    public static void BuildMesh(MeshSample usdMesh,
                                 GameObject go,
                                 SceneImportOptions options) {

      // TODO: Because this method operates on a GameOBject, it must be single threaded. For this
      // reason, it should be extremely light weight. All computation should be completed prior to
      // this step, allowing heavy computations to happen in parallel. This is not currently the
      // case, triangulation and change of basis are non-trivial operations. Computing the mesh
      // bounds, normals and tangents should similarly be moved out of this function and should not
      // rely on the UnityEngine.Mesh API.

      var mf = go.AddComponent<MeshFilter>();
      var mr = go.AddComponent<MeshRenderer>();
      var unityMesh = new Mesh();
      Material mat = null;
      bool changeHandedness = options.changeHandedness == BasisTransformation.SlowAndSafe;

      if (changeHandedness) {
        for (int i = 0; i < usdMesh.points.Length; i++) {
          usdMesh.points[i] = UnityTypeConverter.ChangeBasis(usdMesh.points[i]);
        }
      }

      unityMesh.vertices = usdMesh.points;
      
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

      if (usdMesh.faceVertexIndices.Length > 65535) {
        unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
      }

      unityMesh.SetTriangles(usdMesh.faceVertexIndices, 0);

      bool hasBounds = usdMesh.extent.size.x > 0
                    || usdMesh.extent.size.y > 0
                    || usdMesh.extent.size.z > 0;

      if (ShouldImport(options.meshOptions.boundingBox) && hasBounds) {
        if (changeHandedness) {
          usdMesh.extent.center = UnityTypeConverter.ChangeBasis(usdMesh.extent.center);
          usdMesh.extent.extents = UnityTypeConverter.ChangeBasis(usdMesh.extent.extents);
        }
        unityMesh.bounds = usdMesh.extent;
      } else if (ShouldCompute(options.meshOptions.boundingBox)) {
        unityMesh.RecalculateBounds();
      }

      if (usdMesh.normals != null && ShouldImport(options.meshOptions.normals)) {
        if (changeHandedness) {
          for (int i = 0; i < usdMesh.points.Length; i++) {
            usdMesh.normals[i] = UnityTypeConverter.ChangeBasis(usdMesh.normals[i]);
          }
        }
        unityMesh.normals = usdMesh.normals;
      } else if (ShouldCompute(options.meshOptions.normals)) {
        unityMesh.RecalculateNormals();
      }

      if (usdMesh.tangents != null && ShouldImport(options.meshOptions.tangents)) {
        if (changeHandedness) {
          for (int i = 0; i < usdMesh.points.Length; i++) {
            var w = usdMesh.tangents[i].w;
            var t = UnityTypeConverter.ChangeBasis(usdMesh.tangents[i]);
            usdMesh.tangents[i] = new Vector4(t.x, t.y, t.z, w);
          }
        }
        unityMesh.tangents = usdMesh.tangents;
      } else if (ShouldCompute(options.meshOptions.tangents)) {
        unityMesh.RecalculateTangents();
      }

      if (usdMesh.colors != null && ShouldImport(options.meshOptions.color)) {
        // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
        // For best performance, convert color space to linear off-line and skip conversion.

        if (usdMesh.colors.Length == 1) {
          // Constant color can just be set on the material.
          mat = options.materialMap.InstantiateSolidColor(usdMesh.colors[0].gamma);
        } else if (usdMesh.colors.Length == usdMesh.points.Length) {
          // Vertex colors map on to verts.
          // TODO: move the conversion to C++ and use the color management API.
          for (int i = 0; i < usdMesh.colors.Length; i++) {
            usdMesh.colors[i] = usdMesh.colors[i].gamma;
          }

          unityMesh.colors = usdMesh.colors;
        } else {
          // FaceVarying and uniform both require breaking up the mesh and are not yet handled in
          // this example.
          Debug.LogWarning("Uniform (color per face) and FaceVarying (color per vert per face) "
                         + "display color not supported in this example");
        }
      }

      ImportUv(unityMesh, 0, usdMesh.uv, options.meshOptions.texcoord0, go);
      ImportUv(unityMesh, 1, usdMesh.uv2, options.meshOptions.texcoord1, go);
      ImportUv(unityMesh, 2, usdMesh.uv3, options.meshOptions.texcoord2, go);
      ImportUv(unityMesh, 3, usdMesh.uv4, options.meshOptions.texcoord3, go);

      if (mat == null) {
        mat = options.materialMap.InstantiateSolidColor(Color.white);
      }

      if (unityMesh.subMeshCount == 1) {
        mr.sharedMaterial = mat;
      } else {
        var mats = new Material[unityMesh.subMeshCount];
        for (int i = 0; i < mats.Length; i++) {
          mats[i] = mat;
        }
        mr.sharedMaterials = mats;
      }
      mf.sharedMesh = unityMesh;
    }

    /// <summary>
    /// Imports UV data from USD into the unityMesh at the given index with the given import rules.
    /// </summary>
    private static void ImportUv(Mesh unityMesh,
                                 int uvSetIndex,
                                 object uv,
                                 ImportMode texcoordImportMode,
                                 GameObject go) {
      // As in Unity, UVs are a dynamic type which can be vec2, vec3, or vec4.
      if (uv == null || !ShouldImport(texcoordImportMode)) {
        return;
      }

      Type uvType = uv.GetType();
      if (uvType == typeof(Vector2[])) {
        var uvVec = (Vector2[])uv;
        if (uvVec.Length > unityMesh.vertexCount) {
          Debug.LogWarning("Mesh UVs are face varying, but are being imported as vertex varying" +
            " " + UnityTypeConverter.GetPath(go.transform));
          var tmp = new Vector2[unityMesh.vertexCount];
          Array.Copy(uvVec, tmp, tmp.Length);
          uvVec = tmp;
        }
        if (uvVec.Length < unityMesh.vertexCount) {
          Debug.LogWarning("Mesh UVs are constant or uniform, ignored "
            + UnityTypeConverter.GetPath(go.transform));
          return;
        }
        unityMesh.SetUVs(0, uvVec.ToList());
      } else if (uvType == typeof(Vector3[])) {
        unityMesh.SetUVs(0, ((Vector3[])uv).ToList());
      } else if (uvType == typeof(Vector4[])) {
        unityMesh.SetUVs(0, ((Vector4[])uv).ToList());
      } else {
        throw new Exception("Unexpected uv type: " + uv.GetType());
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

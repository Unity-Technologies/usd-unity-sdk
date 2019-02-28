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

namespace Unity.Formats.USD {

  /// <summary>
  /// A collection of methods used for importing USD Cube data into Unity.
  /// </summary>
  public static class CubeImporter {

    /// <summary>
    /// Copy cube data from USD to Unity with the given import options.
    /// </summary>
    public static void BuildCube(CubeSample usdCube,
                                 GameObject go,
                                 SceneImportOptions options) {


      var mf = ImporterBase.GetOrAddComponent<MeshFilter>(go);
      var mr = ImporterBase.GetOrAddComponent<MeshRenderer>(go);
      Material mat = null;

      var cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
      var unityMesh = cubeGo.GetComponent<MeshFilter>().sharedMesh;
      GameObject.DestroyImmediate(cubeGo);

      bool changeHandedness = options.changeHandedness == BasisTransformation.SlowAndSafe;
      bool hasBounds = usdCube.extent.size.x > 0
                    || usdCube.extent.size.y > 0
                    || usdCube.extent.size.z > 0;

      if (ShouldImport(options.meshOptions.boundingBox) && hasBounds) {
        if (changeHandedness) {
          usdCube.extent.center = UnityTypeConverter.ChangeBasis(usdCube.extent.center);
          usdCube.extent.extents = UnityTypeConverter.ChangeBasis(usdCube.extent.extents);
        }
        unityMesh.bounds = usdCube.extent;
      } else if (ShouldCompute(options.meshOptions.boundingBox)) {
        unityMesh.RecalculateBounds();
      }

      if (usdCube.colors != null && ShouldImport(options.meshOptions.color)) {
        // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
        // For best performance, convert color space to linear off-line and skip conversion.

        if (usdCube.colors.Length == 1) {
          // Constant color can just be set on the material.
          mat = options.materialMap.InstantiateSolidColor(usdCube.colors[0].gamma);
          Debug.Log("constant colors assigned");
        } else if (usdCube.colors.Length == 6) {
          // Uniform colors to verts.
          // Note that USD cubes have 6 uniform colors and Unity cube mesh has 24 (6*4)
          // TODO: move the conversion to C++ and use the color management API.
          Debug.Log(unityMesh.vertexCount);
          for (int i = 0; i < usdCube.colors.Length; i++) {
            usdCube.colors[i] = usdCube.colors[i];
          }

          var unityColors = new Color[24];
          // Front:0, Back:1, Top:2, Bottom:3, Right:4, Left:5
          unityColors[0] = usdCube.colors[0]; // front bottom right
          unityColors[1] = usdCube.colors[0]; // front bottom left
          unityColors[2] = usdCube.colors[0]; // front top right
          unityColors[3] = usdCube.colors[0]; // front top left

          unityColors[4] = usdCube.colors[2]; // top back right
          unityColors[5] = usdCube.colors[2]; // top back left
          unityColors[6] = usdCube.colors[1]; // back bottom right
          unityColors[7] = usdCube.colors[1]; // back bottom left

          unityColors[8] = usdCube.colors[2]; // top front right
          unityColors[9] = usdCube.colors[2]; // top front left
          unityColors[10] = usdCube.colors[1]; // back top right
          unityColors[11] = usdCube.colors[1]; // back top left

          unityColors[12] = usdCube.colors[3]; // Bottom back right
          unityColors[13] = usdCube.colors[3]; // Bottom front right
          unityColors[14] = usdCube.colors[3]; // Bottom front left
          unityColors[15] = usdCube.colors[3]; // Bottom back left

          unityColors[16] = usdCube.colors[5]; // left front bottom
          unityColors[17] = usdCube.colors[5]; // left front top
          unityColors[18] = usdCube.colors[5]; // left back top
          unityColors[19] = usdCube.colors[5]; // left back bottom

          unityColors[20] = usdCube.colors[4]; // right back bottom
          unityColors[21] = usdCube.colors[4]; // right back top
          unityColors[22] = usdCube.colors[4]; // right front top
          unityColors[23] = usdCube.colors[4]; // right front bottom 

          unityMesh.colors = unityColors;
        } else if (usdCube.colors.Length == 24) {
          // Face varying colors to verts.
          // Note that USD cubes have 24 face varying colors and Unity cube mesh has 24 (6*4)
          // TODO: move the conversion to C++ and use the color management API.
          Debug.Log(unityMesh.vertexCount);
          for (int i = 0; i < usdCube.colors.Length; i++) {
            usdCube.colors[i] = usdCube.colors[i];
          }

          // USD order: front, back, top, bottom, right, left
          var unityColors = new Color[24];
          unityColors[0] = usdCube.colors[3]; // front bottom right
          unityColors[1] = usdCube.colors[2]; // front bottom left
          unityColors[2] = usdCube.colors[0]; // front top right
          unityColors[3] = usdCube.colors[1]; // front top left

          unityColors[4] = usdCube.colors[8+1]; // top back right
          unityColors[5] = usdCube.colors[8+2]; // top back left
          unityColors[6] = usdCube.colors[4+3]; // back bottom right
          unityColors[7] = usdCube.colors[4+0]; // back bottom left

          unityColors[8] = usdCube.colors[8+0]; // top front right
          unityColors[9] = usdCube.colors[8+3]; // top front left
          unityColors[10] = usdCube.colors[4+2]; // back top right
          unityColors[11] = usdCube.colors[4+1]; // back top left

          unityColors[12] = usdCube.colors[12+1]; // Bottom back right
          unityColors[13] = usdCube.colors[12+2]; // Bottom front right
          unityColors[14] = usdCube.colors[12+3]; // Bottom front left
          unityColors[15] = usdCube.colors[12+0]; // Bottom back left

          unityColors[16] = usdCube.colors[20+1]; // left front bottom
          unityColors[17] = usdCube.colors[20+2]; // left front top
          unityColors[18] = usdCube.colors[20+3]; // left back top
          unityColors[19] = usdCube.colors[20+0]; // left back bottom

          unityColors[20] = usdCube.colors[16+2]; // right back bottom
          unityColors[21] = usdCube.colors[16+3]; // right back top
          unityColors[22] = usdCube.colors[16+0]; // right front top
          unityColors[23] = usdCube.colors[16+1]; // right front bottom 

          unityMesh.colors = unityColors;
        } else if (usdCube.colors.Length == 8) {
          // Vertex colors map on to verts.
          // Note that USD cubes have 8 verts but Unity cube mesh has 24 (6*4)
          // TODO: move the conversion to C++ and use the color management API.
          Debug.Log(unityMesh.vertexCount);
          for (int i = 0; i < usdCube.colors.Length; i++) {
            usdCube.colors[i] = usdCube.colors[i];
          }

          // USD order: front (top-right -> ccw)
          //            back (bottom-left -> ccw (from back perspective))
          var unityColors = new Color[24];
          unityColors[0] = usdCube.colors[3]; // front bottom right
          unityColors[1] = usdCube.colors[2]; // front bottom left
          unityColors[2] = usdCube.colors[0]; // front top right
          unityColors[3] = usdCube.colors[1]; // front top left

          unityColors[4] = usdCube.colors[6]; // top back right
          unityColors[5] = usdCube.colors[5]; // top back left
          unityColors[6] = usdCube.colors[7]; // back bottom right
          unityColors[7] = usdCube.colors[4]; // back bottom left

          unityColors[8] = usdCube.colors[0]; // top front right
          unityColors[9] = usdCube.colors[1]; // top front left
          unityColors[10] = usdCube.colors[6]; // back top right
          unityColors[11] = usdCube.colors[5]; // back top left

          unityColors[12] = usdCube.colors[7]; // Bottom back right
          unityColors[13] = usdCube.colors[3]; // Bottom front right
          unityColors[14] = usdCube.colors[2]; // Bottom front left
          unityColors[15] = usdCube.colors[4]; // Bottom back left

          unityColors[16] = usdCube.colors[2]; // left front bottom
          unityColors[17] = usdCube.colors[1]; // left front top
          unityColors[18] = usdCube.colors[5]; // left back top
          unityColors[19] = usdCube.colors[4]; // left back bottom

          unityColors[20] = usdCube.colors[7]; // right back bottom
          unityColors[21] = usdCube.colors[6]; // right back top
          unityColors[22] = usdCube.colors[0]; // right front top
          unityColors[23] = usdCube.colors[3]; // right front bottom 

          unityMesh.colors = unityColors;
          Debug.Log("vertex colors assigned");
        } else {
          // FaceVarying and uniform both require breaking up the mesh and are not yet handled in
          // this example.
          Debug.LogWarning("Uniform (color per face) and FaceVarying (color per vert per face) "
                         + "display color not supported in this example");
        }
      }

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
      mf.sharedMesh = Mesh.Instantiate(unityMesh);
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

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using USD.NET;
using USD.NET.Unity;
using pxr;

public class UsdAssetImporter {

  public static GameObject ImportUsd(string usdFilePath, string targetAssetPath) {
    USD.NET.Examples.InitUsd.Initialize();

    var scene = USD.NET.Scene.Open(usdFilePath);
    
    if (scene == null) {
      throw new Exception("Failed to open: " + usdFilePath);
    }

    try {
      return ImportUsdScene(scene, usdFilePath);
    } finally {
      scene.Close();
    }
  }

  private static GameObject ImportUsdScene(USD.NET.Scene scene, string usdFilePath) {

    string fileName = UnityTypeConverter.MakeValidIdentifier(
        System.IO.Path.GetFileNameWithoutExtension(usdFilePath));

    var goRoot = new UnityEngine.GameObject(fileName);
    var primMap = USD.NET.Unity.PrimMap.MapAllPrims(scene, goRoot);
    var solidColorMat = new Material(Shader.Find("USD/FastSolidColorShader"));
    solidColorMat.SetFloat("_Glossiness", 0.2f);
    var matMap = new MaterialMap(solidColorMat);

    // Correct for Z-up vs. Y-up disparity.
    if (scene.UpAxis == Scene.UpAxes.Z) {
      goRoot.transform.localRotation = Quaternion.AngleAxis(90, Vector3.right);
    }

    // Collect all transforms, including on meshes, etc.
    foreach (var pathAndXform in scene.ReadAll<XformableSample>()) {
      pathAndXform.sample.ConvertTransform();
      AssignTransform(pathAndXform.sample, primMap[pathAndXform.path]);
    }

    // Import meshes.
    foreach (var pathAndMesh in scene.ReadAll<MeshSample>()) { 
      ImportMesh(scene,
                 pathAndMesh.path,
                 pathAndMesh.sample,
                 primMap[pathAndMesh.path],
                 matMap);
    }

    return goRoot;
  }

  private static PreviewSurfaceSample GetSurfaceShaderPrim(USD.NET.Scene scene, string meshPath) {
    var materialBinding = new MaterialBindingSample();
    scene.Read(meshPath, materialBinding);
    var matPath = materialBinding.binding.GetTarget(0);
    if (matPath == null) {
      //Debug.LogWarning("No material binding found at: <" + meshPath + ">");
      return null;
    }
    var materialSample = new MaterialSample();
    scene.Read(matPath, materialSample);
    if (string.IsNullOrEmpty(materialSample.surface.connectedPath)) {
      Debug.LogWarning("Material surface not connected: <" + matPath + ">");
    }

    var exportSurf = new PreviewSurfaceSample();
    scene.Read(new pxr.SdfPath(materialSample.surface.connectedPath).GetPrimPath(), exportSurf);

    return exportSurf;
  }

  private static void ImportMesh(USD.NET.Scene scene, SdfPath path, MeshSample meshSample, GameObject goMesh, MaterialMap materials) {
    var shaderSample = GetSurfaceShaderPrim(scene, path.ToString());
    BuildMesh(meshSample, shaderSample, goMesh, materials);
  }
  
  static Vector3 GetUpVector(Scene scene) {
    // Note: currently Y and Z are the only valid values.
    // https://graphics.pixar.com/usd/docs/api/group___usd_geom_up_axis__group.html

    switch (scene.UpAxis) {
    case Scene.UpAxes.Y:
      // Note, this is also Unity's default up vector.
      return Vector3.up;
    case Scene.UpAxes.Z:
      return new Vector3(0, 0, 1);
    default:
      throw new Exception("Invalid upAxis value: " + scene.UpAxis.ToString());
    }
  }

  public static Matrix4x4 ChangeBasis(Matrix4x4 xfInput) {
    Matrix4x4 mat = Matrix4x4.identity;
    mat[2, 2] = -1;
    return mat * xfInput * mat.inverse;
  }

  public static Vector3 ChangeBasis(Vector3 point) {
    Matrix4x4 mat = Matrix4x4.identity;
    mat[2, 2] = -1;
    return mat.MultiplyPoint3x4(point);
  }

  // Convert Matrix4x4 into TSR components.
  static void AssignTransform(XformableSample xf, GameObject go) {
    go.transform.localPosition = ExtractPosition(xf.transform);
    go.transform.localScale = ExtractScale(xf.transform);
    go.transform.localRotation = ExtractRotation(xf.transform);
  }

  static void ImportUv(int channel,
                     Mesh unityMesh,
                     object uv) {
    IList uvList = null;
    if (uv != null) {
      Type uvType = uv.GetType();
      if (uvType == typeof(Vector2[])) {
        uvList = ((Vector2[])uv).ToList();
        unityMesh.SetUVs(channel, (List<Vector2>)uvList);
      } else if (uvType == typeof(Vector3[])) {
        uvList = ((Vector3[])uv).ToList();
        /*
        if (uvSemantic == Semantic.Position
            || uvSemantic == Semantic.Vector) {
          for (int i = 0; i < uvList.Count; i++) {
            uvList[i] = ChangeBasis(((List<Vector3>)uvList)[i]);
          }
        }
        */
        unityMesh.SetUVs(channel, (List<Vector3>)uvList);
      } else if (uvType == typeof(Vector4[])) {
        uvList = ((Vector4[])uv).ToList();
        unityMesh.SetUVs(channel, (List<Vector4>)uvList);
      } else {
        throw new Exception("Unexpected UV type: " + uv.GetType());
      }
    }
  }

  // Copy mesh data to Unity and assign mesh with material.
  static void BuildMesh(MeshSample usdMesh,
                        PreviewSurfaceSample usdShader,
                        GameObject go,
                        MaterialMap materials) {
    var mf = go.AddComponent<MeshFilter>();
    var mr = go.AddComponent<MeshRenderer>();
    var unityMesh = new UnityEngine.Mesh();
    Material mat = materials.GetMaterialForSolidColor(Color.white);

    for (int i = 0; i < usdMesh.points.Length; i++) {
      usdMesh.points[i] = ChangeBasis(usdMesh.points[i]);
    }

    unityMesh.vertices = usdMesh.points;

    // Triangulate n-gons.
    // For best performance, triangulate off-line and skip conversion.
    var indices = UnityTypeConverter.ToVtArray(usdMesh.faceVertexIndices);
    var counts = UnityTypeConverter.ToVtArray(usdMesh.faceVertexCounts);
    pxr.UsdGeomMesh.Triangulate(indices, counts);
    UnityTypeConverter.FromVtArray(indices, ref usdMesh.faceVertexIndices);

    if (usdMesh.orientation != Orientation.LeftHanded) {
      // USD is right-handed, so the mesh needs to be flipped.
      // Unity is left-handed, but that doesn't matter here.
      for (int i = 0; i < usdMesh.faceVertexIndices.Length; i += 3) {
        int tmp = usdMesh.faceVertexIndices[i];
        usdMesh.faceVertexIndices[i] = usdMesh.faceVertexIndices[i + 1];
        usdMesh.faceVertexIndices[i + 1] = tmp;
      }
    }

    unityMesh.triangles = usdMesh.faceVertexIndices;

    if (usdMesh.extent.size.x > 0 || usdMesh.extent.size.y > 0 || usdMesh.extent.size.z > 0) {
      usdMesh.extent.center = ChangeBasis(usdMesh.extent.center);
      usdMesh.extent.extents = ChangeBasis(usdMesh.extent.extents);
      unityMesh.bounds = usdMesh.extent;
    } else {
      unityMesh.RecalculateBounds();
    }

    if (usdMesh.normals != null) {
      for (int i = 0; i < usdMesh.points.Length; i++) {
        usdMesh.normals[i] = ChangeBasis(usdMesh.normals[i]);
      }
      unityMesh.normals = usdMesh.normals;
    } else {
      unityMesh.RecalculateNormals();
    }

    if (usdMesh.tangents != null) {
      for (int i = 0; i < usdMesh.points.Length; i++) {
        var w = usdMesh.tangents[i].w;
        var t = ChangeBasis(usdMesh.tangents[i]);
        usdMesh.tangents[i] = new Vector4(t.x, t.y, t.z, w);
      }
      unityMesh.tangents = usdMesh.tangents;
    } else {
      unityMesh.RecalculateTangents();
    }

    if (usdMesh.colors != null) {
      if (usdMesh.colors.Length == 1) {
        // Constant color can just be set on the material.
        mat = materials.GetMaterialForSolidColor(usdMesh.colors[0]);
      } else if (usdMesh.colors.Length == usdMesh.points.Length) {
        // USD Colors are linear, which is what the toolkit shaders expect, so no translation is
        // needed. When the project is in linear space, the brush shaders will do the conversion
        // automatically.
        for (int i = 0; i < usdMesh.colors.Length; i++) {
          usdMesh.colors[i] = usdMesh.colors[i];
        }

        unityMesh.colors = usdMesh.colors;
      } else {
        // FaceVarying and uniform both require breaking up the mesh and are not yet handled in
        // this example.
        Debug.LogWarning("Uniform (color per face) and FaceVarying (color per vert per face) "
                        + "display color not supported in this example");
      }
    }

    //ImportUv(0, usdShader, unityMesh, usdMesh.uv, usdShader.vertexLayout.uv0Semantic);
    //ImportUv(1, usdShader, unityMesh, usdMesh.uv2, usdShader.vertexLayout.uv1Semantic);

    // Final mesh assignment.
    mr.sharedMaterial = mat;
    mf.sharedMesh = unityMesh;
  }

  // ------------------------------------------------------------------------------------------ //
  // Matrix4x4 helpers, not USD specific.
  // ------------------------------------------------------------------------------------------ //

  static Quaternion ExtractRotation(Matrix4x4 mat4) {
    Vector3 forward;
    forward.x = mat4.m02;
    forward.y = mat4.m12;
    forward.z = mat4.m22;
    Vector3 up;
    up.x = mat4.m01;
    up.y = mat4.m11;
    up.z = mat4.m21;
    return Quaternion.LookRotation(forward, up);
  }

  static Vector3 ExtractPosition(Matrix4x4 mat4) {
    Vector3 position;
    position.x = mat4.m03;
    position.y = mat4.m13;
    position.z = mat4.m23;
    return position;
  }

  static Vector3 ExtractScale(Matrix4x4 mat4) {
    Vector3 scale;
    scale.x = new Vector4(mat4.m00, mat4.m10, mat4.m20, mat4.m30).magnitude;
    scale.y = new Vector4(mat4.m01, mat4.m11, mat4.m21, mat4.m31).magnitude;
    scale.z = new Vector4(mat4.m02, mat4.m12, mat4.m22, mat4.m32).magnitude;
    return scale;
  }
}

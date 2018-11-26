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
using System.IO;
using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// A collection of methods used for importing USD Material data into Unity.
  /// </summary>
  public static class MaterialImporter {

    // Cache the token so we don't create a new one on every use.
    // This value should come from USD, however it's currently only a defacto standard.
    private static readonly pxr.TfToken materialBindToken = new pxr.TfToken("materialBind");

    private static Material AlbedoGlossCombiner;

    public delegate Texture2D TextureResolver(pxr.SdfAssetPath textureAssetPath,
                                              SceneImportOptions importOptions);

    /// <summary>
    /// A callback that allows custom texture resolution logic.
    /// </summary>
    /// <remarks>
    /// Note that this method is only called when texture import is enabled. If this event is wired
    /// up, it must return a value, returning null will cause the texture to be skipped. The same
    /// texture may be resolved multiple times as needed.
    /// </remarks>
    public static TextureResolver OnResolveTexture;

    /// <summary>
    /// Computes the bound material using UsdShade's inherited binding logic.
    /// If a material is bound, the request callback is executed to enable the caller to bind the
    /// material to the Unity geometry.
    /// </summary>
    public static void ProcessMaterialBindings(Scene scene, SceneImportOptions importOptions) {
      var requests = importOptions.materialMap.ClearRequestedBindings();
      var prims = new pxr.UsdPrimVector();
      foreach (var pathAndRequest in requests) {
        var prim = scene.GetPrimAtPath(pathAndRequest.Key);
        if (prim == null) { continue; }
        prims.Add(prim);
      }

      var matVector = pxr.UsdShadeMaterialBindingAPI.ComputeBoundMaterials(prims, materialBindToken);
      var materialSample = new MaterialSample();
      var matIndex = -1;

      foreach (var usdMat in matVector) {
        matIndex++;
        Material unityMat = importOptions.materialMap[usdMat.GetPath()];

        if (unityMat == null) {
          continue;
        }

        // PERF: this is slow and garbage-y.
        string path = prims[matIndex].GetPath();

        if (!requests.ContainsKey(path)) {
          Debug.LogError("Source object key not found: " + path);
          continue;
        }
        requests[path](unityMat);
      }
    }

    /// <summary>
    /// Builds a Unity Material from the given USD material sample.
    /// </summary>
    public static Material BuildMaterial(Scene scene,
                                         string materialPath,
                                         MaterialSample sample,
                                         SceneImportOptions options) {
      if (string.IsNullOrEmpty(sample.surface.connectedPath)) {
        Debug.LogWarning("Material has no connected surface: <" + materialPath + ">");
        return null;
      }
      var previewSurf = new PreviewSurfaceSample();
      scene.Read(new pxr.SdfPath(sample.surface.connectedPath).GetPrimPath(), previewSurf);

      // Currently, only UsdPreviewSurface is supported.
      if (previewSurf.id == null || previewSurf.id != "UsdPreviewSurface") {
        Debug.LogWarning("Unknown surface type: <" + sample.surface.connectedPath + ">"
                         + "Surface ID: " + previewSurf.id);
        return null;
      }

      Material mat = null;
      if (previewSurf.useSpecularWorkflow.defaultValue == 1) {
        // Metallic workflow.
        mat = Material.Instantiate(options.materialMap.SpecularWorkflowMaterial);
      } else {
        // Metallic workflow.
        mat = Material.Instantiate(options.materialMap.MetallicWorkflowMaterial);
      }

      StandardShaderImporter.BuildMaterial(scene, mat, sample, previewSurf, options);

      return mat;
    }

    public static Texture2D ImportConnectedTexture<T>(Scene scene,
                                                    Connectable<T> connection,
                                                    SceneImportOptions options) {
      // TODO: look for the expected texture/primvar reader pair.
      var textureSample = new TextureReaderSample();
      var connectedPrimPath = scene.GetSdfPath(connection.connectedPath).GetPrimPath();
      Texture2D result = null;

      scene.Read(connectedPrimPath, textureSample);

      if (textureSample.file.defaultValue != null &&
          !string.IsNullOrEmpty(textureSample.file.defaultValue.GetResolvedPath())) {

        if (OnResolveTexture != null) {
          result = OnResolveTexture(textureSample.file.defaultValue, options);
        } else {
          result = DefaultTextureResolver(textureSample.file.defaultValue, options);
        }
      }

      return result;
    }

    /// <summary>
    /// Private default texture resolver. Copies the given texture into the asset database.
    /// </summary>
    private static Texture2D DefaultTextureResolver(pxr.SdfAssetPath textureAssetPath,
                                                  SceneImportOptions options) {
#if UNITY_EDITOR
      if (!File.Exists(textureAssetPath.GetResolvedPath())) {
        return null;
      }

      string sourcePath = textureAssetPath.GetResolvedPath();
      string destPath = Path.Combine(options.assetImportPath, Path.GetFileName(sourcePath));
      string assetPath = options.assetImportPath + Path.GetFileName(sourcePath);

      string fullPath = destPath;
      if (fullPath.StartsWith("Assets/")) {
        fullPath = fullPath.Substring("Assets/".Length);
      }

      if (!File.Exists(destPath)) {
        UnityEditor.FileUtil.CopyFileOrDirectory(sourcePath, destPath);
        UnityEditor.AssetDatabase.ImportAsset(assetPath);
        UnityEditor.TextureImporter texImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(assetPath);
        if (texImporter == null) {
          Debug.LogError("Failed to load asset: " + assetPath);
          return null;
        } else {
          texImporter.isReadable = true;
          UnityEditor.EditorUtility.SetDirty(texImporter);
          texImporter.SaveAndReimport();
        }
      }

      return (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
#else
      return null;
#endif
    }

    /// <summary>
    /// Copies the roughness texture into the alpha channel fo the rgb texture, inverting it to
    /// convert roughness into gloss.
    /// </summary>
    public static Texture2D CombineRoughnessToGloss(Texture2D rgbTex, Texture2D roughnessTex) {
      if (!AlbedoGlossCombiner) {
        AlbedoGlossCombiner = new Material(Shader.Find("Hidden/USD/CombineAndConvertRoughness"));
      }
      
      var newTex = new Texture2D(rgbTex.width, rgbTex.height,
                                 TextureFormat.ARGB32, mipChain: true);
      AlbedoGlossCombiner.SetTexture("_RoughnessTex", roughnessTex);
      var tmp = RenderTexture.GetTemporary(rgbTex.width, rgbTex.height, 0,
                                           RenderTextureFormat.ARGB32);
      Graphics.Blit(rgbTex, tmp, AlbedoGlossCombiner);
      RenderTexture.active = tmp;
      newTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
      newTex.Apply();
      RenderTexture.ReleaseTemporary(tmp);

#if UNITY_EDITOR
      Texture2D mainTex = rgbTex != null ? rgbTex : roughnessTex;
      var assetPath = UnityEditor.AssetDatabase.GetAssetPath(mainTex.GetInstanceID());

      var bytes = newTex.EncodeToPNG();
      var newAssetPath = assetPath + "-specGloss.png";
      File.WriteAllBytes(newAssetPath, bytes);
      UnityEditor.AssetDatabase.ImportAsset(newAssetPath);
      var texImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(newAssetPath);
      UnityEditor.EditorUtility.SetDirty(texImporter);
      texImporter.SaveAndReimport();
#endif

      return newTex;
    }

    /// <summary>
    /// Reads and returns the UsdPreviewSurface data for the prim at the given path, if present.
    /// </summary>
    /// <param name="scene">The USD scene object.</param>
    /// <param name="primPath">The path to the object in the USD scene.</param>
    /// <returns>A PreviewSurfaceSample if found, otherwise null.</returns>
    public static PreviewSurfaceSample GetSurfaceShaderPrim(Scene scene, string primPath) {
      var materialBinding = new MaterialBindingSample();
      scene.Read(primPath, materialBinding);

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

  }
}

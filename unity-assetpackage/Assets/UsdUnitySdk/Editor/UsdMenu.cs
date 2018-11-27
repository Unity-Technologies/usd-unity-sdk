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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using USD.NET.Unity;

public class UsdMenu : MonoBehaviour {

  public static USD.NET.Scene InitForSave(string defaultName) {
    var filePath = EditorUtility.SaveFilePanel("Export USD File", "", defaultName, "usda");

    if (filePath == null || filePath.Length == 0) {
      return null;
    }

    var fileDir = Path.GetDirectoryName(filePath);

    if (!Directory.Exists(fileDir)) {
      var di = Directory.CreateDirectory(fileDir);
      if (!di.Exists) {
        Debug.LogError("Failed to create directory: " + fileDir);
        return null;
      }
    }

    USD.NET.Examples.InitUsd.Initialize();
    var scene = USD.NET.Scene.Create(filePath);
    scene.Time = 0;
    scene.StartTime = 0;
    scene.EndTime = 0;
    return scene;
  }

  public static USD.NET.Scene InitForOpen() {
    string path = EditorUtility.OpenFilePanel("Import USD File", "", "usd,usda,usdc,abc");
    if (path == null || path.Length == 0) {
      return null;
    }
    USD.NET.Examples.InitUsd.Initialize();
    return USD.NET.Scene.Open(path);
  }

#if false
  [MenuItem("USD/Export (Fast) Selected with Children", true)]
  static bool EnableExportSelectedFast() {
    return Selection.gameObjects.Length > 0;
  }
  [MenuItem("USD/Export (Fast) Selected with Children")]
  static void ExportSelectedFast() {
    ExportSelected(BasisTransformation.FastWithNegativeScale);
  }
#endif

  [MenuItem("USD/Export Selected with Children", true)]
  static bool EnableExportSelectedSlow() {
    return Selection.gameObjects.Length > 0;
  }
  [MenuItem("USD/Export Selected with Children")]
  static void ExportSelectedSlow() {
    ExportSelected(BasisTransformation.SlowAndSafe);
  }

  static void ExportSelected(BasisTransformation basisTransform) {
    USD.NET.Scene scene = null;

    foreach (GameObject go in Selection.gameObjects) {
      if (scene == null) {
        scene = InitForSave(go.name);
        if (scene == null) {
          return;
        }
      }

      try {
        SceneExporter.Export(go, scene, basisTransform,
                             exportUnvarying: true, zeroRootTransform: true);
      } catch (System.Exception ex) {
        Debug.LogException(ex);
        continue;
      }
    }

    if (scene != null) {
      scene.Save();
      scene.Close();
    }
  }

  [MenuItem("USD/Import as GameObjects")]
  public static void ImportUsdToScene() {
    var scene = InitForOpen();
    if (scene == null) {
      return;
    }
    string path = scene.FilePath;

    var specMat = new Material(Shader.Find("Standard (Specular setup)"));
    var metallicMat = new Material(Shader.Find("Standard (Roughness setup)"));
    var solidColorMat = new Material(Shader.Find("USD/StandardVertexColor"));
    solidColorMat.SetFloat("_Glossiness", 0.2f);

    // Time-varying data is not supported and often scenes are written without "Default" time
    // values, which makes setting an arbitrary time safer (because if only default was authored
    // the time will be ignored and values will resolve to default time automatically).
    scene.Time = 1.0;

    var importOptions = new SceneImportOptions();
    importOptions.assetImportPath = GetSelectedAssetPath();
    importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
    importOptions.materialMap.FallbackMasterMaterial = solidColorMat;
    importOptions.materialMap.SpecularWorkflowMaterial = specMat;
    importOptions.materialMap.MetallicWorkflowMaterial = metallicMat;
    //importOptions.meshOptions.generateLightmapUVs = true;

    GameObject parent = null;
    if (Selection.gameObjects.Length > 0) {
      parent = Selection.gameObjects[0];
    }
    try {
      UsdToGameObject(parent, GetObjectName(path), scene, importOptions);
    } finally {
      scene.Close();
    }
  }

#if false
  [MenuItem("USD/Scripted Import")]
  public static void ImportUsdScripted() {
    var scene = InitForOpen();
    if (scene == null) {
      return;
    }
    string path = scene.FilePath;
    string assetPath = "Assets/Test.usd-hook";
    File.WriteAllText(assetPath, path);
    AssetDatabase.ImportAsset(assetPath);
  }
#endif

  [MenuItem("USD/Import as Prefab")]
  public static void ImportUsdToPrefab() {
    var scene = InitForOpen();
    if (scene == null) {
      return;
    }
    string path = scene.FilePath;

    // Time-varying data is not supported and often scenes are written without "Default" time
    // values, which makes setting an arbitrary time safer (because if only default was authored
    // the time will be ignored and values will resolve to default time automatically).
    scene.Time = 1.0;

    var importOptions = new SceneImportOptions();
    importOptions.assetImportPath = GetSelectedAssetPath();
    importOptions.changeHandedness = BasisTransformation.FastWithNegativeScale;
    importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
    importOptions.materialMap.SpecularWorkflowMaterial = new Material(Shader.Find("Standard (Specular setup)"));
    importOptions.materialMap.MetallicWorkflowMaterial = new Material(Shader.Find("Standard (Roughness setup)"));
    importOptions.materialMap.FallbackMasterMaterial = new Material(Shader.Find("USD/StandardVertexColor"));

    var invalidChars = Path.GetInvalidFileNameChars();
    var prefabName = string.Join("_", GetPrefabName(path).Split(invalidChars,
        System.StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    string prefabPath = importOptions.assetImportPath + prefabName + ".prefab";

    try {
      ImportUsdToPrefab(scene, prefabPath, importOptions);
    } finally {
      scene.Close();
    }
  }

  /// <summary>
  /// Returns the selected object path or "Assets/" if no object is selected.
  /// </summary>
  public static string GetSelectedAssetPath() {
    Object[] selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
    foreach (Object obj in selectedAsset) {
      var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
      if (string.IsNullOrEmpty(path)) {
        continue;
      }
      if (File.Exists(path)) {
        path = Path.GetDirectoryName(path);
      }
      if (!path.EndsWith("/")) {
        path += "/";
      }
      return path;
    }
    return "Assets/";
  }

  public static GameObject UsdToGameObject(GameObject parent,
                                           string name,
                                           USD.NET.Scene scene,
                                           SceneImportOptions importOptions) {
    try {
      UsdAssetImporter.ImportUsd(parent, scene, importOptions);
    } finally {
      scene.Close();
    }

    return parent;
  }

  public static void ImportUsdToPrefab(USD.NET.Scene scene, string prefabPath, SceneImportOptions importOptions) {
    string filePath = scene.FilePath;
    var go = new GameObject();
    UsdToGameObject(go, GetPrefabName(filePath), scene, importOptions);
    SceneImporter.SaveAsSinglePrefab(go, prefabPath, importOptions);
    GameObject.DestroyImmediate(go);
  }

  private static string GetObjectName(string path) {
    return UnityTypeConverter.MakeValidIdentifier(Path.GetFileNameWithoutExtension(path));
  }

  private static string GetPrefabName(string path) {
    var fileName = GetObjectName(path);
    return fileName + "_prefab";
  }

}

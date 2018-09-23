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
using UnityEngine;
using UnityEditor;
using USD.NET.Unity;

public class UsdMenu : MonoBehaviour {

  [MenuItem("USD/Import as Prefab")]
  public static void ImportUsdFile() {
    string path = EditorUtility.OpenFilePanel("Import USD File", "", "usd;usda;usdc;abc");
    if (path.Length == 0) {
      return;
    }

    var solidColorMat = new Material(Shader.Find("Standard"));
    solidColorMat.SetFloat("_Glossiness", 0.2f);

    // Time-varying data is not supported and often scenes are written without "Default" time
    // values, which makes setting an arbitrary time safer (because if only default was authored
    // the time will be ignored and values will resolve to default time automatically).
    double time = 1.0;

    var importOptions = new SceneImportOptions();
    importOptions.changeHandedness = BasisTransformation.FastAndDangerous;
    importOptions.materialMap.FallbackMasterMaterial = solidColorMat;

    var invalidChars = Path.GetInvalidFileNameChars();
    var prefabName = string.Join("_", GetPrefabName(path).Split(invalidChars,
        System.StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    string prefabPath = "Assets/" + prefabName + ".prefab";

    ImportUsdFile(path, prefabPath, time, importOptions);
  }

  private static string GetPrefabName(string path) {
    var fileName = UnityTypeConverter.MakeValidIdentifier(Path.GetFileNameWithoutExtension(path));
    return fileName + "_prefab";
  }

  public static GameObject ImportUsdFile(string path, double time, SceneImportOptions importOptions) {
    var go = new GameObject(GetPrefabName(path));

    UsdAssetImporter.ImportUsd(go, path, time, importOptions);

    var usdImporter = go.AddComponent<UsdAssetImporter>();
    usdImporter.m_usdFile = path;
    usdImporter.m_time = time;
    usdImporter.OptionsToState(importOptions);

    return go;
  }

  public static void ImportUsdFile(string path, string prefabPath, double time, SceneImportOptions importOptions) {
    var go = ImportUsdFile(path, time, importOptions);

    SaveAsSinglePrefab(go, prefabPath, importOptions);

    GameObject.DestroyImmediate(go);
  }

  /// <summary>
  /// Custom importer. This works almost exactly as the ScriptedImporter, but does not require
  /// the new API.
  /// </summary>
  public static void SaveAsSinglePrefab(GameObject rootObject,
                                        string prefabPath,
                                        SceneImportOptions importOptions) {
    Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

    GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    GameObject prefab = null;

    if (oldPrefab == null) {
      // Create the prefab. At this point, the meshes do not yet exist and will be
      // dangling references
      prefab = PrefabUtility.CreatePrefab(prefabPath, rootObject);
      AddObjectsToAsset(rootObject, prefab, importOptions);

      // Fix the dangling references.
      prefab = PrefabUtility.ReplacePrefab(rootObject, prefab);
    } else {
      // ReplacePrefab only removes the GameObjects from the asset.
      // Clear out all non-prefab junk (ie, meshes), because otherwise it piles up.
      // The main difference between LoadAllAssetRepresentations and LoadAllAssets
      // is that the former returns MonoBehaviours and the latter does not.
      foreach (var obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabPath)) {
        if (obj is GameObject) {
          continue;
        }
        if (obj == importOptions.materialMap.FallbackMasterMaterial) {
          continue;
        }
        if (obj == importOptions.materialMap.SolidColorMasterMaterial) {
          continue;
        }
        foreach (KeyValuePair<string, Material> kvp in importOptions.materialMap) {
          if (obj == kvp.Value) {
            continue;
          }
        }
        Object.DestroyImmediate(obj, allowDestroyingAssets: true);
      }
      AddObjectsToAsset(rootObject, oldPrefab, importOptions);
      prefab = PrefabUtility.ReplacePrefab(
          rootObject, oldPrefab, ReplacePrefabOptions.ReplaceNameBased);
    }
    
    AssetDatabase.ImportAsset(prefabPath);
  }

  static void AddObjectsToAsset(GameObject rootObject,
                                Object asset,
                                SceneImportOptions importOptions) {
    var meshes = new HashSet<Mesh>();
    var materials = new HashSet<Material>();

    var tempMat = importOptions.materialMap.FallbackMasterMaterial;
    if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "") {
      materials.Add(tempMat);
      AssetDatabase.AddObjectToAsset(tempMat, asset);
    }

    tempMat = importOptions.materialMap.SolidColorMasterMaterial;
    if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "") {
      materials.Add(tempMat);
      AssetDatabase.AddObjectToAsset(tempMat, asset);
    }

    foreach (var mf in rootObject.GetComponentsInChildren<MeshFilter>()) {
      if (mf.sharedMesh != null && meshes.Add(mf.sharedMesh)) {
        AssetDatabase.AddObjectToAsset(mf.sharedMesh, asset);
      }
    }

    foreach (var mf in rootObject.GetComponentsInChildren<MeshRenderer>()) {
      foreach (var mat in mf.sharedMaterials) {
        if (mat == null || !materials.Add(mat)) {
          continue;
        }
        AssetDatabase.AddObjectToAsset(mat, asset);
      }
    }
  }

}

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

public class UsdMenu : MonoBehaviour {

  [MenuItem("USD/Import as Prefab")]
  public static void ImportUsdFile() {
    string path = EditorUtility.OpenFilePanel("Import USD File", "", "usd;usda;usdc;abc");
    if (path.Length == 0) {
      return;
    }
    var go = UsdAssetImporter.ImportUsd(path, "");
    if (go == null) { return; }
    var invalidChars = Path.GetInvalidFileNameChars();
    var prefabName = string.Join("_", go.name.Split(invalidChars,
        System.StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    SaveAsSinglePrefab(go, "Assets/" + prefabName + ".prefab");
    GameObject.DestroyImmediate(go);
  }

  static void SaveAsSinglePrefab(GameObject rootObject,
                                 string prefabPath) {
    Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

    GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    GameObject prefab = null;

    if (oldPrefab == null) {
      // Create the prefab. At this point, the meshes do not yet exist and will be
      // dangling references
      prefab = PrefabUtility.CreatePrefab(prefabPath, rootObject);
      AddObjectsToAsset(rootObject, prefab);

      // Fix the dangling references.
      prefab = PrefabUtility.ReplacePrefab(rootObject, prefab);
    } else {
      // ReplacePrefab only removes the GameObjects from the asset.
      // Clear out all non-prefab junk (ie, meshes), because otherwise it piles up.
      // The main difference between LoadAllAssetRepresentations and LoadAllAssets
      // is that the former returns MonoBehaviours and the latter does not.
      foreach (var obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabPath)) {
        if (!(obj is GameObject)) {
          Object.DestroyImmediate(obj, allowDestroyingAssets: true);
        }
      }
      AddObjectsToAsset(rootObject, oldPrefab);
      prefab = PrefabUtility.ReplacePrefab(
          rootObject, oldPrefab, ReplacePrefabOptions.ReplaceNameBased);
    }
    
    AssetDatabase.ImportAsset(prefabPath);
  }

  static void AddObjectsToAsset(GameObject rootObject, Object asset) {
    var meshes = new HashSet<Mesh>();
    var materials = new HashSet<Material>();
    foreach (var mf in rootObject.GetComponentsInChildren<MeshFilter>()) {
      if (meshes.Add(mf.sharedMesh)) {
        AssetDatabase.AddObjectToAsset(mf.sharedMesh, asset);
      }
    }
    foreach (var mf in rootObject.GetComponentsInChildren<MeshRenderer>()) {
      foreach (var mat in mf.sharedMaterials) {
        if (!materials.Add(mat)) {
          continue;
        }
        AssetDatabase.AddObjectToAsset(mat, asset);
      }
    }
  }

}

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
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

[CustomEditor(typeof(UsdAssetImporter))]
public class UsdImportEditor : ScriptedImporterEditor {
  public override void OnInspectorGUI() {
    var importer = (UsdAssetImporter)this.target;

    var prefabType = PrefabUtility.GetPrefabType(importer.gameObject);
    if (prefabType != PrefabType.ModelPrefab && prefabType != PrefabType.Prefab) {
      EditorGUILayout.LabelField("USD Source File: " + importer.m_usdFile);
      EditorGUILayout.LabelField("USD Time: " + importer.m_usdTime);
      return;
    }

    base.DrawDefaultInspector();

    GUILayout.Space(10);
    EditorGUILayout.LabelField("Asset Actions", EditorStyles.boldLabel);

    if (GUILayout.Button("Apply")) {

      var prefabPath = "";
      Object prefab = null;

      switch (prefabType) {
      case PrefabType.ModelPrefabInstance:
      case PrefabType.PrefabInstance:
#if UNITY_2017 || UNITY_5 || UNITY_2018_1
        prefab = PrefabUtility.GetPrefabParent(importer.gameObject);
#else
        prefab = PrefabUtility.GetCorrespondingObjectFromSource(importer.gameObject);
#endif
        prefab = PrefabUtility.GetPrefabObject(prefab);
        break;
      case PrefabType.ModelPrefab:
      case PrefabType.Prefab:
        prefab = PrefabUtility.GetPrefabObject(importer.gameObject);
        break;
      }

      if (prefab != null) {
        prefabPath = AssetDatabase.GetAssetPath(prefab);
      } else {
        // TODO: when the prefab is unknown or disconnected, replace the local hierarchy.
        throw new System.Exception("Unknown or disconnected prefab");
      }

      var options = new USD.NET.Unity.SceneImportOptions();
      importer.StateToOptions(ref options);
      var scene = USD.NET.Scene.Open(importer.m_usdFile);
      scene.Time = importer.m_usdTime;
      UsdMenu.ImportUsdToPrefab(scene, prefabPath, options);
    }
  }
}

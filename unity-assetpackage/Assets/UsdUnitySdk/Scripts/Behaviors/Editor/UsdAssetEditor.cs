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
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace USD.NET.Unity {

  [CustomEditor(typeof(UsdAsset))]
  public class UsdAssetEditor : ScriptedImporterEditor {

    private Texture2D m_usdLogo;
    private Texture2D m_refreshButton;
    private Texture2D m_trashButton;
    private Texture2D m_reimportButton;
    private Texture2D m_detachButton;

    public override void OnEnable() {
      base.OnEnable();
      if (!m_usdLogo) {
        var script = MonoScript.FromScriptableObject(this);
        var path = AssetDatabase.GetAssetPath(script);
        var rootPath = Path.GetDirectoryName(path);
        m_usdLogo = AssetDatabase.LoadAssetAtPath(rootPath + "/UsdBanner.png",
                                                         typeof(Texture2D)) as Texture2D;
        m_refreshButton = AssetDatabase.LoadAssetAtPath(rootPath + "/RefreshButton.png",
                                                         typeof(Texture2D)) as Texture2D;
        m_trashButton = AssetDatabase.LoadAssetAtPath(rootPath + "/Trash.png",
                                                 typeof(Texture2D)) as Texture2D;
        m_reimportButton = AssetDatabase.LoadAssetAtPath(rootPath + "/Reimport.png",
                                         typeof(Texture2D)) as Texture2D;
        m_detachButton = AssetDatabase.LoadAssetAtPath(rootPath + "/Detach.png",
                                 typeof(Texture2D)) as Texture2D;
      }
    }

    private GameObject GetPrefabObject(GameObject root) {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
      return PrefabUtility.GetPrefabObject(root);
#else
      // This is a great resource for determining object type, but only covers new APIs:
      // https://github.com/Unity-Technologies/UniteLA2018Examples/blob/master/Assets/Scripts/GameObjectTypeLogging.cs
      return PrefabUtility.GetCorrespondingObjectFromSource(root);
#endif
    }

    private bool IsPrefabInstance(GameObject root) {
      return GetPrefabObject(root) != null;
    }

    public override void OnInspectorGUI() {
      var stageRoot = (UsdAsset)this.target;

      if (stageRoot.m_fallbackMaterial == null) {
        Debug.LogWarning("No fallback material set, reverting to default");
        var matMap = new MaterialMap();
        stageRoot.m_fallbackMaterial = matMap.FallbackMasterMaterial;
      }

      if (stageRoot.m_metallicWorkflowMaterial == null) {
        Debug.LogWarning("No metallic material set, reverting to default");
        var matMap = new MaterialMap();
        stageRoot.m_metallicWorkflowMaterial = matMap.MetallicWorkflowMaterial;
      }

      if (stageRoot.m_specularWorkflowMaterial == null) {
        Debug.LogWarning("No specular material set, reverting to default");
        var matMap = new MaterialMap();
        stageRoot.m_specularWorkflowMaterial = matMap.SpecularWorkflowMaterial;
      }

      var gsImageStyle = new GUIStyle();
      gsImageStyle.alignment = TextAnchor.MiddleCenter;
      gsImageStyle.normal.background = EditorGUIUtility.whiteTexture;
      
      gsImageStyle.padding.bottom = 0;
      GUILayout.Space(5);
      GUILayout.BeginHorizontal(gsImageStyle);
      EditorGUILayout.LabelField(new GUIContent(m_usdLogo), GUILayout.MinHeight(40.0f));
      GUILayout.EndHorizontal();

      GUILayout.Space(5);
      GUILayout.BeginHorizontal();

      var buttonStyle = new GUIStyle(GUI.skin.button);

      if (GUILayout.Button(new GUIContent(m_refreshButton, "Refresh values from USD"), buttonStyle)) {
        ReloadFromUsd(stageRoot, forceRebuild: false);
      }

      if (IsPrefabInstance(stageRoot.gameObject)) {
        GUILayout.EndHorizontal();
        var style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;
        EditorGUILayout.LabelField("Edit prefab for destructive operations", style);
      } else {
        if (GUILayout.Button(new GUIContent(m_reimportButton, "Reimport from USD (destructive)"), buttonStyle)) {
          if (EditorUtility.DisplayDialog("Reimport", "Destroy and rebuild all USD objects?\nAny object with a UsdPrimSource will be destroyed and reimported.", "OK", "Cancel")) {
            ReloadFromUsd(stageRoot, forceRebuild: true);
          }
        }
        if (GUILayout.Button(new GUIContent(m_trashButton, "Remove USD Contents (destructive)"), buttonStyle)) {
          if (EditorUtility.DisplayDialog("Clear Contents", "Destroy all USD objects?\nAny object with a UsdPrimSource will be destroyed.", "OK", "Cancel")) {
            DestroyAllImportedObjects(stageRoot);
          }
        }
        if (GUILayout.Button(new GUIContent(m_detachButton, "Detach, remove all USD components"), buttonStyle)) {
          DetachFromUsd(stageRoot);
        }
        GUILayout.EndHorizontal();
      }

      if (Application.isPlaying && GUILayout.Button("Reload from USD (Coroutine)")) {
        ReloadFromUsdAsCoroutine(stageRoot);
      }

      GUILayout.Space(10);
      base.DrawDefaultInspector();
    }

    private void OpenUsdFile(UsdAsset stageRoot) {
      Scene scene = UsdMenu.InitForOpen();
      if (scene == null) {
        return;
      }
      stageRoot.OpenScene(scene);
    }

    private void ReloadFromUsd(UsdAsset stageRoot, bool forceRebuild) {
      stageRoot.Reload(forceRebuild);
      Repaint();
    }

    private void DestroyAllImportedObjects(UsdAsset stageRoot) {
      stageRoot.DestroyAllImportedObjects();
      Repaint();
    }

    private void DetachFromUsd(UsdAsset stageRoot) {
      stageRoot.RemoveAllUsdComponents();
      Repaint();
    }

    private void ReloadFromUsdAsCoroutine(UsdAsset stageRoot) {
      var options = new SceneImportOptions();
      stageRoot.StateToOptions(ref options);
      var parent = stageRoot.gameObject.transform.parent;
      var root = parent ? parent.gameObject : null;
      stageRoot.ImportUsdAsCoroutine(root, stageRoot.m_usdFile, stageRoot.m_usdTimeOffset, options, targetFrameMilliseconds: 5);
    }

  }
}

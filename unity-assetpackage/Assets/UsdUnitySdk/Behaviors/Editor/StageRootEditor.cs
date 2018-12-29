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

  [CustomEditor(typeof(StageRoot))]
  public class StageRootEditor : ScriptedImporterEditor {

    private Texture2D m_usdLogo;

    public override void OnEnable() {
      base.OnEnable();
      if (!m_usdLogo) {
        var script = MonoScript.FromScriptableObject(this);
        var path = AssetDatabase.GetAssetPath(script);
        var rootPath = Path.GetDirectoryName(path);
        m_usdLogo = AssetDatabase.LoadAssetAtPath(rootPath + "/UsdBanner.png",
                                                         typeof(Texture2D)) as Texture2D;
      }
    }

    public override void OnInspectorGUI() {
      var stageRoot = (StageRoot)this.target;

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

      if (GUILayout.Button("Refresh Values from USD")) {
        ReloadFromUsd(stageRoot, forceRebuild: false);
      }

      if (GUILayout.Button("Reimport")) {
        ReloadFromUsd(stageRoot, forceRebuild: true);
      }

      if (Application.isPlaying && GUILayout.Button("Reload from USD (Coroutine)")) {
        ReloadFromUsdAsCoroutine(stageRoot);
      }

      if (GUILayout.Button("Export Transform Overrides")) {
        UsdMenu.MenuExportTransforms();
      }

      if (GUILayout.Button("Open USD File")) {
        OpenUsdFile(stageRoot);
      }

      GUILayout.Space(10);
      base.DrawDefaultInspector();
    }

    private void OpenUsdFile(StageRoot stageRoot) {
      Scene scene = UsdMenu.InitForOpen();
      if (scene == null) {
        return;
      }
      stageRoot.OpenScene(scene);
    }

    private void ReloadFromUsd(StageRoot stageRoot, bool forceRebuild) {
      stageRoot.Reload(forceRebuild);
      Repaint();
    }

    private void ReloadFromUsdAsCoroutine(StageRoot stageRoot) {
      var options = new SceneImportOptions();
      stageRoot.StateToOptions(ref options);
      var parent = stageRoot.gameObject.transform.parent;
      var root = parent ? parent.gameObject : null;
      stageRoot.ImportUsdAsCoroutine(root, stageRoot.m_usdFile, stageRoot.m_usdTime, options, targetFrameMilliseconds: 5);
    }

  }
}

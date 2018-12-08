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
    public override void OnInspectorGUI() {
      var stageRoot = (StageRoot)this.target;

      EditorGUILayout.LabelField("Asset Actions", EditorStyles.boldLabel);

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
        ExportOverrides(stageRoot);
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

      var options = new SceneImportOptions();
      stageRoot.StateToOptions(ref options);
      var parent = stageRoot.gameObject.transform.parent;
      var root = parent ? parent.gameObject : null;

      scene.Time = stageRoot.m_usdTime;
      try {
        SceneImporter.ImportUsd(root, scene, options);
      } finally {
        scene.Close();
      }
    }

    private void ReloadFromUsd(StageRoot stageRoot, bool forceRebuild) {
      var options = new SceneImportOptions();
      stageRoot.StateToOptions(ref options);

      options.forceRebuild = forceRebuild;

      if (string.IsNullOrEmpty(options.projectAssetPath)) {
        options.projectAssetPath = "Assets/";
        stageRoot.OptionsToState(options);
      }

      var root = stageRoot.gameObject;
      var prefab = PrefabUtility.GetPrefabObject(root);
      var assetPath = AssetDatabase.GetAssetPath(prefab);

      // The prefab asset path will be null for prefab instances.
      // When the assetPath is not null, the object is the prefab itself.
      if (!string.IsNullOrEmpty(assetPath)) {
        if (options.forceRebuild) {
          root = new GameObject();
        }

        SceneImporter.ImportUsd(root, stageRoot.m_usdFile, stageRoot.m_usdTime, options);
        SceneImporter.SaveAsSinglePrefab(root, assetPath, options);
        if (options.forceRebuild) {
          GameObject.DestroyImmediate(root);
        }
        Repaint();
      } else {
        // An instance of a prefab.
        // Just reload the scene into memory and let the user decide if they want to send those
        // changes back to the prefab or not.
        SceneImporter.ImportUsd(root, stageRoot.m_usdFile, stageRoot.m_usdTime, options);
      }
    }

    private void ReloadFromUsdAsCoroutine(StageRoot stageRoot) {
      var options = new SceneImportOptions();
      stageRoot.StateToOptions(ref options);
      var parent = stageRoot.gameObject.transform.parent;
      var root = parent ? parent.gameObject : null;
      stageRoot.ImportUsdAsCoroutine(root, stageRoot.m_usdFile, stageRoot.m_usdTime, options, targetFrameMilliseconds: 5);
    }

    private void ExportOverrides(StageRoot sceneToReference) {
      Scene overs = UsdMenu.InitForSave(
          Path.GetFileNameWithoutExtension(sceneToReference.m_usdFile) + "_overs.usda");

      if (overs == null) {
        return;
      }

      var baseLayer = sceneToReference.GetScene();
      if (baseLayer == null) {
        throw new Exception("Could not open base layer: " + sceneToReference.m_usdFile);
      }

      overs.Time = baseLayer.Time;
      overs.StartTime = baseLayer.StartTime;
      overs.EndTime = baseLayer.EndTime;

      overs.WriteMode = Scene.WriteModes.Over;
      overs.UpAxis = baseLayer.UpAxis;

      try {
        SceneExporter.Export(sceneToReference.gameObject,
                             overs,
                             BasisTransformation.SlowAndSafe,
                             exportUnvarying: false,
                             zeroRootTransform: true);

        var rel = ImporterBase.MakeRelativePath(overs.FilePath, sceneToReference.m_usdFile);
        GetFirstPrim(overs).GetReferences().AddReference(rel, GetFirstPrim(baseLayer).GetPath());
      } catch (System.Exception ex) {
        Debug.LogException(ex);
        return;
      } finally {
        baseLayer.Close();
        if (overs != null) {
          overs.Save();
          overs.Close();
        }
      }

    }

    pxr.UsdPrim GetFirstPrim(Scene scene) {
      var children = scene.Stage.GetPseudoRoot().GetAllChildren().GetEnumerator();
      if (!children.MoveNext()) {
        return null;
      }
      return children.Current;
    }

  }
}

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

      if (GUILayout.Button("Reload from USD")) {
        ReloadFromUsd(stageRoot);
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
        StageRoot.ImportUsd(root, scene, options);
        
      } finally {
        scene.Close();
      }
    }

    private void ReloadFromUsd(StageRoot stageRoot) {
      var options = new SceneImportOptions();
      stageRoot.StateToOptions(ref options);
      var parent = stageRoot.gameObject.transform.parent;
      var root = parent ? parent.gameObject : null;
      StageRoot.ImportUsd(root, stageRoot.m_usdFile, stageRoot.m_usdTime, options);
    }

    private void ExportOverrides(StageRoot sceneToReference) {
      Scene overs = UsdMenu.InitForSave(
          Path.GetFileNameWithoutExtension(sceneToReference.m_usdFile) + "_overs.usda");

      if (overs == null) {
        return;
      }

      var baseLayer = Scene.Open(sceneToReference.m_usdFile);
      if (baseLayer == null) {
        throw new System.Exception("Could not open base layer: " + sceneToReference.m_usdFile);
      }

      overs.WriteMode = Scene.WriteModes.Over;
      overs.UpAxis = baseLayer.UpAxis;

      try {
        SceneExporter.Export(sceneToReference.gameObject,
                             overs,
                             sceneToReference.m_changeHandedness,
                             exportUnvarying: false,
                             zeroRootTransform: true);

        var rel = ImporterBase.MakeRelativePath(overs.FilePath, sceneToReference.m_usdFile);
        GetFirstPrim(overs).GetReferences().AddReference(rel, GetFirstPrim(baseLayer).GetPath());
        baseLayer.Close();
      } catch (System.Exception ex) {
        Debug.LogException(ex);
        return;
      }

      if (overs != null) {
        overs.Save();
        overs.Close();
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

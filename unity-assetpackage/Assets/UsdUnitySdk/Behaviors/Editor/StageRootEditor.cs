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

      scene.Time = stageRoot.m_time;
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
      StageRoot.ImportUsd(root, stageRoot.m_usdFile, stageRoot.m_time, options);
    }

    private void ExportOverrides(StageRoot stageRoot) {
      Scene scene = UsdMenu.InitForSave(
          Path.GetFileNameWithoutExtension(stageRoot.m_usdFile) + "_overs.usda");

      if (scene == null) {
        return;
      }

      var baseLayer = Scene.Open(stageRoot.m_usdFile);
      if (baseLayer == null) {
        throw new System.Exception("Could not open base layer: " + stageRoot.m_usdFile);
      }

      scene.WriteMode = Scene.WriteModes.Over;
      scene.UpAxis = baseLayer.UpAxis;

      try {
        SceneExporter.Export(stageRoot.gameObject,
                              scene,
                              stageRoot.m_changeHandedness,
                              exportUnvarying: false);

        var rel = MakeRelativePath(scene.FilePath, stageRoot.m_usdFile);
        GetFirstPrim(scene).GetReferences().AddReference(rel, GetFirstPrim(baseLayer).GetPath());
        baseLayer.Close();
      } catch (System.Exception ex) {
        Debug.LogException(ex);
        return;
      }

      if (scene != null) {
        scene.Save();
        scene.Close();
      }
    }

    pxr.UsdPrim GetFirstPrim(Scene scene) {
      var children = scene.Stage.GetPseudoRoot().GetAllChildren().GetEnumerator();
      if (!children.MoveNext()) {
        return null;
      }
      return children.Current;
    }

    /// <summary>
    /// Creates a relative path from one file or folder to another.
    /// </summary>
    /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
    /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
    /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="UriFormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static String MakeRelativePath(String fromPath, String toPath) {
      if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
      if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

      Uri fromUri = new Uri(fromPath);
      Uri toUri = new Uri(toPath);

      if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

      Uri relativeUri = fromUri.MakeRelativeUri(toUri);
      String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

      if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)) {
        relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      }

      return relativePath;
    }

  }
}

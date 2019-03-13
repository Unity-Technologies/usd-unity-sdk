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

using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD {

  public class UsdMenu : MonoBehaviour {

    public static Scene InitForSave(string defaultName, string fileExtension = "usd") {
      var filePath = EditorUtility.SaveFilePanel("Export USD File", "", defaultName, fileExtension);

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

      InitUsd.Initialize();
      var scene = Scene.Create(filePath);
      scene.Time = 0;
      scene.StartTime = 0;
      scene.EndTime = 0;
      return scene;
    }

    public static Scene InitForOpen() {
      string path = EditorUtility.OpenFilePanel("Import USD File", "", "usd,usda,usdc,abc");
      if (path == null || path.Length == 0) {
        return null;
      }
      InitUsd.Initialize();
      var stage = pxr.UsdStage.Open(path, pxr.UsdStage.InitialLoadSet.LoadNone);
      return Scene.Open(stage);
    }

    [MenuItem("USD/Export Selected with Children", true)]
    static bool EnableMenuExportSelectedWithChildren() {
      return Selection.gameObjects.Length > 0;
    }
    [MenuItem("USD/Export Selected with Children", priority = 50)]
    static void MenuExportSelectedWithChildren() {
      ExportSelected(BasisTransformation.SlowAndSafe);
    }

    [MenuItem("USD/Export Transform Overrides", true)]
    static bool EnableMenuExportTransforms() {
      return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<UsdAsset>();
    }
    [MenuItem("USD/Export Transform Overrides", priority = 50)]
    static public void MenuExportTransforms() {
      var root = Selection.activeGameObject.GetComponentInParent<UsdAsset>();
      var overs = InitForSave(Path.GetFileNameWithoutExtension(root.usdFullPath) + "_overs.usda");
      root.ExportOverrides(overs);
    }

    [MenuItem("USD/Export Selected as USDZ", true)]
    static bool EnableMenuExportSelectedAsUsdz() {
      return Selection.gameObjects.Length == 1;
    }
    [MenuItem("USD/Export Selected as USDZ", priority = 50)]
    static void MenuExportSelectedAsUsdz() {
      var defaultName = Path.GetFileNameWithoutExtension(Selection.activeGameObject.name);
      var filePath = EditorUtility.SaveFilePanel("Export USDZ File", "", defaultName, "usdz");

      if (filePath == null || filePath.Length == 0) {
        return;
      }

      var fileDir = Path.GetDirectoryName(filePath);

      if (!Directory.Exists(fileDir)) {
        var di = Directory.CreateDirectory(fileDir);
        if (!di.Exists) {
          Debug.LogError("Failed to create directory: " + fileDir);
          return;
        }
      }

      UsdzExporter.ExportUsdz(filePath, Selection.activeGameObject);
    }

#if false
    [MenuItem("USD/Save Unity as USD (Experimental)", true)]
    static bool EnableMenuSaveAsUsd() {
      return Selection.gameObjects.Length == 1;
    }
    [MenuItem("USD/Save Unity as USD (Experimental)", priority = 170)]
    static void MenuExportSaveAsUsd() {
      ExportSelected(BasisTransformation.SlowAndSafe, exportMonoBehaviours: true);
    }

    [MenuItem("USD/Load Unity from USD (Experimental)", priority = 170)]
    static void MenuExportLoadFromUsd() {
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
      importOptions.projectAssetPath = GetSelectedAssetPath();
      importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
      importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
      importOptions.usdRootPath = GetDefaultRoot(scene);
      importOptions.importMonoBehaviours = true;

      GameObject root = new GameObject(GetObjectName(importOptions.usdRootPath, path));

      if (Selection.gameObjects.Length > 0) {
        root.transform.SetParent(Selection.gameObjects[0].transform);
      }

      try {
        UsdToGameObject(root, scene, importOptions);
      } finally {
        scene.Close();
      }
    }
#endif

    static private pxr.SdfPath GetDefaultRoot(Scene scene) {
      // We can't safely assume the default prim is the model root, because Alembic files will
      // always have a default prim set arbitrarily.

      // If there is only one root prim, reference this prim.
      var children = scene.Stage.GetPseudoRoot().GetChildren().ToList();
      if (children.Count == 1) {
        return children[0].GetPath();
      }

      // Otherwise there are 0 or many root prims, in this case the best option is to reference
      // them all, to avoid confusion.
      return pxr.SdfPath.AbsoluteRootPath();
    }

    static void ExportSelected(BasisTransformation basisTransform,
                               string fileExtension = "usd",
                               bool exportMonoBehaviours = false) {
      Scene scene = null;

      foreach (GameObject go in Selection.gameObjects) {
        if (scene == null) {
          scene = InitForSave(go.name, fileExtension);
          if (scene == null) {
            return;
          }
        }

        try {
          SceneExporter.Export(go, scene, basisTransform,
                               exportUnvarying: true, zeroRootTransform: false,
                               exportMonoBehaviours: exportMonoBehaviours);
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

    [MenuItem("USD/Import as GameObjects", priority = 0)]
    public static void MenuImportAsGameObjects() {
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
      importOptions.projectAssetPath = GetSelectedAssetPath();
      importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
      importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
      importOptions.usdRootPath = GetDefaultRoot(scene);

      GameObject root = new GameObject(GetObjectName(importOptions.usdRootPath, path));

      if (Selection.gameObjects.Length > 0) {
        root.transform.SetParent(Selection.gameObjects[0].transform);
      }

      try {
        UsdToGameObject(root, scene, importOptions);
      } finally {
        scene.Close();
      }
    }

    [MenuItem("USD/Import as Prefab", priority = 1)]
    public static void MenuImportAsPrefab() {
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
      importOptions.projectAssetPath = GetSelectedAssetPath();
      importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
      importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
      importOptions.usdRootPath = GetDefaultRoot(scene);

      var invalidChars = Path.GetInvalidFileNameChars();
      var prefabName = string.Join("_", GetPrefabName(path).Split(invalidChars,
          System.StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
      string prefabPath = importOptions.projectAssetPath + prefabName + ".prefab";
      prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
      string clipName = Path.GetFileNameWithoutExtension(path);

      var go = new GameObject(GetObjectName(importOptions.usdRootPath, path));
      try {
        UsdToGameObject(go, scene, importOptions);
        SceneImporter.SavePrefab(go, prefabPath, clipName, importOptions);
      } finally {
        GameObject.DestroyImmediate(go);
        scene.Close();
      }
    }

    [MenuItem("USD/Import as Timeline Clip", priority = 2)]
    public static void MenuImportAsTimelineClip() {
      var scene = InitForOpen();
      if (scene == null) {
        return;
      }

      string path = scene.FilePath;

      var invalidChars = Path.GetInvalidFileNameChars();
      var prefabName = string.Join("_", GetPrefabName(path).Split(invalidChars,
          System.StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

      string prefabPath = GetSelectedAssetPath() + prefabName + ".prefab";
      prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
      string clipName = Path.GetFileNameWithoutExtension(path);

      var importOptions = new SceneImportOptions();
      importOptions.projectAssetPath = GetSelectedAssetPath();
      importOptions.changeHandedness = BasisTransformation.FastWithNegativeScale;
      importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
      importOptions.usdRootPath = GetDefaultRoot(scene);

      var go = new GameObject(GetObjectName(importOptions.usdRootPath, path));

      try {
        // Ensure we have at least one GameObject with the import settings.
        XformImporter.BuildSceneRoot(scene, go.transform, importOptions);
        SceneImporter.SavePrefab(go, prefabPath, clipName, importOptions);
      } finally {
        GameObject.DestroyImmediate(go);
      }
    }

    [MenuItem("USD/Unload Subtree", true)]
    static bool EnableMenuUnloadSubtree() {
      return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<UsdAsset>();
    }
    [MenuItem("USD/Unload Subtree", priority = 100)]
    public static void MenuUnloadSubtree() {
      var src = Selection.activeGameObject.transform;
      int count = 0;

      // Potential payload issue: If the payload policy is set to "Dont Load Payloads" on scene
      // load, then the policy is switched to "Load All" and the refresh button is hit, the
      // internal state of the existing payloads will be incorrect because they will have been
      // loaded by virtue of the UsdStage's payload policy, without a load/unload action.
      // The real underlying issue is that the system rebuilds the PrimMap on every refresh, which
      // enables unloaded and deleted prims to come back to life. What should happen is the
      // PrimMap should be reconstructed from the Unity scene on refresh instead. This would
      // ensure changing the load policy would not trigger previously unloaded prims to be loaded.

      foreach (var payload in src.GetComponentsInChildren<UsdPayload>()) {
        if (payload.IsLoaded) {
          payload.Unload();
          payload.Update();
        }
        count++;
      }

      if (count == 0) {
        Debug.LogWarning("No USD payloads found in subtree.");
        return;
      }
    }

    [MenuItem("USD/Load Subtree", true)]
    static bool EnableMenuLoadSubtree() {
      return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<UsdAsset>();
    }
    [MenuItem("USD/Load Subtree", priority = 101)]
    public static void MenuLoadSubtree() {
      var src = Selection.activeGameObject.transform;
      int count = 0;
      foreach (var payload in src.GetComponentsInChildren<UsdPayload>()) {
        if (!payload.IsLoaded) {
          payload.Load();
          payload.Update();
        }
        count++;
      }

      if (count == 0) {
        Debug.LogWarning("No USD payloads found in subtree.");
        return;
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
                                             Scene scene,
                                             SceneImportOptions importOptions) {
      try {
        SceneImporter.ImportUsd(parent, scene, new PrimMap(), importOptions);
      } finally {
        scene.Close();
      }

      return parent;
    }

    private static string GetObjectName(pxr.SdfPath rootPrimName, string path) {
      return pxr.UsdCs.TfIsValidIdentifier(rootPrimName.GetName())
           ? rootPrimName.GetName()
           : GetObjectName(path);
    }

    private static string GetObjectName(string path) {
      return UnityTypeConverter.MakeValidIdentifier(Path.GetFileNameWithoutExtension(path));
    }

    private static string GetPrefabName(string path) {
      var fileName = GetObjectName(path);
      return fileName + "_prefab";
    }

  }

}
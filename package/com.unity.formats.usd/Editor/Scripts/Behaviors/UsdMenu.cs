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

namespace Unity.Formats.USD
{
    public static class UsdMenu
    {
#if !UNITY_EDITOR_WIN
        [MenuItem("USD/Export Selected with Children as USDA", true)]
#endif
        [MenuItem("USD/Export Selected with Children", true)]
        static bool EnableMenuExportSelectedWithChildren()
        {
            return Selection.gameObjects.Length > 0;
        }

        [MenuItem("USD/Export Selected with Children", priority = 50)]
        static void MenuExportSelectedWithChildren()
        {
#if UNITY_EDITOR_WIN
            ExportSelectedWithChildren("usd,usda,usdc");
#else
            ExportSelectedWithChildren("usd");
#endif
        }

#if !UNITY_EDITOR_WIN
        [MenuItem("USD/Export Selected with Children as USDA", priority = 50)]
        static void MenuExportSelectedWithChildrenToUSDA()
        {
            ExportSelectedWithChildren("usda");
        }

#endif

        static void ExportSelectedWithChildren(string fileExtensions)
        {
            var go = Selection.gameObjects.First();
            var filePath = EditorUtility.SaveFilePanel("Export USD File", "", go.name, fileExtensions);
            var scene = ExportHelpers.InitForSave(filePath);
            ExportHelpers.ExportGameObjects(Selection.gameObjects, scene, BasisTransformation.SlowAndSafe);
        }

        [MenuItem("USD/Export Transform Overrides", true)]
        static bool EnableMenuExportTransforms()
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<UsdAsset>();
        }

        [MenuItem("USD/Export Transform Overrides", priority = 50)]
        static public void MenuExportTransforms()
        {
            var root = Selection.activeGameObject.GetComponentInParent<UsdAsset>();
            string fileExtensions;
#if UNITY_EDITOR_WIN
            fileExtensions = "usd,usda,usdc";
#else
            fileExtensions = "usd";
#endif
            var filePath = EditorUtility.SaveFilePanel("Export USD File", "", Path.GetFileNameWithoutExtension(root.usdFullPath) + "_overs", fileExtensions);
            var overs = ExportHelpers.InitForSave(filePath);
            root.ExportOverrides(overs);
        }

        [MenuItem("USD/Export Selected as USDZ", true)]
        static bool EnableMenuExportSelectedAsUsdz()
        {
            return Selection.gameObjects.Length == 1;
        }

        [MenuItem("USD/Export Selected as USDZ", priority = 50)]
        static void MenuExportSelectedAsUsdz()
        {
            var defaultName = Path.GetFileNameWithoutExtension(Selection.activeGameObject.name);
            var filePath = EditorUtility.SaveFilePanel("Export USDZ File", "", defaultName, "usdz");

            if (filePath == null || filePath.Length == 0)
            {
                return;
            }

            var fileDir = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(fileDir))
            {
                var di = Directory.CreateDirectory(fileDir);
                if (!di.Exists)
                {
                    Debug.LogError("Failed to create directory: " + fileDir);
                    return;
                }
            }

            UsdzExporter.ExportUsdz(filePath, Selection.activeGameObject);
        }

#if false
        [MenuItem("USD/Save Unity as USD (Experimental)", true)]
        static bool EnableMenuSaveAsUsd()
        {
            return Selection.gameObjects.Length == 1;
        }

        [MenuItem("USD/Save Unity as USD (Experimental)", priority = 170)]
        static void MenuExportSaveAsUsd()
        {
            ExportSelected(BasisTransformation.SlowAndSafe, exportMonoBehaviours: true);
        }

        [MenuItem("USD/Load Unity from USD (Experimental)", priority = 170)]
        static void MenuExportLoadFromUsd()
        {
            var scene = InitForOpen();
            if (scene == null)
            {
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

            if (Selection.gameObjects.Length > 0)
            {
                root.transform.SetParent(Selection.gameObjects[0].transform);
            }

            try
            {
                UsdToGameObject(root, scene, importOptions);
            }
            finally
            {
                scene.Close();
            }
        }

#endif


        [MenuItem("USD/Import as GameObjects", priority = 0)]
        public static void MenuImportAsGameObjects()
        {
            var scene = ImportHelpers.InitForOpen();
            if (scene == null)
            {
                return;
            }

            ImportHelpers.ImportSceneAsGameObject(scene, Selection.activeGameObject);
            scene.Close();
        }

        [MenuItem("USD/Import as Prefab", priority = 1)]
        public static void MenuImportAsPrefab()
        {
            var scene = ImportHelpers.InitForOpen();
            if (scene == null)
            {
                return;
            }

            ImportHelpers.ImportAsPrefab(scene, null);
        }

        [MenuItem("USD/Import as Timeline Clip", priority = 2)]
        public static void MenuImportAsTimelineClip()
        {
            var scene = ImportHelpers.InitForOpen();
            if (scene == null)
            {
                return;
            }

            ImportHelpers.ImportAsTimelineClip(scene, null);
        }

        [MenuItem("USD/Unload Payload Subtree", true)]
        static bool EnableMenuUnloadSubtree()
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<UsdAsset>();
        }

        [MenuItem("USD/Unload Payload Subtree", priority = 100)]
        public static void MenuUnloadSubtree()
        {
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

            foreach (var payload in src.GetComponentsInChildren<UsdPayload>())
            {
                if (payload.IsLoaded)
                {
                    payload.Unload();
                    payload.Update();
                }

                count++;
            }

            if (count == 0)
            {
                Debug.LogWarning("No USD payloads found in subtree.");
                return;
            }
        }

        [MenuItem("USD/Load Payload Subtree", true)]
        static bool EnableMenuLoadSubtree()
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<UsdAsset>();
        }

        [MenuItem("USD/Load Payload Subtree", priority = 101)]
        public static void MenuLoadSubtree()
        {
            var src = Selection.activeGameObject.transform;
            int count = 0;
            foreach (var payload in src.GetComponentsInChildren<UsdPayload>())
            {
                if (!payload.IsLoaded)
                {
                    payload.Load();
                    payload.Update();
                }

                count++;
            }

            if (count == 0)
            {
                Debug.LogWarning("No USD payloads found in subtree.");
                return;
            }
        }
    }
}

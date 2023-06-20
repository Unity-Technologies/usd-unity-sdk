using System;
using System.IO;
using System.Linq;
using pxr;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using USD.NET;
using USD.NET.Unity;
using Stopwatch = System.Diagnostics.Stopwatch;
using Object = UnityEngine.Object;
using ImportResult = Unity.Formats.USD.UsdEditorAnalytics.ImportResult;

namespace Unity.Formats.USD
{
    public static class ImportHelpers
    {
        public static GameObject ImportSceneAsGameObject(Scene scene, GameObject parent = null, SceneImportOptions importOptions = null)
        {
            if (scene == null || scene.Stage == null)
            {
                Debug.LogError("The USD Scene needs to be opened before being imported.");
                return null;
            }

            string path = scene.FilePath;

            // Time-varying data is not supported and often scenes are written without "Default" time
            // values, which makes setting an arbitrary time safer (because if only default was authored
            // the time will be ignored and values will resolve to default time automatically).
            scene.Time = 1.0;

            if (importOptions == null)
            {
                importOptions = new SceneImportOptions();
                importOptions.usdRootPath = GetDefaultRoot(scene);
            }

            GameObject root = new GameObject(GetObjectName(importOptions.usdRootPath, path));

            if (parent != null)
            {
                root.transform.SetParent(parent.transform);
            }

            try
            {
                UsdToGameObject(root, scene, importOptions);
                return root;
            }
            catch (SceneImporter.ImportException)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(root);
#else
                Object.Destroy(root);
#endif
                return null;
            }
        }

#if UNITY_EDITOR
        public static string ImportAsPrefab(Scene scene, string prefabPath = null)
        {
            string path = scene.FilePath;

            // Time-varying data is not supported and often scenes are written without "Default" time
            // values, which makes setting an arbitrary time safer (because if only default was authored
            // the time will be ignored and values will resolve to default time automatically).
            scene.Time = 1.0;

            var importOptions = new SceneImportOptions();
            importOptions.projectAssetPath = GetSelectedAssetPath();
            importOptions.usdRootPath = GetDefaultRoot(scene);

            if (string.IsNullOrEmpty(prefabPath))
            {
                prefabPath = GetPrefabPath(path, importOptions.projectAssetPath);
            }
            string clipName = Path.GetFileNameWithoutExtension(path);

            var go = new GameObject(GetObjectName(importOptions.usdRootPath, path));
            try
            {
                UsdToGameObject(go, scene, importOptions);
                SceneImporter.SavePrefab(go, prefabPath, clipName, importOptions);
                return prefabPath;
            }
            finally
            {
                Object.DestroyImmediate(go);
                scene.Close();
            }
        }

        public static string ImportAsTimelineClip(Scene scene, string prefabPath = null)
        {
            Stopwatch analyticsTimer = new Stopwatch();
            analyticsTimer.Start();
            string path = scene.FilePath;

            var importOptions = new SceneImportOptions();
            importOptions.projectAssetPath = GetSelectedAssetPath();
            importOptions.usdRootPath = GetDefaultRoot(scene);
            importOptions.changeHandedness = BasisTransformation.FastWithNegativeScale;

            if (string.IsNullOrEmpty(prefabPath))
            {
                prefabPath = GetPrefabPath(path, importOptions.projectAssetPath);
            }
            string clipName = Path.GetFileNameWithoutExtension(path);

            var go = new GameObject(GetObjectName(importOptions.usdRootPath, path));
            try
            {
                // Ensure we have at least one GameObject with the import settings.
                XformImporter.BuildSceneRoot(scene, go.transform, importOptions);
                SceneImporter.SavePrefab(go, prefabPath, clipName, importOptions);
                return prefabPath;
            }
            finally
            {
                analyticsTimer.Stop();
                ImportResult result = ImportResult.Default;
                result.Success = !string.IsNullOrEmpty(prefabPath);
                UsdEditorAnalytics.SendImportEvent(Path.GetExtension(path), analyticsTimer.Elapsed.TotalMilliseconds, result);

                GameObject.DestroyImmediate(go);
                scene.Close();
            }
        }

        /// <summary>
        /// Returns the selected object path or "Assets/" if no object is selected.
        /// </summary>
        static string GetSelectedAssetPath()
        {
            Object[] selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            foreach (Object obj in selectedAsset)
            {
                var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }

                if (!path.EndsWith("/"))
                {
                    path += "/";
                }

                return path;
            }

            return "Assets/";
        }

#endif

        public static Scene InitForOpen(string path = "", UsdStage.InitialLoadSet loadSet = pxr.UsdStage.InitialLoadSet.LoadNone)
        {
#if UNITY_EDITOR
            if (String.IsNullOrEmpty(path) && !UnityEngine.Application.isPlaying)
                path = EditorUtility.OpenFilePanel("Import USD File", "", "usd,usda,usdc,usdz,abc");
#endif

            if (String.IsNullOrEmpty(path))
                return null;

            InitUsd.Initialize();
            // var editingStage = new EditingStage(path);
            var stage = pxr.UsdStage.Open(path, loadSet);
            return Scene.Open(stage);
        }

        static pxr.SdfPath GetDefaultRoot(Scene scene)
        {
            // We can't safely assume the default prim is the model root, because Alembic files will
            // always have a default prim set arbitrarily.

            // If there is only one root prim, reference this prim.
            var children = scene.Stage.GetPseudoRoot().GetChildren().ToList();
            if (children.Count == 1)
            {
                return children[0].GetPath();
            }

            // Otherwise there are 0 or many root prims, in this case the best option is to reference
            // them all, to avoid confusion.
            return pxr.SdfPath.AbsoluteRootPath();
        }

        static GameObject UsdToGameObject(GameObject parent,
            Scene scene,
            SceneImportOptions importOptions)
        {
            try
            {
                SceneImporter.ImportUsd(parent, scene, new PrimMap(), importOptions);
            }
            finally
            {
                scene.Close();
            }

            return parent;
        }

        static string GetObjectName(pxr.SdfPath rootPrimName, string path)
        {
            return pxr.UsdCs.TfIsValidIdentifier(rootPrimName.GetName())
                ? rootPrimName.GetName()
                : GetObjectName(path);
        }

        static string GetObjectName(string path)
        {
            return UnityTypeConverter.MakeValidIdentifier(Path.GetFileNameWithoutExtension(path));
        }

        static string GetPrefabName(string path)
        {
            var fileName = GetObjectName(path);
            return fileName + "_prefab";
        }

#if UNITY_EDITOR
        static string GetPrefabPath(string usdPath, string dataPath)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var prefabName = string.Join("_", GetPrefabName(usdPath).Split(invalidChars,
                System.StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            string prefabPath = dataPath + prefabName + ".prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            return prefabPath;
        }

#endif
    }
}

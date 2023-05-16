using NUnit.Framework;
using pxr;
using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public static class TestUtilityFunction
    {
#if UNITY_EDITOR
        public static Scene OpenUSDSceneWithGUID(string usdGUID, UsdStage.InitialLoadSet loadSet = UsdStage.InitialLoadSet.LoadNone)
        {
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(usdGUID));
            return OpenUSDSceneWithFullPath(usdPath, loadSet);
        }

        // TODO: If materialImportMode = MaterialImportMode.ImportPreviewSurface, it creates all the texture2d files on the root assets
        // Figure out if the texture2ds can be set into a different location - such as our artifacts directory
        public static void DeleteAllTexture2DFiles(string folderName = "Assets")
        {
            foreach (var textureArtifactGUID in AssetDatabase.FindAssets("t:texture2D", new string[] { folderName }))
            {
                var textureFilePath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(textureArtifactGUID));
                File.Delete(textureFilePath);
                TestUtilityFunction.DeleteMetaFile(textureFilePath);
            }

            AssetDatabase.Refresh();
        }
#endif

        public static Scene OpenUSDSceneWithFullPath(string fullPath, UsdStage.InitialLoadSet loadSet = UsdStage.InitialLoadSet.LoadNone)
        {
            var stage = UsdStage.Open(fullPath, loadSet);
            var scene = Scene.Open(stage);

            return scene;
        }

        public static pxr.UsdPrim GetGameObjectPrimInScene(Scene usdScene, GameObject gameObject)
        {
            Assert.IsNotNull(usdScene);
            Assert.IsNotNull(usdScene.Stage);

            return usdScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(gameObject.transform)));
        }

        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }

        public static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static void DeleteMetaFile(string fullPath)
        {
            if (File.Exists(fullPath.TrimEnd('/') + ".meta"))
            {
                File.Delete(fullPath.TrimEnd('/') + ".meta");
            }
        }

        public static string GetUnityScenePath(string relativePath, string sceneName = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = System.Guid.NewGuid().ToString();
            }

            if (!sceneName.EndsWith(".unity"))
            {
                sceneName += ".unity";
            }

            return Path.Combine(relativePath, sceneName);
        }

        public static string GetUSDScenePath(string fullPath, string usdFileName = null)
        {
            if (string.IsNullOrEmpty(usdFileName))
            {
                usdFileName = System.Guid.NewGuid().ToString();
            }

            if (!usdFileName.EndsWith(".usd") && !usdFileName.EndsWith(".usda") && !usdFileName.EndsWith(".usdz"))
            {
                usdFileName += ".usda";
            }

            return Path.Combine(fullPath, usdFileName);
        }
    }
}

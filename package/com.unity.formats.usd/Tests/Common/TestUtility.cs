// Copyright 2023 Unity Technologies. All rights reserved.
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

using NUnit.Framework;
using pxr;
using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;
using USDScene = USD.NET.Scene;

namespace Unity.Formats.USD
{
    public static class TestUtility
    {
#if UNITY_EDITOR
        public static USDScene OpenUSDSceneWithGUID(string usdGUID, UsdStage.InitialLoadSet loadSet = UsdStage.InitialLoadSet.LoadNone)
        {
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(usdGUID));
            return OpenUSDSceneWithFullPath(usdPath, loadSet);
        }

        // TODO: [USDU-455]
        // If materialImportMode = MaterialImportMode.ImportPreviewSurface, it creates all the texture2d files on the root assets
        // Figure out if the texture2ds can be set into a different location - such as our artifacts directory
        public static void DeleteAllTexture2DFiles(string folderName = "Assets")
        {
            DeleteAllUnityAssetOfType("t:texture2D", folderName);
        }

        public static void DeleteAllGeneratedUnityScenes(string folderName = "Assets")
        {
            DeleteAllUnityAssetOfType("t:scene", folderName);
        }

        private static void DeleteAllUnityAssetOfType(string searchType, string folderName)
        {
            foreach (var unityAssetGUID in AssetDatabase.FindAssets(searchType, new string[] { folderName }))
            {
                var assetFilePath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(unityAssetGUID));
                File.Delete(assetFilePath);
                DeleteMetaFile(assetFilePath);
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

        public static void DeleteMetaFile(string fullPath)
        {
            if (File.Exists(fullPath.TrimEnd('/') + FileExtension.Meta))
            {
                File.Delete(fullPath.TrimEnd('/') + FileExtension.Meta);
            }
        }

        public static string GetUnityScenePath(string relativePath, string sceneName = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = System.Guid.NewGuid().ToString();
            }

            sceneName = !sceneName.EndsWith(FileExtension.Unity) ? sceneName += FileExtension.Unity : sceneName;

            return Path.Combine(relativePath, sceneName);
        }

        public static string GetUSDScenePath(string fullPath, string usdFileName = null)
        {
            if (string.IsNullOrEmpty(usdFileName))
            {
                usdFileName = System.Guid.NewGuid().ToString();
            }

            usdFileName = !EndsWithUSDExtension(usdFileName) ? usdFileName += FileExtension.Usda : usdFileName;

            return Path.Combine(fullPath, usdFileName);
        }

        public static string GetPrefabPath(string relativePath, string prefabName = null, bool resource = false)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                prefabName = System.Guid.NewGuid().ToString();
            }

            prefabName = !prefabName.EndsWith(FileExtension.Prefab) ? prefabName += FileExtension.Prefab : prefabName;

            return Path.Combine(relativePath, resource ? "Resources" : "", prefabName);
        }

        public static bool EndsWithUSDExtension(string fileName)
        {
            return fileName.EndsWith(FileExtension.Usd) || fileName.EndsWith(FileExtension.Usda) || fileName.EndsWith(FileExtension.Usdc) || fileName.EndsWith(FileExtension.Usdz);
        }

        public static string CreateTmpUsdFile(string testArtifactFullPath, string fileName = "tempUsd.usda")
        {
            var usdScenePath = GetUSDScenePath(testArtifactFullPath, fileName);
            var scene = USDScene.Create(usdScenePath);
            scene.Save();
            scene.Close();
            return usdScenePath;
        }

        public static USDScene CreateTestUsdScene(string testArtifactFullPath, string fileName = "testUsd.usda")
        {
            var dummyUsdPath = CreateTmpUsdFile(testArtifactFullPath, fileName);
            var scene = ImportHelpers.InitForOpen(dummyUsdPath);
            scene.Write("/root", new XformSample());
            scene.Write("/root/sphere", new SphereSample());
            scene.Save();
            return scene;
        }

        public struct FileExtension
        {
            public const string Usd = ".usd";
            public const string Usdc = ".usdc";
            public const string Usda = ".usda";
            public const string Usdz = ".usdz";
            public const string Unity = ".unity";
            public const string Prefab = ".prefab";
            public const string Meta = ".meta";
        }
    }
}

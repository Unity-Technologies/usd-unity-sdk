// Copyright 2022 Unity Technologies. All rights reserved.
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
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using USDScene = USD.NET.Scene;
using UnityScene = UnityEngine.SceneManagement.Scene;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD.Tests
{
    public abstract class BaseFixture
    {
        protected UnityScene m_UnityScene;
        protected string ArtifactsDirectoryName => "Artifacts";
        protected string ArtifactsDirectoryFullPath => Path.Combine(Application.dataPath, ArtifactsDirectoryName);
        protected string ArtifactsDirectoryRelativePath => Path.Combine("Assets", ArtifactsDirectoryName);
        protected string TestAssetDirectoryName => TestAssetData.Directory.FolderName;
        protected string TestUsdAssetDirectoryRelativePath => Path.Combine("Packages", "com.unity.formats.usd", "Tests", "Common", "Data", TestAssetDirectoryName);

        public string GetUnityScenePath(string sceneName = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = System.Guid.NewGuid().ToString();
            }

            if (!sceneName.EndsWith(".unity"))
            {
                sceneName += ".unity";
            }

            return Path.Combine(ArtifactsDirectoryRelativePath, sceneName);
        }

        public string GetUSDScenePath(string usdFileName = null)
        {
            if (string.IsNullOrEmpty(usdFileName))
            {
                usdFileName = System.Guid.NewGuid().ToString();
            }

            if (!usdFileName.EndsWith(".usd") && !usdFileName.EndsWith(".usda") && !usdFileName.EndsWith(".usdz"))
            {
                usdFileName += ".usda";
            }

            return Path.Combine(ArtifactsDirectoryFullPath, usdFileName);
        }

        public string GetPrefabPath(string prefabName = null, bool resource = false)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                prefabName = System.Guid.NewGuid().ToString();
            }

            if (!prefabName.EndsWith(".prefab"))
            {
                prefabName += ".prefab";
            }

            return Path.Combine(ArtifactsDirectoryRelativePath, resource ? "Resources" : "", prefabName);
        }

        public string GetTestAssetPath(string fileName)
        {
            if (!fileName.EndsWith(TestAssetData.Extension.Usda))
            {
                fileName += TestAssetData.Extension.Usda;
            }
            return Path.GetFullPath(Path.Combine(TestUsdAssetDirectoryRelativePath, fileName));
        }

        public string CreateTmpUsdFile(string fileName = "tempUsd.usda")
        {
            var usdScenePath = GetUSDScenePath(fileName);
            var scene = USDScene.Create(usdScenePath);
            scene.Save();
            scene.Close();
            return usdScenePath;
        }

        public Scene CreateTestUsdScene(string fileName = "testUsd.usda")
        {
            var dummyUsdPath = CreateTmpUsdFile(fileName);
            var scene = ImportHelpers.InitForOpen(dummyUsdPath);
            scene.Write("/root", new XformSample());
            scene.Write("/root/sphere", new SphereSample());
            scene.Save();
            return scene;
        }

        [SetUp]
        public void InitUSDAndArtifactsDirectory()
        {
            InitUsd.Initialize();
            CleanupTestArtifacts();

            if (!Directory.Exists(ArtifactsDirectoryFullPath))
            {
                Directory.CreateDirectory(ArtifactsDirectoryFullPath);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }

        [TearDown]
        public void CleanupTestArtifacts()
        {
            if (Directory.Exists(ArtifactsDirectoryFullPath))
            {
                Directory.Delete(ArtifactsDirectoryFullPath, true);
            }

            DeleteMetaFile(ArtifactsDirectoryFullPath);

#if UNITY_EDITOR
            // TODO: If materialImportMode = MaterialImportMode.ImportPreviewSurface, it creates all the texture2d files on the root assets
            // Figure out if the texture2ds can be set into a different location - such as our artifacts directory
            foreach (var textureArtifactGUID in AssetDatabase.FindAssets("t:texture2D", new string[] { "Assets" }))
            {
                var textureFilePath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(textureArtifactGUID));
                File.Delete(textureFilePath);
                DeleteMetaFile(textureFilePath);
            }

            AssetDatabase.Refresh();
#endif
        }

        private void DeleteMetaFile(string fullPath)
        {
            if (File.Exists(fullPath.TrimEnd('/') + ".meta"))
            {
                File.Delete(fullPath.TrimEnd('/') + ".meta");
            }
        }
    }
}

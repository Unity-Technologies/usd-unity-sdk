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

using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityScene = UnityEngine.SceneManagement.Scene;
using USD.NET;
using USD.NET.Unity;
using System;

namespace Unity.Formats.USD.Tests
{
    public abstract class BaseFixture
    {
        protected UnityScene m_UnityScene;
        protected string ArtifactsDirectoryName => "Artifacts";
        protected string ArtifactsDirectoryFullPath => Path.Combine(Application.dataPath, ArtifactsDirectoryName);
        protected string ArtifactsDirectoryRelativePath => Path.Combine("Assets", ArtifactsDirectoryName);

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
                try
                {
                    Directory.Delete(ArtifactsDirectoryFullPath, true);
                }
                catch (Exception e)
                {
                    Debug.Log("Artifact Clean up has failed - This should not happen in most cases, but even if so, the test case should not be affected.");
                    Debug.Log($"Exception Message: {e.Message}");
                }
            }

            TestUtility.DeleteMetaFile(ArtifactsDirectoryFullPath);

#if UNITY_EDITOR
            // TODO: If materialImportMode = MaterialImportMode.ImportPreviewSurface, it creates all the texture2d files on the root assets
            // Figure out if the texture2ds can be set into a different location - such as our artifacts directory
            TestUtility.DeleteAllTexture2DFiles();
            TestUtility.DeleteAllGeneratedUnityScenes();
#endif
        }
    }
}

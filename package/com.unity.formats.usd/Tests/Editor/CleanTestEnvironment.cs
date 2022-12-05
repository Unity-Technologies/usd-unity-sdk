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

using NUnit.Framework;

using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

using System.IO;

public abstract class CleanTestEnvironment
{
    protected Scene m_UnityScene;
    protected string ArtifactsDirectory = "Assets/Artifacts/";

    [SetUp]
    public void CreateTestArtifactsAndStreamingAssetsDirectories()
    {
        m_UnityScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

        if (!Directory.Exists(ArtifactsDirectory))
        {
            Directory.CreateDirectory(ArtifactsDirectory);
            AssetDatabase.Refresh();
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(ArtifactsDirectory))
        {
            FileUtil.DeleteFileOrDirectory(ArtifactsDirectory);
            FileUtil.DeleteFileOrDirectory(ArtifactsDirectory.TrimEnd('/') + ".meta");
            AssetDatabase.Refresh();
        }
    }

    public string GetUnityScenePath(string sceneName)
    {
        return ArtifactsDirectory + sceneName + ".unity";
    }
}

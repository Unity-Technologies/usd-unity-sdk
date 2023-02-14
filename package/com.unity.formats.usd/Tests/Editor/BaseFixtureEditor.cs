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
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class BaseFixtureEditor : BaseFixture
    {
        [SetUp]
        public void EnsureEmptyScene()
        {
            m_UnityScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        protected Scene OpenUSDGUIDAssetScene(string guid, out string filePath)
        {
            filePath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(guid));
            var stage = pxr.UsdStage.Open(filePath, pxr.UsdStage.InitialLoadSet.LoadNone);
            return Scene.Open(stage);
        }
    }
}

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

using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Unity.Formats.USD.Tests
{
    public class ImportHelpersTests : BaseFixtureRuntime
    {
        [Test]
        public void InitForOpenTest_EmptyPath()
        {
            var scene = ImportHelpers.InitForOpen("");
            Assert.IsNull(scene);
        }

        [Test]
        public void ImportAsGameObjects_CleanupAfterErrorAtRoot()
        {
            var scenePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = ImportHelpers.InitForOpen(scenePath);
            scene.Close();

            // This will cause an error as scene is closed
            var usdObject = ImportHelpers.ImportSceneAsGameObject(scene);

            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            Assert.IsEmpty(rootGameObjects.Where(o => o == usdObject), "UsdAsset GameObject was not cleaned up after error at root");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "The USD Scene needs to be opened before being imported.");
        }

        [Test]
        public void ImportAsGameObjects_ImportAtRoot()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            bool usdRootIsRoot = Array.Find(SceneManager.GetActiveScene().GetRootGameObjects(), r => r == root);
            Assert.IsTrue(usdRootIsRoot, "UsdAsset GameObject is not a root GameObject.");
        }
    }
}

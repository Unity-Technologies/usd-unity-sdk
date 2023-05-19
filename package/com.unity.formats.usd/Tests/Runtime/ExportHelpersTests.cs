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
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class ExportHelpersTests : BaseFixtureRuntime
    {
        [Test]
        public void InitForSave_EmptyFile()
        {
            var scene = ExportHelpers.InitForSave("");
            Assert.IsNull(scene);
        }

        [Test]
        public void InitForSave_NullFile()
        {
            var scene = ExportHelpers.InitForSave(null);
            Assert.IsNull(scene);
        }

        [Test]
        public void InitForSave_ValidPath()
        {
            var filePath = TestUtility.GetUSDScenePath(ArtifactsDirectoryFullPath, "dummyUsd.usd");
            var scene = ExportHelpers.InitForSave(filePath);
            Assert.IsNotNull(scene);
            Assert.IsNotNull(scene.Stage);
            scene.Close();
        }

        [Test]
        public void ExportGameObjects_NullScene()
        {
            var filePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = Scene.Open(filePath);
            scene.Close();
            var fileInfoBefore = new FileInfo(filePath);

            Assert.DoesNotThrow(delegate ()
            {
                ExportHelpers.ExportGameObjects(null, null, BasisTransformation.SlowAndSafe);
            });
            var fileInfoAfter = new FileInfo(filePath);
            Assert.AreEqual(fileInfoBefore.Length, fileInfoAfter.Length);
        }

        [Test]
        public void ExportGameObjects_EmptyList()
        {
            var filePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = Scene.Open(filePath);
            var fileInfoBefore = new FileInfo(filePath);
            Assert.DoesNotThrow(delegate ()
            {
                ExportHelpers.ExportGameObjects(new GameObject[] { }, scene, BasisTransformation.SlowAndSafe);
            });
            var fileInfoAfter = new FileInfo(filePath);
            Assert.AreEqual(fileInfoBefore.Length, fileInfoAfter.Length);
            scene.Close();
        }

        [Test]
        public void ExportGameObjects_InvalidGO()
        {
            var filePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = Scene.Open(filePath);
            Assert.DoesNotThrow(delegate ()
            {
                ExportHelpers.ExportGameObjects(new GameObject[] { null }, scene, BasisTransformation.SlowAndSafe);
            });

            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, "NullReferenceException: Object reference not set to an instance of an object");
        }

        [Test]
        public void ExportGameObjects_ValidGO()
        {
            var filePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = Scene.Open(filePath);
            ExportHelpers.ExportGameObjects(new[] { new GameObject("test") }, scene, BasisTransformation.SlowAndSafe);
            scene = Scene.Open(filePath);
            var paths = scene.Stage.GetAllPaths();
            Debug.Log(scene.Stage.GetRootLayer().ExportToString());
            Assert.AreEqual(2, paths.Count);
            scene.Close();
        }

        [Test]
        public void ExportGameObjects_SceneClosedAfterExport()
        {
            var filePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = Scene.Open(filePath);
            ExportHelpers.ExportGameObjects(new[] { new GameObject("test") }, scene, BasisTransformation.SlowAndSafe);
            Assert.IsNull(scene.Stage);
        }
    }
}

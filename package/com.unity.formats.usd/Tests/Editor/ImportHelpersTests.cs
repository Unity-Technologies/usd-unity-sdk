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
using UnityEditor;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class ImportHelpersTests : BaseFixtureEditor
    {
        Scene m_scene;

        [SetUp]
        public void CreateTestScene()
        {
            m_scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
        }

        [Test]
        public void ImportAsPrefabTest_SceneClosedAfterImport()
        {
            ImportHelpers.ImportAsPrefab(m_scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));
            Assert.IsNull(m_scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsTimelineClipTest_SceneClosedAfterImport()
        {
            ImportHelpers.ImportAsTimelineClip(m_scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));
            Assert.IsNull(m_scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsPrefabTest_ImportClosedScene_Error()
        {
            m_scene.Close();
            Assert.Throws<System.NullReferenceException>(() => ImportHelpers.ImportAsPrefab(m_scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath)));
        }

        [Test]
        public void ImportAsTimelineClipTest_ImportClosedScene_Error()
        {
            m_scene.Close();
            Assert.Throws<System.NullReferenceException>(() => ImportHelpers.ImportAsTimelineClip(m_scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath)));
        }

        [Test]
        public void ImportAsPrefabTest_ContentOk()
        {
            var assetPath = ImportHelpers.ImportAsPrefab(m_scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));

            Assert.IsTrue(File.Exists(assetPath));
            var usdAsObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // ExpectedGameObjectCount: The Root GameObject + Sphere GameObject added
            // ExpectedPrimSourceCount: Root Source + Sphere Source
            // ExpectedMaterialCount: The 3 default materials + 1 material from Sphere meshRender
            ImportAssert.Editor.IsValidImport(usdAsObjects, expectedGameObjectCount: 2, expectedPrimSourceCount: 2, expectedMaterialCount: 4);
        }

        [Test]
        public void ImportAsTimelineClipTest_ContentOk()
        {
            // Import as timeline clip should not create a hierarchy, only the root and the playable
            var assetPath = ImportHelpers.ImportAsTimelineClip(m_scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));

            Assert.IsTrue(File.Exists(assetPath));
            var usdAsObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // ExpectedGameObjectCount: The Root GameObject
            // ExpectedPrimSourceCount: 0 TODO: Shouldnt there be a prim source object for the root object?
            // ExpectedMaterialCount: The 3 default materials
            ImportAssert.Editor.IsValidImport(usdAsObjects, expectedGameObjectCount: 1, expectedPrimSourceCount: 0, expectedMaterialCount: 3);
        }

        public void ImportAsPrefab_TextureDataImported()
        {
            // TODO: Implement when we can change the SceneImportOptions when calling ImportHelpers.ImportAsPrefab
        }
    }
}

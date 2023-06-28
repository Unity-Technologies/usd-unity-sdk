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
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD.Tests
{
    public class ImportHelpersTests : BaseFixtureEditor
    {
        [Test]
        public void InitForOpenTest_ValidPath_Succeeds()
        {
            var dummyUsdPath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = ImportHelpers.InitForOpen(dummyUsdPath);
            Assert.NotNull(scene);
            Assert.NotNull(scene.Stage);
            scene.Close();
        }

        [Test]
        public void InitForOpenTest_InvalidPath_ThrowsAndLogs()
        {
            var ex = Assert.Throws<NullReferenceException>(
                delegate { ImportHelpers.InitForOpen("/this/is/an/invalid/path.usd"); }, "Opening a non existing file should throw an NullReferenceException");
            Assert.That(ex.Message, Is.EqualTo("Null stage"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, "ApplicationException: USD ERROR: Failed to open layer @/this/is/an/invalid/path.usd@");
        }

        [Test]
        public void ImportAsGameObjects_ImportUnderParent()
        {
            var root = new GameObject("thisIsTheRoot");
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            var usdRoot = ImportHelpers.ImportSceneAsGameObject(scene, root);
            Assert.AreEqual(root.transform, usdRoot.transform.root, "UsdAsset is not a children of the given parent.");
        }

        [Test]
        public void ImportAsGameObjects_SceneClosedAfterImport()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            ImportHelpers.ImportSceneAsGameObject(scene);
            Assert.IsNull(scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsGameObjects_ImportClosedScene_LogsError()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            scene.Close();
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            Assert.IsNull(root);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "The USD Scene needs to be opened before being imported.");
        }

        [Test]
        public void ImportAsGameObjects_CleanupAfterErrorUnderParent()
        {
            var parent = new GameObject();
            var scenePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = ImportHelpers.InitForOpen(scenePath);
            scene.Close();

            // This will cause an error as scene is closed
            ImportHelpers.ImportSceneAsGameObject(scene, parent);

            Assert.IsTrue(parent.transform.childCount == 0, "UsdAsset GameObject was not cleaned up after error under parent");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "The USD Scene needs to be opened before being imported.");
        }

        [Test]
        public void ImportAsGameObjects_UnderInactiveParent()
        {
            var parent = new GameObject();
            var scenePath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath);
            var scene = ImportHelpers.InitForOpen(scenePath);

            parent.SetActive(false);
            var usdObject = ImportHelpers.ImportSceneAsGameObject(scene, parent);

            Assert.IsFalse(usdObject.activeInHierarchy);
            Assert.IsTrue(usdObject.activeSelf, "The USD Scene is self-inactive when imported under an inactive parent");
        }

        [Test]
        public void ImportAsPrefabTest_SceneClosedAfterImport()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            ImportHelpers.ImportAsPrefab(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));
            Assert.IsNull(scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsTimelineClipTest_SceneClosedAfterImport()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            ImportHelpers.ImportAsTimelineClip(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));
            Assert.IsNull(scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsPrefabTest_ImportClosedScene_Error()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            scene.Close();
            Assert.Throws<System.NullReferenceException>(() => ImportHelpers.ImportAsPrefab(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath)));
        }

        [Test]
        public void ImportAsTimelineClipTest_ImportClosedScene_Error()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            scene.Close();
            Assert.Throws<System.NullReferenceException>(() => ImportHelpers.ImportAsTimelineClip(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath)));
        }

        [Test]
        public void ImportAsPrefabTest_ContentOk()
        {
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            var assetPath = ImportHelpers.ImportAsPrefab(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));

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
            var scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath);
            // Import as timeline clip should not create a hierarchy, only the root and the playable
            var assetPath = ImportHelpers.ImportAsTimelineClip(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath));

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

        [TestCase(TestDataGuids.Material.TexturedOpaqueUsd, "TexturedOpaque")]
        [TestCase(TestDataGuids.Material.TexturedTransparentCutoutUsd, "TexturedTransparent_Cutout"), Ignore("[USDU-232] Test On HDRP")]
        public void ImportAsGameObjects_TextureDataImported(string testAssetGUID, string testAssetFileName)
        {
            var scene = TestUtility.OpenUSDSceneWithGUID(testAssetGUID);
            var usdObject = ImportHelpers.ImportSceneAsGameObject(scene, importOptions:
                new SceneImportOptions()
                {
                    materialImportMode = MaterialImportMode.ImportPreviewSurface
                }
            );

            ImportAssert.IsTextureDataSaved(usdObject, testAssetFileName, isPrefab: false);
        }

        [Ignore("[USDU-275] | [USDU-230] | [FTV-202]")]
        [TestCase(TestDataGuids.Material.TexturedOpaqueUsd, "TexturedOpaque")]
        [TestCase(TestDataGuids.Material.TexturedTransparentCutoutUsd, "TexturedTransparent_Cutout")]
        public void ImportAsGameObject_TextureDataImported_FromUsdz(string testAssetGUID, string testAssetFileName)
        {
            var scene = TestUtility.OpenUSDSceneWithGUID(testAssetGUID);
            var importedUsdObject = ImportHelpers.ImportSceneAsGameObject(scene, importOptions:
                new SceneImportOptions()
                {
                    materialImportMode = MaterialImportMode.ImportPreviewSurface
                }
            );

            var usdzPath = TestUtility.GetUSDScenePath(ArtifactsDirectoryFullPath, importedUsdObject.name + TestUtility.FileExtension.Usdz);
            UsdzExporter.ExportUsdz(usdzPath, importedUsdObject);

            var usdzScene = ImportHelpers.InitForOpen(usdzPath);
            var usdzObject = ImportHelpers.ImportSceneAsGameObject(usdzScene, importOptions:
                new SceneImportOptions()
                {
                    materialImportMode = MaterialImportMode.ImportPreviewSurface
                }
            );

            // [USDU-275] | [FTV-202] | [USDU-230]
            ImportAssert.IsTextureDataSaved(usdzObject.transform.GetChild(0).gameObject, testAssetFileName, isPrefab: false);
        }

        [TestCase(TestDataGuids.Instancer.UpAxisYLeftHandedUsd, Description = "Up Axis: Y & Left Handed")]
        [TestCase(TestDataGuids.Instancer.UpAxisYRightHandedUsd, Description = "Up Axis: Y & Right Handed")]
        [TestCase(TestDataGuids.Instancer.UpAxisZLeftHandedUsd, Description = "Up Axis: Z & Left Handed")]
        [TestCase(TestDataGuids.Instancer.UpAxisZRightHandedUsd, Description = "Up Axis: Z & Right Handed")]
        public void ImportInstancerAsGameObject_VertexCheck(string testAssetGUID)
        {
            var originalVertices = new[]
            {
                new Vector3(-1, 0, 3),
                new Vector3(-1, 0, 0),
                new Vector3(0, -1, 1),
                new Vector3(-2, -3, -1),
                new Vector3(-2, -1, 1),
                new Vector3(-3, -0.5f, -1),
                new Vector3(-1, -2, 1),
                new Vector3(-5, -2, -1)
            };

            var testScene = TestUtility.OpenUSDSceneWithGUID(testAssetGUID);

            var testInstanceObjectMesh = ImportHelpers.ImportSceneAsGameObject(testScene).GetComponentInChildren<MeshFilter>().sharedMesh;

            foreach (var vertex in testInstanceObjectMesh.vertices)
            {
                // Flip the z-axis value to compensate for the z-axis difference between Unity and USD
                var zFlippedVertex = new Vector3(vertex.x, vertex.y, -vertex.z);
                Assert.Contains(zFlippedVertex, originalVertices, $"Z Axis flipped vertex <{zFlippedVertex}> not found in original vertices");
            }
        }
    }
}

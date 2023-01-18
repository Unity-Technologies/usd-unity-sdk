using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using USD.NET.Unity;
using System.Collections.Generic;
using USD.NET;
using System.Linq;

namespace Unity.Formats.USD.Tests
{
    public class ImportHelpersTests : BaseFixtureRuntime
    {
        [Test]
        public void InitForOpenTest_ValidPath_Succeeds()
        {
            var dummyUsdPath = CreateTmpUsdFile();
            var scene = ImportHelpers.InitForOpen(dummyUsdPath);
            Assert.NotNull(scene);
            Assert.NotNull(scene.Stage);
            scene.Close();
        }

        [Test]
        public void InitForOpenTest_EmptyPath()
        {
            var scene = ImportHelpers.InitForOpen("");
            Assert.IsNull(scene);
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
        public void ImportAsGameObjects_ImportAtRoot()
        {
            var scene = CreateTestUsdScene();
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            bool usdRootIsRoot = Array.Find(SceneManager.GetActiveScene().GetRootGameObjects(), r => r == root);
            Assert.IsTrue(usdRootIsRoot, "UsdAsset GameObject is not a root GameObject.");
        }

        [Test]
        public void ImportAsGameObjects_ImportUnderParent()
        {
            var root = new GameObject("thisIsTheRoot");
            var scene = CreateTestUsdScene();
            var usdRoot = ImportHelpers.ImportSceneAsGameObject(scene, root);
            Assert.AreEqual(root.transform, usdRoot.transform.root, "UsdAsset is not a children of the given parent.");
        }

        [Test]
        public void ImportAsGameObjects_SceneClosedAfterImport()
        {
            var scene = CreateTestUsdScene();
            ImportHelpers.ImportSceneAsGameObject(scene);
            Assert.IsNull(scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsGameObjects_ImportClosedScene_LogsError()
        {
            var scene = CreateTestUsdScene();
            scene.Close();
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            Assert.IsNull(root);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "The USD Scene needs to be opened before being imported.");
        }

        [Test]
        public void ImportAsGameObjects_CleanupAfterErrorAtRoot()
        {
            var scenePath = CreateTmpUsdFile();
            var scene = ImportHelpers.InitForOpen(scenePath);
            scene.Close();

            // This will cause an error as scene is closed
            var usdObject = ImportHelpers.ImportSceneAsGameObject(scene);

            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            Assert.IsEmpty(rootGameObjects.Where(o => o == usdObject), "UsdAsset GameObject was not cleaned up after error at root");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "The USD Scene needs to be opened before being imported.");
        }

        [Test]
        public void ImportAsGameObjects_CleanupAfterErrorUnderParent()
        {
            var parent = new GameObject();
            var scenePath = CreateTmpUsdFile();
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
            var scenePath = CreateTmpUsdFile();
            var scene = ImportHelpers.InitForOpen(scenePath);

            parent.SetActive(false);
            var usdObject = ImportHelpers.ImportSceneAsGameObject(scene, parent);

            Assert.IsFalse(usdObject.activeInHierarchy);
            Assert.IsTrue(usdObject.activeSelf, "The USD Scene is self-inactive when imported under an inactive parent");
        }

        [TestCase(TestAssetData.FileName.TexturedOpaque, Description = "Opaque Texture")]
        [TestCase(TestAssetData.FileName.TexturedTransparent_Cutout, Description = "Transparent Cutout Texture"), Ignore("[USDU-232] Test On HDRP")]
        public void ImportAsGameObjects_TextureDataImported(string fileName)
        {
            var scene = ImportHelpers.InitForOpen(GetTestAssetPath(fileName));
            var usdObject = ImportHelpers.ImportSceneAsGameObject(scene, importOptions:
                new SceneImportOptions()
                {
                    materialImportMode = MaterialImportMode.ImportPreviewSurface
                }
            );

            ImportAssert.IsTextureDataSaved(usdObject, fileName, isPrefab: false);
        }

        [Ignore("[USDU-275] | [USDU-230] | [FTV-202]")]
        [TestCase(TestAssetData.FileName.TexturedOpaque, Description = "Opaque Texture")]
        [TestCase(TestAssetData.FileName.TexturedTransparent_Cutout, Description = "Transparent Cutout Texture")]
        public void ImportAsGameObject_TextureDataImported_FromUsdz(string fileName)
        {
            var scene = ImportHelpers.InitForOpen(GetTestAssetPath(fileName));
            var importedUsdObject = ImportHelpers.ImportSceneAsGameObject(scene, importOptions:
                new SceneImportOptions()
                {
                    materialImportMode = MaterialImportMode.ImportPreviewSurface
                }
            );

            var usdzPath = GetUSDScenePath(importedUsdObject.name + ".usdz");
            UsdzExporter.ExportUsdz(usdzPath, importedUsdObject);

            var usdzScene = ImportHelpers.InitForOpen(usdzPath);
            var usdzObject = ImportHelpers.ImportSceneAsGameObject(usdzScene, importOptions:
                new SceneImportOptions()
                {
                    materialImportMode = MaterialImportMode.ImportPreviewSurface
                }
            );

            // [USDU-275] | [FTV-202] | [USDU-230]
            ImportAssert.IsTextureDataSaved(usdzObject.transform.GetChild(0).gameObject, fileName, isPrefab: false);
        }
    }
}

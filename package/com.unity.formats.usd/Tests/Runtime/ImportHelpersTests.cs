using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using USD.NET.Unity;
using Scene = USD.NET.Scene;

namespace Unity.Formats.USD.Tests
{
    public class ImportHelpersTests : BaseFixture
    {
        Scene CreateTmpUsdWithData(string fileName)
        {
            var dummyUsdPath = CreateTmpUsdFile(fileName);
            var scene = ImportHelpers.InitForOpen(dummyUsdPath);
            scene.Write("/root", new XformSample());
            scene.Write("/root/sphere", new SphereSample());
            scene.Save();
            return scene;
        }

        [Test]
        public void InitForOpenTest_ValidPath_Succeeds()
        {
            var dummyUsdPath = CreateTmpUsdFile("dummyUsd.usda");
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
            var scene = CreateTmpUsdWithData("dummyUsd.usda");
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            bool usdRootIsRoot = Array.Find(SceneManager.GetActiveScene().GetRootGameObjects(), r => r == root);
            Assert.IsTrue(usdRootIsRoot, "UsdAsset GameObject is not a root GameObject.");
        }

        [Test]
        public void ImportAsGameObjects_ImportUnderParent()
        {
            var root = new GameObject("thisIsTheRoot");
            var scene = CreateTmpUsdWithData("dummyUsd.usda");
            var usdRoot = ImportHelpers.ImportSceneAsGameObject(scene, root);
            Assert.AreEqual(root.transform, usdRoot.transform.root, "UsdAsset is not a children of the given parent.");
        }

        [Test]
        public void ImportAsGameObjects_SceneClosedAfterImport()
        {
            var scene = CreateTmpUsdWithData("dummyUsd.usda");
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            Assert.IsNull(scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsGameObjects_ImportClosedScene_LogsError()
        {
            var rootGOsBefore = SceneManager.GetActiveScene().GetRootGameObjects();
            var scene = CreateTmpUsdWithData("dummyUsd.usda");
            scene.Close();
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            Assert.IsNull(root);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "The USD Scene needs to be opened before being imported.");
        }

        [Test]
        [Ignore("TODO")]
        public void ImportAsGameObjects_CleanupAfterError()
        {
        }
    }
}

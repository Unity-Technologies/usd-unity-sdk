using System.IO;
using NUnit.Framework;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class ExportHelpersTests : BaseFixture
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
            var filePath = System.IO.Path.Combine(Application.dataPath, "dummyUsd.usd");
            m_filesToDelete.Add(filePath);
            var scene = ExportHelpers.InitForSave(filePath);
            Assert.IsNotNull(scene);
            Assert.IsNotNull(scene.Stage);
            scene.Close();
        }

        [Test]
        public void ExportGameObjects_NullScene()
        {
            var filePath = CreateTmpUsdFile("dummyUsd.usda");
            var scene = Scene.Open(filePath);
            scene.Close();
            var fileInfoBefore = new FileInfo(filePath);

            Assert.DoesNotThrow(delegate()
            {
                ExportHelpers.ExportGameObjects(null, null, BasisTransformation.SlowAndSafe);
            });
            var fileInfoAfter = new FileInfo(filePath);
            Assert.AreEqual(fileInfoBefore.Length, fileInfoAfter.Length);
        }

        [Test]
        public void ExportGameObjects_EmptyList()
        {
            var filePath = CreateTmpUsdFile("dummyUsd.usda");
            var scene = Scene.Open(filePath);
            var fileInfoBefore = new FileInfo(filePath);
            Assert.DoesNotThrow(delegate()
            {
                ExportHelpers.ExportGameObjects(new GameObject [] {}, scene, BasisTransformation.SlowAndSafe);
            });
            var fileInfoAfter = new FileInfo(filePath);
            Assert.AreEqual(fileInfoBefore.Length, fileInfoAfter.Length);
            scene.Close();
        }

        [Test]
        public void ExportGameObjects_InvalidGO()
        {

            var filePath = CreateTmpUsdFile("dummyUsd.usda");
            var scene = Scene.Open(filePath);
            Assert.DoesNotThrow(delegate()
            {
                ExportHelpers.ExportGameObjects(new GameObject[] {null}, scene, BasisTransformation.SlowAndSafe);
            });
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, "NullReferenceException: Object reference not set to an instance of an object");
        }

        [Test]
        public void ExportGameObjects_ValidGO()
        {
            var filePath = CreateTmpUsdFile("dummyUsd.usda");
            var scene = Scene.Open(filePath);
            ExportHelpers.ExportGameObjects(new [] {new GameObject("test")}, scene, BasisTransformation.SlowAndSafe);
            scene = Scene.Open(filePath);
            var paths = scene.Stage.GetAllPaths();
            Debug.Log(scene.Stage.GetRootLayer().ExportToString());
            Assert.AreEqual(2, paths.Count);
            scene.Close();
        }

        [Test]
        public void ExportGameObjects_SceneClosedAfterExport()
        {

            var filePath = CreateTmpUsdFile("dummyUsd.usda");
            var scene = Scene.Open(filePath);
            ExportHelpers.ExportGameObjects(new [] {new GameObject("test")}, scene, BasisTransformation.SlowAndSafe);
            Assert.IsNull(scene.Stage);
        }
    }
}

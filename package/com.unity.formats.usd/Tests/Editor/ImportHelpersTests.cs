using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD.Tests
{
    public class ImportHelpersTests : BaseFixtureEditor
    {
        Scene m_scene;

        [SetUp]
        public void CreateTestScene()
        {
            m_scene = CreateTestUsdScene();
        }

        [Test]
        public void ImportAsPrefabTest_SceneClosedAfterImport()
        {
            ImportHelpers.ImportAsPrefab(m_scene, GetPrefabPath());
            Assert.IsNull(m_scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsTimelineClipTest_SceneClosedAfterImport()
        {
            ImportHelpers.ImportAsTimelineClip(m_scene, GetPrefabPath());
            Assert.IsNull(m_scene.Stage, "Scene was not closed after import.");
        }

        [Test]
        public void ImportAsPrefabTest_ImportClosedScene_Error()
        {
            m_scene.Close();
            Assert.Throws<System.NullReferenceException>(() => ImportHelpers.ImportAsPrefab(m_scene, GetPrefabPath()));
        }

        [Test]
        public void ImportAsTimelineClipTest_ImportClosedScene_Error()
        {
            m_scene.Close();
            Assert.Throws<System.NullReferenceException>(() => ImportHelpers.ImportAsTimelineClip(m_scene, GetPrefabPath()));
        }

        [Test]
        public void ImportAsPrefabTest_ContentOk()
        {
            var assetPath = ImportHelpers.ImportAsPrefab(m_scene, GetPrefabPath());

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
            var assetPath = ImportHelpers.ImportAsTimelineClip(m_scene, GetPrefabPath());

            Assert.IsTrue(File.Exists(assetPath));
            var usdAsObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // ExpectedGameObjectCount: The Root GameObject
            // ExpectedPrimSourceCount: 0 TODO: Shouldnt there be a prim source object for the root object?
            // ExpectedMaterialCount: The 3 default materials
            ImportAssert.Editor.IsValidImport(usdAsObjects, expectedGameObjectCount: 1, expectedPrimSourceCount: 0, expectedMaterialCount: 3);
        }

        [TestCase(TestAssetData.FileName.TexturedOpaque, Description = "Opaque Texture")]
        [TestCase(TestAssetData.FileName.TexturedTransparent_Cutout, Description = "Transparent Cutout Texture"), Ignore("[USDU-232] Test On HDRP")]
        public void ImportAsPrefab_TextureDataImported(string fileName)
        {
            var scene = ImportHelpers.InitForOpen(GetTestAssetPath(fileName));
            var assetPath = ImportHelpers.ImportAsPrefab
                (
                    scene,
                    new SceneImportOptions()
                    {
                        projectAssetPath = ImportHelpers.GetSelectedAssetPath(),
                        usdRootPath = ImportHelpers.GetDefaultRoot(scene),
                        materialImportMode = MaterialImportMode.ImportPreviewSurface
                    },
                    GetPrefabPath(resource: true)
                );

            Assert.IsTrue(File.Exists(assetPath));
            var prefabGameObject = (GameObject)Resources.Load(Path.GetFileNameWithoutExtension(assetPath), typeof(GameObject));
            ImportAssert.IsTextureDataSaved(prefabGameObject, fileName);
        }
    }
}

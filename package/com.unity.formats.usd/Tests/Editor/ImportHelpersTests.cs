using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;
using System.Linq;
using Object = UnityEngine.Object;
using System;

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
            EditorImportAssert.IsValidImport(usdAsObjects, expectedGameObjectCount: 2, expectedPrimSourceCount: 2, expectedMaterialCount: 4);
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
            EditorImportAssert.IsValidImport(usdAsObjects, expectedGameObjectCount: 1, expectedPrimSourceCount: 0, expectedMaterialCount: 3);
        }
    }

    public static class EditorImportAssert
    {
        enum ObjectTypeName
        {
            UsdPlayableAsset = 0,
            GameObject = 1,
            Material = 2,
            UsdPrimSource = 3
        }


        public static void IsValidImport(Object[] usdAsObjects, int expectedGameObjectCount, int expectedPrimSourceCount, int expectedMaterialCount)
        {
            Assert.NotZero(usdAsObjects.Length);

            bool playableAssetFound = false;
            int gameObjectCount = 0;
            int materialCount = 0;
            int usdPrimSourceCount = 0;

            foreach (Object childObject in usdAsObjects)
            {
                switch (childObject.GetType().Name)
                {
                    case nameof(ObjectTypeName.UsdPlayableAsset):
                        playableAssetFound = true;
                        break;

                    case nameof(ObjectTypeName.GameObject):
                        gameObjectCount++;
                        break;

                    case nameof(ObjectTypeName.Material):
                        materialCount++;
                        break;

                    case nameof(ObjectTypeName.UsdPrimSource):
                        usdPrimSourceCount++;
                        break;

                    default:
                        break;
                }
            }

            Assert.IsTrue(playableAssetFound, "No PlayableAssset was found in the prefab.");
            Assert.AreEqual(expectedGameObjectCount, gameObjectCount, "Wrong GameObjects count in the prefab.");
            Assert.AreEqual(expectedPrimSourceCount, usdPrimSourceCount, "Wrong USD Prim Source object count in the prefab");
            Assert.AreEqual(expectedMaterialCount, materialCount, "Wrong Materials count in the prefab");
        }
    }
}

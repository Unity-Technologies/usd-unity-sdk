using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;
using Object = UnityEngine.Object;

namespace Unity.Formats.USD.Tests
{
    public class ImportHelpersTests : BaseFixtureEditor
    {
        Scene CreateTestAsset(string fileName)
        {
            var dummyUsdPath = CreateTmpUsdFile(fileName);
            var scene = ImportHelpers.InitForOpen(dummyUsdPath);
            scene.Write("/root", new XformSample());
            scene.Write("/root/sphere", new SphereSample());
            scene.Save();
            return scene;
        }

        [Test]
        public void ImportAsPrefabTest_ContentOk()
        {
            var scene = CreateTestAsset("dummyUsd.usda");
            var assetPath = ImportHelpers.ImportAsPrefab(scene);
            Assert.IsNull(scene.Stage, "Scene was not closed after import.");

            m_assetsToDelete.Add(assetPath);

            Assert.IsTrue(File.Exists(assetPath));
            var allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            Assert.NotZero(allObjects.Length);
            bool playableAssetFound = false;
            int goCount = 0;
            int materialCount = 0;
            foreach (Object thisObject in allObjects)
            {
                string myType = thisObject.GetType().Name;
                if (myType == "UsdPlayableAsset")
                {
                    playableAssetFound = true;
                }
                else if (myType == "GameObject")
                {
                    goCount += 1;
                }
                else if (myType == "Material")
                {
                    materialCount += 1;
                }
            }

            Assert.IsTrue(playableAssetFound, "No PlayableAssset was found in the prefab.");
            Assert.AreEqual(2, goCount, "Wrong GameObjects count in the prefab.");
            // The 3 default materials + 1 material per meshRender
            Assert.AreEqual(4, materialCount, "Wrong Materials count in the prefab");
        }

        [Test]
        public void ImportAsTimelineClipTest_ContentOk()
        {
            // Import as timeline clip should not create a hierarchy, only the root and the playable
            var scene = CreateTestAsset("dummyUsd.usda");
            var assetPath = ImportHelpers.ImportAsTimelineClip(scene);
            Assert.IsNull(scene.Stage, "Scene was not closed after import.");
            m_assetsToDelete.Add(assetPath);

            Assert.IsTrue(File.Exists(assetPath));
            var allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            Assert.NotZero(allObjects.Length);
            bool playableAssetFound = false;
            int goCount = 0;
            int materialCount = 0;
            foreach (Object thisObject in allObjects)
            {
                Debug.Log(thisObject.name);
                Debug.Log(thisObject.GetType().Name);
                string myType = thisObject.GetType().Name;
                if (myType == "UsdPlayableAsset")
                {
                    playableAssetFound = true;
                }
                else if (myType == "GameObject")
                {
                    goCount += 1;
                }
                else if (myType == "Material")
                {
                    materialCount += 1;
                }
            }

            Assert.IsTrue(playableAssetFound, "No PlayableAssset was found in the prefab.");
            Assert.AreEqual(1, goCount, "Wrong GameObjects count in the prefab.");
            // Only 3 default materials and no meshRenderer
            Assert.AreEqual(3, materialCount, "Wrong Materials count in the prefab");

        }
    }
}

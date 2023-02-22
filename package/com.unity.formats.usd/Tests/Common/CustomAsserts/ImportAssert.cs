using UnityEngine;
using NUnit.Framework;

namespace Unity.Formats.USD.Tests
{
    public static class ImportAssert
    {
        public static class Editor
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
                Assert.AreEqual(expectedPrimSourceCount, usdPrimSourceCount, "Wrong USD Prim Source object in the prefab");
                Assert.AreEqual(expectedMaterialCount, materialCount, "Wrong Materials count in the prefab");
            }
        }

        public static void IsTextureDataSaved(GameObject usdObject, string fileName, bool isPrefab)
        {
            var materials = usdObject.transform.Find(TestAssetData.ImportGameObjectName.Material);
            var rootPrim = usdObject.transform.Find(TestAssetData.ImportGameObjectName.RootPrim);

            Assert.IsTrue(rootPrim.childCount == 1);
            Assert.AreEqual(materials.childCount, rootPrim.childCount);

            foreach (Transform child in materials)
            {
                // TODO: Not sure how to access "Prim Type"
                Assert.IsNotNull(child.GetComponent<UsdPrimSource>());
            }

            var renderer = rootPrim.Find(fileName).GetComponent<MeshRenderer>();

            Material[] allMaterials;
            if (isPrefab)
            {
                allMaterials = renderer.sharedMaterials;
            }
            else
            {
                allMaterials = renderer.materials;
            }

            foreach (var material in allMaterials)
            {
                IsTextureFileMapped(fileName, material);
            }
        }

        private static void IsTextureFileMapped(string fileName, Material material)
        {
            switch (fileName)
            {
                case TestAssetData.FileName.TexturedTransparent_Cutout:
                    {
                        Assert.AreEqual("Cutout", material.GetTag("RenderType", false));
                        Assert.AreEqual("textured_transparency", material.mainTexture.name);
                        Assert.AreEqual(1f, material.GetFloat("_Cutoff"));
                        break;
                    }
                case TestAssetData.FileName.TexturedOpaque:
                    {
                        Assert.AreEqual("Opaque", material.GetTag("RenderType", false));
                        Assert.AreEqual("textured", material.mainTexture.name);
                        break;
                    }

                default:
                    break;
            }

            Assert.AreEqual("textured_metallic.metalicRough", material.GetTexture("_MetallicGlossMap").name);
            Assert.AreEqual("textured_normal", material.GetTexture("_BumpMap").name);
            Assert.AreEqual("textured_emissive", material.GetTexture("_EmissionMap").name);
        }
    }
}

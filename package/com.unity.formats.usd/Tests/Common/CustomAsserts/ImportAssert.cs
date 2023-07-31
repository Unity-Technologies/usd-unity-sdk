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

            public static void IsValidGameObjectImport(GameObject rootObject, int expectedPrimSourceCount, int expectedMeshCount)
            {
                Assert.IsNotNull(rootObject.GetComponent<UsdAsset>());

                var actualPrimSourceCount = 1; // root has one
                var actualMeshCount = 0;

                foreach (Transform child in rootObject.transform)
                {
                    if (child.GetComponent<UsdPrimSource>() != null)
                    {
                        actualPrimSourceCount++;
                    }

                    if (child.GetComponent<MeshRenderer>() != null)
                    {
                        actualMeshCount++;
                    }
                }

                Assert.AreEqual(expectedPrimSourceCount, actualPrimSourceCount, "Expected PrimSource count does not match the actual PrimSource count.");
                Assert.AreEqual(expectedMeshCount, actualMeshCount, "Expected Mesh count does not match the actual Mesh count.");
            }

            public static void IsValidPrefabImport(Object[] usdAsObjects, int expectedGameObjectCount, int expectedPrimSourceCount, int expectedMaterialCount)
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
            var materials = usdObject.transform.Find("Material");
            var rootPrim = usdObject.transform.Find("RootPrim");

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
                case "TexturedTransparent_Cutout":
                    {
                        Assert.AreEqual("Cutout", material.GetTag("RenderType", false));
                        Assert.AreEqual("textured_transparency", material.mainTexture.name);
                        Assert.AreEqual(1f, material.GetFloat("_Cutoff"));
                        break;
                    }
                case "TexturedOpaque":
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

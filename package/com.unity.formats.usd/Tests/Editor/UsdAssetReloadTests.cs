using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD.Tests
{
    public class UsdAssetReloadTests : BaseFixtureEditor
    {
        const string k_USDGUID = "68d552f46d3740c47b17d0ac1c531e76";  // reloadTest.usda
        const string k_USDModifiedGUID = "4eccf405e5254fd4089cef2f9bcbd882"; // reloadTest_modified.usda
        const string k_USDOriginGUID = "069ae5d2d8a36fd4b8a0395de731eda0"; // reloadTest_origin.usda
        const string k_USDInstancerGUID = "bfb4012f0c339574296e64f4d3c6c595"; // instanced_cubes.usda

        string testFilePath;
        string testFileModifiedPath;

        GameObject m_usdRoot;
        UsdAsset m_usdAsset;

        [SetUp]
        public void Setup()
        {
            var scene = OpenUSDGUIDAssetScene(k_USDGUID, out testFilePath);
            m_usdRoot = ImportHelpers.ImportSceneAsGameObject(scene);
            scene.Close();

            m_usdAsset = m_usdRoot.GetComponent<UsdAsset>();
            m_usdAsset.Reload(true);
            Assume.That(m_usdAsset, Is.Not.Null, "Could not find USDAsset component on root gameobject.");
        }

        [TearDown]
        public void TearDown()
        {
            m_usdAsset.DestroyAllImportedObjects();
            GameObject.DestroyImmediate(m_usdRoot);

            // Restore test files.
            ResetTestFile();
        }

        [UnityTest]
        public IEnumerator UsdAsset_Reload_FileHasChanged_NewValuesAreRetrieved()
        {
            var xform = new XformSample();

            m_usdAsset.GetScene().Read("/TestPrim", xform);
            var translate = xform.transform.GetColumn(3);
            Assert.AreEqual(new Vector4(1, 1, 1, 1), translate);

            yield return new WaitForSecondsRealtime(1f);
            // Simulate the fact that the usd file was changed on disk.
            UpdateTestFile();

            // Refresh the asset.
            m_usdAsset.Reload(false);

            // Ensure the new attribute's values can be retrieved.
            m_usdAsset.GetScene().Read("/TestPrim", xform);
            translate = xform.transform.GetColumn(3);
            Assert.AreEqual(new Vector4(2, 2, 2, 1), translate);
            yield return new WaitForSecondsRealtime(1f);
        }

        [UnityTest]
        public IEnumerator UsdAsset_Reload_FileHasChangedAndForceRebuild_NewValuesAreRetrieved()
        {
            var xform = new XformSample();

            m_usdAsset.GetScene().Read("/TestPrim", xform);
            var translate = xform.transform.GetColumn(3);
            Assert.AreEqual(new Vector4(1, 1, 1, 1), translate);

            yield return new WaitForSecondsRealtime(1f);
            // Simulate the fact that the usd file was changed on disk.
            UpdateTestFile();

            // Reload the asset.
            m_usdAsset.Reload(true);

            // Ensure the new attribute's values can be retrieved.
            m_usdAsset.GetScene().Read("/TestPrim", xform);
            translate = xform.transform.GetColumn(3);
            Assert.AreEqual(new Vector4(2, 2, 2, 1), translate);
            yield return new WaitForSecondsRealtime(1f);
        }

        [Test]
        public void UsdAsset_Reload_FileDidNotChange_ValuesDoNotChange()
        {
            var xform = new XformSample();

            m_usdAsset.GetScene().Read("/TestPrim", xform);
            var translate = xform.transform.GetColumn(3);
            Assert.AreEqual(new Vector4(1, 1, 1, 1), translate);

            // Refresh the asset.
            m_usdAsset.Reload(false);

            // Ensure that nothing changed.
            m_usdAsset.GetScene().Read("/TestPrim", xform);
            translate = xform.transform.GetColumn(3);
            Assert.AreEqual(new Vector4(1, 1, 1, 1), translate);
        }

        [Test]
        public void UsdInstancerAsset_Reload_NoDuplicateObjects()
        {
            const int expectedChildrenCount = 101;
            const string instancerChildNamePrefix = "Root";

            var scene = OpenUSDGUIDAssetScene(k_USDInstancerGUID, out _);
            var instancerObject = ImportHelpers.ImportSceneAsGameObject(scene);

            var originalChildrenData = new Dictionary<string, int>();
            foreach (Transform child in instancerObject.transform)
            {
                originalChildrenData.Add(child.name, child.GetInstanceID());
            }

            instancerObject.GetComponent<UsdAsset>().Reload(false);

            var newChildrenCount = instancerObject.transform.childCount;
            Assert.AreEqual(newChildrenCount, expectedChildrenCount);

            foreach (Transform child in instancerObject.transform)
            {
                if (child.name.StartsWith(instancerChildNamePrefix))
                {
                    Assert.True(originalChildrenData.ContainsKey(child.name));
                    Assert.That(originalChildrenData[child.name] != child.GetInstanceID());
                }
            }
        }

        void UpdateTestFile()
        {
            testFileModifiedPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(k_USDModifiedGUID));

            File.WriteAllBytes(testFilePath, File.ReadAllBytes(testFileModifiedPath));
        }

        void ResetTestFile()
        {
            var originFile = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(k_USDOriginGUID));

            File.WriteAllBytes(testFilePath, File.ReadAllBytes(originFile));
        }
    }
}

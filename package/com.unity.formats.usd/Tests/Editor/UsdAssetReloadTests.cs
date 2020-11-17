using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD.Tests
{
    public class UsdAssetReloadTests
    {
        const string k_USDGUID = "68d552f46d3740c47b17d0ac1c531e76";  // reloadTest.usda
        const string k_USDModifiedGUID = "4eccf405e5254fd4089cef2f9bcbd882"; // reloadTest_modified.usda
        const string k_USDOriginGUID = "069ae5d2d8a36fd4b8a0395de731eda0"; // reloadTest_origin.usda

        string testFilePath;
        string testFileModifiedPath;
        
        GameObject m_usdRoot;
        UsdAsset m_usdAsset;

        [SetUp]
        public void Setup()
        {
            InitUsd.Initialize();
            testFilePath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(k_USDGUID));
            var stage = pxr.UsdStage.Open(testFilePath, pxr.UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            m_usdRoot = UsdMenu.ImportSceneAsGameObject(scene);
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
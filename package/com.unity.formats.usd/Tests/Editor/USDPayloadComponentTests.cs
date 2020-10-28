using UnityEngine;
using UnityEngine.TestTools;
using USD.NET;
using System.IO;
using UnityEditor;
using NUnit.Framework;
using System.Collections;

namespace Unity.Formats.USD.Tests
{
    public class USDPayloadComponentTests
    {
        const string k_USDGUID = "5f0268198d3d7484cb1877bec2c5d31f"; // GUID of test_collections.usda
 
        private GameObject m_usdRoot;
        private UsdAsset m_usdAsset;

        [SetUp]
        public void Setup()
        {
            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(k_USDGUID));
            var stage = pxr.UsdStage.Open(usdPath, pxr.UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            m_usdRoot = USD.UsdMenu.ImportSceneAsGameObject(scene);
            scene.Close();

            m_usdAsset = m_usdRoot.GetComponent<UsdAsset>();
            Assume.That(m_usdAsset, Is.Not.Null, "Could not find USDAsset component on root gameobject.");
        }

        [TearDown]
        public void TearDown()
        {
            m_usdAsset.DestroyAllImportedObjects();
            GameObject.DestroyImmediate(m_usdRoot);
        }

        [UnityTest]
        public IEnumerator IsLoaded_DefaultIsFalse_WhenPolicyIsSetToDontLoad()
        {
            // Prepare states
            m_usdAsset.m_payloadPolicy = PayloadPolicy.DontLoadPayloads;
            m_usdAsset.Reload(forceRebuild: true);

            yield return null;

            // Tests
            var usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assert.That(usdPayload.IsLoaded, Is.False, "UsdPayload.IsLoaded should be set to false.");
        }

        [UnityTest]
        public IEnumerator IsLoaded_DefaultIsTrue_WhenPolicyIsSetToLoadAll()
        {
            // Prepare states
            m_usdAsset.m_payloadPolicy = PayloadPolicy.LoadAll;
            m_usdAsset.Reload(forceRebuild: true);

            yield return null;

            // Tests
            var usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assert.That(usdPayload.IsLoaded, Is.True, "UsdPayload.IsLoaded should be set to false.");
        }

        [UnityTest]
        public IEnumerator IsLoaded_IsTrue_WhenLoadedFromComponent()
        {
            // Prepare states
            m_usdAsset.m_payloadPolicy = PayloadPolicy.DontLoadPayloads;
            m_usdAsset.Reload(forceRebuild: true);

            yield return null;

            var usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assume.That(usdPayload.IsLoaded, Is.False, "UsdPayload.IsLoaded should be set to false.");

            var usdPrimSource = usdPayload.GetComponent<UsdPrimSource>();
            Assume.That(usdPrimSource, Is.Not.Null, "Could not find USDPrimSource on UsdPayload gameobject.");

            // Tests
            usdPayload.Load();
            usdPayload.Update();

            yield return null;
            var payloadPrim = m_usdAsset.GetScene().GetPrimAtPath(usdPrimSource.m_usdPrimPath);

            Assert.That(payloadPrim.IsLoaded(), Is.True, "Payload should exist.");

            usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assert.That(usdPayload.IsLoaded, Is.True, "UsdPayload.IsLoaded should be set to true.");
        }

        [UnityTest]
        public IEnumerator IsLoaded_IsTrue_WhenSubtreeIsLoadedFromUSDMenu()
        {
            // Prepare states
            m_usdAsset.m_payloadPolicy = PayloadPolicy.DontLoadPayloads;
            m_usdAsset.Reload(forceRebuild: true);

            yield return null;

            var usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assume.That(usdPayload.IsLoaded, Is.False, "UsdPayload.IsLoaded should be set to false.");

            var usdPrimSource = usdPayload.GetComponent<UsdPrimSource>();
            Assume.That(usdPrimSource, Is.Not.Null, "Could not find USDPrimSource on UsdPayload gameobject.");

            Selection.activeGameObject = usdPayload.gameObject;

            yield return null;

            UsdMenu.MenuLoadSubtree();

            yield return null;
            // Tests
            var payloadPrim = m_usdAsset.GetScene().GetPrimAtPath(usdPrimSource.m_usdPrimPath);
            Assert.That(payloadPrim.IsLoaded(), Is.True, "Payload should exist.");

            usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assert.That(usdPayload.IsLoaded, Is.True, "UsdPayload.IsLoaded should be set to true.");
        }

        [UnityTest]
        public IEnumerator IsLoaded_IsFalse_WhenSubtreeIsUnloadedFromUSDMenu()
        {
            // Prepare states
            m_usdAsset.m_payloadPolicy = PayloadPolicy.LoadAll;
            m_usdAsset.Reload(forceRebuild: true);

            yield return null;

            var usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assume.That(usdPayload.IsLoaded, Is.True, "UsdPayload.IsLoaded should be set to false.");

            var usdPrimSource = usdPayload.GetComponent<UsdPrimSource>();
            Assume.That(usdPrimSource, Is.Not.Null, "Could not find USDPrimSource on UsdPayload gameobject.");

            Selection.activeGameObject = usdPayload.gameObject;

            yield return null;

            UsdMenu.MenuUnloadSubtree();

            yield return null;
            // Tests
            var payloadPrim = m_usdAsset.GetScene().GetPrimAtPath(usdPrimSource.m_usdPrimPath);
            Assert.That(payloadPrim.IsLoaded(), Is.False, "Payload should not exist.");

            usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assume.That(usdPayload.IsLoaded, Is.False, "UsdPayload.IsLoaded should be set to false.");
            // Clean up
            m_usdAsset.DestroyAllImportedObjects();

            yield return null;
        }

        [UnityTest]
        public IEnumerator IsLoaded_IsFalse_WhenIsUnloadedFromComponent()
        {
            // Prepare states
            m_usdAsset.m_payloadPolicy = PayloadPolicy.LoadAll;
            m_usdAsset.Reload(forceRebuild: true);

            yield return null;

            var usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assume.That(usdPayload.IsLoaded, Is.True, "UsdPayload.IsLoaded should be set to false.");

            var usdPrimSource = usdPayload.GetComponent<UsdPrimSource>();
            Assume.That(usdPrimSource, Is.Not.Null, "Could not find USDPrimSource on UsdPayload gameobject.");

            usdPayload.Unload();
            usdPayload.Update();

            yield return null;
            // Tests
            var payloadPrim = m_usdAsset.GetScene().GetPrimAtPath(usdPrimSource.m_usdPrimPath);
            Assert.That(payloadPrim.IsLoaded(), Is.False, "Payload should not exist.");

            usdPayload = m_usdRoot.GetComponentInChildren<UsdPayload>();
            Assume.That(usdPayload, Is.Not.Null, "Could not find USDPayload in the subtree.");
            Assume.That(usdPayload.IsLoaded, Is.False, "UsdPayload.IsLoaded should be set to false.");
        }
    }
}
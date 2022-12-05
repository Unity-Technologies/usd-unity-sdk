// Copyright 2022 Unity Technologies. All rights reserved.
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
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using USD.NET;
using System.IO;
using UnityEditor;
using NUnit.Framework;
using System.Collections;

namespace Unity.Formats.USD.Tests
{
    public class USDPayloadComponentTests : CleanTestEnvironment
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
            m_usdRoot = ImportHelpers.ImportSceneAsGameObject(scene);
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

        [UnityTest]
        public IEnumerator ChangingUsdPayloadStateFromUnloadedToLoaded_MarksSceneDirty()
        {
            m_usdAsset.m_payloadPolicy = PayloadPolicy.DontLoadPayloads;
            m_usdAsset.Reload(forceRebuild: true);
            yield return null;

            EditorSceneManager.SaveScene(m_UnityScene, GetUnityScenePath("PayloadTests"));
            Assert.IsFalse(m_UnityScene.isDirty, "Scene should not be dirty after after saving");

            // Change payload state and wait one frame to ensure Update runs
            m_usdRoot.GetComponentInChildren<UsdPayload>().Load();
            yield return null;

            Assert.IsTrue(m_UnityScene.isDirty, "Scene should be dirty after changing UsdPayload objects");
        }

        [UnityTest]
        public IEnumerator ChangingUsdPayloadStateFromLoadedToUnloaded_MarksSceneDirty()
        {
            m_usdAsset.m_payloadPolicy = PayloadPolicy.LoadAll;
            m_usdAsset.Reload(forceRebuild: true);
            yield return null;

            EditorSceneManager.SaveScene(m_UnityScene, GetUnityScenePath("PayloadTests"));
            Assert.IsFalse(m_UnityScene.isDirty, "Scene should not be dirty after after saving");

            // Change payload state and wait one frame to ensure Update runs
            m_usdRoot.GetComponentInChildren<UsdPayload>().Unload();
            yield return null;

            Assert.IsTrue(m_UnityScene.isDirty, "Scene should be dirty after changing UsdPayload objects");
        }
    }
}

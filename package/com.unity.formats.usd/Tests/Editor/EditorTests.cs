using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEngine;
using Scene = USD.NET.Scene;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Formats.USD.Tests
{
    public class FBXHandednessModeTests
    {
        const string fbxGUID = "86a597c63449d2541b7587ff90e75d91"; // GUID of withCamera.fbx
        const string usdGUID = "f377c4260fb216d4dbe2f6e4d67091b5"; // GUID of withCamera.usd

        private GameObject fbxRoot;
        private GameObject usdRoot;
        
        [SetUp]
        public void SetUp()
        {
            var fbxPath = AssetDatabase.GUIDToAssetPath(fbxGUID);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            fbxRoot = PrefabUtility.InstantiatePrefab(asset) as GameObject;

            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(usdGUID));
            var stage = pxr.UsdStage.Open(usdPath, pxr.UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            var importOptions = new SceneImportOptions();
            importOptions.changeHandedness = BasisTransformation.SlowAndSafeAsFBX;
            importOptions.scale = 0.01f;
            importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
            usdRoot = USD.UsdMenu.ImportSceneAsGameObject(scene, importOptions);
            scene.Close();
        }

        [Test]
        public void LoadAsFbxCompareCameraTransforms()
        {
            // Compare camera transforms
            var usdCamTr = usdRoot.transform.Find("group2/camera1");
            var fbxCamTr = fbxRoot.transform.Find("camera1");

            Debug.Log("Comparing camera positions...");
            Assert.AreApproximatelyEqual(usdCamTr.position.x,fbxCamTr.position.x);
            Assert.AreApproximatelyEqual(usdCamTr.position.y,fbxCamTr.position.y);
            Assert.AreApproximatelyEqual(usdCamTr.position.z,fbxCamTr.position.z);
            Debug.Log("Comparing camera rotations...");
            Assert.AreApproximatelyEqual(usdCamTr.localRotation.x,fbxCamTr.localRotation.x);
            Assert.AreApproximatelyEqual(usdCamTr.localRotation.y,fbxCamTr.localRotation.y);
            Assert.AreApproximatelyEqual(usdCamTr.localRotation.z,fbxCamTr.localRotation.z);
        }
        
        [Test]
        public void LoadAsFbxCompareMeshTransforms()
        {
            // Compare camera transforms
            var usdMeshTr = usdRoot.transform.Find("group2/group1/pCube1");
            var fbxMeshTr = fbxRoot.transform.Find("group1/pCube1");

            Debug.Log("Comparing mesh positions...");
            Assert.AreApproximatelyEqual(usdMeshTr.position.x,fbxMeshTr.position.x);
            Assert.AreApproximatelyEqual(usdMeshTr.position.y,fbxMeshTr.position.y);
            Assert.AreApproximatelyEqual(usdMeshTr.position.z,fbxMeshTr.position.z);
            Debug.Log("Comparing mesh rotations...");
            Assert.AreApproximatelyEqual(usdMeshTr.localRotation.x,fbxMeshTr.localRotation.x);
            Assert.AreApproximatelyEqual(usdMeshTr.localRotation.y,fbxMeshTr.localRotation.y);
            Assert.AreApproximatelyEqual(usdMeshTr.localRotation.z,fbxMeshTr.localRotation.z);
        }
    }
    
    public class UsdPrimTypeTest_Scope
    {
        private GameObject m_usdRoot;
        private string m_usdGUID = "5f0268198d3d7484cb1877bec2c5d31f"; // GUI of test_collections.usda
        
        [SetUp]
        public void SetUp()
        {
            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(m_usdGUID));
            var stage = pxr.UsdStage.Open(usdPath, pxr.UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            m_usdRoot = USD.UsdMenu.ImportSceneAsGameObject(scene);
            scene.Close();
        }

        [Test]
        public void ScopeWithoutChildrenExists()
        {
            var scopeWithoutChildren = m_usdRoot.transform.Find("TestComponent/geom");
            Assert.IsNotNull(scopeWithoutChildren);
            Assert.AreEqual(0, scopeWithoutChildren.childCount);
        }
        
        [Test]
        public void ScopeWithChildrenExists()
        {
            var scopeWithChildren = m_usdRoot.transform.Find("TestComponent/ScopeTest");
            Assert.IsNotNull(scopeWithChildren);
            Assert.AreNotEqual(0, scopeWithChildren.childCount);
        }

        [Test]
        public void ScopeHasPrimSourceComponent() {
            var scope = m_usdRoot.transform.Find("TestComponent/ScopeTest");
            var primSourceComponent = scope.GetComponent<UsdPrimSource>();
            Assert.IsNotNull(primSourceComponent);
            var scene = m_usdRoot.GetComponent<UsdAsset>().GetScene();
            var prim = scene.GetPrimAtPath(primSourceComponent.m_usdPrimPath);
            Assert.IsNotNull(prim);
            Assert.IsTrue("Scope" == prim.GetTypeName(), scope.name + " is not of type Scope: " + prim.GetTypeName());
        }
        
        [Test]
        public void ScopeTransformIsIdentity() {
            var scope = m_usdRoot.transform.Find("TestComponent/ScopeTest");
            Assert.IsTrue(Matrix4x4.identity == scope.transform.localToWorldMatrix);
        }
    }

    public class UsdMaterialTest
    {
        private GameObject m_usdRoot;
        private string m_usdGUID = "c06c7eba08022b74ca49dce5f79ef3ba"; // GUI of simpleMaterialTest.usd

        [SetUp]
        public void SetUp()
        {
            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(m_usdGUID));
            var stage = pxr.UsdStage.Open(usdPath, pxr.UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            var importOptions = new SceneImportOptions();
            importOptions.materialImportMode = MaterialImportMode.ImportPreviewSurface;
            m_usdRoot = USD.UsdMenu.ImportSceneAsGameObject(scene, importOptions);
            scene.Close();
        }

        [Test]
        public void TestMaterialNameSetOnImport()
        {
            // Check that the material name is the same after import as in the USD file
            Assert.IsNotNull(m_usdRoot);
            var cube = m_usdRoot.transform.Find("pCube1");
            Assert.IsNotNull(cube);
            var renderer = cube.GetComponent<Renderer>();
            Assert.IsNotNull(renderer);

            var material = renderer.sharedMaterial;
            Assert.IsNotNull(material);
            Assert.IsTrue(material.name == "lambert3SG");
        }
    }
}


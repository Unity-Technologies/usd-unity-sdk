using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEngine;
using Scene = USD.NET.Scene;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Formats.USD.Tests
{
    public class FBXHandednessModeTests : ScriptableObject
    {
        // Cannot store FBX as Object that can be set, as the fileID (stored in the meta file of this script), changes
        // depending on the Unity version.
        const string withCameraFbxGUID = "86a597c63449d2541b7587ff90e75d91"; // GUID of withCamera.fbx
        public Object withCameraUsd;
        public Object leftHandedWithCameraUsd;
        public Object bakedLeftHandedCube;
        
        private GameObject fbxRoot;
        private GameObject usdRoot;

        private GameObject LoadUSD(Object usdObject, BasisTransformation changeHandedness = BasisTransformation.SlowAndSafeAsFBX)
        {
            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GetAssetPath(usdObject));
            var stage = pxr.UsdStage.Open(usdPath, pxr.UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            var importOptions = new SceneImportOptions();
            importOptions.changeHandedness = changeHandedness;
            importOptions.scale = 0.01f;
            importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
            var usdRoot = USD.UsdMenu.ImportSceneAsGameObject(scene, importOptions);
            scene.Close();
            return usdRoot;
        }

        [SetUp]
        public void SetUp()
        {
            var fbxPath = AssetDatabase.GUIDToAssetPath(withCameraFbxGUID);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            fbxRoot = PrefabUtility.InstantiatePrefab(asset) as GameObject;

            usdRoot = LoadUSD(withCameraUsd);
        }

        [Test]
        public void LoadAsFbxCompareCameraTransforms()
        {
            // Compare camera transforms
            var usdCamTr = usdRoot.transform.Find("group2/camera1");
            var fbxCamTr = fbxRoot.transform.Find("camera1");

            Debug.Log("Comparing camera positions...");
            Assert.AreApproximatelyEqual(usdCamTr.position.x, fbxCamTr.position.x);
            Assert.AreApproximatelyEqual(usdCamTr.position.y, fbxCamTr.position.y);
            Assert.AreApproximatelyEqual(usdCamTr.position.z, fbxCamTr.position.z);
            Debug.Log("Comparing camera rotations...");
            Assert.AreApproximatelyEqual(usdCamTr.localRotation.x, fbxCamTr.localRotation.x);
            Assert.AreApproximatelyEqual(usdCamTr.localRotation.y, fbxCamTr.localRotation.y);
            Assert.AreApproximatelyEqual(usdCamTr.localRotation.z, fbxCamTr.localRotation.z);
        }

        [Test]
        public void LoadAsFbxCompareMeshTransforms()
        {
            // Compare camera transforms
            var usdMeshTr = usdRoot.transform.Find("group2/group1/pCube1");
            var fbxMeshTr = fbxRoot.transform.Find("group1/pCube1");

            Debug.Log("Comparing mesh positions...");
            Assert.AreApproximatelyEqual(usdMeshTr.position.x, fbxMeshTr.position.x);
            Assert.AreApproximatelyEqual(usdMeshTr.position.y, fbxMeshTr.position.y);
            Assert.AreApproximatelyEqual(usdMeshTr.position.z, fbxMeshTr.position.z);
            Debug.Log("Comparing mesh rotations...");
            Assert.AreApproximatelyEqual(usdMeshTr.localRotation.x, fbxMeshTr.localRotation.x);
            Assert.AreApproximatelyEqual(usdMeshTr.localRotation.y, fbxMeshTr.localRotation.y);
            Assert.AreApproximatelyEqual(usdMeshTr.localRotation.z, fbxMeshTr.localRotation.z);
        }

        [Test]
        public void TestLeftHandedUsdMeshImport()
        {
            // Compare import of Left handed USD to right handed USD
            var leftHandedUsdRoot = LoadUSD(leftHandedWithCameraUsd);
            Assert.IsNotNull(leftHandedUsdRoot);

            // check that the mesh does not match the right handed one
            var usdCube = usdRoot.transform.Find("group2/group1/pCube1");
            Assert.IsNotNull(usdCube);
            var leftHandedUsdCube = leftHandedUsdRoot.transform.Find("group2/group1/pCube1");
            Assert.IsNotNull(leftHandedUsdCube);

            var cubeMesh = usdCube.GetComponent<MeshFilter>().sharedMesh;
            var leftHandedCubeMesh = leftHandedUsdCube.GetComponent<MeshFilter>().sharedMesh;

            // Since the two files are different handedness, the vertices, triangles, and normals
            // will be different.
            NUnit.Framework.Assert.That(cubeMesh.vertices, Is.Not.EqualTo(leftHandedCubeMesh.vertices));
            NUnit.Framework.Assert.That(cubeMesh.triangles, Is.Not.EqualTo(leftHandedCubeMesh.triangles));
            NUnit.Framework.Assert.That(cubeMesh.normals, Is.Not.EqualTo(leftHandedCubeMesh.normals));

            // Check that the imported left handed cube matches the baked cube.
            var bakedCubeMesh = bakedLeftHandedCube as Mesh;
            Assert.IsNotNull(bakedCubeMesh);

            NUnit.Framework.Assert.That(bakedCubeMesh.vertices, Is.EqualTo(leftHandedCubeMesh.vertices));
            NUnit.Framework.Assert.That(bakedCubeMesh.triangles, Is.EqualTo(leftHandedCubeMesh.triangles));
            NUnit.Framework.Assert.That(bakedCubeMesh.normals, Is.EqualTo(leftHandedCubeMesh.normals));
        }
    }
}
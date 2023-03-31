using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using pxr;
using Unity.Formats.USD;
using UnityEditor;
using UnityEngine;
using USD.NET.Tests;

namespace USD.NET.Unity.Tests
{
    class InstancingTests : UsdTests
    {
        Scene CreatePointInstancerScene()
        {
            var tmpScenePath = CreateTmpUsdFile("PointInstancerTest.usda");
            var scene = Scene.Open(tmpScenePath);
            var pi = new PointInstancerSample();
            var cube = new CubeSample();

            pi.prototypes.targetPaths = new[] { "/Instancer/Cube" };

            // Three instances, all prototype index zero.
            pi.protoIndices = new int[3];
            pi.positions = new Vector3[3];
            pi.positions[0] = new Vector3(0, 0, 0);
            pi.positions[1] = new Vector3(2, 0, 0);
            pi.positions[2] = new Vector3(4, 0, 0);

            scene.Write("/Instancer", pi);
            scene.Write("/Instancer/Cube", cube);

            scene.Save();
            return scene;
        }

        Scene CreateMeshInstanceScene()
        {
            var cube = new CubeSample();
            var xform = new XformSample();

            var cubeScenePath = CreateTmpUsdFile("cube.usda");
            var scene = Scene.Open(cubeScenePath);
            scene.Write("/GEO", xform);
            scene.Write("/GEO/Cube", cube);
            scene.Save();
            scene.Close();

            var tmpScenePath = CreateTmpUsdFile("InstanceTraversalTest.usda");
            scene = Scene.Open(tmpScenePath);
            scene.Write("/InstancedCube1", xform);
            scene.Write("/InstancedCube1/cube", cube);
            scene.Write("/InstancedCube2", xform);
            scene.Write("/InstancedCube2/cube", cube);

            var prim = scene.GetPrimAtPath("/InstancedCube1/cube");
            prim.GetReferences().AddReference(cubeScenePath, new SdfPath("/GEO/Cube"));
            prim.SetInstanceable(true);
            prim = scene.GetPrimAtPath("/InstancedCube2/cube");
            prim.GetReferences().AddReference(cubeScenePath, new SdfPath("/GEO/Cube"));
            prim.SetInstanceable(true);
            scene.Save();
            return scene;
        }

        [Test]
        public void TraversalTest()
        {
            var scene = CreateMeshInstanceScene();
            foreach (UsdPrim prim in scene.Stage.Traverse(new Usd_PrimFlagsPredicate(Usd_PrimFlags.Usd_PrimInstanceFlag)))
            {
                Assert.IsTrue(prim.IsInstance(), "Prim {0} is not an instance.", prim.GetPath());
                var master = prim.GetMaster();
                Assert.AreEqual("/GEO/Cube", master.GetPath());
                Assert.AreEqual(2, master.GetAllDescendants().Count());
            }

            scene.Close();
        }

        [Test]
        public void PointInstancerTest()
        {
            var scene = CreatePointInstancerScene();
            var piSample = new PointInstancerSample();
            scene.Read("/Instancer", piSample);

            Assert.AreEqual(3, piSample.protoIndices.Length);
            Assert.AreEqual(1, piSample.prototypes.targetPaths.Length);
            Assert.AreEqual("/Instancer/Cube", piSample.prototypes.targetPaths[0]);

            var matrices = piSample.ComputeInstanceMatrices(scene, "/Instancer");
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(Matrix4x4.Translate(new Vector3(0 + i * 2, 0, 0)), matrices[i]);
            }
            scene.Close();
        }

        [Test]
        [Ignore("TODO: Enable this when fix for this has been applied")]
        public void InstancerImport_LeftHandedAxis_IsApplied()
        {
            const string testAssetGUID = "8ef366f1b52222347800237d47c9b0e0";

            var testScenePath = AssetDatabase.GUIDToAssetPath(testAssetGUID);
            var testScene = ImportHelpers.InitForOpen(Path.GetFullPath(testScenePath));

            var testInstanceObjectRoot = ImportHelpers.ImportSceneAsGameObject(testScene).transform.GetChild(0);

            var testObjectCenter = testInstanceObjectRoot.transform.GetChild(1).GetChild(0); // axisTest_instanced > instancers > axisTest_0 > Center
            var testObjectLegX = testInstanceObjectRoot.transform.GetChild(1).GetChild(1).GetChild(0); // axisTest_instanced > instancers > axisTest_0 > XPart > XLeg
            var testObjectLegY = testInstanceObjectRoot.transform.GetChild(1).GetChild(2).GetChild(0); // axisTest_instanced > instancers > axisTest_0 > YPart > YLeg
            var testObjectLegZ = testInstanceObjectRoot.transform.GetChild(1).GetChild(3).GetChild(0); // axisTest_instanced > instancers > axisTest_0 > ZPart > ZLeg

            var centerCollider = testObjectCenter.gameObject.AddComponent<MeshCollider>();
            centerCollider.sharedMesh = testObjectCenter.GetComponent<MeshFilter>().mesh;

            var legXCollider = testObjectLegX.gameObject.AddComponent<MeshCollider>();
            legXCollider.sharedMesh = testObjectLegX.GetComponent<MeshFilter>().mesh;

            var legYCollider = testObjectLegY.gameObject.AddComponent<MeshCollider>();
            legYCollider.sharedMesh = testObjectLegY.GetComponent<MeshFilter>().mesh;

            var legZCollider = testObjectLegZ.gameObject.AddComponent<MeshCollider>();
            legZCollider.sharedMesh = testObjectLegZ.GetComponent<MeshFilter>().mesh;

            centerCollider.convex = true; // Physics.ComputePenetration does not work if both colliders are concave

            // Control Test
            var controlResultX = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legXCollider, testObjectLegX.position, testObjectLegX.rotation, out _, out _));
            var controlResultY = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legYCollider, testObjectLegY.position, testObjectLegY.rotation, out _, out _));
            var controlResultZ = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legZCollider, testObjectLegZ.position, testObjectLegZ.rotation, out _, out _));

            Assert.False(controlResultX, "The X Direction Leg should not be overlapping with the center block");
            Assert.False(controlResultY, "The Y Direction Leg should not be overlapping with the center block");
            Assert.False(controlResultZ, "The Z Direction Leg should not be overlapping with the center block");

            // Overlap Test
            var overlapAmount = 1;
            testObjectLegX.transform.position = new Vector3(-overlapAmount, 0, 0);
            testObjectLegY.transform.position = new Vector3(0, -overlapAmount, 0);
            testObjectLegZ.transform.position = new Vector3(0, 0, -overlapAmount);

            var overlapResultX = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legXCollider, testObjectLegX.position, testObjectLegX.rotation, out _, out _));
            var overlapResultY = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legYCollider, testObjectLegY.position, testObjectLegY.rotation, out _, out _));
            var overlapResultZ = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legZCollider, testObjectLegZ.position, testObjectLegZ.rotation, out _, out _));

            Assert.True(overlapResultY, $"The Y Direction Leg should be overlapping with the center block with overlap distance {overlapAmount}");
            Assert.True(overlapResultX, $"The X Direction Leg should be overlapping with the center block with overlap distance {overlapAmount}");
            Assert.True(overlapResultZ, $"The Z Direction Leg should be overlapping with the center block with overlap distance {overlapAmount}");

            // Distanced Test
            testObjectLegX.transform.position = new Vector3(1, 0, 0);
            testObjectLegY.transform.position = new Vector3(0, 1, 0);
            testObjectLegZ.transform.position = new Vector3(0, 0, 1);

            var distancedResultX = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legXCollider, testObjectLegX.position, testObjectLegX.rotation, out _, out _));
            var distancedResultY = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legYCollider, testObjectLegY.position, testObjectLegY.rotation, out _, out _));
            var distancedResultZ = (Physics.ComputePenetration(centerCollider, testObjectCenter.position, testObjectCenter.rotation, legZCollider, testObjectLegZ.position, testObjectLegZ.rotation, out _, out _));

            Assert.False(distancedResultX, "The X Direction Leg should not be overlapping with the center block");
            Assert.False(distancedResultY, "The Y Direction Leg should not be overlapping with the center block");
            Assert.False(distancedResultZ, "The Z Direction Leg should not be overlapping with the center block");
        }
    }
}

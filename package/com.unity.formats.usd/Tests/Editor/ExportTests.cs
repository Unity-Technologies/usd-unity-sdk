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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD.Tests
{
    public class USDExportTests : BaseFixtureEditor
    {
        private Scene m_USDScene;
        private string m_USDScenePath;

        [SetUp]
        public void SetUp()
        {
            m_USDScenePath = GetUSDScenePath("USDExportTests");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_USDScene != null)
            {
                m_USDScene.Close();
            }
        }

        private pxr.UsdPrim GetPrim(GameObject gameObject)
        {
            Assert.IsNotNull(m_USDScene);
            Assert.IsNotNull(m_USDScene.Stage);

            return m_USDScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(gameObject.transform)));
        }

        [Test]
        public void ExportRootGameObjectWithMesh_ExportedPrimHasMeshType()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ExportHelpers.ExportGameObjects(new[] { cube }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);

            var cubePrim = GetPrim(cube);
            Assert.IsNotNull(cubePrim);
            Assert.IsTrue(cubePrim.IsValid());

            var cubeType = cubePrim.GetTypeName();
            var expectedCubeType = new pxr.TfToken("Mesh");
            Assert.AreEqual(expectedCubeType, cubeType);
        }

        [Test]
        public void ExportMultipleRootGameObjectsWithSameName_AllGameObjectsHaveCorrespondingPrims()
        {
            var cubes = new GameObject[] { GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube) };
            cubes[0].name = "Cube_1"; // Name deduplication appends _GetSiblingIndex() and in this case cubes[1] needs 2 iterations
            cubes[1].name = "Cube";
            cubes[2].name = "Cube";

            ExportHelpers.ExportGameObjects(cubes, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);
            var exportedPrims = new HashSet<pxr.UsdPrim>();
            foreach (GameObject cube in cubes)
            {
                var cubePrim = GetPrim(cube);
                Assert.IsNotNull(cubePrim, $"GameObject {cube.name} doesn't have a corresponding Prim");
                Assert.IsTrue(cubePrim.IsValid(), $"GameObject {cube.name} has invalid corresponding Prim");

                exportedPrims.Add(cubePrim);
            }
            Assert.AreEqual(cubes.Length, exportedPrims.Count, "One or more GameObjects don't have a corresponding Prim");
        }

        [Test]
        public void ExportMultipleSiblingGameObjectsWithSameName_AllGameObjectsHaveCorrespondingPrims()
        {
            var cubes = new GameObject[] { GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube) };
            cubes[0].name = "Cube_1"; // Name deduplication appends _GetSiblingIndex() and in this case cubes[1] needs 2 iterations
            cubes[1].name = "Cube";
            cubes[2].name = "Cube";

            var parent = new GameObject("parent");
            foreach (var cube in cubes)
            {
                cube.transform.parent = parent.transform;
            }

            ExportHelpers.ExportGameObjects(new GameObject[] { parent }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);
            var exportedPrims = new HashSet<pxr.UsdPrim>();
            foreach (GameObject cube in cubes)
            {
                var cubePrim = GetPrim(cube);
                Assert.IsNotNull(cubePrim, $"GameObject {cube.name} doesn't have a corresponding Prim");
                Assert.IsTrue(cubePrim.IsValid(), $"GameObject {cube.name} has invalid corresponding Prim");

                exportedPrims.Add(cubePrim);
            }
            Assert.AreEqual(cubes.Length, exportedPrims.Count, "One or more GameObjects don't have a corresponding Prim");
        }

        [Test]
        [Ignore("USDU-292")]
        public void ExportPhysicalCamera_RetainsPhysicalRelatedData()
        {
            var physicalPropertyNames = new List<string> { "focalLength", "sensorSize", "lensShift", "gateFit" };

            var testFocalLength = 100;
            var testSensorSize = new Vector2() { x = 10, y = 10 };
            var testLensShift = new Vector2() { x = 1, y = 1 };
            var testGateFit = Camera.GateFitMode.Overscan;

            var cameraObject = new GameObject("CameraContainer");
            var camera = cameraObject.AddComponent<Camera>();
            camera.usePhysicalProperties = true;
            camera.focalLength = testFocalLength;
            camera.sensorSize = testSensorSize;
            camera.lensShift = testLensShift;
            camera.gateFit = testGateFit;

            ExportHelpers.ExportGameObjects(new GameObject[] { camera.gameObject }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);
            m_USDScene = Scene.Open(m_USDScenePath);

            var cameraPrim = GetPrim(camera.gameObject);

            Assert.AreEqual(cameraPrim.GetTypeName().ToString(), "Camera");

            foreach (var camProperty in cameraPrim.GetProperties())
            {
                physicalPropertyNames.Remove(camProperty.GetBaseName());
            }

            Assert.IsEmpty(physicalPropertyNames, string.Format("Not all Physical Camera properties were found: {0}", String.Join(", ", physicalPropertyNames.ToArray())));
            // TODO: Not sure how to check for property value, would like to do Assertion on value as well if property has been found
        }
    }
}

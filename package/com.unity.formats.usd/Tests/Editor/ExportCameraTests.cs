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

using NUnit.Framework;
using Unity.Formats.USD;
using Unity.Formats.USD.Tests;
using UnityEngine;
using USD.NET;

class CameraRelated : BaseFixtureEditor
{
    private string m_USDScenePath;

    [SetUp]
    public void SetUp()
    {
        m_USDScenePath = TestUtility.GetUSDScenePath(ArtifactsDirectoryFullPath, "USDExportTests");
    }

    class PhysicalCameraMembers : SampleBase
    {
        public float focalLength;
        public float horizontalAperture;
        public float horizontalApertureOffset;
        public float verticalAperture;
        public float verticalApertureOffset;
    }

    [Test]
    [Ignore("USDU-292")]
    public void ExportPhysicalCamera_RetainsPhysicalRelatedData()
    {
        var testFocalLength = 75;
        var testSensorSize = new Vector2() { x = 30, y = 20 };
        var testLensShift = new Vector2() { x = 1, y = 2 };

        var cameraObject = new GameObject("CameraContainer");
        var camera = cameraObject.AddComponent<Camera>();
        camera.usePhysicalProperties = true;
        camera.focalLength = testFocalLength;
        camera.sensorSize = testSensorSize;
        camera.lensShift = testLensShift;

        ExportHelpers.ExportGameObjects(new GameObject[] { camera.gameObject }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);
        var cameraUsdScene = Scene.Open(m_USDScenePath);
        var physicalCameraData = new PhysicalCameraMembers();

        cameraUsdScene.Read("/CameraContainer", physicalCameraData);
        var cameraPrim = TestUtility.GetGameObjectPrimInScene(cameraUsdScene, camera.gameObject);
        cameraUsdScene.Close();

        Assert.AreEqual(cameraPrim.GetTypeName().ToString(), "Camera");
        Assert.AreEqual(testFocalLength, physicalCameraData.focalLength);
        Assert.AreEqual(testSensorSize.x, physicalCameraData.horizontalAperture);
        Assert.AreEqual(testLensShift.x, physicalCameraData.horizontalApertureOffset);
        Assert.AreEqual(testSensorSize.y, physicalCameraData.verticalAperture);
        Assert.AreEqual(testLensShift.y, physicalCameraData.verticalApertureOffset);
    }
}

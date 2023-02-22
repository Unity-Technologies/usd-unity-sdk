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
        m_USDScenePath = GetUSDScenePath("USDExportTests");
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
        var cameraPrim = TestUtilityFunction.GetGameObjectPrimInScene(cameraUsdScene, camera.gameObject);
        cameraUsdScene.Close();

        Assert.AreEqual(cameraPrim.GetTypeName().ToString(), "Camera");
        Assert.AreEqual(testFocalLength, physicalCameraData.focalLength);
        Assert.AreEqual(testSensorSize.x, physicalCameraData.horizontalAperture);
        Assert.AreEqual(testLensShift.x, physicalCameraData.horizontalApertureOffset);
        Assert.AreEqual(testSensorSize.y, physicalCameraData.verticalAperture);
        Assert.AreEqual(testLensShift.y, physicalCameraData.verticalApertureOffset);
    }
}

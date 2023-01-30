using System.IO;
using NUnit.Framework;
using pxr;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class ImportTests : BaseFixtureEditor
    {
        public class CameraRelated : ImportTests
        {
            string k_testCameraGUID = "6aa58f080f5cc0542989c8ff7737bdc3"; // physicalCam.usda
            Camera m_testCamera;

            [SetUp]
            public void SetUp()
            {
                var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(k_testCameraGUID));
                var stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadNone);
                var scene = Scene.Open(stage);
                m_testCamera = ImportHelpers.ImportSceneAsGameObject(scene).transform.GetChild(0).GetComponent<Camera>();
                scene.Close();
            }

            [Test]
            [Ignore("USDU-292")]
            public void ImportPhysicalCamera_PhysicalDataKept()
            {
                Assert.True(m_testCamera.usePhysicalProperties);
                Assert.AreEqual(m_testCamera.focalLength, 100);
                Assert.AreEqual(m_testCamera.sensorSize.x, 30);
                Assert.AreEqual(m_testCamera.lensShift.x, 0);
                Assert.AreEqual(m_testCamera.sensorSize.y, 16.875);
                Assert.AreEqual(m_testCamera.lensShift.y, 0);
            }
        }
    }
}

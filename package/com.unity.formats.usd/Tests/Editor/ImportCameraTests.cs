using System.IO;
using NUnit.Framework;
using pxr;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class ImportCameraTests : BaseFixtureEditor
    {
        [Test]
        [Ignore("USDU-292")]
        public void ImportPhysicalCamera_PhysicalDataKept()
        {
            var cameraScene = TestUtility.OpenUSDSceneWithGUID(TestDataGuids.CameraRelated.PhysicalCameraUsda);
            var m_testCamera = ImportHelpers.ImportSceneAsGameObject(cameraScene).transform.GetChild(0).GetComponent<Camera>();
            cameraScene.Close();

            Assert.True(m_testCamera.usePhysicalProperties);
            Assert.AreEqual(m_testCamera.focalLength, 100);
            Assert.AreEqual(m_testCamera.sensorSize.x, 30);
            Assert.AreEqual(m_testCamera.lensShift.x, 0);
            Assert.AreEqual(m_testCamera.sensorSize.y, 16.875);
            Assert.AreEqual(m_testCamera.lensShift.y, 0);
        }
    }
}

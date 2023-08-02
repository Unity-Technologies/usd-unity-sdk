# if UNITY_2023_3_OR_NEWER

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    public class EditorExportAnalyticsTests : EditorAnalyticsBaseFixture
    {
        static bool[] forceRebuild = new bool[] { true, false };
        static PrimitiveType[] primitiveTypes = new PrimitiveType[] { PrimitiveType.Cube, PrimitiveType.Sphere };

        string m_USDScenePath;

        [SetUp]
        public void SetUp()
        {
            m_USDScenePath = TestUtility.GetUSDScenePath(ArtifactsDirectoryFullPath, "EditorExportAnalyticsTests");
        }

        [UnityTest]
        [Retry(3)]
        // sending valid export analytics returns ok
        // Analytics have correct file ending for valid files exported
        // Analytics for a small / empty export are empty but valid
        // Analytics for valid export contains non-zero timestamp
        public IEnumerator OnValidExportPrimitiveTypeAndExtensions_AnalyticsAreSent([ValueSource("usdExtensions")] string extension, [ValueSource("primitiveTypes")] PrimitiveType primitiveType)
        {
            var unityGO = GameObject.CreatePrimitive(primitiveType);
            ExportHelpers.ExportGameObjects(new GameObject[] { unityGO }, ExportHelpers.InitForSave(m_USDScenePath + extension), BasisTransformation.SlowAndSafe);

            UsdAnalyticsEventExport exportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventExport>(UsdAnalyticsTypes.Export, waitedEvent => {
                exportEvent = waitedEvent;
            }));

            Assert.IsNotNull(exportEvent);
            var exportUsdEventData = exportEvent.msg;

            Assert.IsTrue(exportUsdEventData.Succeeded, "Expected True for export Successful");
            Assert.AreEqual(exportUsdEventData.FileExtension, extension);
            Assert.Greater(exportUsdEventData.TimeTakenMs, 0);
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a small / empty export are empty but valid
        public IEnumerator OnValidExportEmptyGameObject_AnalyticsAreSent()
        {
            var unityGO = new GameObject();
            ExportHelpers.ExportGameObjects(new GameObject[] { unityGO }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            UsdAnalyticsEventExport exportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventExport>(UsdAnalyticsTypes.Export, waitedEvent => {
                exportEvent = waitedEvent;
            }));

            Assert.IsNotNull(exportEvent);
            var exportUsdEventData = exportEvent.msg;

            Assert.IsTrue(exportUsdEventData.Succeeded, "Expected True for export Successful");
            Assert.Greater(exportUsdEventData.TimeTakenMs, 0);
        }

        [UnityTest]
        [Retry(3)]
        public IEnumerator OnInvalidExport_AnalyticsAreSent()
        {
            var unityGO = new GameObject();
            ExportHelpers.ExportGameObjects(new GameObject[] { unityGO }, null, BasisTransformation.SlowAndSafe);

            UsdAnalyticsEventExport exportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventExport>(UsdAnalyticsTypes.Export, waitedEvent => {
                exportEvent = waitedEvent;
            }));

            Assert.IsNotNull(exportEvent);
            var exportUsdEventData = exportEvent.msg;

            Assert.IsFalse(exportUsdEventData.Succeeded, "Expected Fail for export Successful");
            Assert.AreEqual(exportUsdEventData.TimeTakenMs, 0, "Expected 0 for export error of null USD Scene");
            Assert.IsTrue(string.IsNullOrEmpty(exportUsdEventData.FileExtension), "Expected \"\" for export error of null USD Scene");
        }

        [UnityTest]
        [Retry(3)]
        public IEnumerator OnValidExportTransformOverrides_AnalyticsAreSent()
        {
            var unityGO = new GameObject();
            var fullExportScene = ExportHelpers.InitForSave(m_USDScenePath);

            var child1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child1.transform.SetParent(unityGO.transform);
            var child2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            child2.transform.SetParent(unityGO.transform);

            ExportHelpers.ExportGameObjects(new GameObject[] { unityGO }, fullExportScene, BasisTransformation.SlowAndSafe);
            fullExportScene.Close();

            UsdAnalyticsEventExport fullExportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventExport>(UsdAnalyticsTypes.Export, waitedEvent => {
                fullExportEvent = waitedEvent;
            }));

            var fullexportUsdEventData = fullExportEvent.msg;
            Assert.IsFalse(fullexportUsdEventData.OnlyOverrides, "Expected Only overrides to be False for full file export");

            var reImportedObject = ImportHelpers.ImportSceneAsGameObject(ImportHelpers.InitForOpen(m_USDScenePath));
            foreach (Transform child in reImportedObject.transform)
            {
                child.localPosition = Random.insideUnitSphere;
            }

            var overridesExportScene = ExportHelpers.InitForSave(m_USDScenePath + "_overs.usda");
            reImportedObject.GetComponent<UsdAsset>().ExportOverrides(overridesExportScene);

            UsdAnalyticsEventExport overridesExportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventExport>(UsdAnalyticsTypes.Export, waitedEvent => {
                overridesExportEvent = waitedEvent;
            }));

            Assert.IsNotNull(overridesExportEvent);
            var overridesExportUsdEvent = overridesExportEvent.msg;

            Assert.IsTrue(overridesExportUsdEvent.Succeeded, "Expected Only overrides Export to be successful");
            Assert.IsTrue(overridesExportUsdEvent.OnlyOverrides, "Expected Only overrides to be True");
            Assert.Greater(overridesExportUsdEvent.TimeTakenMs, 0, "Expected Only overrides export to take more than 0ms");
        }
    }
}
#endif

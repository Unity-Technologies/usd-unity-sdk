# if UNITY_2023_3_OR_NEWER

using System.Collections;
using NUnit.Framework;
using USD.NET;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    public class EditorReImportAnalyticsTests : EditorAnalyticsBaseFixture
    {
        static bool[] forceRebuild = new bool[] { true, false };
        static string[] testFileGUIDs = new string[] {
            TestDataGuids.Material.SimpleMaterialUsd,
            TestDataGuids.Instancer.PointInstancedUsda,
            TestDataGuids.Mesh.SkinnedMeshUsda
        };

        Scene m_scene;

        [UnityTest]
        [Retry(3)]
        // Analytics have correct file ending for valid files re-imported
        // Analytics for valid re-import contains non-zero timestamp
        public IEnumerator OnValidReImportForceRebuild_AnalyticsAreSent([ValueSource("forceRebuild")] bool forceRebuild)
        {
            m_scene = TestUtility.CreateEmptyTestUsdScene(ArtifactsDirectoryFullPath);
            var importedObject = ImportHelpers.ImportSceneAsGameObject(m_scene);
            importedObject.GetComponent<UsdAsset>().Reload(forceRebuild);

            UsdAnalyticsEventReImport reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, waitedEvent => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = reImportEvent.msg;

            Assert.IsTrue(reImportUsdEventMsg.Succeeded, "Expected True for re-import Successful");
            Assert.AreEqual(reImportUsdEventMsg.ForceRebuild, forceRebuild, "Expected True for re-import Successful");
            Assert.Greater(reImportUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0 for Re-Import");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics have correct file ending for valid files re-imported
        // Analytics for valid re-import contains non-zero timestamp
        public IEnumerator OnValidReImportExtensions_AnalyticsAreSent([ValueSource("usdExtensions")] string extension)
        {
            m_scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath, "testFile" + extension);
            var importedObject = ImportHelpers.ImportSceneAsGameObject(m_scene);

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;

            importedObject.GetComponent<UsdAsset>().Reload(false);

            UsdAnalyticsEventReImport reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, waitedEvent => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = reImportEvent.msg;

            Assert.IsTrue(reImportUsdEventMsg.Succeeded, "Expected True for re-import Successful");
            Assert.Greater(reImportUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0 for Re-Import");
            Assert.AreEqual(extension, reImportUsdEventMsg.FileExtension, "Expected file extension was incorrect");
            Assert.AreEqual(importUsdEventMsg.FileExtension, reImportUsdEventMsg.FileExtension);
        }

        [UnityTest]
        [Retry(3)]
        // ReImport with ImportPreviewSurface will set IncludesMaterials to be True
        public IEnumerator OnValidReImportMaterialsGenerated_AnalyticsAreSent()
        {
            m_scene = OpenUSDGUIDAssetScene(TestDataGuids.Material.SimpleMaterialUsd, out _);

            var importedObject = ImportHelpers.ImportSceneAsGameObject(m_scene);
            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = ((UsdAnalyticsEventImport)importEvent).msg;
            Assert.IsTrue(importUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.IsFalse(importUsdEventMsg.IncludesMaterials, "Expected Includes Materials to be False initially");

            importedObject.GetComponent<UsdAsset>().m_materialImportMode = MaterialImportMode.ImportPreviewSurface;
            importedObject.GetComponent<UsdAsset>().Reload(false);

            UsdAnalyticsEventReImport reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, waitedEvent => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = reImportEvent.msg;

            Assert.IsTrue(reImportUsdEventMsg.Succeeded, "Expected True for re-import Successful");
            Assert.Greater(reImportUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0 for Re-Import");
            Assert.IsTrue(reImportUsdEventMsg.IncludesMaterials, "Expected Includes Materials to be True on ReImport with ImportPreviewSurface setting for Material");
        }

        [UnityTest]
        [Retry(3)]
        // ReImport doesnt have different values for the following Includes: PointInstancer, Meshes, Skel
        public IEnumerator OnValidReImportAndImportPropertiesAreEqual_AnalyticsAreSent([ValueSource("testFileGUIDs")] string testFileGUID)
        {
            m_scene = OpenUSDGUIDAssetScene(testFileGUID, out _);

            var importedObject = ImportHelpers.ImportSceneAsGameObject(m_scene);
            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;

            importedObject.GetComponent<UsdAsset>().m_materialImportMode = MaterialImportMode.ImportPreviewSurface;
            importedObject.GetComponent<UsdAsset>().Reload(false);

            UsdAnalyticsEventReImport reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, waitedEvent => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = reImportEvent.msg;

            Assert.IsTrue(reImportUsdEventMsg.Succeeded, "Expected True for re-import Successful");
            Assert.Greater(reImportUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0 for Re-Import");
            Assert.AreEqual(importUsdEventMsg.IncludesSkel, reImportUsdEventMsg.IncludesSkel);
            Assert.AreEqual(importUsdEventMsg.IncludesPointInstancer, reImportUsdEventMsg.IncludesPointInstancer);
            Assert.AreEqual(importUsdEventMsg.IncludesMeshes, reImportUsdEventMsg.IncludesMeshes);
        }
    }
}
#endif

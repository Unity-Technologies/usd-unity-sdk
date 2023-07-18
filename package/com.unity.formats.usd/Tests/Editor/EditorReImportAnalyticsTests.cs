# if UNITY_2023_2_OR_NEWER

using System.Collections;
using NUnit.Framework;
using USD.NET;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    public class EditorReImportAnalyticsTests : EditorAnalyticsBaseFixture
    {
        const string k_meshedFileGUID = "c06c7eba08022b74ca49dce5f79ef3ba"; // ImportSkinnedMesh.usda
        const string k_materialedFileGUID = "c06c7eba08022b74ca49dce5f79ef3ba"; // simpleMaterialTest.usd
        const string k_pointInstancerFileGUID = "bfb4012f0c339574296e64f4d3c6c595"; // point_instanced_cubes.usda
        const string k_skelFileGUID = "3d00d71254d14bdda401019eb84373ce"; // ImportSkinnedMesh.usda

        static string[] usdExtensions = new string[] { ".usd", ".usda", ".usdc" };
        static bool[] forceRebuild = new bool[] { true, false };
        static string[] testFileGUIDs = new string[] { k_meshedFileGUID, k_pointInstancerFileGUID, k_skelFileGUID };

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

            AnalyticsEvent reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, (AnalyticsEvent waitedEvent) => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = ((UsdAnalyticsEventReImport)reImportEvent.usd).msg;

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

            AnalyticsEvent importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = ((UsdAnalyticsEventImport)importEvent.usd).msg;

            importedObject.GetComponent<UsdAsset>().Reload(false);

            AnalyticsEvent reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, (AnalyticsEvent waitedEvent) => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = ((UsdAnalyticsEventReImport)reImportEvent.usd).msg;

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
            m_scene = OpenUSDGUIDAssetScene(k_materialedFileGUID, out _);

            var importedObject = ImportHelpers.ImportSceneAsGameObject(m_scene);
            AnalyticsEvent importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = ((UsdAnalyticsEventImport)importEvent.usd).msg;
            Assert.IsTrue(importUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.IsFalse(importUsdEventMsg.IncludesMaterials, "Expected Includes Materials to be False initially");

            importedObject.GetComponent<UsdAsset>().m_materialImportMode = MaterialImportMode.ImportPreviewSurface;
            importedObject.GetComponent<UsdAsset>().Reload(false);

            AnalyticsEvent reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, (AnalyticsEvent waitedEvent) => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = ((UsdAnalyticsEventReImport)reImportEvent.usd).msg;

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
            AnalyticsEvent importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = ((UsdAnalyticsEventImport)importEvent.usd).msg;

            importedObject.GetComponent<UsdAsset>().m_materialImportMode = MaterialImportMode.ImportPreviewSurface;
            importedObject.GetComponent<UsdAsset>().Reload(false);

            AnalyticsEvent reImportEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventReImport>(UsdAnalyticsTypes.ReImport, (AnalyticsEvent waitedEvent) => {
                reImportEvent = waitedEvent;
            }));

            Assert.IsNotNull(reImportEvent);
            var reImportUsdEventMsg = ((UsdAnalyticsEventReImport)reImportEvent.usd).msg;

            Assert.IsTrue(reImportUsdEventMsg.Succeeded, "Expected True for re-import Successful");
            Assert.Greater(reImportUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0 for Re-Import");
            Assert.AreEqual(importUsdEventMsg.IncludesSkel, reImportUsdEventMsg.IncludesSkel);
            Assert.AreEqual(importUsdEventMsg.IncludesPointInstancer, reImportUsdEventMsg.IncludesPointInstancer);
            Assert.AreEqual(importUsdEventMsg.IncludesMeshes, reImportUsdEventMsg.IncludesMeshes);
        }
    }
}
#endif

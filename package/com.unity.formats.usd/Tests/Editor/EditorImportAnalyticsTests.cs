# if UNITY_2023_3_OR_NEWER

using System.Collections;
using NUnit.Framework;
using USD.NET;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    public class EditorImportAnalyticsTests : EditorAnalyticsBaseFixture
    {
        static ImportMethods[] importMethod = new ImportMethods[] { ImportMethods.AsGameObject, ImportMethods.AsPrefab, ImportMethods.AsTimelineRecording };

        Scene m_scene;

        [UnityTest]
        [Retry(3)]
        // sending valid import analytics returns ok
        // Analytics have correct file ending for valid files imported
        // Analytics for valid import contains non-zero timestamp
        public IEnumerator OnValidImportExtensions_AnalyticsAreSent([ValueSource("importMethod")] ImportMethods importMethod, [ValueSource("usdExtensions")] string extension)
        {
            m_scene = TestUtility.CreateTestUsdScene(ArtifactsDirectoryFullPath, "testFile" + extension);
            ImportAs(m_scene, importMethod);

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventData = importEvent.msg;
            Assert.IsTrue(importUsdEventData.Succeeded, "Expected True for import successful");
            Assert.AreEqual(extension, importUsdEventData.FileExtension, "Expected file extension was incorrect");
            Assert.Greater(importUsdEventData.TimeTakenMs, 0, "Time Taken MS was reported to be 0 or less");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a mesh 'contains mesh'
        public IEnumerator OnValidImportMeshedScene_AnalyticsAreSent([ValueSource("importMethod")] ImportMethods importMethod)
        {
            m_scene = OpenUSDGUIDAssetScene(TestDataGuids.Material.SimpleMaterialUsd, out _);
            ImportAs(m_scene, importMethod);

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;
            Assert.IsTrue(importUsdEventMsg.Succeeded, "Expected True for import successful");
            Assert.Greater(importUsdEventMsg.TimeTakenMs, 0, "Time Taken MS was reported to be 0 or less");

            // Import as Timeline recording does not include meshes, materials, skels, or point instancers
            var includesMeshes = importMethod != ImportMethods.AsTimelineRecording;
            Assert.AreEqual(importUsdEventMsg.IncludesMeshes, includesMeshes, $"Expected Includes Meshes to be {includesMeshes}");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a mesh 'contains material'
        public IEnumerator OnValidImportSceneWithMaterial_AnalyticsAreSent()
        {
            m_scene = OpenUSDGUIDAssetScene(TestDataGuids.Material.SimpleMaterialUsd, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene, null, new SceneImportOptions() { materialImportMode = MaterialImportMode.ImportPreviewSurface });

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;
            Assert.IsTrue(importUsdEventMsg.Succeeded, "Expected True for import successful");
            Assert.Greater(importUsdEventMsg.TimeTakenMs, 0, "Time Taken MS was reported to be 0 or less");
            Assert.IsTrue(importUsdEventMsg.IncludesMeshes, "Expected Includes Meshes to be True");
            Assert.IsTrue(importUsdEventMsg.IncludesMaterials, "Expected Includes Materials to be True");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for valid import contains non-zero timestamp
        // Analytics for a small / empty import are empty but valid
        public IEnumerator OnValidImportEmptyScene_AnalyticsAreSent([ValueSource("importMethod")] ImportMethods importMethod, [ValueSource("usdExtensions")] string extension)
        {
            m_scene = TestUtility.CreateEmptyTestUsdScene(ArtifactsDirectoryFullPath, "testFile" + extension);
            ImportAs(m_scene, importMethod);

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;
            Assert.IsTrue(importUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.AreEqual(extension, importUsdEventMsg.FileExtension, "Expected file extension was incorrect");
            Assert.Greater(importUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
            Assert.IsFalse(importUsdEventMsg.IncludesMeshes, "Expected Includes Meshes from Empty file to be False");
            Assert.IsFalse(importUsdEventMsg.IncludesMaterials, "Expected Includes Materials from Empty file to be False");
            Assert.IsFalse(importUsdEventMsg.IncludesSkel, "Expected Includes Skel from Empty file to be False");
            Assert.IsFalse(importUsdEventMsg.IncludesPointInstancer, "Expected Includes Point Instancers from Empty file to be False");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a failed import have correct status
        public IEnumerator OnFailedImportNullFile_AnalyticsAreSent()
        {
            Assert.Throws<SceneImporter.ImportException>(() => SceneImporter.ImportUsd(null, null, null, new SceneImportOptions()));
            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;
            Assert.IsFalse(importUsdEventMsg.Succeeded, "Expected False for import Successful");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for failed import contains non-zero timestamp
        public IEnumerator OnFailedImportUnsupportedFileContent_AnalyticsAreSent()
        {
            LogAssert.ignoreFailingMessages = true; // Expecting a bunch of errors on import

            m_scene = OpenUSDGUIDAssetScene(TestDataGuids.Invalid.InvalidContent, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene);

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;
            Assert.IsFalse(importUsdEventMsg.Succeeded, "Expected False for import Successful");
            Assert.Greater(importUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a PointInstancer 'contains pointinstancer'
        public IEnumerator OnValidImportPointInstancerScene_AnalyticsAreSent()
        {
            m_scene = OpenUSDGUIDAssetScene(TestDataGuids.Instancer.PointInstancedUsda, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene);

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;
            Assert.IsTrue(importUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.Greater(importUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
            Assert.IsTrue(importUsdEventMsg.IncludesPointInstancer, "Expected Includes Point Instancers to be True");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a skeleton 'contains skeleton'
        public IEnumerator OnValidImportSkelScene_AnalyticsAreSent()
        {
            // TestDataGuids.Mesh.SkinnedMeshUsda contains USDSkel attribute
            m_scene = OpenUSDGUIDAssetScene(TestDataGuids.Mesh.SkinnedMeshUsda, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene);

            UsdAnalyticsEventImport importEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, waitedEvent => {
                importEvent = waitedEvent;
            }));

            Assert.IsNotNull(importEvent);

            var importUsdEventMsg = importEvent.msg;
            Assert.IsTrue(importUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.Greater(importUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
            Assert.IsTrue(importUsdEventMsg.IncludesSkel, "Expected Includes Skel to be True");
        }

        protected void ImportAs(Scene scene, ImportMethods importMethod)
        {
            switch (importMethod)
            {
                case ImportMethods.AsPrefab:
                    ImportHelpers.ImportAsPrefab(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath, k_TestPrefabName));
                    break;

                case ImportMethods.AsTimelineRecording:
                    ImportHelpers.ImportAsTimelineClip(scene, TestUtility.GetPrefabPath(ArtifactsDirectoryRelativePath, k_TestPrefabName));
                    break;

                case ImportMethods.AsGameObject:
                    ImportHelpers.ImportSceneAsGameObject(scene);
                    break;
            }
        }
    }
}
#endif

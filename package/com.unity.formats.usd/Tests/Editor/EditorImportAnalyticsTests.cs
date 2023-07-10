# if UNITY_2023_2_OR_NEWER

using System.Collections;
using NUnit.Framework;
using USD.NET;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    public class EditorImportAnalyticsTests : EditorAnalyticsBaseFixture
    {
        const string k_unsupportedFileGUID = "75fca2f4d3ec4f646b435a4059d435c8"; // Blend Shapes Kiki
        const string k_meshedFileGUID = "c06c7eba08022b74ca49dce5f79ef3ba"; // ImportSkinnedMesh.usda
        const string k_materialedFileGUID = "c06c7eba08022b74ca49dce5f79ef3ba"; // simpleMaterialTest.usd
        const string k_pointInstancerFileGUID = "bfb4012f0c339574296e64f4d3c6c595"; // point_instanced_cubes.usda
        const string k_skelFileGUID = "3d00d71254d14bdda401019eb84373ce"; // ImportSkinnedMesh.usda

        static string[] usdExtensions = new string[] { ".usd", ".usda", ".usdc" };
        static ImportMethods[] importMethod = new ImportMethods[] { ImportMethods.AsGameObject, ImportMethods.AsPrefab, ImportMethods.AsTimelineRecording };

        Scene m_scene;

        [UnityTest]
        [Retry(3)]
        // sending valid import analytics returns ok
        // Analytics have correct file ending for valid files imported
        // Analytics for valid import contains non-zero timestamp
        public IEnumerator OnValidImportExtensions_AnalyticsAreSent([ValueSource("importMethod")] ImportMethods importMethod, [ValueSource("usdExtensions")] string extension)
        {
            m_scene = CreateTestUsdScene("testFile" + extension);
            ImportAs(m_scene, importMethod);

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsTrue(expectedUsdEventMsg.Succeeded, "Expected True for import successful");
            Assert.AreEqual(extension, expectedUsdEventMsg.FileExtension, "Expected file extension was incorrect");
            Assert.Greater(expectedUsdEventMsg.TimeTakenMs, 0, "Time Taken MS was reported to be 0 or less");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a mesh 'contains mesh'
        public IEnumerator OnValidImportMeshedScene_AnalyticsAreSent([ValueSource("importMethod")] ImportMethods importMethod)
        {
            m_scene = OpenUSDGUIDAssetScene(k_meshedFileGUID, out _);
            ImportAs(m_scene, importMethod);

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsTrue(expectedUsdEventMsg.Succeeded, "Expected True for import successful");
            Assert.Greater(expectedUsdEventMsg.TimeTakenMs, 0, "Time Taken MS was reported to be 0 or less");

            // Import as Timeline recording does not include meshes, materials, skels, or point instancers
            var includesMeshes = importMethod != ImportMethods.AsTimelineRecording;
            Assert.AreEqual(expectedUsdEventMsg.IncludesMeshes, includesMeshes, $"Expected Includes Meshes to be {includesMeshes}");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a mesh 'contains material'
        public IEnumerator OnValidImportSceneWithMaterial_AnalyticsAreSent()
        {
            m_scene = OpenUSDGUIDAssetScene(k_materialedFileGUID, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene, null, new SceneImportOptions() { materialImportMode = MaterialImportMode.ImportPreviewSurface });

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsTrue(expectedUsdEventMsg.Succeeded, "Expected True for import successful");
            Assert.Greater(expectedUsdEventMsg.TimeTakenMs, 0, "Time Taken MS was reported to be 0 or less");
            Assert.IsTrue(expectedUsdEventMsg.IncludesMeshes, "Expected Includes Meshes to be True");
            Assert.IsTrue(expectedUsdEventMsg.IncludesMaterials, "Expected Includes Materials to be True");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for valid import contains non-zero timestamp
        // Analytics for a small / empty import are empty but valid
        public IEnumerator OnValidImportEmptyScene_AnalyticsAreSent([ValueSource("importMethod")] ImportMethods importMethod, [ValueSource("usdExtensions")] string extension)
        {
            m_scene = CreateEmptyTestUsdScene("testFile" + extension);
            ImportAs(m_scene, importMethod);

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsTrue(expectedUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.AreEqual(extension, expectedUsdEventMsg.FileExtension, "Expected file extension was incorrect");
            Assert.Greater(expectedUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
            Assert.IsFalse(expectedUsdEventMsg.IncludesMeshes, "Expected Includes Meshes from Empty file to be False");
            Assert.IsFalse(expectedUsdEventMsg.IncludesMaterials, "Expected Includes Materials from Empty file to be False");
            Assert.IsFalse(expectedUsdEventMsg.IncludesSkel, "Expected Includes Skel from Empty file to be False");
            Assert.IsFalse(expectedUsdEventMsg.IncludesPointInstancer, "Expected Includes Point Instancers from Empty file to be False");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a failed import have correct status
        public IEnumerator OnFailedImportNullFile_AnalyticsAreSent()
        {
            Assert.Throws<SceneImporter.ImportException>(() => SceneImporter.ImportUsd(null, null, null, new SceneImportOptions()));
            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsFalse(expectedUsdEventMsg.Succeeded, "Expected False for import Successful");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for failed import contains non-zero timestamp
        public IEnumerator OnFailedImportUnsupportedFileContent_AnalyticsAreSent()
        {
            LogAssert.ignoreFailingMessages = true; // Expecting a bunch of errors on import

            m_scene = OpenUSDGUIDAssetScene(k_unsupportedFileGUID, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene);

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsFalse(expectedUsdEventMsg.Succeeded, "Expected False for import Successful");
            Assert.Greater(expectedUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a PointInstancer 'contains pointinstancer'
        public IEnumerator OnValidImportPointInstancerScene_AnalyticsAreSent()
        {
            m_scene = OpenUSDGUIDAssetScene(k_pointInstancerFileGUID, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene);

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsTrue(expectedUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.Greater(expectedUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
            Assert.IsTrue(expectedUsdEventMsg.IncludesPointInstancer, "Expected Includes Point Instancers to be True");
        }

        [UnityTest]
        [Retry(3)]
        // Analytics for a skeleton 'contains skeleton'
        public IEnumerator OnValidImportSkelScene_AnalyticsAreSent()
        {
            m_scene = OpenUSDGUIDAssetScene(k_skelFileGUID, out _);
            ImportHelpers.ImportSceneAsGameObject(m_scene);

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics<UsdAnalyticsEventImport>(UsdAnalyticsTypes.Import, (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);

            var expectedUsdEventMsg = ((UsdAnalyticsEventImport)expectedEvent.usd).msg;
            Assert.IsTrue(expectedUsdEventMsg.Succeeded, "Expected True for import Successful");
            Assert.Greater(expectedUsdEventMsg.TimeTakenMs, 0, "Expected Time Taken MS should be greater than 0");
            Assert.IsTrue(expectedUsdEventMsg.IncludesSkel, "Expected Includes Skel to be True");
        }

        protected void ImportAs(Scene scene, ImportMethods importMethod)
        {
            switch (importMethod)
            {
                case ImportMethods.AsPrefab:
                    ImportHelpers.ImportAsPrefab(scene, GetPrefabPath(k_testPrefabName));
                    break;

                case ImportMethods.AsTimelineRecording:
                    ImportHelpers.ImportAsTimelineClip(scene, GetPrefabPath(k_testPrefabName));
                    break;

                case ImportMethods.AsGameObject:
                    ImportHelpers.ImportSceneAsGameObject(scene);
                    break;
            }
        }
    }
}
#endif

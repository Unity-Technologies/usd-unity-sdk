using NUnit.Framework;
using UnityEditor;

namespace Unity.Formats.USD.Tests
{
    public class UsdPrefabTests : BaseFixtureEditor
    {
        static string m_prefabUsdPath;

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(m_prefabUsdPath))
            {
                AssetDatabase.DeleteAsset(m_prefabUsdPath);
            }
        }

        protected void ImportAsPrefab(string usdGUID)
        {
            var scene = OpenUSDGUIDAssetScene(usdGUID, out _);
            m_prefabUsdPath = ImportHelpers.ImportAsPrefab(scene);
            scene.Close();
        }

        public class Reload : UsdPrefabTests
        {
            [TestCase(MaterialImportMode.ImportDisplayColor)]
            [TestCase(MaterialImportMode.None)]
            [TestCase(MaterialImportMode.ImportPreviewSurface)]
            [Ignore("USDU-276")]
            public void UsdPrefab_ForceReload_Succeed(MaterialImportMode mode)
            {
                ImportAsPrefab(TestDataGuids.PrimType.ComponentPayloadUsda);

                var prefabAsset = AssetDatabase.LoadAssetAtPath<UsdAsset>(m_prefabUsdPath);
                prefabAsset.m_materialImportMode = mode;
                prefabAsset.Reload(true);

                // Assertion is not required as it will fail on unhandled error
            }

            [Test]
            [Ignore("USDU-276")]
            public void UsdPrefab_ForceReload_ImportPreviewSurface_DoesNotDirtyScene()
            {
                var initialRootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                ImportAsPrefab(TestDataGuids.PrimType.ComponentPayloadUsda);

                var prefabAsset = AssetDatabase.LoadAssetAtPath<UsdAsset>(m_prefabUsdPath);
                prefabAsset.m_materialImportMode = MaterialImportMode.ImportPreviewSurface;
                prefabAsset.Reload(true);

                var newRootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

                Assert.AreEqual(initialRootGameObjects, newRootGameObjects, "Prefab Reload does not clean up the scene");
            }
        }
    }
}

using NUnit.Framework;
using Unity.PerformanceTesting;
using System.IO;
using UnityEditor;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.Formats.USD.Tests
{
    [Category("Performance")]
    public class BaseUsdPerformanceTests : PerformanceBaseFixture
    {
        #region TestParameters
        // TODO: Change these assets with various static assets in the Test Asset repository once that is properly set up
        // But to do this we might have to separate the Test Projects out of this main repository first
        // since we dont want to bloat the main repo with test assets
        const string k_SmallBaseUsdPrimGUID = "7df73e3eb8d6696408938e54bb9af792";

        // Import Params
        static MaterialImportMode[] modes = new MaterialImportMode[] { MaterialImportMode.None, MaterialImportMode.ImportDisplayColor, MaterialImportMode.ImportPreviewSurface };
        static bool[] forceReloadStates = new bool[] { true, false };

        // Export Params
        static BasisTransformation[] basisTransformations = new BasisTransformation[] { BasisTransformation.None, BasisTransformation.FastWithNegativeScale, BasisTransformation.SlowAndSafe, BasisTransformation.SlowAndSafeAsFBX };
        static int[] childCounts = new int[] { 1, 10, 100, 1000 };
        #endregion

        [Performance]
        [Test]
        public void ImportUsdAsGameObjectImport_PerformanceTest()
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDSceneWithGUID(k_SmallBaseUsdPrimGUID);
                ImportHelpers.ImportSceneAsGameObject(scene);
                scene.Close();
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();
        }

        [Performance]
        [Test]
        public void ImportUsdAsPrefab_PerformanceTest()
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDSceneWithGUID(k_SmallBaseUsdPrimGUID);
                ImportHelpers.ImportAsPrefab(scene, Path.Combine(ArtifactsDirectoryFullPath, System.Guid.NewGuid().ToString() + ".prefab"));
                scene.Close();
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();
        }

        [Performance]
        [Test]
        public void ImportUsdAsTimelineClip_PerformanceTest()
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDSceneWithGUID(k_SmallBaseUsdPrimGUID);
                ImportHelpers.ImportAsTimelineClip(scene, Path.Combine(ArtifactsDirectoryFullPath, System.Guid.NewGuid().ToString() + ".prefab"));
                scene.Close();
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();
        }

        [Performance]
        [UnityTest]
        [Description("Parameters: MaterialImportMode, ForceReload")]
        public IEnumerator UsdGameObjectReload_PerformanceTest([ValueSource("modes")] MaterialImportMode mode, [ValueSource("forceReloadStates")] bool reload)
        {
            var scene = TestUtilityFunction.OpenUSDSceneWithGUID(k_SmallBaseUsdPrimGUID);
            var usdAsset = ImportHelpers.ImportSceneAsGameObject(scene).GetComponent<UsdAsset>();
            usdAsset.m_materialImportMode = mode;

            Measure.Method(() =>
            {
                usdAsset.Reload(reload);
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();

            scene.Close();

            yield return null;
        }

        [Performance]
        [UnityTest]
        [Description("Parameters: MaterialImportMode, ForceReload")]
        [Ignore("USDU-276")]
        public IEnumerator UsdPrefabReload_PerformanceTest([ValueSource("modes")] MaterialImportMode mode, [ValueSource("forceReloadStates")] bool reload)
        {
            var scene = TestUtilityFunction.OpenUSDSceneWithGUID(k_SmallBaseUsdPrimGUID);
            var prefabPath = ImportHelpers.ImportAsPrefab(scene, Path.Combine(ArtifactsDirectoryFullPath, System.Guid.NewGuid().ToString() + ".prefab"));
            var prefabUsdAsset = AssetDatabase.LoadAssetAtPath<UsdAsset>(prefabPath);
            prefabUsdAsset.m_materialImportMode = mode;

            Measure.Method(() =>
            {
                prefabUsdAsset.Reload(true);
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();

            scene.Close();

            yield return null;
        }

        [Performance]
        [UnityTest]
        [Description("Parameters: BasisTransformation, ChildCount")]
        public IEnumerator UsdGameObjectExportWithChildren_PerformanceTest([ValueSource("basisTransformations")] BasisTransformation basisTransformation, [ValueSource("childCounts")] int childCount)
        {
            var testGameObject = CreateExportTestGameObject(childCount);
            var exportPath = TestUtilityFunction.GetUSDScenePath(ArtifactsDirectoryFullPath);

            Measure.Method(() =>
            {
                ExportHelpers.ExportGameObjects(new GameObject[] { testGameObject }, ExportHelpers.InitForSave(exportPath), basisTransformation);
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();

            GameObject.DestroyImmediate(testGameObject);

            yield return null;
        }

        [Performance]
        [UnityTest]
        [Description("Parameters: ChildCount")]
        public IEnumerator UsdGameObjectExportAsUsdz_PerformanceTest([ValueSource("childCounts")] int childCount)
        {
            var testGameObject = CreateExportTestGameObject(childCount);
            var exportPath = TestUtilityFunction.GetUSDScenePath(ArtifactsDirectoryFullPath, "test.usdz");

            Measure.Method(() =>
            {
                UsdzExporter.ExportUsdz(exportPath, testGameObject);
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();

            GameObject.DestroyImmediate(testGameObject);

            yield return null;
        }

        [Performance]
        [UnityTest]
        [Description("Parameters: ChildCount")]
        public IEnumerator UsdGameObjectExportTransformOverrides_PerformanceTest([ValueSource("childCounts")] int childCount)
        {
            var exportPath = TestUtilityFunction.GetUSDScenePath(ArtifactsDirectoryFullPath, "original");
            ExportHelpers.ExportGameObjects(new GameObject[] { CreateExportTestGameObject(childCount) }, ExportHelpers.InitForSave(exportPath), BasisTransformation.SlowAndSafe);

            var testScene = TestUtilityFunction.OpenUSDSceneWithFullPath(exportPath);
            var testGameObject = ImportHelpers.ImportSceneAsGameObject(testScene);
            var overridedPath = TestUtilityFunction.GetUSDScenePath(ArtifactsDirectoryFullPath, "overrided");

            Measure.Method(() =>
            {
                var overrides = ExportHelpers.InitForSave(overridedPath);
                testGameObject.GetComponent<UsdAsset>().ExportOverrides(overrides);
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();

            GameObject.DestroyImmediate(testGameObject);
            testScene.Close();

            yield return null;
        }

        private GameObject CreateExportTestGameObject(int childCount)
        {
            var parent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            parent.name = "parent";
            for (int index = 0; index < childCount; index++)
            {
                var child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                child.name = "child";
                child.transform.parent = parent.transform;
            }

            return parent;
        }
    }
}

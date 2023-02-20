using NUnit.Framework;
using Unity.PerformanceTesting;
using System.IO;

namespace Unity.Formats.USD.Tests
{
    [Category("Performance")]
    public class PointInstancerPerformanceTests : PerformanceBaseFixture
    {
        public enum InstancerSize
        {
            Small,
            Med,
            Large,
            Larger
        }

        const string k_PointInstancerPrim10GUID = "ddba66229ec63364d8ce36112afcdbec";
        const string k_PointInstancerPrim100GUID = "567d2c32dba4a364bb1fb80b134dc92d";
        const string k_PointInstancerPrim1000GUID = "9ca154d52e7e03e4c989b66e33e08212";
        // For Import as Prefab / Timeline it takes more than 3 minutes to run the test, so it wont be included there
        const string k_PointInstancerPrim10000GUID = "b4f2b0c08e1cc0d43aa738154881fcfc";

        private string DeterminePointInstancerSize(InstancerSize testSize)
        {
            switch (testSize)
            {
                case InstancerSize.Small:
                    return k_PointInstancerPrim10GUID;
                case InstancerSize.Med:
                    return k_PointInstancerPrim100GUID;
                case InstancerSize.Large:
                    return k_PointInstancerPrim1000GUID;
                case InstancerSize.Larger:
                    return k_PointInstancerPrim10000GUID;
                default:
                    return "";
            }
        }

        [Performance]
        [TestCase(InstancerSize.Small)]
        [TestCase(InstancerSize.Med)]
        [TestCase(InstancerSize.Large)]
        [TestCase(InstancerSize.Larger)]
        public void ImportPointInstancerAsGameObjectImport_PerformanceTest(InstancerSize testSize)
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDScene(DeterminePointInstancerSize(testSize));
                ImportHelpers.ImportSceneAsGameObject(scene);
                scene.Close();
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();
        }

        [Performance]
        [TestCase(InstancerSize.Small)]
        [TestCase(InstancerSize.Med)]
        [TestCase(InstancerSize.Large)]
        public void ImportPointInstancerAsPrefabImport_PerformanceTest(InstancerSize testSize)
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDScene(DeterminePointInstancerSize(testSize));
                ImportHelpers.ImportAsPrefab(scene, Path.Combine(ArtifactsDirectoryFullPath, System.Guid.NewGuid().ToString() + ".prefab"));
                scene.Close();
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();
        }

        [Performance]
        [TestCase(InstancerSize.Small)]
        [TestCase(InstancerSize.Med)]
        [TestCase(InstancerSize.Large)]
        public void ImportPointInstancerAsTimelineClipImport_PerformanceTest(InstancerSize testSize)
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDScene(DeterminePointInstancerSize(testSize));
                ImportHelpers.ImportAsTimelineClip(scene, Path.Combine(ArtifactsDirectoryFullPath, System.Guid.NewGuid().ToString() + ".prefab"));
                scene.Close();
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();
        }

        [Performance]
        [TestCase(InstancerSize.Small)]
        [TestCase(InstancerSize.Med)]
        [TestCase(InstancerSize.Large)]
        [TestCase(InstancerSize.Larger)]
        public void PointInstancerGameObjectReload_PerformanceTest(InstancerSize testSize)
        {
            var scene = TestUtilityFunction.OpenUSDScene(DeterminePointInstancerSize(testSize));
            var pointInstancerUSD = ImportHelpers.ImportSceneAsGameObject(scene).GetComponent<UsdAsset>();
            Measure.Method(() =>
            {
                pointInstancerUSD.Reload(false);
            })
                .MeasurementCount(TestRunData.MeasurementCount)
                .IterationsPerMeasurement(TestRunData.IterationsPerMeasurement)
                .Run();

            scene.Close();
        }
    }
}

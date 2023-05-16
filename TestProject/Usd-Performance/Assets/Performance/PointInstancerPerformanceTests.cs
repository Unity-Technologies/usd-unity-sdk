using NUnit.Framework;
using Unity.PerformanceTesting;
using System.IO;
using Unity.Formats.USD;

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

        // TODO: Change these test assets with the equivalent files in the Test Asset repository once that is properly set up
        // But to do this we might have to separate the Test Projects out of this main repository first
        // since we dont want to bloat the main repo with test assets
        const string k_PointInstancerPrim10GUID = "ddba66229ec63364d8ce36112afcdbec";
        const string k_PointInstancerPrim100GUID = "567d2c32dba4a364bb1fb80b134dc92d";
        const string k_PointInstancerPrim1000GUID = "9ca154d52e7e03e4c989b66e33e08212";
        // For Import as Prefab / Timeline it takes more than 3 minutes to run the test, so it wont be included there
        const string k_PointInstancerPrim10000GUID = "b4f2b0c08e1cc0d43aa738154881fcfc";

        private string GetGUIDForTestPointInstancerSize(InstancerSize testSize)
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
        public void ImportPointInstancerAsGameObject_PerformanceTest(InstancerSize testSize)
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDSceneWithGUID(GetGUIDForTestPointInstancerSize(testSize));
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
        public void ImportPointInstancerAsPrefab_PerformanceTest(InstancerSize testSize)
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDSceneWithGUID(GetGUIDForTestPointInstancerSize(testSize));
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
        public void ImportPointInstancerAsTimelineClip_PerformanceTest(InstancerSize testSize)
        {
            Measure.Method(() =>
            {
                var scene = TestUtilityFunction.OpenUSDSceneWithGUID(GetGUIDForTestPointInstancerSize(testSize));
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
            var scene = TestUtilityFunction.OpenUSDSceneWithGUID(GetGUIDForTestPointInstancerSize(testSize));
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

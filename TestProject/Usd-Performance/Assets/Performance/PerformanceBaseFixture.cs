using UnityEngine;
using Unity.Formats.USD;
using UnityEngine.TestTools;
using UnityEditor;
using System.IO;

namespace Unity.Formats.USD.Tests
{
    public abstract class PerformanceBaseFixture : IPrebuildSetup, IPostBuildCleanup
    {
        protected string ArtifactsDirectoryName => "Artifacts";
        protected string ArtifactsDirectoryFullPath => Path.Combine(Application.dataPath, ArtifactsDirectoryName);

        public struct TestRunData
        {
            public const int MeasurementCount = 3;
            public const int IterationsPerMeasurement = 1;
        }

        public void Setup()
        {
            InitUsd.Initialize();
            if (Directory.Exists(ArtifactsDirectoryFullPath))
            {
                Cleanup();
            }
            AssetDatabase.Refresh();
            TestUtilityFunction.CreateFolder(ArtifactsDirectoryFullPath);
        }

        public void Cleanup()
        {
            try
            {

                TestUtilityFunction.DeleteFolder(ArtifactsDirectoryFullPath);
                TestUtilityFunction.DeleteMetaFile(ArtifactsDirectoryFullPath);

#if UNITY_EDITOR
                TestUtilityFunction.DeleteAllTexture2DFiles();
#endif
            }
            catch (IOException)
            {
                // Rarely a created prefab file can still be in use by system after tests are complete
                // Do Nothing as it is a rare occurence, and the file usually only contains very small data
            }
            AssetDatabase.Refresh();
        }
    }
}

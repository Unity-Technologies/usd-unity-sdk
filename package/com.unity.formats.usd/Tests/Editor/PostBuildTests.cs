using NUnit.Framework;
using System.IO;
using UnityEditor;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Formats.USD.Tests
{
    public class PostBuildTests
    {
        BuildPlayerOptions m_buildOptions;

        [SetUp]
        public void SetUp()
        {
            m_buildOptions = new BuildPlayerOptions();
            m_buildOptions.locationPathName = "../testBuild/dummy.exe";
            m_buildOptions.options = BuildOptions.None;

#if UNITY_EDITOR_WIN
            m_buildOptions.target = BuildTarget.StandaloneWindows64;
#elif UNITY_EDITOR_OSX
            m_buildOptions.target = BuildTarget.StandaloneOSX;
#elif UNITY_EDITOR_LINUX
            m_buildOptions.target = BuildTarget.StandaloneLinux64;
#endif

            m_buildOptions.options = BuildOptions.None;

            BuildPipeline.BuildPlayer(m_buildOptions);
        }

        [TearDown]
        public void DeleteBuildFolder()
        {
            var buildFolder = new FileInfo(m_buildOptions.locationPathName).Directory;
            Directory.Delete(buildFolder.FullName, true);
        }


        [Test]
        public void PostBuild_UsdPluginsCopy()
        {
            var buildFolder = new FileInfo(m_buildOptions.locationPathName).Directory;
            var dirs = buildFolder.GetDirectories("usd", SearchOption.AllDirectories);

            Assert.IsNotNull(dirs, "The usd plugins directories were not found in the build.");
        }
    }
}

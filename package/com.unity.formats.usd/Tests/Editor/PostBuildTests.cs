﻿using NUnit.Framework;
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
            m_buildOptions.options = BuildOptions.None;

            var tmpPath = System.IO.Path.GetTempPath();
#if UNITY_EDITOR_WIN
            m_buildOptions.locationPathName = Path.Combine(tmpPath, "testBuild", "dummy.exe");
            m_buildOptions.target = BuildTarget.StandaloneWindows64;
#elif UNITY_EDITOR_OSX
            m_buildOptions.locationPathName = Path.Combine(tmpPath, "testBuild", "dummy");
            m_buildOptions.target = BuildTarget.StandaloneOSX;
#elif UNITY_EDITOR_LINUX
            m_buildOptions.locationPathName = Path.Combine(tmpPath, "testBuild", "dummy.x86_64");
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

            Assert.IsTrue(File.Exists(m_buildOptions.locationPathName), "Executable not found. Build failed");
            Assert.IsNotNull(dirs, "The usd plugins directories were not found in the build.");
        }
    }
}

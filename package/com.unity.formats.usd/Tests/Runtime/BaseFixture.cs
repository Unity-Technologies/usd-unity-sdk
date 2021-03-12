using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Scene = USD.NET.Scene;

namespace Unity.Formats.USD.Tests
{
    public class BaseFixture
    {
        public readonly List<string> m_filesToDelete = new List<string>();

        [SetUp]
        public void Setup()
        {
            InitUsd.Initialize();
        }

        public string CreateTmpUsdFile(string fileName)
        {
            var filePath = System.IO.Path.Combine(UnityEngine.Application.dataPath, fileName);
            var scene = Scene.Create(filePath);
            scene.Save();
            scene.Close();
            m_filesToDelete.Add(filePath);
            return filePath;
        }

        [TearDown]
        public void DeleteTmpFiles()
        {
            foreach (var file in m_filesToDelete)
            {
                File.Delete(file);
            }
        }
    }
}

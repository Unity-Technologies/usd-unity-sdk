using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using Scene = USD.NET.Scene;

namespace Unity.Formats.USD.Tests
{
    public class BaseFixtureEditor : BaseFixture
    {
        readonly public List<string> m_assetsToDelete = new List<string>();


        [TearDown]
        public void DeleteAssetsFiles()
        {
            foreach (var file in m_assetsToDelete)
            {
                AssetDatabase.DeleteAsset(file);
            }

            m_assetsToDelete.Clear();
        }
    }
}

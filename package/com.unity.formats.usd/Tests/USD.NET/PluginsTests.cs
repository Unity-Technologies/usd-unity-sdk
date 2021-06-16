using NUnit.Framework;
using UnityEditor;

namespace USD.NET.Tests
{
    class PluginTests : UsdTests
    {
        const string alembicAssetName = "sphere.abc";

        [Test]
        public void AlembicTest_Loads()
        {
            var alembicPath = System.IO.Path.Combine(GetTestDataDirectoryPath(), alembicAssetName);
            USD.NET.Scene scene = USD.NET.Scene.Open(alembicPath);
            var prim = scene.GetPrimAtPath("/Sphere");
            Assert.IsTrue(prim.IsValid());
            scene.Close();
        }
    }
}

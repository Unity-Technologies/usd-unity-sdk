using NUnit.Framework;
using pxr;
using UnityEngine;

namespace USD.NET.Tests
{
    class MinimalSample : SampleBase
    {
        public int number;
        public int? number2;
        public Relationship rel;
    }

    class SceneTests : UsdTests
    {
        MinimalSample sample;
        Scene scene;

        [SetUp]
        public void CreateSceneWithMinimalSample()
        {
            scene = Scene.Create();
            sample = new MinimalSample();
            sample.number = 42;
            sample.rel = new Relationship("/Foo");
            scene.Write("/Foo", sample);
        }

        [Test]
        public void SmokeTest()
        {
            var sample2 = new MinimalSample();
            sample.number2 = null;
            scene.Read("/Foo", sample2);

            AssertEqual(sample2.number, sample.number);
        }

        [Test]
        public void ReadNonExistingPrimsTest()
        {
            var sampleRead = new MinimalSample();
            // Read non existing prim, not in the prim map
            scene.Read("/NotFoo", sampleRead);
            Assert.Zero(sampleRead.number);
            Assert.Null(sampleRead.rel);


            //// Reading the prim adds it in the internal prim map
            var sampleRead2 = new MinimalSample();
            scene.Read("/Foo", sampleRead2);
            Assert.AreEqual(sample.number, sampleRead2.number);
            Assert.NotNull(sample.rel);

            // Reading a non existing prim that is still in the prim map
            scene.Stage.RemovePrim(new SdfPath("/Foo"));
            var sampleRead3 = new MinimalSample();
            scene.Read("/Foo", sampleRead3);
            Assert.Zero(sampleRead3.number);
            Assert.Null(sampleRead3.rel);
        }

        [Test]
        public void GetPrimAtPath_ValidPath_Success()
        {
            const string path = "/Foo";
            var prim = scene.GetPrimAtPath(path);
            Assert.NotNull(prim);
            Assert.AreEqual(prim, scene.Stage.GetPrimAtPath(new SdfPath(path)));
        }

        [Test]
        public void GetPrimAtPath_InvalidPath_ReturnNull()
        {
            const string path = "/NotFoo";
            var prim = scene.GetPrimAtPath(path);
            Assert.Null(prim);
        }

        [Test]
        public void GetAttributeAtPath_ValidPath_Success()
        {
            const string path = "/Foo.number";
            var attr = scene.GetAttributeAtPath(path);
            Assert.NotNull(attr);
            Assert.AreEqual(attr, scene.Stage.GetAttributeAtPath(new SdfPath(path)));
        }

        [TestCase("/Foo.bar", Description = "Invalid attribute")]
        [TestCase("/NotFoo.bar", Description = "Invalid primitive")]
        public void GetAttributeAtPath_InvalidPath_ReturnNull(string path)
        {
            Assert.Null(scene.GetAttributeAtPath(path));
        }

        [Test]
        public void GetRelationshipAtPath_ValidPath_Success()
        {
            const string path = "/Foo.rel";
            var attr = scene.GetRelationshipAtPath(path);
            Assert.NotNull(attr);
            Assert.AreEqual(attr, scene.Stage.GetRelationshipAtPath(new SdfPath(path)));
        }

        [TestCase("/Foo.bar", Description = "Invalid attribute")]
        [TestCase("/Foo.number", Description = "Not a relationship")]
        [TestCase("/NotFoo.rel", Description = "Invalid primitive")]
        public void GetRelationshipAtPath_InvalidPath_ReturnNull(string path)
        {
            Assert.Null(scene.GetRelationshipAtPath(path));
        }

        [TestCase("invalidPath", Description = "Invalid path value")]
        [TestCase("", Description = "Empty path value")]
        [TestCase("../sibling", Description = "Relative path value")]
        public void WritePathToSceneFile_WithInvalidPath_ThrowsException(string path)
        {
            scene = Scene.Create();
            Assert.Throws<System.Exception>(() => scene.Write(path, new SampleBase()));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, string.Format("ApplicationException: USD ERROR: Path must be an absolute path: <{0}>", path));
        }
    }
}

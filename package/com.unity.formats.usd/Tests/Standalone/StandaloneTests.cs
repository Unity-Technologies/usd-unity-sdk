using UnityEngine;
using NUnit.Framework;
using System.IO;
using USD.NET;
using Scene = USD.NET.Scene;

namespace Unity.Formats.USD.Standalone.Tests
{
    public class StandaloneSanityTest
    {
        static string testFilePath = Path.ChangeExtension(Path.GetTempFileName(), "usd");

        class MyCustomData : SampleBase
        {
            public string aString;
            public int[] anArrayOfInts;
            public Bounds aBoundingBox;
        }

        [SetUp]
        public void SetUp()
        {
            InitUsd.Initialize();
        }

        [TearDown]
        public void DeleteTestFile()
        {
            File.Delete(testFilePath);
        }

        [Test]
        public void CanWriteCustomData()
        {
            // Populate Values.
            var value = new MyCustomData();
            value.aString = "IT'S ALIIIIIIIIIIIIIVE!";
            value.anArrayOfInts = new int[] { 1, 2, 3, 4 };
            value.aBoundingBox = new UnityEngine.Bounds();

            // Writing the value.
            var scene = Scene.Create(testFilePath);
            scene.Time = 1.0;
            scene.Write("/someValue", value);
            Debug.Log(scene.Stage.GetRootLayer().ExportToString());
            scene.Save();
            scene.Close();

            Assert.IsTrue(File.Exists(testFilePath), "File not found.");

            // Reading the value.
            Debug.Log(testFilePath);
            var newValue = new MyCustomData();
            scene = Scene.Open(testFilePath);
            scene.Time = 1.0;
            scene.Read("/someValue", newValue);

            Assert.AreEqual(value.aString, newValue.aString, "Serialized data don't match the original data.");

            scene.Close();
        }
    }
}

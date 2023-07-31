using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using USD.NET;
using Scene = USD.NET.Scene;

namespace Unity.Formats.USD.Tests
{
    public class SanityTest : BaseFixtureRuntime
    {
        class MyCustomData : SampleBase
        {
            public string aString;
            public int[] anArrayOfInts;
            public Bounds aBoundingBox;
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
            string usdFile = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath, "sceneFile.usda");
            var scene = ImportHelpers.InitForOpen(usdFile);
            scene.Time = 1.0;
            scene.Write("/someValue", value);
            Debug.Log(scene.Stage.GetRootLayer().ExportToString());
            scene.Save();
            scene.Close();

            Assert.IsTrue(File.Exists(usdFile), "File not found.");

            // Reading the value.
            Debug.Log(usdFile);
            var newValue = new MyCustomData();
            scene = Scene.Open(usdFile);
            scene.Time = 1.0;
            scene.Read("/someValue", newValue);

            Assert.AreEqual(value.aString, newValue.aString, "Serialized data don't match the original data.");

            scene.Close();
        }
    }
}

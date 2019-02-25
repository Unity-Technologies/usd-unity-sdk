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
    public class SanityTest
    {
        List<string> filesToDelete = new List<string>();
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

        [Test]
        public void CanWriteCustomData()
        {
            string usdFile = System.IO.Path.Combine(UnityEngine.Application.dataPath, "sceneFile.usda");
            filesToDelete.Add(usdFile);
            // Populate Values.
            var value = new MyCustomData();
            value.aString = "IT'S ALIIIIIIIIIIIIIVE!";
            value.anArrayOfInts = new int[] { 1, 2, 3, 4 };
            value.aBoundingBox = new UnityEngine.Bounds();

            // Writing the value.
            var scene = Scene.Create(usdFile);
            scene.Time = 1.0;
            scene.Write("/someValue", value);
            Debug.Log(scene.Stage.GetRootLayer().ExportToString());
            scene.Save();
            scene.Close();
            
            Assert.IsTrue(File.Exists(usdFile));

            // Reading the value.
            Debug.Log(usdFile);
            var newValue = new MyCustomData();
            scene = Scene.Open(usdFile);
            scene.Time = 1.0;
            scene.Read("/someValue", newValue);
            
            Assert.AreEqual(value.aString, newValue.aString);
            
            scene.Close();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var file in filesToDelete) 
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}


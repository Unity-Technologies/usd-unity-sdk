using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class SceneTests
    {
        [System.Serializable]
        class MyCustomData : SampleBase {
            public string aString;
            public int[] anArrayOfInts;
            public Bounds aBoundingBox;
        }
        
        readonly string k_UsdFilePath = Path.Combine(Application.dataPath, "testFile.usda");

        [SetUp]
        public void SetUp()
        {
            InitUsd.Initialize();

            // Populate Values.
            var value = new MyCustomData();
            value.aString = "Test String";
            value.anArrayOfInts = new [] { 1, 2, 3, 4 };
            value.aBoundingBox = new Bounds();

            // Writing the value.
            var scene = Scene.Create(k_UsdFilePath);
            scene.Time = 1.0;
            scene.Write("/TestPrim", value);
            scene.Save();
            scene.Close();
        }
        
        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset("Assets/testFile.usda");
        }

        // https://jira.unity3d.com/browse/USDU-60
        [Test]
        public void Scene_ReadWrite_ExistingPrim_PrimIsOverridenWithNewAttributesValue()
        {
            var scene = Scene.Open(k_UsdFilePath);
            scene.Time = 1.0;
            
            var value = new MyCustomData();
            scene.Read("/TestPrim", value);
            
            // Check that value was filled with the prim attributes values.
            Assert.AreEqual("Test String", value.aString);
            Assert.AreEqual(new [] { 1, 2, 3, 4 }, value.anArrayOfInts);
            Assert.AreEqual(new Bounds(), value.aBoundingBox);

            // Override one of the attribute value and write the prim back.
            value.aString = "Existing Test String";
            scene.Write("/TestPrim", value);
            
            // Check that the prim is still correctly written in the file.
            var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath("/TestPrim"));
            Assert.IsTrue(prim.IsValid());
            
            // Check that the attribute value was overriden.
            var vtValue = prim.GetAttribute(new pxr.TfToken("aString")).Get(1);
            Assert.AreEqual((string) vtValue, "Existing Test String");

            scene.Close();
        }

        // https://jira.unity3d.com/browse/USDU-60
        [Test]
        public void Scene_ReadWrite_NonExistingPrim_PrimIsWrittenWithAttributesValue()
        {
            var scene = Scene.Open(k_UsdFilePath);
            scene.Time = 1.0;
            
            var value = new MyCustomData();
            scene.Read("/UnexistingPrim", value);

            // Check that value was not filled with any values (i.e. value is in its default state)
            Assert.IsTrue(string.IsNullOrEmpty(value.aString));
            Assert.IsNull(value.anArrayOfInts);
            Assert.AreEqual(new Bounds(), value.aBoundingBox);
            Assert.IsFalse(scene.Stage.GetPrimAtPath(new pxr.SdfPath("/UnexistingPrim")).IsValid());
            
            // Write the prim to the layer.
            value.aString = "Unexisting Test String";
            scene.Write("/UnexistingPrim", value);

            // Check that the prim was written.
            var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath("/UnexistingPrim"));
            Assert.IsTrue(prim.IsValid());
            
            // Check that the attributes has the right value.
            var vtValue = prim.GetAttribute(new pxr.TfToken("aString")).Get(1);
            Assert.AreEqual((string) vtValue, "Unexisting Test String");
            
            scene.Close();
        }
    }
}
using NUnit.Framework;
using pxr;
using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

public static class TestUtilityFunction
{
    public static class EditorTest
    {
        public static Scene OpenUSDScene(string usdGUID, UsdStage.InitialLoadSet loadSet = UsdStage.InitialLoadSet.LoadNone)
        {
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(usdGUID));
            var stage = UsdStage.Open(usdPath, loadSet);
            var scene = Scene.Open(stage);

            return scene;
        }
    }

    public static class ExportTest
    {
        public static pxr.UsdPrim GetPrim(Scene usdScene, GameObject gameObject)
        {
            Assert.IsNotNull(usdScene);
            Assert.IsNotNull(usdScene.Stage);

            return usdScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(gameObject.transform)));
        }
    }
}

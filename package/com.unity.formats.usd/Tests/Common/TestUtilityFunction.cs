using NUnit.Framework;
using pxr;
using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

public static class TestUtilityFunction
{
#if UNITY_EDITOR
    public static Scene OpenUSDScene(string usdGUID, UsdStage.InitialLoadSet loadSet = UsdStage.InitialLoadSet.LoadNone)
    {
        var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(usdGUID));
        var stage = UsdStage.Open(usdPath, loadSet);
        var scene = Scene.Open(stage);

        return scene;
    }

#endif

    public static pxr.UsdPrim GetGameObjectPrimInScene(Scene usdScene, GameObject gameObject)
    {
        Assert.IsNotNull(usdScene);
        Assert.IsNotNull(usdScene.Stage);

        return usdScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(gameObject.transform)));
    }
}

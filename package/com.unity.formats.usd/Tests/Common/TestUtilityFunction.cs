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

    public static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
    }

    public static void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public static void DeleteMetaFile(string fullPath)
    {
        if (File.Exists(fullPath.TrimEnd('/') + ".meta"))
        {
            File.Delete(fullPath.TrimEnd('/') + ".meta");
        }
    }
}

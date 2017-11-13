using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUsd : MonoBehaviour {
  private static bool m_usdInitialized;

  [System.Serializable]
  class MyCustomData : USD.NET.SampleBase {
    public string aString;
    public int[] anArrayOfInts;
    public UnityEngine.Bounds aBoundingBox;
  }

  // Use this for initialization
  void Start () {
    InitializeUsd();
    Test();
  }
	
  // Update is called once per frame
  void Test() {
    string usdFile = System.IO.Path.Combine(UnityEngine.Application.dataPath, "sceneFile.usda");

    // Populate Values.
    var value = new MyCustomData();  
    value.aString = "Foo";
    value.anArrayOfInts = new int[] { 1, 2, 3, 4 };
    value.aBoundingBox = new UnityEngine.Bounds();

    // Writing the value.
    var scene = USD.NET.Scene.Create(usdFile);
    scene.Time = 1.0;
    scene.Write("/someValue", value);
    Debug.Log(scene.Stage.GetRootLayer().ExportToString());
    scene.Save();
    scene.Close();

    // Reading the value.
    Debug.Log(usdFile);
    value = new MyCustomData();
    scene = USD.NET.Scene.Open(usdFile);
    scene.Time = 1.0;
    scene.Read("/someValue", value);

    Debug.LogFormat("Value: string={0}, ints={1}, bbox={2}",
    value.aString, value.anArrayOfInts, value.aBoundingBox);

    var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath("/someValue"));
    var vtValue = prim.GetAttribute(new pxr.TfToken("aString")).Get(1);
    Debug.Log((string)vtValue);

    scene.Close();
  }

  public static bool InitializeUsd() {
    if (m_usdInitialized) {
      return true;
    }
    try {
      // Initializes native USD plugins and ensures plugins are discoverable on the system path. 
      SetupUsdPath();

      // Type registration enables automatic conversion from Unity-native types to USD types (e.g.
      // Vector3[] -> VtVec3fArray).
      USD.NET.Unity.UnityTypeBindings.RegisterTypes();
      
      // The DiagnosticHandler propagates USD native errors, warnings and info up to C# exceptions
      // and Debug.Log[Warning] respectively.
      USD.NET.Unity.DiagnosticHandler.Register();
    } catch (System.Exception ex) {
      // Make sure failed USD initialization doesn't torpedo all of Tilt Brush.
      Debug.LogException(ex);
      return false;
    }
    m_usdInitialized = true;
    return true;
  }

  // USD has several auxillary C++ plugin discovery files which must be discoverable at run-time
  // We store those libs in Support/ThirdParty/Usd and then set a magic environment variable to let
  // USD's libPlug know where to look to find them.
  private static void SetupUsdPath() {
    var supPath = UnityEngine.Application.dataPath.Replace("\\", "/");

#if (UNITY_EDITOR)
    supPath += @"/UsdUnitySdk/Plugins/x86_64/share/";
#else
    supPath += @"/Plugins/share/";
#endif

    SetupUsdSysPath();

    Debug.LogFormat("Registering plugins: {0}", supPath);
    pxr.PlugRegistry.GetInstance().RegisterPlugins(supPath);
  }

  /// Adds the USD plugin paths to the system path.
  private static void SetupUsdSysPath() {
    var pathVar = "PATH";
    var target = System.EnvironmentVariableTarget.Process;
    var pathvar = System.Environment.GetEnvironmentVariable(pathVar, target);
    var supPath = UnityEngine.Application.dataPath + @"\Plugins;";
    supPath += UnityEngine.Application.dataPath + @"\UsdUnitySdk\Plugins\x86_64;";
    var value = pathvar + @";" + supPath;
    Debug.LogFormat("Adding to sys path: {0}", supPath);
    System.Environment.SetEnvironmentVariable(pathVar, value, target);
  }
}

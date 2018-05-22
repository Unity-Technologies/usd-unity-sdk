using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

public class UsdBuildPostProcess {

  [PostProcessBuildAttribute(1)]
  public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
  {
    var source = Application.dataPath + "/UsdUnitySdk/Plugins";
    var destination = pathToBuiltProject.Replace(".exe", "_Data/Plugins");

    Debug.Log(
      "USDBuildProcess copying USD plugin config files from: \"" +
      source + "\" to \"" + destination + "\""
    );

    // We need to copy the whole share folder and this one plugInfo.json file
    FileUtil.CopyFileOrDirectory(source+"/x86_64/share", destination+"/share");
    FileUtil.CopyFileOrDirectory(source+"/x86_64/plugInfo.json", destination+"/plugInfo.json");
  }
}
#endif

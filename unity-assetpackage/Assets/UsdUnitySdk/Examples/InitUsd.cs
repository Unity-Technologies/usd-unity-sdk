// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USD.NET.Examples {

  public static class InitUsd {
    private static bool m_usdInitialized;

    public static bool Initialize() {
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

#if (UNITY_EDITOR_WIN)
      supPath += @"/UsdUnitySdk/Plugins/x86_64/share/";
#elif (UNITY_EDITOR_OSX)
	  supPath += @"/UsdUnitySdk/Plugins/x86_64/UsdCs.bundle/Contents/Resources/share/";
#elif (UNITY_STANDALONE_WIN)
      supPath += @"/Plugins/share/";
#elif (UNITY_STANDALONE_OSX)
      supPath += @"/Plugins/UsdCs.bundle/Contents/Resources/share/";
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
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
      var supPath = UnityEngine.Application.dataPath + @"\Plugins;";
      supPath += UnityEngine.Application.dataPath + @"\UsdUnitySdk\Plugins\x86_64;";
#elif (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
      var supPath = UnityEngine.Application.dataPath + @"\Plugins;";
      supPath += UnityEngine.Application.dataPath + @"\UsdUnitySdk\Plugins\x86_64/Contents/Resources/share;";
#endif
      var value = pathvar + @";" + supPath;
      Debug.LogFormat("Adding to sys path: {0}", supPath);
      System.Environment.SetEnvironmentVariable(pathVar, value, target);
    }
  }

}
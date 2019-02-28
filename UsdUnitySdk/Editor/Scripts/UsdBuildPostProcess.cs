// Copyright 2018 Pixar Animation Studios
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

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Unity.Formats.USD {
  public class UsdBuildPostProcess {

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
#if UNITY_EDITOR_OSX
      // plugInfo files are already in the UsdCs.bundle
      return;
#else
      var source = System.IO.Path.GetFullPath("Packages/com.unity.formats.usd/Runtime/Plugins");
      var destination = pathToBuiltProject.Replace(".exe", "_Data/Plugins");

      // We need to copy the whole share folder and this one plugInfo.json file
      FileUtil.CopyFileOrDirectory(source + "/x86_64/share", destination + "/share");
      FileUtil.CopyFileOrDirectory(source + "/x86_64/plugInfo.json", destination + "/plugInfo.json");
#endif
    }
  }

}
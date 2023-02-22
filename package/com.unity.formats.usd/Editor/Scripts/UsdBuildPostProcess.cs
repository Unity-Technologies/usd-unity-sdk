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
using System.IO;
using System.Runtime.CompilerServices;

namespace Unity.Formats.USD
{
    public class UsdBuildPostProcess
    {
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var source = Path.Combine(GetCurrentDir(), "..", "..", "Runtime", "Plugins");
            var destination = "";
            if (target == BuildTarget.StandaloneLinux64)
            {
                destination = pathToBuiltProject.Replace(".x86_64", "_Data/Plugins");
            }
            else if (target == BuildTarget.StandaloneOSX)
            {
                destination = pathToBuiltProject + "/Contents/Plugins";
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                destination = pathToBuiltProject.Replace(".exe", "_Data/Plugins");
            }
            else
            {
                Debug.LogWarning("The USD package is not supported in non desktop builds. The USD plugins directory will not be included in the build.");
                return;
            }

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            else
            {
                var attrs = File.GetAttributes(destination);
                attrs &= ~FileAttributes.ReadOnly;
                File.SetAttributes(destination, attrs);
            }

            // We need to copy the whole share folder
            FileUtil.CopyFileOrDirectory(source + "/x86_64/usd", destination + "/usd");
            FileUtil.CopyFileOrDirectory(source + "/x86_64/plugInfo.json", destination + "/plugInfo.json");
        }

        static string GetCurrentDir([CallerFilePath] string filePath = "")
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.DirectoryName;
        }
    }
}

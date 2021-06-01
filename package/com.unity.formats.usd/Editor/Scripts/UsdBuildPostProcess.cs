using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Callbacks;

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
            else if(target == BuildTarget.StandaloneOSX)
            {
                destination = pathToBuiltProject + "/Contents/Plugins";
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                destination = pathToBuiltProject.Replace(".exe", "_Data/Plugins");
            }

            // We need to copy the whole share folder and this one plugInfo.json file
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

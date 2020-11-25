using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD
{
    public static class PackageUtils
    {

        /// <summary>
        /// Utils function to get the relative path of a file considering a base folder
        /// </summary>
        public static string GetRelativeFolderPath(string baseFolderPath, string filePath = "")
        {
            const string universalSeparator = @"/";
            const string windowsSeparator = @"\";

            // Conforming paths to ease next steps
            filePath = filePath.Replace(windowsSeparator, universalSeparator);
            baseFolderPath = baseFolderPath.Replace(windowsSeparator, universalSeparator);

            // baseFolderPath must end with a slash to indicate folder
            baseFolderPath = baseFolderPath.TrimEnd();
            if (!baseFolderPath.EndsWith(universalSeparator))
            {
                baseFolderPath += universalSeparator;
            }

            // Using URIs to find the relative path
            var sampleFolderAbsoluteURI = new Uri(Path.GetDirectoryName(filePath));
            var projectFolderAbsoluteURI = new Uri(baseFolderPath);
            var sampleFolderRelativePath =
                Uri.UnescapeDataString(
                    projectFolderAbsoluteURI.MakeRelativeUri(sampleFolderAbsoluteURI).ToString()
                        .Replace('/', Path.DirectorySeparatorChar)
                );
            return sampleFolderRelativePath;
        }

        /// <summary>
        /// Utils function to get the relative path of the current caller with current Unity directory as a base folder
        /// </summary>
        public static string GetCallerRelativeToProjectFolderPath([CallerFilePath] string filePath = "")
        {
            return GetRelativeFolderPath(Directory.GetCurrentDirectory(), filePath);
        }


        /// <summary>
        /// Utils function to get the absolute path of a MonoBehaviour
        /// </summary>
        public static string GetMonoBehaviourPath(MonoBehaviour mono)
        {
            var script = MonoScript.FromMonoBehaviour(mono);
            return AssetDatabase.GetAssetPath(script);
        }

        /// <summary>
        /// Utils function to get the absolute path of the parent folder of a MonoBehaviour
        /// </summary>
        public static string GetMonoBehaviourFolderPath(MonoBehaviour mono)
        {
            return Directory.GetParent(GetMonoBehaviourPath(mono)).FullName;
        }
    }
}

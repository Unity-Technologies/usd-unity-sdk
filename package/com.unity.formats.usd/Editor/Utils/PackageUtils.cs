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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD
{
    public static class SampleUtils
    {
        /// <summary>
        /// Utils functions for the package Samples
        /// </summary>

        private static EditorWindow GetConsoleWindow()
        {
            var editorWindowTypes = TypeCache.GetTypesDerivedFrom<EditorWindow>();
            foreach (var type in editorWindowTypes)
            {
                if (type.Name == "ConsoleWindow")
                {
                    return EditorWindow.GetWindow(type);
                }
            }

            throw new System.Exception("Error could not find ConsoleWindow type");
        }

        public static void FocusConsoleWindow()
        {
#if UNITY_EDITOR
            var consoleWindow = GetConsoleWindow();
            consoleWindow.Focus();
#endif
        }

        public struct TextColor
        {
            public const string Red = "#FF2D2D";
            public const string Green = "#00FF00";
            public const string Blue = "#338DFF";
        }
    }
}

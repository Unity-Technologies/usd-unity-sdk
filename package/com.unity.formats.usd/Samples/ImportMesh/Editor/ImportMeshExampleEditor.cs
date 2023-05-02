// Copyright 2023 Unity Technologies. All rights reserved.
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
using System.IO;

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(ImportMeshExample))]
    public class ImportMeshExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            var script = (ImportMeshExample)target;
            if (GUILayout.Button("1. Initialize USD"))
            {
                script.InitializeUsd();
            }

            if (GUILayout.Button("2. Set USD File Path..."))
            {
                string lastDir;
                if (string.IsNullOrEmpty(script.GetUsdFilePath()))
                    lastDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                else
                    lastDir = Path.GetDirectoryName(script.GetUsdFilePath());
                string importFilepath =
                    EditorUtility.OpenFilePanelWithFilters("Usd Asset", lastDir, new string[] { "Usd", "us*" });
                if (string.IsNullOrEmpty(importFilepath)) return;
                script.m_usdFile = importFilepath;
            }

            EditorGUILayout.PrefixLabel("USD File");
            GUI.enabled = false;
            EditorGUILayout.TextField(script.m_usdFile, EditorStyles.textField);
            GUI.enabled = true;

            if (string.IsNullOrEmpty(script.GetUsdFilePath()))
            {
                var labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, wordWrap = true };
                labelStyle.normal.textColor = Color.red;

                GUILayout.Label($"\nUSD file path specfied is invalid", labelStyle);
            }
            else
            {
                if (GUILayout.Button("3. Import Mesh"))
                {
                    script.ImportUsdFile();
                }
            }
        }
    }
}

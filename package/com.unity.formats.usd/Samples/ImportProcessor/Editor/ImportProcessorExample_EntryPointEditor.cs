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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(ImportProcessorExample_EntryPoint))]
    public class ImportProcessorExample_EntryPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ImportProcessorExample_EntryPoint script = (ImportProcessorExample_EntryPoint)target;
            script.usdAsset.usdFullPath = Path.Combine(PackageUtils.GetCallerRelativeToProjectFolderPath(), "..", "MultipleMeshes.usda");

            var labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            labelStyle.normal.textColor = SampleUtils.TextColor.Default;

            if (GUI.changed)
            {
                script.ResetUsdAsset();
                script.SetImportProcessorMode(script.sampleMode);
            }

            switch (script.sampleMode)
            {
                case ImportProcessorExample_EntryPoint.SampleMode.CombineMeshes:
                    GUILayout.Label($"<b>Combine Meshes</b> setting will combine the meshes of all children within <{script.usdAsset.gameObject.name}> and create a new child object containing the combined mesh.", labelStyle);
                    break;

                case ImportProcessorExample_EntryPoint.SampleMode.SetHideFlags:
                    GUILayout.Label($"<b>Set Hide Flags</b> setting will apply all children within <{script.usdAsset.gameObject.name}> with the below Unity Hide Flag Settings", labelStyle);
                    GUILayout.Label("Hide Flags Setting:");
                    script.hideFlagsSetting = (HideFlags)EditorGUILayout.EnumFlagsField(script.hideFlagsSetting);
                    break;
            }

            // Empty Space
            GUILayout.Label("");

            if (GUILayout.Button("1. Initialize USD"))
            {
                InitUsd.Initialize();
            }

            if (GUILayout.Button("2. Refresh Asset"))
            {
                script.RefreshUSD();
                SampleUtils.FocusConsoleWindow();
            }

            // Empty Space
            GUILayout.Label("");

            if (GUILayout.Button("Reset Sample USD Asset"))
            {
                script.ResetUsdAsset();
                script.SetImportProcessorMode(script.sampleMode);
            }
        }
    }
}

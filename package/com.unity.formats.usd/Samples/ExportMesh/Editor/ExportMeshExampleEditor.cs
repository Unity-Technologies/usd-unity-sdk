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

using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(ExportMeshExample))]
    public class ExportMeshExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ExportMeshExample script = (ExportMeshExample)target;

            var labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            labelStyle.normal.textColor = SampleUtils.TextColor.Default;

            GUILayout.Label($"\nFor Exporting as <.{script.FileExtension}>, follow these step(s):", labelStyle);

            if (GUILayout.Button("1. Initialize USD Package"))
            {
                script.InitUSD();
            }

            switch (script.FileExtension)
            {
                case ExportMeshExample.UsdFileExtension.usd:
                case ExportMeshExample.UsdFileExtension.usda:
                case ExportMeshExample.UsdFileExtension.usdc:
                    {
                        if (GUILayout.Button("2. Create New USD Scene"))
                        {
                            SampleUtils.FocusConsoleWindow();

                            script.CreateNewUsdScene();
                            Debug.Log(SampleUtils.SetTextColor(SampleUtils.TextColor.Green, $"Created USD file: <b><{script.m_newUsdFileName}.{script.FileExtension}></b> under project <b>'{SampleUtils.SampleArtifactRelativeDirectory}'</b> folder"));
                        }

                        if (GUILayout.Button("3. Set up Export Context"))
                        {
                            SampleUtils.FocusConsoleWindow();

                            script.SetUpExportContext();
                            Debug.Log($"Export Context has been set up.");
                        }

                        if (GUILayout.Button("4. Export"))
                        {
                            SampleUtils.FocusConsoleWindow();

                            script.Export();
                            Debug.Log($"Data of 'Export Context' has been exported to USD file <b><{script.m_newUsdFileName}.{script.FileExtension}></b>.");
                        }

                        if (GUILayout.Button("5. Save Scene"))
                        {
                            SampleUtils.FocusConsoleWindow();

                            script.SaveScene();
                            AssetDatabase.Refresh();
                            Debug.Log(SampleUtils.SetTextColor(SampleUtils.TextColor.Green, $"Data exported to <b><{script.m_newUsdFileName}.{script.FileExtension}></b> has been saved."));
                            Debug.Log($"The file <b><{script.m_newUsdFileName}.{script.FileExtension}></b> is available at your project <b>'{SampleUtils.SampleArtifactRelativeDirectory}'</b> directory.");
                        }

                        if (GUILayout.Button("6. Close Scene"))
                        {
                            script.CloseScene();
                            Debug.Log("Closed USD Scene.");
                        }
                        break;
                    }

                case ExportMeshExample.UsdFileExtension.usdz:
                    {
                        if (GUILayout.Button("2. Export GameObject as USDZ"))
                        {
                            SampleUtils.FocusConsoleWindow();
                            Debug.Log("For USDZ Export the sample will utilize the <b>UsdzExporter.cs</b> script.");
                            script.ExportAsUsdz();
                            AssetDatabase.Refresh();
                            Debug.Log(SampleUtils.SetTextColor(SampleUtils.TextColor.Green, $"Exported details of <b><{script.m_exportRoot.name}></b> into <b><{script.m_newUsdFileName}.usdz></b>"));
                        }
                        break;
                    }
            }
        }
    }
}

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

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(ExportMeshWithAnimationExample))]
    public class ExportMeshWithAnimationExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ExportMeshWithAnimationExample script = (ExportMeshWithAnimationExample)target;

            if (!EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Play Scene to Start Animation"))
                {
                    EditorApplication.isPlaying = true;
                }
            }
            else
            {
                if (script.IsFinishedRecording)
                {
                    if (GUILayout.Button("Recording Complete - Stop Scene"))
                    {
                        EditorApplication.isPlaying = false;
                        Debug.Log($"<color={SampleUtils.TextColor.Green}>Open the <b>'Assets'</b> Folder in your Unity 'Project' Window to see your newly exported USD file <{script.m_newUsdFileName}></color>");
                        SampleUtils.FocusConsoleWindow();
                        AssetDatabase.Refresh();
                    }
                }

                else
                {
                    if (script.IsRecording)
                    {
                        GUI.backgroundColor = Color.white;
                        var labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                        labelStyle.normal.textColor = Color.white;
                        GUILayout.Label("Recording...", labelStyle);
                    }
                    else
                    {
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("Record Animation"))
                        {
                            EditorApplication.isPaused = false;
                            script.StartRecording();
                        }
                    }
                }
            }
        }
    }
}

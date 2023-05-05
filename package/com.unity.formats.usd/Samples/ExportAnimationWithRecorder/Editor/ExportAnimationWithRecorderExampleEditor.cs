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
#if UNITY_RECORDER
using UnityEditor.Recorder;
#endif
using UnityEngine;

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(ExportAnimationWithRecorderExample))]
    public class ExportAnimationWithRecorderExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Empty blank line
            GUILayout.Label("");

#if !UNITY_RECORDER
            var labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            labelStyle.normal.textColor = Color.red;

            GUILayout.Label($"Unity 'Recorder' package is required for this sample.\nPlease install the <b>'Recorder' package (com.unity.recorder)</b> from the 'Unity Package Manager'", labelStyle);
            if (GUILayout.Button("Link"))
            {
                Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.recorder@4.0/manual/index.html");
            }
#else
            if (GUILayout.Button("Open Unity 'Recorder' window."))
            {
                var recorderWindow = (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
            }

            var labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleLeft, wordWrap = true };
            labelStyle.normal.textColor = SampleUtils.TextColor.Default;

            GUILayout.Label($"1. Select the '{SampleUtils.SetTextColor(SampleUtils.TextColor.Green, "START RECORDING")}' button to enter Play Mode, the Recorder will automatically record the animation.", labelStyle);
            GUILayout.Label($"2. Select the '{SampleUtils.SetTextColor(SampleUtils.TextColor.Red, "STOP RECORDING")}' button to Stop your recording - You may also exit Play Mode.", labelStyle);
            GUILayout.Label($"\n<b>Note:</b>", labelStyle);
            GUILayout.Label($" - Set 'Format' > '{SampleUtils.SetTextColor(SampleUtils.TextColor.Blue, "Export Format")}' to your desired USD format.", labelStyle);
            GUILayout.Label($" - Set 'Format' > '{SampleUtils.SetTextColor(SampleUtils.TextColor.Blue, "Override Setting")}' to either '{SampleUtils.SetTextColor(SampleUtils.TextColor.Yellow, "Export In Full")}' or '{SampleUtils.SetTextColor(SampleUtils.TextColor.Yellow, "Export Transform Overrides Only")}'.", labelStyle);
            GUILayout.Label($" - Set 'Output File' > '{SampleUtils.SetTextColor(SampleUtils.TextColor.Blue, "Path")}' to your desired location.", labelStyle);
            GUILayout.Label($" - The resulting exported USD file should be at your designated directory.", labelStyle);
#endif
        }
    }
}

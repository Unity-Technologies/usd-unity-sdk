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

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(HelloUsdExample))]
    public class HelloUsdExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();

            HelloUsdExample helloUsdScript = (HelloUsdExample)target;
            var labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            labelStyle.normal.textColor = Color.white;

            GUILayout.Label($"\nAny USD operation first requires Initialization", labelStyle);
            if (GUILayout.Button("Initialize USD"))
            {
                SampleUtils.FocusConsoleWindow();
                helloUsdScript.InitializeUsd();
            }

            GUILayout.Label($"\nCreate and add custom data to USD Scene", labelStyle);
            if (GUILayout.Button("1. Create a new USD scene file"))
            {
                SampleUtils.FocusConsoleWindow();
                helloUsdScript.CreateUsdScene();
            }

            if (GUILayout.Button("2. Add custom data to created USD scene file"))
            {
                SampleUtils.FocusConsoleWindow();
                helloUsdScript.AddCustomDataToScene();
            }

            if (GUILayout.Button("3. Save added custom data in USD scene file"))
            {
                SampleUtils.FocusConsoleWindow();
                helloUsdScript.SaveDataInScene();
            }

            if (GUILayout.Button("4. Close the USD scene file"))
            {
                helloUsdScript.CloseScene();
            }

            GUILayout.Label($"\nOpen and read custom data from USD Scene", labelStyle);
            if (GUILayout.Button("1. Open USD Scene file"))
            {
                helloUsdScript.OpenScene();
            }

            if (GUILayout.Button("2. Read custom data from USD Scene file"))
            {
                SampleUtils.FocusConsoleWindow();
                helloUsdScript.ReadCustomDataFromScene();
            }

            if (GUILayout.Button("3. Close the USD scene file"))
            {
                helloUsdScript.CloseScene();
            }
        }
    }
}

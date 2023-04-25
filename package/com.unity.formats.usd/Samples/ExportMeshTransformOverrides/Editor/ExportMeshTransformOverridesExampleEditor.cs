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
    [CustomEditor(typeof(ExportMeshTransformOverridesExample))]
    public class ExportMeshTransformOverridesExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ExportMeshTransformOverridesExample exportMeshTransformOverridesScript = (ExportMeshTransformOverridesExample)target;
            if (GUILayout.Button("1. Import the example USD File"))
            {
                if (ExportMeshTransformOverridesExample.m_exampleImportedUsdObject == null)
                {
                    InitUsd.Initialize();
                    exportMeshTransformOverridesScript.ImportInitialUsdFile();
                }
            }

            if (ExportMeshTransformOverridesExample.m_exampleImportedUsdObject != null)
            {
                if (GUILayout.Button("2. Change Transform data of imported USD File"))
                {
                    exportMeshTransformOverridesScript.ChangeExampleTransformData();
                }

                if (GUILayout.Button($"3. Export Transform Overrides as { ExportMeshTransformOverridesExample.m_exampleImportedUsdObject.name }"))
                {
                    exportMeshTransformOverridesScript.ExportTransformOverride();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                var labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                labelStyle.normal.textColor = Color.red;
                GUILayout.Label("Example USD File has not been imported - Please Import the example USD file using the above button", labelStyle);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
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

            var labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            labelStyle.normal.textColor = Color.white;
            GUILayout.Label("\nA) For Exporting, follow these step(s):", labelStyle);

            if (GUILayout.Button("1. Initialize and Create USD Scene for Export"))
            {
                SampleUtils.FocusConsoleWindow();
                if (script.m_newUsdFileName.ToLower().EndsWith(".usdz"))
                {
                    Debug.LogError("To Export as USDZ file reference the section below, section B)");
                    return;
                }

                script.InitializeForExport();
                Debug.Log($"<color=#00FF00>Created USD file: <b><{script.m_newUsdFileName}></b> under project <b>'Assets'</b> folder</color>");
            }

            if (GUILayout.Button("2. Export GameObject"))
            {
                SampleUtils.FocusConsoleWindow();
                if (script.m_newUsdFileName.ToLower().EndsWith(".usdz"))
                {
                    Debug.LogError("To Export as USDZ file reference the section below, section B)");
                    return;
                }

                script.ExportGameObject();
                Debug.Log($"<color=#00FF00>Exported details of <b><{script.m_exportRoot.name}></b> into <b><{script.m_newUsdFileName}></b></color>");
            }

            GUILayout.Label("\nB) For Exporting as USDZ, follow these step(s):", labelStyle);
            if (GUILayout.Button("1. Export GameObject as USDZ"))
            {
                SampleUtils.FocusConsoleWindow();
                if (!script.m_newUsdFileName.ToLower().EndsWith(".usdz"))
                {
                    Debug.LogError($"To Export as non-USDZ file reference the section above, section A)");
                    return;
                }
                script.ExportGameObjectAsUSDZ();
                Debug.Log($"<color=#00FF00>Exported details of <b><{script.m_exportRoot.name}></b> into <b><{script.m_newUsdFileName}></b></color>");
            }
        }
    }
}

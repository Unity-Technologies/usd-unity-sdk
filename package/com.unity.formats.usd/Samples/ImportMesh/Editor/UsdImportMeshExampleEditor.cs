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
using USD.NET;

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(ImportMeshExample))]
    public class UsdImportMeshEditor : Editor
    {
        string MakeOversPath(string path)
        {
            return System.IO.Path.ChangeExtension(path, ".overs.usda");
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            if (GUILayout.Button("Export Overides"))
            {
                var importMesh = (ImportMeshExample)target;
                var oversFilePath = MakeOversPath(importMesh.m_usdFile);

                if (string.IsNullOrEmpty(oversFilePath))
                {
                    Debug.LogWarning("Empty export path.");
                }

                // Let the Scene.Create function throw an exception when it can't create a USD stage.
                var oversScene = Scene.Create(oversFilePath);

                oversScene.UpAxis = importMesh.UsdScene.UpAxis;
                oversScene.Time = importMesh.m_usdTime;
                oversScene.AddSubLayer(importMesh.UsdScene);

                XformExporter.WriteSparseOverrides(oversScene,
                    importMesh.PrimMap,
                    importMesh.m_changeHandedness);

                oversScene.Save();
                oversScene.Close();

                Debug.Log("Written: " + oversFilePath);
            }
        }
    }
}

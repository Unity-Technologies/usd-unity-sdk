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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(ImportMaterialsExample))]
    public class ImportMaterialsExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ImportMaterialsExample script = (ImportMaterialsExample)target;

            if (GUILayout.Button("1. Initialize USD Package"))
            {
                script.InitializeUsd();
            }

            if (GUILayout.Button("2. Initialize Sample Shader Map Dictionary"))
            {
                script.InitializeSampleShaderMapDictionary();
            }

            if (GUILayout.Button("3. Create a Sample USD Scene"))
            {
                script.CreateUsdScene();
            }

            if (GUILayout.Button("4. Initialize Sample USD Material"))
            {
                script.InitializeSampleMaterial();
            }

            if (GUILayout.Button("5. Construct And Set Unity Material"))
            {
                script.ConstructAndSetUnityMaterial();
            }

            if (GUILayout.Button("6. Bind Geometry"))
            {
                script.BindGeometry();
                Debug.Log($"Unity GameObject created and assigned with the imported materials");
            }

            if (GUILayout.Button("7. Close USD Scene"))
            {
                script.CloseUsdScene();
            }
        }
    }
}

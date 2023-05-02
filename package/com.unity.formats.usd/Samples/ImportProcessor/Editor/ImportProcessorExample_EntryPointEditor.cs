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
            script.usdAsset.usdFullPath = Path.Combine(PackageUtils.GetCallerRelativeToProjectFolderPath(), "..", "ExampleAsset", "MultipleMeshes.usda");

            if (GUILayout.Button("1. Initialize USD"))
            {
                InitUsd.Initialize();
            }

            if (GUILayout.Button("2. Refresh Asset"))
            {
                script.RefreshUSD();
            }
        }
    }
}

// Copyright 2018 Jeremy Cowles. All rights reserved.
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
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using USD.NET;

namespace Unity.Formats.USD
{
    [CustomEditor(typeof(UsdLayerStack))]
    public class UsdLayerStackEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            var layerStack = (UsdLayerStack)this.target;

            base.DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Save Overrides to Target Layer"))
            {
                InitUsd.Initialize();
                layerStack.SaveToLayer();
            }

            if (GUILayout.Button("Save Layer Stack"))
            {
                InitUsd.Initialize();
                Scene scene = Scene.Open(layerStack.GetComponent<UsdAsset>().usdFullPath);
                try
                {
                    layerStack.SaveLayerStack(scene, layerStack.m_layerStack);
                }
                finally
                {
                    scene.Close();
                    scene = null;
                }
            }
        }
    }
}

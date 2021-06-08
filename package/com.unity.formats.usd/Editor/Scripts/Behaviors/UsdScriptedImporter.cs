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

using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using USD.NET;

#if false
[ScriptedImporter(1, new string[] { "usd-hook" }, 0)]
public class UsdScriptedImporter : ScriptedImporter
{
    /// <summary>
    /// ScriptedImporter API.
    /// https://docs.unity3d.com/Manual/ScriptedImporters.html
    /// </summary>
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var usdFilePath = File.ReadAllText(ctx.assetPath);
        var importOptions = new SceneImportOptions();

        importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
        importOptions.assetImportPath = Path.GetDirectoryName(ctx.assetPath);
        importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
        importOptions.materialMap.SpecularWorkflowMaterial = new Material(Shader.Find("Standard (Specular setup)"));
        importOptions.materialMap.MetallicWorkflowMaterial = new Material(Shader.Find("Standard (Roughness setup)"));
        importOptions.materialMap.FallbackMasterMaterial = new Material(Shader.Find("USD/StandardVertexColor"));

        var time = 1.0f;
        var go = new GameObject();
        UsdAssetImporter.ImportUsd(go, usdFilePath, time, importOptions);

        var usdImporter = go.AddComponent<UsdAssetImporter>();
        usdImporter.m_usdFile = usdFilePath;
        usdImporter.m_usdTime = time;
        usdImporter.OptionsToState(importOptions);

        var meshes = new HashSet<Mesh>();
        var materials = new HashSet<Material>();

        ctx.AddObjectToAsset(go.GetInstanceID().ToString(), go);
        ctx.SetMainObject(go);

        int objIndex = 0;

        foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
        {
            if (meshes.Add(mf.sharedMesh))
            {
                ctx.AddObjectToAsset(mf.name + "_mesh_" + objIndex++, mf.sharedMesh);
            }
        }

        foreach (var mf in go.GetComponentsInChildren<MeshRenderer>())
        {
            foreach (var mat in mf.sharedMaterials)
            {
                if (!materials.Add(mat))
                {
                    continue;
                }
                ctx.AddObjectToAsset(mf.name + "_mat_" + objIndex++, mat);
            }
        }

        foreach (var mf in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (meshes.Add(mf.sharedMesh))
            {
                ctx.AddObjectToAsset(mf.name + "_mesh_" + objIndex++, mf.sharedMesh);
            }
        }

        foreach (var mf in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            foreach (var mat in mf.sharedMaterials)
            {
                if (!materials.Add(mat))
                {
                    continue;
                }
                ctx.AddObjectToAsset(mf.name + "_mat_" + objIndex++, mat);
            }
        }
    }
}
#endif

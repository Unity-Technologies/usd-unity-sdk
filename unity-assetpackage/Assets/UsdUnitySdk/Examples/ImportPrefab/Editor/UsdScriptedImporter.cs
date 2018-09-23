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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using USD.NET.Unity;

[ScriptedImporter(1, new string[] { "usd", "usda", "usdc", "abc" }, 0)]
public class UsdScriptedImporter : ScriptedImporter {

  /// <summary>
  /// ScriptedImporter API.
  /// https://docs.unity3d.com/Manual/ScriptedImporters.html
  /// </summary>
  public override void OnImportAsset(AssetImportContext ctx) {
    var solidColorMat = new Material(Shader.Find("Standard"));
    solidColorMat.SetFloat("_Glossiness", 0.2f);

    var importOptions = new SceneImportOptions();
    importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
    importOptions.materialMap.FallbackMasterMaterial = solidColorMat;

    var time = 1.0;
    var go = new GameObject();
    UsdAssetImporter.ImportUsd(go, ctx.assetPath, time, importOptions);

    var usdImporter = go.AddComponent<UsdAssetImporter>();
    usdImporter.m_usdFile = ctx.assetPath;
    usdImporter.m_time = time;
    usdImporter.OptionsToState(importOptions);

    var meshes = new HashSet<Mesh>();
    var materials = new HashSet<Material>();

    foreach (var mf in go.GetComponentsInChildren<MeshFilter>()) {
      if (meshes.Add(mf.sharedMesh)) {
        ctx.AddObjectToAsset(go.name, mf.sharedMesh);
      }
    }

    foreach (var mf in go.GetComponentsInChildren<MeshRenderer>()) {
      int matIndex = 0;
      foreach (var mat in mf.sharedMaterials) {
        if (!materials.Add(mat)) {
          continue;
        }
        ctx.AddObjectToAsset(mf.gameObject.name + "_mat_" + matIndex, mat);
      }
    }

    DestroyImmediate(go);
  }
}

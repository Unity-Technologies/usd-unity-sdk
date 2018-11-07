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

using System;
using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// Represents the point at which a UsdStage has been imported into the Unity scene.
  /// The goal is to make it easy to re-import the data in the future or export sparse overrides.
  /// </summary>
  public class StageRoot : MonoBehaviour {

    [Header("Source Asset")]
    public string m_usdFile;
    public double m_time;
    public Scene.InterpolationMode m_interpolation;

    [Header("Conversions")]
    public float m_scale;
    public BasisTransformation m_changeHandedness;

    [Header("Materials")]
    public MaterialImportMode m_materialImportMode = MaterialImportMode.ImportParameters;
    public bool m_enableGpuInstancing;
    public Material m_fallbackMaterial;

    [Header("Mesh Options")]
    public bool m_generateLightmapUVs;
    public ImportMode m_boundingBox;
    public ImportMode m_color;
    public ImportMode m_normals;
    public ImportMode m_tangents;
    public ImportMode m_texcoord0;
    public ImportMode m_texcoord1;
    public ImportMode m_texcoord2;
    public ImportMode m_texcoord3;

    /// <summary>
    /// Convert the SceneImportOptions into a serializable form.
    /// </summary>
    public void OptionsToState(SceneImportOptions options) {
      m_changeHandedness = options.changeHandedness;
      m_scale = options.scale;
      m_interpolation = options.interpolate ?
                        Scene.InterpolationMode.Linear :
                        Scene.InterpolationMode.Held;
      m_boundingBox = options.meshOptions.boundingBox;
      m_color = options.meshOptions.color;
      m_normals = options.meshOptions.normals;
      m_tangents = options.meshOptions.tangents;
      m_texcoord0 = options.meshOptions.texcoord0;
      m_texcoord1 = options.meshOptions.texcoord1;
      m_texcoord2 = options.meshOptions.texcoord2;
      m_texcoord3 = options.meshOptions.texcoord3;
      m_generateLightmapUVs = options.meshOptions.generateLightmapUVs;

      m_materialImportMode = options.materialImportMode;
      m_enableGpuInstancing = options.enableGpuInstancing;
      m_fallbackMaterial = options.materialMap.FallbackMasterMaterial;
    }

    /// <summary>
    /// Converts the current component state into import options.
    /// </summary>
    public void StateToOptions(ref SceneImportOptions options) {
      options.changeHandedness = m_changeHandedness;
      options.scale = m_scale;
      options.interpolate = m_interpolation == Scene.InterpolationMode.Linear;

      options.meshOptions.boundingBox = m_boundingBox;
      options.meshOptions.color = m_color;
      options.meshOptions.normals = m_normals;
      options.meshOptions.tangents = m_tangents;
      options.meshOptions.texcoord0 = m_texcoord0;
      options.meshOptions.texcoord1 = m_texcoord1;
      options.meshOptions.texcoord2 = m_texcoord2;
      options.meshOptions.texcoord3 = m_texcoord3;
      options.meshOptions.generateLightmapUVs = m_generateLightmapUVs;

      options.materialImportMode = m_materialImportMode;
      options.enableGpuInstancing = m_enableGpuInstancing;
      options.materialMap.FallbackMasterMaterial = m_fallbackMaterial;
    }

    public static void ImportUsd(GameObject goRoot,
                                 string usdFilePath,
                                 double time,
                                 SceneImportOptions importOptions) {
      Examples.InitUsd.Initialize();
      var scene = Scene.Open(usdFilePath);
      if (scene == null) {
        throw new Exception("Failed to open: " + usdFilePath);
      }
      scene.Time = time;
      try {
        ImportUsd(goRoot, scene, importOptions);
      } finally {
        scene.Close();
      }
    }

    public static void ImportUsd(GameObject goRoot,
                                 Scene scene,
                                 SceneImportOptions importOptions) {
      if (scene == null) {
        throw new Exception("Null USD Scene");
      }

      scene.SetInterpolation(importOptions.interpolate ?
                             Scene.InterpolationMode.Linear :
                             Scene.InterpolationMode.Held);

      SceneImporter.BuildScene(scene, goRoot, importOptions);
    }

  }
}

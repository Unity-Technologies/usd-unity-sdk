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
using System.Collections.Generic;
using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// Represents the point at which a UsdStage has been imported into the Unity scene.
  /// The goal is to make it easy to re-import the data in the future or export sparse overrides.
  /// </summary>
  [ExecuteInEditMode]
  public class StageRoot : MonoBehaviour {

    // Length of the USD playback time, exposed for Timeline.
    public double Length { get { return ComputeLength(); } }

    [Header("Source Asset")]
    public string m_usdFile;
    public float m_usdTime;
    public string m_assetImportPath;
    public Scene.InterpolationMode m_interpolation;

    [Header("Scenegraph")]
    public bool m_importCameras = true;
    public bool m_importMeshes = true;
    public bool m_importSkinning = true;
    public bool m_importHierarchy = true;
    public bool m_importTransforms = true;
    public bool m_importSceneInstances = true;
    public bool m_importPointInstances = true;

    [Header("Conversions")]
    public float m_scale;
    public BasisTransformation m_changeHandedness;

    [Header("Materials")]
    public MaterialImportMode m_materialImportMode = MaterialImportMode.ImportParameters;
    public bool m_enableGpuInstancing;
    public Material m_fallbackMaterial;
    public Material m_specularWorkflowMaterial;
    public Material m_metallicWorkflowMaterial;

    [Header("Mesh Options")]
    public ImportMode m_points;
    public ImportMode m_topology;
    public bool m_generateLightmapUVs;
    public ImportMode m_boundingBox;
    public ImportMode m_color;
    public ImportMode m_normals;
    public ImportMode m_tangents;
    public ImportMode m_texcoord0;
    public ImportMode m_texcoord1;
    public ImportMode m_texcoord2;
    public ImportMode m_texcoord3;

    [Header("Debug Options")]
    public bool m_debugShowSkeletonBindPose;
    public bool m_debugShowSkeletonRestPose;

    [HideInInspector]
    public BasisTransformation LastHandedness;
    [HideInInspector]
    public float LastScale;

    private float m_lastTime;
    private Scene m_lastScene;

    public Scene GetScene() {
      USD.NET.Examples.InitUsd.Initialize();
      if (m_lastScene == null || m_lastScene.Stage == null || m_lastScene.FilePath != m_usdFile) {
        m_lastScene = Scene.Open(m_usdFile);
      }
      m_lastScene.Time = m_usdTime;
      return m_lastScene;
    }

    #region "Timeline Support"
    private double ComputeLength() {
      var scene = GetScene();
      if (scene == null) { return 0; }
      return (scene.EndTime - scene.StartTime) / (scene.Stage.GetFramesPerSecond());
    }

    /// <summary>
    /// Applies the contents of this USD file to a foreign root object.
    /// </summary>
    /// <remarks>
    /// The idea here is that one may have many animation clips, but only a single GameObject in
    /// the Unity scenegraph.
    /// </remarks>
    public void SetTime(double time, StageRoot foreignRoot) {
      var scene = GetScene();
      if (scene == null) { return; }

      // Careful not to update any local members here, if this data is driven from a prefab, we
      // dont want those changes to be baked back into the asset.

      float usdTime = (float)(scene.StartTime + time * scene.Stage.GetFramesPerSecond());
      scene.Time = usdTime;
      var options = new SceneImportOptions();
      foreignRoot.StateToOptions(ref options);

      try {
        PrepOptionsForTimeChange(ref options);
        SceneImporter.ImportUsd(foreignRoot.gameObject, scene, options);
      } finally {
        scene.Close();
        m_lastScene = null;
      }
    }
    #endregion

    private void Update() {
      if (m_lastTime == m_usdTime) {
        return;
      }
      m_lastTime = m_usdTime;
      var scene = GetScene();
      var options = new SceneImportOptions();
      StateToOptions(ref options);

      try {
        PrepOptionsForTimeChange(ref options);
        SceneImporter.ImportUsd(this.gameObject, scene, options);
      } finally {
        scene.Close();
        m_lastScene = null;
      }
    }

    public static void PrepOptionsForTimeChange(ref SceneImportOptions options) {
      options.forceRebuild = false;
      options.materialImportMode = MaterialImportMode.None;
      options.meshOptions.debugShowSkeletonBindPose = false;
      options.meshOptions.debugShowSkeletonRestPose = false;
      options.meshOptions.generateLightmapUVs = false;

      // Note that tangent and Normals must be updated when the mesh deforms.
      options.importHierarchy = false;

      options.meshOptions.texcoord0 = ImportMode.Ignore;
      options.meshOptions.texcoord1 = ImportMode.Ignore;
      options.meshOptions.texcoord2 = ImportMode.Ignore;
      options.meshOptions.texcoord3 = ImportMode.Ignore;
    }

    /// <summary>
    /// Convert the SceneImportOptions into a serializable form.
    /// </summary>
    public void OptionsToState(SceneImportOptions options) {
      m_assetImportPath = options.assetImportPath;
      m_changeHandedness = options.changeHandedness;
      m_scale = options.scale;
      m_interpolation = options.interpolate ?
                        Scene.InterpolationMode.Linear :
                        Scene.InterpolationMode.Held;

      // Scenegraph options.
      m_importCameras = options.importCameras;
      m_importMeshes = options.importMeshes;
      m_importSkinning = options.importSkinning;
      m_importHierarchy = options.importHierarchy;
      m_importTransforms = options.importTransforms;
      m_importSceneInstances = options.importSceneInstances;
      m_importPointInstances = options.importPointInstances;

      // Mesh options.
      m_points = options.meshOptions.points;
      m_topology = options.meshOptions.topology;
      m_boundingBox = options.meshOptions.boundingBox;
      m_color = options.meshOptions.color;
      m_normals = options.meshOptions.normals;
      m_tangents = options.meshOptions.tangents;
      m_texcoord0 = options.meshOptions.texcoord0;
      m_texcoord1 = options.meshOptions.texcoord1;
      m_texcoord2 = options.meshOptions.texcoord2;
      m_texcoord3 = options.meshOptions.texcoord3;
      m_generateLightmapUVs = options.meshOptions.generateLightmapUVs;

      m_debugShowSkeletonBindPose = options.meshOptions.debugShowSkeletonBindPose;
      m_debugShowSkeletonRestPose = options.meshOptions.debugShowSkeletonRestPose;

      // Materials & instancing.
      m_materialImportMode = options.materialImportMode;
      m_enableGpuInstancing = options.enableGpuInstancing;
      m_fallbackMaterial = options.materialMap.FallbackMasterMaterial;
      m_specularWorkflowMaterial = options.materialMap.SpecularWorkflowMaterial;
      m_metallicWorkflowMaterial = options.materialMap.MetallicWorkflowMaterial;
    }

    /// <summary>
    /// Converts the current component state into import options.
    /// </summary>
    public void StateToOptions(ref SceneImportOptions options) {
      options.assetImportPath = m_assetImportPath;
      options.changeHandedness = m_changeHandedness;
      options.scale = m_scale;
      options.interpolate = m_interpolation == Scene.InterpolationMode.Linear;

      // Scenegraph options.
      options.importCameras = m_importCameras;
      options.importMeshes = m_importMeshes;
      options.importSkinning = m_importSkinning;
      options.importHierarchy = m_importHierarchy;
      options.importTransforms = m_importTransforms;
      options.importSceneInstances = m_importSceneInstances;
      options.importPointInstances = m_importPointInstances;

      // Mesh options.
      options.meshOptions.points = m_points;
      options.meshOptions.topology = m_topology;
      options.meshOptions.boundingBox = m_boundingBox;
      options.meshOptions.color = m_color;
      options.meshOptions.normals = m_normals;
      options.meshOptions.tangents = m_tangents;
      options.meshOptions.texcoord0 = m_texcoord0;
      options.meshOptions.texcoord1 = m_texcoord1;
      options.meshOptions.texcoord2 = m_texcoord2;
      options.meshOptions.texcoord3 = m_texcoord3;
      options.meshOptions.generateLightmapUVs = m_generateLightmapUVs;

      options.meshOptions.debugShowSkeletonBindPose = m_debugShowSkeletonBindPose;
      options.meshOptions.debugShowSkeletonRestPose = m_debugShowSkeletonRestPose;

      // Materials & Instancing.
      options.materialImportMode = m_materialImportMode;
      options.enableGpuInstancing = m_enableGpuInstancing;
      options.materialMap.FallbackMasterMaterial = m_fallbackMaterial;
      options.materialMap.SpecularWorkflowMaterial = m_specularWorkflowMaterial;
      options.materialMap.MetallicWorkflowMaterial = m_metallicWorkflowMaterial;
    }

    public void ImportUsdAsCoroutine(GameObject goRoot,
                                     string usdFilePath,
                                     double time,
                                     SceneImportOptions importOptions,
                                     float targetFrameMilliseconds) {
      Examples.InitUsd.Initialize();
      var scene = Scene.Open(usdFilePath);
      if (scene == null) {
        throw new Exception("Failed to open: " + usdFilePath);
      }

      scene.Time = time;
      if (scene == null) {
        throw new Exception("Null USD Scene");
      }

      scene.SetInterpolation(importOptions.interpolate ?
                             Scene.InterpolationMode.Linear :
                             Scene.InterpolationMode.Held);
      var primMap = new PrimMap();
      var importer = SceneImporter.BuildScene(scene,
                                              goRoot,
                                              pxr.SdfPath.AbsoluteRootPath(),
                                              importOptions,
                                              primMap,
                                              targetFrameMilliseconds,
                                              composingSubtree: false);
      StartCoroutine(importer);
    }

    public void SetVariantSelection(GameObject go,
                                    string usdPrimPath,
                                    Dictionary<string, string> selections) {
      Examples.InitUsd.Initialize();
      var scene = Scene.Open(m_usdFile);
      if (scene == null) {
        throw new Exception("Failed to open: " + m_usdFile);
      }

      var prim = scene.GetPrimAtPath(usdPrimPath);
      if (prim == null || !prim) {
        throw new Exception("Prim not found: " + usdPrimPath);
      }

      var varSets = prim.GetVariantSets();
      foreach (var sel in selections) {
        if (!varSets.HasVariantSet(sel.Key)) {
          throw new Exception("Unknown varient set: " + sel.Key + " at " + usdPrimPath);
        }
        varSets.GetVariantSet(sel.Key).SetVariantSelection(sel.Value);
      }

      // TODO: sparsely remove prims, rather than blowing away all the children.
      foreach (Transform child in go.transform) {
        GameObject.DestroyImmediate(child.gameObject);
      }

      SceneImportOptions importOptions = new SceneImportOptions();
      this.StateToOptions(ref importOptions);
      try {
        SceneImporter.ImportUsd(go, scene, usdPrimPath, importOptions);
      } finally {
        scene.Close();
      }
    }
  }
}

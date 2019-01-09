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
using System.Linq;
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
    public string m_projectAssetPath = "Assets/";
    public string m_usdRootPath = "/";
    public PayloadPolicy m_payloadPolicy = PayloadPolicy.DontLoadPayloads;
    public float m_usdTimeOffset;
    public Scene.InterpolationMode m_interpolation;
    public bool m_usdVariabilityCache = true;

    [HideInInspector]
    public bool m_importHierarchy = true;

    [Header("Conversions")]
    public float m_scale;
    public BasisTransformation m_changeHandedness;

    [Header("Materials")]
    public MaterialImportMode m_materialImportMode = MaterialImportMode.ImportDisplayColor;
    public bool m_enableGpuInstancing;
    public Material m_fallbackMaterial;
    public Material m_specularWorkflowMaterial;
    public Material m_metallicWorkflowMaterial;

    [Header("Scenegraph")]
    public bool m_importCameras = true;
    public bool m_importMeshes = true;
    public bool m_importSkinning = true;
    public bool m_importTransforms = true;
    public bool m_importSceneInstances = true;
    public bool m_importPointInstances = true;

    [Header("Mesh Options")]
    public bool m_generateLightmapUVs;
    public ImportMode m_points;
    public ImportMode m_topology;
    public ImportMode m_boundingBox;
    public ImportMode m_color;
    public ImportMode m_normals;
    public ImportMode m_tangents;

    [HideInInspector]
    public ImportMode m_texcoord0;
    [HideInInspector]
    public ImportMode m_texcoord1;
    [HideInInspector]
    public ImportMode m_texcoord2;
    [HideInInspector]
    public ImportMode m_texcoord3;

    [Header("Debug Options")]
    public bool m_debugShowSkeletonBindPose;
    public bool m_debugShowSkeletonRestPose;
    public bool m_debugPrintVariabilityCache;

    [HideInInspector]
    public BasisTransformation LastHandedness;
    [HideInInspector]
    public float LastScale;

    private float m_lastTime;
    private Scene m_lastScene;
    private PrimMap m_lastPrimMap = null;
    private AccessMask m_lastAccessMask = null;

#if UNITY_EDITOR
    [SerializeField]
    private int m_instanceId = 0;

    void Awake() {
      if (m_instanceId != GetInstanceID()) {
        if (m_instanceId == 0) {
          m_instanceId = GetInstanceID();
        } else {
          m_instanceId = GetInstanceID();
          if (m_instanceId < 0) {
            Debug.Log("Reimporting " + name + " after duplicate");
            Reload(true);
          }
        }
      }
    }
#endif

    /// <summary>
    /// Convert the SceneImportOptions into a serializable form.
    /// </summary>
    public void OptionsToState(SceneImportOptions options) {
      m_usdRootPath = options.usdRootPath;
      m_projectAssetPath = options.projectAssetPath;
      m_changeHandedness = options.changeHandedness;
      m_scale = options.scale;
      m_interpolation = options.interpolate ?
                        Scene.InterpolationMode.Linear :
                        Scene.InterpolationMode.Held;

      // Scenegraph options.
      m_payloadPolicy = options.payloadPolicy;
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
      options.usdRootPath = new pxr.SdfPath(m_usdRootPath);
      options.projectAssetPath = m_projectAssetPath;
      options.changeHandedness = m_changeHandedness;
      options.scale = m_scale;
      options.interpolate = m_interpolation == Scene.InterpolationMode.Linear;

      // Scenegraph options.
      options.payloadPolicy = m_payloadPolicy;

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

    /// <summary>
    /// Returns the USD.NET.Scene object for this USD file.
    /// The caller is NOT expected to close the scene.
    /// </summary>
    public Scene GetScene() {
      USD.NET.Examples.InitUsd.Initialize();
      if (m_lastScene == null || m_lastScene.Stage == null || m_lastScene.FilePath != m_usdFile) {
        pxr.UsdStage stage = null;
        if (m_payloadPolicy == PayloadPolicy.DontLoadPayloads) {
          stage = pxr.UsdStage.Open(m_usdFile, pxr.UsdStage.InitialLoadSet.LoadNone);
        } else {
          stage = pxr.UsdStage.Open(m_usdFile, pxr.UsdStage.InitialLoadSet.LoadAll);
        }

        m_lastScene = Scene.Open(stage);
        m_lastPrimMap = null;
        m_lastAccessMask = null;


        // TODO: This is potentially horrible in terms of performance, LoadAndUnload should be used
        // instead, but the binding is not complete.
        foreach (var payload in GetComponentsInParent<UsdPayload>()) {
          var primSrc = payload.GetComponent<UsdPrimSource>();
          if (payload.IsLoaded && m_payloadPolicy == PayloadPolicy.DontLoadPayloads) {
            var prim = m_lastScene.GetPrimAtPath(primSrc.m_usdPrimPath);
            if (prim == null || !prim) { continue; }
            prim.Load();
          } else {
            var prim = m_lastScene.GetPrimAtPath(primSrc.m_usdPrimPath);
            if (prim == null || !prim) { continue; }
            prim.Unload();
          }
        }

        // Re-apply variant selection state, similar to prim load state.
        foreach (var variants in GetComponentsInChildren<UsdVariantSet>()) {
          ApplyVariantSelectionState(m_lastScene, variants);
        }
      }

      m_lastScene.Time = m_usdTimeOffset;
      m_lastScene.SetInterpolation(m_interpolation);
      return m_lastScene;
    }

    private void OnReload() {
      m_lastPrimMap = null;
      m_lastAccessMask = null;
      if (m_lastScene != null) {
        m_lastScene.Close();
        m_lastScene = null;
      }
    }

    public void OpenScene(Scene scene) {
      var options = new SceneImportOptions();
      StateToOptions(ref options);
      var parent = gameObject.transform.parent;
      var root = parent ? parent.gameObject : null;

      scene.Time = m_usdTimeOffset;
      try {
        OnReload();
        SceneImporter.ImportUsd(root, scene, new PrimMap(), options);
      } finally {
        scene.Close();
      }
    }

    public void Reload(bool forceRebuild) {
      var options = new SceneImportOptions();
      StateToOptions(ref options);

      options.forceRebuild = forceRebuild;

      if (string.IsNullOrEmpty(options.projectAssetPath)) {
        options.projectAssetPath = "Assets/";
        OptionsToState(options);
      }

      var root = gameObject;

      string assetPath =
#if UNITY_EDITOR
          UnityEditor.AssetDatabase.GetAssetPath(
              UnityEditor.PrefabUtility.GetPrefabObject(root));
#else
          null;
#endif

      // The prefab asset path will be null for prefab instances.
      // When the assetPath is not null, the object is the prefab itself.
      if (!string.IsNullOrEmpty(assetPath)) {
        if (options.forceRebuild) {
          root = new GameObject();
        }
        string clipName = System.IO.Path.GetFileNameWithoutExtension(m_usdFile);
        SceneImporter.ImportUsd(root, GetScene(), new PrimMap(), options);
        SceneImporter.SavePrefab(root, assetPath, clipName, options);
        if (options.forceRebuild) {
          GameObject.DestroyImmediate(root);
        }
      } else {
        // An instance of a prefab or a vanilla game object.
        // Just reload the scene into memory and let the user decide if they want to send those
        // changes back to the prefab or not.

        if (forceRebuild) {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
          // First, destroy all existing USD game objects.
          foreach (var src in root.GetComponentsInChildren<UsdPrimSource>(includeInactive: true)) {
            if (src) {
              GameObject.DestroyImmediate(src.gameObject);
            }
          }
#endif
        }

          m_lastScene = null;
          m_lastPrimMap = null;
        m_lastAccessMask = null;

        SceneImporter.ImportUsd(root, GetScene(), new PrimMap(), options);
      }
    }

    pxr.UsdPrim GetFirstPrim(Scene scene) {
      var children = scene.Stage.GetPseudoRoot().GetAllChildren().GetEnumerator();
      if (!children.MoveNext()) {
        return null;
      }
      return children.Current;
    }

    public void ExportOverrides(Scene sceneInWhichToStoreTransforms) {
      var sceneToReference = this;
      var overs = sceneInWhichToStoreTransforms;

      if (overs == null) {
        return;
      }

      var baseLayer = sceneToReference.GetScene();
      if (baseLayer == null) {
        throw new Exception("Could not open base layer: " + sceneToReference.m_usdFile);
      }

      overs.Time = baseLayer.Time;
      overs.StartTime = baseLayer.StartTime;
      overs.EndTime = baseLayer.EndTime;

      overs.WriteMode = Scene.WriteModes.Over;
      overs.UpAxis = baseLayer.UpAxis;

      try {
        SceneExporter.Export(sceneToReference.gameObject,
                             overs,
                             BasisTransformation.SlowAndSafe,
                             exportUnvarying: false,
                             zeroRootTransform: true);

        var rel = ImporterBase.MakeRelativePath(overs.FilePath, sceneToReference.m_usdFile);
        GetFirstPrim(overs).GetReferences().AddReference(rel, GetFirstPrim(baseLayer).GetPath());
      } catch (System.Exception ex) {
        Debug.LogException(ex);
        return;
      } finally {
        if (overs != null) {
          overs.Save();
          overs.Close();
        }
      }
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
      if (scene == null) {
        Debug.LogWarning("Null scene from GetScene() at " + UnityTypeConverter.GetPath(transform));
        return;
      }

      // Careful not to update any local members here, if this data is driven from a prefab, we
      // dont want those changes to be baked back into the asset.
      time += foreignRoot.m_usdTimeOffset;
      float usdTime = (float)(scene.StartTime + time * scene.Stage.GetFramesPerSecond());
      if (usdTime > scene.EndTime) { return; }
      if (usdTime < scene.StartTime) { return; }

      scene.Time = usdTime;

      var options = new SceneImportOptions();
      foreignRoot.StateToOptions(ref options);

      PrepOptionsForTimeChange(ref options);
      if (foreignRoot.m_lastPrimMap == null) {
        foreignRoot.m_lastPrimMap = new PrimMap();
        options.importHierarchy = true;
      }

      if (m_usdVariabilityCache) {
        if (m_lastAccessMask == null) {
          m_lastAccessMask = new AccessMask();
          scene.IsPopulatingAccessMask = true;
        }
      } else {
        m_lastAccessMask = null;
      }

      if (m_debugPrintVariabilityCache && m_lastAccessMask != null
          && !scene.IsPopulatingAccessMask) {
        var sb = new System.Text.StringBuilder();
        foreach (var kvp in m_lastAccessMask.Included) {
          sb.AppendLine(kvp.Key);
          foreach (var member in kvp.Value) {
            sb.AppendLine("  ." + member.Name);
          }
          sb.AppendLine();
        }
        Debug.Log(sb.ToString());
      }

      scene.AccessMask = m_lastAccessMask;
      SceneImporter.ImportUsd(foreignRoot.gameObject,
                              scene,
                              foreignRoot.m_lastPrimMap,
                              options);
      scene.AccessMask = null;

      if (m_lastAccessMask != null) {
        scene.IsPopulatingAccessMask = false;
      }
    }
    #endregion

    private void Update() {
      if (m_lastTime == m_usdTimeOffset) {
        return;
      }
      m_lastTime = m_usdTimeOffset;
      SetTime(m_usdTimeOffset, this);
    }

    public static void PrepOptionsForTimeChange(ref SceneImportOptions options) {
      options.forceRebuild = false;
      options.materialImportMode = MaterialImportMode.None;
      options.meshOptions.debugShowSkeletonBindPose = false;
      options.meshOptions.debugShowSkeletonRestPose = false;

      options.meshOptions.generateLightmapUVs = false;
      options.importSkinWeights = false;

      // Note that tangent and Normals must be updated when the mesh deforms.
      options.importHierarchy = false;

      options.meshOptions.texcoord0 = ImportMode.Ignore;
      options.meshOptions.texcoord1 = ImportMode.Ignore;
      options.meshOptions.texcoord2 = ImportMode.Ignore;
      options.meshOptions.texcoord3 = ImportMode.Ignore;
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
                                              importOptions,
                                              primMap,
                                              targetFrameMilliseconds,
                                              composingSubtree: false);
      StartCoroutine(importer);
    }

    public void SetPayloadState(GameObject go, bool isLoaded) {
      var primSrc = go.GetComponent<UsdPrimSource>();
      if (!primSrc) {
        throw new Exception("UsdPrimSource not found: " + UnityTypeConverter.GetPath(go.transform));
      }
      var usdPrimPath = primSrc.m_usdPrimPath;

      Examples.InitUsd.Initialize();
      var scene = GetScene();
      if (scene == null) {
        throw new Exception("Failed to open: " + m_usdFile);
      }

      var prim = scene.GetPrimAtPath(usdPrimPath);
      if (prim == null || !prim) {
        throw new Exception("Prim not found: " + usdPrimPath);
      }

      foreach (var child in go.transform.GetComponentsInChildren<UsdPrimSource>().ToList()) {
        if (!child || child.gameObject == go) { continue; }
        GameObject.DestroyImmediate(child.gameObject);
      }

      if (!isLoaded) {
        prim.Unload();
        return;
      } else {
        prim.Load();
      }

      SceneImportOptions importOptions = new SceneImportOptions();
      this.StateToOptions(ref importOptions);
      importOptions.usdRootPath = prim.GetPath();
      SceneImporter.ImportUsd(go, scene, new PrimMap(), true, importOptions);
    }

    private void ApplyVariantSelectionState(Scene scene, UsdVariantSet variants) {
      var selections = variants.GetVariantSelections();
      var path = variants.GetComponent<UsdPrimSource>().m_usdPrimPath;
      var prim = scene.GetPrimAtPath(path);
      var varSets = prim.GetVariantSets();
      foreach (var sel in selections) {
        if (!varSets.HasVariantSet(sel.Key)) {
          throw new Exception("Unknown varient set: " + sel.Key + " at " + path);
        }
        varSets.GetVariantSet(sel.Key).SetVariantSelection(sel.Value);
      }
    }

    public void SetVariantSelection(GameObject go,
                                    string usdPrimPath,
                                    Dictionary<string, string> selections) {
      Examples.InitUsd.Initialize();
      var scene = GetScene();
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
      importOptions.usdRootPath = prim.GetPath();
      SceneImporter.ImportUsd(go, scene, new PrimMap(), true, importOptions);
    }
  }
}

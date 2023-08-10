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
using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Unity.Formats.USD
{
    /// <summary>
    /// Represents the point at which a UsdStage has been imported into the Unity scene.
    /// The goal is to make it easy to re-import the data and to export sparse overrides.
    /// </summary>
    [ExecuteInEditMode]
    public class UsdAsset : MonoBehaviour
    {
        /// <summary>
        /// The length of the USD playback time in seconds.
        /// </summary>
        public double Length
        {
            get { return ComputeLength(); }
        }

        /// <summary>
        /// The absolute file path to the USD file from which this asset was created. This path may
        /// point to a location outside of the Unity project and may be any file type supported by
        /// USD (e.g. usd, usda, usdc, abc, ...). Setting this path will not trigger the asset to be
        /// reimported, Reload must be called explicitly.
        /// </summary>
        public string usdFullPath
        {
            get { return string.IsNullOrEmpty(m_usdFile) ? string.Empty : (Path.GetFullPath(m_usdFile)); }
            set { m_usdFile = value; }
        }

        // ----------------------------------------------------------------------------------------- //
        // Source Asset.
        // ----------------------------------------------------------------------------------------- //

        [Header("Source Asset")]
        [SerializeField]
        string m_usdFile;

        [HideInInspector]
        [Tooltip("The Unity project path into which imported files (such as textures) will be placed.")]
        public string m_projectAssetPath = "Assets/";

        [Tooltip("The USD prim path in the USD scene at which to start the import process.")]
        public string m_usdRootPath = "/";

        [Tooltip("An offset applied to all data in the USD file")]
        public float m_usdTimeOffset;

        [Tooltip("For assets with payloads authored, indicates if payloads should be loaded or unloaded by default.")]
        public PayloadPolicy m_payloadPolicy = PayloadPolicy.DontLoadPayloads;

        [HideInInspector] public bool m_importHierarchy = true;

        // ----------------------------------------------------------------------------------------- //
        // Conversions.
        // ----------------------------------------------------------------------------------------- //

        [Header("Conversions")]
        [Tooltip("A scale to be applied to the root asset, useful for converting asset units to meters.")]
        public float m_scale;

        // See enum for details.
        [Tooltip("Conversion method for right-handed (USD) to left-handed conversion (Unity) and vice versa.")]
        public BasisTransformation m_changeHandedness;

        [Tooltip("Behavior to use when no value was authored at the requested time.")]
        public Scene.InterpolationMode m_interpolation;

        // ----------------------------------------------------------------------------------------- //
        // Material Options.
        // ----------------------------------------------------------------------------------------- //

        [Header("Material Options")]
        [Tooltip("If the original shader name is stored in USD, attempt to find that shader in this project.")]
        public bool m_useOriginalShaderIfAvailable = true;

        [Tooltip("The default material to use when importing materials as display color.")]
        public Material m_displayColorMaterial;

        [Tooltip("The default material to use when importing specular workflow USD Preview Surface materials.")]
        public Material m_specularWorkflowMaterial;

        [Tooltip("The default material to use when importing metallic workflow USD Preview Surface materials.")]
        public Material m_metallicWorkflowMaterial;

        [HideInInspector]
        [Tooltip("When enabled, set the GPU Instancing flag on all materials.")]
        public bool m_enableGpuInstancing;

        // ----------------------------------------------------------------------------------------- //
        // Mesh Options.
        // ----------------------------------------------------------------------------------------- //

        [Header("Mesh Options")] public ImportMode m_points;
        public ImportMode m_topology;
        public ImportMode m_boundingBox;

        [Tooltip("Combined import policy for primvars:displayColor and primvars:displayOpacity")]
        public ImportMode m_color;

        [Tooltip("Import policy for normals, note that the 'normals' attribute is built-in, not a primvar")]
        public ImportMode m_normals;

        [Tooltip("Import policy for primvars:tangent")]
        public ImportMode m_tangents;

        // ----------------------------------------------------------------------------------------- //
        // Lightmap UV Unwrapping.
        // ----------------------------------------------------------------------------------------- //

        [Header("Mesh Lightmap UV Unwrapping")]
        public bool m_generateLightmapUVs;

        [Tooltip("Maximum allowed angle distortion")]
        [Range(0, 1)]
        public float m_unwrapAngleError = .08f;

        [Tooltip("Maximum allowed area distortion")]
        [Range(0, 1)]
        public float m_unwrapAreaError = .15f;

        [Tooltip("This angle (in degrees) or greater between triangles will cause seam to be created")]
        [Range(1, 359)]
        public float m_unwrapHardAngle = 88;

        [Tooltip("UV-island padding in pixels")]
        [Range(0, 32)]
        public int m_unwrapPackMargin = 4;

        // ----------------------------------------------------------------------------------------- //
        // Import Settings.
        // ----------------------------------------------------------------------------------------- //

        [Header("Import Settings")]
        public MaterialImportMode m_materialImportMode = MaterialImportMode.ImportDisplayColor;

        public bool m_importCameras = true;
        public bool m_importMeshes = true;
        public bool m_importSkinning = true;
        public bool m_importTransforms = true;
        public bool m_importSceneInstances = true;
        public bool m_importPointInstances = true;
        public bool m_importMonoBehaviors = false;

#if false
        [Header("Export Settings")]
        public bool m_exportCameras = true;
        public bool m_exportMeshes = true;
        public bool m_exportSkinning = true;
        public bool m_exportTransforms = true;
        public bool m_exportSceneInstances = true;
        public bool m_exportPointInstances = true;
        public bool m_exportMonoBehaviors = true;
#endif

        // ----------------------------------------------------------------------------------------- //
        // Debug Options.
        // ----------------------------------------------------------------------------------------- //

        [Header("Debug Options")] public bool m_debugShowSkeletonBindPose;
        public bool m_debugShowSkeletonRestPose;
        public bool m_debugPrintVariabilityCache;

        [Tooltip("Memorizes which attributes change over time, to speed up playback (trades time for memory)")]
        public bool m_usdVariabilityCache = true;

        [HideInInspector] public BasisTransformation LastHandedness;
        [HideInInspector] public float LastScale;

        // ----------------------------------------------------------------------------------------- //
        // Private fields.
        // ----------------------------------------------------------------------------------------- //

        private float m_lastTime;
        private Scene m_lastScene;
        private PrimMap m_lastPrimMap = null;
        private AccessMask m_lastAccessMask = null;

#if UNITY_EDITOR
        /// <summary>
        /// Returns the underlying prefab object, or null.
        /// </summary>
        private GameObject GetPrefabObject(GameObject root)
        {
            // This is a great resource for determining object type, but only covers new APIs:
            // https://github.com/Unity-Technologies/UniteLA2018Examples/blob/master/Assets/Scripts/GameObjectTypeLogging.cs
            return UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(root);
        }

#endif

        private void OnDestroy()
        {
            if (m_lastScene != null)
            {
                m_lastScene.Close();
                m_lastScene = null;
            }
        }

        /// <summary>
        /// Returns the project path to the prefab, or null. Always returns null in player.
        /// </summary>
        private string GetPrefabAssetPath(GameObject root)
        {
            string assetPath = null;
#if UNITY_EDITOR
            if (!UnityEditor.EditorUtility.IsPersistent(root))
            {
#if UNITY_2021_2_OR_NEWER
                var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(root);
#else
                var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(root);
#endif
                if (prefabStage != null)
                {
                    if (!UnityEditor.PrefabUtility.IsPartOfPrefabInstance(root))
                    {
#if UNITY_2020_1_OR_NEWER
                        assetPath = prefabStage.assetPath;
#else
                        assetPath = prefabStage.prefabAssetPath;
#endif
                        // This is a great resource for determining object type, but only covers new APIs:
                        // https://github.com/Unity-Technologies/UniteLA2018Examples/blob/master/Assets/Scripts/GameObjectTypeLogging.cs
                    }
                }
            }
#endif
            return assetPath;
        }

        /// <summary>
        /// Convert the SceneImportOptions to a serializable form.
        /// </summary>
        public void OptionsToState(SceneImportOptions options)
        {
            m_usdRootPath = options.usdRootPath;
            m_projectAssetPath = options.projectAssetPath;
            m_changeHandedness = options.changeHandedness;
            m_scale = options.scale;
            m_interpolation = options.interpolate ? Scene.InterpolationMode.Linear : Scene.InterpolationMode.Held;

            // Scenegraph options.
            m_payloadPolicy = options.payloadPolicy;
            m_importCameras = options.importCameras;
            m_importMeshes = options.importMeshes;
            m_importSkinning = options.importSkinning;
            m_importHierarchy = options.importHierarchy;
            m_importTransforms = options.importTransforms;
            m_importSceneInstances = options.importSceneInstances;
            m_importPointInstances = options.importPointInstances;
            m_importMonoBehaviors = options.importMonoBehaviours;

            // Mesh options.
            m_points = options.meshOptions.points;
            m_topology = options.meshOptions.topology;
            m_boundingBox = options.meshOptions.boundingBox;
            m_color = options.meshOptions.color;
            m_normals = options.meshOptions.normals;
            m_tangents = options.meshOptions.tangents;
            m_generateLightmapUVs = options.meshOptions.generateLightmapUVs;

            m_unwrapAngleError = options.meshOptions.unwrapAngleError;
            m_unwrapAreaError = options.meshOptions.unwrapAreaError;
            m_unwrapHardAngle = options.meshOptions.unwrapHardAngle;
            m_unwrapPackMargin = options.meshOptions.unwrapPackMargin;

            m_debugShowSkeletonBindPose = options.meshOptions.debugShowSkeletonBindPose;
            m_debugShowSkeletonRestPose = options.meshOptions.debugShowSkeletonRestPose;

            // Materials & instancing.
            m_useOriginalShaderIfAvailable = options.materialMap.useOriginalShaderIfAvailable;
            m_materialImportMode = options.materialImportMode;
            m_enableGpuInstancing = options.enableGpuInstancing;
            m_displayColorMaterial = options.materialMap.DisplayColorMaterial;
            m_specularWorkflowMaterial = options.materialMap.SpecularWorkflowMaterial;
            m_metallicWorkflowMaterial = options.materialMap.MetallicWorkflowMaterial;
        }

        /// <summary>
        /// Converts the current component state into import options.
        /// </summary>
        public void StateToOptions(ref SceneImportOptions options)
        {
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
            options.importMonoBehaviours = m_importMonoBehaviors;

            // Mesh options.
            options.meshOptions.points = m_points;
            options.meshOptions.topology = m_topology;
            options.meshOptions.boundingBox = m_boundingBox;
            options.meshOptions.color = m_color;
            options.meshOptions.normals = m_normals;
            options.meshOptions.tangents = m_tangents;
            options.meshOptions.generateLightmapUVs = m_generateLightmapUVs;

            options.meshOptions.unwrapAngleError = m_unwrapAngleError;
            options.meshOptions.unwrapAreaError = m_unwrapAreaError;
            options.meshOptions.unwrapHardAngle = m_unwrapHardAngle;
            options.meshOptions.unwrapPackMargin = m_unwrapPackMargin;

            options.meshOptions.debugShowSkeletonBindPose = m_debugShowSkeletonBindPose;
            options.meshOptions.debugShowSkeletonRestPose = m_debugShowSkeletonRestPose;

            // Materials & Instancing.
            options.materialMap.useOriginalShaderIfAvailable = m_useOriginalShaderIfAvailable;
            options.materialImportMode = m_materialImportMode;
            options.enableGpuInstancing = m_enableGpuInstancing;
            options.materialMap.DisplayColorMaterial = m_displayColorMaterial;
            options.materialMap.SpecularWorkflowMaterial = m_specularWorkflowMaterial;
            options.materialMap.MetallicWorkflowMaterial = m_metallicWorkflowMaterial;
        }

        private bool SceneFileChanged()
        {
            if (m_lastScene == null)
            {
                return true;
            }

            return Path.GetFullPath(m_lastScene.FilePath).ToLower().Replace("\\", "/")
                != usdFullPath.ToLower().Replace("\\", "/");
        }

        /// <summary>
        /// Returns the USD.NET.Scene object for this USD file.
        /// The caller is NOT expected to close the scene.
        /// </summary>
        public Scene GetScene()
        {
            if (!InitUsd.Initialize())
                return null;

            if (m_lastScene?.Stage == null || SceneFileChanged())
            {
                pxr.UsdStage stage = null;
                if (string.IsNullOrEmpty(usdFullPath))
                {
                    return null;
                }

                if (m_payloadPolicy == PayloadPolicy.DontLoadPayloads)
                {
                    stage = pxr.UsdStage.Open(usdFullPath, pxr.UsdStage.InitialLoadSet.LoadNone);
                }
                else
                {
                    stage = pxr.UsdStage.Open(usdFullPath, pxr.UsdStage.InitialLoadSet.LoadAll);
                }

                stage.Reload(); // Ensure the stage is reloaded in case it was already opened and cached.
                m_lastScene = Scene.Open(stage);
                m_lastPrimMap = null;
                m_lastAccessMask = null;

                // TODO: This is potentially horrible in terms of performance, LoadAndUnload should be used
                // instead, but the binding is not complete.
                foreach (var payload in GetComponentsInParent<UsdPayload>())
                {
                    var primSrc = payload.GetComponent<UsdPrimSource>();
                    if (payload.IsLoaded && m_payloadPolicy == PayloadPolicy.DontLoadPayloads)
                    {
                        var prim = m_lastScene.GetPrimAtPath(primSrc.m_usdPrimPath);
                        if (prim == null || !prim)
                        {
                            continue;
                        }

                        prim.Load();
                    }
                    else
                    {
                        var prim = m_lastScene.GetPrimAtPath(primSrc.m_usdPrimPath);
                        if (prim == null || !prim)
                        {
                            continue;
                        }

                        prim.Unload();
                    }
                }

                // Re-apply variant selection state, similar to prim load state.
                foreach (var variants in GetComponentsInChildren<UsdVariantSet>())
                {
                    ApplyVariantSelectionState(m_lastScene, variants);
                }
            }

            m_lastScene.Time = m_usdTimeOffset;
            m_lastScene.SetInterpolation(m_interpolation);
            return m_lastScene;
        }

        /// <summary>
        /// A private event that fires whenever the USD scene was reimported.
        /// </summary>
        private void OnReload()
        {
            m_lastPrimMap = null;
            m_lastAccessMask = null;
            if (m_lastScene != null)
            {
                m_lastScene.Close();
                m_lastScene = null;
            }
        }

        /// <summary>
        /// A null-safe way of destroying components.
        /// </summary>
        private void DestroyComponent(Component comp)
        {
            if (!comp)
            {
                return;
            }

            Component.DestroyImmediate(comp, true);
        }

        /// <summary>
        /// Clear internal data.
        /// Call to <see cref="GetScene">GetScene()</see> to update them with the latest USD data.
        /// </summary>
        private void ClearLastData()
        {
            m_lastScene = null;
            m_lastPrimMap = null;
            m_lastAccessMask = null;
        }

        /// <summary>
        /// Finds all USD behaviors and destroys them, ignores the GameObject and other components.
        /// </summary>
        public void RemoveAllUsdComponents()
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(this, "Remove USD components");
#endif
            foreach (var src in GetComponentsInChildren<UsdPrimSource>(includeInactive: true))
            {
                if (src)
                {
                    var go = src.gameObject;
                    DestroyComponent(go.GetComponent<UsdAssemblyRoot>());
                    DestroyComponent(go.GetComponent<UsdAsset>());
                    DestroyComponent(go.GetComponent<UsdLayerStack>());
                    DestroyComponent(go.GetComponent<UsdModelRoot>());
                    DestroyComponent(go.GetComponent<UsdPayload>());
                    DestroyComponent(go.GetComponent<UsdVariantSet>());
                    DestroyComponent(go.GetComponent<UsdPrimSource>());
                }
            }
        }

        /// <summary>
        /// Finds and destroys all GameObjects that were imported from USD.
        /// </summary>
        public void DestroyAllImportedObjects()
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(this, "Delete USD imported objects");
#endif
            foreach (var src in GetComponentsInChildren<UsdPrimSource>(includeInactive: true))
            {
                // Remove the object if it is valid, but never remove the UsdAsset root GameObject, which
                // is this.gameObject. The current workflow is to remove all objects EXCEPT the root, so
                // stubs of USD Assets can be left in the scene in the scene and imported only as needed.
                if (src && src.gameObject != this.gameObject)
                {
                    GameObject.DestroyImmediate(src.gameObject, true);
                }
            }
        }

        /// <summary>
        /// Reimports the USD scene, either fully rebuilding every object or updating them in-place.
        /// </summary>
        /// <param name="forceRebuild">Destroys each GameObject before reimporting.</param>
        public void Reload(bool forceRebuild)
        {
            var options = new SceneImportOptions();
            StateToOptions(ref options);

            options.forceRebuild = forceRebuild;
            options.ImportType = forceRebuild ? ImportType.ForceRebuild : ImportType.Refresh;

            if (string.IsNullOrEmpty(options.projectAssetPath))
            {
                options.projectAssetPath = "Assets/";
                OptionsToState(options);
            }

            var root = gameObject;

            string assetPath = GetPrefabAssetPath(root);

            // The prefab asset path will be null for prefab instances.
            // When the assetPath is not null, the object is the prefab itself.
            if (!string.IsNullOrEmpty(assetPath))
            {
                if (options.forceRebuild)
                {
                    DestroyAllImportedObjects();
                }

                pxr.UsdStageLoadRules.Rule activeLoadRule = m_lastScene.Stage.GetLoadRules().GetEffectiveRuleForPath(new pxr.SdfPath("/"));
                if ((activeLoadRule == pxr.UsdStageLoadRules.Rule.AllRule && options.payloadPolicy == PayloadPolicy.DontLoadPayloads)
                    || (activeLoadRule == pxr.UsdStageLoadRules.Rule.NoneRule && options.payloadPolicy == PayloadPolicy.LoadAll))
                {
                    ClearLastData();
                }
                SceneImporter.ImportUsd(root, GetScene(), new PrimMap(), options);

#if UNITY_EDITOR
                string clipName = Path.GetFileNameWithoutExtension(usdFullPath);
                // As an optimization, we could detect if any meshes or materials were created and only
                // rebuild the prefab in those cases.
                SceneImporter.SavePrefab(root, assetPath, clipName, options);
#endif
            }
            else
            {
                // An instance of a prefab or a vanilla game object.
                // Just reload the scene into memory and let the user decide if they want to send those
                // changes back to the prefab or not.

                if (forceRebuild)
                {
                    // First, destroy all existing USD game objects.
                    DestroyAllImportedObjects();
                }

                ClearLastData();
                SceneImporter.ImportUsd(root, GetScene(), new PrimMap(), options);
            }
        }

        /// <summary>
        /// Returns the first Prim on the USD stage or null.
        /// </summary>
        pxr.UsdPrim GetFirstPrim(Scene scene)
        {
            var children = scene.Stage.GetPseudoRoot().GetAllChildren().GetEnumerator();
            if (!children.MoveNext())
            {
                return null;
            }

            return children.Current;
        }

        /// <summary>
        /// Writes overrides over the given scene. The given scene is referenced into the override
        /// scene being exported.
        /// </summary>
        /// <param name="sceneInWhichToStoreTransforms"></param>
        public void ExportOverrides(Scene sceneInWhichToStoreTransforms)
        {
            var sceneToReference = this;
            var overs = sceneInWhichToStoreTransforms;

            if (overs == null)
            {
                UsdEditorAnalytics.SendExportEvent(Path.GetExtension(sceneToReference.usdFullPath), 0, false, onlyOverrides: true);
                return;
            }

            bool success = false;

            Stopwatch analyticsTimer = new Stopwatch();
            analyticsTimer.Start();
            var baseLayer = sceneToReference.GetScene();
            if (baseLayer == null)
            {
                analyticsTimer.Stop();
                UsdEditorAnalytics.SendExportEvent(Path.GetExtension(sceneToReference.usdFullPath), analyticsTimer.Elapsed.TotalMilliseconds, success, onlyOverrides: true);
                throw new Exception("Could not open base layer: " + sceneToReference.usdFullPath);
            }
            overs.AddSubLayer(baseLayer);

            overs.Time = baseLayer.Time;
            overs.StartTime = baseLayer.StartTime;
            overs.EndTime = baseLayer.EndTime;

            overs.WriteMode = Scene.WriteModes.Over;
            overs.UpAxis = baseLayer.UpAxis;

            try
            {
                SceneExporter.Export(sceneToReference.gameObject,
                    overs,
                    BasisTransformation.SlowAndSafe,
                    exportUnvarying: false,
                    zeroRootTransform: true,
                    exportOverrides: true);
                success = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                success = false;
            }
            finally
            {
                if (overs != null)
                {
                    // Remove the reference to the original USD from the override file for flexibility in an asset pipeline
                    // TODO: Make this an optional setting
                    overs.Stage.GetRootLayer().GetSubLayerPaths().Erase(0);
                    overs.Save();
                    overs.Close();
                }
                else
                {
                    success = false;
                }

                analyticsTimer.Stop();
                UsdEditorAnalytics.SendExportEvent(Path.GetExtension(sceneToReference.usdFullPath), analyticsTimer.Elapsed.TotalMilliseconds, success, onlyOverrides: true);
            }
        }

        /// <summary>
        /// Computes the playback length of the USD scene in seconds.
        /// </summary>
        /// <returns></returns>
        private double ComputeLength()
        {
            var scene = GetScene();
            if (scene == null)
            {
                return 0;
            }

            return (scene.EndTime - scene.StartTime) / (scene.Stage.GetTimeCodesPerSecond());
        }

        /// <summary>
        /// Applies the contents of this USD file to a foreign root object.
        /// </summary>
        /// <remarks>
        /// The idea here is that one may have many animation clips, but only a single GameObject in
        /// the Unity scenegraph.
        /// </remarks>
        public void SetTime(double time, UsdAsset foreignRoot, bool saveMeshUpdates)
        {
            var scene = GetScene();
            if (scene == null)
            {
                Debug.LogWarning("Null scene from GetScene() at " + UnityTypeConverter.GetPath(transform));
                return;
            }

            // Careful not to update any local members here, if this data is driven from a prefab, we
            // dont want those changes to be baked back into the asset.
            time += foreignRoot.m_usdTimeOffset;
            float usdTime = (float)(scene.StartTime + time * scene.Stage.GetTimeCodesPerSecond());
            if (usdTime > scene.EndTime)
            {
                return;
            }

            if (usdTime < scene.StartTime)
            {
                return;
            }

            scene.Time = usdTime;

            var options = new SceneImportOptions();
            foreignRoot.StateToOptions(ref options);

            PrepOptionsForTimeChange(ref options);

            if (foreignRoot.m_lastPrimMap == null)
            {
                foreignRoot.m_lastPrimMap = new PrimMap();
                options.importHierarchy = true;
            }

            if (m_usdVariabilityCache)
            {
                if (m_lastAccessMask == null)
                {
                    m_lastAccessMask = new AccessMask();
                    scene.IsPopulatingAccessMask = true;
                }
            }
            else
            {
                m_lastAccessMask = null;
            }

            if (m_debugPrintVariabilityCache && m_lastAccessMask != null
                && !scene.IsPopulatingAccessMask)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var kvp in m_lastAccessMask.Included)
                {
                    sb.AppendLine(kvp.Key);
                    foreach (var member in kvp.Value.dynamicMembers)
                    {
                        sb.AppendLine("  ." + member.Name);
                    }

                    sb.AppendLine();
                }

                Debug.Log(sb.ToString());
            }

            scene.AccessMask = m_lastAccessMask;
            options.ImportType = ImportType.Streaming;
            SceneImporter.ImportUsd(foreignRoot.gameObject,
                scene,
                foreignRoot.m_lastPrimMap,
                options);
            scene.AccessMask = null;

            if (m_lastAccessMask != null)
            {
                scene.IsPopulatingAccessMask = false;
            }
        }

        private void Update()
        {
            if (m_lastTime == m_usdTimeOffset)
            {
                return;
            }

            m_lastTime = m_usdTimeOffset;
            SetTime(m_usdTimeOffset, this, saveMeshUpdates: true);
        }

        /// <summary>
        /// Optimizes the given import options for fast playback. This assumes that the asset was
        /// previously imported, therefore it disables import of the material and scene hierarchy.
        /// </summary>
        public static void PrepOptionsForTimeChange(ref SceneImportOptions options)
        {
            options.forceRebuild = false;
            options.materialImportMode = MaterialImportMode.None;
            options.meshOptions.debugShowSkeletonBindPose = false;
            options.meshOptions.debugShowSkeletonRestPose = false;

            options.meshOptions.generateLightmapUVs = false;
            options.importSkinWeights = false;

            // Note that tangent and Normals must be updated when the mesh deforms.
            options.importHierarchy = false;
        }

        /// <summary>
        /// Imports the USD scene incrementally, setting a fixed time budget per frame for import
        /// operations. Uses StartCoroutine.
        /// </summary>
        public void ImportUsdAsCoroutine(GameObject goRoot,
            string usdFilePath,
            double time,
            SceneImportOptions importOptions,
            float targetFrameMilliseconds)
        {
            InitUsd.Initialize();
            var scene = Scene.Open(usdFilePath);
            if (scene == null)
            {
                throw new Exception("Failed to open: " + usdFilePath);
            }

            scene.Time = time;
            if (scene == null)
            {
                throw new Exception("Null USD Scene");
            }

            scene.SetInterpolation(importOptions.interpolate
                ? Scene.InterpolationMode.Linear
                : Scene.InterpolationMode.Held);
            var primMap = new PrimMap();
            var importer = SceneImporter.BuildScene(scene,
                goRoot,
                importOptions,
                primMap,
                targetFrameMilliseconds,
                composingSubtree: false);
            StartCoroutine(importer);
        }

        /// <summary>
        /// Loads or unloads the given payload object. Throws an exception if game object deos not have
        /// a UsdPrimSource behaviour.
        /// </summary>
        public void SetPayloadState(GameObject go, bool isLoaded)
        {
            var primSrc = go.GetComponent<UsdPrimSource>();
            if (!primSrc)
            {
                throw new Exception("UsdPrimSource not found: " + UnityTypeConverter.GetPath(go.transform));
            }

            var usdPrimPath = primSrc.m_usdPrimPath;

            InitUsd.Initialize();
            var scene = GetScene();
            if (scene == null)
            {
                throw new Exception("Failed to open: " + usdFullPath);
            }

            var prim = scene.GetPrimAtPath(usdPrimPath);
            if (prim == null || !prim)
            {
                throw new Exception("Prim not found: " + usdPrimPath);
            }

            foreach (var child in go.transform.GetComponentsInChildren<UsdPrimSource>().ToList())
            {
                if (!child || child.gameObject == go)
                {
                    continue;
                }

                GameObject.DestroyImmediate(child.gameObject);
            }

            if (!isLoaded)
            {
                prim.Unload();
                return;
            }
            else
            {
                prim.Load();
            }

            SceneImportOptions importOptions = new SceneImportOptions();
            this.StateToOptions(ref importOptions);
            importOptions.usdRootPath = prim.GetPath();
            importOptions.ImportType = ImportType.Refresh; // force rebuild is false, so this is a refresh not a full reimport..?
            SceneImporter.ImportUsd(go, scene, new PrimMap(), true, importOptions);
        }

        /// <summary>
        /// Applies changes to the USD variant selection made via UsdVariantSet behaviour. Objects will
        /// be destroyed and imported as a result.
        /// </summary>
        private void ApplyVariantSelectionState(Scene scene, UsdVariantSet variants)
        {
            var primSource = variants.GetComponent<UsdPrimSource>();
            if (!primSource)
            {
                Debug.LogError("Null UsdPrimSource while applying variant selection state.");
                return;
            }

            var selections = variants.GetVariantSelections();
            var path = primSource.m_usdPrimPath;
            var prim = scene.GetPrimAtPath(path);
            var varSets = prim.GetVariantSets();
            foreach (var sel in selections)
            {
                if (!varSets.HasVariantSet(sel.Key))
                {
                    throw new Exception("Unknown varient set: " + sel.Key + " at " + path);
                }

                varSets.GetVariantSet(sel.Key).SetVariantSelection(sel.Value);
            }
        }

        /// <summary>
        /// Sets the variant selections in USD at the given prim path based on the selections parameter.
        /// </summary>
        /// <param name="go">The gameObject at the root of the variant set.</param>
        /// <param name="usdPrimPath">The USD prim at which to set the variant selection.</param>
        /// <param name="selections">A collection of (variant set, selection) pairs.</param>
        /// <remarks>
        /// A USD prim can have zero or more variant sets, for example a single prim amy have
        /// "modelingVariant" and "shadingVariant" sets. Each set can have their own slection.
        /// </remarks>
        /// <example>
        /// If two sets with selections are modelingVariant=CupWithHandle and shadingVariant=BrightBlue,
        /// resulting in a bright blue cup with a handle. In this example, the selections dictionary
        /// would contain:
        ///  { "modelingVariant" = "CupWithHandle",
        ///    "shadingVariant" = "BrightBlue" }
        /// </example>
        public void SetVariantSelection(GameObject go,
            string usdPrimPath,
            Dictionary<string, string> selections)
        {
            InitUsd.Initialize();
            var scene = GetScene();
            if (scene == null)
            {
                throw new Exception("Failed to open: " + usdFullPath);
            }

            var prim = scene.GetPrimAtPath(usdPrimPath);
            if (prim == null || !prim)
            {
                throw new Exception("Prim not found: " + usdPrimPath);
            }

            var varSets = prim.GetVariantSets();
            foreach (var sel in selections)
            {
                if (!varSets.HasVariantSet(sel.Key))
                {
                    throw new Exception("Unknown varient set: " + sel.Key + " at " + usdPrimPath);
                }

                varSets.GetVariantSet(sel.Key).SetVariantSelection(sel.Value);
            }

            // TODO: sparsely remove prims, rather than blowing away all the children.
            foreach (Transform child in go.transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }

            SceneImportOptions importOptions = new SceneImportOptions();
            this.StateToOptions(ref importOptions);
            importOptions.usdRootPath = prim.GetPath();
            importOptions.ImportType = ImportType.Refresh; // force rebuild is false, so this is a refresh not a full reimport..?
            SceneImporter.ImportUsd(go, scene, new PrimMap(), true, importOptions);
        }
    }
}

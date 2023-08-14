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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using USD.NET;
using USD.NET.Unity;
using Unity.Jobs;
using Stopwatch = System.Diagnostics.Stopwatch;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Unity.Formats.USD
{
    /// <summary>
    /// An interface for delegating import behavior to a third party.
    /// </summary>
    public interface IImporter
    {
        void BeginReading(Scene scene, PrimMap primMap, SceneImportOptions importOptions);

        IEnumerator Import(Scene scene,
            PrimMap primMap,
            SceneImportOptions importOptions);
    }

    /// <summary>
    /// Root entry point for importing an entire USD scene.
    /// </summary>
    public static class SceneImporter
    {
        public class ImportException : System.Exception
        {
            public ImportException() : base()
            {
            }

            public ImportException(string message) : base(message)
            {
            }

            public ImportException(string message, System.Exception innerException)
                : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// The active mesh importer to be used when ImportUsd is called.
        /// </summary>
        public static IImporter ActiveMeshImporter;

        static SceneImporter()
        {
            SceneImporter.ActiveMeshImporter = new MeshImportStrategy(
                MeshImporter.BuildMesh,
                MeshImporter.BuildSkinnedMesh);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Custom importer. This works almost exactly as the ScriptedImporter, but does not require
        /// the new API.
        /// </summary>
        public static void SavePrefab(GameObject rootObject,
            string prefabPath,
            string playableClipName,
            SceneImportOptions importOptions)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject prefab = null;

            if (oldPrefab == null)
            {
                // Create the prefab. At this point, the meshes do not yet exist and will be
                // dangling references
                prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath);
                HashSet<Mesh> meshes;
                HashSet<Material> materials;
                AddObjectsToAsset(rootObject, prefab, importOptions, out meshes, out materials);

                foreach (var mesh in meshes)
                {
                    AssetDatabase.AddObjectToAsset(mesh, prefab);
                }

                foreach (var mat in materials)
                {
                    AssetDatabase.AddObjectToAsset(mat, prefab);
                }

                // Fix the dangling references.
                prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath);
                var playable = ScriptableObject.CreateInstance<UsdPlayableAsset>();

                playable.SourceUsdAsset.defaultValue = prefab.GetComponent<UsdAsset>();
                playable.name = playableClipName;
                AssetDatabase.AddObjectToAsset(playable, prefab);
                prefab = PrefabUtility.SavePrefabAsset(prefab);
            }
            else
            {
                HashSet<Mesh> meshes;
                HashSet<Material> materials;

                oldPrefab = PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath);
                AddObjectsToAsset(rootObject, oldPrefab, importOptions, out meshes, out materials);

                // ReplacePrefab only removes the GameObjects from the asset.
                // Clear out all non-prefab junk (ie, meshes), because otherwise it piles up.
                // The main difference between LoadAllAssetRepresentations and LoadAllAssets
                // is that the former returns MonoBehaviours and the latter does not.
                foreach (var obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabPath))
                {
                    if (obj is GameObject)
                    {
                        continue;
                    }

                    if (obj is Mesh && meshes.Contains((Mesh)obj))
                    {
                        meshes.Remove((Mesh)obj);
                        continue;
                    }

                    if (obj is Material && materials.Contains((Material)obj))
                    {
                        materials.Remove((Material)obj);
                        continue;
                    }

                    Object.DestroyImmediate(obj, allowDestroyingAssets: true);
                }

                foreach (var mesh in meshes)
                {
                    AssetDatabase.AddObjectToAsset(mesh, oldPrefab);
                }

                foreach (var mat in materials)
                {
                    AssetDatabase.AddObjectToAsset(mat, oldPrefab);
                }

                prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath);

                var playable = ScriptableObject.CreateInstance<UsdPlayableAsset>();
                playable.SourceUsdAsset.defaultValue = prefab.GetComponent<UsdAsset>();
                playable.name = playableClipName;
                AssetDatabase.AddObjectToAsset(playable, prefab);
                PrefabUtility.SavePrefabAsset(prefab);
            }

            AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }

        static void AddObjectsToAsset(GameObject rootObject,
            Object asset,
            SceneImportOptions importOptions,
            out HashSet<Mesh> usedMeshes,
            out HashSet<Material> usedMaterials)
        {
            var meshes = new HashSet<Mesh>();
            var materials = new HashSet<Material>();

            materials.Add(importOptions.materialMap.DisplayColorMaterial);
            materials.Add(importOptions.materialMap.MetallicWorkflowMaterial);
            materials.Add(importOptions.materialMap.SpecularWorkflowMaterial);

            var tempMat = importOptions.materialMap.DisplayColorMaterial;
            if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "")
            {
                materials.Add(tempMat);
            }

            tempMat = importOptions.materialMap.MetallicWorkflowMaterial;
            if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "")
            {
                materials.Add(tempMat);
            }

            tempMat = importOptions.materialMap.SpecularWorkflowMaterial;
            if (tempMat != null && AssetDatabase.GetAssetPath(tempMat) == "")
            {
                materials.Add(tempMat);
            }

            foreach (var mf in rootObject.GetComponentsInChildren<MeshFilter>())
            {
                if (!mf)
                {
                    continue;
                }

                if (mf.sharedMesh != null && meshes.Add(mf.sharedMesh))
                {
                    mf.sharedMesh.name = mf.name;
                }
            }

            foreach (var mf in rootObject.GetComponentsInChildren<MeshRenderer>())
            {
                if (!mf)
                {
                    continue;
                }

                foreach (var mat in mf.sharedMaterials)
                {
                    if (mat != null && !materials.Add(mat))
                    {
                        mat.name = mf.name;
                        continue;
                    }
                }
            }

            foreach (var mf in rootObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!mf)
                {
                    continue;
                }

                if (mf.sharedMesh != null && meshes.Add(mf.sharedMesh))
                {
                    mf.sharedMesh.name = mf.name;
                }

                foreach (var mat in mf.sharedMaterials)
                {
                    if (mat != null && !materials.Add(mat))
                    {
                        mat.name = mf.name;
                        continue;
                    }
                }
            }

            usedMeshes = meshes;
            usedMaterials = materials;
        }

#endif

        public static void ImportUsd(GameObject goRoot,
            Scene scene,
            PrimMap primMap,
            SceneImportOptions importOptions)
        {
            ImportUsd(goRoot, scene, primMap, false, importOptions);
        }

        public static void ImportUsd(GameObject goRoot,
            Scene scene,
            PrimMap primMap,
            bool composingSubtree,
            SceneImportOptions importOptions)
        {
            UsdEditorAnalytics.ImportResult importResult = UsdEditorAnalytics.ImportResult.Default;
            importResult.ImportType = importOptions.ImportType;
            if (scene == null)
            {
                UsdEditorAnalytics.SendImportEvent("", 0, importResult);
                throw new ImportException("Null USD Scene");
            }

#if UNITY_EDITOR
            Stopwatch analyticsTimer = new Stopwatch();

            if (importOptions.ImportType != ImportType.Streaming)
                analyticsTimer.Start();
# endif

            // The matrix to convert USD (right-handed) to Unity (left-handed) is different for the legacy FBX importer
            // and incorrectly swaps the X-axis rather than the Z-axis. This changes the basisChange matrix to match the
            // user options. <see cref="Unity.Formats.USD.SceneImportOptions.BasisTransformation"/> for additional details.
            // Note that in those specific cases, the inverse matrix are identical to the original one, in general,
            // UnityTypeConverter.inverseBasisChange should be equal to UnityTypeConverter.basisChange.inverse.
            if (importOptions.changeHandedness == BasisTransformation.SlowAndSafeAsFBX)
            {
                // To be consistent with FBX basis change, ensure it's the X axis that is inverted.
                Vector3 matrixDiagonal = new Vector3(-1, 1, 1);
                UnityTypeConverter.basisChange = Matrix4x4.Scale(matrixDiagonal);
                UnityTypeConverter.inverseBasisChange = Matrix4x4.Scale(matrixDiagonal);
            }
            else
            {
                // Ensure it's the Z axis that is inverted.
                Vector3 matrixDiagonal = new Vector3(1, 1, -1);
                UnityTypeConverter.basisChange = Matrix4x4.Scale(matrixDiagonal);
                UnityTypeConverter.inverseBasisChange = Matrix4x4.Scale(matrixDiagonal);
            }

            scene.SetInterpolation(importOptions.interpolate
                ? Scene.InterpolationMode.Linear
                : Scene.InterpolationMode.Held);

            primMap = SceneImporter.BuildScene(scene,
                goRoot,
                importOptions,
                primMap,
                composingSubtree);

#if UNITY_EDITOR // Path APIs are not available in builds, may as well skip the whole thing
            if (importOptions.ImportType != ImportType.Streaming) // don't send analytics when the import is triggered by streaming time sampled data
            {
                analyticsTimer.Stop();
                if (primMap != null)
                {
                    importResult = CreateImportResult(!primMap.HasErrors, primMap, importOptions.ImportType);
                }
                if (importOptions.ImportType == ImportType.Initial)
                    UsdEditorAnalytics.SendImportEvent(Path.GetExtension(scene.FilePath),
                        analyticsTimer.Elapsed.TotalMilliseconds, importResult);
                else
                    UsdEditorAnalytics.SendReimportEvent(Path.GetExtension(scene.FilePath),
                        analyticsTimer.Elapsed.TotalMilliseconds, importResult);

            }
#endif
        }

        private static UsdEditorAnalytics.ImportResult CreateImportResult(bool success, PrimMap primMap, ImportType importType = ImportType.Initial) => new UsdEditorAnalytics.ImportResult()
        {
            Success = success,
            ImportType = importType,
            ContainsMeshes = primMap.Meshes == null ? false : primMap.Meshes.Length > 0,
            ContainsPointInstancer = primMap.ContainsPointInstances,
            ContainsSkel = primMap.SkelRoots == null ? false : primMap.SkelRoots.Length > 0,
            ContainsMaterials = primMap.Materials == null ? false : primMap.Materials.Length > 0
        };

        /// <summary>
        /// Rebuilds the USD scene as Unity GameObjects, maintaining a mapping from USD to Unity.
        /// </summary>
        public static PrimMap BuildScene(Scene scene,
            GameObject root,
            SceneImportOptions importOptions,
            PrimMap primMap,
            bool composingSubtree)
        {
            try
            {
                Profiler.BeginSample("USD: Build Scene");
                var builder = BuildScene(scene,
                    root,
                    importOptions,
                    primMap,
                    0,
                    composingSubtree);
                while (builder.MoveNext())
                {
                }

                return primMap;
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private static void RemoveComponent<T>(GameObject go)
        {
            var c = go.GetComponent<T>() as Component;
            if (c == null)
            {
                return;
            }
#if UNITY_EDITOR
            Component.DestroyImmediate(c, true);
#else
            Component.Destroy(c);
#endif
        }

        /// <summary>
        /// Rebuilds the USD scene as Unity GameObjects, with a limited budget per update.
        /// </summary>
        public static IEnumerator BuildScene(Scene scene,
            GameObject root,
            SceneImportOptions importOptions,
            PrimMap primMap,
            float targetFrameMilliseconds,
            bool composingSubtree)
        {
            var timer = new Stopwatch();
            var usdPrimRoot = new pxr.SdfPath(importOptions.usdRootPath);

            // Setting an arbitrary fudge factor of 20% is very non-scientific, however it's better than
            // nothing. The correct way to hit a deadline is to predict how long each iteration actually
            // takes and then return early if the estimated time is over budget.
            float targetTime = targetFrameMilliseconds * .8f;

            timer.Start();

            // Reconstruct the USD hierarchy as Unity GameObjects.
            // A PrimMap is returned for tracking the USD <-> Unity mapping.
            Profiler.BeginSample("USD: Build Hierarchy");
            if (importOptions.importHierarchy || importOptions.forceRebuild)
            {
                // When a USD file is fully RE-imported, all exsiting USD data must be removed. The old
                // assumption was that the root would never have much more than the UsdAsset component
                // itself, however it's now clear that the root may also have meaningful USD data added
                // too.
                //
                // TODO(jcowles): This feels like a workaround. What we really want here is an "undo"
                // process for changes made to the root GameObject. For example, to clean up non-USD
                // components which may have been added (e.g. what if a mesh is imported to the root?
                // currently the MeshRenderer etc will remain after re-import).
                RemoveComponent<UsdAssemblyRoot>(root);
                RemoveComponent<UsdVariantSet>(root);
                RemoveComponent<UsdModelRoot>(root);
                RemoveComponent<UsdLayerStack>(root);
                RemoveComponent<UsdPayload>(root);
                RemoveComponent<UsdPrimSource>(root);

                primMap.Clear();
                HierarchyBuilder.BuildGameObjects(scene,
                    root,
                    usdPrimRoot,
                    scene.Find(usdPrimRoot.ToString(), "UsdSchemaBase"),
                    primMap,
                    importOptions);
            }

            Profiler.EndSample();

            if (ShouldYield(targetTime, timer))
            {
                yield return null;
                ResetTimer(timer);
            }

            Profiler.BeginSample("USD: Post Process Hierarchy");
            foreach (var processor in root.GetComponents<IImportPostProcessHierarchy>())
            {
                try
                {
                    processor.PostProcessHierarchy(primMap, importOptions);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    primMap.HasErrors = true;
                }
            }

            Profiler.EndSample();

            if (ShouldYield(targetTime, timer))
            {
                yield return null;
                ResetTimer(timer);
            }

            //
            // Pre-process UsdSkelRoots.
            //

            var skelRoots = new List<pxr.UsdSkelRoot>();
            if (importOptions.importSkinning)
            {
                Profiler.BeginSample("USD: Process UsdSkelRoots");
                foreach (var path in primMap.SkelRoots)
                {
                    try
                    {
                        var skelRootPrim = scene.GetPrimAtPath(path);
                        if (!skelRootPrim)
                        {
                            Debug.LogWarning("SkelRoot prim not found: " + path);
                            continue;
                        }

                        var skelRoot = new pxr.UsdSkelRoot(skelRootPrim);
                        if (!skelRoot)
                        {
                            Debug.LogWarning("SkelRoot prim not SkelRoot type: " + path);
                            primMap.HasErrors = true;
                            continue;
                        }

                        skelRoots.Add(skelRoot);
                        GameObject go = primMap[path];
                        ImporterBase.GetOrAddComponent<Animator>(go, true);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error pre-processing SkelRoot <" + path + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                Profiler.EndSample();
            }

            //
            // Import known prim types.
            //

            // Materials.
            Profiler.BeginSample("USD: Build Materials");
            if (importOptions.ShouldBindMaterials)
            {
                foreach (var pathAndSample in scene.ReadAll<MaterialSample>(primMap.Materials))
                {
                    try
                    {
                        var mat = MaterialImporter.BuildMaterial(scene,
                            pathAndSample.path,
                            pathAndSample.sample,
                            importOptions);
                        if (mat != null)
                        {
                            importOptions.materialMap[pathAndSample.path] = mat;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing material <" + pathAndSample.path + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }
            }

            Profiler.EndSample();

            //
            // Start threads.
            //
            ReadAllJob<SanitizedXformSample> readXforms;
            if (importOptions.importTransforms)
            {
                readXforms = new ReadAllJob<SanitizedXformSample>(scene, primMap.Xforms, importOptions);
                readXforms.Schedule(primMap.Xforms.Length, 4);
            }

            if (importOptions.importMeshes)
            {
                ActiveMeshImporter.BeginReading(scene, primMap, importOptions);
            }

            JobHandle.ScheduleBatchedJobs();


            // Xforms.
            //
            // Note that we are specifically filtering on XformSample, not Xformable, this way only
            // Xforms are processed to avoid doing that work redundantly.
            if (importOptions.importTransforms)
            {
                Profiler.BeginSample("USD: Build Xforms");
                foreach (var pathAndSample in readXforms)
                {
                    try
                    {
                        if (pathAndSample.path == usdPrimRoot)
                        {
                            // Never read the xform from the USD root, that will be authored in Unity.
                            continue;
                        }

                        GameObject go = primMap[pathAndSample.path];
                        NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path), importOptions);
                        XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions, scene);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing xform <" + pathAndSample.path + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                foreach (var pathAndSample in scene.ReadAll<XformSample>(primMap.SkelRoots))
                {
                    try
                    {
                        if (pathAndSample.path == usdPrimRoot)
                        {
                            // Never read the xform from the USD root, that will be authored in Unity.
                            continue;
                        }

                        GameObject go = primMap[pathAndSample.path];
                        NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path), importOptions);
                        XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions, scene);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing xform <" + pathAndSample.path + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                if (importOptions.importSkinning)
                {
                    foreach (var pathAndSample in scene.ReadAll<XformSample>(primMap.Skeletons))
                    {
                        try
                        {
                            if (pathAndSample.path == usdPrimRoot)
                            {
                                // Never read the xform from the USD root, that will be authored in Unity.
                                continue;
                            }

                            GameObject go = primMap[pathAndSample.path];
                            NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path),
                                importOptions);
                            XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions,
                                scene);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(
                                new ImportException("Error processing xform <" + pathAndSample.path + ">", ex));
                            primMap.HasErrors = true;
                        }

                        if (ShouldYield(targetTime, timer))
                        {
                            yield return null;
                            ResetTimer(timer);
                        }
                    }
                }

                Profiler.EndSample();
            }

            // Meshes.
            if (importOptions.importMeshes)
            {
                Profiler.BeginSample("USD: Build Meshes");
                IEnumerator it = ActiveMeshImporter.Import(scene, primMap, importOptions);

                while (it.MoveNext())
                {
                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                Profiler.EndSample();

                // Cubes.
                Profiler.BeginSample("USD: Build Cubes");
                foreach (var pathAndSample in scene.ReadAll<CubeSample>(primMap.Cubes))
                {
                    try
                    {
                        GameObject go = primMap[pathAndSample.path];
                        pxr.UsdPrim prim = scene.GetPrimAtPath(pathAndSample.path);

                        NativeImporter.ImportObject(scene, go, prim, importOptions);
                        XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions, scene);
                        bool skinnedMesh = IsSkinnedMesh(prim, primMap, importOptions);
                        CubeImporter.BuildCube(pathAndSample.sample, go, importOptions, skinnedMesh);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing cube <" + pathAndSample.path + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                Profiler.EndSample();

                // Spheres.
                Profiler.BeginSample("USD: Build Spheres");
                foreach (var pathAndSample in scene.ReadAll<SphereSample>(primMap.Spheres))
                {
                    try
                    {
                        GameObject go = primMap[pathAndSample.path];
                        pxr.UsdPrim prim = scene.GetPrimAtPath(pathAndSample.path);

                        NativeImporter.ImportObject(scene, go, prim, importOptions);
                        XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions, scene);
                        bool skinnedMesh = IsSkinnedMesh(prim, primMap, importOptions);
                        SphereImporter.BuildSphere(pathAndSample.sample, go, importOptions, skinnedMesh);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing sphere <" + pathAndSample.path + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                Profiler.EndSample();
            }

            // Cameras.
            if (importOptions.importCameras)
            {
                Profiler.BeginSample("USD: Cameras");
                foreach (var pathAndSample in scene.ReadAll<SanitizedCameraSample>(primMap.Cameras))
                {
                    try
                    {
                        GameObject go = primMap[pathAndSample.path];
                        pathAndSample.sample.Sanitize(scene, importOptions);
                        NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path), importOptions);
                        XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions, scene);

                        // In order to match FBX importer buggy behavior, the camera xform need an extra rotation.
                        // FBX importer is fixed in 2020 though with an option to do an axis bake on import.
                        // If axis bake is used, no need to use the SlowAndSafeAsFBX mode.
                        if (importOptions.changeHandedness == BasisTransformation.SlowAndSafeAsFBX)
                        {
                            go.transform.localRotation *= Quaternion.Euler(180.0f, 0.0f, 180.0f);
                        }

                        // The camera has many value-type parameters that need to be handled correctly when not
                        // not animated. For now, only the camera transform will animate, until this is fixed.
                        if (scene.AccessMask == null || scene.IsPopulatingAccessMask)
                        {
                            CameraImporter.BuildCamera(pathAndSample.sample, go, importOptions);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing camera <" + pathAndSample.path + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                Profiler.EndSample();
            }

            // Build out masters for instancing.
            Profiler.BeginSample("USD: Build Instances");
            foreach (var masterRootPath in primMap.GetMasterRootPaths())
            {
                try
                {
                    Transform masterRootXf = primMap[masterRootPath].transform;

                    // Transforms
                    if (importOptions.importTransforms)
                    {
                        Profiler.BeginSample("USD: Build Xforms");
                        foreach (var pathAndSample in scene.ReadAll<SanitizedXformSample>(masterRootPath))
                        {
                            try
                            {
                                GameObject go = primMap[pathAndSample.path];
                                NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path),
                                    importOptions);
                                pathAndSample.sample.Sanitize(scene, importOptions);
                                XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions,
                                    scene);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(
                                    new ImportException("Error processing xform <" + pathAndSample.path + ">", ex));
                                primMap.HasErrors = true;
                            }
                        }

                        foreach (var pathAndSample in scene.ReadAll<SanitizedXformSample>(primMap.Skeletons))
                        {
                            try
                            {
                                GameObject go = primMap[pathAndSample.path];
                                NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path),
                                    importOptions);
                                pathAndSample.sample.Sanitize(scene, importOptions);
                                XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions,
                                    scene);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(
                                    new ImportException("Error processing xform <" + pathAndSample.path + ">", ex));
                                primMap.HasErrors = true;
                            }
                        }

                        Profiler.EndSample();
                    }

                    // Meshes.
                    if (importOptions.importMeshes)
                    {
                        Profiler.BeginSample("USD: Build Meshes");
                        foreach (var pathAndSample in scene.ReadAll<SanitizedMeshSample>(masterRootPath))
                        {
                            try
                            {
                                GameObject go = primMap[pathAndSample.path];
                                NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path),
                                    importOptions);
                                // TODO: should we restore the DeserializationContext here?
                                pathAndSample.sample.Sanitize(scene, importOptions);
                                XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions,
                                    scene);
                                var subsets = MeshImporter.ReadGeomSubsets(scene, pathAndSample.path);
                                bool isDynamic = scene.AccessMask != null
                                    ? scene.AccessMask.Included.ContainsKey(pathAndSample.path)
                                    : false;
                                MeshImporter.BuildMesh(pathAndSample.path, pathAndSample.sample, subsets, go,
                                    importOptions, isDynamic);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(
                                    new ImportException("Error processing mesh <" + pathAndSample.path + ">", ex));
                                primMap.HasErrors = true;
                            }
                        }

                        Profiler.EndSample();

                        // Cubes.
                        Profiler.BeginSample("USD: Build Cubes");
                        foreach (var pathAndSample in scene.ReadAll<CubeSample>(masterRootPath))
                        {
                            try
                            {
                                GameObject go = primMap[pathAndSample.path];
                                NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path),
                                    importOptions);
                                XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions,
                                    scene);
                                CubeImporter.BuildCube(pathAndSample.sample, go, importOptions);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(
                                    new ImportException("Error processing cube <" + pathAndSample.path + ">", ex));
                                primMap.HasErrors = true;
                            }
                        }

                        Profiler.EndSample();

                        // Spheres.
                        Profiler.BeginSample("USD: Build Spheres");
                        var sphereSamples = scene.ReadAll<SphereSample>(masterRootPath);
                        foreach (var pathAndSample in sphereSamples)
                        {
                            try
                            {
                                GameObject go = primMap[pathAndSample.path];
                                NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path),
                                    importOptions);
                                XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions,
                                    scene);
                                SphereImporter.BuildSphere(pathAndSample.sample, go, importOptions);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(
                                    new ImportException("Error processing sphere <" + pathAndSample.path + ">", ex));
                                primMap.HasErrors = true;
                            }
                        }

                        Profiler.EndSample();
                    }

                    // Cameras.
                    if (importOptions.importCameras)
                    {
                        Profiler.BeginSample("USD: Build Cameras");
                        foreach (var pathAndSample in scene.ReadAll<SanitizedCameraSample>(masterRootPath))
                        {
                            try
                            {
                                GameObject go = primMap[pathAndSample.path];
                                pathAndSample.sample.Sanitize(scene, importOptions);
                                NativeImporter.ImportObject(scene, go, scene.GetPrimAtPath(pathAndSample.path),
                                    importOptions);
                                XformImporter.BuildXform(pathAndSample.path, pathAndSample.sample, go, importOptions,
                                    scene);
                                CameraImporter.BuildCamera(pathAndSample.sample, go, importOptions);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(
                                    new ImportException("Error processing camera <" + pathAndSample.path + ">", ex));
                                primMap.HasErrors = true;
                            }
                        }

                        Profiler.EndSample();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(
                        new ImportException("Error processing master <" + masterRootPath + ">", ex));
                    primMap.HasErrors = true;
                }

                if (ShouldYield(targetTime, timer))
                {
                    yield return null;
                    ResetTimer(timer);
                }
            } // Instances.

            Profiler.EndSample();

            //
            // Post-process dependencies: materials and bones.
            //

            Profiler.BeginSample("USD: Process Material Bindings");
            try
            {
                // TODO: Currently ProcessMaterialBindings runs too long and will go over budget for any
                // large scene. However, pulling the loop into this code feels wrong in terms of
                // responsibilities.

                // Process all material bindings in a single vectorized request.
                MaterialImporter.ProcessMaterialBindings(scene, importOptions);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(new ImportException("Failed in ProcessMaterialBindings", ex));
                primMap.HasErrors = true;
            }

            Profiler.EndSample();

            if (ShouldYield(targetTime, timer))
            {
                yield return null;
                ResetTimer(timer);
            }

            //
            // SkinnedMesh bone bindings.
            //
            if (importOptions.importSkinning)
            {
                Profiler.BeginSample("USD: Build Skeletons");
                var skeletonSamples = new Dictionary<pxr.SdfPath, SkeletonSample>();
                foreach (var skelRoot in skelRoots)
                {
                    try
                    {
                        var bindings = new pxr.UsdSkelBindingVector();
                        if (!primMap.SkelBindings.TryGetValue(skelRoot.GetPath(), out bindings))
                        {
                            Debug.LogWarning("No bindings found skelRoot: " + skelRoot.GetPath());
                            primMap.HasErrors = true;
                        }

                        if (bindings.Count == 0)
                        {
                            Debug.LogWarning("No bindings found skelRoot: " + skelRoot.GetPath());
                            primMap.HasErrors = true;
                        }

                        foreach (var skelBinding in bindings)
                        {
                            // The SkelRoot will likely have a skeleton binding, but it's inherited, so the bound
                            // skeleton isn't actually known until it's queried from the binding. Still, we would
                            // like not to reprocess skeletons redundantly, so skeletons are cached into a
                            // dictionary.

                            Profiler.BeginSample("Build Bind Transforms");
                            var skelPath = skelBinding.GetSkeleton().GetPath();
                            SkeletonSample skelSample = null;
                            if (!skeletonSamples.TryGetValue(skelPath, out skelSample))
                            {
                                skelSample = new SkeletonSample();

                                Profiler.BeginSample("Read Skeleton");
                                scene.Read(skelPath, skelSample);
                                Profiler.EndSample();

                                skeletonSamples.Add(skelPath, skelSample);

                                // The bind pose is bone's inverse transformation matrix. This is done once here, so each
                                // skinned mesh doesn't need to do it redundantly.
                                SkeletonImporter.BuildBindTransforms(skelPath, skelSample, importOptions);

                                // Validate the binding transforms.
                                var bindXforms = new pxr.VtMatrix4dArray();
                                var prim = scene.GetPrimAtPath(skelPath);
                                var skel = new pxr.UsdSkelSkeleton(prim);

                                Profiler.BeginSample("Get SkelQuery");
                                pxr.UsdSkelSkeletonQuery skelQuery = primMap.SkelCache.GetSkelQuery(skel);
                                Profiler.EndSample();

                                Profiler.BeginSample("Get JointWorldBind Transforms");
                                if (!skelQuery.GetJointWorldBindTransforms(bindXforms))
                                {
                                    throw new ImportException("Failed to compute binding transforms for <" + skelPath +
                                        ">");
                                }

                                Profiler.EndSample();

                                SkeletonImporter.BuildDebugBindTransforms(skelSample, primMap[skelPath], importOptions);
                            }

                            Profiler.EndSample();

                            if (importOptions.importSkinWeights)
                            {
                                //
                                // Apply skinning weights to each skinned mesh.
                                //
                                Profiler.BeginSample("Apply Skin Weights");
                                foreach (var skinningQuery in skelBinding.GetSkinningTargetsAsVector())
                                {
                                    pxr.SdfPath meshPath = skinningQuery.GetPrim().GetPath();
                                    try
                                    {
                                        var goMesh = primMap[meshPath];

                                        Profiler.BeginSample("Build Skinned Mesh");
                                        SkeletonImporter.BuildSkinnedMesh(
                                            meshPath,
                                            skelPath,
                                            skelSample,
                                            skinningQuery,
                                            goMesh,
                                            primMap,
                                            importOptions);
                                        Profiler.EndSample();

                                        // In terms of performance, this is almost free.
                                        // TODO: Check if this is correct or should be something specific (not always the first child).
                                        goMesh.GetComponent<SkinnedMeshRenderer>().rootBone =
                                            primMap[skelPath].transform.GetChild(0);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Debug.LogException(new ImportException("Error skinning mesh: " + meshPath, ex));
                                        primMap.HasErrors = true;
                                    }
                                }

                                Profiler.EndSample();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing SkelRoot <" + skelRoot.GetPath() + ">", ex));
                        primMap.HasErrors = true;
                    }
                } // foreach SkelRoot

                Profiler.EndSample();

                if (ShouldYield(targetTime, timer))
                {
                    yield return null;
                    ResetTimer(timer);
                }

                //
                // Bone transforms.
                //
                Profiler.BeginSample("USD: Pose Bones");
                foreach (var pathAndSample in skeletonSamples)
                {
                    var skelPath = pathAndSample.Key;

                    try
                    {
                        var prim = scene.GetPrimAtPath(skelPath);
                        var skel = new pxr.UsdSkelSkeleton(prim);

                        pxr.UsdSkelSkeletonQuery skelQuery = primMap.SkelCache.GetSkelQuery(skel);
                        var joints = skelQuery.GetJointOrder();
                        var restXforms = new pxr.VtMatrix4dArray();
                        var time = scene.Time.HasValue ? scene.Time.Value : pxr.UsdTimeCode.Default();

                        Profiler.BeginSample("Compute Joint Local Transforms");
                        if (!skelQuery.ComputeJointLocalTransforms(restXforms, time, atRest: false))
                        {
                            throw new ImportException("Failed to compute bind transforms for <" + skelPath + ">");
                        }

                        Profiler.EndSample();

                        Profiler.BeginSample("Build Bones");
                        for (int i = 0; i < joints.size(); i++)
                        {
                            var jointPath = scene.GetSdfPath(joints[i]);
                            if (joints[i] == "/")
                            {
                                jointPath = skelPath;
                            }
                            else if (jointPath.IsAbsolutePath())
                            {
                                Debug.LogException(
                                    new System.Exception("Unexpected absolute joint path: " + jointPath));
                                jointPath = new pxr.SdfPath(joints[i].ToString().TrimStart('/'));
                                jointPath = skelPath.AppendPath(jointPath);
                            }
                            else
                            {
                                jointPath = skelPath.AppendPath(jointPath);
                            }

                            var goBone = primMap[jointPath];

                            Profiler.BeginSample("Convert Matrix");
                            var restXform = UnityTypeConverter.FromMatrix(restXforms[i]);
                            Profiler.EndSample();

                            Profiler.BeginSample("Build Bone");
                            SkeletonImporter.BuildSkeletonBone(skelPath, goBone, restXform, joints, importOptions);
                            Profiler.EndSample();
                        }

                        Profiler.EndSample();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(
                            new ImportException("Error processing SkelRoot <" + skelPath + ">", ex));
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                Profiler.EndSample();
            }

            //
            // Apply instancing.
            //
            if (importOptions.importSceneInstances)
            {
                Profiler.BeginSample("USD: Build Scene-Instances");
                try
                {
                    // Build scene instances.
                    InstanceImporter.BuildSceneInstances(primMap, importOptions);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(new ImportException("Failed in BuildSceneInstances", ex));
                }

                Profiler.EndSample();
            }

            if (ShouldYield(targetTime, timer))
            {
                yield return null;
                ResetTimer(timer);
            }

            // Build point instances.
            if (importOptions.importPointInstances)
            {
                Profiler.BeginSample("USD: Build Point-Instances");
                // TODO: right now all point instancer data is read, but we only need prototypes and indices.
                var pointInstancerSamples = scene.ReadAll<PointInstancerSample>();
                primMap.ContainsPointInstances = pointInstancerSamples.Length > 0;

                foreach (var pathAndSample in pointInstancerSamples)
                {
                    try
                    {
                        GameObject instancerGo = primMap[pathAndSample.path];

                        // Now build the point instances.
                        InstanceImporter.BuildPointInstances(scene,
                            primMap,
                            pathAndSample.path,
                            pathAndSample.sample,
                            instancerGo,
                            importOptions);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Error processing point instancer <" + pathAndSample.path + ">: " + ex.Message);
                        primMap.HasErrors = true;
                    }

                    if (ShouldYield(targetTime, timer))
                    {
                        yield return null;
                        ResetTimer(timer);
                    }
                }

                Profiler.EndSample();
            }

            //
            // Apply root transform corrections.
            //
            Profiler.BeginSample("USD: Build Root Transforms");
            if (!composingSubtree)
            {
                if (!root)
                {
                    // There is no single root,
                    // Apply root transform corrections to all imported root prims.
                    foreach (KeyValuePair<pxr.SdfPath, GameObject> kvp in primMap)
                    {
                        if (kvp.Key.IsRootPrimPath() && kvp.Value != null)
                        {
                            // The root object at which the USD scene will be reconstructed.
                            // It may need a Z-up to Y-up conversion and a right- to left-handed change of basis.
                            XformImporter.BuildSceneRoot(scene, kvp.Value.transform, importOptions);
                        }
                    }
                }
                else
                {
                    // There is only one root, apply a single transform correction.
                    XformImporter.BuildSceneRoot(scene, root.transform, importOptions);
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("USD: Post Process Components");
            foreach (var processor in root.GetComponents<IImportPostProcessComponents>())
            {
                try
                {
                    processor.PostProcessComponents(primMap, importOptions);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    primMap.HasErrors = true;
                }
            }

            Profiler.EndSample();
        }

        private static bool ShouldYield(float targetTime, Stopwatch timer)
        {
            return timer.ElapsedMilliseconds > targetTime;
        }

        private static void ResetTimer(Stopwatch timer)
        {
            timer.Stop();
            timer.Reset();
            timer.Start();
        }

        private static bool IsSkinnedMesh(pxr.UsdPrim prim, PrimMap primMap, SceneImportOptions importOptions)
        {
            bool skinnedMesh = false;

            if (importOptions.importSkinning && primMap.SkelCache != null)
            {
                pxr.UsdSkelSkinningQuery skinningQuery = primMap.SkelCache.GetSkinningQuery(prim);
                primMap.SkinningQueries[prim.GetPath()] = skinningQuery;
                skinnedMesh = skinningQuery.IsValid();
            }

            return skinnedMesh;
        }
    }
}

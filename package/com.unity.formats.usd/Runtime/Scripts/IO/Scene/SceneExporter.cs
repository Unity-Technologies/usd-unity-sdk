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
using System.Linq;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Unity.Formats.USD
{
    // The export function allows for dispatch to different export functions without knowing what
    // type of data they export (e.g. mesh vs. transform).
    public delegate void ExportFunction(ObjectContext objContext, ExportContext exportContext);

    delegate void ObjectProcessor(GameObject go,
        ExportContext context);

    public struct ObjectContext
    {
        public GameObject gameObject;
        public string path;
        public SampleBase sample;
        public object additionalData;
    }

    public enum ActiveExportPolicy
    {
        // Inactive GameObjects in Unity become invisible objects in USD, which is actually the
        // closest semantic mapping.
        ExportAsVisibility,

        // Inactive GameObjects in Unity become deactivated objects in USD. Caution, this is not truly
        // an equivalent state because deactivated objects in USD are fully unloaded and their
        // subtree will not exist after being deactivated.
        ExportAsActive,

        // Inactive GameObjects will not be exported.
        DoNotExport,

        // Inactive GameObjects will be exported without special handling.
        Ignore,
    }

    public class ExportContext
    {
        public Scene scene;
        public Transform exportRoot;
        public bool exportMaterials = true;
        public bool exportNative = false;
        public float scale = 1.0f;
        public bool exportTransformOverrides = false;

        public BasisTransformation basisTransform = BasisTransformation.FastWithNegativeScale;
        public ActiveExportPolicy activePolicy = ActiveExportPolicy.ExportAsVisibility;
        public Dictionary<GameObject, ExportPlan> plans = new Dictionary<GameObject, ExportPlan>();
        public Dictionary<Material, string> matMap = new Dictionary<Material, string>();

        public Dictionary<Transform, Transform> meshToSkelRoot = new Dictionary<Transform, Transform>();
        public Dictionary<Transform, Transform[]> meshToBones = new Dictionary<Transform, Transform[]>();

        public Dictionary<Transform, List<string>> skelSortedMap = new Dictionary<Transform, List<string>>();

        // Dictionary from <oldRoot> to <newRoot>
        public Dictionary<string, Transform> pathToBone = new Dictionary<string, Transform>();
        public Dictionary<Transform, Transform> boneToRoot = new Dictionary<Transform, Transform>();
        public Dictionary<Transform, Matrix4x4> bindPoses = new Dictionary<Transform, Matrix4x4>();

        // Sample object instances, shared across multiple export methods.
        public Dictionary<Type, SampleBase> samples = new Dictionary<Type, SampleBase>();

        // For analytics purposes
        public Stopwatch analyticsTotalTimeStopwatch = new Stopwatch();
    }

    public class Exporter
    {
        // The USD path at which the Unity data will be written.
        public string path;

        // The sample type to be used when exporting.
        public SampleBase sample;

        // The export function which implements the logic to populate the sample.
        public ExportFunction exportFunc;

        // Additional arguments required for export.
        public object data;
    }

    // An export plan will be created for each path in the scene. Each ExportPlan will use one of
    // the fixed export functions. For example, when setting up export for a mesh, an ExportPlan
    // will be created for that path in the scenegraph and the ExportFunction will the one which is
    // capable of exporting a mesh.
    public class ExportPlan
    {
        // The functions to run when exporting this object.
        public List<Exporter> exporters = new List<Exporter>();
    }

    /// <summary>
    /// The scene exporter can be used to export data to USD.
    /// </summary>
    public static class SceneExporter
    {
        // ------------------------------------------------------------------------------------------ //
        // Main Export Logic.
        // ------------------------------------------------------------------------------------------ //

        public static void Export(GameObject root,
            Scene scene,
            BasisTransformation basisTransform,
            bool exportUnvarying,
            bool zeroRootTransform,
            bool exportMaterials = false,
            bool exportMonoBehaviours = false,
            bool exportOverrides = false)
        {
            var context = new ExportContext();
            context.scene = scene;
            context.basisTransform = basisTransform;
            context.exportRoot = root.transform.parent;
            context.exportTransformOverrides = exportOverrides;
            SyncExportContext(root, context);

            // Since this is a one-shot convenience function, we will automatically split the export
            // into varying and unvarying data, unless the user explicitly requested unvarying.
            if (exportUnvarying && scene.Time != null)
            {
                double? oldTime = scene.Time;
                scene.Time = null;
                Export(root, context, zeroRootTransform);
                scene.Time = oldTime;
            }

            // Export data for the requested time.
            context.exportMaterials = exportMaterials;
            Export(root, context, zeroRootTransform);
        }

        public static void Export(GameObject root,
            ExportContext context,
            bool zeroRootTransform)
        {
            // Remove parent transform effects while exporting.
            // This must be restored before returning from this function.
            var parent = root.transform.parent;
            if (zeroRootTransform)
            {
                root.transform.SetParent(null, worldPositionStays: false);
            }

            // Also zero out and restore local rotations on the root.
            var localPos = root.transform.localPosition;
            var localRot = root.transform.localRotation;
            var localScale = root.transform.localScale;
            if (zeroRootTransform)
            {
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;
            }

            // Scale overall scene for export (e.g. USDZ export needs scale 100)
            root.transform.localScale *= context.scale;

            UnityEngine.Profiling.Profiler.BeginSample("USD: Export");
            try
            {
                ExportImpl(root, context);
                var path = new pxr.SdfPath(UnityTypeConverter.GetPath(root.transform));
                var prim = context.scene.Stage.GetPrimAtPath(path);
                if (prim)
                {
                    context.scene.Stage.SetDefaultPrim(prim);
                }
            }
            finally
            {
                if (zeroRootTransform)
                {
                    root.transform.localPosition = localPos;
                    root.transform.localRotation = localRot;
                    root.transform.localScale = localScale;
                    root.transform.SetParent(parent, worldPositionStays: false);
                }
                else
                {
                    root.transform.localScale = localScale;
                }

                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private static void ExportImpl(GameObject root,
            ExportContext context)
        {
            var scene = context.scene;
            bool skipInactive = context.activePolicy == ActiveExportPolicy.DoNotExport;

            if (context.exportMaterials)
            {
                // TODO: should account for skipped objects and also skip their materials.
                UnityEngine.Profiling.Profiler.BeginSample("USD: Export Materials");
                foreach (var kvp in context.matMap)
                {
                    Material mat = kvp.Key;
                    string usdPath = kvp.Value;
                    if (!mat || usdPath == null)
                    {
                        continue;
                    }

                    try
                    {
                        MaterialExporter.ExportMaterial(scene, kvp.Key, kvp.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(new Exception("Error exporting material: " + kvp.Value, ex));
                    }
                }

                UnityEngine.Profiling.Profiler.EndSample();
            }

            UnityEngine.Profiling.Profiler.BeginSample("USD: Process Export Plans");
            foreach (var kvp in context.plans)
            {
                GameObject go = kvp.Key;
                ExportPlan exportPlan = kvp.Value;

                if (!go || exportPlan == null)
                {
                    continue;
                }

                if (go != root && !go.transform.IsChildOf(root.transform))
                {
                    continue;
                }

                if (skipInactive && go.activeInHierarchy == false)
                {
                    continue;
                }

                foreach (Exporter exporter in exportPlan.exporters)
                {
                    string path = exporter.path;
                    SampleBase sample = exporter.sample;
                    var objCtx = new ObjectContext
                    {
                        gameObject = go,
                        path = path,
                        sample = sample,
                        additionalData = exporter.data
                    };

                    try
                    {
                        exporter.exportFunc(objCtx, context);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(new Exception("Error exporting: " + path, ex));
                        continue;
                    }

                    UnityEngine.Profiling.Profiler.BeginSample("USD: Process Visibility");
                    try
                    {
                        if (!go.gameObject.activeSelf)
                        {
                            switch (context.activePolicy)
                            {
                                case ActiveExportPolicy.Ignore:
                                    // Nothing to see here.
                                    break;

                                case ActiveExportPolicy.ExportAsVisibility:
                                    // Make the prim invisible.
                                    var im = new pxr.UsdGeomImageable(scene.GetPrimAtPath(path));
                                    if (im)
                                    {
                                        im.CreateVisibilityAttr().Set(pxr.UsdGeomTokens.invisible);
                                    }

                                    break;

                                case ActiveExportPolicy.ExportAsActive:
                                    // TODO: this may actually cause errors because exported prims will not exist in
                                    // the USD scene graph. Right now, that's too much responsibility on the caller,
                                    // because the error messages will be mysterious.

                                    // Make the prim inactive.
                                    scene.GetPrimAtPath(path).SetActive(false);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(new Exception("Error setting visibility: " + path, ex));
                        continue;
                    }

                    UnityEngine.Profiling.Profiler.EndSample();
                } // foreach exporter
            } // foreach plan

            UnityEngine.Profiling.Profiler.EndSample();
        }

        // ------------------------------------------------------------------------------------------ //
        // Init Hierarchy.
        // ------------------------------------------------------------------------------------------ //

        static void Traverse(GameObject obj,
            ObjectProcessor processor,
            ExportContext context)
        {
            processor(obj, context);
            foreach (Transform child in obj.transform)
            {
                Traverse(child.gameObject, processor, context);
            }
        }

        static void AccumNestedBones(Transform curXf,
            List<Transform> children,
            ExportContext ctx)
        {
            if (ctx.bindPoses.ContainsKey(curXf))
            {
                children.Add(curXf);
            }

            foreach (Transform child in curXf.transform)
            {
                AccumNestedBones(child, children, ctx);
            }
        }

        static T CreateSample<T>(ExportContext context) where T : SampleBase, new()
        {
            return new T();
            /*
            SampleBase sb;
            if (context.samples.TryGetValue(typeof(T), out sb)) {
              return (T)sb;
            }

            sb = (new T());
            context.samples[typeof(T)] = sb;
            return (T)sb;
            */
        }

        public static void SyncExportContext(GameObject exportRoot,
            ExportContext context)
        {
            context.exportRoot = exportRoot.transform.parent;
            Traverse(exportRoot, InitExportableObjects, context);

            Transform expRoot = context.exportRoot;
            var foundAnimators = new List<Transform>();
            foreach (var rootBoneXf in context.meshToSkelRoot.Values.ToArray())
            {
                bool alreadyProcessed = false;
                foreach (var xf in foundAnimators)
                {
                    if (rootBoneXf.IsChildOf(xf))
                    {
                        alreadyProcessed = true;
                        break;
                    }
                }

                if (alreadyProcessed)
                {
                    continue;
                }

                var animatorXf = rootBoneXf;

                while (animatorXf != null)
                {
                    // If there is an animator, assume this is the root of the rig.
                    // This feels very ad hoc, it would be nice to not use a heuristic.
                    var anim = animatorXf.GetComponent<Animator>();
                    if (anim != null)
                    {
                        // Any root bones under this animator will be merged into their most common ancestor,
                        // which is returned here and becomes the skeleton root.
                        Transform skeletonRoot = MergeBonesBelowAnimator(animatorXf, context);

                        if (skeletonRoot == null)
                        {
                            animatorXf = animatorXf.parent;
                            Debug.LogWarning("No children found under animator: " +
                                UnityTypeConverter.GetPath(animatorXf) + " Root bone XF: " +
                                UnityTypeConverter.GetPath(rootBoneXf));
                            continue;
                        }

                        foundAnimators.Add(anim.transform);

                        // The skeleton is exported at the skeleton root and UsdSkelAnimation is nested under
                        // this prim as a new prim called "_anim".
                        SkelRootSample rootSample = CreateSample<SkelRootSample>(context);
                        string skelRootPath = UnityTypeConverter.GetPath(animatorXf.transform, expRoot);
                        string skelPath = UnityTypeConverter.GetPath(skeletonRoot, expRoot);
                        string skelPathSuffix = "";
                        string skelAnimSuffix = "/_anim";

                        // When there is a collision between the SkelRoot and the Skeleton, make a new USD Prim
                        // for the Skeleton object. The reason this is safe is as follows: if the object was
                        // imported from USD, then the structure should already be correct and this code path will
                        // not be hit (and hence overrides, etc, will work correctly). If the object was created
                        // in Unity and there happened to be a collision, then we can safely create a new prim
                        // for the Skeleton prim because there will be no existing USD skeleton for which
                        // the namespace must match, hence adding a new prim is still safe.
                        if (skelPath == skelRootPath)
                        {
                            Debug.LogWarning("SkelRoot and Skeleton have the same path, renaming Skeleton");
                            skelPathSuffix = "/_skel";
                        }

                        rootSample.animationSource = skelPath + skelAnimSuffix;

                        // For any skinned mesh exported under this SkelRoot, pass along the skeleton path in
                        // the "additional data" member of the exporter. Note that this feels very ad hoc and
                        // should probably be formalized in some way (perhaps as a separate export event for
                        // which the SkinnedMesh exporter can explicitly register).
                        //
                        // While it is possible to bind the skel:skeleton relationship at the SkelRoot and
                        // have it inherit down namespace, the Apple importer did not respect this inheritance
                        // and it sometimes causes issues with geometry embedded in the bone hierarchy.
                        foreach (var p in context.plans)
                        {
                            if (p.Key.transform.IsChildOf(animatorXf.transform))
                            {
                                foreach (var e in p.Value.exporters)
                                {
                                    if (e.exportFunc == MeshExporter.ExportSkinnedMesh)
                                    {
                                        e.data = skelPath + skelPathSuffix;
                                    }
                                }
                            }
                        }

                        CreateExportPlan(
                            animatorXf.gameObject,
                            rootSample,
                            SkeletonExporter.ExportSkelRoot,
                            context,
                            insertFirst: true);
                        CreateExportPlan(
                            animatorXf.gameObject,
                            rootSample,
                            NativeExporter.ExportObject,
                            context,
                            insertFirst: false);

                        CreateExportPlan(
                            skeletonRoot.gameObject,
                            CreateSample<SkeletonSample>(context),
                            SkeletonExporter.ExportSkeleton,
                            context,
                            insertFirst: true,
                            pathSuffix: skelPathSuffix);
                        CreateExportPlan(
                            skeletonRoot.gameObject,
                            CreateSample<SkeletonSample>(context),
                            NativeExporter.ExportObject,
                            context,
                            insertFirst: false,
                            pathSuffix: skelPathSuffix);

                        CreateExportPlan(
                            skeletonRoot.gameObject,
                            CreateSample<SkelAnimationSample>(context),
                            SkeletonExporter.ExportSkelAnimation,
                            context,
                            insertFirst: true,
                            pathSuffix: skelAnimSuffix);

                        // Exporting animation is only possible while in-editor (in 2018 and earlier).
#if UNITY_EDITOR
#if false // Currently disabled, future work.
                        if (anim.layerCount > 0)
                        {
                            for (int l = 0; l < anim.layerCount; l++)
                            {
                                int clipCount = anim.GetCurrentAnimatorClipInfoCount(l);
                                var clipInfos = anim.GetCurrentAnimatorClipInfo(l);
                                foreach (var clipInfo in clipInfos)
                                {
                                    var bindings = UnityEditor.AnimationUtility.GetCurveBindings(clipInfo.clip);
                                    // Properties are expressed as individual values, for transforms this is:
                                    //   m_LocalPosition.x,y,z
                                    //   m_LocalScale.x,y,z
                                    //   m_LocalRotation.x,y,z,w
                                    // Which means they must be reaggregated into matrices.
                                    foreach (var binding in bindings)
                                    {
                                        if (binding.type != typeof(Transform))
                                        {
                                            continue;
                                        }
                                        Debug.Log(binding.path + "." + binding.propertyName);
                                        var knot = UnityEditor.AnimationUtility.GetEditorCurve(clipInfo.clip, binding);
                                    }
                                }
                            }
                        }
#endif // disabled.
#endif // Editor only.

                        break;
                    }

                    animatorXf = animatorXf.parent;
                }
            }
        }

        static void InitExportableObjects(GameObject go,
            ExportContext context)
        {
            if (context.exportTransformOverrides)
            {
                CreateExportPlan(go, CreateSample<XformSample>(context), XformExporter.ExportXform, context);
            }
            else
            {
                var smr = go.GetComponent<SkinnedMeshRenderer>();

                var mr = go.GetComponent<MeshRenderer>();
                var mf = go.GetComponent<MeshFilter>();
                var cam = go.GetComponent<Camera>();
                Transform expRoot = context.exportRoot;

                var tmpPath = new pxr.SdfPath(UnityTypeConverter.GetPath(go.transform, expRoot));
                while (!tmpPath.IsRootPrimPath())
                {
                    tmpPath = tmpPath.GetParentPath();
                }

                // TODO: What if this path is in use?
                string materialBasePath = tmpPath.ToString() + "/Materials/";

                // Ensure the "Materials" prim is defined with a valid prim type.
                context.scene.Write(materialBasePath.TrimEnd('/'), new ScopeSample());

                if (smr != null)
                {
                    foreach (var mat in smr.sharedMaterials)
                    {
                        if (!context.matMap.ContainsKey(mat))
                        {
                            string usdPath = materialBasePath +
                                pxr.UsdCs.TfMakeValidIdentifier(
                                mat.name + "_" + mat.GetInstanceID().ToString());
                            context.matMap.Add(mat, usdPath);
                        }
                    }

                    CreateExportPlan(go, CreateSample<MeshSample>(context), MeshExporter.ExportSkinnedMesh, context);
                    CreateExportPlan(go, CreateSample<MeshSample>(context), NativeExporter.ExportObject, context,
                        insertFirst: false);
                    if (smr.rootBone == null)
                    {
                        Debug.LogWarning("No root bone at: " + UnityTypeConverter.GetPath(go.transform, expRoot));
                    }
                    else if (smr.bones == null || smr.bones.Length == 0)
                    {
                        Debug.LogWarning("No bones at: " + UnityTypeConverter.GetPath(go.transform, expRoot));
                    }
                    else
                    {
                        // Each mesh in a model may have a different root bone, which now must be merged into a
                        // single skeleton for export to USD.
                        try
                        {
                            MergeBonesSimple(smr.transform, smr.rootBone, smr.bones, smr.sharedMesh.bindposes, context);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(
                                new Exception("Failed to merge bones for " + UnityTypeConverter.GetPath(smr.transform),
                                    ex));
                        }
                    }
                }
                else if (mf != null && mr != null)
                {
                    foreach (var mat in mr.sharedMaterials)
                    {
                        if (mat == null)
                        {
                            continue;
                        }

                        if (!context.matMap.ContainsKey(mat))
                        {
                            string usdPath = materialBasePath +
                                pxr.UsdCs.TfMakeValidIdentifier(
                                mat.name + "_" + mat.GetInstanceID().ToString());
                            context.matMap.Add(mat, usdPath);
                        }
                    }

                    CreateExportPlan(go, CreateSample<MeshSample>(context), MeshExporter.ExportMesh, context);
                    CreateExportPlan(go, CreateSample<MeshSample>(context), NativeExporter.ExportObject, context,
                        insertFirst: false);
                }
                else if (cam)
                {
                    CreateExportPlan(go, CreateSample<CameraSample>(context), CameraExporter.ExportCamera, context);
                    CreateExportPlan(go, CreateSample<CameraSample>(context), NativeExporter.ExportObject, context,
                        insertFirst: false);
                }
            }
        }

        static Transform MergeBonesBelowAnimator(Transform animator, ExportContext context)
        {
            var toRemove = new Dictionary<Transform, Transform>();
            Transform commonRoot = null;

            foreach (var sourceAndRoot in context.meshToSkelRoot)
            {
                var meshXf = sourceAndRoot.Key;
                var meshRootBone = sourceAndRoot.Value;
                if (!meshRootBone.IsChildOf(animator))
                {
                    continue;
                }

                toRemove.Add(meshXf, meshRootBone);
                if (commonRoot == null)
                {
                    // We use the parent because the root bone is part of the skeleton and we're establishing
                    // the skeleton root here. If the root bone is used as the skeleton root, its transform
                    // will get applied twice after export to USD: once for the UsdPrim which is the skeleton
                    // root and once for the bone which is in the skeleton itself. The root bone could be
                    // excluded from the skeleton, but this seems simpler.
                    commonRoot = meshRootBone.parent;
                }
                else if (meshRootBone.IsChildOf(commonRoot))
                {
                    // Nothing to do.
                }
                else if (commonRoot.IsChildOf(meshRootBone))
                {
                    // The new root is a parent of the current common root, use it as the root instead.
                    commonRoot = meshRootBone.parent;
                }
                else
                {
                    // We have an animator which is a common parent of two disjoint skeletons, this is not
                    // desirable because it requires that the animator be the common root, however this
                    // root will be tagged as a guide, which will cuase the geometry not to render, which
                    // will be confusing. Another option would be to construct a new common parent in USD,
                    // but this will cause the asset namespace to change, which is almost never a good idea.
                    commonRoot = animator;
                }
            }

            if (toRemove.Count == 0)
            {
                return null;
            }

            // At this point, some number of root bones have been aggregated under some potentially new
            // common root. Next, we need to merge all these root bones and preserve the requirement that
            // the bones are in "parent first" order.
            var allBones = new List<Transform>();

            foreach (var kvp in toRemove)
            {
                Transform curMeshXf = kvp.Key;
                Transform rootBone = kvp.Value;

                allBones.AddRange(context.meshToBones[curMeshXf]);

                // Downstream code will have a root bone and need to know how to make bone paths relative
                // to the new, arbitrary, common root which we have chosen.
                context.boneToRoot[rootBone] = commonRoot;

                context.meshToSkelRoot.Remove(curMeshXf);
                context.meshToBones.Remove(curMeshXf);
            }

            // Maintain a sorted list of bone names to ensure "parent first" ordering for UsdSkel.
            var allNames = allBones.Select(boneXf => UnityTypeConverter.GetPath(boneXf))
                .OrderBy(str => str)
                .Distinct()
                .ToList();
            context.skelSortedMap[commonRoot] = allNames;

            return commonRoot;
        }

        static void MergeBonesSimple(Transform source,
            Transform rootBone,
            Transform[] bones,
            Matrix4x4[] bindPoses,
            ExportContext context)
        {
            context.meshToSkelRoot.Add(source, rootBone);
            context.meshToBones.Add(source, bones);
            Matrix4x4 existingMatrix;
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                if (bone == null)
                {
                    var srcPath = UnityTypeConverter.GetPath(source);
                    Debug.LogWarning("Null bone at in bones list at position (" + i + ") " + srcPath);
                    continue;
                }

                var path = UnityTypeConverter.GetPath(bone);
                context.pathToBone[path] = bone;
                context.boneToRoot[bone] = rootBone;
                context.bindPoses[bone] = bindPoses[i];
                if (context.bindPoses.TryGetValue(bone, out existingMatrix) && existingMatrix != bindPoses[i])
                {
                    Debug.LogWarning("Duplicate bone with different bind poses: " + path + "\n" +
                        existingMatrix.ToString() + "\n" + bindPoses[i].ToString());
                }
            }
        }

        static void CreateExportPlan(GameObject go,
            SampleBase sample,
            ExportFunction exportFunc,
            ExportContext context,
            string pathSuffix = null,
            bool insertFirst = true)
        {
            // This is an exportable object.
            Transform expRoot = context.exportRoot;
            string path = UnityTypeConverter.GetPath(go.transform, expRoot);
            if (!string.IsNullOrEmpty(pathSuffix))
            {
                path += pathSuffix;
            }

            if (!context.plans.ContainsKey(go))
            {
                context.plans.Add(go, new ExportPlan());
            }

            var exp = new Exporter { exportFunc = exportFunc, sample = sample, path = path };
            if (insertFirst)
            {
                context.plans[go].exporters.Insert(0, exp);
            }
            else
            {
                context.plans[go].exporters.Add(exp);
            }

            // Include the parent xform hierarchy.
            // Note that the parent hierarchy is memoised, so despite looking expensive, the time
            // complexity is linear.
            Transform xf = go.transform.parent;
            if (xf != context.exportRoot && !context.plans.ContainsKey(xf.gameObject))
            {
                // Since all GameObjects have a Transform, export all un-exported parents as transform.
                CreateExportPlan(xf.gameObject, CreateSample<XformSample>(context), XformExporter.ExportXform, context);
                CreateExportPlan(xf.gameObject, CreateSample<XformSample>(context), NativeExporter.ExportObject,
                    context, insertFirst: false);
            }
        }

        static Matrix4x4 ComputeWorldXf(Transform curBone, ExportContext context)
        {
            if (!context.bindPoses.ContainsKey(curBone))
            {
                return curBone.parent.localToWorldMatrix;
            }

            return context.bindPoses[curBone] * ComputeWorldXf(curBone.parent, context);
        }
    }
}

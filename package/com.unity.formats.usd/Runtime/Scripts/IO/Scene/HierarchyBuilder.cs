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
using UnityEngine.Profiling;
using pxr;
using USD.NET;
using USD.NET.Unity;

#if !UNITY_2017
using Unity.Jobs;
#endif

namespace Unity.Formats.USD {

  /// <summary>
  /// A collection of methods for building the USD scene hierarchy in Unity.
  /// </summary>
  public static class HierarchyBuilder {
    static readonly SdfPath kAbsoluteRootPath = SdfPath.AbsoluteRootPath();

    struct HierInfo {
      public bool isVisible;
      public bool isInstance;
      public bool isAssembly;
      public bool isModel;
      public bool hasPayload;
      public UsdPrim prim;
      public SdfPath[] skelJoints;
      public string modelAssetPath;
      public string modelName;
      public string modelVersion;
    }

    struct ReadHierJob
#if !UNITY_2017
      : IJobParallelFor
#endif
      {
      public static HierInfo[] result;
      public static Scene scene;
      public static SdfPath[] paths;

      public void Run() {
        for (int i = 0; i < paths.Length; i++) {
          Execute(i);
        }
      }

      public void Execute(int index) {
        HierInfo info = new HierInfo();
        info.prim = scene.Stage.GetPrimAtPath(paths[index]);

        if (!info.prim) {
          info.prim = null;
          return;
        }

        info.isVisible = HierarchyBuilder.IsVisible(info.prim);
        info.isInstance = info.prim.IsInstance();
        HierarchyBuilder.ReadModelInfo(ref info);
        HierarchyBuilder.ReadSkeleton(ref info);
        info.hasPayload = info.prim.GetPrimIndex().HasPayload();

        result[index] = info;
      }
    }

    struct FindPathsJob
#if !UNITY_2017
      : IJobParallelFor
#endif
      {
      public interface IQuery {
        SdfPath[] Find(Scene scene, SdfPath usdRoot);
      }
      public struct Query<T> : IQuery where T : SampleBase, new() {
        public SdfPath[] Find(Scene scene, SdfPath usdRoot) {
          return scene.Find<T>(usdRoot);
        }
      }
      public static SdfPath usdRoot;
      public static Scene scene;
      public static SdfPath[][] results;
      public static IQuery[] queries;

      public void Run() {
        for (int i = 0; i < queries.Length; i++) {
          Execute(i);
        }
      }

      public void Execute(int index) {
        var query = queries[index];
        if (query == null) { return; }
        results[index] = query.Find(scene, usdRoot);
      }
    }

    static
#if !UNITY_2017
      JobHandle
#else
      void
#endif
      BeginReading(Scene scene,
                                    SdfPath usdRoot,
                                    PrimMap map,
                                    SceneImportOptions options) {
      FindPathsJob.usdRoot = usdRoot;
      FindPathsJob.scene = scene;
      FindPathsJob.results = new SdfPath[7][];
      FindPathsJob.queries = new FindPathsJob.IQuery[7];

      if (options.ShouldBindMaterials) {
        FindPathsJob.queries[0] = (FindPathsJob.IQuery)new FindPathsJob.Query<MaterialSample>();
      }
      if (options.importCameras) {
        FindPathsJob.queries[1] = (FindPathsJob.IQuery)new FindPathsJob.Query<CameraSample>();
      }
      if (options.importMeshes) {
        FindPathsJob.queries[2] = (FindPathsJob.IQuery)new FindPathsJob.Query<MeshSample>();
        FindPathsJob.queries[3] = (FindPathsJob.IQuery)new FindPathsJob.Query<CubeSample>();
      }

      FindPathsJob.queries[4] = (FindPathsJob.IQuery)new FindPathsJob.Query<SkelRootSample>();

      if (options.importSkinning) {
        FindPathsJob.queries[5] = (FindPathsJob.IQuery)new FindPathsJob.Query<SkeletonSample>();
      }
      if (options.importTransforms) {
        FindPathsJob.queries[6] = (FindPathsJob.IQuery)new FindPathsJob.Query<XformSample>();
      }

      var findPathsJob = new FindPathsJob();
#if !UNITY_2017
      var findHandle = findPathsJob.Schedule(FindPathsJob.queries.Length, 1);
      findHandle.Complete();
#else
      findPathsJob.Run();
#endif

      map.Materials = FindPathsJob.results[0];
      map.Cameras = FindPathsJob.results[1];
      map.Meshes = FindPathsJob.results[2];
      map.Cubes = FindPathsJob.results[3];
      map.SkelRoots = FindPathsJob.results[4];
      map.Skeletons = FindPathsJob.results[5];
      map.Xforms = FindPathsJob.results[6];

      ReadHierJob.paths = FindPathsJob.results.Where(i => i != null).SelectMany(i => i).ToArray();
      ReadHierJob.result = new HierInfo[ReadHierJob.paths.Length];
      ReadHierJob.scene = scene;
      var readHierInfo = new ReadHierJob();
#if !UNITY_2017
      return readHierInfo.Schedule(ReadHierJob.paths.Length, 8, dependsOn: findHandle);
#else
      readHierInfo.Run();
      return;
#endif
    }

    static HierInfo[] BuildObjectLists(Scene scene,
                                       GameObject unityRoot,
                                       SdfPath usdRoot,
                                       PrimMap map,
                                       SceneImportOptions options) {

#if !UNITY_2017
      BeginReading(scene, usdRoot, map, options).Complete();
#else
      BeginReading(scene, usdRoot, map, options);
#endif

      ProcessPaths(ReadHierJob.result, scene, unityRoot, usdRoot, map, options);

      return ReadHierJob.result;
    }

    /// <summary>
    /// Map all UsdPrims and build Unity GameObjects, reconstructing the parent relationship.
    /// </summary>
    /// <remarks>
    /// When forceRebuild is true, game objects will be destroyed and recreated. If buildHierarchy
    /// is false, the primMap will be populated, but missing game objects will not be created.
    /// </remarks>
    static public PrimMap BuildGameObjects(Scene scene,
                                           GameObject unityRoot,
                                           SdfPath usdRoot,
                                           IEnumerable<SdfPath> paths,
                                           PrimMap map,
                                           SceneImportOptions options) {
      map[usdRoot] = unityRoot;

      Profiler.BeginSample("Build Object Lists");
      var hierInfo = BuildObjectLists(scene, unityRoot, usdRoot, map, options);
      Profiler.EndSample();

      // TODO: Should recurse to discover deeply nested instancing.
      // TODO: Generates garbage for every prim, but we expect few masters.
      if (options.importPointInstances || options.importSceneInstances) {
        Profiler.BeginSample("Build Masters");
        foreach (var masterRootPrim in scene.Stage.GetMasters()) {
          var goMaster = FindOrCreateGameObject(unityRoot.transform,
                                                masterRootPrim.GetPath(),
                                                unityRoot.transform,
                                                map,
                                                options);

          goMaster.hideFlags = HideFlags.HideInHierarchy;
          goMaster.SetActive(false);
          map.AddMasterRoot(masterRootPrim.GetPath(), goMaster);
          try {
            var info = new HierInfo();
            info.prim = masterRootPrim;
            ReadModelInfo(ref info);
            AddModelRoot(goMaster, info);
            AddVariantSet(goMaster, masterRootPrim);
          } catch (Exception ex) {
            Debug.LogException(new Exception("Error processing " + masterRootPrim.GetPath(), ex));
          }

          foreach (var usdPrim in masterRootPrim.GetDescendants()) {
            var parentPath = usdPrim.GetPath().GetParentPath();
            Transform parentXf = null;
            if (parentPath == masterRootPrim.GetPath()) {
              parentXf = goMaster.transform;
            } else {
              parentXf = map[parentPath].transform;
            }

            var goPrim = FindOrCreateGameObject(parentXf,
                                                usdPrim.GetPath(),
                                                unityRoot.transform,
                                                map,
                                                options);
            ApplySelfVisibility(goPrim, usdPrim);

            if (usdPrim.IsInstance()) {
              map.AddInstanceRoot(usdPrim.GetPath(), goPrim, usdPrim.GetMaster().GetPath());
            }

            try {
              var info = new HierInfo();
              info.prim = usdPrim;
              ReadModelInfo(ref info);
              AddModelRoot(goPrim, info);
              AddVariantSet(goPrim, usdPrim);
            } catch (Exception ex) {
              Debug.LogException(new Exception("Error processing " + usdPrim.GetPath(), ex));
              continue;
            }
          }
        }
        Profiler.EndSample();
      }

      if (options.importSkinning) {
        Profiler.BeginSample("Expand Skeletons");
        foreach (var info in hierInfo) { 
          if (info.skelJoints == null || info.skelJoints.Length == 0) {
            continue;
          }

          try {
            ExpandSkeleton(info, unityRoot, usdRoot, info.prim, map, options);
          } catch (Exception ex) {
            Debug.LogException(new Exception("Error expanding skeleton at " + info.prim.GetPath(), ex));
          }
        }
        Profiler.EndSample();
      }

      return map;
    }

    static bool IsVisible(UsdPrim usdPrim) {
      if (!usdPrim) { return false; }

      var img = new UsdGeomImageable(usdPrim);
      if (!img) { return true; }

      // Using time=0.0 will enable this to pickup a single animated value by virtue of
      // interpolation, but correct handling of animated visibility would query it over time.

      // Also note this code is intentionally not using ComputeVisibility(), since Unity will apply
      // inherited visibility. This is technically incorrect, since USD supports "super vis",
      // enabling visible children with invisible parents. The goal here is to avoid spamming all
      // descendent children with active=false. A better implementation would check for visible
      // children with invisible parents and somehow translate that to the Unity scenegraph, but
      // is left as a future improvement.

      VtValue visValue = new VtValue();
      if (!img.GetVisibilityAttr().Get(visValue, 0.0)) {
        return true;
      }

      return UsdCs.VtValueToTfToken(visValue) != UsdGeomTokens.invisible;
    }

    static void ApplySelfVisibility(GameObject go, UsdPrim usdPrim) {
      if (!go) { return; }

      if (IsVisible(usdPrim)) {
        return;
      }

      go.SetActive(false);
    }

    // Creates ancestors, but note that this method does not apply visibility, since it was
    // designed to create bones, which cannot have visibility opinions in USD.
    static void CreateAncestors(SdfPath path,
                                PrimMap map,
                                GameObject unityRoot,
                                SdfPath usdRoot,
                                SceneImportOptions options,
                                out GameObject parentGo) {
      var parentPath = path.GetParentPath();
      if (path == parentPath) {
        Debug.LogException(new Exception("Parent path was identical to current path: " + path.ToString()));
        parentGo = null;
        return;
      }
      if (map.TryGetValue(parentPath, out parentGo) && parentGo) {
        return;
      }

      // Base case.
      if (parentPath == usdRoot) {
        map[parentPath] = unityRoot;
        return;
      }

      if (parentPath == kAbsoluteRootPath) {
        // Something went wrong.
        Debug.LogException(new Exception(
            "Error: unexpected path </> creating ancestors for <" + usdRoot.ToString() + ">"));
      }

      // Recursive case.
      // First, get the grandparent (parent's parent).
      GameObject grandparentGo;
      CreateAncestors(parentPath, map, unityRoot, usdRoot, options, out grandparentGo);
      if (!grandparentGo) {
        Debug.LogError("Failed to find ancestor for " + parentPath);
        return;
      }

      // Then find/create the current parent.
      parentGo = FindOrCreateGameObject(grandparentGo.transform,
                                        parentPath,
                                        unityRoot.transform,
                                        map,
                                        options);
    }

    static void ProcessPaths(HierInfo[] infos,
                             Scene scene,
                             GameObject unityRoot,
                             SdfPath usdRoot,
                             PrimMap map,
                             SceneImportOptions options) {
      Profiler.BeginSample("Process all paths");
      foreach (var info in infos) {
        var prim = info.prim;
        var path = info.prim.GetPath();

        GameObject go;
        if (path == usdRoot) {
          go = unityRoot;
        } else {
          GameObject parentGo = null;
          CreateAncestors(path, map, unityRoot, usdRoot, options, out parentGo);

          if (!parentGo) {
            Debug.LogWarning("Parent path not found for child: " + path.ToString());
            continue;
          }

          var parent = parentGo ? parentGo.transform : null;
          if (!map.TryGetValue(path, out go)) {
            go = FindOrCreateGameObject(parent,
                                        path,
                                        unityRoot.transform,
                                        map,
                                        options);
          }
        }

        if (options.importSceneInstances) {
          Profiler.BeginSample("Add Scene Instance Root");
          if (prim.IsInstance()) {
            map.AddInstanceRoot(prim.GetPath(), go, prim.GetMaster().GetPath());
          }
          Profiler.EndSample();
        }

        if (!options.importHierarchy) {
          continue;
        }

        ApplySelfVisibility(go, prim);

        try {
          Profiler.BeginSample("Add Model Root");
          AddModelRoot(go, info);
          Profiler.EndSample();

          Profiler.BeginSample("Add Variant Set");
          AddVariantSet(go, prim);
          Profiler.EndSample();

          Profiler.BeginSample("Add Payload");
          AddPayload(go, info, options);
          Profiler.EndSample();
        } catch (Exception ex) {
          Debug.LogException(new Exception("Error processing " + prim.GetPath(), ex));
        }
      }
      Profiler.EndSample();
    }

    static void ReadSkeleton(ref HierInfo info) {
      if (info.prim == null) { return; }

      var skelRoot = new UsdSkelRoot(info.prim);
      if (!skelRoot) { return; }

      var skelRel = info.prim.GetRelationship(UsdSkelTokens.skelSkeleton);
      if (!skelRel) { return; }

      SdfPathVector targets = skelRel.GetForwardedTargets();
      if (targets == null || targets.Count == 0) { return; }

      var skelPrim = info.prim.GetStage().GetPrimAtPath(targets[0]);
      if (!skelPrim) { return; }

      var skel = new UsdSkelSkeleton(skelPrim);
      if (!skel) { return; }

      var jointsAttr = skel.GetJointsAttr();
      if (!jointsAttr) { return; }

      var vtJoints = jointsAttr.Get();
      if (vtJoints.IsEmpty()) { return; }
      var vtStrings = UsdCs.VtValueToVtTokenArray(vtJoints);
      var joints = UnityTypeConverter.FromVtArray(vtStrings);

      var skelPath = skelPrim.GetPath();
      info.skelJoints = new SdfPath[joints.Length];

      for (int i = 0; i < joints.Length; i++) {
        var jointPath = new SdfPath(joints[i]);
        if (joints[i] == "/") {
          info.skelJoints[i] = skelPath;
          continue;
        } else if (jointPath.IsAbsolutePath()) {
          Debug.LogException(new Exception("Unexpected absolute joint path: " + jointPath));
          jointPath = new SdfPath(joints[i].TrimStart('/'));
        }
        info.skelJoints[i] = skelPath.AppendPath(jointPath);
      }
    }

    /// <summary>
    /// Given an array of bone names (HierInfo.skelJoints), creates GameObjects under unityRoot.
    /// </summary>
    static void ExpandSkeleton(HierInfo info,
                               GameObject unityRoot,
                               SdfPath usdRoot,
                               UsdPrim prim,
                               PrimMap map,
                               SceneImportOptions options) {
      foreach (var joint in info.skelJoints) {
        var path = joint;
        GameObject parentGo = null;
        if (!map.TryGetValue(path.GetParentPath(), out parentGo)) {
          // This will happen when the joints are discontinuous, for example:
          //
          //   Foo/Bar
          //   Foo/Bar/Baz/Qux
          //
          // Baz is implicitly defined, which is allowed by UsdSkel.
          CreateAncestors(path, map, unityRoot, usdRoot, options, out parentGo);
          if (!parentGo) {
            Debug.LogException(new Exception("Failed to create ancestors for " + path + " for prim: " + prim.GetPath()));
            continue;
          }
        }

        Transform child = parentGo.transform.Find(path.GetName());
        if (!child) {
          child = new GameObject(path.GetName()).transform;
          child.SetParent(parentGo.transform, worldPositionStays: false);
        }

        map[path] = child.gameObject;
      }
    }

    static void ReadModelInfo(ref HierInfo info) {
      if (!info.prim) {
        return;
      }

      var modelApi = new UsdModelAPI(info.prim);
      if (!modelApi) { return; }

      var kindTok = new TfToken();
      if (!modelApi.GetKind(kindTok)) {
        return;
      }

      if (KindRegistry.IsA(kindTok, KindTokens.assembly)) {
        info.isAssembly = true;
      } else if (!modelApi.IsModel() || modelApi.IsGroup()) {
        return;
      }

      var modelInfo = new VtDictionary();
      if (!modelApi.GetAssetInfo(modelInfo)) {
        return;
      }

      info.isModel = true;

      var valName = modelInfo.GetValueAtPath("name");
      var valVersion = modelInfo.GetValueAtPath("version");
      var valIdentifier = modelInfo.GetValueAtPath("identifier");

      if (valIdentifier != null && !valIdentifier.IsEmpty()) {
        info.modelAssetPath = UsdCs.VtValueToSdfAssetPath(valIdentifier).GetAssetPath().ToString();
      }

      if (valName != null && !valName.IsEmpty()) {
        info.modelName = UsdCs.VtValueTostring(valName);
      }

      if (valVersion != null && !valVersion.IsEmpty()) {
        info.modelVersion = UsdCs.VtValueTostring(valVersion);
      }

    }

    /// <summary>
    /// Exposes model root and asset metadata. The game object is primarily a tag which is useful
    /// for smart selection of models instead of geometry.
    /// </summary>
    static void AddModelRoot(GameObject go, HierInfo info) {
      if (info.isAssembly) {
        var asm = go.GetComponent<UsdAssemblyRoot>();
        if (!asm) {
          go.AddComponent<UsdAssemblyRoot>();
        }
      } else if (info.isModel) {
        var mdl = go.GetComponent<UsdModelRoot>();
        if (!mdl) {
          mdl = go.AddComponent<UsdModelRoot>();
        }
        mdl.m_modelAssetPath = info.modelAssetPath;
        mdl.m_modelName = info.modelName;
        mdl.m_modelVersion = info.modelVersion;
      } else {
        // If these tags were added previously, remove them.
        var mdl = go.GetComponent<UsdModelRoot>();
        if (mdl) {
          Component.DestroyImmediate(mdl);
        }
        var asm = go.GetComponent<UsdAssemblyRoot>();
        if (asm) {
          Component.DestroyImmediate(asm);
        }
      }
    }

    /// <summary>
    /// If there is a Payload authored on this prim, expose it so the user can change the
    /// load state.
    /// </summary>
    static void AddPayload(GameObject go, HierInfo info, SceneImportOptions options) {
      var pl = go.GetComponent<UsdPayload>();

      if (!info.hasPayload) {
        if (pl) {
          Component.DestroyImmediate(pl);
        }
        return;
      }

      if (!pl) {
        pl = go.AddComponent<UsdPayload>();
        pl.SetInitialState(options.payloadPolicy == PayloadPolicy.LoadAll);
      }
    }

    /// <summary>
    /// If there is a variant set authored on this prim, expose it so the user can change the
    /// variant selection.
    /// </summary>
    static void AddVariantSet(GameObject go, UsdPrim prim) {
      var setNames = prim.GetVariantSets().GetNames();
      var vs = go.GetComponent<UsdVariantSet>();

      if (setNames.Count == 0) {
        if (vs) {
          Component.DestroyImmediate(vs);
        }
        return;
      }

      if (!vs) {
        vs = go.AddComponent<UsdVariantSet>();
      }

      vs.LoadFromUsd(prim);
    }

    /// <summary>
    /// Checks for a child named "name" under the given parent, if it exists it is returned,
    /// else a new child is created with this name.
    /// </summary>
    static GameObject FindOrCreateGameObject(Transform parent,
                                             SdfPath path,
                                             Transform unityRoot,
                                             PrimMap primMap,
                                             SceneImportOptions options) {
      Transform root = null;
      GameObject go = null;
      string name = path.GetName();

      if (parent == null) {
        go = GameObject.Find(name);
        root = go ? go.transform : null;
      } else {
        root = parent.Find(name);
        go = root ? root.gameObject : null;
      }

      if (!go) {
        // TODO: this should really not construct a game object if ImportHierarchy is false,
        // but it requires all downstream code be driven by the primMap instead of finding prims
        // via the usd scene. In addition, this requies the prim map to store lists of prims by
        // type, e.g. cameras, meshes, cubes, etc.
        go = new GameObject(name);
      }

      if (!go.GetComponent<UsdPrimSource>()) {
        var ua = go.AddComponent<UsdPrimSource>();
        ua.m_usdPrimPath = path.ToString();
      }

      if (parent != null) {
        go.transform.SetParent(parent, worldPositionStays: false);
      }

      Profiler.BeginSample("Add to PrimMap");
      primMap[path] = go;
      Profiler.EndSample();
      return go;
    }

  }
}

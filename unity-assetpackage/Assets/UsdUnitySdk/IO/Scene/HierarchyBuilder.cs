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
using pxr;

namespace USD.NET.Unity {

  /// <summary>
  /// A collection of methods for building the USD scene hierarchy in Unity.
  /// </summary>
  public static class HierarchyBuilder {

    /// <summary>
    /// Map all UsdPrims and build Unity GameObjects, reconstructing the parent relationship.
    /// </summary>
    /// <param name="scene">The Scene to map</param>
    /// <param name="unityRoot">The root game object under which all prims will be parented</param>
    /// <returns></returns>
    static public PrimMap BuildGameObjects(Scene scene,
                                           GameObject unityRoot) {
      return BuildGameObjects(scene, unityRoot, pxr.SdfPath.AbsoluteRootPath(), scene.AllPaths);
    }

    /// <summary>
    /// Map all UsdPrims and build Unity GameObjects, reconstructing the parent relationship.
    /// </summary>
    /// <param name="scene">The Scene to map</param>
    /// <param name="unityRoot">The root game object under which all prims will be parented</param>
    /// <param name="usdRoot">The path at which to begin mapping paths.</param>
    static public PrimMap BuildGameObjects(Scene scene,
                                           GameObject unityRoot,
                                           SdfPath usdRoot) {
      // TODO: add an API for finding paths.
      return BuildGameObjects(scene,
                              unityRoot,
                              usdRoot,
                              scene.Find(usdRoot.ToString(), "UsdSchemaBase"));
    }

    /// <summary>
    /// Private implementation of BuildGameObjects.
    /// </summary>
    static private PrimMap BuildGameObjects(Scene scene,
                                            GameObject unityRoot,
                                            pxr.SdfPath usdRoot,
                                            IEnumerable<SdfPath> paths) {
      var map = new PrimMap();
      map[usdRoot] = unityRoot;

      // TODO: Should recurse to discover deeply nested instancing.
      // TODO: Generates garbage for every prim, but we expect few masters.
      foreach (var masterRootPrim in scene.Stage.GetMasters()) {
        var goMaster = new GameObject(masterRootPrim.GetPath().GetName());

        goMaster.hideFlags = HideFlags.HideInHierarchy;
        goMaster.SetActive(false);
        if (unityRoot != null) {
          goMaster.transform.SetParent(unityRoot.transform, worldPositionStays: false);
        }
        map.AddMasterRoot(masterRootPrim.GetPath(), goMaster);
        try {
          AddModelRoot(goMaster, masterRootPrim);
          AddVariantSet(goMaster, masterRootPrim);
        } catch (Exception ex) {
          Debug.LogException(new Exception("Error processing " + masterRootPrim.GetPath(), ex));
        }

        foreach (var usdPrim in masterRootPrim.GetDescendants()) {
          var goPrim = new GameObject(usdPrim.GetName());
          try {
            AddModelRoot(goPrim, usdPrim);
            AddVariantSet(goPrim, usdPrim);
          } catch (Exception ex) {
            Debug.LogException(new Exception("Error processing " + usdPrim.GetPath(), ex));
          }

          if (usdPrim.IsInstance()) {
            map.AddInstanceRoot(usdPrim.GetPath(), goPrim, usdPrim.GetMaster().GetPath());
          }

          var parentPath = usdPrim.GetPath().GetParentPath();
          Transform parentXf = null;
          if (parentPath == masterRootPrim.GetPath()) {
            parentXf = goMaster.transform;
          } else {
            parentXf = map[parentPath].transform;
          }

          map[usdPrim.GetPath()] = goPrim;
          goPrim.transform.SetParent(parentXf);
        }
      }

      foreach (var path in paths) {
        var prim = scene.GetPrimAtPath(path);
        var parentGo = map[path.GetParentPath()];
        var parent = parentGo ? parentGo.transform : null;
        var go = FindOrCreateGameObject(parent, path.GetName());
        try {
          AddModelRoot(go, prim);
          AddVariantSet(go, prim);
        } catch (Exception ex) {
          Debug.LogException(new Exception("Error processing " + prim.GetPath(), ex));
        }

        if (prim.IsInstance()) {
          map.AddInstanceRoot(prim.GetPath(), go, prim.GetMaster().GetPath());
        }

        map[new SdfPath(path)] = go;
      }

      return map;
    }

    /// <summary>
    /// Exposes model root and asset metadata. The game object is primarily a tag which is useful
    /// for smart selection of models instead of geometry.
    /// </summary>
    static void AddModelRoot(GameObject go, UsdPrim prim) {
      if (!prim) {
        return;
      }

      var modelApi = new pxr.UsdModelAPI(prim);
      if (!modelApi) { return; }

      var kindTok = new TfToken();
      if (!modelApi.GetKind(kindTok)) {
        return;
      }

      if (KindRegistry.IsA(kindTok, KindTokens.assembly)) {
        var asm = go.GetComponent<UsdAssemblyRoot>();
        if (!asm) {
          go.AddComponent<UsdAssemblyRoot>();
        }
      } else if (modelApi.IsModel() && !modelApi.IsGroup()) {
        var mdl = go.GetComponent<UsdModelRoot>();
        if (!mdl) {
          mdl = go.AddComponent<UsdModelRoot>();
        }
        var info = new VtDictionary();
        if (modelApi.GetAssetInfo(info)) {
          var valName = info.GetValueAtPath("name");
          var valVersion = info.GetValueAtPath("version");
          var valIdentifier = info.GetValueAtPath("identifier");
          if (valIdentifier != null && !valIdentifier.IsEmpty()) {
            mdl.m_modelAssetPath = UsdCs.VtValueToSdfAssetPath(valIdentifier).GetAssetPath().ToString();
          }
          if (valName != null && !valName.IsEmpty()) {
            mdl.m_modelName = pxr.UsdCs.VtValueTostring(valName);
          }
          if (valVersion != null && !valVersion.IsEmpty()) {
            mdl.m_modelVersion = pxr.UsdCs.VtValueTostring(valVersion);
          }
        }
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
    /// If there is a variant set authored on this prim, expose it so the user can change the
    /// variant selection.
    /// </summary>
    static void AddVariantSet(GameObject go, UsdPrim prim) {
      var sets = prim.GetVariantSets();
      var setNames = sets.GetNames();
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

      vs.SyncVariants(prim, sets);
    }

    /// <summary>
    /// Checks for a child named "name" under the given parent, if it exists it is returned,
    /// else a new child is created with this name.
    /// </summary>
    static GameObject FindOrCreateGameObject(Transform parent, string name) {
      Transform root = null;
      GameObject go = null;
      if (parent == null) {
        go = GameObject.Find(name);
        root = go ? go.transform : null;
      } else {
        root = parent.Find(name);
        go = root ? root.gameObject : null;
      }
      
      if (!go) {
        go = new GameObject(name);
      }
      if (parent != null) {
        go.transform.SetParent(parent, worldPositionStays: false);
      }
      return go;
    }

  }
}

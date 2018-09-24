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
    static public PrimMap BuildGameObjects(Scene scene, GameObject unityRoot) {
      return BuildGameObjects(scene, unityRoot, scene.AllPaths);
    }

    /// <summary>
    /// Map all UsdPrims and build Unity GameObjects, reconstructing the parent relationship.
    /// </summary>
    /// <param name="scene">The Scene to map</param>
    /// <param name="unityRoot">The root game object under which all prims will be parented</param>
    /// <param name="rootPath">The path at which to begin mapping paths.</param>
    static public PrimMap BuildGameObjects(Scene scene, GameObject unityRoot, SdfPath rootPath) {
      // TODO: add an API for finding paths.
      return BuildGameObjects(scene, unityRoot, scene.Find(rootPath.ToString(), "UsdSchemaBase"));
    }

    /// <summary>
    /// Private implementation of BuildGameObjects.
    /// </summary>
    static private PrimMap BuildGameObjects(Scene scene,
                                            GameObject unityRoot,
                                            IEnumerable<SdfPath> paths) {
      var map = new PrimMap();
      map[SdfPath.AbsoluteRootPath()] = unityRoot;

      // TODO: Should recurse to discover deeply nested instancing.
      // TODO: Generates garbage for every prim, but we expect few masters.
      foreach (var masterRootPrim in scene.Stage.GetMasters()) {
        var goMaster = new GameObject(masterRootPrim.GetPath().GetName());

        goMaster.hideFlags = HideFlags.HideInHierarchy;
        goMaster.SetActive(false);
        goMaster.transform.SetParent(unityRoot.transform, worldPositionStays: false);
        map.AddMasterRoot(masterRootPrim.GetPath(), goMaster);

        foreach (var usdPrim in masterRootPrim.GetDescendants()) {
          var goPrim = new GameObject(usdPrim.GetName());

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
        var go = new GameObject(path.GetName());

        if (prim.IsInstance()) {
          map.AddInstanceRoot(prim.GetPath(), go, prim.GetMaster().GetPath());
        }

        map[new SdfPath(path)] = go;
        go.transform.SetParent(map[path.GetParentPath()].transform, worldPositionStays: false);
      }

      return map;
    }

  }
}

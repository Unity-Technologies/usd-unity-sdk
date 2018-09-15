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
using pxr;
using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// Maps from UsdPrim paths to Unity GameObjects.
  /// </summary>
  public class PrimMap {

    private Dictionary<SdfPath, GameObject> m_map = new Dictionary<SdfPath, GameObject>();

    public PrimMap() {
    }

    public GameObject this[SdfPath path]
    {
      get {
        GameObject go;
        if (m_map.TryGetValue(path, out go)) {
          return go;
        }
        throw new KeyNotFoundException("The path <" + path + "> does not exist in the PrimMap");
      }
      set { m_map[path] = value; }
    }

    /// <summary>
    /// Map all UsdPrims and build Unity GameObjects, reconstructing the parent relationship.
    /// </summary>
    /// <param name="scene">The Scene to map</param>
    /// <param name="unityRoot">The root game object under which all prims will be parented</param>
    /// <returns></returns>
    static public PrimMap MapAllPrims(Scene scene, GameObject unityRoot) {
      return MapAllPrims(scene, unityRoot, scene.AllPaths);
    }

    /// <summary>
    /// Map all UsdPrims and build Unity GameObjects, reconstructing the parent relationship.
    /// </summary>
    /// <param name="scene">The Scene to map</param>
    /// <param name="unityRoot">The root game object under which all prims will be parented</param>
    /// <param name="rootPath">The path at which to begin mapping paths.</param>
    static public PrimMap MapAllPrims(Scene scene, GameObject unityRoot, SdfPath rootPath) {
      // TODO: add an API for finding paths.
      return MapAllPrims(scene, unityRoot, scene.Find(rootPath.ToString(), "UsdSchemaBase"));
    }

    static private PrimMap MapAllPrims(Scene scene, GameObject unityRoot, IEnumerable<SdfPath> paths) {
      var map = new PrimMap();
      map[SdfPath.AbsoluteRootPath()] = unityRoot;

      foreach (var path in paths) {
        var go = new GameObject(path.GetName());
        map[new SdfPath(path)] = go;
        go.transform.SetParent(map[path.GetParentPath()].transform, worldPositionStays: false);
      }

      return map;
    }
  }
}

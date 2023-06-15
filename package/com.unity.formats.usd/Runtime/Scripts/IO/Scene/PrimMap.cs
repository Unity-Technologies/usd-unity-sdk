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
using pxr;
using UnityEngine;

namespace Unity.Formats.USD
{
    /// <summary>
    /// Maps from UsdPrim paths to Unity GameObjects.
    /// </summary>
    public class PrimMap : IEnumerable<KeyValuePair<SdfPath, GameObject>>, IEnumerable
    {
        public struct InstanceRoot
        {
            public GameObject gameObject;
            public SdfPath masterPath;
        }

        public UsdSkelCache SkelCache = null;
        public Dictionary<SdfPath, UsdSkelBindingVector> SkelBindings;

        public Dictionary<SdfPath, UsdSkelSkinningQuery> SkinningQueries =
            new Dictionary<SdfPath, UsdSkelSkinningQuery>();

        public Dictionary<SdfPath, MeshImporter.GeometrySubsets> MeshSubsets =
            new Dictionary<SdfPath, MeshImporter.GeometrySubsets>();

        public SdfPath[] Cameras { get; set; }
        public SdfPath[] Meshes { get; set; }
        public SdfPath[] Cubes { get; set; }
        public SdfPath[] Spheres { get; set; }
        public SdfPath[] Xforms { get; set; }
        public SdfPath[] SkelRoots { get; set; }
        public SdfPath[] Skeletons { get; set; }
        public SdfPath[] Materials { get; set; }

        // Flags for import analytics
        public bool ContainsPointInstances { get; set; }
        public bool HasErrors { get; set; } = false;

        // Normal objects in the hierarchy.
        private Dictionary<SdfPath, GameObject> m_prims = new Dictionary<SdfPath, GameObject>();

        // Objects at the root of an instanced sub-tree.
        // Instances may be found in masters as well.
        private Dictionary<SdfPath, InstanceRoot> m_instanceRoots = new Dictionary<SdfPath, InstanceRoot>();
        private HashSet<GameObject> m_instances = new HashSet<GameObject>();

        // Objects which exist only as source of instances.
        private Dictionary<SdfPath, GameObject> m_masterRoots = new Dictionary<SdfPath, GameObject>();

        public PrimMap()
        {
            ContainsPointInstances = false;
        }

        public GameObject this[SdfPath path]
        {
            get
            {
                GameObject go;
                if (m_prims.TryGetValue(path, out go))
                {
                    return go;
                }

                throw new KeyNotFoundException("The path <" + path + "> does not exist in the PrimMap");
            }
            set { m_prims[path] = value; }
        }

        public bool TryGetValue(SdfPath key, out GameObject obj)
        {
            return m_prims.TryGetValue(key, out obj);
        }

        public IEnumerator GetEnumerator()
        {
            return m_prims.GetEnumerator();
        }

        IEnumerator<KeyValuePair<SdfPath, GameObject>>
        IEnumerable<KeyValuePair<SdfPath, GameObject>>.GetEnumerator()
        {
            return m_prims.GetEnumerator();
        }

        public void AddInstance(GameObject goInst)
        {
            m_instances.Add(goInst);
        }

        public void AddMasterRoot(SdfPath path, GameObject go)
        {
            m_masterRoots[path] = go;
            this[path] = go;
        }

        public void AddInstanceRoot(SdfPath instancePath, GameObject go, SdfPath masterPath)
        {
            m_instanceRoots[instancePath] = new InstanceRoot { gameObject = go, masterPath = masterPath };
        }

        public Dictionary<SdfPath, GameObject>.KeyCollection GetMasterRootPaths()
        {
            return m_masterRoots.Keys;
        }

        public Dictionary<SdfPath, InstanceRoot>.ValueCollection GetInstanceRoots()
        {
            return m_instanceRoots.Values;
        }

        /// <summary>
        /// Destroy all GameObjects and clear the map.
        /// </summary>
        public void DestroyAll()
        {
            // When running in-editor DestroyImmediate must be used.
            foreach (var go in m_prims.Values)
            {
                GameObject.DestroyImmediate(go);
            }

            foreach (var instance in m_instanceRoots.Values)
            {
                GameObject.DestroyImmediate(instance.gameObject);
            }

            foreach (var go in m_instances)
            {
                GameObject.DestroyImmediate(go);
            }

            foreach (var go in m_masterRoots.Values)
            {
                GameObject.DestroyImmediate(go);
            }

            m_prims.Clear();

            ContainsPointInstances = false;
            HasErrors = false;
        }

        /// <summary>
        /// Clear the map without destroying game objects.
        /// </summary>
        public void Clear()
        {
            m_prims.Clear();
            m_masterRoots.Clear();
            m_instances.Clear();
            m_instanceRoots.Clear();
            Cameras = null;
            Meshes = null;
            Cubes = null;
            Spheres = null;
            Xforms = null;
            SkelRoots = null;
            Skeletons = null;
            Materials = null;

            ContainsPointInstances = false;
            HasErrors = false;
        }
    }
}

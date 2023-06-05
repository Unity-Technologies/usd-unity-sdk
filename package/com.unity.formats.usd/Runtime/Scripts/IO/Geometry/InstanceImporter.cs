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
using UnityEngine;
using USD.NET;
using PointInstancerSample = USD.NET.Unity.PointInstancerSample;

namespace Unity.Formats.USD
{
    /// <summary>
    /// A collection of methods used for translating USD instances into Unity instances (CPU or GPU).
    /// </summary>
    public static class InstanceImporter
    {
        /// <summary>
        /// Given a PrimMap, finds all instanced objects and their respective master objects and
        /// instantiates Unity clones using GameObject.Instantiate. Note that this does not result
        /// in GPU instancing.
        /// </summary>
        public static void BuildSceneInstances(PrimMap primMap, SceneImportOptions options)
        {
            if (options.enableGpuInstancing)
            {
                foreach (var masterPath in primMap.GetMasterRootPaths())
                {
                    EnableGpuInstancing(primMap[masterPath]);
                }
            }

            foreach (var instance in primMap.GetInstanceRoots())
            {
                GameObject goInstance = instance.gameObject;
                GameObject goMaster = primMap[instance.masterPath];
                foreach (Transform child in goMaster.transform)
                {
                    Transform newChild = goInstance.transform.Find(child.name);

                    // If the old instance exists, we must destroy it to avoid a duplicate
                    // because the prototypes may have changed during re-import
                    if (newChild != null)
                        GameObject.DestroyImmediate(newChild.gameObject);

                    newChild = GameObject.Instantiate(child.gameObject).transform;
                    newChild.name = child.name;
                    newChild.transform.SetParent(goInstance.transform, worldPositionStays: false);

                    primMap.AddInstance(newChild.gameObject);
                }
            }
        }

        public static void BuildPointInstances(Scene scene,
            PrimMap primMap,
            string pointInstancerPath,
            PointInstancerSample sample,
            GameObject root,
            SceneImportOptions options)
        {
            Matrix4x4[] transforms = sample.ComputeInstanceMatrices(scene, pointInstancerPath);

            foreach (var prototypeRoot in sample.prototypes.targetPaths)
            {
                GameObject go;
                if (!primMap.TryGetValue(new pxr.SdfPath(prototypeRoot), out go))
                {
                    Debug.LogWarning($"Prototype not found in PrimMap: {prototypeRoot}. Instances of this prototype cannot be instantiated.");
                    continue;
                }

                go.SetActive(false);
                if (options.enableGpuInstancing)
                {
                    EnableGpuInstancing(go);
                }
            }

            /*
             * Disabled until this bug is resolved:
             * https://github.com/PixarAnimationStudios/USD/issues/639
             *
            var inactiveIds = new System.Collections.Generic.HashSet<long>();
            if (sample.inactiveIds != null) {
              foreach (long id in sample.inactiveIds.GetExplicitItems()) {
                inactiveIds.Add(id);
              }
            }
            */

            // Validate each of the instances we're about to create and store them to be created after.
            // If the instances exist from a previous import, destroy them so they are not duplicated.
            (Matrix4x4 transform, string instanceName, GameObject goMaster)[] instancesToCreate = new (Matrix4x4, string, GameObject)[sample.protoIndices.Length];
            int instanceCount = 0;
            foreach (var index in sample.protoIndices)
            {
                /*
                if (inactiveIds.Contains(index))
                {
                    continue;
                }
                */

                if (index >= sample.prototypes.targetPaths.Length)
                {
                    Debug.LogWarning("ProtoIndex out of bounds: [" + index + "] " +
                        "for instancer: " + pointInstancerPath);
                    continue;
                }

                var targetPath = sample.prototypes.targetPaths[index];

                GameObject goMaster;
                if (!primMap.TryGetValue(new pxr.SdfPath(targetPath), out goMaster))
                {
                    continue;
                }

                if (instanceCount >= transforms.Length)
                {
                    Debug.LogWarning("No transform for instance index [" + instanceCount + "] " +
                        "for instancer: " + pointInstancerPath);
                    break;
                }

                var transform = transforms[instanceCount];

                var instanceName = $"{goMaster.name}_{instanceCount}";

                // If the old instance exists, we must destroy it to avoid a duplicate
                // because the prototypes may have changed during re-import.
                // The Transform hierarchy was created in the same order as it is now deleted,
                // so this Find() operation is best-case performance.
                var existingInstance = root.transform.Find(instanceName);
                if (existingInstance != null)
                {
                    GameObject.DestroyImmediate(existingInstance.gameObject);
                }

                instancesToCreate[instanceCount] = (transform, instanceName, goMaster);

                instanceCount++;
            }

            // Perform the re-instantiation in a secondary loop, so that these new instances are not being
            // looped over during the Transform.Find() in the previous for-loop and wasting cycles.
            for (int instanceNum = 0; instanceNum < instanceCount; instanceNum++)
            {
                var goInstance = GameObject.Instantiate(instancesToCreate[instanceNum].goMaster, root.transform);
                goInstance.SetActive(true);
                goInstance.name = instancesToCreate[instanceNum].instanceName;
                XformImporter.BuildXform(instancesToCreate[instanceNum].transform, goInstance, options);

                primMap.AddInstance(goInstance);
            }
        }

        private static void EnableGpuInstancing(GameObject go)
        {
            foreach (MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>())
            {
                if (mr.sharedMaterial != null && !mr.sharedMaterial.enableInstancing)
                {
                    mr.sharedMaterial = Material.Instantiate(mr.sharedMaterial);
                    mr.sharedMaterial.enableInstancing = true;
                }

                for (int i = 0; i < mr.sharedMaterials.Length; i++)
                {
                    var im = mr.sharedMaterials[i];
                    if (im == null || im.enableInstancing == true)
                    {
                        continue;
                    }

                    mr.sharedMaterials[i] = Material.Instantiate(im);
                    mr.sharedMaterials[i].enableInstancing = true;
                }
            }
        }
    }
}

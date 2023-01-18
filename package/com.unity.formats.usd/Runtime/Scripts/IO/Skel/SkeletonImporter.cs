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
using pxr;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    /// <summary>
    /// Import support for UsdSkelSkeleton
    /// </summary>
    public static class SkeletonImporter
    {
        public static void BuildSkeletonBone(string skelPath,
            GameObject go,
            Matrix4x4 restXform,
            VtTokenArray joints,
            SceneImportOptions importOptions)
        {
            // Perform change of basis, if needed.
            XformImporter.ImportXform(ref restXform, importOptions);

            // Decompose into TSR.
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            Vector3 scale = Vector3.one;
            if (!UnityTypeConverter.Decompose(restXform, out pos, out rot, out scale))
            {
                throw new Exception("Failed to decompose bind transforms for <" + skelPath + ">");
            }

            go.transform.localScale = scale;
            go.transform.localRotation = rot;
            go.transform.localPosition = pos;

            var cubeDebugName = "usdSkel_restPose_debug_cube";
            if (importOptions.meshOptions.debugShowSkeletonRestPose)
            {
                var cube = go.transform.Find(cubeDebugName);
                if (!cube)
                {
                    cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    cube.name = cubeDebugName;
                    cube.SetParent(go.transform, worldPositionStays: false);
                    cube.localScale = Vector3.one * 2;
                }
            }
            else
            {
                var existing = go.transform.Find(cubeDebugName);
                if (existing)
                {
                    GameObject.DestroyImmediate(existing.gameObject);
                }
            }
        }

        public static void BuildDebugBindTransforms(SkeletonSample skelSample,
            GameObject goSkeleton,
            SceneImportOptions options)
        {
            var debugPrefix = "usdSkel_bindPose_debug_cube";
            if (options.meshOptions.debugShowSkeletonBindPose)
            {
                int i = 0;
                foreach (var bindXf in skelSample.bindTransforms)
                {
                    // Undo the bindXf inversion for visualization.
                    var mat = bindXf.inverse;
                    var cubeName = debugPrefix + i++;
                    var cube = goSkeleton.transform.Find(cubeName);
                    if (!cube)
                    {
                        cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                        cube.SetParent(goSkeleton.transform, worldPositionStays: false);
                        cube.name = cubeName;
                    }

                    Vector3 t, s;
                    Quaternion r;
                    UnityTypeConverter.Decompose(mat, out t, out r, out s);
                    cube.localPosition = t;
                    cube.localScale = s;
                    cube.localRotation = r;
                }
            }
            else
            {
                var zero = goSkeleton.transform.Find(debugPrefix + 0);
                if (zero)
                {
                    var toDelete = new List<GameObject>();
                    foreach (Transform child in goSkeleton.transform)
                    {
                        if (child.name.StartsWith(debugPrefix))
                        {
                            toDelete.Add(child.gameObject);
                        }
                    }

                    foreach (var child in toDelete)
                    {
                        GameObject.DestroyImmediate(child);
                    }
                }
            }
        }

        /// <summary>
        /// Unity expects bind transforms to be the bone's inverse transformation matrix.
        /// USD doesn't do that, so this function does it for us, prepping the data to be used in Unity.
        /// <summary>
        public static void BuildBindTransforms(string path,
            SkeletonSample skelSample,
            SceneImportOptions options)
        {
            if (skelSample.bindTransforms == null)
            {
                return;
            }

            for (int i = 0; i < skelSample.bindTransforms.Length; i++)
            {
                var xf = skelSample.bindTransforms[i];
                XformImporter.ImportXform(ref xf, options);
                skelSample.bindTransforms[i] = xf.inverse;
            }
        }

        static bool JointsMatch(string[] lhs, string[] rhs)
        {
            if (lhs == null && rhs == null)
            {
                return true;
            }

            if (lhs == null || rhs == null)
            {
                return false;
            }

            if (lhs == rhs)
            {
                return true;
            }

            // The prim can use a subset of the joints
            if (lhs.Length != rhs.Length)
            {
                return false;
            }

            // The prim can have a custom joint order if skel:joints is set on it
            for (int i = 0; i < lhs.Length; i++)
            {
                if (lhs[i] != rhs[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static void BuildSkinnedMesh(string meshPath,
            string skelPath,
            SkeletonSample skeleton,
            UsdSkelSkinningQuery skinningQuery,
            GameObject go,
            PrimMap primMap,
            SceneImportOptions options)
        {
            // The mesh renderer must already exist, since hte mesh also must already exist.
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (!smr)
            {
                throw new Exception(
                    "Error importing " + meshPath + " SkinnnedMeshRenderer not present on GameObject");
            }

            // Get and validate the local list of joints.
            VtTokenArray jointsAttr = new VtTokenArray();
            skinningQuery.GetJointOrder(jointsAttr);

            // If jointsAttr wasn't define, GetJointOrder return an empty array and FromVtArray as well.
            string[] joints = IntrinsicTypeConverter.FromVtArray(jointsAttr);

            // WARNING: Do not mutate skeleton values.
            string[] skelJoints = skeleton.joints;

            if (joints == null || joints.Length == 0)
            {
                if (skelJoints == null || skelJoints.Length == 0)
                {
                    throw new Exception("Joints array empty: " + meshPath);
                }
                else
                {
                    joints = skelJoints;
                }
            }

            // TODO: bind transform attribute can be animated. It's not handled yet.
            Matrix4x4 geomXf = UnityTypeConverter.FromMatrix(skinningQuery.GetGeomBindTransform());

            // A mesh prim can have a custom joint definition order or use a subset of the joints.
            // If it's the case reconstruct the bindpose accordingly.
            // TODO: a better way of doing this would be to get a jointMapper with skinningQuery.GetJointMapper
            //       and use it to map the bindTransforms to the joints list. But the UsdSkelAnimMapper is not in the bindings yet.
            var bindPoses = skeleton.bindTransforms;
            if (!JointsMatch(skeleton.joints, joints))
            {
                var boneToPose = new Dictionary<string, Matrix4x4>();
                bindPoses = new Matrix4x4[joints.Length];
                for (int i = 0; i < skelJoints.Length; i++)
                {
                    boneToPose[skelJoints[i]] = skeleton.bindTransforms[i];
                }

                for (int i = 0; i < joints.Length; i++)
                {
                    bindPoses[i] = boneToPose[joints[i]];
                }
            }

            // When geomXf is identity, we can take a shortcut and just use the exact skeleton bindPoses.
            if (!ImporterBase.ApproximatelyEqual(geomXf, Matrix4x4.identity))
            {
                // Note that the bind poses were transformed when the skeleton was imported, but the
                // geomBindTransform is per-mesh, so it must be transformed here so it is in the same space
                // as the bind pose.
                XformImporter.ImportXform(ref geomXf, options);

                // Make a copy only if we haven't already copied the bind poses earlier.
                if (bindPoses == skeleton.bindTransforms)
                {
                    var newBindPoses = new Matrix4x4[skeleton.bindTransforms.Length];
                    Array.Copy(bindPoses, newBindPoses, bindPoses.Length);
                    bindPoses = newBindPoses;
                }

                // Concatenate the geometry bind transform with the skeleton bind poses.
                for (int i = 0; i < bindPoses.Length; i++)
                {
                    // The geometry transform should be applied to the points before any other transform,
                    // hence the right hand multiply here.
                    bindPoses[i] = bindPoses[i] * geomXf;
                }
            }
            smr.sharedMesh.bindposes = bindPoses;

            var bones = new Transform[joints.Length];
            var sdfSkelPath = new SdfPath(skelPath);
            for (int i = 0; i < joints.Length; i++)
            {
                var jointPath = new SdfPath(joints[i]);

                if (joints[i] == "/")
                {
                    jointPath = sdfSkelPath;
                }
                else if (jointPath.IsAbsolutePath())
                {
                    Debug.LogException(new Exception("Unexpected absolute joint path: " + jointPath));
                    jointPath = new SdfPath(joints[i].TrimStart('/'));
                    jointPath = sdfSkelPath.AppendPath(jointPath);
                }
                else
                {
                    jointPath = sdfSkelPath.AppendPath(jointPath);
                }

                var jointGo = primMap[jointPath];
                if (!jointGo)
                {
                    Debug.LogError("Error importing " + meshPath + " "
                        + "Joint not found: " + joints[i]);
                    continue;
                }

                bones[i] = jointGo.transform;
            }
            smr.bones = bones;
        }
    }
}

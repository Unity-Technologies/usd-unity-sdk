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

#if !UNITY_2017 && !UNITY_2018
using Unity.Collections;
#endif

namespace Unity.Formats.USD {

  /// <summary>
  /// Import support for UsdSkelSkeleton
  /// </summary>
  public static class SkeletonImporter {

    public static void BuildSkeletonBone(string skelPath,
                                         GameObject go,
                                         Matrix4x4 restXform,
                                         VtTokenArray joints,
                                         SceneImportOptions importOptions) {
      // Perform change of basis, if needed.
      XformImporter.ImportXform(ref restXform, importOptions);

      // Decompose into TSR.
      Vector3 pos = Vector3.zero;
      Quaternion rot = Quaternion.identity;
      Vector3 scale = Vector3.one;
      if (!UnityTypeConverter.Decompose(restXform, out pos, out rot, out scale)) {
        throw new Exception("Failed to decompose bind trnsforms for <" + skelPath + ">");
      }
      go.transform.localScale = scale;
      go.transform.localRotation = rot;
      go.transform.localPosition = pos;

      var cubeDebugName = "usdSkel_restPose_debug_cube";
      if (importOptions.meshOptions.debugShowSkeletonRestPose) {
        var cube = go.transform.Find(cubeDebugName);
        if (!cube) {
          cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
          cube.name = cubeDebugName;
          cube.SetParent(go.transform, worldPositionStays: false);
          cube.localScale = Vector3.one * 2;
        }
      } else {
        var existing = go.transform.Find(cubeDebugName);
        if (existing) {
          GameObject.DestroyImmediate(existing.gameObject);
        }
      }
    }

    public static void BuildDebugBindTransforms(SkeletonSample skelSample,
                                                GameObject goSkeleton,
                                                SceneImportOptions options) {
      var debugPrefix = "usdSkel_bindPose_debug_cube";
      if (options.meshOptions.debugShowSkeletonBindPose) {

        int i = 0;
        foreach (var bindXf in skelSample.bindTransforms) {
          // Undo the bindXf inversion for visualization.
          var mat = bindXf.inverse;
          var cubeName = debugPrefix + i++;
          var cube = goSkeleton.transform.Find(cubeName);
          if (!cube) {
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
      } else {
        var zero = goSkeleton.transform.Find(debugPrefix + 0);
        if (zero) {
          var toDelete = new List<GameObject>();
          foreach (Transform child in goSkeleton.transform) {
            if (child.name.StartsWith(debugPrefix)) {
              toDelete.Add(child.gameObject);
            }
          }
          foreach (var child in toDelete) {
            GameObject.DestroyImmediate(child);
          }
        }
      }
    }

    public static void BuildBindTransforms(string path,
                                           SkeletonSample skelSample,
                                           SceneImportOptions options) {
      if (skelSample.bindTransforms == null) { return; }
      for (int i = 0; i < skelSample.bindTransforms.Length; i++) {
        var xf = skelSample.bindTransforms[i];
        XformImporter.ImportXform(ref xf, options);
        skelSample.bindTransforms[i] = xf.inverse;
      }
    }

    static bool JointsMatch(string[] lhs, string[] rhs) {
      if (lhs == null && rhs == null) { return true; }
      if (lhs == null || rhs == null) { return false; }
      if (lhs == rhs) { return true; }

      for (int i = 0; i < lhs.Length; i++) {
        if (lhs[i] != rhs[i]) {
          return false;
        }
      }

      return true;
    }

    public static void BuildSkinnedMesh(string meshPath,
                                        string skelPath,
                                        SkeletonSample skeleton,
                                        SkelBindingSample meshBinding,
                                        GameObject go,
                                        PrimMap primMap,
                                        SceneImportOptions options) {
      string[] joints = meshBinding.joints;

      // WARNING: Do not mutate skeleton values.
      string[] skelJoints = skeleton.joints;
      bool isConstant = meshBinding.jointWeights.interpolation == PrimvarInterpolation.Constant;

      if (joints == null || joints.Length == 0) {
        if (skelJoints == null || skelJoints.Length == 0) {
          throw new Exception("Joints array empty: " + meshPath);
        } else {
          joints = skelJoints;
        }
      }

      // The mesh renderer must already exist, since hte mesh also must already exist.
      var smr = go.GetComponent<SkinnedMeshRenderer>();
      if (!smr) {
        throw new Exception("Error importing " + meshPath
            + " SkinnnedMeshRenderer not present on GameObject");
      }

      var mesh = smr.sharedMesh;
      var geomXf = meshBinding.geomBindTransform.value;

      // If the joints list is a different length than the bind transforms, then this is likely
      // a mesh using a subset of the total bones in the skeleton and the bindTransforms must be
      // reconstructed.
      var bindPoses = skeleton.bindTransforms;
      if (!JointsMatch(skeleton.joints, joints)) {
        var boneToPose = new Dictionary<string, Matrix4x4>();
        bindPoses = new Matrix4x4[joints.Length];
        for (int i = 0; i < skelJoints.Length; i++) {
          boneToPose[skelJoints[i]] = skeleton.bindTransforms[i];
        }
        for (int i = 0; i < joints.Length; i++) {
          bindPoses[i] = boneToPose[joints[i]];
        }
      }

      // When geomXf is identity, we can take a shortcut and just use the exact skeleton bindPoses.
      if (!ImporterBase.ApproximatelyEqual(geomXf, Matrix4x4.identity)) {
        // Note that the bind poses were transformed when the skeleton was imported, but the
        // geomBindTransform is per-mesh, so it must be transformed here so it is in the same space
        // as the bind pose.
        XformImporter.ImportXform(ref geomXf, options);

        // Make a copy only if we haven't already copied the bind poses earlier.
        if (bindPoses == skeleton.bindTransforms) {
          var newBindPoses = new Matrix4x4[skeleton.bindTransforms.Length];
          Array.Copy(bindPoses, newBindPoses, bindPoses.Length);
          bindPoses = newBindPoses;
        }

        // Concatenate the geometry bind transform with the skeleton bind poses.
        for (int i = 0; i < bindPoses.Length; i++) {
          // The geometry transform should be applied to the points before any other transform,
          // hence the right hand multiply here.
          bindPoses[i] = bindPoses[i] * geomXf;
        }
      }
      mesh.bindposes = bindPoses;

      var bones = new Transform[joints.Length];
      var sdfSkelPath = new SdfPath(skelPath);
      for (int i = 0; i < joints.Length; i++) {
        var jointPath = new SdfPath(joints[i]);

        if (joints[i] == "/") {
          jointPath = sdfSkelPath;
        } else if (jointPath.IsAbsolutePath()) {
          Debug.LogException(new Exception("Unexpected absolute joint path: " + jointPath));
          jointPath = new SdfPath(joints[i].TrimStart('/'));
          jointPath = sdfSkelPath.AppendPath(jointPath);
        } else {
          jointPath = sdfSkelPath.AppendPath(jointPath);
        }
        var jointGo = primMap[jointPath];
        if (!jointGo) {
          Debug.LogError("Error importing " + meshPath + " "
                       + "Joint not found: " + joints[i]);
          continue;
        }
        bones[i] = jointGo.transform;
      }
      smr.bones = bones;

      int[] indices = meshBinding.jointIndices.value;
      float[] weights = meshBinding.jointWeights.value;

      // Unity 2019 supports many-bone rigs, older versions of Unity only support four bones.
#if UNITY_2019
      var bonesPerVertex = new NativeArray<byte>(mesh.vertexCount, Allocator.Persistent);
      var boneWeights1 = new NativeArray<BoneWeight1>(mesh.vertexCount * meshBinding.jointWeights.elementSize, Allocator.Persistent);
      for (int i = 0; i < mesh.vertexCount; i++) {
        int unityIndex = i * meshBinding.jointWeights.elementSize;
        int usdIndex = isConstant
                     ? 0
                     : unityIndex;

        bonesPerVertex[i] = (byte)meshBinding.jointWeights.elementSize;

        for (int wi = 0; wi < meshBinding.jointWeights.elementSize; wi++) {
          var bw = boneWeights1[unityIndex + wi];
          bw.boneIndex = indices[usdIndex + wi];
          bw.weight = weights[usdIndex + wi];
          boneWeights1[unityIndex + wi] = bw;
        }
      }
      mesh.SetBoneWeights(bonesPerVertex, boneWeights1);
      bonesPerVertex.Dispose();
      boneWeights1.Dispose();
#else
      var boneWeights = new BoneWeight[mesh.vertexCount];
      for (int i = 0; i < boneWeights.Length; i++) {
        // When interpolation is constant, the base usdIndex should always be zero.
        // When non-constant, the offset is the index times the number of weights per vertex.
        int usdIndex = isConstant
                     ? 0
                     : i * meshBinding.jointWeights.elementSize;

        var boneWeight = boneWeights[i];

        if (usdIndex >= indices.Length) {
          Debug.Log("UsdIndex out of bounds: " + usdIndex
                  + " indices.Length: " + indices.Length
                  + " boneWeights.Length: " + boneWeights.Length
                  + " mesh: " + meshPath);
        }

        boneWeight.boneIndex0 = indices[usdIndex];
        boneWeight.weight0 = weights[usdIndex];

        if (meshBinding.jointIndices.elementSize >= 2) {
          boneWeight.boneIndex1 = indices[usdIndex + 1];
          boneWeight.weight1 = weights[usdIndex + 1];
        }
        if (meshBinding.jointIndices.elementSize >= 3) {
          boneWeight.boneIndex2 = indices[usdIndex + 2];
          boneWeight.weight2 = weights[usdIndex + 2];
        }
        if (meshBinding.jointIndices.elementSize >= 4) {
          boneWeight.boneIndex3 = indices[usdIndex + 3];
          boneWeight.weight3 = weights[usdIndex + 3];
        }

        // If weights are less than 1, Unity will not automatically renormalize.
        // If weights are greater than 1, Unity will renormalize.
        // Only normalize when less than one to make it easier to diff bone weights which were
        // round-tripped and were being normalized by Unity.
        float sum = boneWeight.weight0 + boneWeight.weight1 + boneWeight.weight2 + boneWeight.weight3;
        if (sum < 1) {
          boneWeight.weight0 /= sum;
          boneWeight.weight1 /= sum;
          boneWeight.weight2 /= sum;
          boneWeight.weight3 /= sum;
        }

        boneWeights[i] = boneWeight;
      }

      mesh.boneWeights = boneWeights;
#endif
    }
  } // class
} // namespace

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

namespace USD.NET.Unity {

  /// <summary>
  /// Import support for UsdSkelSkeleton
  /// </summary>
  public static class SkeletonImporter {

    public static void BuildSkinnedMesh(string meshPath,
                                        string skelPath,
                                        SkeletonSample skeleton,
                                        SkelBindingSample meshBinding,
                                        GameObject go,
                                        PrimMap primMap,
                                        SceneImportOptions options) {
      int[] indices   = meshBinding.jointIndices.value;
      float[] weights = meshBinding.jointWeights.value;
      string[] joints = meshBinding.joints;
      var bindPoses = skeleton.bindTransforms;
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
        Debug.LogWarning(meshPath);
        // TODO: copying these after the fact is not great, they should be constructed as skinned
        // mesh renderers from creation.
        var mr = go.GetComponent<MeshRenderer>();
        var mf = go.GetComponent<MeshFilter>();
        if (!mr) {
          throw new Exception("Error importing " + meshPath
              + " MeshRenderer not present on GameObject");
        }
        if (!mf) {
          throw new Exception("Error importing " + meshPath
              + " MeshFilter not present on GameObject");
        }
        smr = go.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = mf.sharedMesh;
        smr.sharedMaterial = mr.sharedMaterial;
        smr.sharedMaterials = mr.sharedMaterials;

        Component.DestroyImmediate(mf);
        Component.DestroyImmediate(mr);
      }

      var bones = new Transform[joints.Length];
      var mesh = smr.sharedMesh;
      var boneWeights = new BoneWeight[mesh.vertexCount];

      for (int i = 0; i < bindPoses.Length; i++) {
        bindPoses[i] = bindPoses[i].inverse;
        if (options.changeHandedness == BasisTransformation.SlowAndSafe) {
          bindPoses[i] = UnityTypeConverter.ChangeBasis(bindPoses[i]);
        }
      }

      var sdfSkelPath = new SdfPath(skelPath);
      for (int i = 0; i < joints.Length; i++) {
        var jointGo = primMap[sdfSkelPath.AppendPath(new SdfPath(joints[i]))];
        if (!jointGo) {
          Debug.LogError("Error importing " + meshPath + " "
                       + "Joint not found: " + joints[i]);
          continue;
        }
        bones[i] = jointGo.transform;
      }
      smr.bones = bones;

      for (int i = 0; i < boneWeights.Length; i++) {
        // When interpolation is constant, the base usdIndex should always be zero.
          // When non-constant, the offset is the index times the number of weights per vertex.
        int usdIndex = isConstant
                     ? 0
                     : usdIndex = i * meshBinding.jointWeights.elementSize;

        var boneWeight = boneWeights[i];

        if (usdIndex >= indices.Length) {
          Debug.Log("UsdIndex out of bounds: " + usdIndex
                  + " indices.Length: " + indices.Length
                  + " boneWeights.Length: " + boneWeights.Length
                  + " mesh: " + meshPath);
        }
        boneWeight.boneIndex0 = indices[usdIndex];
        boneWeight.weight0 = weights[usdIndex];

        if (meshBinding.jointIndices.elementSize == 2) {
          boneWeight.boneIndex0 = indices[usdIndex + 1];
          boneWeight.weight0 = weights[usdIndex + 1];
        }
        if (meshBinding.jointIndices.elementSize == 3) {
          boneWeight.boneIndex0 = indices[usdIndex + 2];
          boneWeight.weight0 = weights[usdIndex + 2];
        }
        if (meshBinding.jointIndices.elementSize >=4) {
          boneWeight.boneIndex0 = indices[usdIndex + 3];
          boneWeight.weight0 = weights[usdIndex + 3];
        }
      }
    }

  } // class
} // namespace

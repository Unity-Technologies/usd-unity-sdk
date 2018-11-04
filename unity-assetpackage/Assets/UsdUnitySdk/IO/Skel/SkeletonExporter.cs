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

using UnityEngine;

namespace USD.NET.Unity {
  public static class SkeletonExporter {

    public static void ExportSkeleton(ObjectContext objContext, ExportContext exportContext) {
      var scene = exportContext.scene;
      var sample = (SkeletonSample)objContext.sample;
      var bones = exportContext.skelMap[objContext.gameObject.transform];
      sample.joints = new string[bones.Length];
      sample.bindTransforms = new Matrix4x4[bones.Length];
      sample.restTransforms = new Matrix4x4[bones.Length];

      string rootPath = UnityTypeConverter.GetPath(objContext.gameObject.transform);

      int i = 0;
      foreach (Transform bone in bones) {
        var bonePath = UnityTypeConverter.GetPath(bone);
        sample.joints[i] = bonePath;
        sample.bindTransforms[i] = exportContext.bones[bone].inverse;
        sample.restTransforms[i] = XformExporter.GetLocalTransformMatrix(
            bone, false, false, exportContext.basisTransform);

        if (exportContext.basisTransform == BasisTransformation.SlowAndSafe) {
          sample.bindTransforms[i] = UnityTypeConverter.ChangeBasis(sample.bindTransforms[i]);
          // The restTransforms will get a change of basis from GetLocalTransformMatrix().
        }

        i++;
      }

      scene.Write(objContext.path, sample);

      // Stop Skeleton from rendering bones in usdview by default.
      var im = new pxr.UsdGeomImageable(scene.GetPrimAtPath(objContext.path));
      im.CreatePurposeAttr().Set(pxr.UsdGeomTokens.guide);
    }

    public static void ExportSkelRoot(ObjectContext objContext, ExportContext exportContext) {
      var sample = (SkelRootSample)objContext.sample;
      var bindings = ((string[])objContext.additionalData);

      sample.extent = new Bounds(objContext.gameObject.transform.position, Vector3.zero);

      if (bindings != null) {
        sample.skeleton = bindings[0];
        if (bindings.Length > 1) {
          sample.animationSource = bindings[1];
        }
      }

      // Compute bounds for the root, required by USD.
      foreach (var r in objContext.gameObject.GetComponentsInChildren<Renderer>()) {
        sample.extent.Encapsulate(r.bounds);
      }

      exportContext.scene.Write(objContext.path, sample);
    }

    public static void ExportSkelAnimation(ObjectContext objContext, ExportContext exportContext) {
      var scene = exportContext.scene;
      var sample = (SkelAnimationSample)objContext.sample;
      var go = objContext.gameObject;
      var bones = exportContext.skelMap[go.transform];
      var skelRoot = go.transform;
      sample.joints = new string[bones.Length];

      var worldXf = new Matrix4x4[bones.Length];
      var worldXfInv = new Matrix4x4[bones.Length];

      string rootPath = UnityTypeConverter.GetPath(go.transform);

      var basisChange = Matrix4x4.identity;
      basisChange[2, 2] = -1;

      for (int i = 0; i < bones.Length; i++) {
        var bone = bones[i];
        var bonePath = UnityTypeConverter.GetPath(bone);
        sample.joints[i] = bonePath;
        worldXf[i] = bone.localToWorldMatrix;
        if (exportContext.basisTransform == BasisTransformation.SlowAndSafe) {
          worldXf[i] = UnityTypeConverter.ChangeBasis(worldXf[i]);
        } else if (exportContext.basisTransform == BasisTransformation.FastWithNegativeScale) {
          // Normally, this would only be applied at the root, but since each matrix is in world
          // space, it is also required here to put the rig in the same space as the exported world.
          //worldXf[i] = worldXf[i] * basisChange;
        }
        worldXfInv[i] = worldXf[i].inverse;
      }

      var rootXf = skelRoot.localToWorldMatrix;
      if (exportContext.basisTransform == BasisTransformation.FastWithNegativeScale) {
        //rootXf = rootXf * basisChange;
      }
      var skelWorldTransform = UnityTypeConverter.ToGfMatrix(rootXf);
      pxr.VtMatrix4dArray vtJointsLS = new pxr.VtMatrix4dArray((uint)bones.Length);
      pxr.VtMatrix4dArray vtJointsWS = UnityTypeConverter.ToVtArray(worldXf);
      pxr.VtMatrix4dArray vtJointsWSInv = UnityTypeConverter.ToVtArray(worldXfInv);

      var translations = new pxr.VtVec3fArray();
      var rotations = new pxr.VtQuatfArray();
      sample.scales = new pxr.VtVec3hArray();

      var topo = new pxr.UsdSkelTopology(UnityTypeConverter.ToVtArray(sample.joints));
      var localSpaceXforms = pxr.UsdCs.UsdSkelComputeJointLocalTransforms(topo,
          vtJointsWS,
          vtJointsWSInv,
          vtJointsLS,
          skelWorldTransform);

      pxr.UsdCs.UsdSkelDecomposeTransforms(
          vtJointsLS,
          translations,
          rotations,
          sample.scales);
      sample.translations = UnityTypeConverter.FromVtArray(translations);
      sample.rotations = UnityTypeConverter.FromVtArray(rotations);

      scene.Write(objContext.path, sample);
    }

  }
}

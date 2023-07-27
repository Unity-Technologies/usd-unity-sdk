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
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public static class SkeletonExporter
    {
        public static void ExportSkeleton(ObjectContext objContext, ExportContext exportContext)
        {
            var scene = exportContext.scene;
            var sample = (SkeletonSample)objContext.sample;
            var boneNames = exportContext.skelSortedMap[objContext.gameObject.transform];
            sample.joints = new string[boneNames.Count];
            sample.bindTransforms = new Matrix4x4[boneNames.Count];
            sample.restTransforms = new Matrix4x4[boneNames.Count];

            string rootPath = UnityTypeConverter.GetPath(objContext.gameObject.transform);

            sample.transform = XformExporter.GetLocalTransformMatrix(
                objContext.gameObject.transform,
                scene.UpAxis == Scene.UpAxes.Z,
                new pxr.SdfPath(rootPath).IsRootPrimPath(),
                exportContext.basisTransform);

            int i = 0;
            foreach (string bonePath in boneNames)
            {
                if (string.IsNullOrEmpty(bonePath))
                {
                    sample.joints[i] = "";
                    i++;
                    continue;
                }

                var bone = exportContext.pathToBone[bonePath];
                if (bonePath == rootPath)
                {
                    sample.joints[i] = "/";
                }
                else
                {
                    sample.joints[i] = bonePath.Replace(rootPath + "/", "");
                }

                // TODO: When the bone bind transform contains the geomBindTransform from USD import, it
                // will be mixed into each bone. This transform should be saved in some way and removed
                // when exported as a skeleton.
                sample.bindTransforms[i] = exportContext.bindPoses[bone].inverse;
                sample.restTransforms[i] = XformExporter.GetLocalTransformMatrix(
                    bone, false, false, exportContext.basisTransform);

                if (exportContext.basisTransform == BasisTransformation.SlowAndSafe)
                {
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

        public static void ExportSkelRoot(ObjectContext objContext, ExportContext exportContext)
        {
            var sample = (SkelRootSample)objContext.sample;

            // Compute bounds for the root, required by USD.
            bool first = true;

            // Ensure the bounds are computed in root-local space.
            // This is required because USD expects the extent to be a local bound for the SkelRoot.
            var oldParent = objContext.gameObject.transform.parent;
            objContext.gameObject.transform.SetParent(null, worldPositionStays: false);

            try
            {
                foreach (var r in objContext.gameObject.GetComponentsInChildren<Renderer>())
                {
                    if (first)
                    {
                        // Ensure the bounds object starts growing from the first valid child bounds.
                        first = false;
                        sample.extent = r.bounds;
                    }
                    else
                    {
                        sample.extent.Encapsulate(r.bounds);
                    }
                }
            }
            finally
            {
                // Restore the root parent.
                objContext.gameObject.transform.SetParent(oldParent, worldPositionStays: false);
            }

            // Convert handedness if needed.
            if (exportContext.basisTransform == BasisTransformation.SlowAndSafe)
            {
                sample.extent.center = UnityTypeConverter.ChangeBasis(sample.extent.center);
            }

            pxr.UsdPrim usdPrim = exportContext.scene.GetPrimAtPath(objContext.path);
            pxr.UsdSkelBindingAPI.Apply(usdPrim);

            // Convert the transform
            var path = new pxr.SdfPath(objContext.path);

            // If exporting for Z-Up, rotate the world.
            bool correctZUp = exportContext.scene.UpAxis == Scene.UpAxes.Z;

            sample.transform = XformExporter.GetLocalTransformMatrix(
                objContext.gameObject.transform,
                correctZUp,
                path.IsRootPrimPath(),
                exportContext.basisTransform);

            exportContext.scene.Write(objContext.path, sample);
        }

        public static void ExportSkelAnimation(ObjectContext objContext, ExportContext exportContext)
        {
            var scene = exportContext.scene;
            var sample = (SkelAnimationSample)objContext.sample;
            var go = objContext.gameObject;
            var boneNames = exportContext.skelSortedMap[go.transform];
            var skelRoot = go.transform;
            sample.joints = new string[boneNames.Count];

            var worldXf = new Matrix4x4[boneNames.Count];
            var worldXfInv = new Matrix4x4[boneNames.Count];

            string rootPath = UnityTypeConverter.GetPath(go.transform);

            var basisChange = Matrix4x4.identity;
            basisChange[2, 2] = -1;

            for (int i = 0; i < boneNames.Count; i++)
            {
                var bonePath = boneNames[i];
                if (!exportContext.pathToBone.ContainsKey(bonePath))
                {
                    sample.joints[i] = "";
                    continue;
                }

                var bone = exportContext.pathToBone[bonePath];
                sample.joints[i] = bonePath.Replace(rootPath + "/", "");

                worldXf[i] = bone.localToWorldMatrix;
                if (exportContext.basisTransform == BasisTransformation.SlowAndSafe)
                {
                    worldXf[i] = UnityTypeConverter.ChangeBasis(worldXf[i]);
                }

                worldXfInv[i] = worldXf[i].inverse;
            }

            var rootXf = skelRoot.localToWorldMatrix.inverse;
            if (exportContext.basisTransform == BasisTransformation.SlowAndSafe)
            {
                rootXf = UnityTypeConverter.ChangeBasis(rootXf);
            }

            var skelWorldTransform = UnityTypeConverter.ToGfMatrix(rootXf);
            pxr.VtMatrix4dArray vtJointsLS = new pxr.VtMatrix4dArray((uint)boneNames.Count);
            pxr.VtMatrix4dArray vtJointsWS = UnityTypeConverter.ToVtArray(worldXf);
            pxr.VtMatrix4dArray vtJointsWSInv = UnityTypeConverter.ToVtArray(worldXfInv);

            var translations = new pxr.VtVec3fArray();
            var rotations = new pxr.VtQuatfArray();
            sample.scales = new pxr.VtVec3hArray();

            var topo = new pxr.UsdSkelTopology(UnityTypeConverter.ToVtArray(sample.joints));
            pxr.UsdCs.UsdSkelComputeJointLocalTransforms(topo,
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

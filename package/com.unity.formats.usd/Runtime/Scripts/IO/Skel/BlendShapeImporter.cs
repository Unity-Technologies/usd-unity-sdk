using System;
using System.Collections.Generic;
using System.Net;
using pxr;
using UnityEngine;
using UnityEngine.Profiling;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public static class BlendShapeImporter
    {
        public static void BuildBlendShapeTargets(
            string meshPath,
            GameObject go,
            Scene scene,
            UsdSkelSkinningQuery skinningQuery,
            SceneImportOptions options
        )
        {
            if (!skinningQuery.HasBlendShapes())
            {
                return;
            }

            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (!smr)
            {
                throw new Exception(
                    $"Error importing {meshPath} SkinnedMeshRenderer not present on GameObject"
                );
            }

            var mesh = smr.sharedMesh;
            mesh.ClearBlendShapes();

            Dictionary<string, BlendShapeSample> blendShapeSamples = new Dictionary<string, BlendShapeSample>();
            var skelBindingApi = new UsdSkelBindingAPI(skinningQuery.GetPrim());
            var blendShapeQuery = new UsdSkelBlendShapeQuery(skelBindingApi);

            for (uint i = 0; i < blendShapeQuery.GetNumBlendShapes(); ++i)
            {
                BlendShapeSample blendShapeSample = new BlendShapeSample();
                var blendShape = blendShapeQuery.GetBlendShape(i).GetPrim();
                scene.Read(blendShape.GetPath(), blendShapeSample);
                blendShapeSamples[blendShape.GetName()] = blendShapeSample;
            }

            bool changeHandedness = options.changeHandedness == BasisTransformation.SlowAndSafe ||
                                    options.changeHandedness == BasisTransformation.SlowAndSafeAsFBX;

            VtTokenArray blendShapeOrder = new VtTokenArray();
            skinningQuery.GetBlendShapeOrder(blendShapeOrder);

            Profiler.BeginSample("Add Blend Shape Offsets");
            for (int i = 0; i < blendShapeQuery.GetNumBlendShapes(); ++i)
            {
                var blendShapeName = blendShapeOrder[i].ToString();
                var blendShapeIndices = blendShapeSamples[blendShapeName].pointIndices;
                var blendShapeOffsets = blendShapeSamples[blendShapeName].offsets;
                var blendShapeNormalOffsets = blendShapeSamples[blendShapeName].normalOffsets;

                if (blendShapeIndices != null && blendShapeIndices?.Length != mesh.vertexCount)
                {
                    Vector3[] offsets = new Vector3[mesh.vertexCount];
                    Vector3[] normalOffsets = new Vector3[mesh.vertexCount];
                    for (int j = 0; j < blendShapeIndices?.Length; ++j)
                    {
                        int blendIndex = blendShapeIndices[j];
                        offsets[blendIndex] = blendShapeOffsets[j];
                        normalOffsets[blendIndex] = blendShapeNormalOffsets[j];
                        if (changeHandedness)
                        {
                            offsets[blendIndex] = UnityTypeConverter.ChangeBasis(offsets[blendIndex]);
                            normalOffsets[blendIndex] = UnityTypeConverter.ChangeBasis(normalOffsets[blendIndex]);
                        }
                    }
                    mesh.AddBlendShapeFrame(blendShapeName, 100f,
                        offsets, normalOffsets, null);
                }
                else
                {
                    if (changeHandedness)
                    {
                        Vector3[] offsets = new Vector3[mesh.vertexCount];
                        Vector3[] normalOffsets = new Vector3[mesh.vertexCount];
                        for (int j = 0; j < mesh.vertexCount; ++j)
                        {
                            offsets[j] = UnityTypeConverter.ChangeBasis(blendShapeOffsets[j]);
                            normalOffsets[j] = UnityTypeConverter.ChangeBasis(blendShapeNormalOffsets[j]);
                        }
                        mesh.AddBlendShapeFrame(blendShapeName, 100f,
                            offsets, normalOffsets, null);
                    }
                    else
                    {
                        mesh.AddBlendShapeFrame(blendShapeName, 100f,
                            blendShapeOffsets, blendShapeNormalOffsets, null);
                    }
                }
            }
            Profiler.EndSample();
        }

        public static void BuildBlendShapeWeights(
            GameObject go,
            Scene scene,
            UsdSkelSkinningQuery skinningQuery
        )
        {
            Profiler.BeginSample("Get Skeleton");
            var skelBindingApi = new UsdSkelBindingAPI(skinningQuery.GetPrim());
            var skeletonTargets = skelBindingApi.GetSkeletonRel().GetForwardedTargets();
            Profiler.EndSample();
            if (skeletonTargets.Count == 0)
            {
                return;
            }
            Profiler.BeginSample("Get Animation Target");
            skelBindingApi = new UsdSkelBindingAPI(scene.GetPrimAtPath(skeletonTargets[0]));
            var animTargets = skelBindingApi.GetAnimationSourceRel().GetForwardedTargets();
            Profiler.EndSample();
            if (animTargets.Count == 0)
            {
                return;
            }

            Profiler.BeginSample("Get SkelAnim");
            var skelAnimTarget = scene.GetPrimAtPath(animTargets[0]);
            var skelAnimation = new UsdSkelAnimation(skelAnimTarget);
            var skelAnimSample = new SkelAnimationSample();
            Profiler.EndSample();

            Profiler.BeginSample("Read Animation Sample");
            scene.Read(skelAnimation.GetPath(), skelAnimSample);
            Profiler.EndSample();

            Profiler.BeginSample("Get Skinned Mesh");
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (!smr)
            {
                throw new Exception(
                    $"Error settingWeights on {skinningQuery.GetPrim().GetPath()} SkinnedMeshRenderer not present on GameObject"
                );
            }
            Profiler.EndSample();

            Profiler.BeginSample("Set Blend Shape Weights");
            for (int i = 0; i < skelAnimSample.blendShapeWeights.Length; ++i)
            {
                smr.SetBlendShapeWeight(i, skelAnimSample.blendShapeWeights[i] * 100f);
            }
            Profiler.EndSample();
        }
    }
}

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
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public class UnitySkeleton
    {
        public Transform rootBone;
        public Transform[] bones;
    }

    [System.Serializable]
    public class SkelBindingSample : SampleBase
    {
        // Blend Shapes.
        [UsdNamespace("skel")]
        [UsdVariability(Variability.Uniform)]
        public string[] blendShapes;

        [UsdNamespace("skel")] public Relationship blendShapeTargets = new Relationship();

        // Skeleton & Animation Binding.
        [UsdNamespace("skel")] public Relationship animationSource = new Relationship();

        [UsdNamespace("skel")] public Relationship skeleton = new Relationship();

        // Skeleton Binding Data.
        [UsdNamespace("skel")]
        [UsdVariability(Variability.Uniform)]
        public string[] joints;

        [UsdNamespace("skel")]
        [UsdVariability(Variability.Uniform)]
        public Primvar<int[]> jointIndices = new Primvar<int[]>();

        [UsdNamespace("skel")]
        [UsdVariability(Variability.Uniform)]
        public Primvar<Matrix4x4> geomBindTransform = new Primvar<Matrix4x4>();

        [UsdNamespace("skel")]
        [UsdVariability(Variability.Uniform)]
        public Primvar<float[]> jointWeights = new Primvar<float[]>();
    }

    [System.Serializable]
    [UsdSchema("SkelRoot")]
    public class SkelRootSample : BoundableSample
    {
        [UsdNamespace("skel")] public Relationship skeleton = new Relationship();
        [UsdNamespace("skel")] public Relationship animationSource = new Relationship();
    }

    [System.Serializable]
    [UsdSchema("Skeleton")]
    public class SkeletonSample : XformableSample
    {
        [UsdVariability(Variability.Uniform)] public string[] joints;

        [UsdVariability(Variability.Uniform)] public Matrix4x4[] bindTransforms;

        [UsdVariability(Variability.Uniform)] public Matrix4x4[] restTransforms;
    }

    [System.Serializable]
    [UsdSchema("SkelAnimation")]
    public class SkelAnimationSample : SampleBase
    {
        [UsdVariability(Variability.Uniform)] public string[] joints;

        // Intended to work with pxr.UsdCs.UsdSkelDecomposeTransforms()
        public Vector3[] translations;

        public Quaternion[] rotations;

        // TODO: How to support Vector3h?
        public pxr.VtVec3hArray scales;

        [UsdVariability(Variability.Uniform)] public string[] blendShapes;

        [UsdVariability(Variability.Uniform)] public float[] blendShapeWeights;
    }

    [System.Serializable]
    [UsdSchema("BlendShape")]
    public class BlendShapeSample : SampleBase
    {
        public Vector3[] offsets;
        public uint[] pointIndices;
    }

    public class SkeletonIo
    {
        private Dictionary<Transform, Transform[]> m_bindings = new Dictionary<Transform, Transform[]>();

        public Transform[] GetBones(Transform rootBone)
        {
            return m_bindings[rootBone];
        }

        public void RegisterSkeleton(Transform rootBone, Transform[] bones)
        {
            m_bindings.Add(rootBone, bones);
        }
    }
}

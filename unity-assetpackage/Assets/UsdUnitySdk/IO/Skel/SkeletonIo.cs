using System.Collections.Generic;
using UnityEngine;

namespace USD.NET.Unity {

  public class UnitySkeleton {
    public Transform rootBone;
    public Transform[] bones;
  }

  [System.Serializable]
  public class SkelBindingSample : SampleBase {
    // Blend Shapes.
    [UsdNamespace("skel")]
    public string[] blendShapes;

    [UsdNamespace("skel")]
    public Relationship blendShapeTargets = new Relationship();

    // Skeleton & Animation Binding.
    [UsdNamespace("skel")]
    public Relationship animationSource = new Relationship();

    [UsdNamespace("skel")]
    public Relationship skeleton = new Relationship();

    // Skeleton Binding Data.
    [UsdNamespace("skel")]
    public string[] joints;

    [VertexData]
    [UsdNamespace("primvars:skel")]
    public int[] jointIndices;

    [VertexData]
    [UsdNamespace("primvars:skel")]
    public Matrix4x4 geomBindTransform;

    [VertexData]
    [UsdNamespace("primvars:skel")]
    public float[] jointWeights;
  }

  [System.Serializable]
  [UsdSchema("SkelRoot")]
  public class SkelRootSample : BoundableSample {
  }

  [System.Serializable]
  [UsdSchema("Skeleton")]
  public class SkeletonSample : SampleBase {
    public string[] joints;
    public Matrix4x4[] bindTransforms;
    public Matrix4x4[] restTransforms;
  }

  [System.Serializable]
  [UsdSchema("SkelAnimation")]
  public class SkelAnimationSample : SampleBase {
    public string[] joints;
    public Vector3[] translations;
    public Quaternion[] rotations;
    public float[] scales;

    public string[] blendShapes;
    public float[] blendShapeWeights;
  }

  [System.Serializable]
  [UsdSchema("BlendShape")]
  public class BlendShapeSample : SampleBase {
    public Vector3[] offsets;
    public uint[] pointIndices;
  }

  public class SkeletonIo {
    private Dictionary<Transform, Transform[]> m_bindings = new Dictionary<Transform, Transform[]>();
    
    public Transform[] GetBones(Transform rootBone) {
      return m_bindings[rootBone];
    }

    public void RegisterSkeleton(Transform rootBone, Transform[] bones) {
      m_bindings.Add(rootBone, bones);
    }
  }

}

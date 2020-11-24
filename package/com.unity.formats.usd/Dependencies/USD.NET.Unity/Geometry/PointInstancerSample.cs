using System;
using UnityEngine;

namespace USD.NET.Unity {

  [Serializable]
  [UsdSchema("PointInstancer")]
  public class PointInstancerPrototypesSample : BoundableSample {
    public Relationship prototypes = new Relationship();
  }

  [Serializable]
  [UsdSchema("PointInstancer")]
  public class PointInstancerSample : PointInstancerPrototypesSample {

    // TODO(jcowles): This data type cannot currently be serialized due to this bug:
    // https://github.com/PixarAnimationStudios/USD/issues/639
    //[MetaData]
    //public pxr.SdfInt64ListOp inactiveIds = new pxr.SdfInt64ListOp();

    public int[] protoIndices;
    public long[] ids;
    public long[] invisibleIds;
    public Vector3[] positions;
    public Quaternion[] rotations;
    public Vector3[] scales;
    public Vector3[] velocities;
    public Vector3[] angularVelocities;

    public Matrix4x4[] ComputeInstanceMatrices(Scene scene, string primPath) {
      var prim = scene.GetPrimAtPath(primPath);
      var pi = new pxr.UsdGeomPointInstancer(prim);
      var xforms = new pxr.VtMatrix4dArray();

      pi.ComputeInstanceTransformsAtTime(xforms, scene.Time == null ? pxr.UsdTimeCode.Default() : scene.Time, pxr.UsdTimeCode.Default());

      // Slow, but works.
      var matrices = new Matrix4x4[xforms.size()];
      for (int i = 0; i < xforms.size(); i++) {
        matrices[i] = UnityTypeConverter.FromMatrix(xforms[i]);
      }
      return matrices;
    }
  }
}

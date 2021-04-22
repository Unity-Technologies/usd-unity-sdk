using UnityEngine;

namespace USD.NET.Unity {

  [System.Serializable]
  [UsdSchema("Curves")]
  public class CurvesSample : PointBasedSample {

    // TODO: Split into a base class and share with MeshSample (as defined in the schema).

    // UsdGeomGprim
    // ------------

    // USD splits display color from opacity, which allows opacity to be overridden without
    // writing color, however the cost of recombining these in C# is too great (time/memory), so
    // instead, they are fused during serialization in C++.
    [VertexData, FusedDisplayColor]
    public Color[] colors;

    [UsdVariability(Variability.Uniform)]
    public bool doubleSided;

    [UsdVariability(Variability.Uniform)]
    public Orientation orientation;

    // UsdGeomPointBased
    // -----------------
    public Vector3[] points;
    public Vector3[] normals;
    public Vector3[] velocities;

    // UsdGeomCurves
    // -------------
    public int[] curveVertexCounts;
    public float[] widths;
  }
}

using UnityEngine;

namespace USD.NET.Unity {

  [System.Serializable]
  [UsdSchema("UsdGeomGprim")]
  public class GprimSample : BoundableSample {

    // USD splits display color from opacity, which allows opacity to be overridden without
    // writing color, however the cost of recombining these in C# is too great (time/memory), so
    // instead, they are fused during serialization in C++.
    [VertexData, FusedDisplayColor]
    public Color[] colors;

  }

}

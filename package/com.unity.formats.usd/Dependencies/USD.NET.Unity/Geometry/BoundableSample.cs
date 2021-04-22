using UnityEngine;

namespace USD.NET.Unity {

  [System.Serializable]
  [UsdSchema("UsdGeomBoundable")]
  public class BoundableSample : XformableSample {
    public Bounds extent;
  }
}

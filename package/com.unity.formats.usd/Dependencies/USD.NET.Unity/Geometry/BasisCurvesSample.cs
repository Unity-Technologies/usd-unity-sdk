namespace USD.NET.Unity {

  [System.Serializable]
  [UsdSchema("BasisCurves")]
  public class BasisCurvesSample : CurvesSample {

    public enum CurveType {
      Linear,
      Cubic,
    }

    public enum Basis {
      Bezier,
      Bspline,
      CatmullRom,
      Hermite,
      Power,
    }

    public enum WrapMode {
      Nonperiodic,
      Periodic,
    }

    public CurveType type;
    public Basis basis;
    public WrapMode wrap;
  }
}

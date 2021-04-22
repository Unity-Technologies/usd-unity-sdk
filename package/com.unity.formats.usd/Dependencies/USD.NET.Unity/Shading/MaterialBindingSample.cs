namespace USD.NET.Unity {

  /// <summary>
  /// Provides access to the material binding (if any) on the UsdPrim.
  /// </summary>
  [System.Serializable]
  public class MaterialBindingSample : SampleBase {
    public MaterialBindingSample() : base() {
    }

    public MaterialBindingSample(string materialPath) : base() {
      binding = new Relationship(materialPath);
    }

    [UsdNamespace("material")]
    public Relationship binding;
  }

}

namespace USD.NET.Unity {

  [System.Serializable]
  [UsdSchema("Texture2D")]
  public class Texture2DSample : SampleBase {
    [UsdNamespace("inputs"), UsdAssetPath]
    public Connectable<string> sourceFile = new Connectable<string>();
    public bool sRgb;
  }

}

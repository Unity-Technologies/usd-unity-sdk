namespace USD.NET.Unity {

  /// <summary>
  /// A declaration that a given value is a shader input, used to discover shader parameters at
  /// runtime. Implicitly declares the parameter to be in the "inputs" namespace.
  /// </summary>
  public class InputParameterAttribute : UsdNamespaceAttribute {

    public string UnityName { get; private set; }

    public InputParameterAttribute() : base("inputs") {
      UnityName = "";
    }

    /// <param name="unityName">The corresponding name in the Unity shader.</param>
    public InputParameterAttribute(string unityName) : base("inputs") {
      UnityName = unityName;
    }
  }

  /// <summary>
  /// Declares a given parameter to be a texture, used to discover shader textures at runtime.
  /// Implicitly declares the parameter to be in the "inputs" namespace.
  /// </summary>
  public class InputTextureAttribute : UsdNamespaceAttribute {

    public string UnityName { get; private set; }

    public InputTextureAttribute() : base("inputs") {
      UnityName = "";
    }

    /// <param name="unityName">The corresponding name in the Unity shader.</param>
    public InputTextureAttribute(string unityName) : base("inputs") {
      UnityName = unityName;
    }
  }

  /// <summary>
  /// Declares required keywords when the given parameter is used.
  /// Only currently applicable to textures.
  /// </summary>
  public class RequireShaderKeywordsAttribute : System.Attribute {

    public string[] Keywords { get; private set; }

    public RequireShaderKeywordsAttribute(string keyword) {
      Keywords = new string[] { keyword };
    }

    public RequireShaderKeywordsAttribute(string[] keywords) {
      Keywords = keywords;
    }
  }

}

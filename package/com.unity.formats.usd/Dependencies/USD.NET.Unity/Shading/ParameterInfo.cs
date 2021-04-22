namespace USD.NET.Unity {

  /// <summary>
  /// A class describing a single shader parameter, used to simplify reading values from USD.
  /// </summary>
  public struct ParameterInfo {
    /// <summary>
    /// The value to use when the parameter is not connected.
    /// </summary>
    public object value;

    /// <summary>
    /// A source connected to the parameter, null if not connected.
    /// Note that this path will always target an attribute, not the prim itself.
    /// </summary>
    public string connectedPath;

    /// <summary>
    /// The name of the parameter, as declared in the USD Sample class.
    /// </summary>
    public string usdName;

    /// <summary>
    /// The name of the parameter, as declared in the Unity shader source file.
    /// </summary>
    public string unityName;

    /// <summary>
    /// Some shaders require enabling keywords to enable features, this is the list of required
    /// keywords associated with this parameter.
    /// </summary>
    public string[] requiredShaderKeywords;

    public override string ToString() {
      return usdName + " (" + unityName + ") "
           + "<" + connectedPath + "> "
           + (value != null ? value.GetType().ToString() : "null");
    }
  }

}

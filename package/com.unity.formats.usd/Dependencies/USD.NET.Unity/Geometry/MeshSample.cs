namespace USD.NET.Unity {

  // This class adds the additional data needed to make the mesh a proper USD mesh. This is split
  // out as a separate class because in the common read case, this data is not needed. Rather than
  // splitting the class, the values could also be read individually, though with a performance hit.

  [System.Serializable]
  [UsdSchema("Mesh")]
  public class MeshSample : MeshSampleBase {
    public Visibility visibility;

    [UsdVariability(Variability.Uniform)]
    public Purpose purpose;

    [UsdVariability(Variability.Uniform)]
    public bool doubleSided;

    [UsdVariability(Variability.Uniform)]
    public Orientation orientation;

    // Should be an array of "3", one for each triangle, unles arbitrary polygons are used.
    public int[] faceVertexCounts;

    // ------------------------------------------------------------------------------------------ //
    // Helper Functions
    // ------------------------------------------------------------------------------------------ //

    /// <summary>
    /// Sets the faceVertexIndices and faceVertexCounts from triangle indices alone.
    /// </summary>
    public void SetTriangles(int[] triangleIndices) {
      faceVertexIndices = triangleIndices;
      faceVertexCounts = new int[faceVertexIndices.Length / 3];
      for (int i = 0; i < faceVertexCounts.Length; i++) {
        faceVertexCounts[i] = 3;
      }
    }
  }
}

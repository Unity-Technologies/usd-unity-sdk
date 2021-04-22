namespace USD.NET.Unity {

  [System.Serializable]
  [UsdSchema("Cube")]
  public class CubeSample : GprimSample {

    public CubeSample() : base() {
    }

    public CubeSample(double size) : base() {
      m_size = size;
    }

    // Indicates the length of each side of the cube.
    public double size {
      get { return m_size; }
      set {
        m_size = value;
        // Authoring size requires authoring extent.
        // TODO(jcowles): this should be disable during deserialization.
        extent = new UnityEngine.Bounds(UnityEngine.Vector3.zero,
                                        UnityEngine.Vector3.one * (float)m_size);
      }
    }

    private double m_size;
  }
}

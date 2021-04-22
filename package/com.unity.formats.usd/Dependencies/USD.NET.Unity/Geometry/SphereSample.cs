namespace USD.NET.Unity {

  [System.Serializable]
  [UsdSchema("Sphere")]
  public class SphereSample : GprimSample {

    public SphereSample() : base() {
    }

    public SphereSample(double radius) : base() {
      m_radius = radius;
    }

    // Indicates the radius of the sphere.
    public double radius {
      get { return m_radius; }
      set {
        m_radius = value;
        // Authoring radius requires authoring extent.
        // TODO(jcowles): this should be disable during deserialization.
        extent = new UnityEngine.Bounds(UnityEngine.Vector3.zero,
                                        UnityEngine.Vector3.one * (float)m_radius * 2);
      }
    }

    private double m_radius;
  }
}

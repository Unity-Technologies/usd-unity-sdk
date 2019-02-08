using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace USD.NET.Unity {

  [System.Serializable]
  public class UsdPlayableAsset : PlayableAsset {
    private UsdAsset m_sourceUsdAsset;

    [Tooltip("USD Player to Control")]
    public ExposedReference<UsdAsset> SourceUsdAsset;
    public string UsdRootPath;

    public ClipCaps clipCaps { get { return ClipCaps.Extrapolation | ClipCaps.Looping | ClipCaps.SpeedMultiplier | ClipCaps.ClipIn; } }

    public UsdAsset GetUsdAsset() {
      m_sourceUsdAsset.m_usdRootPath = UsdRootPath;
      return m_sourceUsdAsset;
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
      var usdPlayable = ScriptPlayable<UsdPlayableBehaviour>.Create(graph);
      UsdPlayableBehaviour behaviour = usdPlayable.GetBehaviour();
      m_sourceUsdAsset = SourceUsdAsset.Resolve(graph.GetResolver());

      if (m_sourceUsdAsset != null) {
        behaviour.playableAsset = this;
        UsdRootPath = m_sourceUsdAsset.m_usdRootPath;
        name = System.IO.Path.GetFileName(m_sourceUsdAsset.m_usdFile);
      }

      return usdPlayable;
    }

    public override double duration {
      get {
        double dur = 0;
        try {
          dur = m_sourceUsdAsset == null ? 0 : m_sourceUsdAsset.Length;
        } catch (System.Exception ex) {
          Debug.LogException(new System.Exception("Failed to read clip duration", ex));
        }
        return dur;
      }
    }

  }
}
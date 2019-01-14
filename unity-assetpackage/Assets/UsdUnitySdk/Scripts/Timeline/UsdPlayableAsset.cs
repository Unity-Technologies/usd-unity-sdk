using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace USD.NET.Unity {

  [System.Serializable]
  public class UsdPlayableAsset : PlayableAsset {
    UsdAsset m_runtimeRoot;

    [Tooltip("USD Player to Control")]
    public ExposedReference<UsdAsset> UsdStageRoot;

    public ClipCaps clipCaps { get { return ClipCaps.Extrapolation | ClipCaps.Looping | ClipCaps.SpeedMultiplier | ClipCaps.ClipIn; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
      var usdPlayable = ScriptPlayable<UsdPlayableBehaviour>.Create(graph);
      UsdPlayableBehaviour behaviour = usdPlayable.GetBehaviour();
      m_runtimeRoot = UsdStageRoot.Resolve(graph.GetResolver());
	  if (m_runtimeRoot != null) {
	    behaviour.player = m_runtimeRoot;
	    name = System.IO.Path.GetFileName(m_runtimeRoot.m_usdFile);
	  }
      return usdPlayable;
    }

    public override double duration {
      get {
        double dur = 0;
        try {
          dur = m_runtimeRoot == null ? 0 : m_runtimeRoot.Length;
        } catch (System.Exception ex) {
          Debug.LogException(new System.Exception("Failed to read clip duration", ex));
        }
        return dur;
      }
    }

  }
}
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace USD.NET.Unity.Extensions.Timeline {
  [System.Serializable]
  public class USDPlayableAsset : PlayableAsset {
    StageRoot player;

    [Tooltip("USD Player to Control")]
    public ExposedReference<StageRoot> UsdStageRoot;

    public ClipCaps clipCaps { get { return ClipCaps.Extrapolation | ClipCaps.Looping | ClipCaps.SpeedMultiplier | ClipCaps.ClipIn; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
      var playable = ScriptPlayable<USDPlayableBehaviour>.Create(graph);
      USDPlayableBehaviour behaviour = playable.GetBehaviour();
      player = UsdStageRoot.Resolve(graph.GetResolver());
      behaviour.player = player;
      return playable;
    }

    public override double duration {
      get {
        return player == null ? 0 : player.Length;
      }
    }
  }
}
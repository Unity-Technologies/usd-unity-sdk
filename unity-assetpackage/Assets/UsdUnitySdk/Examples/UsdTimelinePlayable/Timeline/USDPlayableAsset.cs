using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using USD.NET.Unity.Extensions.Player;

namespace USD.NET.Unity.Extensions.Timeline {
  [System.Serializable]
  public class USDPlayableAsset : PlayableAsset {
    UsdPlayer player;

    [Tooltip("USD Player to Control")]
    public ExposedReference<UsdPlayer> Player;

    public ClipCaps clipCaps { get { return ClipCaps.Extrapolation | ClipCaps.Looping | ClipCaps.SpeedMultiplier | ClipCaps.ClipIn; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
      var playable = ScriptPlayable<USDPlayableBehaviour>.Create(graph);
      var behaviour = playable.GetBehaviour();
      player = Player.Resolve(graph.GetResolver());
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
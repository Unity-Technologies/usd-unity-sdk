using UnityEngine.Playables;
using UnityEngine;

namespace USD.NET.Unity.Extensions.Timeline {
  // A behaviour that is attached to a playable
  public class USDPlayableBehaviour : PlayableBehaviour {
    public StageRoot player;
    public GameObject root;

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable) {
      USD.NET.Examples.InitUsd.Initialize();
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable) {

    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info) {

    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info) {

    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info) {
      player.SetTime(playable.GetTime());
    }
  }
}
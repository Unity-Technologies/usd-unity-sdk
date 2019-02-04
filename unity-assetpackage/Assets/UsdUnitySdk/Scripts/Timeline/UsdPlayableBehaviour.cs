using UnityEngine.Playables;
using UnityEngine;

namespace USD.NET.Unity {
  // A behaviour that is attached to a playable
  public class UsdPlayableBehaviour : PlayableBehaviour {
    public UsdPlayableAsset playableAsset;

    private bool m_errorOnce = true;

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
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
      if (playableAsset == null) { return; }

      var sourceUsdAsset = playableAsset.GetUsdAsset();
      if (sourceUsdAsset == null) { return; }

      var targetUsdAsset = playerData as UsdAsset;
      if (targetUsdAsset == null) {
        if (m_errorOnce) {
          Debug.LogError("Error: track data has no target UsdStageRoot");
          m_errorOnce = false;
        }
        return;
      } else {
        m_errorOnce = true;
      }

      if (!targetUsdAsset.isActiveAndEnabled) {
        return;
      }

      sourceUsdAsset.SetTime(playable.GetTime(), targetUsdAsset, saveMeshUpdates: false);
    }

    public override void PrepareData(Playable playable, FrameData info) {
    }

  }
}
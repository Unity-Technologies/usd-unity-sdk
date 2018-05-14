using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using USD.NET.Unity.Extensions.Player;

namespace USD.NET.Unity.Extensions.Timeline
{
    // A behaviour that is attached to a playable
    public class USDPlayableBehaviour : PlayableBehaviour
    {
        public UsdPlayer player;

        // Called when the owning graph starts playing
        public override void OnGraphStart(Playable playable)
        {
            player.SetupScene();
        }

        // Called when the owning graph stops playing
        public override void OnGraphStop(Playable playable)
        {

        }

        // Called when the state of the playable is set to Play
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {

        }

        // Called when the state of the playable is set to Paused
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {

        }

        // Called each frame while the state is set to Play
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            player.SetTime(playable.GetTime());
        }
    }
}
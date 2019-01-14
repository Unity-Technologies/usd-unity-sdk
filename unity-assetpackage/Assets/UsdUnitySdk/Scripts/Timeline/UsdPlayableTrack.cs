using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace USD.NET.Unity {

#if UNITY_2017
  [TrackMediaType(TimelineAsset.MediaType.Script)]
#endif
  [TrackClipType(typeof(UsdPlayableAsset))]
  [TrackBindingType(typeof(UsdAsset))]
  [TrackColor(0.1f, 0.2f, 0.8f)]
  [System.Serializable]
  public class UsdPlayableTrack : TrackAsset {
#if !UNITY_2018_1 && !UNITY_2017
    protected override void OnCreateClip(TimelineClip clip) {
      base.OnCreateClip(clip);
      clip.displayName = clip.asset.name;
    }
#endif
  }
}
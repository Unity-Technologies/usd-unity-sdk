using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace USD.NET.Unity.Extensions.Timeline {
  [System.Serializable]
  [TrackClipType(typeof(USDPlayableAsset))]
#if UNITY_2017
  [TrackMediaType(TimelineAsset.MediaType.Script)]
#endif
  [TrackColor(0.1f, 0.2f, 0.8f)]
  public class USDPlayableTrack : TrackAsset {

  }
}
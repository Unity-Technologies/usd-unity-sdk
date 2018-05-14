using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace USD.NET.Unity.Extensions.Timeline
{
    [System.Serializable]
    [TrackClipType(typeof(USDPlayableAsset))]
    [TrackMediaType(TimelineAsset.MediaType.Script)]
    [TrackColor(0.1f, 0.2f, 0.8f)]
    public class USDPlayableTrack : TrackAsset
    {

    }
}
// Copyright 2018 Jeremy Cowles. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine.Timeline;

namespace Unity.Formats.USD
{
    [TrackClipType(typeof(UsdPlayableAsset))]
    [TrackBindingType(typeof(UsdAsset))]
    [TrackColor(0.1f, 0.2f, 0.8f)]
    [System.Serializable]
    public class UsdPlayableTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);
            clip.displayName = clip.asset.name;
        }
    }
}

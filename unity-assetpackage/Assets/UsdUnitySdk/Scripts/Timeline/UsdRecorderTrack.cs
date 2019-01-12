// Copyright 2019 Jeremy Cowles. All rights reserved.
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

using UnityEngine;
using UnityEngine.Timeline;

namespace USD.NET.Unity {
  [System.Serializable]
  [TrackClipType(typeof(UsdRecorderClip))]
#if UNITY_2017 || UNITY_2018_2 || UNITY_2018_1
  [TrackMediaType(TimelineAsset.MediaType.Script)]
#endif
  [TrackColor(0.33f, 0.0f, 0.08f)]
  public class UsdRecorderTrack : TrackAsset {
  }
}
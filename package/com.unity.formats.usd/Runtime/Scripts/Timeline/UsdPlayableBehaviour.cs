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

using UnityEngine.Playables;
using UnityEngine;

namespace Unity.Formats.USD
{
    // A behaviour that is attached to a playable
    public class UsdPlayableBehaviour : PlayableBehaviour
    {
        public UsdPlayableAsset playableAsset;

        private bool m_errorOnce = true;

        // Called when the owning graph starts playing
        public override void OnGraphStart(Playable playable)
        {
            InitUsd.Initialize();
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
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playableAsset == null)
            {
                return;
            }

            var targetUsdAsset = playerData as UsdAsset;
            if (targetUsdAsset == null)
            {
                if (m_errorOnce)
                {
                    Debug.LogError("Error: track data has no target UsdStageRoot");
                    m_errorOnce = false;
                }

                return;
            }
            else
            {
                m_errorOnce = true;
            }

            var sourceUsdAsset = playableAsset.GetUsdAsset(targetUsdAsset.m_usdRootPath);
            if (sourceUsdAsset == null)
            {
                return;
            }

            if (!targetUsdAsset.isActiveAndEnabled)
            {
                return;
            }

            sourceUsdAsset.SetTime(playable.GetTime(), targetUsdAsset, saveMeshUpdates: false);
        }

        public override void PrepareData(Playable playable, FrameData info)
        {
        }
    }
}

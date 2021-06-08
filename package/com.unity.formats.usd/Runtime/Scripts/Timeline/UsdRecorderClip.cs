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
using UnityEngine.Playables;
using UnityEngine.Timeline;
using USD.NET;

namespace Unity.Formats.USD
{
    [System.ComponentModel.DisplayName("USD Recorder Clip")]
    public class UsdRecorderClip : PlayableAsset, ITimelineClipAsset
    {
        // The root GameObject to export to USD.
        public ExposedReference<GameObject> m_exportRoot;
        private GameObject[] m_trackedRoots;

        public bool m_exportMaterials = true;
        public BasisTransformation m_convertHandedness = BasisTransformation.SlowAndSafe;
        public ActiveExportPolicy m_activePolicy = ActiveExportPolicy.ExportAsVisibility;

        // The path to where the USD file will be written.
        // If null/empty, the file will be created in memory only.
        public string m_usdFile = "Assets/recording.usd";

        // The scene object to which the recording will be saved.
        public Scene UsdScene { get; set; }

        ExportContext m_context = new ExportContext();

        public ExportContext Context
        {
            get { return m_context; }
            set { m_context = value; }
        }

        public GameObject GetExportRoot(PlayableGraph graph)
        {
            return m_exportRoot.Resolve(graph.GetResolver());
        }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public bool IsUSDZ => !string.IsNullOrEmpty(m_usdFile) && m_usdFile.ToLowerInvariant().EndsWith(".usdz");

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var ret = ScriptPlayable<UsdRecorderBehaviour>.Create(graph);
            var behaviour = ret.GetBehaviour();
            behaviour.Clip = this;
            return ret;
        }

        public virtual void OnDestroy()
        {
        }
    }
}

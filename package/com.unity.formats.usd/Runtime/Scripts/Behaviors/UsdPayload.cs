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

using UnityEditor;
using UnityEngine;

namespace Unity.Formats.USD
{
    [ExecuteInEditMode]
    public class UsdPayload : MonoBehaviour
    {
        [SerializeField]
        private bool m_isLoaded = true;

        // Variable used to track dirty load state.
        [SerializeField]
        [HideInInspector]
        private bool m_wasLoaded = true;

        /// <summary>
        /// Returns true of the prim is currently loaded. Note that this will return the currently
        /// requested load state, which may be pending to be applied on the next update.
        /// </summary>
        public bool IsLoaded
        {
            get { return m_isLoaded; }
        }

        /// <summary>
        /// Sets the current state to loaded. The actual chagne will be applied on next Update().
        /// </summary>
        public void Load()
        {
            m_isLoaded = true;
        }

        /// <summary>
        /// Sets the current state to unloaded. The actual change will be applied on next Update().
        /// </summary>
        public void Unload()
        {
            m_isLoaded = false;
        }

        /// <summary>
        /// Sets the current state without triggering load/unload operation from the USD scene state.
        /// </summary>
        /// <param name="loaded"></param>
        public void SetInitialState(bool loaded)
        {
            m_isLoaded = loaded;
            m_wasLoaded = loaded;
        }

        /// <summary>
        /// Synchronize the current state in Unity into the USD scenegraph.
        /// </summary>
        public void Update()
        {
            if (m_isLoaded == m_wasLoaded)
            {
                return;
            }

            m_wasLoaded = m_isLoaded;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif

            var stageRoot = transform.GetComponentInParent<UsdAsset>();
            stageRoot.SetPayloadState(this.gameObject, m_isLoaded);
        }
    }
}

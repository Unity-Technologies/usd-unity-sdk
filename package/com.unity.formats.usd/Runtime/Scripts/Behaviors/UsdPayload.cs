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

using System.Linq;
using UnityEngine;

namespace USD.NET.Unity {

  [ExecuteInEditMode]
  public class UsdPayload : MonoBehaviour {
    [SerializeField]
    private bool m_isLoaded = true;
    private bool m_wasLoaded = true;

    public bool IsLoaded { get { return m_isLoaded; } }

    private void Start() {
      bool loaded = GetComponentInParent<UsdAsset>().m_payloadPolicy == PayloadPolicy.LoadAll;
      m_isLoaded = loaded;
      m_wasLoaded = m_isLoaded;
    }

    public void Load() {
      m_isLoaded = true;
    }

    public void Unload() {
      m_isLoaded = false;
    }

    /// <summary>
    /// Sets the current state without changing the USD scene state.
    /// </summary>
    /// <param name="loaded"></param>
    public void SetInitialState(bool loaded) {
      m_isLoaded = loaded;
      m_wasLoaded = loaded;
    }

    public void Update() {
      if (m_isLoaded == m_wasLoaded) { return; }
      m_wasLoaded = m_isLoaded;

      //Debug.Log("SetPayload: " + GetComponent<UsdPrimSource>().m_usdPrimPath);
      var stageRoot = transform.GetComponentInParent<UsdAsset>();
      stageRoot.SetPayloadState(this.gameObject, m_isLoaded);
    }

  }
}

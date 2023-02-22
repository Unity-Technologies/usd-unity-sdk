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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Formats.USD
{
    [ExecuteInEditMode]
    class UsdWaitForEndOfFrame : MonoBehaviour
    {
        List<Action> m_pending = new List<Action>();

        static UsdWaitForEndOfFrame s_instance;

        static UsdWaitForEndOfFrame GetInstance()
        {
            if (s_instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                s_instance = GameObject.FindAnyObjectByType<UsdWaitForEndOfFrame>();
#else
                s_instance = GameObject.FindObjectOfType<UsdWaitForEndOfFrame>();
#endif
                if (s_instance == null)
                {
                    var go = new GameObject();
                    go.name = "UsdRecorderHelper";
                    go.hideFlags = HideFlags.HideAndDontSave;
                    s_instance = go.AddComponent<UsdWaitForEndOfFrame>();
                }
            }

            return s_instance;
        }

        public static void Add(Action callback)
        {
            GetInstance().m_pending.Add(callback);
        }

        IEnumerator WaitForEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            foreach (var callback in m_pending)
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            m_pending.Clear();
        }

        void LateUpdate()
        {
            StartCoroutine(WaitForEndOfFrame());
        }
    }
}

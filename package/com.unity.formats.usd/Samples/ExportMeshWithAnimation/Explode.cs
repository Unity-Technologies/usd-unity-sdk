// Copyright 2017 Google Inc. All rights reserved.
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

namespace USD.NET.Examples
{
    public class Explode : MonoBehaviour
    {
        public float m_explodeTime = 10;
        public Transform m_effectRoot;
        public float m_force = 1;
        public float m_radius = 1;

        private float currTime;

        void Update()
        {
            currTime += Time.deltaTime;

            if (currTime > m_explodeTime)
            {
                currTime = 0;

                foreach (Rigidbody rb in m_effectRoot.GetComponentsInChildren<Rigidbody>())
                {
                    rb.AddExplosionForce(m_force, transform.position, m_radius);
                }
            }
        }
    }
}

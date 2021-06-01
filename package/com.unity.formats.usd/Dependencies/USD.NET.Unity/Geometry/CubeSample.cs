// Copyright 2021 Unity Technologies. All rights reserved.
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

namespace USD.NET.Unity
{
    [System.Serializable]
    [UsdSchema("Cube")]
    public class CubeSample : GprimSample
    {
        public CubeSample() : base()
        {
        }

        public CubeSample(double size) : base()
        {
            m_size = size;
        }

        // Indicates the length of each side of the cube.
        public double size
        {
            get { return m_size; }
            set
            {
                m_size = value;
                // Authoring size requires authoring extent.
                // TODO(jcowles): this should be disable during deserialization.
                extent = new UnityEngine.Bounds(UnityEngine.Vector3.zero,
                    UnityEngine.Vector3.one * (float)m_size);
            }
        }

        private double m_size;
    }
}

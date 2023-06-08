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

using System.Collections;
using System.Collections.Generic;
using pxr;

namespace USD.NET
{
    /// <summary>
    /// Provides custom enumeration over an SdfPathVector to avoid generation of garbage.
    /// </summary>
    public class SampleCollection<T> : IEnumerable<SampleEnumerator<T>.SampleHolder> where T : SampleBase, new()
    {
        // CAUTION: Swig's current wrapper for std::vector returns references into the vectors private memory,
        // which means any item taken from the vector cannot be used long term and should be cloned. For
        // this reason, the vector is proactively copied into an array for safety.
        private SdfPath[] m_paths;
        private Scene m_scene;

        public int Length
        {
            get => m_paths.Length;
        }

        public SampleCollection(Scene scene, SdfPathVector paths)
        {
            m_paths = new SdfPath[paths.Count];
            paths.CopyTo(m_paths);
            m_scene = scene;
        }

        public IEnumerator<SampleEnumerator<T>.SampleHolder> GetEnumerator()
        {
            return new SampleEnumerator<T>(m_scene, m_paths);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SampleEnumerator<T>(m_scene, m_paths);
        }
    }
}

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
    /// Enumerates all (SdfPath,Sample) pairs for a given path set and sample type.
    /// </summary>
    public class SampleEnumerator<T> : IEnumerator<SampleEnumerator<T>.SampleHolder> where T : SampleBase, new()
    {
        private SdfPath[] m_paths;

        private int m_i = -1;

        // Avoid garbage churn.
        private SampleHolder m_currentSample = new SampleHolder();
        private Scene m_scene;

        public class SampleHolder
        {
            public T sample = new T();
            public SdfPath path = new SdfPath();
        }

        public SampleEnumerator(Scene scene, SdfPath[] paths)
        {
            m_scene = scene;
            m_paths = paths;
            m_i = -1;
        }

        public SampleHolder Current
        {
            get
            {
                return m_currentSample;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return m_currentSample;
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            m_i++;
            bool valid = m_i < m_paths.Length;
            if (valid)
            {
                m_currentSample.path = m_paths[m_i];
                m_currentSample.sample = new T();
                m_scene.Read(m_currentSample.path, m_currentSample.sample);
            }
            return valid;
        }

        public void Reset()
        {
            m_i = -1;
        }
    }
}

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

using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using pxr;
using System.Collections;
using System.Threading;
using USD.NET;

namespace Unity.Formats.USD
{
    /// <summary>
    /// Uses the C# job system to read all data for the given path list and presents it as an
    /// enumerator.
    /// </summary>
    /// <remarks>
    /// Internally the reads happen in a background thread while the main thread is unblocked to
    /// begin processing the data as it arrives.
    ///
    /// Note because this class is templated on T, the static variables are unique to each
    /// instantiation of T; this is true regardless of whether or not the static uses type T.
    /// </remarks>
    public struct ReadAllJob<T> :
        IEnumerator<SampleEnumerator<T>.SampleHolder>,
            IEnumerable<SampleEnumerator<T>.SampleHolder>,
            IJobParallelFor
        where T : SampleBase, ISanitizable, new()
    {
        static private Scene m_scene;
        static private SdfPath[] m_paths;
        static private T[] m_results;

        // Previously, setting elements in a bool[] caused a data race, resumably because of
        // bit packing. Here we use an object[] to avoid this, however there was no negative
        // effect of using a bool[] here, though the bug may have just not presented itself.
        // An alternative lock-free implementation would also be fine (preferable) here.
        static private object[] m_done;

        static SampleEnumerator<T>.SampleHolder m_current;
        static private AutoResetEvent m_ready;

        static SceneImportOptions m_importOptions;

        public SampleEnumerator<T>.SampleHolder Current
        {
            get { return m_current; }
        }

        object IEnumerator.Current
        {
            get { return m_current; }
        }

        public ReadAllJob(Scene scene, SdfPath[] paths, SceneImportOptions importOptions)
        {
            m_ready = new AutoResetEvent(false);
            m_scene = scene;
            m_results = new T[paths.Length];
            m_done = new object[paths.Length];
            m_current = new SampleEnumerator<T>.SampleHolder();
            m_paths = paths;
            m_importOptions = importOptions;
        }

        private bool ShouldReadPath(Scene scene, SdfPath path)
        {
            return scene.AccessMask == null
                || scene.IsPopulatingAccessMask
                || scene.AccessMask.Included.ContainsKey(path);
        }

        public void Run()
        {
            for (int i = 0; i < m_paths.Length; i++)
            {
                Execute(i);
            }
        }

        public void Execute(int index)
        {
            var sample = new T();
            if (ShouldReadPath(m_scene, m_paths[index]))
            {
                var sampleWithExtraPrimvars = sample as IArbitraryPrimvars;
                if (sampleWithExtraPrimvars != null)
                {
                    AddPrimvarsFromMaterial(index, ref sampleWithExtraPrimvars);
                }

                m_scene.Read(m_paths[index], sample);

                var restorableSample = sample as IRestorable;
                DeserializationContext deserializationContext = null;
                m_scene.AccessMask?.Included.TryGetValue(m_paths[index], out deserializationContext);
                if (restorableSample != null && deserializationContext != null)
                {
                    restorableSample.FromCachedData(deserializationContext.cachedData);
                    sample.Sanitize(m_scene, m_importOptions);
                    //Don't update the state after the first frame
                    if (deserializationContext.cachedData == null)
                        deserializationContext.cachedData = restorableSample.ToCachedData();
                }
                else
                {
                    sample.Sanitize(m_scene, m_importOptions);
                }
            }
            else
            {
                sample = null;
                // Any object value works here, the test below is if m_done[i] == null.
                m_done[index] = true;
            }

            m_results[index] = sample;

            m_ready.Set();
        }

        /// <summary>
        /// Add all the primvars needed by the material to the sample arbitrary primvars list.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sample"></param>
        static void AddPrimvarsFromMaterial(int index, ref IArbitraryPrimvars sample)
        {
            var materialPath = "";
            var bind = new UsdShadeMaterialBindingAPI(m_scene.GetPrimAtPath(m_paths[index]));
            //what happens for materials per face?
            var rel = bind.GetDirectBindingRel();
            if (rel.GetTargets().Count > 0)
            {
                materialPath = rel.GetTargets()[0].GetPrimPath();
            }

            var primvars = m_importOptions.materialMap.GetPrimvars(materialPath);
            if (primvars != null)
            {
                sample.AddPrimvars(primvars);
            }
        }

        public bool MoveNext()
        {
            bool hasWork = true;

            int j = 0;
            while (hasWork)
            {
                hasWork = false;
                for (int i = 0; i < m_done.Length; i++)
                {
                    hasWork = hasWork || (m_done[i] == null);
                }

                if (!hasWork)
                {
                    return false;
                }

                for (int i = 0; i < m_done.Length; i++)
                {
                    if (m_done[i] == null && m_results[i] != null)
                    {
                        m_current.path = m_paths[i];
                        m_current.sample = m_results[i];
                        m_done[i] = true;
                        return true;
                    }
                }

                j++;
                if (!m_ready.WaitOne(10000))
                {
                    Debug.LogError("Timed out while waiting for thread read");
                    return false;
                }
            }

            return false;
        }

        public void Reset()
        {
            for (int i = 0; i < m_done.Length; i++)
            {
                m_done[i] = false;
            }
        }

        public void Dispose()
        {
            m_ready.Close();
        }

        public IEnumerator<SampleEnumerator<T>.SampleHolder> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}

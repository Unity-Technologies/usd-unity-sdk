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

#if !UNITY_2017
using Unity.Jobs;
#endif

using pxr;
using System.Collections;
using System.Threading;
using USD.NET;

namespace Unity.Formats.USD {

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
      IEnumerable<SampleEnumerator<T>.SampleHolder>
#if !UNITY_2017
      , IJobParallelFor
#endif
      where T : SampleBase, new() {

    static private Scene m_scene;
    static private SdfPath[] m_paths;
    static private T[] m_results;
    static private bool[] m_done;
    static private bool[] m_written;
    static SampleEnumerator<T>.SampleHolder m_current;
    static private AutoResetEvent m_ready;

    public SampleEnumerator<T>.SampleHolder Current
    {
      get {
        return m_current;
      }
    }

    object IEnumerator.Current
    {
      get {
        return m_current;
      }
    }

    public ReadAllJob(Scene scene, SdfPath[] paths) {
      m_ready = new AutoResetEvent(false);
      m_scene = scene;
      m_results = new T[paths.Length];
      m_done = new bool[paths.Length];
      m_written = new bool[paths.Length];
      m_current = new SampleEnumerator<T>.SampleHolder();
      m_paths = paths;
    }

    private bool ShouldReadPath(Scene scene, SdfPath path) {
      return scene.AccessMask == null
          || scene.IsPopulatingAccessMask
          || scene.AccessMask.Included.ContainsKey(path);
    }

    public void Run() {
      for (int i = 0; i < m_paths.Length; i++) {
        Execute(i);
      }
    }

    public void Execute(int index) {
      var sample = new T();
      if (ShouldReadPath(m_scene, m_paths[index])) {
        m_scene.Read(m_paths[index], sample);
      } else {
        sample = null;
        m_done[index] = true;
      }
      m_results[index] = sample;
      m_written[index] = true;
      
      m_ready.Set();
    }

    public bool MoveNext() {
      bool hasWork = true;

      int j = 0;
      while (hasWork) {
        hasWork = false;
        for (int i = 0; i < m_done.Length; i++) {
          hasWork = hasWork || (m_done[i] == false);
        }

        if (!hasWork) {
          return false;
        }

        for (int i = 0; i < m_done.Length; i++) {
          if (m_done[i] == false && m_written[i] == true) {
            m_current.path = m_paths[i];
            m_current.sample = m_results[i];
            m_done[i] = true;
            return true;
          }
        }

        j++;
        if (!m_ready.WaitOne(1000)) {
          Debug.LogError("Timed out while waiting for thread read");
          return false;
        }
      }

      return false;
    }

    public void Reset() {
      for (int i = 0; i < m_done.Length; i++) {
        m_done[i] = false;
      }
    }

    public void Dispose() {
      m_ready.Close();
    }

    public IEnumerator<SampleEnumerator<T>.SampleHolder> GetEnumerator() {
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this;
    }
  }
}

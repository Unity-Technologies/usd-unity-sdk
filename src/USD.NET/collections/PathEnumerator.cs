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

namespace USD.NET {

  /// <summary>
  /// Enumerates all SdfPaths in an SdfPathVector without generating garbage from temp SdfPaths.
  /// </summary>
  internal class PathEnumerator : IEnumerator<SdfPath> {
    private SdfPathVector m_paths;
    private int m_i = -1;
    private int m_size = 0;

    // Avoid garbage churn.
    private SdfPath m_current = new SdfPath();

    public PathEnumerator(SdfPathVector paths) {
      m_paths = paths;
      m_i = -1;
      m_size = paths.Count;
    }

    public SdfPath Current {
      get {
        return m_current;
      }
    }

    object IEnumerator.Current {
      get {
        return m_current;
      }
    }

    public void Dispose() {
    }

    public bool MoveNext() {
      m_i++;
      bool valid = m_i < m_size;
      if (valid) {
        UsdCs.GetPathFromVector(m_paths, m_i, m_current);
      }
      return valid;
    }

    public void Reset() {
      m_i = -1;
    }
  }
}

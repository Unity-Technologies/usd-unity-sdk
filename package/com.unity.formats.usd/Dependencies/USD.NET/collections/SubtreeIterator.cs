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
    /// Convenience class to convert a UsdPrimSubtreeRange into something easily enumerable. Note that
    /// UsdPrimSubtreeRange returns an instance of this class to implement IEnumerable.
    /// </summary>
    public class SubtreeIterator :
        IEnumerable,
        IEnumerable<pxr.UsdPrim>,
        IEnumerator,
        IEnumerator<pxr.UsdPrim>
    {
        private UsdPrimSubtreeRange m_range;
        private UsdPrimSubtreeIterator m_cur;

        // C# iterators need to be primed, but UsdPrimSubtreeRange starts on a valid entry.
        private bool m_primed;

        // End is not strictly necessary, but saves quite a bit of overhead per MoveNext.
        private UsdPrimSubtreeIterator m_end;

        public SubtreeIterator(UsdPrimSubtreeRange range)
        {
            m_range = range;
            m_cur = range.GetStart();
            m_end = range.GetEnd();
            m_primed = false;
        }

        // ------------------------------------------------------------------------------------------ //
        // Enumerable
        // ------------------------------------------------------------------------------------------ //

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public IEnumerator<UsdPrim> GetEnumerator()
        {
            return this;
        }

        // ------------------------------------------------------------------------------------------ //
        // Enumerator
        // ------------------------------------------------------------------------------------------ //

        public virtual void Dispose()
        {
            if (m_cur != null) { m_cur.Dispose(); m_cur = null; }
            if (m_range != null) { m_range.Dispose(); m_range = null; }
        }

        public bool MoveNext()
        {
            if (!m_primed)
            {
                m_primed = true;
            }
            else
            {
                m_cur.MoveNext();
            }
            return m_cur != m_end;
        }

        public UsdPrim Current
        {
            get { return m_cur.GetCurrent(); }
        }

        object IEnumerator.Current
        {
            get { return m_cur.GetCurrent(); }
        }

        public void Reset()
        {
            m_cur = m_range.GetStart();
            m_primed = false;
        }
    }
}

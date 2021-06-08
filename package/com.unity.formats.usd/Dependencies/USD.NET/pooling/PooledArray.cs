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

using System;

namespace USD.NET
{
    /// <summary>
    /// Decorator to hold pooled arrays and follow the RIAA pattern.
    /// </summary>
    internal class PooledArray<T> : IDisposable
    {
        private ArrayPool m_allocator;
        private uint m_size;

        public PooledArray(ArrayPool allocator, uint size)
        {
            m_allocator = allocator;
            Resize(size);
        }

        public void Resize(uint size)
        {
            Free();
            m_size = size;
            Value = m_allocator.Malloc<T>(size);
        }

        public void Free()
        {
            if (Value == null) { return; }
            m_allocator.Free(typeof(T), m_size, Value);
            Value = null;
        }

        public T[] Value
        {
            get;
            private set;
        }

        // -------------------------------------------------------------------------------------------- //
        // Disposable object pattern (for managed objects).
        // -------------------------------------------------------------------------------------------- //

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                m_allocator.Free(typeof(T), m_size, Value);
            }

            disposed = true;
        }

        ~PooledArray()
        {
            Dispose(false);
        }
    }
}

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
    internal class PooledHandle<T> : IDisposable
    {
        private ArrayPool m_allocator;

        public PooledHandle(ArrayPool allocator)
        {
            m_allocator = allocator;
            Value = (T)m_allocator.MallocHandle(typeof(T));
        }

        public T Value { get; set; }

        // ------------------------------------------------------------------------------------------ //
        // Disposable object pattern (for managed objects).
        // ------------------------------------------------------------------------------------------ //

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
                m_allocator.FreeHandle(Value);
            }

            disposed = true;
        }

        ~PooledHandle()
        {
            Dispose(false);
        }
    }
}

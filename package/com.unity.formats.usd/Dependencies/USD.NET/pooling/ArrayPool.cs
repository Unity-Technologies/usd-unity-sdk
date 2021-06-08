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
using System.Collections.Generic;

namespace USD.NET
{
    /// <summary>
    /// A pooled memory allocator. May be used optionally during serializaiton to avoid generation of
    /// garbage.
    /// </summary>
    public class ArrayPool
    {
        protected static readonly Type[] sm_defaultCtor = new Type[0];
        protected static readonly object[] sm_noParameters = new object[0];

        private Dictionary<Type, Dictionary<uint, List<Array>>> m_data =
            new Dictionary<Type, Dictionary<uint, List<Array>>>();

        private Dictionary<Type, List<object>> m_hndData =
            new Dictionary<Type, List<object>>();

        /// <summary>
        /// Allocates a new array of type T, returning ownership to the caller. Uses an existing array
        /// from the pool if available.
        /// </summary>
        /// <typeparam name="T">The element type of the array</typeparam>
        /// <param name="size">The number of elements to allocate in the array</param>
        /// <returns></returns>
        virtual public T[] Malloc<T>(uint size)
        {
            if (typeof(T[]) == typeof(string[]))
            {
                return new T[size];
            }
            lock (this) {
                Dictionary<uint, List<Array>> pool;
                List<Array> vec;
                Type arrayType = typeof(T[]);

                if (!m_data.TryGetValue(arrayType, out pool))
                {
                    pool = new Dictionary<uint, List<Array>>();
                    m_data.Add(arrayType, pool);
                }

                if (!pool.TryGetValue(size, out vec))
                {
                    vec = new List<Array>();
                    pool.Add(size, vec);
                }

                if (vec.Count == 0)
                {
                    return new T[size];
                }
                else
                {
                    Array array = vec[vec.Count - 1];
                    vec.RemoveAt(vec.Count - 1);
                    return (T[])array;
                }
            }
        }

        /// <summary>
        /// Allocates a new object of the specified type, transferring ownership to the caller. If an
        /// an existing object is available in the pool, it will be reused.
        /// </summary>
        /// <remarks>
        /// This is primarily intended for use with wrapped, unmanaged objects (hence the name "handle")
        /// which tend to have a fixed overhead for every object instance.
        /// </remarks>
        virtual public object MallocHandle(Type type)
        {
            List<object> pool;

            lock (this) {
                if (!m_hndData.TryGetValue(type, out pool))
                {
                    pool = new List<object>();
                    m_hndData.Add(type, pool);
                }

                if (pool.Count == 0)
                {
                    return type.GetConstructor(sm_defaultCtor).Invoke(sm_noParameters);
                }
                else
                {
                    object array = pool[pool.Count - 1];
                    pool.RemoveAt(pool.Count - 1);
                    return array;
                }
            }
        }

        /// <summary>
        /// Adds the given handle to the allocator pool. Note that the handle need not have been
        /// allocated via malloc.
        /// </summary>
        /// <remarks>
        /// Note that objects returned to the allocator pool will not be garbage collected and will not
        /// be disposed.
        /// </remarks>
        virtual public void FreeHandle<T>(T handle)
        {
            FreeHandle(handle.GetType(), handle);
        }

        /// <summary>
        /// Adds the given handle to the allocator pool. Note that the handle need not have been
        /// allocated via malloc.
        /// </summary>
        /// <remarks>
        /// Note that objects returned to the allocator pool will not be garbage collected and will not
        /// be disposed.
        /// </remarks>
        virtual public void FreeHandle(Type type, object handle)
        {
            List<object> pool;

            lock (this) {
                if (!m_hndData.TryGetValue(type, out pool))
                {
                    pool = new List<object>();
                    m_hndData.Add(type, pool);
                }
                pool.Add(handle);
            }
        }

        /// <summary>
        /// Adds the given array to the allocator pool. Note that the array need not have been
        /// allocated via malloc.
        /// </summary>
        /// <remarks>
        /// Note that objects returned to the allocator pool will not be garbage collected and will not
        /// be disposed.
        /// </remarks>
        virtual public void Free(Type arrayType, uint size, Array array)
        {
            if (arrayType == typeof(string[]))
            {
                return;
            }
            Dictionary<uint, List<Array>> pool;
            List<Array> vec;

            lock (this) {
                if (!m_data.TryGetValue(arrayType, out pool))
                {
                    pool = new Dictionary<uint, List<Array>>();
                    m_data.Add(arrayType, pool);
                }

                if (!pool.TryGetValue(size, out vec))
                {
                    vec = new List<Array>();
                    pool.Add(size, vec);
                }

                vec.Add(array);
            }
        }
    }
}

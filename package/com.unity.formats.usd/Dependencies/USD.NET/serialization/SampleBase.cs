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
    /// The base class for all automatically serializable classes.
    /// </summary>
    [Serializable]
    public class SampleBase
    {
        // TODO: Should this class hold a reference to the allocator, in prep for when ArrayAllocator
        // is no longer global state?

        // TODO: convert this to Dispose pattern
        // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose

        /// <summary>
        /// Visits all arrays held by this object and returns them to the associated allocator.
        /// </summary>
        public void Free()
        {
            foreach (Array array in Reflect.ExtractArrays(this))
            {
                if (array.Rank != 1)
                {
                    throw new NotImplementedException("Multi-dimensional arrays are not supported");
                }
                UsdIo.ArrayAllocator.Free(array.GetType(), (uint)array.GetLength(0), array);
            }
        }
    }
}

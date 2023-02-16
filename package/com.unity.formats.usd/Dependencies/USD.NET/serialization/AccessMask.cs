// Copyright 2019 Jeremy Cowles. All rights reserved.
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
using System.Reflection;
using pxr;

namespace USD.NET
{
    /// <summary>
    /// An interface to enable storage of data which can be retrieved later.
    /// </summary>
    /// <remarks>
    /// Used, for example, to speed up the deserialization of animated prims. As we only load the dynamic properties per sample,
    /// if the downstream logic needs some static properties they can be stored temporarily as an IRestorableData in a DeserializationContext.
    /// </remarks>
    public interface IRestorableData
    {
    }

    /// <summary>
    /// This class holds the data required by the deserializer to optimize reading of primitive with dynamic properties. 
    /// </summary>
    /// <remarks>
    /// The cachedData can, for example, store data that is constant over time so that it is not read for each sample.
    /// </remarks>
    public class DeserializationContext
    {
        public HashSet<MemberInfo> dynamicMembers;
        public IRestorableData cachedData;

        public DeserializationContext()
        {
            dynamicMembers = new HashSet<MemberInfo>();
        }
    }

    /// <summary>
    /// Records what Prims and Attributes should be read over time.
    /// </summary>
    /// <remarks>
    /// Used, for example, when tracking which prims are animated. By adding the dynamic prims to an
    /// AccessMask, only the dynamic prims are loaded and downstream logic need not know why.
    /// </remarks>
    public class AccessMask
    {
        public Dictionary<SdfPath, DeserializationContext> Included = new Dictionary<SdfPath, DeserializationContext>();
    }
}

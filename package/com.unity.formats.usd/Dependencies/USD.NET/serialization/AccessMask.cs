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

using System;
using System.Collections.Generic;
using System.Reflection;
using pxr;

namespace USD.NET
{
    /// <summary>
    /// Records what Prims and Attributes should be read over time.
    /// </summary>
    /// <remarks>
    /// Used, for example, when tracking what prims are animated. By adding the dynamic prims to an
    /// AccessMask, only the dynamic prims are loaded and downstream logic need not know why.
    /// </remarks>
    public class AccessMask
    {
        public Dictionary<SdfPath, HashSet<MemberInfo>> Included = new Dictionary<SdfPath, HashSet<MemberInfo>>();
    }
}

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

using UnityEngine;

namespace Unity.Formats.USD
{
    /// <summary>
    /// Indicates that the source was an Assembly in USD. See Kind in USD for details:
    /// https://graphics.pixar.com/usd/docs/api/kind_page_front.html
    /// </summary>
    /// <remarks>
    /// An assembly is a model made of more models, for example a film set or a game level. It is
    /// often useful to have special processing behavior for assemblies, so this is actually less of
    /// a behavior and more of a marker or attribute on the GameObject.
    /// </remarks>
    public class UsdAssemblyRoot : MonoBehaviour
    {
    }
}

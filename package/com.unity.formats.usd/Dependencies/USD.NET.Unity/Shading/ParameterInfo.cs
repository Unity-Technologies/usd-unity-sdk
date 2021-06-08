// Copyright 2021 Unity Technologies. All rights reserved.
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

namespace USD.NET.Unity
{
    /// <summary>
    /// A class describing a single shader parameter, used to simplify reading values from USD.
    /// </summary>
    public struct ParameterInfo
    {
        /// <summary>
        /// The value to use when the parameter is not connected.
        /// </summary>
        public object value;

        /// <summary>
        /// A source connected to the parameter, null if not connected.
        /// Note that this path will always target an attribute, not the prim itself.
        /// </summary>
        public string connectedPath;

        /// <summary>
        /// The name of the parameter, as declared in the USD Sample class.
        /// </summary>
        public string usdName;

        /// <summary>
        /// The name of the parameter, as declared in the Unity shader source file.
        /// </summary>
        public string unityName;

        /// <summary>
        /// Some shaders require enabling keywords to enable features, this is the list of required
        /// keywords associated with this parameter.
        /// </summary>
        public string[] requiredShaderKeywords;

        public override string ToString()
        {
            return usdName + " (" + unityName + ") "
                + "<" + connectedPath + "> "
                + (value != null ? value.GetType().ToString() : "null");
        }
    }
}

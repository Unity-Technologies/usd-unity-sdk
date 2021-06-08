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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace USD.NET.Unity
{
    /// <summary>
    /// A base class for all shader samples.
    /// </summary>
    [System.Serializable]
    [UsdSchema("Shader")]
    public class ShaderSample : SampleBase
    {
        // The attribute "info:id" is required by UsdShadeShader.
        [UsdNamespace("info")]
        public pxr.TfToken id;

        // ------------------------------------------------------------------------------------------ //
        // Helper functions.
        // ------------------------------------------------------------------------------------------ //

        #region "Private Helpers"
        private System.Type GetClassType()
        {
            return this.GetType();
        }

        private object GetValue(FieldInfo info)
        {
            return info.GetValue(this);
        }

        #endregion

        public IEnumerable<ParameterInfo> GetInputParameters()
        {
            var inputParamType = typeof(InputParameterAttribute);
            var flags = BindingFlags.Public | BindingFlags.Instance;
            foreach (var info in GetClassType().GetFields(flags).Where(
                (info) => System.Attribute.IsDefined(info, inputParamType)))
            {
                var param = new ParameterInfo();
                var conn = (Connectable)(GetValue(info));
                var pi = (InputParameterAttribute)info.GetCustomAttributes(inputParamType, inherit: true)[0];
                param.value = conn.GetValue();
                param.connectedPath = conn.GetConnectedPath();
                param.usdName = info.Name;
                param.unityName = pi.UnityName;
                yield return param;
            }
        }

        public IEnumerable<ParameterInfo> GetInputTextures()
        {
            var inputParamType = typeof(InputTextureAttribute);
            var requireKeywordType = typeof(RequireShaderKeywordsAttribute);
            var flags = BindingFlags.Public | BindingFlags.Instance;
            foreach (var info in GetClassType().GetFields(flags).Where(
                (info) => System.Attribute.IsDefined(info, inputParamType)))
            {
                var param = new ParameterInfo();
                var conn = (Connectable)(GetValue(info));
                var pi = (InputTextureAttribute)info.GetCustomAttributes(inputParamType, inherit: true)[0];
                param.value = conn.GetValue();
                param.connectedPath = conn.GetConnectedPath();
                param.usdName = info.Name;
                param.unityName = pi.UnityName;

                if (System.Attribute.IsDefined(info, requireKeywordType))
                {
                    var rk = (RequireShaderKeywordsAttribute)info.GetCustomAttributes(requireKeywordType, inherit: true)[0];
                    param.requiredShaderKeywords = rk.Keywords;
                }
                else
                {
                    param.requiredShaderKeywords = new string[0];
                }

                yield return param;
            }
        }
    }
}

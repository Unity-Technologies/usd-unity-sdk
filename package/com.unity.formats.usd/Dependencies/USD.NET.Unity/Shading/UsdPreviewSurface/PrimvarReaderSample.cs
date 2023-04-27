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

using UnityEngine;

namespace USD.NET.Unity
{
    /// <summary>
    /// The following is based on the Pixar specification found here:
    /// https://graphics.pixar.com/usd/docs/UsdPreviewSurface-Proposal.html
    /// </summary>
    [System.Serializable]
    [UsdSchema("Shader")]
    public class PrimvarReaderSample<T> : ShaderSample where T : struct
    {
        public PrimvarReaderSample()
        {
            if (typeof(T) == typeof(float))
            {
                id = new pxr.TfToken("UsdPrimvarReader_float");
            }
            else if (typeof(T) == typeof(Vector2))
            {
                id = new pxr.TfToken("UsdPrimvarReader_float2");
            }
            else if (typeof(T) == typeof(Vector3))
            {
                id = new pxr.TfToken("UsdPrimvarReader_float3");
            }
            else if (typeof(T) == typeof(Vector4))
            {
                id = new pxr.TfToken("UsdPrimvarReader_float4");
            }
            else if (typeof(T) == typeof(int))
            {
                id = new pxr.TfToken("UsdPrimvarReader_int");
            }
            else if (typeof(T) == typeof(string))
            {
                id = new pxr.TfToken("UsdPrimvarReader_string");

                // TODO(jcowles): the "normal" type aliases to Vector3 in Unity.
                //} else if (typeof(T) == typeof(Vector3)) {
                //  id = new pxr.TfToken("UsdPrimvarReader_normal");
            }
            else if (typeof(T) == typeof(Matrix4x4))
            {
                id = new pxr.TfToken("UsdPrimvarReader_matrix");
            }
            else
            {
                throw new System.ArgumentException("Invalid template type: " + typeof(T).Name);
            }
        }

        /// <summary>
        /// Name of the primvar to be read from the primitive.
        /// </summary>
        [InputParameter("_Fallback")]
        public Connectable<T> fallback = new Connectable<T>();

        public class Outputs : SampleBase
        {
            public T? result;
        }

        [UsdNamespace("outputs")]
        public Outputs outputs = new Outputs();
    }

    // Workaround so that exported shaders fit the USD spec for 20.08, as that's the version included in the package.
    // In 21.11+ the varname was switched to a string, and this should be removed when we upgrade our USD version.
    [System.Serializable]
    [UsdSchema("Shader")]
    public class PrimvarReaderExportSample<T> : PrimvarReaderSample<T> where T : struct
    {
        /// <summary>
        /// Name of the primvar to be read from the primitive as a TfToken.
        /// </summary>
        [InputParameter("_Varname")]
        public Connectable<pxr.TfToken> varname = new Connectable<pxr.TfToken>();
    }

    [System.Serializable]
    [UsdSchema("Shader")]
    public class PrimvarReaderImportSample<T> : PrimvarReaderSample<T> where T : struct
    {
        /// <summary>
        /// Name of the primvar to be read from the primitive as a TfToken.
        /// </summary>
        /// <remarks>
        /// In 21.11 the varname was switched from a TfToken to a string. However, as a TfToken can be implicitly converted to a string,
        /// making this a string type works for both during *import*. For export, we have chosen to export as TfToken to align with
        /// the version of USD this package is built against (see PrimvarReaderSampleExport override, below)
        /// </remarks>
        [InputParameter("_Varname")]
        public Connectable<string> varname = new Connectable<string>("st");
    }
}

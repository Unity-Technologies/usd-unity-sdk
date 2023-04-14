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
    /// https://graphics.pixar.com/usd/release/spec_usdpreviewsurface.html#transform2d
    /// </summary>
    [System.Serializable]
    [UsdSchema("Shader")]
    public class UsdTransform2dSample : ShaderSample
    {
        public UsdTransform2dSample()
        {
            id = new pxr.TfToken("UsdTransform2d");
        }

        /// <summary>
        /// Name of the primvar to be read from the primitive.
        /// </summary>
        [InputParameter("in")]
        public Connectable<pxr.TfToken> @in = new Connectable<pxr.TfToken>();

        [InputParameter("_Scale")]
        public Connectable<Vector2> scale = new Connectable<Vector2>(new Vector2(1.0f, 1.0f));

        [InputParameter("_Translation")]
        public Connectable<Vector2> translation = new Connectable<Vector2>(new Vector2(0.0f, 0.0f));

        [InputParameter("_Rotation")]
        public Connectable<float> rotation = new Connectable<float>(0.0f);

        public class Outputs : SampleBase
        {
            public Vector2? result;
        }

        [UsdNamespace("outputs")]
        public Outputs outputs = new Outputs();
    }
}

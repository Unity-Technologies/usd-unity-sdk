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
    [System.Serializable]
    [UsdSchema("UsdGeomXformable")]
    public class XformableSample : ImageableSample
    {
        private readonly string[] kXformOpTransform = new string[] { "xformOp:transform" };
        private Matrix4x4 m_xf;

        public static XformableSample FromTransform(UnityEngine.Transform transform)
        {
            var xf = new XformSample();
            var mx = Matrix4x4.TRS(transform.localPosition,
                transform.localRotation,
                transform.localScale);
            xf.transform = mx;
            return xf;
        }

        public XformableSample() : base()
        {
            transform = Matrix4x4.identity;
        }

        [UsdNamespace("xformOp"), FusedTransform]
        public Matrix4x4 transform
        {
            get { return m_xf; }
            set
            {
                if (value == null)
                {
                    xformOpOrder = null;
                }
                else
                {
                    xformOpOrder = kXformOpTransform;
                }
                m_xf = value;
            }
        }

        // Ideally this would be private, but it needs to be serialized.
        [UsdVariability(Variability.Uniform)]
        public string[] xformOpOrder;

        /// <summary>
        /// Converts the transform from Unity to USD or vice versa. This is required after reading
        /// values from USD or before writing values to USD.
        /// </summary>
        public void ConvertTransform()
        {
            m_xf = UnityTypeConverter.ChangeBasis(m_xf);
        }
    }
}

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

using System.Collections.Generic;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public class UnityMaterialData : SampleBase
    {
        public string shaderName;
        public string[] shaderKeywords;

        [UsdNamespace("floats")] public Dictionary<string, float> floatArgs = new Dictionary<string, float>();

        [UsdNamespace("colors")] public Dictionary<string, Color> colorArgs = new Dictionary<string, Color>();

        [UsdNamespace("vectors")] public Dictionary<string, Vector4> vectorArgs = new Dictionary<string, Vector4>();
    }

    public class UnityPreviewSurfaceSample : PreviewSurfaceSample
    {
        [UsdNamespace("unity")] public UnityMaterialData unity = new UnityMaterialData();
    }
}

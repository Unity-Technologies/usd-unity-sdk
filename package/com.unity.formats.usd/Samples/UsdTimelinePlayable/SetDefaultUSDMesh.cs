// Copyright 2023 Unity Technologies. All rights reserved.
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
using UnityEngine;
using System.IO;
using USD.NET;

namespace Unity.Formats.USD.Examples
{
    /// <summary>Fill in a default USD Asset in case the user didn't choose one
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(UsdAsset))]
    public class SetDefaultUSDMesh : MonoBehaviour
    {
        private const string K_DEFAULT_MESH = "TimelinePlayableUsd.usd";

        private void Awake()
        {
            SetSampleUsdFilePath();
        }

        void Update()
        {
            SetSampleUsdFilePath();
        }

        private void SetSampleUsdFilePath()
        {
            var sampleUsdAsset = this.GetComponent<UsdAsset>();
            sampleUsdAsset.usdFullPath = Path.Combine(PackageUtils.GetCallerRelativeToProjectFolderPath(), K_DEFAULT_MESH);
        }
    }
}

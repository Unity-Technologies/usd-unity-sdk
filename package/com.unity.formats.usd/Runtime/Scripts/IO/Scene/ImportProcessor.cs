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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USD.NET;
using pxr;

namespace Unity.Formats.USD
{
    /// <summary>
    /// Called by SceneImporter after Usd Scene is loaded, but before any instantiation or recreation in Unity
    /// </summary>
    public interface IImportProcessUsd
    {
        void ProcessUsd(Scene scene, SceneImportOptions sceneImportOptions);
    }

    /// <summary>
    /// Called by SceneImporter after GameObject hierarchy created, but before it is populated with geometry and other components
    /// </summary>
    public interface IImportPostProcessHierarchy
    {
        void PostProcessHierarchy(PrimMap primMap, SceneImportOptions sceneImportOptions);
    }

    /// <summary>
    /// Called by SceneImporter after GameObject hierarchy created and populated with geometry and other components
    /// </summary>
    public interface IImportPostProcessComponents
    {
        void PostProcessComponents(PrimMap primMap, SceneImportOptions sceneImportOptions);
    }
}

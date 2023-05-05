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
    /// <summary>
    /// Imports meshes and transforms from a USD file into the Unity GameObject hierarchy and
    /// creates meshes.
    /// </summary>
    public class ImportMeshExample : MonoBehaviour
    {
        [HideInInspector]
        public string m_usdFile = null;
        private const string K_DEFAULT_MESH = "mesh.usd";

        [HeaderAttribute("Import Settings:")]
        public Material m_material;

        [Tooltip("Typically USD data is right-handed and Unity is left handed, this option indicates how that conversion should be handled.")]
        public BasisTransformation m_changeHandedness = BasisTransformation.SlowAndSafe;

        [Tooltip("Enable GPU instancing on materials for USD point or scene instances.")]
        public bool m_enableGpuInstancing = false;

        private Scene m_scene;

        public void InitializeUsd()
        {
            InitUsd.Initialize();
        }

        public string GetUsdFilePath()
        {
            if (string.IsNullOrEmpty(m_usdFile) || !Directory.Exists(m_usdFile))
                m_usdFile = Path.Combine(PackageUtils.GetCallerRelativeToProjectFolderPath(), K_DEFAULT_MESH);

            return m_usdFile;
        }

        public void ImportUsdFile()
        {
            // Import the new scene.
            m_scene = Scene.Open(m_usdFile);
            if (m_scene == null)
            {
                throw new Exception("Failed to import");
            }

            // When converting right handed (USD) to left handed (Unity), there are four options:
            //
            //  FastWithNegativeScale:
            //      Apply a single negative scale and rotation at the root of the scene hierarchy.
            //      Fastest, but may introduce additional complications when later exporting.
            //
            //  SlowAndSafe:
            //      Transform to left-handed basis by processing all positions, triangles, transforms, and primvar data.
            //      While slower, this is the safest option.
            //
            //  SlowAndSafeAsFBX:
            //      Transform to left-handed basis by processing all positions, triangles, transforms, and primvar data.
            //      It transforms to match the basis transformation of FBX which is from (X,Y,Z) to (-X,Y,Z) instead of the standard (SlowAndSafe option) (X,Y,Z) to (X,Y,-Z).
            //      This is not a conventional behavior and this option is here only to allow consistency between geometry importers.
            //
            //  None:
            //      Preform no transformation.
            //      Should only be used when the USD file is known to contain data which was (non-portably) stored in a left-handed basis.
            //
            // SlowAndSafe is more computationally expensive, but results in fewer down stream surprises - It is set to be our default.
            var importOptions = new SceneImportOptions();
            importOptions.changeHandedness = m_changeHandedness;
            importOptions.materialMap.DisplayColorMaterial = m_material;
            importOptions.enableGpuInstancing = m_enableGpuInstancing;

            // The root object at which the USD scene will be reconstructed.
            // It may need a Z-up to Y-up conversion and a right- to left-handed change of basis.
            var rootXf = new GameObject("ImportedMesh_GameObject");
            SceneImporter.BuildScene(m_scene,
                rootXf,
                importOptions,
                new PrimMap(),
                composingSubtree: false);

            // Ensure the file and the identifier match.
            m_usdFile = m_scene.FilePath;
        }
    }
}

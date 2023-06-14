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

namespace Unity.Formats.USD
{
    public enum PayloadPolicy
    {
        DontLoadPayloads,
        LoadAll,
    }

    /// <summary>
    /// How to transform from the USD basis (typically right-handed) to Unity (left-handed).
    /// </summary>
    public enum BasisTransformation
    {
        /// <summary>
        /// Apply a single negative scale and rotation at the root of the scene hierarchy.
        /// Fastest, but may introduce additional complications when later exporting.
        /// </summary>
        FastWithNegativeScale,

        /// <summary>
        /// Transform to left-handed basis by processing all positions, triangles, transforms, and
        /// primvar data. While slower, this is the safest option.
        /// </summary>
        SlowAndSafe,

        /// <summary>
        /// Transform to left-handed basis by processing all positions, triangles, transforms, and
        /// primvar data. It transforms to match the basis transformation of FBX which is from (X,Y,Z) to (-X,Y,Z)
        /// instead of the standard (SlowAndSafe option) (X,Y,Z) to (X,Y,-Z).
        /// This is not a conventional behavior and this option is here only to allow consistency between
        /// geometry importers.
        /// </summary>
        SlowAndSafeAsFBX,

        /// <summary>
        /// Preform no transformation; should only be used when the USD file is known to contain
        /// data which was (non-portably) stored in a left-handed basis.
        /// </summary>
        None,
    }

    /// <summary>
    /// Indicates how to handle import materials.
    /// </summary>
    [System.Serializable]
    public enum MaterialImportMode
    {
        /// <summary>
        /// Fully imports the material, parameters, and textures (may be slow).
        /// </summary>
        ImportPreviewSurface,

        /// <summary>
        /// Ignores the bound material and only uses the object's displayColor.
        /// </summary>
        ImportDisplayColor,

        /// <summary>
        /// Do not assign materials to imported objects.
        /// </summary>
        None,
    }

    /// <summary>
    /// Indicates how the scene should be imported from USD to Unity.
    /// </summary>
    [System.Serializable]
    public class SceneImportOptions
    {
        /// <summary>
        /// The root location in the Unity project where imported files will be created.
        /// </summary>
        public string projectAssetPath = "Assets/";

        /// <summary>
        /// The path in the USD file at which to import/export.
        /// </summary>
        public pxr.SdfPath usdRootPath = pxr.SdfPath.AbsoluteRootPath();

        /// <summary>
        /// Force components and objects to be rebuilt, instead of reusing existing objects.
        /// </summary>
        public bool forceRebuild = false;

        public PayloadPolicy payloadPolicy = PayloadPolicy.DontLoadPayloads;

        public bool importHierarchy = true;
        public bool importCameras = true;
        public bool importMeshes = true;
        public bool importSkinning = true;
        public bool importSkinWeights = true;
        public bool importTransforms = true;
        public bool importSceneInstances = true;
        public bool importPointInstances = true;
        public bool importMonoBehaviours = false;

        /// <summary>
        /// Typically USD data is right-handed and Unity is left handed; this option indicates how
        /// that conversion should be handled.
        /// </summary>
        public BasisTransformation changeHandedness = BasisTransformation.SlowAndSafe;

        /// <summary>
        /// A uniform scale to apply to the entire imported scene.
        /// Note that this scale is baked into every object, which is required for the Unity skinned
        /// mesh renderer, since it uses a parent-relative mesh baking scheme.
        /// </summary>
        public float scale = 1.0f;

        /// <summary>
        /// Apply linear interpolation when requesting values between time samples.
        /// </summary>
        public bool interpolate = true;

        /// <summary>
        /// Enable GPU instancing on materials for point and scene instances. Note this may negatively
        /// impact framerate in some cases.
        /// </summary>
        public bool enableGpuInstancing = false;

        /// <summary>
        /// When no material is bound, setup a default material to consume the display color.
        /// If the object color is not imported, a default white material will be assigned.
        /// </summary>
        public bool useDisplayColorAsFallbackMaterial = true;

        /// <summary>
        /// Indicates how materials are handled, see enum for details.
        /// </summary>
        public MaterialImportMode materialImportMode = MaterialImportMode.ImportDisplayColor;

        /// <summary>
        /// A set of registered mappings from USD shader ID to Unity material.
        /// </summary>
        [System.NonSerialized] public MaterialMap materialMap = new MaterialMap();

        /// <summary>
        /// The default options for how to import meshes.
        /// </summary>
        public MeshImportOptions meshOptions = new MeshImportOptions();

        /// <summary>
        /// Indicates if the importer should attempt to bind materials.
        /// </summary>
        public bool ShouldBindMaterials
        {
            get { return materialImportMode == MaterialImportMode.ImportPreviewSurface; }
        }

        /// <summary>
        /// Indicates what type of import is being performed.
        /// </summary>
        /// <remarks>
        /// This is used to distinguish different analytics.
        /// </remarks>
        public ImportType ImportType { get; set; } = ImportType.Initial;
    }
}

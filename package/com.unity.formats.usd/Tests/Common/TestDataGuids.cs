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

namespace Unity.Formats.USD.Tests
{
    public struct TestDataGuids
    {
        public struct PrimType
        {
            public const string CollectionsUsda = "5f0268198d3d7484cb1877bec2c5d31f"; // Tests/Common/Data/PrimType/test_collections.usda
            public const string ComponentPayloadUsda = "9b90106787621ca46b666d6e9b483d7a"; // Tests/Common/Data/PrimType/test_component.payload.usda
        }

        public struct Material
        {
            public const string SimpleMaterialUsd = "c06c7eba08022b74ca49dce5f79ef3ba"; // Tests/Common/Data/Material/simpleMaterialTest.usd
            public const string TexturedOpaqueUsda = "7df73e3eb8d6696408938e54bb9af792"; // Tests/Common/Data/Material/Textured/TexturedOpaque.usda
            public const string TexturedTransparentCutoutUsda = "59eedccadbb71c943819e1ef8ed94343"; // Tests/Common/Data/Material/Textured/TexturedTransparent_Cutout.usda.usda
        }

        public struct Variability
        {
            // Fully defined prims
            public const string CubesUsd = "a4a575275d6254853ac93b867a1a8471"; // Tests/Common/Data/Variability/cubes_variability_test.usd

            // Prims with references
            public const string ReferencedCubesUsd = "b66c9dc38fc1e0044b3d93a196ad1365"; // Tests/Common/Data/Variability/referenced_cubes_variability_test.usd
        }

        public struct Animation
        {
            public const string SkinnedCharacterPrefab = "7c508a5d4c9764686a6bcd60561eb2f2"; // Tests/Editor/Data/SkinnedCharacter/Player/Player.prefab
        }

        public struct Mesh
        {
            public const string SkinnedMeshUsda = "3d00d71254d14bdda401019eb84373ce"; // Tests/Common/Data/Mesh/ImportSkinnedMesh.usda
        }

        public struct CameraRelated
        {
            public const string CameraIncludedFbx = "86a597c63449d2541b7587ff90e75d91"; // Tests/Common/Data/CameraRelated/WithCamera/withCamera.fbx
            public const string PhysicalCameraUsda = "6aa58f080f5cc0542989c8ff7737bdc3"; // Tests/Common/Data/CameraRelated/physicalCam.usda
        }

        public struct Simple
        {
            public const string SimpleUsda = "68d552f46d3740c47b17d0ac1c531e76"; // Tests/Common/Data/Simple/reloadTest.usda
            public const string SimpleModifiedUsda = "4eccf405e5254fd4089cef2f9bcbd882"; // Tests/Common/Data/Simple/reloadTest_modified.usda
            public const string SimpleOriginUsda = "069ae5d2d8a36fd4b8a0395de731eda0"; // Tests/Common/Data/Simple/reloadTest_origin.usda
        }

        public struct VariedCollection
        {
            public const string AttributeScopeUsda = "b2bf96b7908c0a74682f7e92cb5ba652"; // Tests/Common/Data/VariedCollection/attribute_scope_test_data.usda
        }

        public struct Instancer
        {
            public const string PointInstancedUsda = "bfb4012f0c339574296e64f4d3c6c595"; // Tests/Common/Data/Instancer/point_instanced_cubes.usda
            public const string MeshInstancesUsd = "c5046ed8700c010469c4c557353bc241"; // Tests/Common/Data/Instancer/mesh_instances.usd
            public const string UpAxisYLeftHandedUsda = "8eb3de471e929a44589a4574170a6f28"; // Tests/Common/Data/Instancer/UsdInstance_UpAxisY_LeftHanded.usda
            public const string UpAxisYRightHandedUsda = "57af6f9bc59a14040bf024350e7630e1"; // Tests/Common/Data/Instancer/UsdInstance_UpAxisY_RightHanded.usda
            public const string UpAxisZLeftHandedUsda = "a01f0befc190ba34e9b0a5405f145a86"; // Tests/Common/Data/Instancer/UsdInstance_UpAxisZ_LeftHanded.usda
            public const string UpAxisZRightHandedUsda = "051b4a1cb85f6024191275df09e12fc2"; // Tests/Common/Data/Instancer/UsdInstance_UpAxisZ_RightHanded.usda
        }
    }
}

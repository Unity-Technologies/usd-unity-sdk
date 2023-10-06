// Copyright 2022 Unity Technologies. All rights reserved.
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

using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD.Tests
{
    public class USDExportTests : BaseFixtureEditor
    {
        private Scene m_USDScene;
        private string m_USDScenePath;

        [SetUp]
        public void SetUp()
        {
            m_USDScenePath = TestUtility.GetUSDScenePath(ArtifactsDirectoryRelativePath, "USDExportTests");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_USDScene != null)
            {
                m_USDScene.Close();
            }
        }

        [Test]
        public void ExportRootGameObjectWithMesh_ExportedPrimHasMeshType()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ExportHelpers.ExportGameObjects(new[] { cube }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);

            var cubePrim = TestUtility.GetGameObjectPrimInScene(m_USDScene, cube);
            Assert.IsNotNull(cubePrim);
            Assert.IsTrue(cubePrim.IsValid());

            var cubeType = cubePrim.GetTypeName();
            var expectedCubeType = new pxr.TfToken("Mesh");
            Assert.AreEqual(expectedCubeType, cubeType);
        }

        [Test]
        public void ExportMultipleRootGameObjectsWithSameName_AllGameObjectsHaveCorrespondingPrims()
        {
            var cubes = new GameObject[] { GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube) };
            cubes[0].name = "Cube_1"; // Name deduplication appends _GetSiblingIndex() and in this case cubes[1] needs 2 iterations
            cubes[1].name = "Cube";
            cubes[2].name = "Cube";

            ExportHelpers.ExportGameObjects(cubes, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);
            var exportedPrims = new HashSet<pxr.UsdPrim>();
            foreach (GameObject cube in cubes)
            {
                var cubePrim = TestUtility.GetGameObjectPrimInScene(m_USDScene, cube);
                Assert.IsNotNull(cubePrim, $"GameObject {cube.name} doesn't have a corresponding Prim");
                Assert.IsTrue(cubePrim.IsValid(), $"GameObject {cube.name} has invalid corresponding Prim");

                exportedPrims.Add(cubePrim);
            }
            Assert.AreEqual(cubes.Length, exportedPrims.Count, "One or more GameObjects don't have a corresponding Prim");
        }

        [Test]
        public void ExportMultipleSiblingGameObjectsWithSameName_AllGameObjectsHaveCorrespondingPrims()
        {
            var cubes = new GameObject[] { GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube) };
            cubes[0].name = "Cube_1"; // Name deduplication appends _GetSiblingIndex() and in this case cubes[1] needs 2 iterations
            cubes[1].name = "Cube";
            cubes[2].name = "Cube";

            var parent = new GameObject("parent");
            foreach (var cube in cubes)
            {
                cube.transform.parent = parent.transform;
            }

            ExportHelpers.ExportGameObjects(new GameObject[] { parent }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);
            var exportedPrims = new HashSet<pxr.UsdPrim>();
            foreach (GameObject cube in cubes)
            {
                var cubePrim = TestUtility.GetGameObjectPrimInScene(m_USDScene, cube);
                Assert.IsNotNull(cubePrim, $"GameObject {cube.name} doesn't have a corresponding Prim");
                Assert.IsTrue(cubePrim.IsValid(), $"GameObject {cube.name} has invalid corresponding Prim");

                exportedPrims.Add(cubePrim);
            }
            Assert.AreEqual(cubes.Length, exportedPrims.Count, "One or more GameObjects don't have a corresponding Prim");
        }

        // TODO: This test will need to be updated in later versions of the package where the UsdGeomPrimvarsAPI is implemented
        [Test]
        public void ExportGameObjectWithMesh_STPrimVarInterpolationSetToVarying()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "cube";

            ExportHelpers.ExportGameObjects(new GameObject[] { cube }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);
            pxr.UsdPrim cubePrim = TestUtility.GetGameObjectPrimInScene(m_USDScene, cube);

            pxr.UsdGeomMesh usdGeomMesh = new pxr.UsdGeomMesh(cubePrim);
            var stPrimvar = usdGeomMesh.GetPrimvar(new pxr.TfToken("st"));
            Assert.IsNotNull(stPrimvar, $"Mesh {cube.name} has no 'st' primvar.");
            Assert.AreEqual(pxr.UsdGeomTokens.varying, stPrimvar.GetInterpolation(), $"st on mesh {cube.name} is not set to varying interpolation.");
        }

        [Test]
        [Ignore("USDU-245")]
        public void ExportObjectWithEditorOnlyTag_DoesNotExportEditorOnly()
        {
            const string editorOnly = "EditorOnly";

            var rootObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var defaultChild = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            defaultChild.transform.SetParent(rootObject.transform);

            var editorOnlyChild = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            editorOnlyChild.name = "EditorOnlyTag_Object";
            editorOnlyChild.tag = editorOnly;
            editorOnlyChild.transform.SetParent(rootObject.transform);

            ExportHelpers.ExportGameObjects(new GameObject[] { rootObject }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);
            m_USDScene = Scene.Open(m_USDScenePath);

            Assert.IsNotNull(TestUtility.GetGameObjectPrimInScene(m_USDScene, defaultChild), $"GameObject without Tag '{editorOnly}' should have been exported");
            Assert.IsNull(TestUtility.GetGameObjectPrimInScene(m_USDScene, editorOnlyChild), $"GameObject <{editorOnlyChild.name}> with Tag '{editorOnly}' Shouldn't have been exported");
        }

        [Test]
        public void ExportSkeletalAnimation_ExportedPrimsHaveSkeletalTypes()
        {
            // "Packages/com.unity.formats.usd/Tests/Editor/Data/SkinnedCharacter/Player/Player.prefab"
            // When running the tests on CI usd package paths get changed and the Prefab can't be loaded by path
            var animationPrefabGUID = TestDataGuids.Animation.SkinnedCharacterPrefab;
            var animationPrefabPath = AssetDatabase.GUIDToAssetPath(animationPrefabGUID);
            var animationPrefab = AssetDatabase.LoadMainAssetAtPath(animationPrefabPath) as GameObject;
            var skeletalRoot = GameObject.Instantiate(animationPrefab) as GameObject;
            Assert.IsTrue(skeletalRoot != null, "Failed to find Skeletal root GameObject");

            var skeleton = GameObject.Find("Skeleton");
            Assert.IsTrue(skeleton != null, "Failed to find Skeleton GameObject");

            var mesh = GameObject.Find("Mesh");
            Assert.IsTrue(mesh != null, "Failed to find Mesh GameObject");
            ExportHelpers.ExportGameObjects(new GameObject[] { skeletalRoot }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);

            var skeletalRootInUsd = m_USDScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(skeletalRoot.transform)));
            var skeletalRootInUsdType = skeletalRootInUsd.GetTypeName();
            var expectedSkeletalRootInUsdType = new pxr.TfToken("SkelRoot");
            Assert.AreEqual(expectedSkeletalRootInUsdType, skeletalRootInUsdType);

            var skeletonInUsd = m_USDScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(skeleton.transform)));
            var skeletonInUsdType = skeletonInUsd.GetTypeName();
            var expectedSkeletonInUsdType = new pxr.TfToken("Skeleton");
            Assert.AreEqual(expectedSkeletonInUsdType, skeletonInUsdType);

            // check for SkelBindingAPI/MaterialBindingAPI
            var primInUsd = m_USDScene.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(mesh.transform)));
            var skelBindingAPIType = pxr.TfType.FindByName("UsdSkelBindingAPI");
            Assert.IsTrue(!skelBindingAPIType.IsUnknown(), "SkelBindingAPI TfType not found");
            Assert.IsTrue(primInUsd.HasAPI(skelBindingAPIType), "Prim does not have skel binding API applied to it");
            var matBindingAPIType = pxr.TfType.FindByName("UsdShadeMaterialBindingAPI");
            Assert.IsTrue(!matBindingAPIType.IsUnknown(), "MaterialBindingAPI TfType not found");
            Assert.IsTrue(primInUsd.HasAPI(matBindingAPIType), "Prim does not have material binding API applied to it");
        }

        [Test]
        public void ExportPrefabFromProjectWindow_ExportsWithoutErrors()
        {
            // "Packages/com.unity.formats.usd/Tests/Editor/Data/Prefabs/TestPrefab.prefab"
            // When running the tests on CI usd package paths get changed and the Prefab can't be loaded by path
            var testPrefabGUID = "90633c08c7035c74aab77931779245bd";
            var testPrefabPath = AssetDatabase.GUIDToAssetPath(testPrefabGUID);
            var testPrefabGO = AssetDatabase.LoadMainAssetAtPath(testPrefabPath) as GameObject;

            // Export without instantiating into the Scene
            ExportHelpers.ExportGameObjects(new GameObject[] { testPrefabGO }, ExportHelpers.InitForSave(m_USDScenePath), BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);

            var rootTestPrefabPrim = m_USDScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(testPrefabGO.transform)));
            var rootTestPrefabPrimType = rootTestPrefabPrim.GetTypeName();
            var expectedRootInUsdType = new pxr.TfToken("Mesh");
            Assert.AreEqual(expectedRootInUsdType, rootTestPrefabPrimType);


            var childTestPrefabPrim = m_USDScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(testPrefabGO.transform.GetChild(0))));
            var childTestPrefabPrimType = childTestPrefabPrim.GetTypeName();
            var expectedChildInUsdType = new pxr.TfToken("Mesh");
            Assert.AreEqual(expectedChildInUsdType, childTestPrefabPrimType);
        }
    }
}

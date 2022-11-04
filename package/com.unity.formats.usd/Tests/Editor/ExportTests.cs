using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

using USDScene = USD.NET.Scene;
using UnityScene = UnityEngine.SceneManagement.Scene;
using NUnit.Framework.Constraints;
using USD.NET.Unity;

namespace Unity.Formats.USD.Tests
{
    public class USDExportTests : BaseFixture
    {
        private string m_USDFilePath;
        private USDScene m_USDScene;
        private UnityScene m_UnityScene;

        [SetUp]
        public void SetUp()
        {
            m_UnityScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            m_USDFilePath = CreateTmpUsdFile("USDExportTests.usda");
            m_USDScene = USDScene.Create(m_USDFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            m_UnityScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            if (m_USDScene != null)
            {
                m_USDScene.Close();
            }
        }

        private pxr.UsdPrim GetPrim(GameObject gameObject)
        {
            Assert.IsNotNull(m_USDScene);
            Assert.IsNotNull(m_USDScene.Stage);

            return m_USDScene.Stage.GetPrimAtPath(new pxr.SdfPath(UnityTypeConverter.GetPath(gameObject.transform)));
        }


        [Test]
        public void ExportRootGameObjectWithMesh_ExportedPrimHasMeshType()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ExportHelpers.ExportGameObjects(new[] { cube }, m_USDScene, BasisTransformation.SlowAndSafe);

            m_USDScene = USDScene.Open(m_USDFilePath);

            var cubePrim = GetPrim(cube);
            Assert.IsNotNull(cubePrim);
            Assert.IsTrue(cubePrim.IsValid());

            var cubeType = cubePrim.GetTypeName();
            var expectedCubeType = new pxr.TfToken("Mesh");
            Assert.AreEqual(expectedCubeType, cubeType);
        }

        [Test]
        public void ExportMultipleRootGameObjectsWithSameName_AllGameObjecsHaveCorrespondingPrims()
        {
            var cubes = new GameObject[] { GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube) };
            foreach (GameObject cube in cubes)
            {
                cube.name = "Cube";
            }
            ExportHelpers.ExportGameObjects(cubes, m_USDScene, BasisTransformation.SlowAndSafe);

            m_USDScene = USDScene.Open(m_USDFilePath);
            foreach (GameObject cube in cubes)
            {
                var cubePrim = GetPrim(cube);
                Assert.IsNotNull(cubePrim);
                Assert.IsTrue(cubePrim.IsValid());
            }
        }

        [Test]
        public void ExportMultipleSiblingGameObjectsWithSameName_AllGameObjecsHaveCorrespondingPrims()
        {
            var parent = new GameObject("parent");
            var cubes = new GameObject[] { GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube), GameObject.CreatePrimitive(PrimitiveType.Cube) };
            foreach (GameObject cube in cubes)
            {
                cube.name = "Cube";
                cube.transform.parent = parent.transform;
            }
            ExportHelpers.ExportGameObjects(new GameObject[] { parent }, m_USDScene, BasisTransformation.SlowAndSafe);

            m_USDScene = USDScene.Open(m_USDFilePath);
            foreach (GameObject cube in cubes)
            {
                var cubePrim = GetPrim(cube);
                Assert.IsNotNull(cubePrim);
                Assert.IsTrue(cubePrim.IsValid());
            }
        }
    }
}

using System.IO;
using System.Linq;
using NUnit.Framework;
using pxr;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using USD.NET;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Formats.USD.Tests
{
    public class UsdPrimTypeTest_Scope
    {
        private GameObject m_usdRoot;
        private string m_usdGUID = "5f0268198d3d7484cb1877bec2c5d31f"; // GUI of test_collections.usda

        [SetUp]
        public void SetUp()
        {
            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(m_usdGUID));
            var stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            m_usdRoot = ImportHelpers.ImportSceneAsGameObject(scene);
            scene.Close();
        }

        [Test]
        public void ScopeWithoutChildrenExists()
        {
            var scopeWithoutChildren = m_usdRoot.transform.Find("TestComponent/geom");
            Assert.IsNotNull(scopeWithoutChildren);
            Assert.AreEqual(0, scopeWithoutChildren.childCount);
        }

        [Test]
        public void ScopeWithChildrenExists()
        {
            var scopeWithChildren = m_usdRoot.transform.Find("TestComponent/ScopeTest");
            Assert.IsNotNull(scopeWithChildren);
            Assert.AreNotEqual(0, scopeWithChildren.childCount);
        }

        [Test]
        public void ScopeHasPrimSourceComponent()
        {
            var scope = m_usdRoot.transform.Find("TestComponent/ScopeTest");
            var primSourceComponent = scope.GetComponent<UsdPrimSource>();
            Assert.IsNotNull(primSourceComponent);
            var scene = m_usdRoot.GetComponent<UsdAsset>().GetScene();
            var prim = scene.GetPrimAtPath(primSourceComponent.m_usdPrimPath);
            Assert.IsNotNull(prim);
            Assert.IsTrue("Scope" == prim.GetTypeName(), scope.name + " is not of type Scope: " + prim.GetTypeName());
        }

        [Test]
        public void ScopeTransformIsIdentity()
        {
            var scope = m_usdRoot.transform.Find("TestComponent/ScopeTest");
            Assert.IsTrue(Matrix4x4.identity == scope.transform.localToWorldMatrix);
        }
    }

    public class UsdMaterialTest
    {
        private GameObject m_usdRoot;
        private string m_usdGUID = "c06c7eba08022b74ca49dce5f79ef3ba"; // GUI of simpleMaterialTest.usd

        [SetUp]
        public void SetUp()
        {
            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(m_usdGUID));
            var stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            var importOptions = new SceneImportOptions();
            importOptions.materialImportMode = MaterialImportMode.ImportPreviewSurface;
            m_usdRoot = ImportHelpers.ImportSceneAsGameObject(scene, importOptions: importOptions);
            scene.Close();
        }

        [Test]
        public void TestMaterialNameSetOnImport()
        {
            // Check that the material name is the same after import as in the USD file
            Assert.IsNotNull(m_usdRoot);
            var cube = m_usdRoot.transform.Find("pCube1");
            Assert.IsNotNull(cube);
            var renderer = cube.GetComponent<Renderer>();
            Assert.IsNotNull(renderer);

            var material = renderer.sharedMaterial;
            Assert.IsNotNull(material);
            Assert.IsTrue(material.name == "lambert3SG");
        }
    }

    [TestFixture("a4a575275d6254853ac93b867a1a8471")] // Fully defined prims
    [TestFixture("b66c9dc38fc1e0044b3d93a196ad1365")] // Prims with references
    class AttributeScope
    {
        GameObject gameObject;
        Scene scene;
        string assetGuid;

        public AttributeScope(string assetGuid)
        {
            this.assetGuid = assetGuid;
        }

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

            scene = ImportHelpers.InitForOpen(Path.GetFullPath(assetPath));
            gameObject = ImportHelpers.ImportSceneAsGameObject(scene, null,
                new SceneImportOptions { payloadPolicy = PayloadPolicy.LoadAll });
        }

        [TearDown]
        public void TearDown()
        {
            scene.Close();
        }

        [Test]
        public void ConstantColours_DontWindUpInMeshes() // they get read into the material
        {
            var mesh = GameObject.Find("cube_constant_color").GetComponent<MeshFilter>().sharedMesh;
            CollectionAssert.IsEmpty(mesh.colors);
        }

        [Test]
        public void FaceColours_AreReadCorrectly() // they get read into the material
        {
            var mesh = GameObject.Find("cube_face_color").GetComponent<MeshFilter>().sharedMesh;
            var colours = Enumerable.Repeat(new Color(1.0f, 0.0f, 0, 1), 6)
                .Concat(Enumerable.Repeat(new Color(0.0f, 1.0f, 0, 1), 6))
                .Concat(Enumerable.Repeat(new Color(0.0f, 0.0f, 1.0f, 1), 6))
                .Concat(Enumerable.Repeat(new Color(1.0f, 1.0f, 0, 1), 6))
                .Concat(Enumerable.Repeat(new Color(1.0f, 0.0f, 1.0f, 1), 6))
                .Concat(Enumerable.Repeat(new Color(0.0f, 1.0f, 1.0f, 1), 6));
            CollectionAssert.AreEqual(colours, mesh.colors);
        }

        [TestCase("cube_vertex_color")]
        [TestCase("cube_varying_color")]
        public void VaryingAndVertexColours_AreReadCorrectly(string cubeName) // they get read into the material
        {
            var mesh = GameObject.Find(cubeName).GetComponent<MeshFilter>().sharedMesh;
            var colours = new[]
            {
                new Color(1.0f, 0.0f, 0, 1),
                new Color(0.0f, 1.0f, 0, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(1.0f, 1.0f, 0, 1),
                new Color(1.0f, 0.0f, 1.0f, 1),
                new Color(0.0f, 1.0f, 1.0f, 1),
                new Color(1.0f, 1.0f, 1.0f, 1),
                new Color(0.0f, 0.0f, 0.0f, 1),
            };
            CollectionAssert.AreEqual(colours, mesh.colors);
        }

        [Test]
        public void FaceVaryingColours_AreReadCorrectly() // they get read into the material
        {
            var mesh = GameObject.Find("cube_face_varying_color").GetComponent<MeshFilter>().sharedMesh;
            var colours = new[]
            {
                new Color(0.0f, 1.0f, 0, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(1.0f, 1.0f, 0, 1),
                new Color(0.0f, 1.0f, 0, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(1.0f, 1.0f, 0, 1),
                new Color(0.0f, 1.0f, 0, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(1.0f, 1.0f, 0, 1),
                new Color(0.0f, 1.0f, 0, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(1.0f, 1.0f, 0, 1),
                new Color(0.0f, 1.0f, 0, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(1.0f, 1.0f, 0, 1),
                new Color(0.0f, 1.0f, 0, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(0.0f, 0.0f, 1.0f, 1),
                new Color(1.0f, 0.0f, 0, 1),
                new Color(1.0f, 1.0f, 0, 1),
            };
            CollectionAssert.AreEqual(colours, mesh.colors);
        }
    }
}

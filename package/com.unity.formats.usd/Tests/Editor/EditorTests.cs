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
    public class UsdPrimTypeTest_Scope : BaseFixtureEditor
    {
        private GameObject m_usdRoot;

        [SetUp]
        public void SetUp()
        {
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(TestDataGuids.PrimType.CollectionsUsda));
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

    public class UsdMaterialTest : BaseFixtureEditor
    {
        private GameObject m_usdRoot;

        [SetUp]
        public void SetUp()
        {
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(TestDataGuids.Material.SimpleMaterialUsd));
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

    [TestFixture(TestDataGuids.Variability.CubesUsd)]
    [TestFixture(TestDataGuids.Variability.ReferencedCubesUsd)] // Prims with references
    class AttributeScope : BaseFixtureEditor
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

    class ExportXFormOverride
    {
        static readonly string SourceFilePath = Path.ChangeExtension(Path.GetTempFileName(), "usda");
        static readonly string OverFilePath = Path.ChangeExtension(Path.GetTempFileName(), "usda");

        [OneTimeSetUp]
        public void CreateLoadExport()
        {
            // Create a new Stage
            var scene = ExportHelpers.InitForSave(SourceFilePath);
            var xformToken = new TfToken("Xform");
            scene.Stage.DefinePrim(new SdfPath("/root/A"), xformToken);
            scene.Stage.DefinePrim(new SdfPath("/root/B"), xformToken);
            scene.Save();

            // Load the stage and modify /root/A transform
            var root = ImportHelpers.ImportSceneAsGameObject(scene);
            scene.Close();
            var primA = root.transform.Find("A");
            primA.transform.localPosition = new Vector3(10.0f, 10.0f, 10.0f);

            // Export overrides
            var usdAsset = root.GetComponentInParent<UsdAsset>();
            var overs = ExportHelpers.InitForSave(OverFilePath);
            usdAsset.ExportOverrides(overs);
        }

        [Test]
        public void ExportXFormOverride_OnlyExportChanges_Success()
        {
            var outScene = Scene.Open(OverFilePath);
            NUnit.Framework.Assert.IsTrue(outScene.Stage.GetPrimAtPath(new SdfPath("/root/A")).IsValid());
            NUnit.Framework.Assert.IsFalse(outScene.Stage.GetPrimAtPath(new SdfPath("/root/B")).IsValid());
        }

        [Test]
        public void ExportXFormOverride_NoSublayers_True()
        {
            var outScene = Scene.Open(OverFilePath);
            NUnit.Framework.Assert.Zero(outScene.Stage.GetRootLayer().GetNumSubLayerPaths());
        }

        [Test]
        public void ExportXFormOverride_NoPrimDefined_True()
        {
            var outScene = Scene.Open(OverFilePath);
            foreach (var prim in outScene.Stage.GetAllPrims())
            {
                Debug.Log(prim.GetPath().ToString());
                NUnit.Framework.Assert.AreEqual(SdfSpecifier.SdfSpecifierOver, prim.GetSpecifier());
            }
        }

        [OneTimeTearDown]
        public void DeleteTestFiles()
        {
            File.Delete(OverFilePath);
            File.Delete(SourceFilePath);
        }
    }
}

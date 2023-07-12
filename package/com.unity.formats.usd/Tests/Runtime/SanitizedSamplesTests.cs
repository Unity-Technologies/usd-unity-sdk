using System.IO;
using System.Linq;
using NUnit.Framework;
using pxr;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
#if UNITY_EDITOR
    public class SanitizedMeshesTests : BaseFixtureRuntime
    {
        class SanitizeTests
        {
            Scene scene;

            [SetUp]
            public void SetUp()
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(TestDataGuids.VariedCollection.AttributeScopeUsda);
                scene = ImportHelpers.InitForOpen(Path.GetFullPath(assetPath));
            }

            [TearDown]
            public void TearDown()
            {
                scene.Close();
            }

            [TestCase("/box_face_colors")]
            [TestCase("/box_facevarying_colors")]
            [TestCase("/box_face_colors_indexed")]
            [TestCase("/box_facevarying_colors_indexed")]
            public void Sanitize_CheckUnroll_ColorsOnly_True(string primPath)
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read(primPath, sanitizedSample);
                var importOptions = new SceneImportOptions();
                sanitizedSample.Sanitize(scene, importOptions);

                Assert.True(sanitizedSample.arePrimvarsFaceVarying,
                    "The mesh has not been unrolled");

                Assert.AreEqual(3, sanitizedSample.faceVertexCounts.Max(),
                    "The mesh has not been triangulated");
                Assert.AreEqual(sanitizedSample.points.Length, sanitizedSample.faceVertexIndices.Length,
                    "Points have not been unrolled");
                Assert.AreEqual(sanitizedSample.points.Length, sanitizedSample.colors.Length,
                    "Points and colors arrays don't match");
            }

            [TestCase("/box_constant_colors_face_normals")]
            [TestCase("/box_constant_colors_facevarying_normals")]
            [TestCase("/box_face_colors_face_normals")]
            [TestCase("/box_face_colors_vertex_normals")]
            [TestCase("/box_face_colors_facevarying_normals")]
            [TestCase("/box_vertex_colors_face_normals")]
            [TestCase("/box_vertex_colors_facevarying_normals")]
            [TestCase("/box_facevarying_colors_face_normals")]
            [TestCase("/box_facevarying_colors_vertex_normals")]
            [TestCase("/box_facevarying_colors_facevarying_normals")]
            public void Sanitize_CheckUnroll_ColorsAndNormals_True(string primPath)
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read(primPath, sanitizedSample);
                var importOptions = new SceneImportOptions();
                sanitizedSample.Sanitize(scene, importOptions);

                Assert.True(sanitizedSample.arePrimvarsFaceVarying,
                    "The mesh has not been unrolled");
                Assert.AreEqual(3, sanitizedSample.faceVertexCounts.Max(),
                    "The mesh has not been triangulated");
                Assert.AreEqual(sanitizedSample.points.Length, sanitizedSample.faceVertexIndices.Length,
                    "Points have not been unrolled");
                if (sanitizedSample.colors.Length > 1)
                    Assert.AreEqual(sanitizedSample.points.Length, sanitizedSample.colors.Length,
                        "Points and colors arrays don't match");
                Assert.AreEqual(sanitizedSample.points.Length, sanitizedSample.normals.Length,
                    "Points and normals arrays don't match");
            }

            [TestCase("/box_constant_colors")]
            [TestCase("/box_vertex_colors")]
            [TestCase("/box_vertex_colors_indexed")]
            [TestCase("/box_constant_colors_vertex_normals")]
            [TestCase("/box_vertex_colors_vertex_normals")]
            public void Sanitize_CheckUnroll_False(string primPath)
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read(primPath, sanitizedSample);
                var importOptions = new SceneImportOptions();
                sanitizedSample.Sanitize(scene, importOptions);

                Assert.False(sanitizedSample.arePrimvarsFaceVarying,
                    "The mesh has been unrolled");
            }

            [TestCase("/box_vertex_colors_facevarying_uvs")]
            public void Sanitize_CheckUnroll_WithUVs_LoadColors_False(string primPath)
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read(primPath, sanitizedSample);
                var importOptions = new SceneImportOptions();
                sanitizedSample.Sanitize(scene, importOptions);

                Assert.False(sanitizedSample.arePrimvarsFaceVarying,
                    "The mesh has been unrolled");
            }

            [TestCase("/box_vertex_colors_facevarying_uvs")]
            public void Sanitize_CheckUnroll_WithUVs_LoadMaterials_True(string primPath)
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read(primPath, sanitizedSample);
                var importOptions = new SceneImportOptions();
                importOptions.materialImportMode = MaterialImportMode.ImportPreviewSurface;
                sanitizedSample.Sanitize(scene, importOptions);

                Assert.True(sanitizedSample.arePrimvarsFaceVarying,
                    "The mesh has not been unrolled");
            }
        }

        class MethodUnitTests
        {
            Scene scene;

            [SetUp]
            public void SetUp()
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(TestDataGuids.VariedCollection.AttributeScopeUsda);
                scene = ImportHelpers.InitForOpen(Path.GetFullPath(assetPath));
            }

            [TearDown]
            public void TearDown()
            {
                scene.Close();
            }

            [TestCase("/grid_righthanded", true, new[] {1, 0, 4, 4, 0, 3, 2, 1, 5, 5, 1, 4})]
            [TestCase("/grid_righthanded", false, new[] {0, 1, 4, 0, 4, 3, 1, 2, 5, 1, 5, 4})]
            public void Triangulate_TrianglesAreCorrect(string path, bool unwindVertices, int[] indices)
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read(path, sanitizedSample);
                sanitizedSample.Triangulate(unwindVertices);

                CollectionAssert.AreEqual(indices, sanitizedSample.faceVertexIndices);
            }

            [Test]
            public void Flatten_ValuesAreCorrect()
            {
                var values = new[] {0, 10, 20, 30};
                var indices = new[] {0, 0, 1, 1, 2, 2, 3, 3};

                SanitizedMeshSample.Flatten(ref values, indices);

                Assert.AreEqual(indices.Length, values.Length, "New values count don't match indices count.");
                CollectionAssert.AreEqual(new[] {0, 0, 10, 10, 20, 20, 30, 30}, values,
                    "New values are not correct.");
            }

            [Test]
            public void Flatten_NullValues_NoOp()
            {
                int[] values = null;
                var indices = new[] {0, 0, 1, 1, 2, 2, 3, 3};
                Assert.DoesNotThrow(delegate { SanitizedMeshSample.Flatten(ref values, indices); });
            }

            [TestCase(false, new[] {0, 1, 2, 3, 4, 5, 3, 5, 6, 7, 8, 9, 7, 9, 10, 7, 10, 11})]
            [TestCase(true, new[] {1, 0, 2, 4, 3, 5, 5, 3, 6, 8, 7, 9, 9, 7, 10, 10, 7, 11})]
            public void TriangulateAttributes_ValuesAreCorrect(bool changeHandedness, int[] result)
            {
                var values = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
                var faceVertexCount = new[] {3, 4, 5};

                SanitizedMeshSample.TriangulateAttributes(ref values, faceVertexCount, changeHandedness);
                CollectionAssert.AreEqual(result, values);
            }

            [Test]
            public void GuessInterpolation_ValuesAreCorrect()
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read("/grid_righthanded", sanitizedSample);

                var defaultToken = new TfToken();
                Assert.AreEqual(defaultToken, sanitizedSample.GuessInterpolation(0));
                Assert.AreEqual(UsdGeomTokens.constant, sanitizedSample.GuessInterpolation(1));
                Assert.AreEqual(UsdGeomTokens.vertex, sanitizedSample.GuessInterpolation(6));

                // If the topology is not backed up there's no way to guess uniform or facevarying
                Assert.AreEqual(defaultToken, sanitizedSample.GuessInterpolation(2));
                Assert.AreEqual(defaultToken, sanitizedSample.GuessInterpolation(8));

                sanitizedSample.BackupTopology();
                Assert.AreEqual(UsdGeomTokens.uniform, sanitizedSample.GuessInterpolation(2));
                Assert.AreEqual(UsdGeomTokens.faceVarying, sanitizedSample.GuessInterpolation(8));
            }

            [Test]
            public void ShouldUnwindVertices_ValuesAreCorrect()
            {
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read("/grid_righthanded", sanitizedSample);
                Assert.True(sanitizedSample.ShouldUnwindVertices(true));
                Assert.False(sanitizedSample.ShouldUnwindVertices(false));


                scene.Read("/grid_lefthanded", sanitizedSample);
                Assert.False(sanitizedSample.ShouldUnwindVertices(true));
                Assert.True(sanitizedSample.ShouldUnwindVertices(false));
            }

            [Test]
            public void UniformToFaceVarying_ValuesAreCorrect()
            {
                var values = new[] {5, 10};
                var sanitizedSample = new SanitizedMeshSample();
                scene.Read("/grid_righthanded", sanitizedSample);
                sanitizedSample.Triangulate(false);

                sanitizedSample.UniformToFaceVarying(ref values, sanitizedSample.faceVertexIndices.Length);
                CollectionAssert.AreEqual(new[] {5, 5, 5, 5, 5, 5, 10, 10, 10, 10, 10, 10}, values);
            }
        }
    }

    public class SanitizedInstancesTests
    {
        GameObject m_usdRoot;

        [SetUp]
        public void SetUp()
        {
            InitUsd.Initialize();
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(TestDataGuids.Instancer.MeshInstancesUsd));
            var stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            m_usdRoot = ImportHelpers.ImportSceneAsGameObject(scene);
            scene.Close();
        }

        [Test]
        public void MeshInstancesLoading_MeshesSanitized_True()
        {
            var mesh = m_usdRoot.transform.Find("cube_01/cube_geo").GetComponent<MeshFilter>().sharedMesh;
            var vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f)
            };
            CollectionAssert.AreEqual(vertices, mesh.vertices);
        }

        [Test]
        public void MeshInstancesLoading_XformsAreSanitized_True()
        {
            var xform = m_usdRoot.transform.Find("cube_01");
            Assert.AreEqual(new Vector3(2, 0 , -2), xform.position);
        }

        [Test]
        public void MeshInstancesLoading_MeshesAreShared_True()
        {
            var mesh1 = m_usdRoot.transform.Find("cube_01/cube_geo").GetComponent<MeshFilter>().sharedMesh;
            var mesh2 = m_usdRoot.transform.Find("cube_02/cube_geo").GetComponent<MeshFilter>().sharedMesh;
            Assert.AreEqual(mesh1, mesh2);
        }
    }
#endif
}

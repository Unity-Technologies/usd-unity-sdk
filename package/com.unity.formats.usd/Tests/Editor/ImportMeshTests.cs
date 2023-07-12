using System.IO;
using NUnit.Framework;
using pxr;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class ImportSkinnedMeshTests : BaseFixtureEditor
    {
        GameObject m_usdRoot;

        [SetUp]
        public void SetUp()
        {
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(TestDataGuids.Mesh.SkinnedMeshUsda));
            var stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            m_usdRoot = ImportHelpers.ImportSceneAsGameObject(scene);
            scene.Close();
        }

        public void MatrixCompare(Matrix4x4 actual, Matrix4x4 expected, float epsilon = 1e-5f)
        {
            Assert.That(actual.m00, Is.EqualTo(expected.m00).Within(epsilon));
            Assert.That(actual.m01, Is.EqualTo(expected.m01).Within(epsilon));
            Assert.That(actual.m02, Is.EqualTo(expected.m02).Within(epsilon));
            Assert.That(actual.m03, Is.EqualTo(expected.m03).Within(epsilon));
            Assert.That(actual.m10, Is.EqualTo(expected.m10).Within(epsilon));
            Assert.That(actual.m11, Is.EqualTo(expected.m11).Within(epsilon));
            Assert.That(actual.m12, Is.EqualTo(expected.m12).Within(epsilon));
            Assert.That(actual.m13, Is.EqualTo(expected.m13).Within(epsilon));
            Assert.That(actual.m20, Is.EqualTo(expected.m20).Within(epsilon));
            Assert.That(actual.m21, Is.EqualTo(expected.m21).Within(epsilon));
            Assert.That(actual.m22, Is.EqualTo(expected.m22).Within(epsilon));
            Assert.That(actual.m23, Is.EqualTo(expected.m23).Within(epsilon));
            Assert.That(actual.m30, Is.EqualTo(expected.m30).Within(epsilon));
            Assert.That(actual.m31, Is.EqualTo(expected.m31).Within(epsilon));
            Assert.That(actual.m32, Is.EqualTo(expected.m32).Within(epsilon));
            Assert.That(actual.m33, Is.EqualTo(expected.m33).Within(epsilon));
        }

        [Test]
        public void ImportSkinnedMesh_Success()
        {
            var go = m_usdRoot.transform.Find("geo/prism_allJoints").gameObject;
            var skinnedMesh = go.GetComponent<SkinnedMeshRenderer>();
            var bindPose = new[]
            {
                new Matrix4x4(
                    new Vector4(0.08966722f, 0.0f, -0.9918941f, 0.0f),
                    new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                    new Vector4(0.9918941f, 0.0f, 0.08966722f, 0.0f),
                    new Vector4(0.02053221f, 1.3494f, 0.001856089f, 1.0f)
                ),
                new Matrix4x4(
                    new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                    new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                    new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                    new Vector4(0.0f, -0.4696887f, -1.908196E-16f, 1.0f)
                ),
                new Matrix4x4(
                    new Vector4(0.08966722f, 0.0f, -0.9918941f, 0.0f),
                    new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                    new Vector4(0.9918941f, 0.0f, 0.08966722f, 0.0f),
                    new Vector4(-0.08966722f, 1.879f, 0.9918941f, 1.0f)
                ),
                new Matrix4x4(
                    new Vector4(0.08966721f, 0.0f, -0.9918938f, 0.0f),
                    new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                    new Vector4(0.9918939f, 0.0f, 0.0896694f, 0.0f),
                    new Vector4(0.08966721f, 1.879f, -0.9918938f, 1.0f)
                ),
            };

            Assert.AreEqual(bindPose.Length, skinnedMesh.sharedMesh.bindposes.Length);
            for (int i = 0; i < 4; i++)
            {
                MatrixCompare(bindPose[i], skinnedMesh.sharedMesh.bindposes[i]);
            }
        }

        [Test]
        public void ImportSkinnedMesh_JointsSubset_Success()
        {
            var go = m_usdRoot.transform.Find("geo/cube_jointsSubset").gameObject;
            var skinnedMesh = go.GetComponent<SkinnedMeshRenderer>();
            var bindPose = new[]
            {
                new Matrix4x4(
                    new Vector4(0.08966721f, 0f, -0.9918938f, 0f),
                    new Vector4(0f, 1f, 0f, 0f),
                    new Vector4(0.9918939f, 0f, 0.0896694f, 0f),
                    new Vector4(0f, 0f, 0f, 1f)
                ),
            };
            Assert.AreEqual(bindPose.Length, skinnedMesh.sharedMesh.bindposes.Length);
            for (int i = 0; i < skinnedMesh.sharedMesh.bindposes.Length; i++)
            {
                MatrixCompare(bindPose[i], skinnedMesh.sharedMesh.bindposes[i]);
            }
        }

        [Test]
        public void ImportSkinnedMesh_CustomJointsOrder_Success()
        {
            var go = m_usdRoot.transform.Find("geo/pyramid_jointReorder").gameObject;
            var skinnedMesh = go.GetComponent<SkinnedMeshRenderer>();
            var bindPose = new[]
            {
                new Matrix4x4(
                    new Vector4(0.08966722f, 0f, -0.9918941f, 0f),
                    new Vector4(0f, 1f, 0f, 0f),
                    new Vector4(0.9918941f, 0f, 0.08966722f, 0f),
                    new Vector4(0.1101994f, -0.5296f, -0.990038f, 1f)
                ),
                new Matrix4x4(
                    new Vector4(0.08966722f, 0f, -0.9918941f, 0f),
                    new Vector4(0f, 1f, 0f, 0f),
                    new Vector4(0.9918941f, 0f, 0.08966722f, 0f),
                    new Vector4(0f, 0f, 0f, 1f)
                ),
                new Matrix4x4(
                    new Vector4(0.08966721f, 0f, -0.9918938f, 0f),
                    new Vector4(0f, 1f, 0f, 0f),
                    new Vector4(0.9918939f, 0f, 0.0896694f, 0f),
                    new Vector4(0.1793344f, 0f, -1.983788f, 1f)
                ),
                new Matrix4x4(
                    new Vector4(1f, 0f, 0f, 0f),
                    new Vector4(0f, 1f, 0f, 0f),
                    new Vector4(0f, 0f, 1f, 0f),
                    new Vector4(1f, -2.348689f, -1.908196E-16f, 1f)
                ),
            };
            Assert.AreEqual(bindPose.Length, skinnedMesh.sharedMesh.bindposes.Length);
            for (int i = 0; i < skinnedMesh.sharedMesh.bindposes.Length; i++)
            {
                MatrixCompare(bindPose[i], skinnedMesh.sharedMesh.bindposes[i]);
            }
        }
    }
}

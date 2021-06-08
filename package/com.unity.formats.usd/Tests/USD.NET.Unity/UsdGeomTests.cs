using NUnit.Framework;
using pxr;
using UnityEngine;

namespace USD.NET.Unity.Tests
{
    class UsdGeomTests : UsdTests
    {
        class PrimvarSample : SampleBase
        {
            public Primvar<float[]> NotSerialized;

            [UsdNamespace("foo:bar")] public Primvar<float[]> serialized = new Primvar<float[]>();

            public Primvar<float> scalar = new Primvar<float>();

            public Primvar<Vector4[]> vector = new Primvar<Vector4[]>();
        }

        [Test]
        public static void PrimvarTest()
        {
            var sample = new PrimvarSample();
            var sample2 = new PrimvarSample();
            sample.serialized.value = new float[10];
            sample.serialized.interpolation = PrimvarInterpolation.FaceVarying;
            sample.serialized.elementSize = 3;
            sample.serialized.indices = new int[] {1, 2, 3, 4, 1};

            sample.scalar.value = 2.1f;
            Assert.AreEqual(PrimvarInterpolation.Constant, sample.scalar.interpolation);
            Assert.AreEqual(UsdGeomTokens.constant, sample.scalar.GetInterpolationToken());

            sample.vector.value = new[]
            {
                new Vector4(1, 2, 3, 4),
                new Vector4(4, 5, 6, 7)
            };
            sample.vector.elementSize = 4;

            WriteAndRead(ref sample, ref sample2);

            Assert.AreEqual(sample.serialized.value, sample2.serialized.value);
            Assert.AreEqual(sample.serialized.indices, sample2.serialized.indices);
            Assert.AreEqual(sample.serialized.interpolation, sample2.serialized.interpolation);
            Assert.AreEqual(sample.serialized.elementSize, sample2.serialized.elementSize);

            Assert.AreEqual(sample.vector.value, sample2.vector.value);
            Assert.AreEqual(sample.vector.indices, sample2.vector.indices);
            Assert.AreEqual(sample.vector.interpolation, sample2.vector.interpolation);
            Assert.AreEqual(sample.vector.elementSize, sample2.vector.elementSize);

            sample.NotSerialized = new Primvar<float[]>();
            WriteAndRead(ref sample, ref sample2);
        }

        [Test]
        public static void CurvesTest()
        {
            UsdStage stage = UsdStage.CreateInMemory();
            var path = new SdfPath("/Parent/Curves");
            var curvesGprim = UsdGeomBasisCurves.Define(new UsdStageWeakPtr(stage), path);
            var vertCounts = IntrinsicTypeConverter.ToVtArray(new[] {4});
            curvesGprim.CreateCurveVertexCountsAttr(vertCounts);

            var basisCurves = new BasisCurvesSample();
            basisCurves.basis = BasisCurvesSample.Basis.Bspline;
            basisCurves.type = BasisCurvesSample.CurveType.Cubic;
            basisCurves.wrap = BasisCurvesSample.WrapMode.Nonperiodic;
            basisCurves.colors = new UnityEngine.Color[4];
            basisCurves.colors[0] = new UnityEngine.Color(1, 2, 3, 4);
            basisCurves.colors[3] = new UnityEngine.Color(6, 7, 8, 9);
            basisCurves.curveVertexCounts = new int[1] {4};
            basisCurves.doubleSided = true;
            basisCurves.normals = new UnityEngine.Vector3[4];
            basisCurves.normals[0] = new UnityEngine.Vector3(1, 0, 0);
            basisCurves.normals[3] = new UnityEngine.Vector3(0, 0, 1);

            basisCurves.widths = new float[4];
            basisCurves.widths[0] = .5f;
            basisCurves.widths[1] = 1f;
            basisCurves.widths[2] = .2f;
            basisCurves.widths[3] = 2f;

            basisCurves.orientation = Orientation.RightHanded;
            basisCurves.points = new Vector3[4];
            basisCurves.points[0] = new Vector3(1, 2, 3);
            basisCurves.points[3] = new Vector3(7, 8, 9);

            basisCurves.velocities = new Vector3[4];
            basisCurves.velocities[0] = new Vector3(11, 22, 33);
            basisCurves.velocities[3] = new Vector3(77, 88, 99);

            basisCurves.wrap = BasisCurvesSample.WrapMode.Periodic;
            basisCurves.transform = Matrix4x4.identity;

            var basisCurves2 = new BasisCurvesSample();
            WriteAndRead(ref basisCurves, ref basisCurves2);

            Assert.AreEqual(basisCurves.basis, basisCurves2.basis);
            AssertEqual(basisCurves.colors, basisCurves2.colors);
            AssertEqual(basisCurves.curveVertexCounts, basisCurves2.curveVertexCounts);
            Assert.AreEqual(basisCurves.doubleSided, basisCurves2.doubleSided);
            AssertEqual(basisCurves.normals, basisCurves2.normals);
            Assert.AreEqual(basisCurves.orientation, basisCurves2.orientation);
            AssertEqual(basisCurves.points, basisCurves2.points);
            Assert.AreEqual(basisCurves.type, basisCurves2.type);
            AssertEqual(basisCurves.velocities, basisCurves2.velocities);
            AssertEqual(basisCurves.widths, basisCurves2.widths);
            Assert.AreEqual(basisCurves.wrap, basisCurves2.wrap);
            Assert.AreEqual(basisCurves.transform, basisCurves2.transform);
            AssertEqual(basisCurves.xformOpOrder, basisCurves2.xformOpOrder);
        }

        [Test]
        public static void CameraTest()
        {
            var cam = new CameraSample
            {
                projection = CameraSample.ProjectionType.Perspective,
                clippingPlanes = new[] {new Vector4(0, 1, 2, 3), new UnityEngine.Vector4(4, 5, 6, 7)},
                clippingRange = new Vector2(0.01f, 1000.0f),
                focalLength = 50,
                focusDistance = 1.0f,
                fStop = 2.5f,
                horizontalAperture = 20.9550f,
                horizontalApertureOffset = 0.001f,
                stereoRole = CameraSample.StereoRole.Mono,
                verticalAperture = 15.2908f,
                verticalApertureOffset = 0.002f,
                shutter = new CameraSample.Shutter {open = 0.001, close = 0.002}
            };

            // Prep a new camera sample to be populated.
            var cam2 = new CameraSample {shutter = new CameraSample.Shutter()};

            WriteAndRead(ref cam, ref cam2);

            AssertEqual(cam.clippingPlanes, cam2.clippingPlanes);
            Assert.AreEqual(cam.clippingRange, cam2.clippingRange);
            Assert.AreEqual(cam.focalLength, cam2.focalLength);
            Assert.AreEqual(cam.focusDistance, cam2.focusDistance);
            Assert.AreEqual(cam.fStop, cam2.fStop);
            Assert.AreEqual(cam.horizontalAperture, cam2.horizontalAperture);
            Assert.AreEqual(cam.horizontalApertureOffset, cam2.horizontalApertureOffset);
            Assert.AreEqual(cam.projection, cam2.projection);
            Assert.AreEqual(cam.shutter.open, cam2.shutter.open);
            Assert.AreEqual(cam.shutter.close, cam2.shutter.close);
            Assert.AreEqual(cam.stereoRole, cam2.stereoRole);
            Assert.AreEqual(cam.verticalAperture, cam2.verticalAperture);
            Assert.AreEqual(cam.verticalApertureOffset, cam2.verticalApertureOffset);
        }

        [Test]
        public static void CameraRoundtripTest()
        {
            var unityCamOrig = new GameObject().AddComponent<Camera>();
            unityCamOrig.transform.position = new Vector3(0, 0, 5);
            unityCamOrig.transform.eulerAngles = new Vector3(10, 90, 5);
            unityCamOrig.transform.localScale = new Vector3(2, 2, 2);

            unityCamOrig.fieldOfView = 25;
            unityCamOrig.nearClipPlane = 0.2f;
            unityCamOrig.farClipPlane = 100;

            var unityCamCopied = new GameObject().AddComponent<Camera>();
            var sampleIn = new CameraSample();
            var sampleOut = new CameraSample(unityCamOrig);

            WriteAndRead(ref sampleOut, ref sampleIn);

            sampleIn.CopyToCamera(unityCamCopied, false);


            Assert.AreEqual(unityCamOrig.orthographic, unityCamCopied.orthographic);
            Assert.AreEqual(unityCamOrig.orthographicSize, unityCamCopied.orthographicSize);
            Assert.AreEqual(unityCamOrig.fieldOfView, unityCamCopied.fieldOfView);
            Assert.AreEqual(unityCamOrig.aspect, unityCamCopied.aspect);
            Assert.AreEqual(unityCamOrig.nearClipPlane, unityCamCopied.nearClipPlane);
            Assert.AreEqual(unityCamOrig.farClipPlane, unityCamCopied.farClipPlane);
            Assert.AreNotEqual(unityCamOrig.transform.localToWorldMatrix, unityCamCopied.transform.localToWorldMatrix);
            Assert.IsFalse(unityCamCopied.usePhysicalProperties);

            sampleIn.CopyToCamera(unityCamCopied, true);

            var copiedTransformConverted = UnityTypeConverter.ChangeBasis(unityCamCopied.transform.localToWorldMatrix);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m00, copiedTransformConverted.m00, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m01, copiedTransformConverted.m01, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m02, copiedTransformConverted.m02, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m03, copiedTransformConverted.m03, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m10, copiedTransformConverted.m10, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m11, copiedTransformConverted.m11, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m12, copiedTransformConverted.m12, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m13, copiedTransformConverted.m13, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m20, copiedTransformConverted.m20, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m21, copiedTransformConverted.m21, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m22, copiedTransformConverted.m22, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m23, copiedTransformConverted.m23, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m30, copiedTransformConverted.m30, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m31, copiedTransformConverted.m31, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m32, copiedTransformConverted.m32, 0.00001);
            Assert.AreEqual(unityCamOrig.transform.localToWorldMatrix.m33, copiedTransformConverted.m33, 0.00001);
        }
    }
}

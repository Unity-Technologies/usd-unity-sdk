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
using pxr;
using USD.NET;
using USD.NET.Unity;

namespace Tests.Cases {
  class UsdGeomTests : UnitTest {

    private class PrimvarSample : SampleBase {
      public Primvar<float[]> notSerialized;

      [UsdNamespace("foo:bar")]
      public Primvar<float[]> serialized = new Primvar<float[]>();

      public Primvar<float> scalar = new Primvar<float>();

      public Primvar<UnityEngine.Vector4[]> vector = new Primvar<UnityEngine.Vector4[]>();
    }

    public static void PrimvarTest() {
      var scene = Scene.Create();
      var sample = new PrimvarSample();
      var sample2 = new PrimvarSample();
      sample.serialized.value = new float[10];
      sample.serialized.interpolation = PrimvarInterpolation.FaceVarying;
      sample.serialized.elementSize = 3;
      sample.serialized.indices = new int[] { 1, 2, 3, 4, 1 };

      sample.scalar.value = 2.1f;
      AssertEqual(sample.scalar.interpolation, PrimvarInterpolation.Constant);
      AssertEqual(sample.scalar.GetInterpolationToken(), UsdGeomTokens.constant);

      sample.vector.value = new UnityEngine.Vector4[] {
        new UnityEngine.Vector4(1, 2, 3, 4),
        new UnityEngine.Vector4(4, 5, 6, 7) };
      sample.vector.elementSize = 4;
      
      scene.Write("/Foo/Bar", sample);
      WriteAndRead(ref sample, ref sample2, true);

      AssertEqual(sample.serialized.value, sample2.serialized.value);
      AssertEqual(sample.serialized.indices, sample2.serialized.indices);
      AssertEqual(sample.serialized.interpolation, sample2.serialized.interpolation);
      AssertEqual(sample.serialized.elementSize, sample2.serialized.elementSize);

      AssertEqual(sample.vector.value, sample2.vector.value);
      AssertEqual(sample.vector.indices, sample2.vector.indices);
      AssertEqual(sample.vector.interpolation, sample2.vector.interpolation);
      AssertEqual(sample.vector.elementSize, sample2.vector.elementSize);

      sample.notSerialized = new Primvar<float[]>();
      WriteAndRead(ref sample, ref sample2, true);
    }

    public static void CurvesTest() {
      UsdStage stage = UsdStage.CreateInMemory();
      var path = new SdfPath("/Parent/Curves");
      var curvesGprim = UsdGeomBasisCurves.Define(new UsdStageWeakPtr(stage), path);
      var vertCounts = IntrinsicTypeConverter.ToVtArray(new int[] { 4 });
      var basisAttr = curvesGprim.CreateBasisAttr(UsdGeomTokens.bezier);
      curvesGprim.CreateCurveVertexCountsAttr(vertCounts);

      var basisCurves = new BasisCurvesSample();
      basisCurves.basis = BasisCurvesSample.Basis.Bspline;
      basisCurves.type = BasisCurvesSample.CurveType.Cubic;
      basisCurves.wrap = BasisCurvesSample.WrapMode.Nonperiodic;
      basisCurves.colors = new UnityEngine.Color[4];
      basisCurves.colors[0] = new UnityEngine.Color(1, 2, 3, 4);
      basisCurves.colors[3] = new UnityEngine.Color(6, 7, 8, 9);
      basisCurves.curveVertexCounts = new int[1] { 4 };
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
      basisCurves.points = new UnityEngine.Vector3[4];
      basisCurves.points[0] = new UnityEngine.Vector3(1, 2, 3);
      basisCurves.points[3] = new UnityEngine.Vector3(7, 8, 9);

      basisCurves.velocities = new UnityEngine.Vector3[4];
      basisCurves.velocities[0] = new UnityEngine.Vector3(11, 22, 33);
      basisCurves.velocities[3] = new UnityEngine.Vector3(77, 88, 99);
      
      basisCurves.wrap = BasisCurvesSample.WrapMode.Periodic;
      basisCurves.transform = UnityEngine.Matrix4x4.identity;

      var basisCurves2 = new BasisCurvesSample();
      WriteAndRead(ref basisCurves, ref basisCurves2, true);

      AssertEqual(basisCurves.basis, basisCurves2.basis);
      AssertEqual(basisCurves.colors, basisCurves2.colors);
      AssertEqual(basisCurves.curveVertexCounts, basisCurves2.curveVertexCounts);
      AssertEqual(basisCurves.doubleSided, basisCurves2.doubleSided);
      AssertEqual(basisCurves.normals, basisCurves2.normals);
      AssertEqual(basisCurves.orientation, basisCurves2.orientation);
      AssertEqual(basisCurves.points, basisCurves2.points);
      AssertEqual(basisCurves.type, basisCurves2.type);
      AssertEqual(basisCurves.velocities, basisCurves2.velocities);
      AssertEqual(basisCurves.widths, basisCurves2.widths);
      AssertEqual(basisCurves.wrap, basisCurves2.wrap);
      AssertEqual(basisCurves.transform, basisCurves2.transform);
      AssertEqual(basisCurves.xformOpOrder, basisCurves2.xformOpOrder);
    }

    public static void Camera2Test() {
      CameraSample sample = new CameraSample();
      sample.transform = UnityEngine.Matrix4x4.identity;
      sample.clippingRange = new UnityEngine.Vector2(.01f, 10);

      // GfCamera is a gold mine of camera math.
      pxr.GfCamera c = new pxr.GfCamera(UnityTypeConverter.ToGfMatrix(UnityEngine.Matrix4x4.identity));

      sample.focalLength = c.GetFocalLength();
      sample.horizontalAperture = c.GetHorizontalAperture();
      sample.verticalAperture = c.GetVerticalAperture();

      var scene = Scene.Create();
      scene.Write("/Foo/Bar", sample);
    }

    public static void CameraTest() {
      var scene = Scene.Create();
      var cam = new CameraSample();

      cam.projection = CameraSample.ProjectionType.Perspective;
      cam.clippingPlanes = new UnityEngine.Vector4[] {
          new UnityEngine.Vector4(0, 1, 2, 3),
          new UnityEngine.Vector4(4, 5, 6, 7) };
      cam.clippingRange = new UnityEngine.Vector2(0.01f, 1000.0f);
      cam.focalLength = 50;
      cam.focusDistance = 1.0f;
      cam.fStop = 2.5f;
      cam.horizontalAperture = 20.9550f;
      cam.horizontalApertureOffset = 0.001f;
      cam.stereoRole = CameraSample.StereoRole.Mono;
      cam.verticalAperture = 15.2908f;
      cam.verticalApertureOffset = 0.002f;

      cam.shutter = new CameraSample.Shutter();
      cam.shutter.open = 0.001;
      cam.shutter.close = 0.002;

      // Prep a new camera sample to be populated.
      var cam2 = new CameraSample();
      cam2.shutter = new CameraSample.Shutter();

      UsdGeomTests.WriteAndRead(ref cam, ref cam2, true);

      AssertEqual(cam.clippingPlanes, cam2.clippingPlanes);
      AssertEqual(cam.clippingRange, cam2.clippingRange);
      AssertEqual(cam.focalLength, cam2.focalLength);
      AssertEqual(cam.focusDistance, cam2.focusDistance);
      AssertEqual(cam.fStop, cam2.fStop);
      AssertEqual(cam.horizontalAperture, cam2.horizontalAperture);
      AssertEqual(cam.horizontalApertureOffset, cam2.horizontalApertureOffset);
      AssertEqual(cam.projection, cam2.projection);
      AssertEqual(cam.shutter.open, cam2.shutter.open);
      AssertEqual(cam.shutter.close, cam2.shutter.close);
      AssertEqual(cam.stereoRole, cam2.stereoRole);
      AssertEqual(cam.verticalAperture, cam2.verticalAperture);
      AssertEqual(cam.verticalApertureOffset, cam2.verticalApertureOffset);
    }
  }
}

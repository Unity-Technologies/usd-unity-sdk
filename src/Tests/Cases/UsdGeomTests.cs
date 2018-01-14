using System;
using System.Collections.Generic;
using System.Linq;
using pxr;

namespace Tests.Cases {
  class UsdGeomTests : UnitTest {
    public static void CurvesTest() {
      UsdStage stage = UsdStage.CreateInMemory();
      var path = new SdfPath("/Parent/Curves");
      var curvesGprim = UsdGeomBasisCurves.Define(new UsdStageWeakPtr(stage), path);
      var vertCounts = USD.NET.IntrinsicTypeConverter.ToVtArray(new int[] { 4 });
      var basisAttr = curvesGprim.CreateBasisAttr(UsdGeomTokens.bezier);
      curvesGprim.CreateCurveVertexCountsAttr(vertCounts);
    }

    public static void CameraTest() {
      var scene = USD.NET.Scene.Create();
      var cam = new USD.NET.Unity.CameraSample();

      cam.projection = USD.NET.Unity.CameraSample.ProjectionType.Perspective;
      cam.clippingPlanes = new UnityEngine.Vector4[] {
          new UnityEngine.Vector4(0, 1, 2, 3),
          new UnityEngine.Vector4(4, 5, 6, 7) };
      cam.clippingRange = new UnityEngine.Vector2(0.01f, 1000.0f);
      cam.focalLength = 50;
      cam.focusDistance = 1.0f;
      cam.fStop = 2.5f;
      cam.horizontalAperture = 20.9550f;
      cam.horizontalApertureOffset = 0.001f;
      cam.stereoRole = USD.NET.Unity.CameraSample.StereoRole.Mono;
      cam.verticalAperture = 15.2908f;
      cam.verticalApertureOffset = 0.002f;

      cam.shutter = new USD.NET.Unity.CameraSample.Shutter();
      cam.shutter.open = 0.001;
      cam.shutter.close = 0.002;

      // Prep a new camera sample to be populated.
      var cam2 = new USD.NET.Unity.CameraSample();
      cam2.shutter = new USD.NET.Unity.CameraSample.Shutter();

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

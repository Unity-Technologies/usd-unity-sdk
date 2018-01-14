using System;
using System.Collections.Generic;
using System.Linq;
using pxr;

namespace Tests.Cases {
  class UsdGeomTests {
    public static void CurvesTest() {
      UsdStage stage = UsdStage.CreateInMemory();
      var path = new SdfPath("/Parent/Curves");
      var curvesGprim = UsdGeomBasisCurves.Define(new UsdStageWeakPtr(stage), path);
      var vertCounts = USD.NET.IntrinsicTypeConverter.ToVtArray(new int[] { 4 });
      var basisAttr = curvesGprim.CreateBasisAttr(UsdGeomTokens.bezier);
      curvesGprim.CreateCurveVertexCountsAttr(vertCounts);
    }

  }
}

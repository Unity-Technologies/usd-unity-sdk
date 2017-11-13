using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.Cases {
  class UnityIO : UnitTest {

    public static void TestXform() {
      var sample = new USD.NET.Unity.XformSample();
      var sample2 = new USD.NET.Unity.XformSample();

      sample.transform = UnityEngine.Matrix4x4.identity;

      WriteAndRead(ref sample, ref sample2, true);

      if (sample2.transform != sample.transform) { throw new Exception("Values do not match"); }
      if (sample2.xformOpOrder[0] != sample.xformOpOrder[0]) { throw new Exception("XformOpOrder do not match"); }
    }

    public static void TestXform2() {
      var sample = new USD.NET.Unity.XformSample();
      var sample2 = new USD.NET.Unity.XformSample();

      var mat = new UnityEngine.Matrix4x4();
      for (int i = 0; i < 16; i++) {
        mat[i] = i;
      }
      sample.transform = mat;

      WriteAndRead(ref sample, ref sample2, true);

      AssertEqual(sample2.transform, sample.transform);
      AssertEqual(sample2.xformOpOrder, sample.xformOpOrder);
    }
  }
}

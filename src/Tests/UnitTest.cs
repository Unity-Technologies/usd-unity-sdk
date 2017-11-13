using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests {
  class UnitTest {

    public static void WriteAndRead<T>(ref T inputSample, ref T outputSample, bool printLayer)
          where T : USD.NET.SampleBase
    {
      var scene = USD.NET.Scene.Create();
      scene.Write("/Foo", inputSample);

      if (printLayer) {
        string layer;
        scene.Stage.ExportToString(out layer);
        Console.WriteLine(layer);
      }

      scene.SaveAs("test.usda");
      scene.Close();

      var scene2 = USD.NET.Scene.Open("test.usda");
      scene2.Read("/Foo", outputSample);
      scene2.Close();
    }

    public static void AssertEqual<T>(T[] first, T[] second) {
      if (first.Length != second.Length) {
        throw new Exception("Length of arrays do not match");
      }

      for (int i = 0; i < first.Length; i++) {
        AssertEqual(first[i], second[i]);
      }
    }

    public static void AssertEqual<T>(T first, T second) {
      if (!first.Equals(second)) {
        throw new Exception("Values do not match for " + typeof(T).Name);
      }
    }

  }
}

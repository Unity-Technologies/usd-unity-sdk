// Copyright 2017 Google Inc. All rights reserved.
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests {
  class UnitTest {

    public static void WriteAndRead<T>(ref T inputSample, ref T outputSample, bool printLayer)
          where T : USD.NET.SampleBase
    {
      string filename = GetTempFile();
      var scene = USD.NET.Scene.Create(filename);
      scene.Write("/Foo", inputSample);

      if (printLayer) {
        PrintScene(scene);
      }
      
      scene.Save();
      scene.Close();

      var scene2 = USD.NET.Scene.Open(filename);
      scene2.Read("/Foo", outputSample);
      scene2.Close();

      System.IO.File.Delete(filename);
    }

    public static void TestVariability<T>(T inputSample)
      where T : USD.NET.SampleBase, new() {
      // ----------------------------------------------- //
      // Test with default time --> not varying.
      // ----------------------------------------------- //
      string filename = GetTempFile();
      var scene = USD.NET.Scene.Create(filename);
      var outputSample = new T();
      scene.Time = null;

      scene.Write("/Foo", inputSample);

      scene.Save();
      scene.Close();

      var varMap = new USD.NET.AccessMask();
      var scene2 = USD.NET.Scene.Open(filename);
      scene2.AccessMask = varMap;
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
      scene2.Close();

      AssertTrue(varMap.Included.Count == 0);
      Console.WriteLine("Expected zero dynamic prims and members: success.");

      System.IO.File.Delete(filename);

      // ----------------------------------------------- //
      // Test with time = 1.0 --> not varying.
      // ----------------------------------------------- //
      filename = GetTempFile();
      scene = USD.NET.Scene.Create(filename);
      scene.Time = 1.0;

      scene.Write("/Foo", inputSample);

      scene.Save();
      scene.Close();

      varMap = new USD.NET.AccessMask();
      scene2 = USD.NET.Scene.Open(filename);
      scene2.AccessMask = varMap;
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
      scene2.Close();

      AssertTrue(varMap.Included.Count == 0);
      Console.WriteLine("Expected zero dynamic prims and members: success.");
      System.IO.File.Delete(filename);

      // ----------------------------------------------- //
      // Test with time = 1.0, 2.0 --> varying.
      // ----------------------------------------------- //
      filename = GetTempFile();
      scene = USD.NET.Scene.Create(filename);

      scene.Time = 1.0;
      scene.Write("/Foo", inputSample);

      scene.Time = 2.0;
      scene.Write("/Foo", inputSample);

      scene.Save();
      scene.Close();

      varMap = new USD.NET.AccessMask();
      scene2 = USD.NET.Scene.Open(filename);
      scene2.Time = 1.0;
      scene2.AccessMask = varMap;
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);

      // Still expect nothing dynamic, since varMap was never populated.
      AssertTrue(varMap.Included.Count == 0);
      Console.WriteLine("Expected zero dynamic prims and members: success.");

      scene2.IsPopulatingAccessMask = true;
      scene2.AccessMask = varMap;
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);

      // Extra reads while populating should result in the same map values.
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
      scene2.IsPopulatingAccessMask = false;

      // Reading while not populating should not clear the map.
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
      scene2.Close();

      Console.WriteLine("(Expect only </Foo>)");
      foreach (var pathAndMembers in varMap.Included) {
        Console.WriteLine("  Dynamic Members: " + pathAndMembers.Key);
        foreach (var memberInfo in pathAndMembers.Value) {
          Console.WriteLine("    ." + memberInfo.Name);
        }
      }

      AssertTrue(varMap.Included.Count > 0);
      AssertTrue(varMap.Included[new pxr.SdfPath("/Foo")].Count > 0);

      foreach (var memberInfo in varMap.Included[new pxr.SdfPath("/Foo")]) {
        var fi = memberInfo as System.Reflection.FieldInfo;
        var pi = memberInfo as System.Reflection.PropertyInfo;
        object vIn = null;
        object vOut = null;
        if (fi != null && fi.FieldType.IsClass) {
          vIn = fi.GetValue(inputSample);
          vOut = fi.GetValue(outputSample);
        } else if (pi != null && pi.PropertyType.IsClass) {
          vIn = pi.GetValue(inputSample, null);
          vOut = pi.GetValue(outputSample, null);
        }

        AssertEqual(vIn, vOut);
      }
      System.IO.File.Delete(filename);

      // ----------------------------------------------- //
      // Test that reference values are not populated.
      // ----------------------------------------------- //
      filename = GetTempFile();
      scene = USD.NET.Scene.Create(filename);
      outputSample = new T();

      scene.Time = null;
      scene.Write("/Foo", inputSample);

      // Bar IS time varying.
      scene.Time = 1.0;
      scene.Write("/Foo/Bar", inputSample);
      scene.Time = 2.0;
      scene.Write("/Foo/Bar", inputSample);

      scene.Save();
      scene.Close();

      scene2 = USD.NET.Scene.Open(filename);
      scene2.Time = 1.0;
      varMap = new USD.NET.AccessMask();
      scene2.IsPopulatingAccessMask = true;
      scene2.AccessMask = varMap;
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
      scene2.Read(new pxr.SdfPath("/Foo/Bar"), outputSample);
      scene2.IsPopulatingAccessMask = false;

      // Variability map now has all </Bar> members cached as time-varying.
      outputSample = new T();
      var barSample = new T();
      scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
      scene2.Read(new pxr.SdfPath("/Foo/Bar"), barSample);
      scene2.Close();

      Console.WriteLine("");
      Console.WriteLine("(Expect only </Foo/Bar>)");
      foreach (var pathAndMembers in varMap.Included) {
        Console.WriteLine("  Dynamic Members: " + pathAndMembers.Key);
        foreach (var memberInfo in pathAndMembers.Value) {
          Console.WriteLine("    ." + memberInfo.Name);
        }
      }

      // Assert that all </Foo> values are default.
      var defaultSample = new T();
      AssertTrue(varMap.Included.ContainsKey(new pxr.SdfPath("/Foo")) == false);
      var bindFlags = System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance;
      foreach (var memberInfo in typeof(T).GetMembers(bindFlags)) {
        var fi = memberInfo as System.Reflection.FieldInfo;
        var pi = memberInfo as System.Reflection.PropertyInfo;
        if (fi != null && fi.FieldType.IsClass) {
          AssertEqual(fi.GetValue(outputSample), fi.GetValue(defaultSample));
        } else if (pi != null && pi.PropertyType.IsClass) {
          AssertEqual(pi.GetValue(outputSample, null), fi.GetValue(defaultSample));
        }
      }

      // Assert that all </Bar> values are non-default.
      foreach (var memberInfo in varMap.Included[new pxr.SdfPath("/Foo/Bar")]) {
        var fi = memberInfo as System.Reflection.FieldInfo;
        var pi = memberInfo as System.Reflection.PropertyInfo;
        if (fi != null && fi.FieldType.IsClass) {
          AssertEqual(fi.GetValue(barSample), fi.GetValue(inputSample));
        } else if (pi != null && pi.PropertyType.IsClass) {
          AssertEqual(pi.GetValue(barSample, null), fi.GetValue(inputSample));
        }
      }

      System.IO.File.Delete(filename);
    }

    public static void PrintScene(USD.NET.Scene scene) {
      string layer;
      scene.Stage.ExportToString(out layer, addSourceFileComment: false);
      Console.WriteLine(layer);
    }

    static protected string GetTempFile(string extension = "usd") {
      return System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), extension);
    }

    static protected void AssertEqual<T>(T[] first, T[] second) {
      if (first == null && second == null) {
        return;
      }
      if (first.Length != second.Length) {
        throw new Exception("Length of arrays do not match");
      }

      for (int i = 0; i < first.Length; i++) {
        AssertEqual(first[i], second[i]);
      }
    }

    static protected void AssertEqual(Array first, Array second) {
      if (first == null && second == null) {
        return;
      }
      if (first.Length != second.Length) {
        throw new Exception("Length of arrays do not match");
      }

      for (int i = 0; i < first.Length; i++) {
        AssertEqual(first.GetValue(i), second.GetValue(i));
      }
    }

    static protected void AssertEqual(IList first, IList second) {
      if (first == null && second == null) {
        return;
      }
      if (first.Count != second.Count) {
        throw new Exception("Length of arrays do not match");
      }

      for (int i = 0; i < first.Count; i++) {
        AssertEqual(first[i], second[i]);
      }
    }

    static protected void AssertEqual(IDictionary first, IDictionary second) {
      if (first == null && second == null) {
        return;
      }
      if (first.Count != second.Count) {
        throw new Exception("Length of arrays do not match");
      }

      foreach (System.Collections.DictionaryEntry kvp in first) {
        if (!second.Contains(kvp.Key)) {
          throw new Exception("Key in first not found in second: " + kvp.Key);
        }
        AssertEqual(kvp.Value, second[kvp.Key]);
      }
    }

    static protected void AssertEqual<T>(T first, T second) {
      if (first == null && second == null) {
        return;
      }

      if ((first as IList) != null) {
        AssertEqual(first as IList, second as IList);
      } else if ((first as IDictionary) != null) {
        AssertEqual(first as IDictionary, second as IDictionary);
      } else if ((first as Array) != null) {
        AssertEqual(first as Array, second as Array);
      } else if (!first.Equals(second)) {
        throw new Exception("Values do not match for " + typeof(T).Name);
      }
    }

    static protected void AssertNotEqual<T>(T first, T second) {
      if (first == null && second == null) {
        throw new Exception("Both values are null for " + typeof(T).Name);
      }
      if (first.Equals(second)) {
        throw new Exception("Values do match for " + typeof(T).Name);
      }
    }

    static protected void AssertTrue(bool value) {
      AssertEqual(value, true);
    }

    static protected void AssertFalse(bool value) {
      AssertEqual(value, false);
    }
  }
  }

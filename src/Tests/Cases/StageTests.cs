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

using pxr;

namespace Tests.Cases {
  class StageTests : UnitTest {
    public static void PointerTest() {
      UsdStage s = UsdStage.CreateInMemory();
    }

    public static void MemoryTest() {
      const int kCount = 100;
      for (int i = 0; i < kCount; i++) {
        UsdStage s = UsdStage.CreateInMemory();
        if (s == null) { throw new System.Exception("Init Stage failed");}
        s.Dispose();

        string tmpName = GetTempFile();
        UsdStage ss = UsdStage.CreateNew(tmpName);
        if (ss == null) { throw new System.Exception("Init Stage failed"); }
        ss.Dispose();
        System.IO.File.Delete(tmpName);
      }
    }

    public static void TraverseTest() {
      UsdStage s = UsdStage.CreateInMemory();
      s.DefinePrim(new SdfPath("/Foo"));
      s.DefinePrim(new SdfPath("/Foo/P1"));
      s.DefinePrim(new SdfPath("/Foo/P2"));
      s.DefinePrim(new SdfPath("/Foo/P3"));
      s.DefinePrim(new SdfPath("/Foo/P4"));
      s.DefinePrim(new SdfPath("/Foo/P5"));
      s.DefinePrim(new SdfPath("/Bar"));
      s.DefinePrim(new SdfPath("/Bar/B1"));
      s.DefinePrim(new SdfPath("/Bar/B2"));
      s.DefinePrim(new SdfPath("/Bar/B3"));
      s.DefinePrim(new SdfPath("/Bar/B3/C1"));
      s.DefinePrim(new SdfPath("/Bar/B3/C2"));
      s.DefinePrim(new SdfPath("/Bar/B3/C3"));
      s.DefinePrim(new SdfPath("/Bar/B4"));

      // Basic traversal.
      System.Console.WriteLine("All Prims:");
      foreach (UsdPrim curPrim in s.Traverse()) {
        System.Console.WriteLine(curPrim.GetPath());
      }

      // Make sure pruing logic works.
      System.Console.WriteLine("");
      System.Console.WriteLine("/Bar children pruned:");
      var range = new USD.NET.RangeIterator(s.Traverse());
      foreach (UsdPrim curPrim in range) {
        System.Console.WriteLine(curPrim.GetPath());
        if (curPrim.GetPath() == "/Bar/B3") {
          range.PruneChildren();
          System.Console.WriteLine("pruned.");
        }
      }

      // Make sure pre/post traversal logic works.
      System.Console.WriteLine("");
      System.Console.WriteLine("Pre/Post Traversal with all children pruned:");
      var prePostRange = new USD.NET.RangeIterator(UsdPrimRange.PreAndPostVisit(s.GetPseudoRoot()));
      bool[] expected = { false, false, true, false, true, true };
      bool[] actual = new bool[6];
      int i = 0;
      foreach (UsdPrim curPrim in prePostRange) {
        System.Console.WriteLine("IsPostVisit: " + prePostRange.IsPostVisit().ToString()
                               + ", " + curPrim.GetPath());
        if (!prePostRange.IsPostVisit() && i > 0) {
          // It's only valid to prune on the pre-traversal.
          prePostRange.PruneChildren();
        }

        actual[i++] = prePostRange.IsPostVisit();
      }
      AssertEqual(expected, actual);
    }

    public static void ApiTest() {
      UsdStage s = UsdStage.CreateInMemory();
      var prim = s.DefinePrim(new SdfPath("/Foo"));
      s.SetStartTimeCode(1.0);
      s.SetEndTimeCode(10.0);
      AssertEqual(1.0, s.GetStartTimeCode());
      AssertEqual(10.0, s.GetEndTimeCode());

      var defultPrimToken = new TfToken("defaultPrim");
      var value = new VtValue();

      // Verify initial default prim / built-in metadata states.
      {
        AssertFalse(s.HasDefaultPrim());
        AssertFalse(s.GetDefaultPrim().IsValid());

        // defaultPrim is always valid metadata, it's built-in.
        AssertTrue(s.HasMetadata(defultPrimToken));
        // But it should not yet have an *authored* value.
        AssertFalse(s.HasAuthoredMetadata(defultPrimToken));
        // Even though it's not authored, reading it should succeed.
        AssertTrue(s.GetMetadata(defultPrimToken, value));
        AssertEqual(((TfToken)value).ToString(), "");
      }

      // Set the default prim, which should set the defaultPrim metadata.
      s.SetDefaultPrim(prim);
      {
        // Verify via the Stage API
        AssertTrue(s.HasDefaultPrim());
        AssertTrue(s.GetDefaultPrim().IsValid());
        AssertEqual(s.GetDefaultPrim().GetPath(), new SdfPath("/Foo"));

        // Verify via the metadata API
        AssertTrue(s.HasMetadata(defultPrimToken));
        AssertTrue(s.HasAuthoredMetadata(defultPrimToken));
        AssertTrue(s.GetMetadata(defultPrimToken, value));
        AssertEqual(((TfToken)value).ToString(), "Foo");
      }

      string str = s.GetRootLayer().ExportToString();

      s.ClearDefaultPrim();
      {
        AssertFalse(s.HasDefaultPrim());
        AssertFalse(s.GetDefaultPrim().IsValid());
        AssertFalse(s.HasAuthoredMetadata(defultPrimToken));
        AssertTrue(s.GetMetadata(defultPrimToken, value));
        AssertEqual(((TfToken)value).ToString(), "");
      }

      var classPrim = s.CreateClassPrim(new SdfPath("/Foo_class_"));
      {
        AssertTrue(classPrim.IsValid());
        AssertEqual(classPrim.GetPath(), new SdfPath("/Foo_class_"));
      }

      AssertEqual(s.GetFramesPerSecond(), 24.0);
      s.SetFramesPerSecond(90.0);
      AssertEqual(s.GetFramesPerSecond(), 90.0);
    }
  }
}

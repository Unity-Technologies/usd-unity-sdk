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

namespace Tests {
  class Program {
    // Force static initialization as early as possible.
    //   This is really only required because the system PATH needs to be modified and that must
    //   happen before any other static variables are initialized (which may be using USD).
    static private int sm_unused = Util.UsdInitializer.forceInit;

    static void Main(string[] args) {
      RunTestCases();
    }

    static void RunTestCases() {
      Cases.MeshTests.TestTriangulation();
      Cases.StageTests.PointerTest();
      Cases.StageTests.MemoryTest();
      Cases.StageTests.ApiTest();
      Cases.Basic.SmokeTest();
      Cases.Basic.IntrinsicTypes();
      Cases.UnityIO.TestXform();
      Cases.UnityIO.TestXform2();
      Console.ReadKey();
    }

  }
}

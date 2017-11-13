using System;
using UnityEngine;

namespace Tests {
  class Program {
    // Force static initialization as early as possible.
    //   This is really only required because the system PATH needs to be modified and that must
    //   happen before any other static variables are initialized (which may be using USD).
    static private int sm_dummy = Util.UsdInitializer.forceInit;

    static void Main(string[] args) {
      RunTestCases();
    }

    static void RunTestCases() {
      Cases.Basic.SmokeTest();
      Cases.Basic.IntrinsicTypes();
      Cases.UnityIO.TestXform();
      Cases.UnityIO.TestXform2();
      Console.ReadKey();
    }

  }
}

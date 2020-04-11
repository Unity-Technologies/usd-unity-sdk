// Copyright 2020 Unity Technologies All rights reserved.
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
using System;

namespace Tests.Cases {
  class VerifyAotAttributes : UnitTest {

    private static bool HasMonoCallbackAttr(Type parentType, string methodName, System.Reflection.BindingFlags flags) {
      Console.Write("Checking " + methodName + " ...");
      System.Reflection.MethodInfo callback = parentType.GetMethod(methodName, flags | System.Reflection.BindingFlags.Static);
      AssertNotEqual(callback, null);
      foreach (object attr in callback.GetCustomAttributes(true)) {
        Console.WriteLine("    " + attr.GetType().Name);
        if (attr.GetType().Name == "MonoPInvokeCallbackAttribute") {
          return true;
        }
      }

      return false;
    }

    public static void HasMonoPInvokeCallbackAttrsTest() {
      string exceptionHelperName = "pxr.UsdCsPINVOKE";
      Type pinvoke = System.Reflection.Assembly.GetAssembly(typeof(ArResolverContext)).GetType(exceptionHelperName, throwOnError: true);
      AssertNotEqual(pinvoke, null);

      Type excHelper = pinvoke.GetNestedType("SWIGExceptionHelper", System.Reflection.BindingFlags.NonPublic);
      AssertNotEqual(excHelper, null);

      string[] nonPublicMethods = new string[] {
        "SetPendingApplicationException",
        "SetPendingArithmeticException",
        "SetPendingDivideByZeroException",
        "SetPendingIndexOutOfRangeException",
        "SetPendingInvalidCastException",
        "SetPendingInvalidOperationException",
        "SetPendingIOException",
        "SetPendingNullReferenceException",
        "SetPendingOutOfMemoryException",
        "SetPendingOverflowException",
        "SetPendingSystemException",
        "SetPendingArgumentException",
        "SetPendingArgumentNullException",
        "SetPendingArgumentOutOfRangeException",
      };

      AssertTrue(HasMonoCallbackAttr(excHelper, "SWIGRegisterExceptionCallbacks_UsdCs", System.Reflection.BindingFlags.Public));
      AssertTrue(HasMonoCallbackAttr(excHelper, "SWIGRegisterExceptionCallbacksArgument_UsdCs", System.Reflection.BindingFlags.Public));

      foreach (string methodName in nonPublicMethods) {
        AssertTrue(HasMonoCallbackAttr(excHelper, methodName, System.Reflection.BindingFlags.NonPublic));
      }

      string stringHelperName = "SWIGStringHelper";
      Type stringHelper = pinvoke.GetNestedType(stringHelperName, System.Reflection.BindingFlags.NonPublic);
      AssertTrue(HasMonoCallbackAttr(stringHelper, "CreateString", System.Reflection.BindingFlags.NonPublic));
    }

    private static bool HasPreserveAttribute(System.Reflection.MethodInfo method) {
      Console.Write("Checking " + method.Name + " ...");
      AssertNotEqual(method, null);
      foreach (object attr in method.GetCustomAttributes(true)) {
        Console.WriteLine("    " + attr.GetType().Name);
        if (attr.GetType().Name == "PreserveAttribute") {
          return true;
        }
      }

      return false;
    }

    public static void HasPreserveAttrsTest() {
      Console.WriteLine("Intrinsic Type Converter\n");
      foreach (var method in typeof(USD.NET.IntrinsicTypeConverter).GetMethods()) {
        var name = method.Name;
        if (name.Contains("ToVt") || name.Contains("FromVt")) {
          AssertTrue(HasPreserveAttribute(method));
        }
      }

      Console.WriteLine("\nUnity Type Converter\n");
      foreach (var method in typeof(USD.NET.Unity.UnityTypeConverter).GetMethods()) {
        var name = method.Name;
        if (name.Contains("ToVt") || name.Contains("FromVt")) {
          AssertTrue(HasPreserveAttribute(method));
        }
      }
    }

  }
}

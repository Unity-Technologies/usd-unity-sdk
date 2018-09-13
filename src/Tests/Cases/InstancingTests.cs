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

using System;
using System.Collections.Generic;

namespace Tests.Cases {
  class InstancingTests : UnitTest {
    public static void TraversalTest() {
      var scene = USD.NET.Scene.Open(@"D:\usd\Kitchen_set.usd");
      foreach (pxr.UsdPrim prim in scene.Stage.Traverse()) {

        var mesh = new pxr.UsdGeomMesh(prim);
        if (mesh) {
          Console.WriteLine("Non-instanced mesh: " + prim.GetPath());
        }

        //
        // See if this is instanced.
        //
        if (prim.IsInstance()) {
          Console.WriteLine("Instanced prim: " + prim.GetPath() + "\n\tMaster: " + prim.GetMaster().GetPath());
          
          //
          // Now traverse the master.
          //
          var master = prim.GetMaster();
          foreach (pxr.UsdPrim masterPrim in master.GetAllDescendants()) {
            var masterMesh = new pxr.UsdGeomMesh(masterPrim);
            if (masterMesh) {
              Console.WriteLine("\tMaster mesh: " + masterPrim.GetPath());
            }
          }
        }
      }
    }
  }
}

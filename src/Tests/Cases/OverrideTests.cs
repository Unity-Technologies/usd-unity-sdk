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
using USD.NET;
using USD.NET.Unity;

namespace Tests.Cases {
  class OverrideTests : UnitTest {

    public static void WriteToOverTest() {

      var cubeSample = new CubeSample();

      var sceneUnder = Scene.Create();
      sceneUnder.UpAxis = Scene.UpAxes.Z;

      var sceneOver = Scene.Create();
      sceneOver.UpAxis = Scene.UpAxes.Z;

      sceneUnder.AddSubLayer(sceneOver);

      cubeSample.size = 1.1;
      sceneUnder.Write("/Cube", cubeSample);

      sceneOver.WriteMode = Scene.WriteModes.Over;
      cubeSample.size = 2.2;
      sceneOver.Write("/Cube", cubeSample);

      PrintScene(sceneUnder);
      PrintScene(sceneOver);
    }

    public static void WriteToUnderTest() {

      // Create the base scene layer.
      var strongerLayer = Scene.Create("D:\\stronger.usda");
      strongerLayer.UpAxis = Scene.UpAxes.Z;

      // Create a layer for overrides.
      var weakerLayer = Scene.Create("D:\\weaker.usda");
      weakerLayer.UpAxis = Scene.UpAxes.Z;

      // Create a cube for testing.
      var cubeSample = new CubeSample();

      // Add the subLayer to the main layer.
      strongerLayer.AddSubLayer(weakerLayer);

      // Write some data to the cube.
      // Set the WriteMode to "Over" and write a different /Cube value.
      strongerLayer.WriteMode = Scene.WriteModes.Over;
      cubeSample.size = 1.1;
      strongerLayer.Write("/Cube", cubeSample);

      // Change the edit target.
      strongerLayer.SetEditTarget(weakerLayer);

      // Set the WriteMode to "Define" and write a different /Cube value.
      strongerLayer.WriteMode = Scene.WriteModes.Define;
      cubeSample.size = 2.2;
      strongerLayer.Write("/Cube", cubeSample);

      // Save.
      strongerLayer.Save();
      weakerLayer.Save();
    }


    public static void WriteOverOnlyTest() {

      // Create the base scene layer.
      var strongerLayer = Scene.Create("D:\\stronger-no-sublayer.usda");
      strongerLayer.UpAxis = Scene.UpAxes.Z;

      // Create a cube for testing.
      var cubeSample = new CubeSample();

      // Write some data to the cube.
      // Set the WriteMode to "Over" and write a different /Cube value.
      strongerLayer.WriteMode = Scene.WriteModes.Over;
      cubeSample.size = 1.1;
      strongerLayer.Write("/Cube", cubeSample);

      // Save.
      strongerLayer.Save();
    }

  }
}

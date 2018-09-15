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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USD.NET.Examples {

  public class HelloUsdExample : MonoBehaviour {
    [System.Serializable]
    class MyCustomData : USD.NET.SampleBase {
      public string aString;
      public int[] anArrayOfInts;
      public UnityEngine.Bounds aBoundingBox;
    }

    void Start() {
      InitUsd.Initialize();
      Test();
    }

    void Test() {
      string usdFile = System.IO.Path.Combine(UnityEngine.Application.dataPath, "sceneFile.usda");

      // Populate Values.
      var value = new MyCustomData();
      value.aString = "It Worked!";
      value.anArrayOfInts = new int[] { 1, 2, 3, 4 };
      value.aBoundingBox = new UnityEngine.Bounds();

      // Writing the value.
      var scene = USD.NET.Scene.Create(usdFile);
      scene.Time = 1.0;
      scene.Write("/someValue", value);
      Debug.Log(scene.Stage.GetRootLayer().ExportToString());
      scene.Save();
      scene.Close();

      // Reading the value.
      Debug.Log(usdFile);
      value = new MyCustomData();
      scene = USD.NET.Scene.Open(usdFile);
      scene.Time = 1.0;
      scene.Read("/someValue", value);

      Debug.LogFormat("Value: string={0}, ints={1}, bbox={2}",
      value.aString, value.anArrayOfInts, value.aBoundingBox);

      var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath("/someValue"));
      var vtValue = prim.GetAttribute(new pxr.TfToken("aString")).Get(1);
      Debug.Log((string)vtValue);

      scene.Close();
    }
  }

}
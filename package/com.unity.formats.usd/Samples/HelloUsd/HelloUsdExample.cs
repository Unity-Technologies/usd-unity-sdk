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

using UnityEditor;
using UnityEngine;
using USD.NET;
using Unity.Formats.USD;

namespace Unity.Formats.USD.Examples
{
    public class HelloUsdExample : MonoBehaviour
    {
        [System.Serializable]
        class MyCustomData : SampleBase
        {
            public string aString;
            public int[] anArrayOfInts;
            public Bounds aBoundingBox;
        }

        public void DoHelloUsdExampleDemo()
        {
#if UNITY_EDITOR
            SampleUtils.FocusConsoleWindow();
#endif

            InitUsd.Initialize();
            Debug.Log($"<color=#00FF00>Starting HelloUsd Test Example...</color>");
            Test();
            Debug.Log($"<color=#FF2D2D>HelloUsd Test Example Finished</color>");
        }

        void Test()
        {
            string usdFile = System.IO.Path.Combine(UnityEngine.Application.dataPath, "sceneFile.usda");

            // Create and Populate CustomData Values.
            var value = new MyCustomData();
            value.aString = "Custom string value!";
            value.anArrayOfInts = new int[] { 1, 2, 3, 4 };
            value.aBoundingBox = new UnityEngine.Bounds();

            // Writing the value.
            var scene = Scene.Create(usdFile);
            scene.Time = 1.0;
            Debug.Log($"<color=#338DFF>Writing data to: <{usdFile}> with values: </color>");
            Debug.LogFormat("Value: string={0}, ints=[{1}, {2}, {3}, {4}], bounding box={5}",
                value.aString, value.anArrayOfInts[0], value.anArrayOfInts[1], value.anArrayOfInts[2], value.anArrayOfInts[3], value.aBoundingBox);
            scene.Write("/someCustomValue", value);
            Debug.Log(scene.Stage.GetRootLayer().ExportToString());
            scene.Save();
            scene.Close();

            // Reading the value.
            Debug.Log($"<color=#FF2090>Reading data from: <{usdFile}> values: </color>");
            value = new MyCustomData();
            scene = Scene.Open(usdFile);
            scene.Time = 1.0;
            scene.Read("/someCustomValue", value);

            Debug.LogFormat("Value: string={0}, ints=[{1}, {2}, {3}, {4}], bounding box={5}",
                value.aString, value.anArrayOfInts[0], value.anArrayOfInts[1], value.anArrayOfInts[2], value.anArrayOfInts[3], value.aBoundingBox);

            var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath("/someCustomValue"));
            var vtValue = prim.GetAttribute(new pxr.TfToken("aString")).Get(1);
            Debug.Log($"(string)VTValue of added 'Custom Value', TfToken 'aString': <color=#FFE92D><{(string)vtValue}></color>");

            scene.Close();
        }
    }
}

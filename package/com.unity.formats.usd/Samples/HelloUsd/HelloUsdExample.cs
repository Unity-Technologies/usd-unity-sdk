// Copyright 2023 Unity Technologies. All rights reserved.
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
        static Scene m_scene;
        const string k_exampleUsdFileName = "HelloUsdExample.usda";

        [System.Serializable]
        class MyCustomData : SampleBase
        {
            public string aString;
            public int[] anArrayOfInts;
            public Bounds aBoundingBox;
        }

        public void InitializeUsd()
        {
            // Before doing any USD related actions on Unity, Initialization is required.
            // This USD Package requires several auxilliary C++ plugin discovery files to be discoverable at run-time.
            // The initialization stores these libs in Support/ThirdParty/Usd and then set a magic environment variable to let
            // USD's libPlug know where to look to find them.
            // Once initialized, Initialization will ignore all subsequent Initialization calls until the USD Package is re-compiled by Unity
            InitUsd.Initialize();
        }

        public void CreateUsdScene()
        {
            string usdFile = System.IO.Path.Combine(SampleUtils.SampleArtifactDirectory, k_exampleUsdFileName);
            Debug.Log($"<color={SampleUtils.TextColor.Blue}>Creating empty USD file called: <{k_exampleUsdFileName}> within your project's '{SampleUtils.SampleArtifactRelativeDirectory}' folder</color>");
            m_scene = Scene.Create(usdFile);
        }

        public void AddCustomDataToScene()
        {
            // Create and Populate CustomData Values.
            var value = new MyCustomData();

            // Your custom data can contain C# Built-in types.
            value.aString = "Hello USD!";
            value.anArrayOfInts = new int[] { 1, 2, 3, 4 };

            // As well as UnityEngine types.
            value.aBoundingBox = new UnityEngine.Bounds();

            Debug.Log($"<color={SampleUtils.TextColor.Blue}>Writing data to: <{k_exampleUsdFileName}> with values: </color>");
            Debug.LogFormat("Value: string={0}, ints=[{1}, {2}, {3}, {4}], bounding box={5}",
                value.aString, value.anArrayOfInts[0], value.anArrayOfInts[1], value.anArrayOfInts[2], value.anArrayOfInts[3], value.aBoundingBox);
            m_scene.Write("/someCustomValue", value); // Here "/someCustomvalue" is the path of the USD Prim the custom data, 'value' will be placed in
        }

        public void SaveDataInScene()
        {
            // Once any data is written in a USD Scene file, you need to save it to preserve it
            m_scene.Save();

            Debug.Log($"<color={SampleUtils.TextColor.Green}>Custom data has been saved in USD file named <{k_exampleUsdFileName}> within your project's '{SampleUtils.SampleArtifactRelativeDirectory}' folder</color>");

            AssetDatabase.Refresh();
        }

        public void OpenScene()
        {
            string usdFile = System.IO.Path.Combine(SampleUtils.SampleArtifactDirectory, k_exampleUsdFileName);
            m_scene = Scene.Open(usdFile);
        }

        public void ReadCustomDataFromScene()
        {
            var value = new MyCustomData();

            Debug.Log($"<color={SampleUtils.TextColor.Blue}>Reading data from: <{k_exampleUsdFileName}> values: </color>");
            // Reading the added custom data.
            m_scene.Read("/someCustomValue", value);
            Debug.LogFormat("Value: string={0}, ints=[{1}, {2}, {3}, {4}], bounding box={5}",
                value.aString, value.anArrayOfInts[0], value.anArrayOfInts[1], value.anArrayOfInts[2], value.anArrayOfInts[3], value.aBoundingBox);

            var prim = m_scene.Stage.GetPrimAtPath(new pxr.SdfPath("/someCustomValue"));
            var vtValue = prim.GetAttribute(new pxr.TfToken("aString")).Get(1);
            Debug.Log($"(string)VTValue of added 'Custom Value', TfToken 'aString': <color={SampleUtils.TextColor.Blue}><{(string)vtValue}></color>");

        }

        public void CloseScene()
        {
            // USD Scenes must be closed once you are done operating on it
            m_scene.Close();
            m_scene = null;
        }
    }
}

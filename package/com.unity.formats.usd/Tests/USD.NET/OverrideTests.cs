// Copyright 2018 Jeremy Cowles. All rights reserved.
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

using NUnit.Framework;
using pxr;
using Unity.Formats.USD;

namespace USD.NET.Tests
{
    class OverrideTests : UsdTests
    {
        class MinimalSample : USD.NET.SampleBase
        {
            public int index;
            public float value;
        }

        [Test]
        public static void WriteToOverTest()
        {
            var sample = new MinimalSample();

            var sceneUnder = Scene.Create();
            sceneUnder.UpAxis = Scene.UpAxes.Z;

            var sceneOver = Scene.Create();
            sceneOver.UpAxis = Scene.UpAxes.Z;

            sceneUnder.AddSubLayer(sceneOver);

            sample.index = 1;
            sceneUnder.Write("/Foo", sample);

            sceneOver.WriteMode = Scene.WriteModes.Over;
            sample.index = 3;
            sample.value = 13.0f;
            sceneOver.Write("/Foo", sample);

            var foo = sceneUnder.GetPrimAtPath("/Foo");
            Assert.AreEqual(1, (int)foo.GetAttribute(new TfToken("index")).Get());
            Assert.AreEqual(0.0f, (float)foo.GetAttribute(new TfToken("value")).Get());
        }

        [Test]
        public void WriteToUnderTest()
        {
            // Create the base scene layer.
            var strongerLayerPath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath, "stronger.usda");
            var strongerLayer = Scene.Open(strongerLayerPath);
            strongerLayer.UpAxis = Scene.UpAxes.Z;

            // Create a layer for overrides.
            var weakerLayerPath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath, "weaker.usda");
            var weakerLayer = Scene.Open(weakerLayerPath);
            weakerLayer.UpAxis = Scene.UpAxes.Z;

            // Create a sample for testing.
            var sample = new MinimalSample();

            // Add the subLayer to the main layer.
            strongerLayer.AddSubLayer(weakerLayer);

            // Write some data to the Foo
            // Set the WriteMode to "Over" and write a different /Foo value.
            strongerLayer.WriteMode = Scene.WriteModes.Over;
            sample.value = 1.1f;
            strongerLayer.Write("/Foo", sample);

            // Change the edit target.
            strongerLayer.SetEditTarget(weakerLayer);

            // Set the WriteMode to "Define" and write a different /Foo value.
            strongerLayer.WriteMode = Scene.WriteModes.Define;
            sample.value = 2.2f;
            strongerLayer.Write("/Foo", sample);

            var foo = strongerLayer.GetPrimAtPath("/Foo");

            var valueAttr = foo.GetAttribute(new TfToken("value"));
            Assert.AreEqual(1.1f, (float)valueAttr.Get());

            strongerLayer.Close();
            weakerLayer.Close();
        }

        [Test]
        public void WriteOverOnlyTest()
        {
            // Create the base scene layer.
            var strongerLayerPath = TestUtility.CreateTmpUsdFile(ArtifactsDirectoryFullPath, "stronger.usda");
            var strongerLayer = Scene.Open(strongerLayerPath);
            strongerLayer.UpAxis = Scene.UpAxes.Z;

            // Create a sample for testing.
            var sample = new MinimalSample();

            // Write some data to the prim.
            // Set the WriteMode to "Over" and write a different value.
            strongerLayer.WriteMode = Scene.WriteModes.Over;
            sample.value = 1.1f;
            strongerLayer.Write("/Foo", sample);

            // Save.
            // strongerLayer.Save();
            var prim = strongerLayer.GetPrimAtPath("/Foo");
            var v = prim.GetAttribute(new TfToken("value"));
            Assert.AreEqual(1.1f, (float)v.Get());

            strongerLayer.Close();
        }
    }
}

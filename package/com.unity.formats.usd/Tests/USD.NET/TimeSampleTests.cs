// Copyright 2018 Google Inc. All rights reserved.
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

using System.Collections.Generic;
using NUnit.Framework;

namespace USD.NET.Tests
{
    class TimeSampleTests : UsdTests
    {
        public class KeyFramesTestSample : SampleBase
        {
            public int intValue;

            [UsdVariability(Variability.Uniform)] public int uniformValue;
        }

        [Test]
        public static void ComputeKeyFramesTest()
        {
            var scene = Scene.Create();
            var s1 = new KeyFramesTestSample();
            var baseline = new Dictionary<string, double[]>();
            baseline["/Foo"] = new double[] { 1.0, 2.0, 3.0 };
            baseline["/Foo/Bar"] = new double[] { 5.0, 6.0, 7.0 };
            baseline["/Baz"] = new double[] { 1.0, 2.0, 3.0 };

            foreach (var kvp in baseline)
            {
                string path = kvp.Key;
                double[] times = kvp.Value;

                foreach (double time in times)
                {
                    s1.intValue = (int)time;
                    s1.uniformValue = (int)time;
                    scene.Time = time;
                    scene.Write(path, s1);
                }
            }

            var dict = scene.ComputeKeyFrames("/", "intValue");
            AssertEqual(baseline, dict);

            // Filter just on /Foo and descendants, so /Baz should be excluded.
            dict = scene.ComputeKeyFrames("/Foo", "intValue");
            baseline.Remove("/Baz");
            AssertEqual(baseline, dict);

            // Check uniform values, which should have no time samples.
            dict = scene.ComputeKeyFrames("/", "uniformValue");
            baseline.Clear();
            AssertEqual(baseline, dict);

            scene.Close();
        }
    }
}

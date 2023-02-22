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

using UnityEngine;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public static class CameraExporter
    {
        public static void ExportCamera(ObjectContext objContext, ExportContext exportContext)
        {
            UnityEngine.Profiling.Profiler.BeginSample("USD: Camera Conversion");

            CameraSample sample = (CameraSample)objContext.sample;
            Camera camera = objContext.gameObject.GetComponent<Camera>();
            var path = objContext.path;
            var scene = exportContext.scene;
            bool fastConvert = exportContext.basisTransform == BasisTransformation.FastWithNegativeScale;

            // If doing a fast conversion, do not let the constructor do the change of basis for us.
            sample.CopyFromCamera(camera, convertTransformToUsd: !fastConvert);

            if (fastConvert)
            {
                // Partial change of basis.
                var basisChange = Matrix4x4.identity;
                // Invert the forward vector.
                basisChange[2, 2] = -1;
                // Full change of basis would be b*t*b-1, but here we're placing only a single inversion
                // at the root of the hierarchy, so all we need to do is get the camera into the same
                // space.
                sample.transform = sample.transform * basisChange;

                // Is this also a root path?
                // If so the partial basis conversion must be completed on the camera itself.
                if (path.LastIndexOf("/") == 0)
                {
                    sample.transform = basisChange * sample.transform;
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("USD: Camera Write");
            scene.Write(path, sample);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}

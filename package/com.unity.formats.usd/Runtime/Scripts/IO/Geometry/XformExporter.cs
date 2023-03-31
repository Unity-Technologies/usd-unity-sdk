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
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public static class XformExporter
    {
        public static void ExportXform(ObjectContext objContext, ExportContext exportContext)
        {
            UnityEngine.Profiling.Profiler.BeginSample("USD: Xform Conversion");

            XformSample sample = (XformSample)objContext.sample;
            var path = new pxr.SdfPath(objContext.path);

            // If exporting for Z-Up, rotate the world.
            bool correctZUp = exportContext.scene.UpAxis == Scene.UpAxes.Z;

            sample.transform = GetLocalTransformMatrix(
                objContext.gameObject.transform,
                correctZUp,
                path.IsRootPrimPath(),
                exportContext.basisTransform);
            UnityEngine.Profiling.Profiler.EndSample();

            // If exporting overrides, only export what changed
            if (exportContext.exportTransformOverrides)
            {
                UnityEngine.Profiling.Profiler.BeginSample("USD: Xform override check");
                var sourceSample = new XformSample();
                float tolerance = 0.0001f;
                exportContext.scene.Read(path, sourceSample);
                bool areClose = true;
                for (int i = 0; i < 16; i++)
                {
                    if (Mathf.Abs(sample.transform[i] - sourceSample.transform[i]) > tolerance)
                    {
                        areClose = false;
                        break;
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();
                if (areClose)
                {
                    return;
                }
            }


            UnityEngine.Profiling.Profiler.BeginSample("USD: Xform Write");
            exportContext.scene.Write(objContext.path, sample);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public static void WriteSparseOverrides(Scene scene,
            PrimMap primMap,
            BasisTransformation changeHandedness,
            float tolerance = 0.0001f)
        {
            var oldMode = scene.WriteMode;
            scene.WriteMode = Scene.WriteModes.Over;

            try
            {
                foreach (var path in scene.Find<XformSample>())
                {
                    GameObject go;
                    if (!primMap.TryGetValue(path, out go))
                    {
                        continue;
                    }

                    var tx = go.transform;
                    var xfNew = XformSample.FromTransform(tx);
                    var xfOld = new XformSample();

                    scene.Read(path, xfOld);

                    bool areClose = true;
                    for (int i = 0; i < 16; i++)
                    {
                        if (Mathf.Abs(xfNew.transform[i] - xfOld.transform[i]) > tolerance)
                        {
                            areClose = false;
                            break;
                        }
                    }

                    if (areClose)
                    {
                        continue;
                    }

                    if (changeHandedness == BasisTransformation.SlowAndSafe)
                    {
                        xfNew.ConvertTransform();
                    }

                    scene.Write(path, xfNew);
                }
            }
            finally
            {
                scene.WriteMode = oldMode;
            }
        }

        public static Matrix4x4 GetLocalTransformMatrix(
            Transform tr,
            bool correctZUp,
            bool isRootPrim,
            BasisTransformation conversionType)
        {
            var localRot = tr.localRotation;
            bool fastConvert = conversionType == BasisTransformation.FastWithNegativeScale;

            if (correctZUp && isRootPrim)
            {
                float invert = fastConvert ? 1 : -1;
                localRot = localRot * Quaternion.AngleAxis(invert * 90, Vector3.right);
            }

            var mat = Matrix4x4.TRS(tr.localPosition, localRot, tr.localScale);

            // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
            // basis is required. There are shortcuts, but this is fully general.
            //
            // Here we can either put a partial conversion at the root (fast & dangerous) or convert the
            // entire hierarchy, along with the points, normals and triangle winding. The benefit of the
            // full conversion is that there are no negative scales left in the hierarchy.
            //
            // Note that this is the correct partial conversion for the root transforms, however the
            // camera and light matrices must contain the other half of the conversion
            // (e.g. mat * basisChangeInverse).
            if (fastConvert && isRootPrim)
            {
                // Partial change of basis.
                var basisChange = Matrix4x4.identity;
                // Invert the forward vector.
                basisChange[2, 2] = -1;
                mat = basisChange * mat;
            }
            else if (!fastConvert)
            {
                // Full change of basis.
                mat = UnityTypeConverter.ChangeBasis(mat);
            }

            return mat;
        }
    }
}

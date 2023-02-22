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
    /// <summary>
    /// A collection of methods used for importing USD Xform data into Unity.
    /// </summary>
    public static class XformImporter
    {
        #region "Import API"

        /// <summary>
        /// Copies the transform value from USD to Unity, optionally changing handedness in the
        /// process.
        /// </summary>
        public static void BuildXform(pxr.SdfPath path,
            XformableSample usdXf,
            GameObject go,
            SceneImportOptions options,
            Scene scene)
        {
            // If there is an access mask and it's not initially being populated, check to see if the
            // transform for this object actually varies over time, if not, we can simply return.
            if (scene.AccessMask != null && !scene.IsPopulatingAccessMask)
            {
                System.Reflection.MemberInfo transformMember = null;
                transformMember = usdXf.GetType().GetMember("transform")[0];
                DeserializationContext deserializationContext;
                if (!scene.AccessMask.Included.TryGetValue(path, out deserializationContext) ||
                    !deserializationContext.dynamicMembers.Contains(transformMember))
                {
                    return;
                }
            }

            BuildXform(usdXf.transform, go, options);
        }

        public static void BuildXform(Matrix4x4 xf,
            GameObject go,
            SceneImportOptions options)
        {
            Vector3 localPos;
            Quaternion localRot;
            Vector3 localScale;

            UnityEngine.Profiling.Profiler.BeginSample("Decompose Matrix");
            bool success = UnityTypeConverter.Decompose(xf, out localPos, out localRot, out localScale);
            UnityEngine.Profiling.Profiler.EndSample();

            if (!success)
            {
                Debug.LogError("Non-decomposable transform matrix for " + go.name);
                return;
            }

            UnityEngine.Profiling.Profiler.BeginSample("Assign Values");
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.transform.localRotation = localRot;
            UnityEngine.Profiling.Profiler.EndSample();
        }

        /// <summary>
        /// Imports a matrix transform, correctly handling the change of basis.
        /// </summary>
        public static void ImportXform(ref Matrix4x4 mat, SceneImportOptions options)
        {
            if (options.changeHandedness == BasisTransformation.FastWithNegativeScale)
            {
                return;
            }

            mat = UnityTypeConverter.ChangeBasis(mat);
        }

        /// <summary>
        /// Build the root of a scene under which more USD data will be imported. If the handedness
        /// is changed here, no subsequent changes are required below, however the root will contain
        /// a negative scale.
        /// </summary>
        public static void BuildSceneRoot(Scene scene, Transform root, SceneImportOptions options)
        {
            var stageRoot = root.GetComponent<UsdAsset>();
            bool newStageRoot = false;

            if (stageRoot == null)
            {
                stageRoot = root.gameObject.AddComponent<UsdAsset>();
                stageRoot.usdFullPath = scene.FilePath;
                newStageRoot = true;
                ImporterBase.MoveComponentFirst(stageRoot);
                stageRoot.OptionsToState(options);
            }

            if (newStageRoot
                || options.changeHandedness != stageRoot.LastHandedness
                || options.scale != stageRoot.LastScale
                || options.forceRebuild)
            {
                var localScale = root.transform.localScale;
                var localRotation = root.transform.localRotation;

                if (options.forceRebuild)
                {
                    localScale = Vector3.one;
                    localRotation = Quaternion.identity;
                }
                else if (!newStageRoot)
                {
                    // Undo the previous transforms.
                    UndoRootTransform(scene, stageRoot, ref localScale, ref localRotation);
                }

                stageRoot.LastScale = options.scale;
                stageRoot.LastHandedness = options.changeHandedness;

                // Handle configurable up-axis (Y or Z).
                float invert = options.changeHandedness == BasisTransformation.FastWithNegativeScale ? -1 : 1;
                if (scene.UpAxis == Scene.UpAxes.Z)
                {
                    localRotation *= Quaternion.AngleAxis(invert * 90, Vector3.right);
                }

                if (options.changeHandedness == BasisTransformation.FastWithNegativeScale)
                {
                    // Convert from right-handed (USD) to left-handed (Unity).
                    if (scene.UpAxis == Scene.UpAxes.Z)
                    {
                        localScale.y *= -1;
                    }
                    else
                    {
                        localScale.z *= -1;
                    }
                }

                if (Mathf.Abs(options.scale - 1.0f) > 0.0001)
                {
                    // Unilaterally setting the scale here is a little wrong, since it will stomp the root
                    // object scale if set in Unity.
                    localScale *= options.scale;
                }

                root.transform.localScale = localScale;
                root.transform.localRotation = localRotation;
            }
        }

        public static void UndoRootTransform(Scene scene,
            UsdAsset stageRoot,
            ref Vector3 localScale,
            ref Quaternion localRotation)
        {
            localScale /= stageRoot.LastScale;

            float invertPrev = stageRoot.LastHandedness == BasisTransformation.FastWithNegativeScale ? -1 : 1;
            if (scene.UpAxis == Scene.UpAxes.Z)
            {
                localRotation *= Quaternion.AngleAxis(-1 * invertPrev * 90, Vector3.right);
            }

            if (stageRoot.LastHandedness == BasisTransformation.FastWithNegativeScale)
            {
                // Convert from right-handed (USD) to left-handed (Unity).
                if (scene.UpAxis == Scene.UpAxes.Z)
                {
                    localScale.y *= -1;
                }
                else
                {
                    localScale.z *= -1;
                }
            }
        }

        #endregion
    }
}

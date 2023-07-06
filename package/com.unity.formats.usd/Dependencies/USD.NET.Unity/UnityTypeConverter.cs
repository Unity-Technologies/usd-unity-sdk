// Copyright 2021 Unity Technologies. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using pxr;
using UnityEngine;

namespace USD.NET.Unity
{
    /// <summary>
    /// Prevent a method, class, field, or property from being stripped by bytecode optimization.
    /// </summary>
    /// <remarks>
    /// When IL2CPP optimizes the generated IL, unused code will be stripped. Any methods only called by reflection will
    /// also be stripped in this way. To prevent this, this attribute is copied from Unity.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    class PreserveAttribute : Attribute
    {
    }

    public class UnityTypeConverter : IntrinsicTypeConverter
    {
        /// <summary>
        /// Configurable matrix used for change of basis from USD to Unity and vice versa (change handedness).
        /// </summary>
        /// <remarks>
        /// Allows global configuration of the change of basis matrix, which e.g. is used to make the USD importer conform
        /// to the legacy FBX import convention in Unity, swapping the X-axis instead of the Z-axis.
        /// By default this matrix is set to change handedness by swapping the Z-axis.
        /// </remarks>
        public static UnityEngine.Matrix4x4 basisChange = UnityEngine.Matrix4x4.Scale(new UnityEngine.Vector3(1, 1, -1));
        public static UnityEngine.Matrix4x4 inverseBasisChange = UnityEngine.Matrix4x4.Scale(new UnityEngine.Vector3(1, 1, -1));

        /// <summary>
        /// Converts to and from the USD transform space.
        /// This method should be applied to all Unity matrices before being written and all USD
        /// matrices after being read, unless the USD file is stored in the Unity transform space
        /// (though doing so will result in a non-standard USD file).
        /// </summary>
        static public UnityEngine.Matrix4x4 ChangeBasis(UnityEngine.Matrix4x4 input)
        {
            // Furthermore, this could be simplified to multiplying -1 by input elements [2,6,8,9,11,14].
            return UnityTypeConverter.basisChange * input * UnityTypeConverter.inverseBasisChange;
        }

        public static UnityEngine.Vector3 ChangeBasis(UnityEngine.Vector3 point)
        {
            return UnityTypeConverter.basisChange.MultiplyPoint3x4(point);
        }

        /// <summary>
        /// Sets the local transform matrix on the given Unity Transform given a Matrix4x4.
        /// </summary>
        static public void SetTransform(UnityEngine.Matrix4x4 localXf,
            UnityEngine.Transform transform)
        {
            var T = new UnityEngine.Vector3();
            var S = new UnityEngine.Vector3();
            var R = new UnityEngine.Quaternion();

            Decompose(localXf, out T, out R, out S);

            transform.localPosition = T;
            transform.localRotation = R;
            transform.localScale = S;
        }

        /// <summary>
        /// Decompose the given matrix into translation, rotation, and scale, accounting for potential
        /// handedness changes in the matrix. Returns false if the matrix is singular.
        /// </summary>
        /// <remarks>
        /// Note that for a change of handedness, all scales will invert and a corrective rotation will
        /// be aded, which will not match the original TSR values, but will be correct in terms of
        /// orientation, position and scale.
        /// </remarks>
        public static bool Decompose(
            UnityEngine.Matrix4x4 matrix,
            out UnityEngine.Vector3 translation,
            out UnityEngine.Quaternion rotation,
            out UnityEngine.Vector3 scale)
        {
            // PERFORMANCE: Move this into C++.

            translation = new UnityEngine.Vector3();
            rotation = new UnityEngine.Quaternion();
            scale = new UnityEngine.Vector3();

            if (matrix[3, 3] == 0.0f)
            {
                return false;
            }

            // Normalize the matrix.
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    matrix[i, j] /= matrix[3, 3];
                }
            }

            // perspectiveMatrix is used to solve for perspective, but it also provides
            // an easy way to test for singularity of the upper 3x3 component.
            UnityEngine.Matrix4x4 persp = matrix;

            for (int i = 0; i < 3; i++)
            {
                persp[3, i] = 0;
            }
            persp[3, 3] = 1;

            if (persp.determinant == 0.0f)
            {
                return false;
            }

            // Next take care of translation (easy).
            translation = new UnityEngine.Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
            matrix[3, 0] = 0;
            matrix[3, 1] = 0;
            matrix[3, 2] = 0;

            UnityEngine.Vector3[] rows = new UnityEngine.Vector3[3];
            UnityEngine.Vector3 Pdum3;

            // Now get scale and shear.
            for (int i = 0; i < 3; ++i)
            {
                rows[i].x = matrix[0, i];
                rows[i].y = matrix[1, i];
                rows[i].z = matrix[2, i];
            }

            // Compute X scale factor and normalize first row.
            scale.x = rows[0].magnitude;
            rows[0] = rows[0].normalized;

            // Compute XY shear factor and make 2nd row orthogonal to 1st.
            UnityEngine.Vector3 Skew;
            Skew.z = UnityEngine.Vector3.Dot(rows[0], rows[1]);
            rows[1] = WeightedAvg(rows[1], rows[0], 1, -Skew.z);

            // Now, compute Y scale and normalize 2nd row.
            scale.y = rows[1].magnitude;
            rows[1] = rows[1].normalized;

            // Compute XZ and YZ shears, orthogonalize 3rd row.
            Skew.y = UnityEngine.Vector3.Dot(rows[0], rows[2]);
            rows[2] = WeightedAvg(rows[2], rows[0], 1, -Skew.y);

            Skew.x = UnityEngine.Vector3.Dot(rows[1], rows[2]);
            rows[2] = WeightedAvg(rows[2], rows[1], 1, -Skew.x);

            // Next, get Z scale and normalize 3rd row.
            scale.z = rows[2].magnitude;
            rows[2] = rows[2].normalized;

            // At this point, the matrix (in rows[]) is orthonormal.
            // Check for a coordinate system flip.  If the determinant
            // is -1, then negate the matrix and the scaling factors.
            Pdum3 = UnityEngine.Vector3.Cross(rows[1], rows[2]);
            if (UnityEngine.Vector3.Dot(rows[0], Pdum3) < 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    scale[i] *= -1;
                    rows[i] *= -1;
                }
            }

            // Now, get the rotations out, as described in the gem.

#if false
            // Euler Angles.
            rotation.y = UnityEngine.Mathf.Asin(-rows[0][2]);
            if (Mathf.Cos(rotation.y) != 0)
            {
                rotation.x = UnityEngine.Mathf.Atan2(rows[1][2], rows[2][2]);
                rotation.z = UnityEngine.Mathf.Atan2(rows[0][1], rows[0][0]);
            }
            else
            {
                rotation.x = UnityEngine.Mathf.Atan2(-rows[2][0], rows[1][1]);
                rotation.z = 0;
            }
#else
            // Quaternions.
            {
                int i, j, k = 0;
                float root, trace = rows[0].x + rows[1].y + rows[2].z;
                if (trace > 0)
                {
                    root = UnityEngine.Mathf.Sqrt(trace + 1.0f);
                    rotation.w = 0.5f * root;
                    root = 0.5f / root;
                    rotation.x = root * (rows[1].z - rows[2].y);
                    rotation.y = root * (rows[2].x - rows[0].z);
                    rotation.z = root * (rows[0].y - rows[1].x);
                } // End if > 0
                else
                {
                    int[] Next = new int[] { 1, 2, 0 };
                    i = 0;
                    if (rows[1].y > rows[0].x) i = 1;
                    if (rows[2].z > rows[i][i]) i = 2;
                    j = Next[i];
                    k = Next[j];

                    root = UnityEngine.Mathf.Sqrt(rows[i][i] - rows[j][j] - rows[k][k] + 1.0f);

                    rotation[i] = 0.5f * root;
                    root = 0.5f / root;
                    rotation[j] = root * (rows[i][j] + rows[j][i]);
                    rotation[k] = root * (rows[i][k] + rows[k][i]);
                    rotation.w = root * (rows[j][k] - rows[k][j]);
                } // End if <= 0
            }
#endif
            return true;
        }

        private static UnityEngine.Vector3 WeightedAvg(
            UnityEngine.Vector3 a,
            UnityEngine.Vector3 b,
            float aWeight,
            float bWeight)
        {
            return (a * aWeight) + (b * bWeight);
        }

        /// <summary>
        /// Extracts the local position, rotation and scale from the given Matrix4x4.
        /// </summary>
        static public void ExtractTrs(UnityEngine.Matrix4x4 transform,
            ref UnityEngine.Vector3 localPosition,
            ref UnityEngine.Quaternion localRotation,
            ref UnityEngine.Vector3 localScale)
        {
            localPosition = ExtractPosition(transform);
            localRotation = ExtractRotation(transform);
            localScale = ExtractScale(transform);
        }

        /// <summary>
        /// Extracts the local rotation from the given matrix.
        /// </summary>
        static private UnityEngine.Quaternion ExtractRotation(UnityEngine.Matrix4x4 mat4)
        {
            var forward = new UnityEngine.Vector3(mat4.m02, mat4.m12, mat4.m22);
            var up = new UnityEngine.Vector3(mat4.m01, mat4.m11, mat4.m21);
            return UnityEngine.Quaternion.LookRotation(forward, up);
        }

        /// <summary>
        /// Extracts the local position from the given matrix.
        /// </summary>
        static UnityEngine.Vector3 ExtractPosition(UnityEngine.Matrix4x4 mat4)
        {
            return new UnityEngine.Vector3(mat4.m03, mat4.m13, mat4.m23);
        }

        /// <summary>
        /// Extracts the local scale from the given matrix.
        /// </summary>
        static UnityEngine.Vector3 ExtractScale(UnityEngine.Matrix4x4 mat4)
        {
            UnityEngine.Vector3 scale;
            scale.x = new UnityEngine.Vector4(mat4.m00, mat4.m10, mat4.m20, mat4.m30).magnitude;
            scale.y = new UnityEngine.Vector4(mat4.m01, mat4.m11, mat4.m21, mat4.m31).magnitude;
            scale.z = new UnityEngine.Vector4(mat4.m02, mat4.m12, mat4.m22, mat4.m32).magnitude;
            return scale;
        }

        private static bool HasAnySiblingsWithName(Transform transform, string name)
        {
            if (transform.parent == null)
            {
                // When exporting prefabs from project window, scene is null.
                // If the GameObject doesn't have a parent, it can't have any siblings anyway.
                if (!transform.gameObject.scene.IsValid())
                    return false;
                return transform.gameObject.scene.GetRootGameObjects().Any(sibling => sibling != transform.gameObject && sibling.name == name);
            }

            foreach (Transform sibling in transform.parent)
            {
                if (sibling != transform && sibling.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        static public string GetUniqueName(Transform transform)
        {
            var uniqueName = transform.name;
            while (HasAnySiblingsWithName(transform, uniqueName))
            {
                uniqueName = $"{uniqueName}_{transform.GetSiblingIndex()}";
            }

            return uniqueName;
        }

        // ----------------------------------------------------------------------------------------- //
        // Paths
        // ----------------------------------------------------------------------------------------- //
        /// <summary>
        /// Returns a valid UsdPath for the given Unity GameObject.
        /// </summary>
        /// <remarks>
        /// Note that illegal characters are converted into legal characters, so invalid names may
        /// collide in the USD namespace.
        /// </remarks>
        static public string GetPath(UnityEngine.Transform unityObj)
        {
            return GetPath(unityObj, null);
        }

        /// <summary>
        /// Returns a valid UsdPath for the given Unity GameObject, relative to the given root.
        /// For example: obj = /Foo/Bar/Baz, root = /Foo the result will be /Bar/Baz
        /// </summary>
        /// <remarks>
        /// Note that illegal characters are converted into legal characters, so invalid names may
        /// collide in the USD namespace.
        /// </remarks>
        static public string GetPath(Transform unityObj, Transform unityObjRoot)
        {
            // Base case.
            if (unityObjRoot != null && unityObj == null)
            {
                throw new Exception("Expected to find root " + unityObjRoot.name + " but did not.");
            }

            if (unityObj == unityObjRoot)
            {
                return "";
            }

            // Build the path from root to leaf.
            return GetPath(unityObj.transform.parent, unityObjRoot) + "/" + UnityTypeConverter.MakeValidIdentifier(GetUniqueName(unityObj));
        }

        // ----------------------------------------------------------------------------------------- //
        // Matrix4x4, Matrix4d
        // Note that Unity uses float matrices, but USD favors double-valued matrices.
        // ----------------------------------------------------------------------------------------- //
        static public UnityEngine.Matrix4x4 GetLocalToParentXf(UnityEngine.Transform unityXf)
        {
            return UnityEngine.Matrix4x4.TRS(unityXf.localPosition,
                unityXf.localRotation,
                unityXf.localScale);
        }

        [Preserve]
        static public GfMatrix4d ToGfMatrix(UnityEngine.Transform unityXf)
        {
            return ToGfMatrix(GetLocalToParentXf(unityXf));
        }

        [Preserve]
        static public GfMatrix4d ToGfMatrix(UnityEngine.Matrix4x4 unityMat4)
        {
            // Note that USD is row-major and Unity is column-major, the arguments below are pivoted
            // to account for this.
            return new GfMatrix4d(unityMat4[0, 0], unityMat4[1, 0], unityMat4[2, 0], unityMat4[3, 0],
                unityMat4[0, 1], unityMat4[1, 1], unityMat4[2, 1], unityMat4[3, 1],
                unityMat4[0, 2], unityMat4[1, 2], unityMat4[2, 2], unityMat4[3, 2],
                unityMat4[0, 3], unityMat4[1, 3], unityMat4[2, 3], unityMat4[3, 3]);
        }

        [Preserve]
        static public UnityEngine.Matrix4x4 FromMatrix(GfMatrix4d gfMat)
        {
            // Note that USD is row-major and Unity is column-major, the arguments below are pivoted
            // to account for this.
            UnityEngine.Matrix4x4 ret = new UnityEngine.Matrix4x4();
            double[] tmp = UsdIo.ArrayAllocator.Malloc<double>(16);
            gfMat.CopyToArray(tmp);

            ret[0, 0] = (float)tmp[0];
            ret[1, 0] = (float)tmp[1];
            ret[2, 0] = (float)tmp[2];
            ret[3, 0] = (float)tmp[3];

            ret[0, 1] = (float)tmp[4 + 0];
            ret[1, 1] = (float)tmp[4 + 1];
            ret[2, 1] = (float)tmp[4 + 2];
            ret[3, 1] = (float)tmp[4 + 3];

            ret[0, 2] = (float)tmp[8 + 0];
            ret[1, 2] = (float)tmp[8 + 1];
            ret[2, 2] = (float)tmp[8 + 2];
            ret[3, 2] = (float)tmp[8 + 3];

            ret[0, 3] = (float)tmp[12 + 0];
            ret[1, 3] = (float)tmp[12 + 1];
            ret[2, 3] = (float)tmp[12 + 2];
            ret[3, 3] = (float)tmp[12 + 3];
            UsdIo.ArrayAllocator.Free(tmp.GetType(), (uint)tmp.Length, tmp);
            return ret;
        }

        // ----------------------------------------------------------------------------------------- //
        // Matrix4x4[] / VtArray<GfMatrix4d>
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtMatrix4dArray ToVtArray(UnityEngine.Matrix4x4[] input)
        {
            var output = new VtMatrix4dArray((uint)input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = ToGfMatrix(input[i]);
            }
            return output;
        }

        [Preserve]
        static public UnityEngine.Matrix4x4[] FromVtArray(VtMatrix4dArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Matrix4x4>(input.size());
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = FromMatrix(input[i]);
            }
            /* This doesn't work because we're converting double/float.
             * Should add a specialized C++ function to do this conversion.
            unsafe
            {
              fixed (UnityEngine.Matrix4x4* p = output) {
                input.CopyToArray((IntPtr)p);
              }
            }
            */
            return output;
        }

        [Preserve]
        static public VtMatrix4dArray ListToVtArray(List<UnityEngine.Matrix4x4> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<UnityEngine.Matrix4x4> ListFromVtArray(VtMatrix4dArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Matrix4x4>(input.size());
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = FromMatrix(input[i]);
            }
            return output.ToList();
        }

        // ----------------------------------------------------------------------------------------- //
        // Color32 direct conversion
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public GfVec4f Color32ToVec4f(UnityEngine.Color32 c)
        {
            return ColorToVec4f(c);
        }

        [Preserve]
        static public UnityEngine.Color32 Vec4fToColor32(GfVec4f v)
        {
            return Vec4fToColor(v);
        }

        [Preserve]
        static public VtVec4fArray ToVtArray(List<UnityEngine.Color32> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public void ToVtArray(List<UnityEngine.Color32> input,
            out VtVec3fArray rgb,
            out VtFloatArray alpha)
        {
            ToVtArray(input.ToArray(), out rgb, out alpha);
        }

        [Preserve]
        static public VtVec4fArray ToVtArray(UnityEngine.Color32[] input)
        {
            var color = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>((uint)input.Length);
            // PERFORMANCE: this sucks, should implement conversion in C++.
            for (int i = 0; i < input.Length; i++)
            {
                color[i] = input[i];
            }
            return ToVtArray(color);
        }

        [Preserve]
        static public UnityEngine.Color32[] Color32FromVtArray(VtVec4fArray input)
        {
            UnityEngine.Color32[] ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color32>(input.size());
            UnityEngine.Vector4[] rgb = FromVtArray(input);

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new UnityEngine.Color(rgb[i][0], rgb[i][1], rgb[i][2], rgb[i][3]);
            }

            return ret;
        }

        // ----------------------------------------------------------------------------------------- //
        // Color direct conversion
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public GfVec4f ColorToVec4f(UnityEngine.Color c)
        {
            return new GfVec4f(c.r, c.g, c.b, c.a);
        }

        [Preserve]
        static public UnityEngine.Color Vec4fToColor(GfVec4f v)
        {
            return new UnityEngine.Color(v[0], v[1], v[2], v[3]);
        }

        [Preserve]
        static public VtVec4fArray ToVtArray(List<UnityEngine.Color> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public void ToVtArray(List<UnityEngine.Color> input,
            out VtVec3fArray rgb,
            out VtFloatArray alpha)
        {
            ToVtArray(input.ToArray(), out rgb, out alpha);
        }

        [Preserve]
        static public VtVec4fArray ToVtArray(UnityEngine.Color[] input)
        {
            var output = new VtVec4fArray((uint)input.Length);
            unsafe
            {
                fixed (UnityEngine.Color* p = input)
                {
                    output.CopyFromArray((IntPtr)p);
                }
            }
            return output;
        }

        [Preserve]
        static public UnityEngine.Color[] ColorFromVtArray(VtVec4fArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>(input.size());
            return ColorFromVtArray(input, ref output);
        }

        [Preserve]
        static public UnityEngine.Color[] ColorFromVtArray(VtVec4fArray input,
            ref UnityEngine.Color[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>(input.size());
            }
            unsafe
            {
                fixed (UnityEngine.Color* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            return output;
        }

        // ----------------------------------------------------------------------------------------- //
        // Color32 / Vec3f,Float split conversion
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public void ToVtArray(UnityEngine.Color32[] input,
            out VtVec3fArray rgb,
            out VtFloatArray alpha)
        {
            // Unfortunate, but faster than using the USD bindings currently.
            var unityRgb = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)input.Length);
            float[] unityAlpha = UsdIo.ArrayAllocator.Malloc<float>((uint)input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                unityAlpha[i] = input[i].a;
                unityRgb[i] = new UnityEngine.Vector3(input[i].r, input[i].g, input[i].b);
            }

            // Convert to Vt.
            rgb = ToVtArray(unityRgb);
            alpha = ToVtArray(unityAlpha);
        }

        /// Returns just the RGB elements of a Color array as a new Vector3 array.
        [Preserve]
        static public UnityEngine.Vector3[] ExtractRgb(UnityEngine.Color32[] colors)
        {
            var ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                ret[i] = new UnityEngine.Vector3(colors[i].r / 255f,
                    colors[i].g / 255f,
                    colors[i].b / 255f);
            }
            return ret;
        }

        /// Returns just the alpha component of a Color array as a new array of float.
        [Preserve]
        static public float[] ExtractAlpha(UnityEngine.Color32[] colors)
        {
            var ret = UsdIo.ArrayAllocator.Malloc<float>((uint)colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                ret[i] = colors[i].a / 255f;
            }
            return ret;
        }

        // ----------------------------------------------------------------------------------------- //
        // Color / Vec3f,Float
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public void ToVtArray(UnityEngine.Color[] input,
            out VtVec3fArray rgb,
            out VtFloatArray alpha)
        {
            // Unfortunate, but faster than using the USD bindings currently.
            var unityRgb = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)input.Length);
            var unityAlpha = UsdIo.ArrayAllocator.Malloc<float>((uint)input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                unityAlpha[i] = input[i].a;
                unityRgb[i] = new UnityEngine.Vector3(input[i].r, input[i].g, input[i].b);
            }

            // Convert to Vt.
            rgb = ToVtArray(unityRgb);
            alpha = ToVtArray(unityAlpha);
        }

        [Preserve]
        static public UnityEngine.Color[] FromVtArray(VtVec3fArray rgbIn, VtFloatArray alphaIn)
        {
            var ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>(rgbIn.size());
            float[] alpha = FromVtArray(alphaIn);
            UnityEngine.Vector3[] rgb = FromVtArray(rgbIn);

            for (int i = 0; i < rgbIn.size(); i++)
            {
                ret[i].r = rgb[i][0];
                ret[i].g = rgb[i][1];
                ret[i].b = rgb[i][2];
                ret[i].a = alpha[i];
            }

            return ret;
        }

        /// Returns just the RGB elements of a Color array as a new Vector3 array.
        [Preserve]
        static public UnityEngine.Vector3[] ExtractRgb(UnityEngine.Color[] colors)
        {
            var ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                ret[i] = new UnityEngine.Vector3(colors[i].r, colors[i].g, colors[i].b);
            }
            return ret;
        }

        /// Returns just the alpha component of a Color array as a new array of float.
        [Preserve]
        static public float[] ExtractAlpha(UnityEngine.Color[] colors)
        {
            var ret = UsdIo.ArrayAllocator.Malloc<float>((uint)colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                ret[i] = colors[i].a;
            }
            return ret;
        }

        // ----------------------------------------------------------------------------------------- //
        // Quaternion
        // ----------------------------------------------------------------------------------------- //
        //
        // USD GfQuaternion is layed out in memory as [real] [i0, i1, i2]
        // Unity Quaternion is [i0, i1, i2] [real]
        // Swapping seems preferable to making a temp array copy.

        [Preserve]
        static private void SwapQuaternionReal(ref UnityEngine.Quaternion[] input)
        {
            float tmp;
            for (int i = 0; i < input.Length; i++)
            {
                tmp = input[i][0];
                input[i][0] = input[i][3];
                input[i][3] = tmp;
            }
        }

        [Preserve]
        static public GfQuatf QuaternionToQuatf(UnityEngine.Quaternion quaternion)
        {
            // See pxr/unity quaternion layout above.
            return new GfQuatf(quaternion.w, quaternion.x, quaternion.y, quaternion.z);
        }

        [Preserve]
        static public UnityEngine.Quaternion QuatfToQuaternion(GfQuatf quat)
        {
            // See pxr/unity quaternion layout above.
            GfVec3f img = quat.GetImaginary();
            return new UnityEngine.Quaternion(img[0], img[1], img[2], quat.GetReal());
        }

        [Preserve]
        static public VtQuatfArray ToVtArray(UnityEngine.Quaternion[] input)
        {
            SwapQuaternionReal(ref input);

            var output = new VtQuatfArray((uint)input.Length);
            unsafe
            {
                // Copy to USD/C++
                fixed (UnityEngine.Quaternion* p = input)
                {
                    output.CopyFromArray((IntPtr)p);
                }
            }

            // Swap back
            SwapQuaternionReal(ref input);

            return output;
        }

        [Preserve]
        static public UnityEngine.Quaternion[] FromVtArray(VtQuatfArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Quaternion>(input.size());
            unsafe
            {
                fixed (UnityEngine.Quaternion* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            // Swap real component for USD/Unity mis-match.
            SwapQuaternionReal(ref output);
            return output;
        }

        [Preserve]
        static public VtQuatfArray ListToVtArray(List<UnityEngine.Quaternion> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<UnityEngine.Quaternion> ListFromVtArray(VtQuatfArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Quaternion>(input.size());
            unsafe
            {
                fixed (UnityEngine.Quaternion* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            // Swap real component for USD/Unity mis-match.
            SwapQuaternionReal(ref output);
            return output.ToList();
        }

        // ----------------------------------------------------------------------------------------- //
        // Vector4 / Vec4f
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtVec4fArray ToVtArray(UnityEngine.Vector4[] input)
        {
            var output = new VtVec4fArray((uint)input.Length);
            unsafe
            {
                fixed (UnityEngine.Vector4* p = input)
                {
                    output.CopyFromArray((IntPtr)p);
                }
            }
            return output;
        }

        [Preserve]
        static public UnityEngine.Vector4[] FromVtArray(VtVec4fArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector4>(input.size());
            unsafe
            {
                fixed (UnityEngine.Vector4* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            return output;
        }

        [Preserve]
        static public VtVec4fArray ListToVtArray(List<UnityEngine.Vector4> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<UnityEngine.Vector4> ListFromVtArray(VtVec4fArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector4>(input.size());
            unsafe
            {
                fixed (UnityEngine.Vector4* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            return output.ToList();
        }

        // ----------------------------------------------------------------------------------------- //
        // VtVec3f[] / Bounds
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtVec3fArray BoundsToVtArray(UnityEngine.Bounds input)
        {
            var vtArr = new VtVec3fArray(2);
            vtArr[0] = new GfVec3f(input.min[0], input.min[1], input.min[2]);
            vtArr[1] = new GfVec3f(input.max[0], input.max[1], input.max[2]);
            return vtArr;
        }

        [Preserve]
        static public UnityEngine.Bounds BoundsFromVtArray(VtValue vtBounds)
        {
            var bbox = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(2);
            var v3 = (VtVec3fArray)UsdIo.ArrayAllocator.MallocHandle(typeof(VtVec3fArray));
            UsdCs.VtValueToVtVec3fArray(vtBounds, v3);
            FromVtArray(v3, ref bbox);
            UsdIo.ArrayAllocator.FreeHandle(v3.GetType(), v3);
            UnityEngine.Bounds bnds = new UnityEngine.Bounds();
            bnds.min = bbox[0];
            bnds.max = bbox[1];
            UsdIo.ArrayAllocator.Free(bbox.GetType(), (uint)bbox.Length, bbox);
            return bnds;
        }

        [Preserve]
        static public UnityEngine.Bounds BoundsFromVtArray(VtValue vtBounds,
            UnityEngine.Vector3[] bbox)
        {
            //UnityEngine.Vector3[] bbox = new UnityEngine.Vector3[2];
            System.Diagnostics.Debug.Assert(bbox.Length == 2);
            FromVtArray(vtBounds, ref bbox);
            UnityEngine.Bounds bnds = new UnityEngine.Bounds();
            bnds.min = bbox[0];
            bnds.max = bbox[1];
            return bnds;
        }

        // ----------------------------------------------------------------------------------------- //
        // Vector3 / Vec3f
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtVec3fArray ToVtArray(UnityEngine.Vector3[] input)
        {
            var output = new VtVec3fArray((uint)input.Length);
            unsafe
            {
                fixed (UnityEngine.Vector3* p = input)
                {
                    output.CopyFromArray((IntPtr)p);
                }
            }
            return output;
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public UnityEngine.Vector3[] FromVtArray(VtVec3fArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtVec3fArray input, ref UnityEngine.Vector3[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(input.size());
            }
            unsafe
            {
                fixed (UnityEngine.Vector3* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
        }

        [Preserve]
        static public VtVec3fArray ListToVtArray(List<UnityEngine.Vector3> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<UnityEngine.Vector3> ListFromVtArray(VtVec3fArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(input.size());
            unsafe
            {
                fixed (UnityEngine.Vector3* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            return output.ToList();
        }

        // ----------------------------------------------------------------------------------------- //
        // Vector2 / Vec2f
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtVec2fArray ToVtArray(UnityEngine.Vector2[] input)
        {
            var output = new VtVec2fArray((uint)input.Length);
            unsafe
            {
                fixed (UnityEngine.Vector2* p = input)
                {
                    output.CopyFromArray((IntPtr)p);
                }
            }
            return output;
        }

        [Preserve]
        static public UnityEngine.Vector2[] FromVtArray(VtVec2fArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector2>(input.size());
            unsafe
            {
                fixed (UnityEngine.Vector2* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            return output;
        }

        [Preserve]
        static public VtVec2fArray ListToVtArray(List<UnityEngine.Vector2> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<UnityEngine.Vector2> ListFromVtArray(VtVec2fArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector2>(input.size());
            unsafe
            {
                fixed (UnityEngine.Vector2* p = output)
                {
                    input.CopyToArray((IntPtr)p);
                }
            }
            return output.ToList();
        }

        // ----------------------------------------------------------------------------------------- //
        // Vector2/Vec2
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public GfVec2f Vector2ToVec2f(UnityEngine.Vector2 vec2)
        {
            return new GfVec2f(vec2[0], vec2[1]);
        }

        [Preserve]
        static public UnityEngine.Vector2 Vec2fToVector2(GfVec2f value)
        {
            GfVec2f obj = pxr.UsdCs.VtValueToGfVec2f(value);
            return new UnityEngine.Vector2(obj[0], obj[1]);
        }

        // ----------------------------------------------------------------------------------------- //
        // Vector3/Vec3
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public GfVec3f Vector3ToVec3f(UnityEngine.Vector3 vec3)
        {
            return new pxr.GfVec3f(vec3[0], vec3[1], vec3[2]);
        }

        [Preserve]
        static public UnityEngine.Vector3 Vec3fToVector3(GfVec3f v3)
        {
            return new UnityEngine.Vector3(v3[0], v3[1], v3[2]);
        }

        // ----------------------------------------------------------------------------------------- //
        // Vector4/Vec4
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public pxr.GfVec4f Vector4ToVec4f(UnityEngine.Vector4 vector4)
        {
            return new pxr.GfVec4f(vector4[0], vector4[1], vector4[2], vector4[3]);
        }

        [Preserve]
        static public UnityEngine.Vector4 Vec4fToVector4(GfVec4f v4)
        {
            return new UnityEngine.Vector4(v4[0], v4[1], v4[2], v4[3]);
        }

        // ----------------------------------------------------------------------------------------- //
        // Rect
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public GfVec4f RectToVtVec4(UnityEngine.Rect rect)
        {
            return new pxr.GfVec4f(rect.x, rect.y, rect.width, rect.height);
        }

        [Preserve]
        static public UnityEngine.Rect Vec4fToRect(GfVec4f v4)
        {
            return new UnityEngine.Rect(v4[0], v4[1], v4[2], v4[3]);
        }
    }
}

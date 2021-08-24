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

using UnityEngine;
using System.Collections.Generic;

namespace USD.NET.Unity
{
    public static class UnityTypeBindings
    {
        /// Static constructor automatically registers conversions with the global type binder.
        static UnityTypeBindings()
        {
            RegisterTypes();
        }

        private static bool isInitialized = false;

        /// <summary>
        /// Registers Unity-specifc data type conversions in the global type binder (UsdIo.Bindings).
        /// </summary>
        static public void RegisterTypes()
        {
            if (isInitialized) { return; }
            isInitialized = true;

            TypeBinder binder = USD.NET.UsdIo.Bindings;

            //
            // Quaternion
            //
            binder.BindType(typeof(Quaternion), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.QuaternionToQuatf((Quaternion)obj),
                (pxr.VtValue value) => UnityTypeConverter.QuatfToQuaternion(value),
                SdfValueTypeNames.Quatf));
            binder.BindArrayType<UnityTypeConverter>(typeof(Quaternion[]), typeof(pxr.VtQuatfArray), SdfValueTypeNames.QuatfArray);
            binder.BindArrayType<UnityTypeConverter>(typeof(List<Quaternion>), typeof(pxr.VtQuatfArray), SdfValueTypeNames.QuatfArray, "List");

            //
            // Scalar Vector{2,3,4}
            //
            binder.BindType(typeof(Vector2), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.Vector2ToVec2f((Vector2)obj),
                (pxr.VtValue value) => UnityTypeConverter.Vec2fToVector2(value),
                SdfValueTypeNames.Float2));
            binder.AddTypeAlias(SdfValueTypeNames.TexCoord2f, SdfValueTypeNames.Float2);
            binder.BindType(typeof(Vector3), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.Vector3ToVec3f((Vector3)obj),
                (pxr.VtValue value) => UnityTypeConverter.Vec3fToVector3(value),
                SdfValueTypeNames.Float3));
            binder.AddTypeAlias(SdfValueTypeNames.TexCoord3f, SdfValueTypeNames.Float3);
            binder.BindType(typeof(Vector4), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.Vector4ToVec4f((Vector4)obj),
                (pxr.VtValue value) => UnityTypeConverter.Vec4fToVector4(value),
                SdfValueTypeNames.Float4));

            //
            // Scaler Rect <-> GfVec4f
            //
            binder.BindType(typeof(Rect), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.RectToVtVec4((Rect)obj),
                (pxr.VtValue value) => UnityTypeConverter.Vec4fToRect(value),
                SdfValueTypeNames.Float4));

            //
            // Vector {2,3,4} {[],List<>}
            //
            binder.BindArrayType<UnityTypeConverter>(typeof(Vector2[]), typeof(pxr.VtVec2fArray), SdfValueTypeNames.Float2Array);
            binder.BindArrayType<UnityTypeConverter>(typeof(List<Vector2>), typeof(pxr.VtVec2fArray), SdfValueTypeNames.Float2Array, "List");
            binder.AddTypeAlias(SdfValueTypeNames.TexCoord2fArray, SdfValueTypeNames.Float2Array);

            binder.BindArrayType<UnityTypeConverter>(typeof(Vector3[]), typeof(pxr.VtVec3fArray), SdfValueTypeNames.Float3Array);
            binder.BindArrayType<UnityTypeConverter>(typeof(List<Vector3>), typeof(pxr.VtVec3fArray), SdfValueTypeNames.Float3Array, "List");
            binder.AddTypeAlias(SdfValueTypeNames.TexCoord3fArray, SdfValueTypeNames.Float3Array);

            binder.BindArrayType<UnityTypeConverter>(typeof(Vector4[]), typeof(pxr.VtVec4fArray), SdfValueTypeNames.Float4Array);
            binder.BindArrayType<UnityTypeConverter>(typeof(List<Vector4>), typeof(pxr.VtVec4fArray), SdfValueTypeNames.Float4Array, "List");

            //
            // Color / Color32
            //
            binder.BindType(typeof(Color[]), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.ToVtArray(((Color[])obj)),
                (pxr.VtValue vtVal) => UnityTypeConverter.ColorFromVtArray(vtVal),
                SdfValueTypeNames.Color4fArray));

            binder.BindType(typeof(Color), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.ColorToVec4f(((Color)obj)),
                (pxr.VtValue vtVal) => UnityTypeConverter.Vec4fToColor(vtVal),
                SdfValueTypeNames.Color4f));

            binder.BindType(typeof(Color32), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.Color32ToVec4f(((Color32)obj)),
                (pxr.VtValue vtVal) => UnityTypeConverter.Vec4fToColor32(vtVal),
                SdfValueTypeNames.Color4f));

            binder.BindType(typeof(Bounds), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.BoundsToVtArray(((Bounds)obj)),
                (pxr.VtValue vtVal) => UnityTypeConverter.BoundsFromVtArray(vtVal),
                SdfValueTypeNames.Float3Array));

            binder.BindArrayType<UnityTypeConverter>(typeof(List<Color>), typeof(pxr.VtVec4fArray), SdfValueTypeNames.Color4fArray);
            binder.BindArrayType<UnityTypeConverter>(typeof(Color32[]), typeof(pxr.VtVec4fArray), SdfValueTypeNames.Color4fArray);

            //
            // Matrix4x4
            //

            // Note that UsdGeom exclusively uses double-precision matrices for storage.
            // In the future, it may be nice to support single-precision for clients who aren't targeting
            // UsdGeom.
            binder.BindType(typeof(Matrix4x4), new UsdTypeBinding(
                (object obj) => UnityTypeConverter.ToGfMatrix(((Matrix4x4)obj)),
                (pxr.VtValue vtVal) => UnityTypeConverter.FromMatrix((pxr.GfMatrix4d)vtVal),
                SdfValueTypeNames.Matrix4d));

            binder.BindArrayType<UnityTypeConverter>(typeof(Matrix4x4[]), typeof(pxr.VtMatrix4dArray), SdfValueTypeNames.Matrix4dArray);
            binder.BindArrayType<UnityTypeConverter>(typeof(List<Matrix4x4>), typeof(pxr.VtMatrix4dArray), SdfValueTypeNames.Matrix4dArray, "List");
        }
    }
}

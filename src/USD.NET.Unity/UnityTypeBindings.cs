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

namespace USD.NET.Unity {
  public static class UnityTypeBindings {

    /// Static constructor automatically registers conversions with the global type binder.
    static UnityTypeBindings() {
      RegisterTypes();
    }

    private static bool isInitialized = false;

    static public void RegisterTypes() {
      if (isInitialized) { return; }
      isInitialized = true;

      TypeBinder binder = USD.NET.UsdIo.Bindings;

      //
      // Quaternion
      //
      binder.BindType(typeof(Quaternion), new UsdTypeBinding(
          (object obj) => UnityTypeConverter.QuaternionToQuatf((Quaternion)obj),
          (pxr.VtValue vtVal) => UnityTypeConverter.QuatfToQuaternion(vtVal),
          SdfValueTypeNames.Quatf));
      binder.BindArrayType<UnityTypeConverter>(typeof(Quaternion[]), typeof(pxr.VtQuatfArray), SdfValueTypeNames.QuatfArray);
      binder.BindArrayType<UnityTypeConverter>(typeof(List<Quaternion>), typeof(pxr.VtQuatfArray), SdfValueTypeNames.QuatfArray, "List");

      //
      // Vector {2,3,4} {[],List<>}
      //
      binder.BindType(typeof(Vector2), new UsdTypeBinding(
          (object obj) => UnityTypeConverter.Vector2ToVec2f((Vector2)obj),
          (pxr.VtValue vtVal) => UnityTypeConverter.Vec2fToVector2(vtVal),
          SdfValueTypeNames.Color4fArray));
      binder.BindArrayType<UnityTypeConverter>(typeof(Vector2[]), typeof(pxr.VtVec2fArray), SdfValueTypeNames.Float2Array);
      binder.BindArrayType<UnityTypeConverter>(typeof(List<Vector2>), typeof(pxr.VtVec2fArray), SdfValueTypeNames.Float2Array);

      binder.BindArrayType<UnityTypeConverter>(typeof(Vector3[]), typeof(pxr.VtVec3fArray), SdfValueTypeNames.Float3Array);
      binder.BindArrayType<UnityTypeConverter>(typeof(List<Vector3>), typeof(pxr.VtVec3fArray), SdfValueTypeNames.Float3Array);

      binder.BindArrayType<UnityTypeConverter>(typeof(Vector4[]), typeof(pxr.VtVec4fArray), SdfValueTypeNames.Float4Array);
      binder.BindArrayType<UnityTypeConverter>(typeof(List<Vector4>), typeof(pxr.VtVec4fArray), SdfValueTypeNames.Float4Array);

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

    }
  }
}

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

%typemap(cscode) VtValue %{

    public static implicit operator GfHalf (VtValue value) {
        return UsdCs.VtValueToGfHalf(value);
    }

    public static implicit operator GfMatrix2d (VtValue value) {
        return UsdCs.VtValueToGfMatrix2d(value);
    }

    public static implicit operator GfMatrix3d (VtValue value) {
        return UsdCs.VtValueToGfMatrix3d(value);
    }

    public static implicit operator GfMatrix4d (VtValue value) {
        return UsdCs.VtValueToGfMatrix4d(value);
    }

    public static implicit operator GfQuatd (VtValue value) {
        return UsdCs.VtValueToGfQuatd(value);
    }

    public static implicit operator GfQuatf (VtValue value) {
        return UsdCs.VtValueToGfQuatf(value);
    }

    public static implicit operator GfQuath (VtValue value) {
        return UsdCs.VtValueToGfQuath(value);
    }

    public static implicit operator GfVec2d (VtValue value) {
        return UsdCs.VtValueToGfVec2d(value);
    }

    public static implicit operator GfVec2f (VtValue value) {
        return UsdCs.VtValueToGfVec2f(value);
    }

    public static implicit operator GfVec2h (VtValue value) {
        return UsdCs.VtValueToGfVec2h(value);
    }

    public static implicit operator GfVec2i (VtValue value) {
        return UsdCs.VtValueToGfVec2i(value);
    }

    public static implicit operator GfVec3d (VtValue value) {
        return UsdCs.VtValueToGfVec3d(value);
    }

    public static implicit operator GfVec3f (VtValue value) {
        return UsdCs.VtValueToGfVec3f(value);
    }

    public static implicit operator GfVec3h (VtValue value) {
        return UsdCs.VtValueToGfVec3h(value);
    }

    public static implicit operator GfVec3i (VtValue value) {
        return UsdCs.VtValueToGfVec3i(value);
    }

    public static implicit operator GfVec4d (VtValue value) {
        return UsdCs.VtValueToGfVec4d(value);
    }

    public static implicit operator GfVec4f (VtValue value) {
        return UsdCs.VtValueToGfVec4f(value);
    }

    public static implicit operator GfVec4h (VtValue value) {
        return UsdCs.VtValueToGfVec4h(value);
    }

    public static implicit operator GfVec4i (VtValue value) {
        return UsdCs.VtValueToGfVec4i(value);
    }

    public static implicit operator SdfAssetPath (VtValue value) {
        return UsdCs.VtValueToSdfAssetPath(value);
    }

    public static implicit operator SdfAssetPathArray (VtValue value) {
        return UsdCs.VtValueToSdfAssetPathArray(value);
    }

    public static implicit operator TfToken (VtValue value) {
        return UsdCs.VtValueToTfToken(value);
    }

    public static implicit operator VtBoolArray (VtValue value) {
        return UsdCs.VtValueToVtBoolArray(value);
    }

    public static implicit operator VtDoubleArray (VtValue value) {
        return UsdCs.VtValueToVtDoubleArray(value);
    }

    public static implicit operator VtFloatArray (VtValue value) {
        return UsdCs.VtValueToVtFloatArray(value);
    }

    public static implicit operator VtHalfArray (VtValue value) {
        return UsdCs.VtValueToVtHalfArray(value);
    }

    public static implicit operator VtInt64Array (VtValue value) {
        return UsdCs.VtValueToVtInt64Array(value);
    }

    public static implicit operator VtIntArray (VtValue value) {
        return UsdCs.VtValueToVtIntArray(value);
    }

    public static implicit operator VtMatrix2dArray (VtValue value) {
        return UsdCs.VtValueToVtMatrix2dArray(value);
    }

    public static implicit operator VtMatrix3dArray (VtValue value) {
        return UsdCs.VtValueToVtMatrix3dArray(value);
    }

    public static implicit operator VtMatrix4dArray (VtValue value) {
        return UsdCs.VtValueToVtMatrix4dArray(value);
    }

    public static implicit operator VtQuatdArray (VtValue value) {
        return UsdCs.VtValueToVtQuatdArray(value);
    }

    public static implicit operator VtQuatfArray (VtValue value) {
        return UsdCs.VtValueToVtQuatfArray(value);
    }

    public static implicit operator VtQuathArray (VtValue value) {
        return UsdCs.VtValueToVtQuathArray(value);
    }

    public static implicit operator VtStringArray (VtValue value) {
        return UsdCs.VtValueToVtStringArray(value);
    }

    public static implicit operator VtTokenArray (VtValue value) {
        return UsdCs.VtValueToVtTokenArray(value);
    }

    public static implicit operator VtUCharArray (VtValue value) {
        return UsdCs.VtValueToVtUCharArray(value);
    }

    public static implicit operator VtUInt64Array (VtValue value) {
        return UsdCs.VtValueToVtUInt64Array(value);
    }

    public static implicit operator VtUIntArray (VtValue value) {
        return UsdCs.VtValueToVtUIntArray(value);
    }

    public static implicit operator VtVec2dArray (VtValue value) {
        return UsdCs.VtValueToVtVec2dArray(value);
    }

    public static implicit operator VtVec2fArray (VtValue value) {
        return UsdCs.VtValueToVtVec2fArray(value);
    }

    public static implicit operator VtVec2hArray (VtValue value) {
        return UsdCs.VtValueToVtVec2hArray(value);
    }

    public static implicit operator VtVec2iArray (VtValue value) {
        return UsdCs.VtValueToVtVec2iArray(value);
    }

    public static implicit operator VtVec3dArray (VtValue value) {
        return UsdCs.VtValueToVtVec3dArray(value);
    }

    public static implicit operator VtVec3fArray (VtValue value) {
        return UsdCs.VtValueToVtVec3fArray(value);
    }

    public static implicit operator VtVec3hArray (VtValue value) {
        return UsdCs.VtValueToVtVec3hArray(value);
    }

    public static implicit operator VtVec3iArray (VtValue value) {
        return UsdCs.VtValueToVtVec3iArray(value);
    }

    public static implicit operator VtVec4dArray (VtValue value) {
        return UsdCs.VtValueToVtVec4dArray(value);
    }

    public static implicit operator VtVec4fArray (VtValue value) {
        return UsdCs.VtValueToVtVec4fArray(value);
    }

    public static implicit operator VtVec4hArray (VtValue value) {
        return UsdCs.VtValueToVtVec4hArray(value);
    }

    public static implicit operator VtVec4iArray (VtValue value) {
        return UsdCs.VtValueToVtVec4iArray(value);
    }

    public static implicit operator bool (VtValue value) {
        return UsdCs.VtValueTobool(value);
    }

    public static implicit operator double (VtValue value) {
        return UsdCs.VtValueTodouble(value);
    }

    public static implicit operator float (VtValue value) {
        return UsdCs.VtValueTofloat(value);
    }

    public static implicit operator int (VtValue value) {
        return UsdCs.VtValueToint(value);
    }

    public static implicit operator long (VtValue value) {
        return UsdCs.VtValueTolong(value);
    }

    public static implicit operator string (VtValue value) {
        return UsdCs.VtValueTostring(value);
    }

    public static implicit operator ulong (VtValue value) {
        return UsdCs.VtValueToulong(value);
    }

    public static implicit operator byte (VtValue value) {
        return UsdCs.VtValueTobyte(value);
    }

    public static implicit operator uint (VtValue value) {
        return UsdCs.VtValueTouint(value);
    }

    public static implicit operator VtValue (GfHalf value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfMatrix2d value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfMatrix3d value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfMatrix4d value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfQuatd value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfQuatf value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfQuath value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec2d value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec2f value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec2h value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec2i value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec3d value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec3f value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec3h value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec3i value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec4d value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec4f value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec4h value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (GfVec4i value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (SdfAssetPath value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (SdfAssetPathArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (TfToken value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtBoolArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtDoubleArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtFloatArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtHalfArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtInt64Array value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtIntArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtMatrix2dArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtMatrix3dArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtMatrix4dArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtQuatdArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtQuatfArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtQuathArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtStringArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtTokenArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtUCharArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtUInt64Array value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtUIntArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec2dArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec2fArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec2hArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec2iArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec3dArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec3fArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec3hArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec3iArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec4dArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec4fArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec4hArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (VtVec4iArray value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (bool value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (double value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (float value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (int value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (long value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (string value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (ulong value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (byte value) {
        return new VtValue(value);
    }

    public static implicit operator VtValue (uint value) {
        return new VtValue(value);
    }
%}

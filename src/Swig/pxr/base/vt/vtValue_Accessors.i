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

%inline %{
// This code manifests in UsdCs class.


extern GfHalf VtValueToGfHalf(VtValue const& value) {
  if (value.IsHolding<GfHalf>()) {
    return value.UncheckedGet<GfHalf>();
  }
  return GfHalf();
}
extern void VtValueToGfHalf(VtValue const& value, GfHalf* output) {
  if (value.IsHolding<GfHalf>()) {
    *output = value.UncheckedGet<GfHalf>();
  }
}

extern GfMatrix2d VtValueToGfMatrix2d(VtValue const& value) {
  if (value.IsHolding<GfMatrix2d>()) {
    return value.UncheckedGet<GfMatrix2d>();
  }
  return GfMatrix2d();
}
extern void VtValueToGfMatrix2d(VtValue const& value, GfMatrix2d* output) {
  if (value.IsHolding<GfMatrix2d>()) {
    *output = value.UncheckedGet<GfMatrix2d>();
  }
}

extern GfMatrix3d VtValueToGfMatrix3d(VtValue const& value) {
  if (value.IsHolding<GfMatrix3d>()) {
    return value.UncheckedGet<GfMatrix3d>();
  }
  return GfMatrix3d();
}
extern void VtValueToGfMatrix3d(VtValue const& value, GfMatrix3d* output) {
  if (value.IsHolding<GfMatrix3d>()) {
    *output = value.UncheckedGet<GfMatrix3d>();
  }
}

extern GfMatrix4d VtValueToGfMatrix4d(VtValue const& value) {
  if (value.IsHolding<GfMatrix4d>()) {
    return value.UncheckedGet<GfMatrix4d>();
  }
  return GfMatrix4d();
}
extern void VtValueToGfMatrix4d(VtValue const& value, GfMatrix4d* output) {
  if (value.IsHolding<GfMatrix4d>()) {
    *output = value.UncheckedGet<GfMatrix4d>();
  }
}

extern GfQuatd VtValueToGfQuatd(VtValue const& value) {
  if (value.IsHolding<GfQuatd>()) {
    return value.UncheckedGet<GfQuatd>();
  }
  return GfQuatd();
}
extern void VtValueToGfQuatd(VtValue const& value, GfQuatd* output) {
  if (value.IsHolding<GfQuatd>()) {
    *output = value.UncheckedGet<GfQuatd>();
  }
}

extern GfQuatf VtValueToGfQuatf(VtValue const& value) {
  if (value.IsHolding<GfQuatf>()) {
    return value.UncheckedGet<GfQuatf>();
  }
  return GfQuatf();
}
extern void VtValueToGfQuatf(VtValue const& value, GfQuatf* output) {
  if (value.IsHolding<GfQuatf>()) {
    *output = value.UncheckedGet<GfQuatf>();
  }
}

extern GfQuath VtValueToGfQuath(VtValue const& value) {
  if (value.IsHolding<GfQuath>()) {
    return value.UncheckedGet<GfQuath>();
  }
  return GfQuath();
}
extern void VtValueToGfQuath(VtValue const& value, GfQuath* output) {
  if (value.IsHolding<GfQuath>()) {
    *output = value.UncheckedGet<GfQuath>();
  }
}

extern GfVec2d VtValueToGfVec2d(VtValue const& value) {
  if (value.IsHolding<GfVec2d>()) {
    return value.UncheckedGet<GfVec2d>();
  }
  return GfVec2d();
}
extern void VtValueToGfVec2d(VtValue const& value, GfVec2d* output) {
  if (value.IsHolding<GfVec2d>()) {
    *output = value.UncheckedGet<GfVec2d>();
  }
}

extern GfVec2f VtValueToGfVec2f(VtValue const& value) {
  if (value.IsHolding<GfVec2f>()) {
    return value.UncheckedGet<GfVec2f>();
  }
  return GfVec2f();
}
extern void VtValueToGfVec2f(VtValue const& value, GfVec2f* output) {
  if (value.IsHolding<GfVec2f>()) {
    *output = value.UncheckedGet<GfVec2f>();
  }
}

extern GfVec2h VtValueToGfVec2h(VtValue const& value) {
  if (value.IsHolding<GfVec2h>()) {
    return value.UncheckedGet<GfVec2h>();
  }
  return GfVec2h();
}
extern void VtValueToGfVec2h(VtValue const& value, GfVec2h* output) {
  if (value.IsHolding<GfVec2h>()) {
    *output = value.UncheckedGet<GfVec2h>();
  }
}

extern GfVec2i VtValueToGfVec2i(VtValue const& value) {
  if (value.IsHolding<GfVec2i>()) {
    return value.UncheckedGet<GfVec2i>();
  }
  return GfVec2i();
}
extern void VtValueToGfVec2i(VtValue const& value, GfVec2i* output) {
  if (value.IsHolding<GfVec2i>()) {
    *output = value.UncheckedGet<GfVec2i>();
  }
}

extern GfVec3d VtValueToGfVec3d(VtValue const& value) {
  if (value.IsHolding<GfVec3d>()) {
    return value.UncheckedGet<GfVec3d>();
  }
  return GfVec3d();
}
extern void VtValueToGfVec3d(VtValue const& value, GfVec3d* output) {
  if (value.IsHolding<GfVec3d>()) {
    *output = value.UncheckedGet<GfVec3d>();
  }
}

extern GfVec3f VtValueToGfVec3f(VtValue const& value) {
  if (value.IsHolding<GfVec3f>()) {
    return value.UncheckedGet<GfVec3f>();
  }
  return GfVec3f();
}
extern void VtValueToGfVec3f(VtValue const& value, GfVec3f* output) {
  if (value.IsHolding<GfVec3f>()) {
    *output = value.UncheckedGet<GfVec3f>();
  }
}

extern GfVec3h VtValueToGfVec3h(VtValue const& value) {
  if (value.IsHolding<GfVec3h>()) {
    return value.UncheckedGet<GfVec3h>();
  }
  return GfVec3h();
}
extern void VtValueToGfVec3h(VtValue const& value, GfVec3h* output) {
  if (value.IsHolding<GfVec3h>()) {
    *output = value.UncheckedGet<GfVec3h>();
  }
}

extern GfVec3i VtValueToGfVec3i(VtValue const& value) {
  if (value.IsHolding<GfVec3i>()) {
    return value.UncheckedGet<GfVec3i>();
  }
  return GfVec3i();
}
extern void VtValueToGfVec3i(VtValue const& value, GfVec3i* output) {
  if (value.IsHolding<GfVec3i>()) {
    *output = value.UncheckedGet<GfVec3i>();
  }
}

extern GfVec4d VtValueToGfVec4d(VtValue const& value) {
  if (value.IsHolding<GfVec4d>()) {
    return value.UncheckedGet<GfVec4d>();
  }
  return GfVec4d();
}
extern void VtValueToGfVec4d(VtValue const& value, GfVec4d* output) {
  if (value.IsHolding<GfVec4d>()) {
    *output = value.UncheckedGet<GfVec4d>();
  }
}

extern GfVec4f VtValueToGfVec4f(VtValue const& value) {
  if (value.IsHolding<GfVec4f>()) {
    return value.UncheckedGet<GfVec4f>();
  }
  return GfVec4f();
}
extern void VtValueToGfVec4f(VtValue const& value, GfVec4f* output) {
  if (value.IsHolding<GfVec4f>()) {
    *output = value.UncheckedGet<GfVec4f>();
  }
}

extern GfVec4h VtValueToGfVec4h(VtValue const& value) {
  if (value.IsHolding<GfVec4h>()) {
    return value.UncheckedGet<GfVec4h>();
  }
  return GfVec4h();
}
extern void VtValueToGfVec4h(VtValue const& value, GfVec4h* output) {
  if (value.IsHolding<GfVec4h>()) {
    *output = value.UncheckedGet<GfVec4h>();
  }
}

extern GfVec4i VtValueToGfVec4i(VtValue const& value) {
  if (value.IsHolding<GfVec4i>()) {
    return value.UncheckedGet<GfVec4i>();
  }
  return GfVec4i();
}
extern void VtValueToGfVec4i(VtValue const& value, GfVec4i* output) {
  if (value.IsHolding<GfVec4i>()) {
    *output = value.UncheckedGet<GfVec4i>();
  }
}

extern SdfAssetPath VtValueToSdfAssetPath(VtValue const& value) {
  if (value.IsHolding<SdfAssetPath>()) {
    return value.UncheckedGet<SdfAssetPath>();
  }
  return SdfAssetPath();
}
extern void VtValueToSdfAssetPath(VtValue const& value, SdfAssetPath* output) {
  if (value.IsHolding<SdfAssetPath>()) {
    *output = value.UncheckedGet<SdfAssetPath>();
  }
}

extern SdfAssetPathArray VtValueToSdfAssetPathArray(VtValue const& value) {
  if (value.IsHolding<SdfAssetPathArray>()) {
    return value.UncheckedGet<SdfAssetPathArray>();
  }
  return SdfAssetPathArray();
}
extern void VtValueToSdfAssetPathArray(VtValue const& value, SdfAssetPathArray* output) {
  if (value.IsHolding<SdfAssetPathArray>()) {
    *output = value.UncheckedGet<SdfAssetPathArray>();
  }
}

extern TfToken VtValueToTfToken(VtValue const& value) {
  if (value.IsHolding<TfToken>()) {
    return value.UncheckedGet<TfToken>();
  }
  return TfToken();
}
extern void VtValueToTfToken(VtValue const& value, TfToken* output) {
  if (value.IsHolding<TfToken>()) {
    *output = value.UncheckedGet<TfToken>();
  }
}

extern VtBoolArray VtValueToVtBoolArray(VtValue const& value) {
  if (value.IsHolding<VtBoolArray>()) {
    return value.UncheckedGet<VtBoolArray>();
  }
  return VtBoolArray();
}
extern void VtValueToVtBoolArray(VtValue const& value, VtBoolArray* output) {
  if (value.IsHolding<VtBoolArray>()) {
    *output = value.UncheckedGet<VtBoolArray>();
  }
}

extern VtDoubleArray VtValueToVtDoubleArray(VtValue const& value) {
  if (value.IsHolding<VtDoubleArray>()) {
    return value.UncheckedGet<VtDoubleArray>();
  }
  return VtDoubleArray();
}
extern void VtValueToVtDoubleArray(VtValue const& value, VtDoubleArray* output) {
  if (value.IsHolding<VtDoubleArray>()) {
    *output = value.UncheckedGet<VtDoubleArray>();
  }
}

extern VtFloatArray VtValueToVtFloatArray(VtValue const& value) {
  if (value.IsHolding<VtFloatArray>()) {
    return value.UncheckedGet<VtFloatArray>();
  }
  return VtFloatArray();
}
extern void VtValueToVtFloatArray(VtValue const& value, VtFloatArray* output) {
  if (value.IsHolding<VtFloatArray>()) {
    *output = value.UncheckedGet<VtFloatArray>();
  }
}

extern VtHalfArray VtValueToVtHalfArray(VtValue const& value) {
  if (value.IsHolding<VtHalfArray>()) {
    return value.UncheckedGet<VtHalfArray>();
  }
  return VtHalfArray();
}
extern void VtValueToVtHalfArray(VtValue const& value, VtHalfArray* output) {
  if (value.IsHolding<VtHalfArray>()) {
    *output = value.UncheckedGet<VtHalfArray>();
  }
}

extern VtInt64Array VtValueToVtInt64Array(VtValue const& value) {
  if (value.IsHolding<VtInt64Array>()) {
    return value.UncheckedGet<VtInt64Array>();
  }
  return VtInt64Array();
}
extern void VtValueToVtInt64Array(VtValue const& value, VtInt64Array* output) {
  if (value.IsHolding<VtInt64Array>()) {
    *output = value.UncheckedGet<VtInt64Array>();
  }
}

extern VtIntArray VtValueToVtIntArray(VtValue const& value) {
  if (value.IsHolding<VtIntArray>()) {
    return value.UncheckedGet<VtIntArray>();
  }
  return VtIntArray();
}
extern void VtValueToVtIntArray(VtValue const& value, VtIntArray* output) {
  if (value.IsHolding<VtIntArray>()) {
    *output = value.UncheckedGet<VtIntArray>();
  }
}

extern VtMatrix2dArray VtValueToVtMatrix2dArray(VtValue const& value) {
  if (value.IsHolding<VtMatrix2dArray>()) {
    return value.UncheckedGet<VtMatrix2dArray>();
  }
  return VtMatrix2dArray();
}
extern void VtValueToVtMatrix2dArray(VtValue const& value, VtMatrix2dArray* output) {
  if (value.IsHolding<VtMatrix2dArray>()) {
    *output = value.UncheckedGet<VtMatrix2dArray>();
  }
}

extern VtMatrix3dArray VtValueToVtMatrix3dArray(VtValue const& value) {
  if (value.IsHolding<VtMatrix3dArray>()) {
    return value.UncheckedGet<VtMatrix3dArray>();
  }
  return VtMatrix3dArray();
}
extern void VtValueToVtMatrix3dArray(VtValue const& value, VtMatrix3dArray* output) {
  if (value.IsHolding<VtMatrix3dArray>()) {
    *output = value.UncheckedGet<VtMatrix3dArray>();
  }
}

extern VtMatrix4dArray VtValueToVtMatrix4dArray(VtValue const& value) {
  if (value.IsHolding<VtMatrix4dArray>()) {
    return value.UncheckedGet<VtMatrix4dArray>();
  }
  return VtMatrix4dArray();
}
extern void VtValueToVtMatrix4dArray(VtValue const& value, VtMatrix4dArray* output) {
  if (value.IsHolding<VtMatrix4dArray>()) {
    *output = value.UncheckedGet<VtMatrix4dArray>();
  }
}

extern VtQuatdArray VtValueToVtQuatdArray(VtValue const& value) {
  if (value.IsHolding<VtQuatdArray>()) {
    return value.UncheckedGet<VtQuatdArray>();
  }
  return VtQuatdArray();
}
extern void VtValueToVtQuatdArray(VtValue const& value, VtQuatdArray* output) {
  if (value.IsHolding<VtQuatdArray>()) {
    *output = value.UncheckedGet<VtQuatdArray>();
  }
}

extern VtQuatfArray VtValueToVtQuatfArray(VtValue const& value) {
  if (value.IsHolding<VtQuatfArray>()) {
    return value.UncheckedGet<VtQuatfArray>();
  }
  return VtQuatfArray();
}
extern void VtValueToVtQuatfArray(VtValue const& value, VtQuatfArray* output) {
  if (value.IsHolding<VtQuatfArray>()) {
    *output = value.UncheckedGet<VtQuatfArray>();
  }
}

extern VtQuathArray VtValueToVtQuathArray(VtValue const& value) {
  if (value.IsHolding<VtQuathArray>()) {
    return value.UncheckedGet<VtQuathArray>();
  }
  return VtQuathArray();
}
extern void VtValueToVtQuathArray(VtValue const& value, VtQuathArray* output) {
  if (value.IsHolding<VtQuathArray>()) {
    *output = value.UncheckedGet<VtQuathArray>();
  }
}

extern VtStringArray VtValueToVtStringArray(VtValue const& value) {
  if (value.IsHolding<VtStringArray>()) {
    return value.UncheckedGet<VtStringArray>();
  }
  return VtStringArray();
}
extern void VtValueToVtStringArray(VtValue const& value, VtStringArray* output) {
  if (value.IsHolding<VtStringArray>()) {
    *output = value.UncheckedGet<VtStringArray>();
  }
}

extern VtTokenArray VtValueToVtTokenArray(VtValue const& value) {
  if (value.IsHolding<VtTokenArray>()) {
    return value.UncheckedGet<VtTokenArray>();
  }
  return VtTokenArray();
}
extern void VtValueToVtTokenArray(VtValue const& value, VtTokenArray* output) {
  if (value.IsHolding<VtTokenArray>()) {
    *output = value.UncheckedGet<VtTokenArray>();
  }
}

extern VtUCharArray VtValueToVtUCharArray(VtValue const& value) {
  if (value.IsHolding<VtUCharArray>()) {
    return value.UncheckedGet<VtUCharArray>();
  }
  return VtUCharArray();
}
extern void VtValueToVtUCharArray(VtValue const& value, VtUCharArray* output) {
  if (value.IsHolding<VtUCharArray>()) {
    *output = value.UncheckedGet<VtUCharArray>();
  }
}

extern VtUInt64Array VtValueToVtUInt64Array(VtValue const& value) {
  if (value.IsHolding<VtUInt64Array>()) {
    return value.UncheckedGet<VtUInt64Array>();
  }
  return VtUInt64Array();
}
extern void VtValueToVtUInt64Array(VtValue const& value, VtUInt64Array* output) {
  if (value.IsHolding<VtUInt64Array>()) {
    *output = value.UncheckedGet<VtUInt64Array>();
  }
}

extern VtUIntArray VtValueToVtUIntArray(VtValue const& value) {
  if (value.IsHolding<VtUIntArray>()) {
    return value.UncheckedGet<VtUIntArray>();
  }
  return VtUIntArray();
}
extern void VtValueToVtUIntArray(VtValue const& value, VtUIntArray* output) {
  if (value.IsHolding<VtUIntArray>()) {
    *output = value.UncheckedGet<VtUIntArray>();
  }
}

extern VtVec2dArray VtValueToVtVec2dArray(VtValue const& value) {
  if (value.IsHolding<VtVec2dArray>()) {
    return value.UncheckedGet<VtVec2dArray>();
  }
  return VtVec2dArray();
}
extern void VtValueToVtVec2dArray(VtValue const& value, VtVec2dArray* output) {
  if (value.IsHolding<VtVec2dArray>()) {
    *output = value.UncheckedGet<VtVec2dArray>();
  }
}

extern VtVec2fArray VtValueToVtVec2fArray(VtValue const& value) {
  if (value.IsHolding<VtVec2fArray>()) {
    return value.UncheckedGet<VtVec2fArray>();
  }
  return VtVec2fArray();
}
extern void VtValueToVtVec2fArray(VtValue const& value, VtVec2fArray* output) {
  if (value.IsHolding<VtVec2fArray>()) {
    *output = value.UncheckedGet<VtVec2fArray>();
  }
}

extern VtVec2hArray VtValueToVtVec2hArray(VtValue const& value) {
  if (value.IsHolding<VtVec2hArray>()) {
    return value.UncheckedGet<VtVec2hArray>();
  }
  return VtVec2hArray();
}
extern void VtValueToVtVec2hArray(VtValue const& value, VtVec2hArray* output) {
  if (value.IsHolding<VtVec2hArray>()) {
    *output = value.UncheckedGet<VtVec2hArray>();
  }
}

extern VtVec2iArray VtValueToVtVec2iArray(VtValue const& value) {
  if (value.IsHolding<VtVec2iArray>()) {
    return value.UncheckedGet<VtVec2iArray>();
  }
  return VtVec2iArray();
}
extern void VtValueToVtVec2iArray(VtValue const& value, VtVec2iArray* output) {
  if (value.IsHolding<VtVec2iArray>()) {
    *output = value.UncheckedGet<VtVec2iArray>();
  }
}

extern VtVec3dArray VtValueToVtVec3dArray(VtValue const& value) {
  if (value.IsHolding<VtVec3dArray>()) {
    return value.UncheckedGet<VtVec3dArray>();
  }
  return VtVec3dArray();
}
extern void VtValueToVtVec3dArray(VtValue const& value, VtVec3dArray* output) {
  if (value.IsHolding<VtVec3dArray>()) {
    *output = value.UncheckedGet<VtVec3dArray>();
  }
}

extern VtVec3fArray VtValueToVtVec3fArray(VtValue const& value) {
  if (value.IsHolding<VtVec3fArray>()) {
    return value.UncheckedGet<VtVec3fArray>();
  }
  return VtVec3fArray();
}
extern void VtValueToVtVec3fArray(VtValue const& value, VtVec3fArray* output) {
  if (value.IsHolding<VtVec3fArray>()) {
    *output = value.UncheckedGet<VtVec3fArray>();
  }
}

extern VtVec3hArray VtValueToVtVec3hArray(VtValue const& value) {
  if (value.IsHolding<VtVec3hArray>()) {
    return value.UncheckedGet<VtVec3hArray>();
  }
  return VtVec3hArray();
}
extern void VtValueToVtVec3hArray(VtValue const& value, VtVec3hArray* output) {
  if (value.IsHolding<VtVec3hArray>()) {
    *output = value.UncheckedGet<VtVec3hArray>();
  }
}

extern VtVec3iArray VtValueToVtVec3iArray(VtValue const& value) {
  if (value.IsHolding<VtVec3iArray>()) {
    return value.UncheckedGet<VtVec3iArray>();
  }
  return VtVec3iArray();
}
extern void VtValueToVtVec3iArray(VtValue const& value, VtVec3iArray* output) {
  if (value.IsHolding<VtVec3iArray>()) {
    *output = value.UncheckedGet<VtVec3iArray>();
  }
}

extern VtVec4dArray VtValueToVtVec4dArray(VtValue const& value) {
  if (value.IsHolding<VtVec4dArray>()) {
    return value.UncheckedGet<VtVec4dArray>();
  }
  return VtVec4dArray();
}
extern void VtValueToVtVec4dArray(VtValue const& value, VtVec4dArray* output) {
  if (value.IsHolding<VtVec4dArray>()) {
    *output = value.UncheckedGet<VtVec4dArray>();
  }
}

extern VtVec4fArray VtValueToVtVec4fArray(VtValue const& value) {
  if (value.IsHolding<VtVec4fArray>()) {
    return value.UncheckedGet<VtVec4fArray>();
  }
  return VtVec4fArray();
}
extern void VtValueToVtVec4fArray(VtValue const& value, VtVec4fArray* output) {
  if (value.IsHolding<VtVec4fArray>()) {
    *output = value.UncheckedGet<VtVec4fArray>();
  }
}

extern VtVec4hArray VtValueToVtVec4hArray(VtValue const& value) {
  if (value.IsHolding<VtVec4hArray>()) {
    return value.UncheckedGet<VtVec4hArray>();
  }
  return VtVec4hArray();
}
extern void VtValueToVtVec4hArray(VtValue const& value, VtVec4hArray* output) {
  if (value.IsHolding<VtVec4hArray>()) {
    *output = value.UncheckedGet<VtVec4hArray>();
  }
}

extern VtVec4iArray VtValueToVtVec4iArray(VtValue const& value) {
  if (value.IsHolding<VtVec4iArray>()) {
    return value.UncheckedGet<VtVec4iArray>();
  }
  return VtVec4iArray();
}
extern void VtValueToVtVec4iArray(VtValue const& value, VtVec4iArray* output) {
  if (value.IsHolding<VtVec4iArray>()) {
    *output = value.UncheckedGet<VtVec4iArray>();
  }
}

extern bool VtValueTobool(VtValue const& value) {
  if (value.IsHolding<bool>()) {
    return value.UncheckedGet<bool>();
  }
  return bool();
}
extern void VtValueTobool(VtValue const& value, bool* output) {
  if (value.IsHolding<bool>()) {
    *output = value.UncheckedGet<bool>();
  }
}

extern double VtValueTodouble(VtValue const& value) {
  if (value.IsHolding<double>()) {
    return value.UncheckedGet<double>();
  }
  return double();
}
extern void VtValueTodouble(VtValue const& value, double* output) {
  if (value.IsHolding<double>()) {
    *output = value.UncheckedGet<double>();
  }
}

extern float VtValueTofloat(VtValue const& value) {
  if (value.IsHolding<float>()) {
    return value.UncheckedGet<float>();
  }
  return float();
}
extern void VtValueTofloat(VtValue const& value, float* output) {
  if (value.IsHolding<float>()) {
    *output = value.UncheckedGet<float>();
  }
}

extern int VtValueToint(VtValue const& value) {
  if (value.IsHolding<int>()) {
    return value.UncheckedGet<int>();
  }
  return int();
}
extern void VtValueToint(VtValue const& value, int* output) {
  if (value.IsHolding<int>()) {
    *output = value.UncheckedGet<int>();
  }
}

extern int64_t VtValueTolong(VtValue const& value) {
  if (value.IsHolding<int64_t>()) {
    return value.UncheckedGet<int64_t>();
  }
  return int64_t();
}
extern void VtValueTolong(VtValue const& value, int64_t* output) {
  if (value.IsHolding<int64_t>()) {
    *output = value.UncheckedGet<int64_t>();
  }
}

extern std::string VtValueTostring(VtValue const& value) {
  if (value.IsHolding<std::string>()) {
    return value.UncheckedGet<std::string>();
  }
  return std::string();
}
extern void VtValueTostring(VtValue const& value, std::string* output) {
  if (value.IsHolding<std::string>()) {
    *output = value.UncheckedGet<std::string>();
  }
}

extern uint64_t VtValueToulong(VtValue const& value) {
  if (value.IsHolding<uint64_t>()) {
    return value.UncheckedGet<uint64_t>();
  }
  return uint64_t();
}
extern void VtValueToulong(VtValue const& value, uint64_t* output) {
  if (value.IsHolding<uint64_t>()) {
    *output = value.UncheckedGet<uint64_t>();
  }
}

extern unsigned char VtValueTobyte(VtValue const& value) {
  if (value.IsHolding<unsigned char>()) {
    return value.UncheckedGet<unsigned char>();
  }
  return (unsigned char)(0);
}
extern void VtValueTobyte(VtValue const& value, unsigned char* output) {
  if (value.IsHolding<unsigned char>()) {
    *output = value.UncheckedGet<unsigned char>();
  }
}

extern unsigned int VtValueTouint(VtValue const& value) {
  if (value.IsHolding<unsigned int>()) {
    return value.UncheckedGet<unsigned int>();
  }
  return (unsigned int)(0);
}
extern void VtValueTouint(VtValue const& value, unsigned int* output) {
  if (value.IsHolding<unsigned int>()) {
    *output = value.UncheckedGet<unsigned int>();
  }
}
%}

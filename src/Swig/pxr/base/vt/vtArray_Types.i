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


CSHARP_ARRAYS(SdfAssetPath, SdfAssetPath);
WRAP_EQUAL(SdfAssetPathArray)
%template (SdfAssetPathArray) VtArray<SdfAssetPath>;
typedef VtArray<SdfAssetPath> SdfAssetPathArray;

CSHARP_ARRAYS(bool, bool);
WRAP_EQUAL(VtBoolArray)
%template (VtBoolArray) VtArray<bool>;
typedef VtArray<bool> VtBoolArray;

CSHARP_ARRAYS(double, double);
WRAP_EQUAL(VtDoubleArray)
%template (VtDoubleArray) VtArray<double>;
typedef VtArray<double> VtDoubleArray;

CSHARP_ARRAYS(float, float);
WRAP_EQUAL(VtFloatArray)
%template (VtFloatArray) VtArray<float>;
typedef VtArray<float> VtFloatArray;

CSHARP_ARRAYS(GfHalf, GfHalf);
WRAP_EQUAL(VtHalfArray)
%template (VtHalfArray) VtArray<GfHalf>;
typedef VtArray<GfHalf> VtHalfArray;

CSHARP_ARRAYS(int64_t, long);
WRAP_EQUAL(VtInt64Array)
%template (VtInt64Array) VtArray<int64_t>;
typedef VtArray<int64_t> VtInt64Array;

CSHARP_ARRAYS(int, int);
WRAP_EQUAL(VtIntArray)
%template (VtIntArray) VtArray<int>;
typedef VtArray<int> VtIntArray;

CSHARP_ARRAYS(GfMatrix2d, GfMatrix2d);
WRAP_EQUAL(VtMatrix2dArray)
%template (VtMatrix2dArray) VtArray<GfMatrix2d>;
typedef VtArray<GfMatrix2d> VtMatrix2dArray;

CSHARP_ARRAYS(GfMatrix3d, GfMatrix3d);
WRAP_EQUAL(VtMatrix3dArray)
%template (VtMatrix3dArray) VtArray<GfMatrix3d>;
typedef VtArray<GfMatrix3d> VtMatrix3dArray;

CSHARP_ARRAYS(GfMatrix4d, GfMatrix4d);
WRAP_EQUAL(VtMatrix4dArray)
%template (VtMatrix4dArray) VtArray<GfMatrix4d>;
typedef VtArray<GfMatrix4d> VtMatrix4dArray;

CSHARP_ARRAYS(GfQuatd, GfQuatd);
WRAP_EQUAL(VtQuatdArray)
%template (VtQuatdArray) VtArray<GfQuatd>;
typedef VtArray<GfQuatd> VtQuatdArray;

CSHARP_ARRAYS(GfQuatf, GfQuatf);
WRAP_EQUAL(VtQuatfArray)
%template (VtQuatfArray) VtArray<GfQuatf>;
typedef VtArray<GfQuatf> VtQuatfArray;

CSHARP_ARRAYS(GfQuath, GfQuath);
WRAP_EQUAL(VtQuathArray)
%template (VtQuathArray) VtArray<GfQuath>;
typedef VtArray<GfQuath> VtQuathArray;

CSHARP_ARRAYS(std::string, string);
WRAP_EQUAL(VtStringArray)
%template (VtStringArray) VtArray<std::string >;
typedef VtArray<std::string > VtStringArray;

CSHARP_ARRAYS(TfToken, TfToken);
WRAP_EQUAL(VtTokenArray)
%template (VtTokenArray) VtArray<TfToken>;
typedef VtArray<TfToken> VtTokenArray;

CSHARP_ARRAYS(unsigned char, byte);
WRAP_EQUAL(VtUCharArray)
%template (VtUCharArray) VtArray<unsigned char>;
typedef VtArray<unsigned char> VtUCharArray;

CSHARP_ARRAYS(uint64_t, ulong);
WRAP_EQUAL(VtUInt64Array)
%template (VtUInt64Array) VtArray<uint64_t>;
typedef VtArray<uint64_t> VtUInt64Array;

CSHARP_ARRAYS(unsigned int, uint);
WRAP_EQUAL(VtUIntArray)
%template (VtUIntArray) VtArray<unsigned int>;
typedef VtArray<unsigned int> VtUIntArray;

CSHARP_ARRAYS(GfVec2d, GfVec2d);
WRAP_EQUAL(VtVec2dArray)
%template (VtVec2dArray) VtArray<GfVec2d>;
typedef VtArray<GfVec2d> VtVec2dArray;

CSHARP_ARRAYS(GfVec2f, GfVec2f);
WRAP_EQUAL(VtVec2fArray)
%template (VtVec2fArray) VtArray<GfVec2f>;
typedef VtArray<GfVec2f> VtVec2fArray;

CSHARP_ARRAYS(GfVec2h, GfVec2h);
WRAP_EQUAL(VtVec2hArray)
%template (VtVec2hArray) VtArray<GfVec2h>;
typedef VtArray<GfVec2h> VtVec2hArray;

CSHARP_ARRAYS(GfVec2i, GfVec2i);
WRAP_EQUAL(VtVec2iArray)
%template (VtVec2iArray) VtArray<GfVec2i>;
typedef VtArray<GfVec2i> VtVec2iArray;

CSHARP_ARRAYS(GfVec3d, GfVec3d);
WRAP_EQUAL(VtVec3dArray)
%template (VtVec3dArray) VtArray<GfVec3d>;
typedef VtArray<GfVec3d> VtVec3dArray;

CSHARP_ARRAYS(GfVec3f, GfVec3f);
WRAP_EQUAL(VtVec3fArray)
%template (VtVec3fArray) VtArray<GfVec3f>;
typedef VtArray<GfVec3f> VtVec3fArray;

CSHARP_ARRAYS(GfVec3h, GfVec3h);
WRAP_EQUAL(VtVec3hArray)
%template (VtVec3hArray) VtArray<GfVec3h>;
typedef VtArray<GfVec3h> VtVec3hArray;

CSHARP_ARRAYS(GfVec3i, GfVec3i);
WRAP_EQUAL(VtVec3iArray)
%template (VtVec3iArray) VtArray<GfVec3i>;
typedef VtArray<GfVec3i> VtVec3iArray;

CSHARP_ARRAYS(GfVec4d, GfVec4d);
WRAP_EQUAL(VtVec4dArray)
%template (VtVec4dArray) VtArray<GfVec4d>;
typedef VtArray<GfVec4d> VtVec4dArray;

CSHARP_ARRAYS(GfVec4f, GfVec4f);
WRAP_EQUAL(VtVec4fArray)
%template (VtVec4fArray) VtArray<GfVec4f>;
typedef VtArray<GfVec4f> VtVec4fArray;

CSHARP_ARRAYS(GfVec4h, GfVec4h);
WRAP_EQUAL(VtVec4hArray)
%template (VtVec4hArray) VtArray<GfVec4h>;
typedef VtArray<GfVec4h> VtVec4hArray;

CSHARP_ARRAYS(GfVec4i, GfVec4i);
WRAP_EQUAL(VtVec4iArray)
%template (VtVec4iArray) VtArray<GfVec4i>;
typedef VtArray<GfVec4i> VtVec4iArray;



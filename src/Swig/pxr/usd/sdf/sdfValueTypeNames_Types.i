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
    SdfValueTypeName SdfGetValueTypeBool() { return SdfValueTypeNames->Bool; }
    SdfValueTypeName SdfGetValueTypeUChar() { return SdfValueTypeNames->UChar; }
    SdfValueTypeName SdfGetValueTypeInt() { return SdfValueTypeNames->Int; }
    SdfValueTypeName SdfGetValueTypeUInt() { return SdfValueTypeNames->UInt; }
    SdfValueTypeName SdfGetValueTypeInt64() { return SdfValueTypeNames->Int64; }
    SdfValueTypeName SdfGetValueTypeUInt64() { return SdfValueTypeNames->UInt64; }
    SdfValueTypeName SdfGetValueTypeHalf() { return SdfValueTypeNames->Half; }
    SdfValueTypeName SdfGetValueTypeFloat() { return SdfValueTypeNames->Float; }
    SdfValueTypeName SdfGetValueTypeDouble() { return SdfValueTypeNames->Double; }
    SdfValueTypeName SdfGetValueTypeTimeCode() { return SdfValueTypeNames->TimeCode; }
    SdfValueTypeName SdfGetValueTypeString() { return SdfValueTypeNames->String; }
    SdfValueTypeName SdfGetValueTypeToken() { return SdfValueTypeNames->Token; }
    SdfValueTypeName SdfGetValueTypeAsset() { return SdfValueTypeNames->Asset; }
    SdfValueTypeName SdfGetValueTypeInt2() { return SdfValueTypeNames->Int2; }
    SdfValueTypeName SdfGetValueTypeInt3() { return SdfValueTypeNames->Int3; }
    SdfValueTypeName SdfGetValueTypeInt4() { return SdfValueTypeNames->Int4; }
    SdfValueTypeName SdfGetValueTypeHalf2() { return SdfValueTypeNames->Half2; }
    SdfValueTypeName SdfGetValueTypeHalf3() { return SdfValueTypeNames->Half3; }
    SdfValueTypeName SdfGetValueTypeHalf4() { return SdfValueTypeNames->Half4; }
    SdfValueTypeName SdfGetValueTypeFloat2() { return SdfValueTypeNames->Float2; }
    SdfValueTypeName SdfGetValueTypeFloat3() { return SdfValueTypeNames->Float3; }
    SdfValueTypeName SdfGetValueTypeFloat4() { return SdfValueTypeNames->Float4; }
    SdfValueTypeName SdfGetValueTypeDouble2() { return SdfValueTypeNames->Double2; }
    SdfValueTypeName SdfGetValueTypeDouble3() { return SdfValueTypeNames->Double3; }
    SdfValueTypeName SdfGetValueTypeDouble4() { return SdfValueTypeNames->Double4; }
    SdfValueTypeName SdfGetValueTypePoint3h() { return SdfValueTypeNames->Point3h; }
    SdfValueTypeName SdfGetValueTypePoint3f() { return SdfValueTypeNames->Point3f; }
    SdfValueTypeName SdfGetValueTypePoint3d() { return SdfValueTypeNames->Point3d; }
    SdfValueTypeName SdfGetValueTypeVector3h() { return SdfValueTypeNames->Vector3h; }
    SdfValueTypeName SdfGetValueTypeVector3f() { return SdfValueTypeNames->Vector3f; }
    SdfValueTypeName SdfGetValueTypeVector3d() { return SdfValueTypeNames->Vector3d; }
    SdfValueTypeName SdfGetValueTypeNormal3h() { return SdfValueTypeNames->Normal3h; }
    SdfValueTypeName SdfGetValueTypeNormal3f() { return SdfValueTypeNames->Normal3f; }
    SdfValueTypeName SdfGetValueTypeNormal3d() { return SdfValueTypeNames->Normal3d; }
    SdfValueTypeName SdfGetValueTypeColor3h() { return SdfValueTypeNames->Color3h; }
    SdfValueTypeName SdfGetValueTypeColor3f() { return SdfValueTypeNames->Color3f; }
    SdfValueTypeName SdfGetValueTypeColor3d() { return SdfValueTypeNames->Color3d; }
    SdfValueTypeName SdfGetValueTypeColor4h() { return SdfValueTypeNames->Color4h; }
    SdfValueTypeName SdfGetValueTypeColor4f() { return SdfValueTypeNames->Color4f; }
    SdfValueTypeName SdfGetValueTypeColor4d() { return SdfValueTypeNames->Color4d; }
    SdfValueTypeName SdfGetValueTypeQuath() { return SdfValueTypeNames->Quath; }
    SdfValueTypeName SdfGetValueTypeQuatf() { return SdfValueTypeNames->Quatf; }
    SdfValueTypeName SdfGetValueTypeQuatd() { return SdfValueTypeNames->Quatd; }
    SdfValueTypeName SdfGetValueTypeMatrix2d() { return SdfValueTypeNames->Matrix2d; }
    SdfValueTypeName SdfGetValueTypeMatrix3d() { return SdfValueTypeNames->Matrix3d; }
    SdfValueTypeName SdfGetValueTypeMatrix4d() { return SdfValueTypeNames->Matrix4d; }
    SdfValueTypeName SdfGetValueTypeFrame4d() { return SdfValueTypeNames->Frame4d; }
    SdfValueTypeName SdfGetValueTypeTexCoord2h() { return SdfValueTypeNames->TexCoord2h; }
    SdfValueTypeName SdfGetValueTypeTexCoord2f() { return SdfValueTypeNames->TexCoord2f; }
    SdfValueTypeName SdfGetValueTypeTexCoord2d() { return SdfValueTypeNames->TexCoord2d; }
    SdfValueTypeName SdfGetValueTypeTexCoord3h() { return SdfValueTypeNames->TexCoord3h; }
    SdfValueTypeName SdfGetValueTypeTexCoord3f() { return SdfValueTypeNames->TexCoord3f; }
    SdfValueTypeName SdfGetValueTypeTexCoord3d() { return SdfValueTypeNames->TexCoord3d; }
    SdfValueTypeName SdfGetValueTypeBoolArray() { return SdfValueTypeNames->BoolArray; }
    SdfValueTypeName SdfGetValueTypeUCharArray() { return SdfValueTypeNames->UCharArray; }
    SdfValueTypeName SdfGetValueTypeIntArray() { return SdfValueTypeNames->IntArray; }
    SdfValueTypeName SdfGetValueTypeUIntArray() { return SdfValueTypeNames->UIntArray; }
    SdfValueTypeName SdfGetValueTypeInt64Array() { return SdfValueTypeNames->Int64Array; }
    SdfValueTypeName SdfGetValueTypeUInt64Array() { return SdfValueTypeNames->UInt64Array; }
    SdfValueTypeName SdfGetValueTypeHalfArray() { return SdfValueTypeNames->HalfArray; }
    SdfValueTypeName SdfGetValueTypeFloatArray() { return SdfValueTypeNames->FloatArray; }
    SdfValueTypeName SdfGetValueTypeDoubleArray() { return SdfValueTypeNames->DoubleArray; }
    SdfValueTypeName SdfGetValueTypeTimeCodeArray() { return SdfValueTypeNames->TimeCodeArray; }
    SdfValueTypeName SdfGetValueTypeStringArray() { return SdfValueTypeNames->StringArray; }
    SdfValueTypeName SdfGetValueTypeTokenArray() { return SdfValueTypeNames->TokenArray; }
    SdfValueTypeName SdfGetValueTypeAssetArray() { return SdfValueTypeNames->AssetArray; }
    SdfValueTypeName SdfGetValueTypeInt2Array() { return SdfValueTypeNames->Int2Array; }
    SdfValueTypeName SdfGetValueTypeInt3Array() { return SdfValueTypeNames->Int3Array; }
    SdfValueTypeName SdfGetValueTypeInt4Array() { return SdfValueTypeNames->Int4Array; }
    SdfValueTypeName SdfGetValueTypeHalf2Array() { return SdfValueTypeNames->Half2Array; }
    SdfValueTypeName SdfGetValueTypeHalf3Array() { return SdfValueTypeNames->Half3Array; }
    SdfValueTypeName SdfGetValueTypeHalf4Array() { return SdfValueTypeNames->Half4Array; }
    SdfValueTypeName SdfGetValueTypeFloat2Array() { return SdfValueTypeNames->Float2Array; }
    SdfValueTypeName SdfGetValueTypeFloat3Array() { return SdfValueTypeNames->Float3Array; }
    SdfValueTypeName SdfGetValueTypeFloat4Array() { return SdfValueTypeNames->Float4Array; }
    SdfValueTypeName SdfGetValueTypeDouble2Array() { return SdfValueTypeNames->Double2Array; }
    SdfValueTypeName SdfGetValueTypeDouble3Array() { return SdfValueTypeNames->Double3Array; }
    SdfValueTypeName SdfGetValueTypeDouble4Array() { return SdfValueTypeNames->Double4Array; }
    SdfValueTypeName SdfGetValueTypePoint3hArray() { return SdfValueTypeNames->Point3hArray; }
    SdfValueTypeName SdfGetValueTypePoint3fArray() { return SdfValueTypeNames->Point3fArray; }
    SdfValueTypeName SdfGetValueTypePoint3dArray() { return SdfValueTypeNames->Point3dArray; }
    SdfValueTypeName SdfGetValueTypeVector3hArray() { return SdfValueTypeNames->Vector3hArray; }
    SdfValueTypeName SdfGetValueTypeVector3fArray() { return SdfValueTypeNames->Vector3fArray; }
    SdfValueTypeName SdfGetValueTypeVector3dArray() { return SdfValueTypeNames->Vector3dArray; }
    SdfValueTypeName SdfGetValueTypeNormal3hArray() { return SdfValueTypeNames->Normal3hArray; }
    SdfValueTypeName SdfGetValueTypeNormal3fArray() { return SdfValueTypeNames->Normal3fArray; }
    SdfValueTypeName SdfGetValueTypeNormal3dArray() { return SdfValueTypeNames->Normal3dArray; }
    SdfValueTypeName SdfGetValueTypeColor3hArray() { return SdfValueTypeNames->Color3hArray; }
    SdfValueTypeName SdfGetValueTypeColor3fArray() { return SdfValueTypeNames->Color3fArray; }
    SdfValueTypeName SdfGetValueTypeColor3dArray() { return SdfValueTypeNames->Color3dArray; }
    SdfValueTypeName SdfGetValueTypeColor4hArray() { return SdfValueTypeNames->Color4hArray; }
    SdfValueTypeName SdfGetValueTypeColor4fArray() { return SdfValueTypeNames->Color4fArray; }
    SdfValueTypeName SdfGetValueTypeColor4dArray() { return SdfValueTypeNames->Color4dArray; }
    SdfValueTypeName SdfGetValueTypeQuathArray() { return SdfValueTypeNames->QuathArray; }
    SdfValueTypeName SdfGetValueTypeQuatfArray() { return SdfValueTypeNames->QuatfArray; }
    SdfValueTypeName SdfGetValueTypeQuatdArray() { return SdfValueTypeNames->QuatdArray; }
    SdfValueTypeName SdfGetValueTypeMatrix2dArray() { return SdfValueTypeNames->Matrix2dArray; }
    SdfValueTypeName SdfGetValueTypeMatrix3dArray() { return SdfValueTypeNames->Matrix3dArray; }
    SdfValueTypeName SdfGetValueTypeMatrix4dArray() { return SdfValueTypeNames->Matrix4dArray; }
    SdfValueTypeName SdfGetValueTypeFrame4dArray() { return SdfValueTypeNames->Frame4dArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord2hArray() { return SdfValueTypeNames->TexCoord2hArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord2fArray() { return SdfValueTypeNames->TexCoord2fArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord2dArray() { return SdfValueTypeNames->TexCoord2dArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord3hArray() { return SdfValueTypeNames->TexCoord3hArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord3fArray() { return SdfValueTypeNames->TexCoord3fArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord3dArray() { return SdfValueTypeNames->TexCoord3dArray; }
%}

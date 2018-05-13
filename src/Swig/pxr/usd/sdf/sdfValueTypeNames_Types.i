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
    SdfValueTypeName SdfGetValueTypeColor4dArray() { return SdfValueTypeNames->Color4dArray; }
    SdfValueTypeName SdfGetValueTypeMatrix3d() { return SdfValueTypeNames->Matrix3d; }
    SdfValueTypeName SdfGetValueTypeTexCoord3dArray() { return SdfValueTypeNames->TexCoord3dArray; }
    SdfValueTypeName SdfGetValueTypePoint3h() { return SdfValueTypeNames->Point3h; }
    SdfValueTypeName SdfGetValueTypeAsset() { return SdfValueTypeNames->Asset; }
    SdfValueTypeName SdfGetValueTypePoint3d() { return SdfValueTypeNames->Point3d; }
    SdfValueTypeName SdfGetValueTypePoint3f() { return SdfValueTypeNames->Point3f; }
    SdfValueTypeName SdfGetValueTypeQuathArray() { return SdfValueTypeNames->QuathArray; }
    SdfValueTypeName SdfGetValueTypeUChar() { return SdfValueTypeNames->UChar; }
    SdfValueTypeName SdfGetValueTypeMatrix2dArray() { return SdfValueTypeNames->Matrix2dArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord2fArray() { return SdfValueTypeNames->TexCoord2fArray; }
    SdfValueTypeName SdfGetValueTypeVector3dArray() { return SdfValueTypeNames->Vector3dArray; }
    SdfValueTypeName SdfGetValueTypeColor4h() { return SdfValueTypeNames->Color4h; }
    SdfValueTypeName SdfGetValueTypeColor4f() { return SdfValueTypeNames->Color4f; }
    SdfValueTypeName SdfGetValueTypeColor4d() { return SdfValueTypeNames->Color4d; }
    SdfValueTypeName SdfGetValueTypeFloat2Array() { return SdfValueTypeNames->Float2Array; }
    SdfValueTypeName SdfGetValueTypeFloat4Array() { return SdfValueTypeNames->Float4Array; }
    SdfValueTypeName SdfGetValueTypeAssetArray() { return SdfValueTypeNames->AssetArray; }
    SdfValueTypeName SdfGetValueTypeUInt() { return SdfValueTypeNames->UInt; }
    SdfValueTypeName SdfGetValueTypeNormal3fArray() { return SdfValueTypeNames->Normal3fArray; }
    SdfValueTypeName SdfGetValueTypeToken() { return SdfValueTypeNames->Token; }
    SdfValueTypeName SdfGetValueTypeFloat3Array() { return SdfValueTypeNames->Float3Array; }
    SdfValueTypeName SdfGetValueTypeTexCoord2hArray() { return SdfValueTypeNames->TexCoord2hArray; }
    SdfValueTypeName SdfGetValueTypeUCharArray() { return SdfValueTypeNames->UCharArray; }
    SdfValueTypeName SdfGetValueTypeDoubleArray() { return SdfValueTypeNames->DoubleArray; }
    SdfValueTypeName SdfGetValueTypeFrame4d() { return SdfValueTypeNames->Frame4d; }
    SdfValueTypeName SdfGetValueTypeQuatdArray() { return SdfValueTypeNames->QuatdArray; }
    SdfValueTypeName SdfGetValueTypePoint3hArray() { return SdfValueTypeNames->Point3hArray; }
    SdfValueTypeName SdfGetValueTypeString() { return SdfValueTypeNames->String; }
    SdfValueTypeName SdfGetValueTypeDouble4() { return SdfValueTypeNames->Double4; }
    SdfValueTypeName SdfGetValueTypeNormal3dArray() { return SdfValueTypeNames->Normal3dArray; }
    SdfValueTypeName SdfGetValueTypeColor3dArray() { return SdfValueTypeNames->Color3dArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord2dArray() { return SdfValueTypeNames->TexCoord2dArray; }
    SdfValueTypeName SdfGetValueTypeHalf3Array() { return SdfValueTypeNames->Half3Array; }
    SdfValueTypeName SdfGetValueTypeMatrix4dArray() { return SdfValueTypeNames->Matrix4dArray; }
    SdfValueTypeName SdfGetValueTypeNormal3h() { return SdfValueTypeNames->Normal3h; }
    SdfValueTypeName SdfGetValueTypeColor3fArray() { return SdfValueTypeNames->Color3fArray; }
    SdfValueTypeName SdfGetValueTypeColor4hArray() { return SdfValueTypeNames->Color4hArray; }
    SdfValueTypeName SdfGetValueTypeNormal3d() { return SdfValueTypeNames->Normal3d; }
    SdfValueTypeName SdfGetValueTypeMatrix2d() { return SdfValueTypeNames->Matrix2d; }
    SdfValueTypeName SdfGetValueTypeNormal3f() { return SdfValueTypeNames->Normal3f; }
    SdfValueTypeName SdfGetValueTypeUInt64() { return SdfValueTypeNames->UInt64; }
    SdfValueTypeName SdfGetValueTypeFloatArray() { return SdfValueTypeNames->FloatArray; }
    SdfValueTypeName SdfGetValueTypeColor3f() { return SdfValueTypeNames->Color3f; }
    SdfValueTypeName SdfGetValueTypeColor3d() { return SdfValueTypeNames->Color3d; }
    SdfValueTypeName SdfGetValueTypeInt64Array() { return SdfValueTypeNames->Int64Array; }
    SdfValueTypeName SdfGetValueTypeIntArray() { return SdfValueTypeNames->IntArray; }
    SdfValueTypeName SdfGetValueTypePoint3dArray() { return SdfValueTypeNames->Point3dArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord3hArray() { return SdfValueTypeNames->TexCoord3hArray; }
    SdfValueTypeName SdfGetValueTypeBool() { return SdfValueTypeNames->Bool; }
    SdfValueTypeName SdfGetValueTypeColor3hArray() { return SdfValueTypeNames->Color3hArray; }
    SdfValueTypeName SdfGetValueTypeDouble2() { return SdfValueTypeNames->Double2; }
    SdfValueTypeName SdfGetValueTypeDouble3() { return SdfValueTypeNames->Double3; }
    SdfValueTypeName SdfGetValueTypeUInt64Array() { return SdfValueTypeNames->UInt64Array; }
    SdfValueTypeName SdfGetValueTypeStringArray() { return SdfValueTypeNames->StringArray; }
    SdfValueTypeName SdfGetValueTypeVector3hArray() { return SdfValueTypeNames->Vector3hArray; }
    SdfValueTypeName SdfGetValueTypeMatrix3dArray() { return SdfValueTypeNames->Matrix3dArray; }
    SdfValueTypeName SdfGetValueTypeQuatf() { return SdfValueTypeNames->Quatf; }
    SdfValueTypeName SdfGetValueTypeDouble3Array() { return SdfValueTypeNames->Double3Array; }
    SdfValueTypeName SdfGetValueTypeQuatd() { return SdfValueTypeNames->Quatd; }
    SdfValueTypeName SdfGetValueTypeQuath() { return SdfValueTypeNames->Quath; }
    SdfValueTypeName SdfGetValueTypeInt4() { return SdfValueTypeNames->Int4; }
    SdfValueTypeName SdfGetValueTypeHalf4Array() { return SdfValueTypeNames->Half4Array; }
    SdfValueTypeName SdfGetValueTypeInt2() { return SdfValueTypeNames->Int2; }
    SdfValueTypeName SdfGetValueTypePoint3fArray() { return SdfValueTypeNames->Point3fArray; }
    SdfValueTypeName SdfGetValueTypeInt3() { return SdfValueTypeNames->Int3; }
    SdfValueTypeName SdfGetValueTypeBoolArray() { return SdfValueTypeNames->BoolArray; }
    SdfValueTypeName SdfGetValueTypeHalf2Array() { return SdfValueTypeNames->Half2Array; }
    SdfValueTypeName SdfGetValueTypeMatrix4d() { return SdfValueTypeNames->Matrix4d; }
    SdfValueTypeName SdfGetValueTypeInt() { return SdfValueTypeNames->Int; }
    SdfValueTypeName SdfGetValueTypeDouble4Array() { return SdfValueTypeNames->Double4Array; }
    SdfValueTypeName SdfGetValueTypeHalfArray() { return SdfValueTypeNames->HalfArray; }
    SdfValueTypeName SdfGetValueTypeFloat4() { return SdfValueTypeNames->Float4; }
    SdfValueTypeName SdfGetValueTypeFloat2() { return SdfValueTypeNames->Float2; }
    SdfValueTypeName SdfGetValueTypeFloat3() { return SdfValueTypeNames->Float3; }
    SdfValueTypeName SdfGetValueTypeUIntArray() { return SdfValueTypeNames->UIntArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord2d() { return SdfValueTypeNames->TexCoord2d; }
    SdfValueTypeName SdfGetValueTypeHalf3() { return SdfValueTypeNames->Half3; }
    SdfValueTypeName SdfGetValueTypeTexCoord3h() { return SdfValueTypeNames->TexCoord3h; }
    SdfValueTypeName SdfGetValueTypeFrame4dArray() { return SdfValueTypeNames->Frame4dArray; }
    SdfValueTypeName SdfGetValueTypeVector3fArray() { return SdfValueTypeNames->Vector3fArray; }
    SdfValueTypeName SdfGetValueTypeTexCoord3d() { return SdfValueTypeNames->TexCoord3d; }
    SdfValueTypeName SdfGetValueTypeTexCoord3f() { return SdfValueTypeNames->TexCoord3f; }
    SdfValueTypeName SdfGetValueTypeColor3h() { return SdfValueTypeNames->Color3h; }
    SdfValueTypeName SdfGetValueTypeHalf() { return SdfValueTypeNames->Half; }
    SdfValueTypeName SdfGetValueTypeVector3h() { return SdfValueTypeNames->Vector3h; }
    SdfValueTypeName SdfGetValueTypeVector3d() { return SdfValueTypeNames->Vector3d; }
    SdfValueTypeName SdfGetValueTypeDouble() { return SdfValueTypeNames->Double; }
    SdfValueTypeName SdfGetValueTypeVector3f() { return SdfValueTypeNames->Vector3f; }
    SdfValueTypeName SdfGetValueTypeInt4Array() { return SdfValueTypeNames->Int4Array; }
    SdfValueTypeName SdfGetValueTypeColor4fArray() { return SdfValueTypeNames->Color4fArray; }
    SdfValueTypeName SdfGetValueTypeQuatfArray() { return SdfValueTypeNames->QuatfArray; }
    SdfValueTypeName SdfGetValueTypeDouble2Array() { return SdfValueTypeNames->Double2Array; }
    SdfValueTypeName SdfGetValueTypeTokenArray() { return SdfValueTypeNames->TokenArray; }
    SdfValueTypeName SdfGetValueTypeNormal3hArray() { return SdfValueTypeNames->Normal3hArray; }
    SdfValueTypeName SdfGetValueTypeInt3Array() { return SdfValueTypeNames->Int3Array; }
    SdfValueTypeName SdfGetValueTypeTexCoord2h() { return SdfValueTypeNames->TexCoord2h; }
    SdfValueTypeName SdfGetValueTypeHalf4() { return SdfValueTypeNames->Half4; }
    SdfValueTypeName SdfGetValueTypeFloat() { return SdfValueTypeNames->Float; }
    SdfValueTypeName SdfGetValueTypeTexCoord2f() { return SdfValueTypeNames->TexCoord2f; }
    SdfValueTypeName SdfGetValueTypeHalf2() { return SdfValueTypeNames->Half2; }
    SdfValueTypeName SdfGetValueTypeInt2Array() { return SdfValueTypeNames->Int2Array; }
    SdfValueTypeName SdfGetValueTypeInt64() { return SdfValueTypeNames->Int64; }
    SdfValueTypeName SdfGetValueTypeTexCoord3fArray() { return SdfValueTypeNames->TexCoord3fArray; }
%}

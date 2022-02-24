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
using pxr;
public static class SdfValueTypeNames
{
    static public SdfValueTypeName Bool = UsdCs.SdfGetValueTypeBool();
    static public SdfValueTypeName UChar = UsdCs.SdfGetValueTypeUChar();
    static public SdfValueTypeName Int = UsdCs.SdfGetValueTypeInt();
    static public SdfValueTypeName UInt = UsdCs.SdfGetValueTypeUInt();
    static public SdfValueTypeName Int64 = UsdCs.SdfGetValueTypeInt64();
    static public SdfValueTypeName UInt64 = UsdCs.SdfGetValueTypeUInt64();
    static public SdfValueTypeName Half = UsdCs.SdfGetValueTypeHalf();
    static public SdfValueTypeName Float = UsdCs.SdfGetValueTypeFloat();
    static public SdfValueTypeName Double = UsdCs.SdfGetValueTypeDouble();
    static public SdfValueTypeName TimeCode = UsdCs.SdfGetValueTypeTimeCode();
    static public SdfValueTypeName String = UsdCs.SdfGetValueTypeString();
    static public SdfValueTypeName Token = UsdCs.SdfGetValueTypeToken();
    static public SdfValueTypeName Asset = UsdCs.SdfGetValueTypeAsset();
    static public SdfValueTypeName Int2 = UsdCs.SdfGetValueTypeInt2();
    static public SdfValueTypeName Int3 = UsdCs.SdfGetValueTypeInt3();
    static public SdfValueTypeName Int4 = UsdCs.SdfGetValueTypeInt4();
    static public SdfValueTypeName Half2 = UsdCs.SdfGetValueTypeHalf2();
    static public SdfValueTypeName Half3 = UsdCs.SdfGetValueTypeHalf3();
    static public SdfValueTypeName Half4 = UsdCs.SdfGetValueTypeHalf4();
    static public SdfValueTypeName Float2 = UsdCs.SdfGetValueTypeFloat2();
    static public SdfValueTypeName Float3 = UsdCs.SdfGetValueTypeFloat3();
    static public SdfValueTypeName Float4 = UsdCs.SdfGetValueTypeFloat4();
    static public SdfValueTypeName Double2 = UsdCs.SdfGetValueTypeDouble2();
    static public SdfValueTypeName Double3 = UsdCs.SdfGetValueTypeDouble3();
    static public SdfValueTypeName Double4 = UsdCs.SdfGetValueTypeDouble4();
    static public SdfValueTypeName Point3h = UsdCs.SdfGetValueTypePoint3h();
    static public SdfValueTypeName Point3f = UsdCs.SdfGetValueTypePoint3f();
    static public SdfValueTypeName Point3d = UsdCs.SdfGetValueTypePoint3d();
    static public SdfValueTypeName Vector3h = UsdCs.SdfGetValueTypeVector3h();
    static public SdfValueTypeName Vector3f = UsdCs.SdfGetValueTypeVector3f();
    static public SdfValueTypeName Vector3d = UsdCs.SdfGetValueTypeVector3d();
    static public SdfValueTypeName Normal3h = UsdCs.SdfGetValueTypeNormal3h();
    static public SdfValueTypeName Normal3f = UsdCs.SdfGetValueTypeNormal3f();
    static public SdfValueTypeName Normal3d = UsdCs.SdfGetValueTypeNormal3d();
    static public SdfValueTypeName Color3h = UsdCs.SdfGetValueTypeColor3h();
    static public SdfValueTypeName Color3f = UsdCs.SdfGetValueTypeColor3f();
    static public SdfValueTypeName Color3d = UsdCs.SdfGetValueTypeColor3d();
    static public SdfValueTypeName Color4h = UsdCs.SdfGetValueTypeColor4h();
    static public SdfValueTypeName Color4f = UsdCs.SdfGetValueTypeColor4f();
    static public SdfValueTypeName Color4d = UsdCs.SdfGetValueTypeColor4d();
    static public SdfValueTypeName Quath = UsdCs.SdfGetValueTypeQuath();
    static public SdfValueTypeName Quatf = UsdCs.SdfGetValueTypeQuatf();
    static public SdfValueTypeName Quatd = UsdCs.SdfGetValueTypeQuatd();
    static public SdfValueTypeName Matrix2d = UsdCs.SdfGetValueTypeMatrix2d();
    static public SdfValueTypeName Matrix3d = UsdCs.SdfGetValueTypeMatrix3d();
    static public SdfValueTypeName Matrix4d = UsdCs.SdfGetValueTypeMatrix4d();
    static public SdfValueTypeName Frame4d = UsdCs.SdfGetValueTypeFrame4d();
    static public SdfValueTypeName TexCoord2h = UsdCs.SdfGetValueTypeTexCoord2h();
    static public SdfValueTypeName TexCoord2f = UsdCs.SdfGetValueTypeTexCoord2f();
    static public SdfValueTypeName TexCoord2d = UsdCs.SdfGetValueTypeTexCoord2d();
    static public SdfValueTypeName TexCoord3h = UsdCs.SdfGetValueTypeTexCoord3h();
    static public SdfValueTypeName TexCoord3f = UsdCs.SdfGetValueTypeTexCoord3f();
    static public SdfValueTypeName TexCoord3d = UsdCs.SdfGetValueTypeTexCoord3d();
    static public SdfValueTypeName BoolArray = UsdCs.SdfGetValueTypeBoolArray();
    static public SdfValueTypeName UCharArray = UsdCs.SdfGetValueTypeUCharArray();
    static public SdfValueTypeName IntArray = UsdCs.SdfGetValueTypeIntArray();
    static public SdfValueTypeName UIntArray = UsdCs.SdfGetValueTypeUIntArray();
    static public SdfValueTypeName Int64Array = UsdCs.SdfGetValueTypeInt64Array();
    static public SdfValueTypeName UInt64Array = UsdCs.SdfGetValueTypeUInt64Array();
    static public SdfValueTypeName HalfArray = UsdCs.SdfGetValueTypeHalfArray();
    static public SdfValueTypeName FloatArray = UsdCs.SdfGetValueTypeFloatArray();
    static public SdfValueTypeName DoubleArray = UsdCs.SdfGetValueTypeDoubleArray();
    static public SdfValueTypeName TimeCodeArray = UsdCs.SdfGetValueTypeTimeCodeArray();
    static public SdfValueTypeName StringArray = UsdCs.SdfGetValueTypeStringArray();
    static public SdfValueTypeName TokenArray = UsdCs.SdfGetValueTypeTokenArray();
    static public SdfValueTypeName AssetArray = UsdCs.SdfGetValueTypeAssetArray();
    static public SdfValueTypeName Int2Array = UsdCs.SdfGetValueTypeInt2Array();
    static public SdfValueTypeName Int3Array = UsdCs.SdfGetValueTypeInt3Array();
    static public SdfValueTypeName Int4Array = UsdCs.SdfGetValueTypeInt4Array();
    static public SdfValueTypeName Half2Array = UsdCs.SdfGetValueTypeHalf2Array();
    static public SdfValueTypeName Half3Array = UsdCs.SdfGetValueTypeHalf3Array();
    static public SdfValueTypeName Half4Array = UsdCs.SdfGetValueTypeHalf4Array();
    static public SdfValueTypeName Float2Array = UsdCs.SdfGetValueTypeFloat2Array();
    static public SdfValueTypeName Float3Array = UsdCs.SdfGetValueTypeFloat3Array();
    static public SdfValueTypeName Float4Array = UsdCs.SdfGetValueTypeFloat4Array();
    static public SdfValueTypeName Double2Array = UsdCs.SdfGetValueTypeDouble2Array();
    static public SdfValueTypeName Double3Array = UsdCs.SdfGetValueTypeDouble3Array();
    static public SdfValueTypeName Double4Array = UsdCs.SdfGetValueTypeDouble4Array();
    static public SdfValueTypeName Point3hArray = UsdCs.SdfGetValueTypePoint3hArray();
    static public SdfValueTypeName Point3fArray = UsdCs.SdfGetValueTypePoint3fArray();
    static public SdfValueTypeName Point3dArray = UsdCs.SdfGetValueTypePoint3dArray();
    static public SdfValueTypeName Vector3hArray = UsdCs.SdfGetValueTypeVector3hArray();
    static public SdfValueTypeName Vector3fArray = UsdCs.SdfGetValueTypeVector3fArray();
    static public SdfValueTypeName Vector3dArray = UsdCs.SdfGetValueTypeVector3dArray();
    static public SdfValueTypeName Normal3hArray = UsdCs.SdfGetValueTypeNormal3hArray();
    static public SdfValueTypeName Normal3fArray = UsdCs.SdfGetValueTypeNormal3fArray();
    static public SdfValueTypeName Normal3dArray = UsdCs.SdfGetValueTypeNormal3dArray();
    static public SdfValueTypeName Color3hArray = UsdCs.SdfGetValueTypeColor3hArray();
    static public SdfValueTypeName Color3fArray = UsdCs.SdfGetValueTypeColor3fArray();
    static public SdfValueTypeName Color3dArray = UsdCs.SdfGetValueTypeColor3dArray();
    static public SdfValueTypeName Color4hArray = UsdCs.SdfGetValueTypeColor4hArray();
    static public SdfValueTypeName Color4fArray = UsdCs.SdfGetValueTypeColor4fArray();
    static public SdfValueTypeName Color4dArray = UsdCs.SdfGetValueTypeColor4dArray();
    static public SdfValueTypeName QuathArray = UsdCs.SdfGetValueTypeQuathArray();
    static public SdfValueTypeName QuatfArray = UsdCs.SdfGetValueTypeQuatfArray();
    static public SdfValueTypeName QuatdArray = UsdCs.SdfGetValueTypeQuatdArray();
    static public SdfValueTypeName Matrix2dArray = UsdCs.SdfGetValueTypeMatrix2dArray();
    static public SdfValueTypeName Matrix3dArray = UsdCs.SdfGetValueTypeMatrix3dArray();
    static public SdfValueTypeName Matrix4dArray = UsdCs.SdfGetValueTypeMatrix4dArray();
    static public SdfValueTypeName Frame4dArray = UsdCs.SdfGetValueTypeFrame4dArray();
    static public SdfValueTypeName TexCoord2hArray = UsdCs.SdfGetValueTypeTexCoord2hArray();
    static public SdfValueTypeName TexCoord2fArray = UsdCs.SdfGetValueTypeTexCoord2fArray();
    static public SdfValueTypeName TexCoord2dArray = UsdCs.SdfGetValueTypeTexCoord2dArray();
    static public SdfValueTypeName TexCoord3hArray = UsdCs.SdfGetValueTypeTexCoord3hArray();
    static public SdfValueTypeName TexCoord3fArray = UsdCs.SdfGetValueTypeTexCoord3fArray();
    static public SdfValueTypeName TexCoord3dArray = UsdCs.SdfGetValueTypeTexCoord3dArray();
}

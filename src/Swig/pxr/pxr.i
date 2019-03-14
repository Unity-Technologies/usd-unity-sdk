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

%module pxr

/* TODO:
 *   1) Upgrade Swig to get multiple cscode blocks and fix operator[] on GfVec*
 */

%{
#include "pxr/base/tf/hash.h"
%}
%include "stl.i"

%include "std_vector.i"
%include "std_string.i"

// for uint64_t
%include "stdint.i"

namespace std {
  %template(StdStringVector) vector<std::string>;
}
typedef std::vector<std::string> StdStringVector;

namespace std {
  %template(StdIntVector) vector<int>;
}
typedef std::vector<int> StdIntVector;

namespace std {
  %template(StdUIntVector) vector<unsigned int>;
}
typedef std::vector<unsigned int> StdUIntVector;

namespace std {
  %template(StdUInt64Vector) vector<uint64_t>;
}
typedef std::vector<uint64_t> StdUInt64Vector;

namespace std {
  %template(StdFloatVector) vector<float>;
}
typedef std::vector<float> StdFloatVector;

namespace std {
  %template(StdFloatVectorVector) vector< vector<float> >;
}
typedef vector< vector<float> > StdFloatVectorVector;

namespace std {
  %template(StdDoubleVector) vector<double>;
}
typedef std::vector<double> StdDoubleVector;

namespace std {
  %template(StdDoubleVectorVector) vector< vector<double> >;
}
typedef std::vector< vector<double> > StdDoubleVectorVector;

#define PXR_USE_NAMESPACES 0
#include "pxr/pxr.h"

#define PXR_NAMESPACE_OPEN_SCOPE 
#define PXR_NAMESPACE_CLOSE_SCOPE 

// Included for TF_DECLARE_WEAK_PTRS
%include "pxr/base/tf/declarePtrs.h"

%include "std_string.i"
%include "typemaps.i"
%include "stdint.i"  // Required for int64_t and friends

%include "operators.i"

%include "base/base.i"
%include "usd/usd.i"

%include "callback.i"

%{
#include "pxr/base/arch/env.h"
#include "pxr/usd/usdGeom/xformable.h"
%}

%inline %{

void SetEnv(std::string name, std::string value) {
  ArchSetEnv(name, value, true);
}

VtValue GetFusedTransform(UsdPrim prim, UsdTimeCode time) {
  VtValue value;
  
  if (!prim) {
    return value;
  }

  UsdGeomXformable xf(prim);
  GfMatrix4d mat;
  bool resetsXfStack = false;
  if (!xf.GetLocalTransformation(&mat, &resetsXfStack, time)) {
    return VtValue();
  }

  return VtValue(mat);
}

bool WriteUsdZip(const std::string& usdzFilePath,
                 const std::vector<std::string>& filesToArchive) {
  auto writer = UsdZipFileWriter::CreateNew(usdzFilePath);
  for (auto filePath : filesToArchive) {
    // Empty string indicates failure.
    writer.AddFile(filePath);
  }
  return writer.Save();
}

VtValue GetFusedDisplayColor(UsdPrim prim, UsdTimeCode time) {
  VtValue value;
  
  if (!prim) {
    return value;
  }

  auto rgbAttr = prim.GetAttribute(UsdGeomTokens->primvarsDisplayColor);
  auto alphaAttr = prim.GetAttribute(UsdGeomTokens->primvarsDisplayOpacity);

  size_t n = 0;
  VtVec3fArray rgb;
  VtFloatArray alpha;
  if (rgbAttr) { rgbAttr.Get(&rgb, time); }
  if (alphaAttr) { alphaAttr.Get(&alpha, time); }

  n = rgb.size() > alpha.size() ? rgb.size() : alpha.size();

  if (n == 0) {
    return value;
  }

  VtVec4fArray fused(n, GfVec4f(1, 1, 1, 1));
  for (size_t i = 0; i < n; i++) {
    if (i < rgb.size()) {
      fused[i][0] = rgb[i][0];
      fused[i][1] = rgb[i][1];
      fused[i][2] = rgb[i][2];
    }
    if (i < alpha.size()) {
      fused[i][3] = alpha[i];
    }
  }

  return VtValue(fused);
}

bool SetFusedDisplayColor(UsdPrim prim, VtVec4fArray values, UsdTimeCode time) {
  if (!prim) { return false; }

  UsdGeomMesh mesh(prim);
  auto rgbPv = mesh.CreateDisplayColorAttr();
  auto alphaPv = mesh.CreateDisplayOpacityAttr();
  
  mesh.GetDisplayColorPrimvar().SetInterpolation(UsdGeomTokens->vertex);
  mesh.GetDisplayOpacityPrimvar().SetInterpolation(UsdGeomTokens->vertex);

  size_t n = values.size();
  VtVec3fArray rgb(n);
  VtFloatArray alpha(n);

  for (size_t i = 0; i < n; i++) {
    rgb[i][0] = values[i][0];
    rgb[i][1] = values[i][1];
    rgb[i][2] = values[i][2];
    alpha[i] = values[i][3];
  }

  return rgbPv.Set(rgb, time) && alphaPv.Set(alpha, time);
}

%}

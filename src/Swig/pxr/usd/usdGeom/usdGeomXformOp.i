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

%module UsdGeomXformOp

%{
#include "pxr/usd/usdGeom/xformOp.h"
%}

%include "std_vector.i"
namespace std {
  %template(UsdGeomXformOpVector) vector<UsdGeomXformOp>;
}
typedef std::vector<UsdGeomXformOp> UsdGeomXformOpVector;

%ignore UsdGeomXformOp::UsdGeomXformOp(const UsdAttribute &attr, bool isInverseOp, _ValidAttributeTagType);
%ignore UsdGeomXformOp::UsdGeomXformOp(UsdAttributeQuery &&query, bool isInverseOp, _ValidAttributeTagType);

#define TF_DECLARE_PUBLIC_TOKENS(UsdGeomXformOpTypes, USDGEOM_API, USDGEOM_XFORM_OP_TYPES)

%include "pxr/usd/usdGeom/xformOp.h"

%extend UsdGeomXformOp {
  bool Set(GfMatrix4d const & value, UsdTimeCode time = UsdTimeCode::Default()) const {
    return self->Set(value, time);
  }
  //bool Set(GfVec3f const & value, UsdTimeCode time = UsdTimeCode::Default()) const;
  //bool Set(GfVec3d const & value, UsdTimeCode time = UsdTimeCode::Default()) const;
  //bool Set(float const & value, UsdTimeCode time = UsdTimeCode::Default()) const;
}

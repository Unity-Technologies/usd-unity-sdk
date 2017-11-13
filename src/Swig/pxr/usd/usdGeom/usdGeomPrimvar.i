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

%module UsdGeomPrimvar

%{
#include "pxr/usd/usdGeom/primvar.h"
%}

%include "typemaps.i"

%apply int *OUTPUT { int *elementSize }

%typemap(cstype) TfToken * "/*cstype*/ out TfToken"
%typemap(csin,
   pre="    $csclassname temp$csinput = new $csclassname();",
  post="    $csinput = temp$csinput;"
) TfToken * "$csclassname.getCPtr(temp$csinput)"

%typemap(cstype) SdfValueTypeName * "/*cstype*/ out SdfValueTypeName"
%typemap(csin,
   pre="    $csclassname temp$csinput = new $csclassname();",
  post="    $csinput = temp$csinput;"
) SdfValueTypeName * "$csclassname.getCPtr(temp$csinput)"

%typemap(cstype) VtIntArray * "out VtIntArray"
%typemap(csin,
   pre="    $csclassname temp$csinput = new $csclassname();",
  post="    $csinput = temp$csinput;"
) VtIntArray * "$csclassname.getCPtr(temp$csinput)"

%include "std_vector.i"
namespace std {
	%template(UsdGeomPrimvarVector) vector<UsdGeomPrimvar>;
}
typedef std::vector<UsdGeomPrimvar> UsdGeomPrimvarVector;

%include "pxr/usd/usdGeom/primvar.h"

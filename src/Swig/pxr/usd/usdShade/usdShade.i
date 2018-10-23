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

%module UsdShade

#define USDSHADE_API

// ---------------------------------------------------------------------------------------------- //
// Similar to csharp/typemaps.i definitions for OUTPUT types
// ---------------------------------------------------------------------------------------------- //
%define OUTPUT_TYPEMAP(TYPE, CTYPE, CSTYPE, TYPECHECKPRECEDENCE)
%typemap(ctype) TYPE *OUTPUT, TYPE &OUTPUT "CTYPE *"
%typemap(imtype) TYPE *OUTPUT, TYPE &OUTPUT "out CSTYPE"
%typemap(cstype) TYPE *OUTPUT, TYPE &OUTPUT "out CSTYPE"
%typemap(csin) TYPE *OUTPUT, TYPE &OUTPUT "out $csinput"

%typemap(in) TYPE *OUTPUT, TYPE &OUTPUT
%{ $1 = ($1_ltype)$input; %}

%typecheck(SWIG_TYPECHECK_##TYPECHECKPRECEDENCE) TYPE *OUTPUT, TYPE &OUTPUT ""
%enddef

//
// Add a special mapping for UsdShadeAttributeType
//
OUTPUT_TYPEMAP(UsdShadeAttributeType, UsdShadeAttributeType, UsdShadeAttributeType, INT32_PTR)

#undef OUTPUT_TYPEMAP
// ---------------------------------------------------------------------------------------------- //

%include "usdShadeUtils.i"
%include "usdShadeNodeGraph.i"
%include "usdShadeMaterial.i"
%include "usdShadeMaterialBindingApi.i"
%include "usdShadeShader.i"
%include "usdShadeShaderDefParser.i"
%include "usdShadeShaderDefUtils.i"
%include "usdShadeInput.i"
%include "usdShadeOutput.i"
%include "usdShadeConnectableAPI.i"

%include "usdShadeTokens.i"

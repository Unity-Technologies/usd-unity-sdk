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

%module gfQuath

%{
#include "pxr/base/gf/quath.h"
%}

IGNORE_OPERATORS(GfMatrix2d)
WRAP_EQUAL(GfQuath)

// Error  LNK2019  unresolved external symbol "public: __cdecl GfQuath::GfQuath(class GfQuatd const &)" (??0GfQuath@@QEAA@AEBVGfQuatd@@@Z) referenced in function CSharp_pxr_new_GfQuath__SWIG_4  UsdCs  C:\src\usd\UsdBindings\vs2015\UsdCs\usdCs_wrap.obj
//%include "pxr/base/gf/quath.h"

class GfQuath { };
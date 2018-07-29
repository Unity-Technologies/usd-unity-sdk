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

%module gfMatrix3f

%{
#include "pxr/base/gf/matrix3f.h"
%}

IGNORE_OPERATORS(GfMatrix2d)

%ignore GfMatrix3f(const float m[3][3]);
%ignore GfMatrix3f::Set(const float m[3][3]);
%ignore GfMatrix3f::Get(float m[3][3]);
%ignore GfMatrix3f::GetArray;

%include <arrays_csharp.i>

WRAP_EQUAL(GfMatrix3f)

%include "pxr/base/gf/matrix3f.h"

%extend GfMatrix3f {
  %csmethodmodifiers ToString() "public override";
    std::string ToString() {
      std::stringstream s;
    s << *self;
    return s.str();
  }

  %apply float OUTPUT[] { float* dest }
    void CopyToArray(float* dest) {
    memcpy(dest, self->GetArray(), 9 * sizeof(float)); 
  }

  %apply float INPUT[] { float* src }
  void CopyFromArray(float* src) { 
    memcpy(self->GetArray(), src, 9 * sizeof(float));
  }
}
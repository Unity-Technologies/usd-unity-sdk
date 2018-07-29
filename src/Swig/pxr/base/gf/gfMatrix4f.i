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

%module gfMatrix4f

%{
#include "pxr/base/gf/matrix4f.h"
%}

IGNORE_OPERATORS(GfMatrix2d)

%ignore GfMatrix4f(const float m[4][4]);
%ignore GfMatrix4f::Set(const float m[4][4]);
%ignore GfMatrix4f::Get(float m[4][4]);

// XXX: Look into why these fail to compile.
%ignore GfMatrix4f::GfMatrix4f(const GfMatrix3f& rotmx, const GfVec3f& translate);;
%ignore GfMatrix4f::RemoveScaleShear;
%ignore GfMatrix4f::GetArray;

%include <arrays_csharp.i>

WRAP_EQUAL(GfMatrix4f)

%include "pxr/base/gf/matrix4f.h"

%extend GfMatrix4f {
  %csmethodmodifiers ToString() "public override";
    std::string ToString() {
      std::stringstream s;
    s << *self;
    return s.str();
  }

  %apply float OUTPUT[] { float* dest }
    void CopyToArray(float* dest) {
    memcpy(dest, self->GetArray(), 16 * sizeof(float)); 
  }

  %apply float INPUT[] { float* src }
  void CopyFromArray(float* src) { 
    memcpy(self->GetArray(), src, 16 * sizeof(float));
  }
}
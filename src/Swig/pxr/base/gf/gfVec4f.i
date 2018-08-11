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

%module gfVec4f

%{
#include "pxr/base/gf/vec4f.h"
%}

WRAP_EQUAL(GfVec4f)

%ignore GfVec4f::Set(float const* a);
%ignore GfVec4f::GetArray() const;
%ignore GfVec4f::data();
%ignore GfVec4f::data() const;
%ignore GfVec4f::operator[](size_t);
%ignore GfVec4f::operator[](size_t) const;
%ignore GfVec4f::operator-() const;
%ignore operator-(GfVec4f const &, GfVec4f const &);
%ignore operator+(GfVec4f const &, GfVec4f const &);
%ignore GfVec4f::operator+=(GfVec4f const &);
%ignore GfVec4f::operator-=(GfVec4f const &);
%ignore GfVec4f::operator*(double) const;
%ignore GfVec4f::operator*=(double);
%ignore GfVec4f::operator*=(double) const;
%ignore operator*(double, GfVec4f const &);
%ignore GfVec4f::operator*(GfVec4f const &) const;
%ignore GfVec4f::operator/(double) const;
%ignore GfVec4f::operator/=(double);
%ignore GfVec4f::operator/=(double) const;
%ignore operator<<(std::ostream &, GfVec4f const &);

namespace std {
  %template (GfVec4fVector) vector<GfVec4f>;
}

%include "pxr/base/gf/vec4f.h"

%extend GfVec4f {
  %csmethodmodifiers GetValue(int index) "protected";
  float GetValue(int index) {
    return (*self)[index];
  }
  
  %csmethodmodifiers SetValue(int index, float value) "protected";
  void SetValue(int index, float value) {
    (*self)[index] = value;
  }
  
  %proxycode %{
  public float this[int index] {
    get { return GetValue(index); }
    set { SetValue(index, value); }
  }
  %}
}

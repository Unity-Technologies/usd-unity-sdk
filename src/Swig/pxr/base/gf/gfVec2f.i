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

%module gfVec2f

%{
#include "pxr/base/gf/vec2f.h"
%}

WRAP_EQUAL(GfVec2f)

%ignore GfVec2f::Set(float const* a);
%ignore GfVec2f::GetArray() const;
%ignore GfVec2f::data();
%ignore GfVec2f::data() const;
%ignore GfVec2f::operator[](size_t);
%ignore GfVec2f::operator[](size_t) const;
%ignore GfVec2f::operator-() const;
%ignore operator-(GfVec2f const &, GfVec2f const &);
%ignore operator+(GfVec2f const &, GfVec2f const &);
%ignore GfVec2f::operator+=(GfVec2f const &);
%ignore GfVec2f::operator-=(GfVec2f const &);
%ignore GfVec2f::operator*(double) const;
%ignore GfVec2f::operator*=(double);
%ignore GfVec2f::operator*=(double) const;
%ignore operator*(double, GfVec2f const &);
%ignore GfVec2f::operator*(GfVec2f const &) const;
%ignore GfVec2f::operator/(double) const;
%ignore GfVec2f::operator/=(double);
%ignore GfVec2f::operator/=(double) const;
%ignore operator<<(std::ostream &, GfVec2f const &);

%include "pxr/base/gf/vec2f.h"

%extend GfVec2f {
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
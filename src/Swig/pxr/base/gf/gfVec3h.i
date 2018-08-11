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

%module gfVec3d

%{
#include "pxr/base/gf/vec3h.h"
%}

WRAP_EQUAL(GfVec3h)

%ignore GfVec3h::Set(int const* a);
%ignore GfVec3h::GetArray() const;
%ignore GfVec3h::data();
%ignore GfVec3h::data() const;
%ignore GfVec3h::operator[](size_t);
%ignore GfVec3h::operator[](size_t) const;
%ignore GfVec3h::operator-() const;
%ignore operator-(GfVec3h const &, GfVec3h const &);
%ignore operator+(GfVec3h const &, GfVec3h const &);
%ignore GfVec3h::operator+=(GfVec3h const &);
%ignore GfVec3h::operator-=(GfVec3h const &);
%ignore GfVec3h::operator*(double) const;
%ignore GfVec3h::operator*=(double);
%ignore GfVec3h::operator*=(double) const;
%ignore operator*(double, GfVec3h const &);
%ignore GfVec3h::operator*(GfVec3h const &) const;
%ignore GfVec3h::operator/(double) const;
%ignore GfVec3h::operator/=(double);
%ignore GfVec3h::operator/=(double) const;
%ignore operator<<(std::ostream &, GfVec3h const &);

%include "pxr/base/gf/vec3h.h"

%extend GfVec3h {
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
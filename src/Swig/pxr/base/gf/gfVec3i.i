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

%module gfVec3i

%{
#include "pxr/base/gf/vec3i.h"
%}

WRAP_EQUAL(GfVec3i)

%ignore GfVec3i::Set(int const* a);
%ignore GfVec3i::GetArray() const;
%ignore GfVec3i::data();
%ignore GfVec3i::data() const;
%ignore GfVec3i::operator[](size_t);
%ignore GfVec3i::operator[](size_t) const;
%ignore GfVec3i::operator-() const;
%ignore operator-(GfVec3i const &, GfVec3i const &);
%ignore operator+(GfVec3i const &, GfVec3i const &);
%ignore GfVec3i::operator+=(GfVec3i const &);
%ignore GfVec3i::operator-=(GfVec3i const &);
%ignore GfVec3i::operator*(double) const;
%ignore GfVec3i::operator*=(double);
%ignore GfVec3i::operator*=(double) const;
%ignore operator*(double, GfVec3i const &);
%ignore GfVec3i::operator*(GfVec3i const &) const;
%ignore GfVec3i::operator/(int) const;
%ignore GfVec3i::operator/=(int);
%ignore GfVec3i::operator/=(int) const;
%ignore operator<<(std::ostream &, GfVec3i const &);

%include "pxr/base/gf/vec3i.h"

%extend GfVec3i {
  %csmethodmodifiers GetValue(int index) "protected";
  int GetValue(int index) {
    return (*self)[index];
  }
  
  %csmethodmodifiers SetValue(int index, int value) "protected";
  void SetValue(int index, int value) {
    (*self)[index] = value;
  }

  %proxycode %{
  public int this[int index] {
    get { return GetValue(index); }
    set { SetValue(index, value); }
  }
  %}
}
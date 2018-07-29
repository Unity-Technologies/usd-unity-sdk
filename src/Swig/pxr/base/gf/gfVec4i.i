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

%module gfVec4i

%{
#include "pxr/base/gf/vec4i.h"
%}

WRAP_EQUAL(GfVec4i)

%ignore GfVec4i::Set(int const* a);
%ignore GfVec4i::GetArray() const;
%ignore GfVec4i::data();
%ignore GfVec4i::data() const;
%ignore GfVec4i::operator[](size_t);
%ignore GfVec4i::operator[](size_t) const;
%ignore GfVec4i::operator-() const;
%ignore operator-(GfVec4i const &, GfVec4i const &);
%ignore operator+(GfVec4i const &, GfVec4i const &);
%ignore GfVec4i::operator+=(GfVec4i const &);
%ignore GfVec4i::operator-=(GfVec4i const &);
%ignore GfVec4i::operator*(double) const;
%ignore GfVec4i::operator*=(double);
%ignore GfVec4i::operator*=(double) const;
%ignore operator*(double, GfVec4i const &);
%ignore GfVec4i::operator*(GfVec4i const &) const;
%ignore GfVec4i::operator/(int) const;
%ignore GfVec4i::operator/=(int);
%ignore GfVec4i::operator/=(int) const;
%ignore operator<<(std::ostream &, GfVec4i const &);

%include "pxr/base/gf/vec4i.h"

%extend GfVec4i {
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
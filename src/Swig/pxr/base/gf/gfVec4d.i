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

%module gfVec4d

%{
#include "pxr/base/gf/vec4d.h"
%}

WRAP_EQUAL(GfVec4d)

%ignore GfVec4d::Set(double const* a);
%ignore GfVec4d::GetArray() const;
%ignore GfVec4d::data();
%ignore GfVec4d::data() const;
%ignore GfVec4d::operator[](size_t);
%ignore GfVec4d::operator[](size_t) const;
%ignore GfVec4d::operator-() const;
%ignore operator-(GfVec4d const &, GfVec4d const &);
%ignore operator+(GfVec4d const &, GfVec4d const &);
%ignore GfVec4d::operator+=(GfVec4d const &);
%ignore GfVec4d::operator-=(GfVec4d const &);
%ignore GfVec4d::operator*(double) const;
%ignore GfVec4d::operator*=(double);
%ignore GfVec4d::operator*=(double) const;
%ignore operator*(double, GfVec4d const &);
%ignore GfVec4d::operator*(GfVec4d const &) const;
%ignore GfVec4d::operator/(double) const;
%ignore GfVec4d::operator/=(double);
%ignore GfVec4d::operator/=(double) const;
%ignore operator<<(std::ostream &, GfVec4d const &);

%include "pxr/base/gf/vec4d.h"

%extend GfVec4d {
  %csmethodmodifiers GetValue(int index) "protected";
  double GetValue(int index) {
    return (*self)[index];
  }
  
  %csmethodmodifiers SetValue(int index, double value) "protected";
  void SetValue(int index, double value) {
    (*self)[index] = value;
  }

  %proxycode %{
  public double this[int index] {
    get { return GetValue(index); }
    set { SetValue(index, value); }
  }
  %}
}
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

%module gfVec2d

%{
#include "pxr/base/gf/vec2d.h"
%}

WRAP_EQUAL(GfVec2d)

%ignore GfVec2d::Set(double const* a);
%ignore GfVec2d::GetArray() const;
%ignore GfVec2d::data();
%ignore GfVec2d::data() const;
%ignore GfVec2d::operator[](size_t);
%ignore GfVec2d::operator[](size_t) const;
%ignore GfVec2d::operator-() const;
%ignore operator-(GfVec2d const &, GfVec2d const &);
%ignore operator+(GfVec2d const &, GfVec2d const &);
%ignore GfVec2d::operator+=(GfVec2d const &);
%ignore GfVec2d::operator-=(GfVec2d const &);
%ignore GfVec2d::operator*(double) const;
%ignore GfVec2d::operator*=(double);
%ignore GfVec2d::operator*=(double) const;
%ignore operator*(double, GfVec2d const &);
%ignore GfVec2d::operator*(GfVec2d const &) const;
%ignore GfVec2d::operator/(double) const;
%ignore GfVec2d::operator/=(double);
%ignore GfVec2d::operator/=(double) const;
%ignore operator<<(std::ostream &, GfVec2d const &);

%include "pxr/base/gf/vec2d.h"

%extend GfVec2d {
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
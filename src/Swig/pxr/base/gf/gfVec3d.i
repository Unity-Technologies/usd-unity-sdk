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
#include "pxr/base/gf/vec3d.h"
%}

WRAP_EQUAL(GfVec3d)

%ignore GfVec3d::Set(double const* a);
%ignore GfVec3d::GetArray() const;
%ignore GfVec3d::data();
%ignore GfVec3d::data() const;
%ignore GfVec3d::operator[](size_t);
%ignore GfVec3d::operator[](size_t) const;
%ignore GfVec3d::operator-() const;
%ignore operator-(GfVec3d const &, GfVec3d const &);
%ignore operator+(GfVec3d const &, GfVec3d const &);
%ignore GfVec3d::operator+=(GfVec3d const &);
%ignore GfVec3d::operator-=(GfVec3d const &);
%ignore GfVec3d::operator*(double) const;
%ignore GfVec3d::operator*=(double);
%ignore GfVec3d::operator*=(double) const;
%ignore operator*(double, GfVec3d const &);
%ignore GfVec3d::operator*(GfVec3d const &) const;
%ignore GfVec3d::operator/(double) const;
%ignore GfVec3d::operator/=(double);
%ignore GfVec3d::operator/=(double) const;
%ignore operator<<(std::ostream &, GfVec3d const &);

%include "pxr/base/gf/vec3d.h"

%extend GfVec3d {
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
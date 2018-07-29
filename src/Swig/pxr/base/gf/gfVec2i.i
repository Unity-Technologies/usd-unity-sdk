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

%module gfVec2i

%{
#include "pxr/base/gf/vec2i.h"
%}

WRAP_EQUAL(GfVec2i)

%ignore GfVec2i::Set(int const* a);
%ignore GfVec2i::GetArray() const;
%ignore GfVec2i::data();
%ignore GfVec2i::data() const;
%ignore GfVec2i::operator[](size_t);
%ignore GfVec2i::operator[](size_t) const;
%ignore GfVec2i::operator-() const;
%ignore operator-(GfVec2i const &, GfVec2i const &);
%ignore operator+(GfVec2i const &, GfVec2i const &);
%ignore GfVec2i::operator+=(GfVec2i const &);
%ignore GfVec2i::operator-=(GfVec2i const &);
%ignore GfVec2i::operator*(double) const;
%ignore GfVec2i::operator*=(double);
%ignore GfVec2i::operator*=(double) const;
%ignore operator*(double, GfVec2i const &);
%ignore GfVec2i::operator*(GfVec2i const &) const;
%ignore GfVec2i::operator/(int) const;
%ignore GfVec2i::operator/=(int);
%ignore GfVec2i::operator/=(int) const;
%ignore operator<<(std::ostream &, GfVec2i const &);

%include "pxr/base/gf/vec2i.h"

%rename(op_LessThan) GfVec2i::operator<(const Class1&)const;

%extend GfVec2i {
  
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
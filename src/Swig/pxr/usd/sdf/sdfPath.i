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

%module sdfPath

%{
#include "pxr/usd/sdf/path.h"
%}

%include "std_string.i"

%ignore operator==;
%ignore operator!=;

%extend SdfPath {
  static bool Equals(SdfPath const& lhs, SdfPath const& rhs) {
  return lhs == rhs;
  }
}

%typemap(cscode) SdfPath %{
  public override int GetHashCode() {
    return (int)GetHash();
  }
    public static implicit operator string (SdfPath value) {
        return value.GetText();
    }
  public override string ToString()
  {
    return (string)this;
  }
  public static bool operator==(SdfPath lhs, SdfPath rhs){
      // The Swig binding glew will re-enter this operator comparing to null, so 
    // that case must be handled explicitly to avoid an infinite loop. This is still
    // not great, since it crosses the C#/C++ barrier twice. A better approache might
    // be to return a simple value from C++ that can be compared in C#.
        bool lnull = lhs as object == null;
        bool rnull = rhs as object == null;
        return (lnull == rnull) && ((lnull && rnull) || SdfPath.Equals(lhs, rhs));
    }
    public static bool operator !=(SdfPath lhs, SdfPath rhs) {
        return !(lhs == rhs);
    }
  override public bool Equals(object rhs) {
    return SdfPath.Equals(this, rhs as SdfPath);
  }
%}

%include "std_vector.i"
namespace std {
  %template(SdfPathVector) vector<SdfPath>;
  %template(SdfPathStringPair) pair<string, string>;
  %template(SdfPathPair) pair<SdfPath, SdfPath>;
}
typedef std::vector<SdfPath> SdfPathVector;

%inline {
  void GetPathFromVector(std::vector<SdfPath> const& paths, int index, SdfPath* output) {
    *output = paths[index];
  }
}

%include "pxr/usd/sdf/path.h"
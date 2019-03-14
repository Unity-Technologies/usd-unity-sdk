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

%module usdTimeCode

%{
#include "pxr/usd/usd/timeCode.h"
%}

%include "std_vector.i"
namespace std {
  %template(UsdTimeCodeVector) vector<UsdTimeCode>;
}

%typemap(cscode) UsdTimeCode %{
    public static implicit operator UsdTimeCode (double value) {
        return new UsdTimeCode(value);
    }
  public static bool operator==(UsdTimeCode lhs, UsdTimeCode rhs){
      // The Swig binding glew will re-enter this operator comparing to null, so 
    // that case must be handled explicitly to avoid an infinite loop. This is still
    // not great, since it crosses the C#/C++ barrier twice. A better approache might
    // be to return a simple value from C++ that can be compared in C#.
        bool lnull = lhs as object == null;
        bool rnull = rhs as object == null;
        return (lnull == rnull) && ((lnull && rnull) || UsdTimeCode.Equals(lhs, rhs));
    }
    public static bool operator !=(UsdTimeCode lhs, UsdTimeCode rhs) {
        return !(lhs == rhs);
    }
  override public bool Equals(object rhs) {
    return UsdTimeCode.Equals(this, rhs as UsdTimeCode);
  }
%}

//WRAP_EQUAL( UsdTimeCode )

%include "pxr/usd/usd/timeCode.h"


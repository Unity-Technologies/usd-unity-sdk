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

%module usdSchemaBase

%{
#include "pxr/usd/usd/schemaBase.h"
%}

// WRAP_EQUAL(UsdSchemaBase)
// WRAP_EQUAL doesn't work because we need to compare the held prims, not the schema objects.

%ignore UsdSchemaBase::schemaType;

%extend UsdSchemaBase {
  static bool Equals(UsdSchemaBase const& lhs, UsdSchemaBase const& rhs) {
    return bool(lhs) == bool(rhs) && (!bool(lhs) || lhs.GetPrim() == rhs.GetPrim());
  }

  %csmethodmodifiers GetHashCode() "override public";
  int GetHashCode() {
    return (int)TfHash()(self);
  }

  %proxycode %{
    public static bool operator==($typemap(cstype, UsdSchemaBase) lhs, $typemap(cstype, UsdSchemaBase) rhs){
      // The Swig binding glue will re-enter this operator comparing to null, so 
      // that case must be handled explicitly to avoid an infinite loop. This is still
      // not great, since it crosses the C#/C++ barrier twice. A better approache might
      // be to return a simple value from C++ that can be compared in C#.
      bool lnull = lhs as object == null;
      bool rnull = rhs as object == null;
      return (lnull == rnull) && ((lnull && rnull) || $typemap(cstype, UsdSchemaBase).Equals(lhs, rhs));
    }

    public static bool operator !=($typemap(cstype, UsdSchemaBase) lhs, $typemap(cstype, UsdSchemaBase) rhs) {
        return !(lhs == rhs);
    }

    override public bool Equals(object rhs) {
      return $typemap(cstype, UsdSchemaBase).Equals(this, rhs as $typemap(cstype, UsdSchemaBase));
    }
  %}
}


%typemap(cscode) UsdSchemaBase %{

  public static implicit operator bool(UsdSchemaBase b) {
    return b._IsValid();
  }

%}

// --

%include "pxr/usd/usd/schemaBase.h"

// --

%extend UsdSchemaBase {

  bool _IsValid() {
    return bool(*self);
  }

}
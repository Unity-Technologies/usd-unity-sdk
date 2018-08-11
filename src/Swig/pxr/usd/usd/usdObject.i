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

%module usdObject

%{
#include "pxr/usd/usd/object.h"
%}

%ignore operator==;
%ignore operator!=;

%typemap(cscode) UsdObject %{
  public static bool operator==(UsdObject lhs, UsdObject rhs){
    // The Swig binding glew will re-enter this operator comparing to null, so 
    // that case must be handled explicitly to avoid an infinite loop. This is still
    // not great, since it crosses the C#/C++ barrier twice. A better approache might
    // be to return a simple value from C++ that can be compared in C#.
    bool lnull = lhs as object == null;
    bool rnull = rhs as object == null;
    return (lnull == rnull) && ((lnull && rnull) || UsdObject.Equals(lhs, rhs));
  }
  public static bool operator !=(UsdObject lhs, UsdObject rhs) {
      return !(lhs == rhs);
  }
  override public bool Equals(object rhs) {
    return UsdObject.Equals(this, rhs as UsdObject);
  }

  public static implicit operator bool(UsdObject obj) {
    return obj._IsValid();
  }
%}

%ignore UsdPrim::_UnspecifiedBoolType() const;

%extend UsdObject {
  static bool Equals(UsdObject const& lhs, UsdObject const& rhs) {
  return lhs == rhs;
  }

  bool _IsValid() {
  return bool(*self);
  }

  VtValue GetMetadata(TfToken key) const {
  VtValue v;
  self->GetMetadata(key, &v);
  return v;
  }
  VtValue GetMetadataByDictKey(const TfToken& key, const TfToken &keyPath) const {
  VtValue v;
  self->GetMetadataByDictKey(key, keyPath, &v);
  return v;
  }
}
%ignore UsdObject::GetMetadata(const TfToken& key, VtValue* value) const;
%ignore UsdObject::GetMetadataByDictKey(const TfToken& key, const TfToken &keyPath, VtValue *value) const;
%ignore _Detail::Const;

#define BOOST_MPL_ASSERT_MSG(x,y,z)
%include "pxr/usd/usd/object.h"

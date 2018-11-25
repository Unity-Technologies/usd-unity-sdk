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
#include "pxr/base/vt/value.h"
#include "pxr/base/tf/token.h"
#include "pxr/usd/usd/common.h"
%}

%extend UsdMetadataValueMap {

  std::vector<TfToken> GetKeys() {
    std::vector<TfToken> ret;
    TF_FOR_ALL(kvp, *self) {
      ret.push_back(kvp->first);
    }
    return ret;
  }

  %csmethodmodifiers GetValue(TfToken const& key) "protected";
  VtValue const& GetValue(TfToken const& key) const {
    return self->at(key);
  }
  
  %csmethodmodifiers SetValue(TfToken const& key, VtValue const& value) "protected";
  void SetValue(TfToken const& key, VtValue const& value) {
    (*self)[key] = value;
  }
}

class UsdMetadataValueMap {
  %typemap(cscode) VtArray %{
  public $typemap(cstype, VtValue) this[$typemap(cstype, TfToken) index] {
  get { return GetValue(index); }
  set { SetValue(index, value); }
  }
  %}
};
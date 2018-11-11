// Copyright 2018 Google Inc. All rights reserved.
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

%module UsdSkelBinding
%{
#include "pxr/usd/usdSkel/binding.h"
%}

%include "std_vector.i"
%template(UsdSkelBindingVector) std::vector<UsdSkelBinding>;
%template(UsdSkelSkinningQueryVector) std::vector<UsdSkelSkinningQuery>;

%ignore UsdSkelBindingGetSkinningTargets();

%include "pxr/usd/usdSkel/binding.h"

//
// This overload of GetSkinningTargets is provided because VtArra<SkinningQuery> seems to have
// a compiler issue that is triggered by Swig. Issue filed:
// https://github.com/PixarAnimationStudios/USD/issues/670
//
%extend UsdSkelBinding {
  std::vector<UsdSkelSkinningQuery> GetSkinningTargetsAsVector() {
    std::vector<UsdSkelSkinningQuery> targets;
    for (auto&& p : self->GetSkinningTargets()) {
      targets.push_back(p);
    }
    return targets;
  }
}
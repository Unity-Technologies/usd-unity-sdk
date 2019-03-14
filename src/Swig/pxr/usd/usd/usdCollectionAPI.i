// Copyright 2018 Jeremy Cowles. All rights reserved.
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

%module usdCollectionAPI

%{
#include "pxr/usd/usd/collectionAPI.h"
%}

// TODO: std_set is not implemented in Swig for C# :( it would be nice to implement this and
//       send a pull request back to them.
// %include <std_set.i>

namespace std {
  %template(UsdObjectVector) vector<UsdObject>;
  %template(UsdCollectionAPIVector) vector<UsdCollectionAPI>;
}

%ignore UsdCollectionAPI::schemaType;

%ignore ComputeIncludedObjects(
    const MembershipQuery &query,
    const UsdStageWeakPtr &stage,
    const Usd_PrimFlagsPredicate &pred);

// Must be defined before interfaces to which it applies
%extend UsdCollectionAPI {
  static std::vector<UsdObject> ComputeIncludedObjects(const MembershipQuery &query, const UsdStageWeakPtr &stage) {
    std::set<UsdObject> tmp = UsdCollectionAPI::ComputeIncludedObjects(query, stage);
    std::vector<UsdObject> result(tmp.begin(), tmp.end());
    return result;
  }
}

// Declared but not defined.
// https://github.com/PixarAnimationStudios/USD/issues/669
%ignore UsdCollectionAPI::IsCollectionPath;

//%include "pxr/usd/usd/collectionAPI.h"
%include "third_party/include/pxr/usd/usd/collectionAPI.h"

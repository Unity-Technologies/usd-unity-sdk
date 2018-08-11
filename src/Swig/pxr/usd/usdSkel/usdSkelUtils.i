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

%module UsdSkelUtils
%{
#include "pxr/usd/usdSkel/utils.h"
%}

// Definition does not match declaration (link error).
%ignore UsdSkelComputeJointsExtent(const GfMatrix4d* xforms,
                           size_t numXforms,
                           VtVec3fArray* extent,
                           const GfVec3f& pad=GfVec3f(0,0,0),
                           const GfMatrix4d* rootXform=nullptr);

%include "pxr/usd/usdSkel/utils.h"

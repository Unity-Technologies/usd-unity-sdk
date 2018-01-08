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

%module usd

#define USD_API

%ignore boost::noncopyable;

%ignore Usd_SpecPathMapping;

%include "usdTimeCode.i"
%include "usdEditTarget.i"
%include "usdInterpolation.i"
%include "usdPrimFlags.i"
%include "usdPrimRange.i"

%include "usdStage.i"

%include "usdMetadataValueMap.i"
%include "usdObject.i"
%include "usdPrim.i"
%include "usdProperty.i"
%include "usdRelationship.i"
%include "usdAttribute.i"

%include "usdVariantSets.i"
%include "usdInherits.i"
%include "usdReferences.i"
%include "usdSpecializes.i"

%include "usdSchemaBase.i"
%include "usdTyped.i"


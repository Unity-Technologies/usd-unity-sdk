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

// ---------------------------------------------------------------------------------------------- //
// STAGE SMART POINTERS
// ---------------------------------------------------------------------------------------------- //

%TfRefPtr(UsdStage);

// The potential problem here is that the object is always held by the smart pointer type, but
// a without a reference, weak pointer expires immediately.
//
// XXX: First class weak ptrs still need work. Following the RefPtr pattern (as WeakPtr currently
//      does) is wrong.
//
//%TfWeakPtr(UsdStage);
class UsdStageWeakPtr{
public:
    UsdStageWeakPtr(UsdStage* stage);
    UsdStage const* operator->();
    // Required when there is no automagic smart pointer support enabled.
    // explicit UsdStageWeakPtr(UsdStageRefPtr const& stage);
};

typedef TfWeakPtr<UsdStage> UsdStageWeakPtr;
typedef UsdStageWeakPtr UsdStagePtr;
typedef TfRefPtr<UsdStage> UsdStageRefPtr;

// ---------------------------------------------------------------------------------------------- //

%include "usdMetadataValueMap.i"
%include "usdCommon.i"
%include "usdApiSchemaBase.i"
%include "usdModelApi.i"

%include "usdTimeCode.i"
%include "usdEditTarget.i"
%include "usdInterpolation.i"
%include "usdPrimFlags.i"
%include "usdPrimRange.i"

%include "usdStagePopulationMask.i"
%include "usdStage.i"
%include "usdStageCache.i"

%include "usdResolveInfo.i"
%include "usdObject.i"
%include "usdPrim.i"
%include "usdProperty.i"
%include "usdRelationship.i"
%include "usdAttribute.i"
%include "usdAttributeQuery.i"

%include "usdVariantSets.i"
%include "usdInherits.i"
%include "usdPayloads.i"
%include "usdReferences.i"
%include "usdSpecializes.i"

%include "usdClipsAPI.i"
%include "usdCollectionAPI.i"
%include "usdSchemaBase.i"
%include "usdTyped.i"

%include "usdZipFile.i"


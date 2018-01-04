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

%module usdPrim

%{
#include "pxr/usd/usd/prim.h"
#include "pxr/usd/usd/variantSets.h"
#include "pxr/usd/usd/inherits.h"
#include "pxr/usd/usd/references.h"
#include "pxr/usd/usd/specializes.h"
#include "pxr/usd/usd/property.h"
#include "pxr/usd/usd/attribute.h"
#include "pxr/usd/usd/relationship.h"
%}

%inline {
  void GetPrimFromVector(std::vector<UsdPrim> const& prims, int index, UsdPrim* output) {
    *output = prims[index];
  }
}

namespace std {
	%template(UsdPrimVector) vector<UsdPrim>;
}

typedef std::vector<UsdPrim> UsdPrimVector;

%extend UsdPrim {
  bool GetAttributeValue(TfToken attrName, VtValue* valueOut, UsdTimeCode time) {
    UsdAttribute attr = self->GetAttribute(attrName);
    if (!attr) { return false; }
    return attr.Get(valueOut, time);
  }
}

%ignore UsdPrim::FindAllRelationshipTargetPaths;
%ignore UsdPrim::FindAllAttributeConnectionPaths;

%include "third_party/include/pxr/usd/usd/prim.h"


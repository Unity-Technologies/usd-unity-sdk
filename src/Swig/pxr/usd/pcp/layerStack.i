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

%module pcplayerStack
%{
#include "pxr/usd/pcp/layerStack.h"
%}

%TfRefPtr(PcpLayerStack);
class PcpLayerStackPtr{
public:
    PcpLayerStackPtr(PcpLayerStack* ptr);
    PcpLayerStack const* operator->();
};

typedef TfWeakPtr<PcpLayerStack> PcpLayerStackPtr;
typedef TfRefPtr<PcpLayerStack> PcpLayerStackRefPtr;

%include <std_vector.i>
%template(PcpLayerStackPtrVector) std::vector< PcpLayerStackPtr >;
typedef std::vector< PcpLayerStackPtr > PcpLayerStackPtrVector;

%ignore Pcp_ComputeRelocationsForLayerStack(
    const SdfLayerRefPtrVector & layers,
    SdfRelocatesMap *relocatesSourceToTarget,
    SdfRelocatesMap *relocatesTargetToSource,
    SdfRelocatesMap *incrementalRelocatesSourceToTarget,
    SdfRelocatesMap *incrementalRelocatesTargetToSource,
    SdfPathVector *relocatesPrimPaths);

%ignore Pcp_NeedToRecomputeDueToAssetPathChange(const PcpLayerStackPtr& layerStack);
%include "pxr/usd/pcp/layerStack.h"


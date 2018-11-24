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

%module pcpnode
%{
#include "pxr/usd/pcp/node.h"

// Ultra hacky fix for:
// https://github.com/PixarAnimationStudios/USD/issues/704

// Hack part 1: include this private header to get sizeof(Graph).
#include "../../third_party/include/pxr/usd/pcp/primIndex_Graph.h"

// Hack part 2: in-line the implementation of GetUniqueIdentifier.
void* 
PcpNodeRef::GetUniqueIdentifier() const
{
    return _graph + _nodeIdx;
}

%}

%include <std_vector.i>
%template(PcpNodeRefVector) std::vector<PcpNodeRef>;

%ignore PcpNodeRef::GetUniqueIdentifier;
%ignore PcpNodeRef::GetOwningGraph;
%ignore PcpNodeRef::GetUniqueIdentifier;
%ignore PcpNodeRef::GetChildrenRange;

// Disabled until PcpLayerStackSite API is fixed:
// https://github.com/PixarAnimationStudios/USD/issues/703
%ignore PcpNodeRef::InsertChild;
%ignore PcpNodeRef::GetSite;

// Disabled until PcpNodeRef::Hash no longer references a missing external function:
// https://github.com/PixarAnimationStudios/USD/issues/704#L104
%ignore PcpNodeRef::Hash;

%ignore PcpNode_GetNonVariantPathElementCount;

%extend PcpNodeRef {
  std::vector<PcpNodeRef> GetChildren() {
    std::vector<PcpNodeRef> result;
    TF_FOR_ALL(child, self->GetChildrenRange()) {
      result.push_back(*child);
    }
    return result;
  }
}

%include "pxr/usd/pcp/node.h"


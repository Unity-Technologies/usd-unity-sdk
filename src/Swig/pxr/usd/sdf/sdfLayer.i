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

%module sdfLayer

%{
#include "pxr/usd/sdf/layer.h"
#include "pxr/usd/sdf/attributeSpec.h"
#include "pxr/usd/sdf/relationshipSpec.h"
%}

// Output double* parameters.
%apply double * INOUT { double * tUpper };
%apply double * INOUT { double * tLower };

%include "std_string.i"
%include "std_vector.i"

// ---------------------------------------------------------------------------------------------- //
// SMART POINTERS
// ---------------------------------------------------------------------------------------------- //

%template(SdfLayerHandleVector) std::vector<SdfLayerHandle>;
typedef std::vector<SdfLayerHandle> SdfLayerHandleVector;

// Must be defined before interfaces to which it applies
%extend SdfLayer {
  std::string ExportToString() const {
  std::string str;
  self->ExportToString(&str);
  return str;
  }
}
%ignore SdfLayer::ExportToString(std::string*) const;

%TfRefPtr(SdfLayer);
%TfRefPtr(SdfLayerBase);

typedef TfRefPtr<SdfLayer> SdfLayerRefPtr;

%template(SdfLayerRefPtrVector) std::vector<SdfLayerRefPtr>;
typedef std::vector<SdfLayerRefPtr> SdfLayerRefPtrVector;

class SdfLayerBase;

class SdfLayerHandle{
public:
    SdfLayerHandle(SdfLayer* layer);
    //explicit SdfLayerHandle(SdfLayerRefPtr const& layer);
    SdfLayer* operator->();
};

// ---------------------------------------------------------------------------------------------- //


%ignore SdfLayer::GetMetadata;
%ignore SdfLayer::GetSpecType(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::ListFields(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::ListTimeSamplesForPath(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::GetNumTimeSamplesForPath(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::GetBracketingTimeSamplesForPath(const SdfAbstractDataSpecId& id, 
                                         double time,
                                         double* tLower, double* tUpper);
%ignore SdfLayer::SetTimeSample(const SdfAbstractDataSpecId& id, double time, 
                       const VtValue & value);
%ignore SdfLayer::SetTimeSample(const SdfAbstractDataSpecId& id, double time, 
                       const SdfAbstractDataConstValue& value);
%ignore SdfLayer::EraseTimeSample(const SdfAbstractDataSpecId& id, double time);
%ignore SdfLayer::GetSpecType(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::ListFields(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::ListTimeSamplesForPath(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::GetRootPrimOrder;
%ignore SdfLayer::QueryTimeSample(const SdfAbstractDataSpecId& id, double time, 
                         VtValue *value=NULL) const;
%ignore SdfLayer::QueryTimeSample(const SdfAbstractDataSpecId& id, double time, 
                         SdfAbstractDataValue *value) const;
%ignore SdfLayer::HasSpec(const SdfAbstractDataSpecId& id) const;
%ignore SdfLayer::HasField(const SdfAbstractDataSpecId& id, const TfToken& fieldName,
        VtValue *value=NULL) const;
%ignore SdfLayer::HasField(const SdfAbstractDataSpecId& id, const TfToken& fieldName,
        SdfAbstractDataValue *value) const;
%ignore SdfLayer::HasFieldDictKey(const SdfAbstractDataSpecId& id,
                         const TfToken &fieldName,
                         const TfToken &keyPath,
                         VtValue *value=NULL) const;
%ignore SdfLayer::HasFieldDictKey(const SdfAbstractDataSpecId& id,
                         const TfToken &fieldName,
                         const TfToken &keyPath,
                         SdfAbstractDataValue *value) const;
%ignore SdfLayer::GetField(const SdfAbstractDataSpecId& id,
                     const TfToken& fieldName) const;
%ignore SdfLayer::GetFieldDictValueByKey(const SdfAbstractDataSpecId& id,
                                   const TfToken& fieldName,
                                   const TfToken& keyPath) const;
%ignore SdfLayer::SetField(const SdfAbstractDataSpecId& id, const TfToken& fieldName,
        const VtValue& value);
%ignore SdfLayer::SetField(const SdfAbstractDataSpecId& id, const TfToken& fieldName,
        const SdfAbstractDataConstValue& value);
%ignore SdfLayer::SetTimeSample(const SdfAbstractDataSpecId& id, double time, 
                       const VtValue & value);
%ignore SdfLayer::SetTimeSample(const SdfAbstractDataSpecId& id, double time, 
                       const SdfAbstractDataConstValue& value);
%ignore SdfLayer::SetFieldDictValueByKey(const SdfAbstractDataSpecId& id,
                                const TfToken& fieldName,
                                const TfToken& keyPath,
                                const VtValue& value);
%ignore SdfLayer::SetFieldDictValueByKey(const SdfAbstractDataSpecId& id,
                                const TfToken& fieldName,
                                const TfToken& keyPath,
                                const SdfAbstractDataConstValue& value);
%ignore SdfLayer::EraseField(const SdfAbstractDataSpecId& id, const TfToken& fieldName);
%ignore SdfLayer::EraseFieldDictValueByKey(const SdfAbstractDataSpecId& id,
                                  const TfToken& fieldName,
                                  const TfToken& keyPath);
%ignore SdfLayer::Traverse(const SdfPath& path, const TraversalFunction& func);


%include "pxr/usd/sdf/layer.h"


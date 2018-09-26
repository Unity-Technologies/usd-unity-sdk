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

%module sdfListOp

%{
#include "pxr/usd/sdf/listOp.h"
#include "pxr/usd/sdf/reference.h"
%}

// boost::optional and boost::function need to be wrapped, though it's not clear that these methods
// need to be called from C++.
%ignore SdfListOp::ApplyOperations;
%ignore SdfListOp::ModifyOperations;

%include "pxr/usd/sdf/listOp.h"

%template (SdfIntListOpVector) std::vector< SdfIntListOp >;
%template (SdfIntListOp) SdfListOp<int>;

%template (SdfUIntListOpVector) std::vector< SdfUIntListOp >;
%template (SdfUIntListOp) SdfListOp<unsigned int>;

%template (SdfInt64ListOpVector) std::vector< SdfInt64ListOp >;
%template (SdfInt64ListOp) SdfListOp<int64_t>;

%template (SdfUInt64ListOpVector) std::vector< SdfUInt64ListOp >;
%template (SdfUInt64ListOp) SdfListOp<uint64_t>;

%template (SdfTfTokenListOpVector) std::vector< SdfTokenListOp >;
%template (SdfTokenListOp) SdfListOp<TfToken>;

%template (SdfStringListOpVector) std::vector< SdfStringListOp >;
%template (SdfStringListOp) SdfListOp<std::string>;

%template (SdfPathListOpVector) std::vector< SdfPathListOp >;
%template (SdfPathListOp) SdfListOp<SdfPath>;

%template (StdReferenceVector) std::vector< SdfReference >;
%template (SdfReferenceListOpVector) std::vector< SdfReferenceListOp >;
%template (SdfReferenceListOp) SdfListOp<SdfReference>;

class SdfUnregisteredValue {};
%template (StdUnregisteredValueVector) std::vector< SdfUnregisteredValue >;
%template (SdfUnregisteredValueListOpVector) std::vector< SdfUnregisteredValueListOp >;
%template (SdfUnregisteredValueListOp) SdfListOp<SdfUnregisteredValue>;


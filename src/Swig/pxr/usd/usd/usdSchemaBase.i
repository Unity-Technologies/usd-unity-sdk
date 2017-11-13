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

%module usdSchemaBase

%{
#include "pxr/usd/usd/schemaBase.h"
%}

WRAP_EQUAL(UsdSchemaBase)

%typemap(cscode) UsdSchemaBase %{

	public static implicit operator bool(UsdSchemaBase b) {
		return b._IsValid();
	}

%}

%ignore UsdSchemaBase::_UnspecifiedBoolType() const;

// --

%include "pxr/usd/usd/schemaBase.h"

// --

%extend UsdSchemaBase {

	bool _IsValid() {
		return bool(*self);
	}

}
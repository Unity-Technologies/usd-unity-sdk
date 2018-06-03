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

%module vtValue

%{
#include "pxr/base/vt/value.h"
%}

// Must be defined before interfaces to which it applies
//%typemap(cscode) VtValue %{
//    public static explicit operator string(VtValue value) {
//        return UsdCs.VtValueTostring(value);
//    }
//  public static explicit operator TfToken(VtValue value) {
//        return UsdCs.VtValueToTfToken(value);
//    }
//%}

%include "stdint.i"

%rename (GetTfType) VtValue::GetType;

%include "vtValue_Casts.i"

%import "vtArray.i"
%include "vtValue_Accessors.i"

WRAP_EQUAL(VtValue)

%include "third_party/include/pxr/base/vt/value.h"

%include "vtValue_Types.i"

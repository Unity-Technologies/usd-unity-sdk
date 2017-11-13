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

%module tf

#define TF_API

%include "tfStringUtils.i"
%include "tfType.i"
%include "tfToken.i"

// Ultra magical Tf Ref and Weak ptr support.
// These were based on the swig smart_ptr implementation.
%include "tfRefPtr.i"
%include "tfWeakPtr.i"

// Required to get TfTokenVector to translate correctly
%ignore TfToStringVector;
%include "pxr/base/tf/token.h"


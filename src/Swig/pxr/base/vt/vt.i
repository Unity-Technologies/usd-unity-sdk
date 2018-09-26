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

%module vt

#define VT_API

// For VtValueTo output parameter.
%apply bool    * INOUT { bool    * output };
%apply float   * INOUT { float   * output };
%apply double  * INOUT { double  * output };
%apply int     * INOUT { int     * output };
%apply long    * INOUT { long    * output };
%apply int64_t * INOUT { int64_t * output };
%apply char    * INOUT { char    * output };
%apply unsigned char    * INOUT { unsigned char    * output };
%apply unsigned int     * INOUT { unsigned int     * output };
%apply uint64_t * INOUT { uint64_t * output };

%include "vtArray.i"
%include "vtValue.i"
%include "vtDictionary.i"


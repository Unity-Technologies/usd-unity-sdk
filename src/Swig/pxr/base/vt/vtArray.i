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

%module vtArray

%{
#include "pxr/base/vt/array.h"
%}

%include <arrays_csharp.i>

 /*  %apply int FIXED[] { int* sourceArray, int *targetArray }
 *   %csmethodmodifiers myArrayCopy "public unsafe";
 *   void myArrayCopy( int *sourceArray, int* targetArray, int nitems );
 */

%include "third_party/include/pxr/base/vt/array.h"


%extend VtArray {
  static bool Equals(VtArray<ELEM> const& lhs, VtArray<ELEM> const& rhs) {
    return lhs == rhs;
  }

  %csmethodmodifiers ToString() "public override";
    std::string ToString() {
      std::stringstream s;
    s << *self;
    return s.str();
  }
  %apply ELEM OUTPUT[] { ELEM* dest }
    void CopyToArray(ELEM* dest) { 
    memcpy(dest, self->data(), self->size() * sizeof(ELEM));
  }
  %apply ELEM INPUT[] { ELEM* src }
  void CopyFromArray(ELEM* src) { 
    memcpy(self->data(), src, self->size() * sizeof(ELEM));
  }

  %typemap(ctype)  void* "void *"
  %typemap(imtype) void* "System.IntPtr"
  %typemap(cstype) void* "System.IntPtr"
  %typemap(csin)   void* "$csinput"
  void CopyToArray(void* dest) { 
    memcpy(dest, self->data(), self->size() * sizeof(ELEM));
  }
  void CopyFromArray(void* src) { 
    memcpy(self->data(), src, self->size() * sizeof(ELEM));
  }

  %csmethodmodifiers GetValue(int index) "protected";
  ELEM const& GetValue(int index) {
    return (*self)[index];
  }
  
  %csmethodmodifiers SetValue(int index, ELEM const& value) "protected";
  void SetValue(int index, ELEM const& value) {
    (*self)[index] = value;
  }
}

%include "vtArray_Types.i"

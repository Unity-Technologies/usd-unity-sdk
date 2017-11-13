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

// This is a helper file for TfRefPtr and should not be included directly.

// The main implementation detail in using this smart pointer of a type is to customise the code generated
// to use a pointer to the smart pointer of the type, rather than the usual pointer to the underlying type.
// So for some type T, TfRefPtr<T> * is used rather than T *.

template <class T> class TfRefPtr {
};

/*
%fragment("SWIG_null_deleter", "header") {
struct SWIG_null_deleter {
  void operator() (void const *) const {
  }
};
%#define SWIG_NO_NULL_DELETER_0 , SWIG_null_deleter()
%#define SWIG_NO_NULL_DELETER_1
%#define SWIG_NO_NULL_DELETER_SWIG_POINTER_NEW
%#define SWIG_NO_NULL_DELETER_SWIG_POINTER_OWN
}
*/

// Workaround empty first macro argument bug
#define xxxEMPTYHACK

// Main user macro for defining TfRefPtr typemaps for both const and non-const pointer types
%define %TfRefPtr(TYPE...)
%feature("smartptr", noblock=1) TYPE { TfRefPtr< TYPE > }
PXR_TF_REF_PTR_TYPEMAPS(xxxEMPTYHACK, TYPE)
PXR_TF_REF_PTR_TYPEMAPS(const, TYPE)
%enddef



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

// Users can provide their own PXR_TF_REF_PTR_TYPEMAPS macro before including this file to change the
// visibility of the constructor and getCPtr method if desired to public if using multiple modules.
#ifndef PXR_TF_REF_PTR_TYPEMAPS
#define PXR_TF_REF_PTR_TYPEMAPS(CONST, TYPE...) PXR_TF_REF_PTR_TYPEMAPS_IMPLEMENTATION(internal, internal, CONST, TYPE)
#endif

%include "tfRefPtrBase.i"

// Language specific macro implementing all the customisations for handling the smart pointer
%define PXR_TF_REF_PTR_TYPEMAPS_IMPLEMENTATION(PTRCTOR_VISIBILITY, CPTR_VISIBILITY, CONST, TYPE...)

// %naturalvar is as documented for member variables
%naturalvar TYPE;
%naturalvar TfRefPtr< CONST TYPE >;

// destructor mods
%feature("unref") TYPE 
//"if (debug_shared) { cout << \"deleting use_count: \" << (*smartarg1).use_count() << \" [\" << (boost::get_deleter<SWIG_null_deleter>(*smartarg1) ? std::string(\"CANNOT BE DETERMINED SAFELY\") : ( (*smartarg1).get() ? (*smartarg1)->getValue() : std::string(\"NULL PTR\") )) << \"]\" << endl << flush; }\n"
                               "(void)arg1; delete smartarg1;"

// Typemap customisations...

// plain value
%typemap(in, canthrow=1) CONST TYPE ($&1_type argp = 0) %{
  argp = ((TfRefPtr< CONST TYPE > *)$input) ? ((TfRefPtr< CONST TYPE > *)$input)->operator->() : 0;
  if (!argp) {
    SWIG_CSharpSetPendingExceptionArgument(SWIG_CSharpArgumentNullException, "Attempt to dereference null $1_type", 0);
    return $null;
  }
  $1 = *argp; %}
%typemap(out) CONST TYPE 
%{ $result = new TfRefPtr<CONST TYPE>((TfCreateRefPtr(new $1_ltype(($1_ltype &)$1))); %}

// plain pointer
%typemap(in, canthrow=1) CONST TYPE * (TfRefPtr< CONST TYPE > *smartarg = 0) %{
  smartarg = (TfRefPtr< CONST TYPE > *)$input;
  $1 = (TYPE *)(smartarg ? smartarg->operator->() : 0); %}

// plain reference
%typemap(in, canthrow=1) CONST TYPE & %{
  $1 = ($1_ltype)(((TfRefPtr< CONST TYPE > *)$input) ? ((TfRefPtr< CONST TYPE > *)$input)->operator->() : 0);
  if (!$1) {
    SWIG_CSharpSetPendingExceptionArgument(SWIG_CSharpArgumentNullException, "$1_type reference is null", 0);
    return $null;
  } %}

// plain pointer by reference
%typemap(in) TYPE *CONST& ($*1_ltype temp = 0)
%{ temp = (TYPE *)(((TfRefPtr< CONST TYPE > *)$input) ? ((TfRefPtr< CONST TYPE > *)$input)->operator->() : 0);
   $1 = &temp; %}

// TfRefPtr by value
%typemap(in) TfRefPtr< CONST TYPE >
%{ if ($input) $1 = *($&1_ltype)$input; %}
%typemap(out) TfRefPtr< CONST TYPE >
%{ $result = $1 ? new TfRefPtr<CONST TYPE>($1) : 0; %}

// TfRefPtr by reference
%typemap(in, canthrow=1) TfRefPtr< CONST TYPE > & ($*1_ltype tempnull)
%{ $1 = $input ? ($1_ltype)$input : &tempnull; %}
%typemap(out) TfRefPtr< CONST TYPE > &
%{ $result = *$1 ? new TfRefPtr<CONST TYPE>(*$1) : 0; %} 

// TfRefPtr by pointer
%typemap(in) TfRefPtr< CONST TYPE > * ($*1_ltype tempnull)
%{ $1 = $input ? ($1_ltype)$input : &tempnull; %}

// TfRefPtr by pointer reference
%typemap(in) TfRefPtr< CONST TYPE > *& (TfRefPtr< CONST TYPE > tempnull, $*1_ltype temp = 0)
%{ temp = $input ? *($1_ltype)&$input : &tempnull;
   $1 = &temp; %}
%typemap(out) TfRefPtr< CONST TYPE > *&
%{ *($1_ltype)&$result = (*$1 && **$1) ? new TfRefPtr<CONST TYPE>(TfCreateRefPtr(**$1)) : 0; %} 

// various missing typemaps - If ever used (unlikely) ensure compilation error rather than runtime bug
%typemap(in) CONST TYPE[], CONST TYPE[ANY], CONST TYPE (CLASS::*) %{
#error "typemaps for $1_type not available"
%}
%typemap(out) CONST TYPE[], CONST TYPE[ANY], CONST TYPE (CLASS::*) %{
#error "typemaps for $1_type not available"
%}


%typemap (ctype)  TfRefPtr< CONST TYPE >, 
                  TfRefPtr< CONST TYPE > &,
                  TfRefPtr< CONST TYPE > *,
                  TfRefPtr< CONST TYPE > *& "void *"
%typemap (imtype, out="global::System.IntPtr") TfRefPtr< CONST TYPE >, 
                                TfRefPtr< CONST TYPE > &,
                                TfRefPtr< CONST TYPE > *,
                                TfRefPtr< CONST TYPE > *& "global::System.Runtime.InteropServices.HandleRef"
%typemap (cstype) TfRefPtr< CONST TYPE >, 
                  TfRefPtr< CONST TYPE > &,
                  TfRefPtr< CONST TYPE > *,
                  TfRefPtr< CONST TYPE > *& "$typemap(cstype, TYPE)"

%typemap(csin) TfRefPtr< CONST TYPE >, 
               TfRefPtr< CONST TYPE > &,
               TfRefPtr< CONST TYPE > *,
               TfRefPtr< CONST TYPE > *& "$typemap(cstype, TYPE).getCPtr($csinput)"

%typemap(csout, excode=SWIGEXCODE) TfRefPtr< CONST TYPE > {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) TfRefPtr< CONST TYPE > & {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) TfRefPtr< CONST TYPE > * {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) TfRefPtr< CONST TYPE > *& {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }


%typemap(csout, excode=SWIGEXCODE) CONST TYPE {
    $typemap(cstype, TYPE) ret = new $typemap(cstype, TYPE)($imcall, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) CONST TYPE & {
    $typemap(cstype, TYPE) ret = new $typemap(cstype, TYPE)($imcall, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) CONST TYPE * {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) TYPE *CONST& {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }

%typemap(csvarout, excode=SWIGEXCODE2) CONST TYPE & %{
    get {
      $csclassname ret = new $csclassname($imcall, true);$excode
      return ret;
    } %}
%typemap(csvarout, excode=SWIGEXCODE2) CONST TYPE * %{
    get {
      global::System.IntPtr cPtr = $imcall;
      $csclassname ret = (cPtr == global::System.IntPtr.Zero) ? null : new $csclassname(cPtr, true);$excode
      return ret;
    } %}

%typemap(csvarout, excode=SWIGEXCODE2) TfRefPtr< CONST TYPE > & %{
    get {
      global::System.IntPtr cPtr = $imcall;
      $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
      return ret;
    } %}
%typemap(csvarout, excode=SWIGEXCODE2) TfRefPtr< CONST TYPE > * %{
    get {
      global::System.IntPtr cPtr = $imcall;
      $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
      return ret;
    } %}


// Proxy classes (base classes, ie, not derived classes)
%typemap(csbody) TYPE %{
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  private bool swigCMemOwnBase;

  PTRCTOR_VISIBILITY $csclassname(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwnBase = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  CPTR_VISIBILITY static global::System.Runtime.InteropServices.HandleRef getCPtr($csclassname obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }
%}

// Derived proxy classes
%typemap(csbody_derived) TYPE %{
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  private bool swigCMemOwnDerived;

  PTRCTOR_VISIBILITY $csclassname(global::System.IntPtr cPtr, bool cMemoryOwn) : base($imclassname.$csclazznameSWIGSmartPtrUpcast(cPtr), true) {
    swigCMemOwnDerived = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  CPTR_VISIBILITY static global::System.Runtime.InteropServices.HandleRef getCPtr($csclassname obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }
%}

%typemap(csdestruct, methodname="Dispose", methodmodifiers="public") TYPE {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwnBase) {
          swigCMemOwnBase = false;
          $imcall;
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

%typemap(csdestruct_derived, methodname="Dispose", methodmodifiers="public") TYPE {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwnDerived) {
          swigCMemOwnDerived = false;
          $imcall;
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

%template() TfRefPtr< CONST TYPE >;
%enddef


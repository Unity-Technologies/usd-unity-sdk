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

// Users can provide their own PXR_TF_WEAK_PTR_TYPEMAPS macro before including this file to change the
// visibility of the constructor and getCPtr method if desired to public if using multiple modules.
#ifndef PXR_TF_WEAK_PTR_TYPEMAPS
#define PXR_TF_WEAK_PTR_TYPEMAPS(CONST, TYPE...) PXR_TF_WEAK_PTR_TYPEMAPS_IMPLEMENTATION(internal, internal, CONST, TYPE)
#endif

%include "tfWeakPtrBase.i"

// Language specific macro implementing all the customisations for handling the smart pointer
%define PXR_TF_WEAK_PTR_TYPEMAPS_IMPLEMENTATION(PTRCTOR_VISIBILITY, CPTR_VISIBILITY, CONST, TYPE...)

// %naturalvar is as documented for member variables
%naturalvar TYPE;
%naturalvar TfWeakPtr< CONST TYPE >;

// destructor mods
%feature("unref") TYPE 
//"if (debug_shared) { cout << \"deleting use_count: \" << (*smartarg1).use_count() << \" [\" << (boost::get_deleter<SWIG_null_deleter>(*smartarg1) ? std::string(\"CANNOT BE DETERMINED SAFELY\") : ( (*smartarg1).get() ? (*smartarg1)->getValue() : std::string(\"NULL PTR\") )) << \"]\" << endl << flush; }\n"
                               "(void)arg1; delete smartarg1;"

// Typemap customisations...

// plain value
%typemap(in, canthrow=1) CONST TYPE ($&1_type argp = 0) %{
  argp = ((TfWeakPtr< CONST TYPE > *)$input) ? ((TfWeakPtr< CONST TYPE > *)$input)->operator->() : 0;
  if (!argp) {
    SWIG_CSharpSetPendingExceptionArgument(SWIG_CSharpArgumentNullException, "Attempt to dereference null $1_type", 0);
    return $null;
  }
  $1 = *argp; %}
%typemap(out) CONST TYPE 
%{ $result = new TfWeakPtr< CONST TYPE >(new $1_ltype(($1_ltype &)$1)); %}

// plain pointer
%typemap(in, canthrow=1) CONST TYPE * (TfWeakPtr< CONST TYPE > *smartarg = 0) %{
  smartarg = (TfWeakPtr< CONST TYPE > *)$input;
  $1 = (TYPE *)(smartarg ? smartarg->operator->() : 0); %}

//%typemap(out, fragment="SWIG_null_deleter") CONST TYPE * %{
//  $result = $1 ? new TfWeakPtr< CONST TYPE >($1 SWIG_NO_NULL_DELETER_$owner) : 0;
//%}

// plain reference
%typemap(in, canthrow=1) CONST TYPE & %{
  $1 = ($1_ltype)(((TfWeakPtr< CONST TYPE > *)$input) ? ((TfWeakPtr< CONST TYPE > *)$input)->operator->() : 0);
  if (!$1) {
    SWIG_CSharpSetPendingExceptionArgument(SWIG_CSharpArgumentNullException, "$1_type reference is null", 0);
    return $null;
  } %}
//%typemap(out, fragment="SWIG_null_deleter") CONST TYPE &
//%{ $result = new TfWeakPtr< CONST TYPE >($1 SWIG_NO_NULL_DELETER_$owner); %}

// plain pointer by reference
%typemap(in) TYPE *CONST& ($*1_ltype temp = 0)
%{ temp = (TYPE *)(((TfWeakPtr< CONST TYPE > *)$input) ? ((TfWeakPtr< CONST TYPE > *)$input)->operator->() : 0);
   $1 = &temp; %}
//%typemap(out, fragment="SWIG_null_deleter") TYPE *CONST&
//%{ $result = new TfWeakPtr< CONST TYPE >(*$1 SWIG_NO_NULL_DELETER_$owner); %}

// TfWeakPtr by value
%typemap(in) TfWeakPtr< CONST TYPE >
%{ if ($input) $1 = *($&1_ltype)$input; %}
%typemap(out) TfWeakPtr< CONST TYPE >
%{ $result = $1 ? new $1_ltype($1) : 0; %}

// TfWeakPtr by reference
%typemap(in, canthrow=1) TfWeakPtr< CONST TYPE > & ($*1_ltype tempnull)
%{ $1 = $input ? ($1_ltype)$input : &tempnull; %}
%typemap(out) TfWeakPtr< CONST TYPE > &
%{ $result = *$1 ? new $*1_ltype(*$1) : 0; %} 

// TfWeakPtr by pointer
%typemap(in) TfWeakPtr< CONST TYPE > * ($*1_ltype tempnull)
%{ $1 = $input ? ($1_ltype)$input : &tempnull; %}
//%typemap(out, fragment="SWIG_null_deleter") TfWeakPtr< CONST TYPE > *
//%{ $result = ($1 && *$1) ? new $*1_ltype(*($1_ltype)$1) : 0;
//   if ($owner) delete $1; %}

// TfWeakPtr by pointer reference
%typemap(in) TfWeakPtr< CONST TYPE > *& (TfWeakPtr< CONST TYPE > tempnull, $*1_ltype temp = 0)
%{ temp = $input ? *($1_ltype)&$input : &tempnull;
   $1 = &temp; %}
%typemap(out) TfWeakPtr< CONST TYPE > *&
%{ *($1_ltype)&$result = (*$1 && **$1) ? new TfWeakPtr< CONST TYPE >(**$1) : 0; %} 

// various missing typemaps - If ever used (unlikely) ensure compilation error rather than runtime bug
%typemap(in) CONST TYPE[], CONST TYPE[ANY], CONST TYPE (CLASS::*) %{
#error "typemaps for $1_type not available"
%}
%typemap(out) CONST TYPE[], CONST TYPE[ANY], CONST TYPE (CLASS::*) %{
#error "typemaps for $1_type not available"
%}


%typemap (ctype)  TfWeakPtr< CONST TYPE >, 
                  TfWeakPtr< CONST TYPE > &,
                  TfWeakPtr< CONST TYPE > *,
                  TfWeakPtr< CONST TYPE > *& "void *"
%typemap (imtype, out="global::System.IntPtr") TfWeakPtr< CONST TYPE >, 
                                TfWeakPtr< CONST TYPE > &,
                                TfWeakPtr< CONST TYPE > *,
                                TfWeakPtr< CONST TYPE > *& "global::System.Runtime.InteropServices.HandleRef"
%typemap (cstype) TfWeakPtr< CONST TYPE >, 
                  TfWeakPtr< CONST TYPE > &,
                  TfWeakPtr< CONST TYPE > *,
                  TfWeakPtr< CONST TYPE > *& "$typemap(cstype, TYPE)"

%typemap(csin) TfWeakPtr< CONST TYPE >, 
               TfWeakPtr< CONST TYPE > &,
               TfWeakPtr< CONST TYPE > *,
               TfWeakPtr< CONST TYPE > *& "$typemap(cstype, TYPE).getCPtr($csinput)"

%typemap(csout, excode=SWIGEXCODE) TfWeakPtr< CONST TYPE > {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) TfWeakPtr< CONST TYPE > & {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) TfWeakPtr< CONST TYPE > * {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(csout, excode=SWIGEXCODE) TfWeakPtr< CONST TYPE > *& {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }

/*
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
*/

%typemap(csvarout, excode=SWIGEXCODE2) TfWeakPtr< CONST TYPE > & %{
    get {
      global::System.IntPtr cPtr = $imcall;
      $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
      return ret;
    } %}
%typemap(csvarout, excode=SWIGEXCODE2) TfWeakPtr< CONST TYPE > * %{
    get {
      global::System.IntPtr cPtr = $imcall;
      $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero) ? null : new $typemap(cstype, TYPE)(cPtr, true);$excode
      return ret;
    } %}

/*
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
*/
/*
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
*/

%template() TfWeakPtr< CONST TYPE >;
%enddef


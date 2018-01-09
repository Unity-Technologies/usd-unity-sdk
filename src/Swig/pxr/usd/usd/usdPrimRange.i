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

%module usdPrimRange

%{
#include "pxr/usd/usd/primRange.h"
%}

%typemap(csinterfaces) UsdPrimRange %{
    global::System.Collections.IEnumerable,
    global::System.Collections.Generic.IEnumerable<UsdPrim>
%}

%typemap(cscode) UsdPrimRange %{
  // Returning an externally defined class is ugly, but dramatically simplifies the bindings while
  // allowing the use of C#'s foreach mechanism.
  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
    return new USD.NET.RangeIterator(this);
  }

  public System.Collections.Generic.IEnumerator<UsdPrim> GetEnumerator() {
    return new USD.NET.RangeIterator(this);
  }
%}

%extend UsdPrimRange::iterator {
  void MoveNext() {
    ++(*self);
  }
  UsdPrim GetCurrent() {
    return **self;
  }
}

%rename UsdPrimRange::front GetCurrent;
%rename UsdPrimRange::begin GetStart;
%rename UsdPrimRange::end GetEnd;
%rename UsdPrimRange::increment_begin IncrementBegin;
%rename UsdPrimRange::set_begin SetBegin;
%rename UsdPrimRange::empty IsEmpty;

%ignore UsdPrimRange::cbegin;
%ignore UsdPrimRange::cend;

WRAP_EQUAL(UsdPrimRange)
WRAP_EQUAL(UsdPrimRange::iterator)

%include "pxr/usd/usd/primRange.h"

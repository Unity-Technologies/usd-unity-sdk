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

%module usdPrim

%{
#include "pxr/usd/usd/prim.h"
#include "pxr/usd/usd/variantSets.h"
#include "pxr/usd/usd/inherits.h"
#include "pxr/usd/usd/references.h"
#include "pxr/usd/usd/specializes.h"
#include "pxr/usd/usd/property.h"
#include "pxr/usd/usd/attribute.h"
#include "pxr/usd/usd/relationship.h"
%}

%inline {
  void GetPrimFromVector(std::vector<UsdPrim> const& prims, int index, UsdPrim* output) {
    *output = prims[index];
  }
}

namespace std {
  %template(UsdPrimVector) vector<UsdPrim>;
}

typedef std::vector<UsdPrim> UsdPrimVector;

%extend UsdPrim {
  bool GetAttributeValue(TfToken attrName, VtValue* valueOut, UsdTimeCode time) {
    UsdAttribute attr = self->GetAttribute(attrName);
    if (!attr) { return false; }
    return attr.Get(valueOut, time);
  }
}

%ignore UsdPrim::FindAllRelationshipTargetPaths;
%ignore UsdPrim::FindAllAttributeConnectionPaths;

// ---------------------------------------------------------------------------------------------- //
// UsdPrimSiblingIterator
// ---------------------------------------------------------------------------------------------- //
%extend UsdPrimSiblingIterator {
  void MoveNext() {
    ++(*self);
  }
  UsdPrim GetCurrent() {
    return **self;
  }
}

WRAP_EQUAL(UsdPrimSiblingIterator)

class UsdPrimSiblingIterator {
public:
  typedef class UsdPrim value_type;
  typedef value_type reference;
};

// ---------------------------------------------------------------------------------------------- //
// UsdPrimSubtreeIterator
// ---------------------------------------------------------------------------------------------- //
%extend UsdPrimSubtreeIterator {
  void MoveNext() {
    ++(*self);
  }
  UsdPrim GetCurrent() {
    return **self;
  }
}

WRAP_EQUAL(UsdPrimSubtreeIterator)

class UsdPrimSubtreeIterator {
public:
  typedef class UsdPrim value_type;
  typedef value_type reference;
};

// ---------------------------------------------------------------------------------------------- //
// UsdPrimSiblingRange
// ---------------------------------------------------------------------------------------------- //
%extend boost::iterator_range<UsdPrimSiblingIterator> {
  UsdPrimSiblingIterator GetStart() {
    return self->begin();
  }
  UsdPrimSiblingIterator GetEnd() {
    return self->end();
  }
}

%typemap(csinterfaces) boost::iterator_range<UsdPrimSiblingIterator> %{
    global::System.Collections.IEnumerable,
    global::System.Collections.Generic.IEnumerable<UsdPrim>
%}

%typemap(cscode) boost::iterator_range<UsdPrimSiblingIterator> %{
  // Returning an externally defined class is ugly, but dramatically simplifies the bindings while
  // allowing the use of C#'s foreach mechanism.
  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
    return new USD.NET.SiblingIterator(this);
  }

  public System.Collections.Generic.IEnumerator<UsdPrim> GetEnumerator() {
    return new USD.NET.SiblingIterator(this);
  }
%}

// ---------------------------------------------------------------------------------------------- //
// UsdPrimSubtreeRange
// ---------------------------------------------------------------------------------------------- //
%extend boost::iterator_range<UsdPrimSubtreeIterator> {
  UsdPrimSubtreeIterator GetStart() {
    return self->begin();
  }
  UsdPrimSubtreeIterator GetEnd() {
    return self->end();
  }
}

%typemap(csinterfaces) boost::iterator_range<UsdPrimSubtreeIterator> %{
    global::System.Collections.IEnumerable,
    global::System.Collections.Generic.IEnumerable<UsdPrim>
%}

%typemap(cscode) boost::iterator_range<UsdPrimSubtreeIterator> %{
  // Returning an externally defined class is ugly, but dramatically simplifies the bindings while
  // allowing the use of C#'s foreach mechanism.
  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
    return new USD.NET.SubtreeIterator(this);
  }

  public System.Collections.Generic.IEnumerator<UsdPrim> GetEnumerator() {
    return new USD.NET.SubtreeIterator(this);
  }
%}


namespace boost {
  template<class T>
  class iterator_range{
  public:
    T begin() const;
    T end() const;
  };

  %template(UsdPrimSiblingRange) iterator_range<UsdPrimSiblingIterator>;
  %template(UsdPrimSubtreeRange) iterator_range<UsdPrimSubtreeIterator>;
}

%include "third_party/include/pxr/usd/usd/prim.h"


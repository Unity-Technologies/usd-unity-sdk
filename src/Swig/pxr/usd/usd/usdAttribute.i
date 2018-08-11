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

%module usdAttribute

%{
#include "pxr/usd/usd/attribute.h"
%}

%extend UsdAttribute {
  std::vector<double> GetTimeSamples() const {
  std::vector<double> v;
  self->GetTimeSamples(&v);
  return v;
  }
  std::vector<double> GetTimeSamplesInInterval(const GfInterval& interval) const {
  std::vector<double> v;
  self->GetTimeSamplesInInterval(interval, &v);
  return v;
  }
  VtValue Get() const {
  VtValue v;
  self->Get(&v);
  return v;
  }
  VtValue Get(UsdTimeCode time) const {
  VtValue v;
  self->Get(&v, time);
  return v;
  }
}

// For GetBracketingTimeSamples
%apply double *OUTPUT { double *upper };
%apply double *OUTPUT { double *lower };
%apply double *OUTPUT { bool *hasTimeSamples };

%include "std_vector.i"
namespace std {
  %template(UsdAttributeVector) vector<UsdAttribute>;
}
typedef std::vector<UsdAttribute> UsdAttributeVector;

%ignore GetTimeSamplesInInterval(const GfInterval& interval,
                                  std::vector<double>* times) const;
%ignore UsdAttribute::GetTimeSamples(std::vector<double>* times) const;
%ignore UsdAttribute::Get(VtValue* value) const;

%include "pxr/usd/usd/attribute.h"
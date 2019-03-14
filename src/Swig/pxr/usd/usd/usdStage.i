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

%module usdStage

%{
#include "pxr/usd/usd/stage.h"
#include "pxr/usd/usd/prim.h"
#include "pxr/usd/usd/primRange.h"
#include <string>
%}

namespace std {

%naturalvar string;
class string;

// string *
%typemap(ctype) std::string * "char**"  // how it gets passed in C
%typemap(imtype) std::string * "/*imtype*/ out string" // P/Invoike call intermediate type
%typemap(cstype) std::string * "/*cstype*/ out string" // how it gets passed into the binding

//C++
%typemap(in, canthrow=1) std::string *
%{  //typemap in
    std::string temp;
    $1 = &temp; 
 %}

//C++
%typemap(argout) std::string *
%{ 
    //Typemap argout in c++ file.
    //This will convert c++ string to c# string
    *$input = SWIG_csharp_string_callback($1->c_str());
%}

%typemap(argout) const std::string *
%{ 
    //argout typemap for const std::string*
%}

%typemap(csin) std::string * "out $csinput"

%typemap(throws, canthrow=1) string *
%{ SWIG_CSharpSetPendingException(SWIG_CSharpApplicationException, $1.c_str());
   return $null; %}
}

%ignore UsdStage::Traverse() const;
%ignore UsdStage::ExpandPopulationMask;

%include "pxr/usd/usd/stage.h"

%extend UsdStage {
  std::vector<UsdPrim> GetAllPrims() {
    std::vector<UsdPrim> targets;
    for (auto&& p : self->Traverse()) { targets.push_back(p); }
    return targets;
  }
  std::vector<UsdPrim> GetAllPrimsByType(std::string typeName) {
    std::vector<UsdPrim> targets;
    for (auto&& p : self->Traverse()) {
      if (p.GetTypeName() == typeName) {
        targets.push_back(p);
      }
    }
    return targets;
  }

  std::vector<SdfPath> GetAllPaths() {
    std::vector<SdfPath> targets;
    for (auto&& p : self->Traverse()) {
      targets.push_back(p.GetPath());
    }
    return targets;
  }

  std::vector<SdfPath> GetAllPathsByType(std::string typeName, SdfPath rootPath) {
    std::vector<SdfPath> targets;

    // Required so type aliases work (e.g. "Mesh" vs "UsdGeomMesh");
    TfType schemaBaseType = TfType::Find<UsdSchemaBase>();

    TfType baseType = schemaBaseType.FindDerivedByName(typeName);
    
    if (schemaBaseType == TfType::GetUnknownType()) {
      TF_RUNTIME_ERROR("Schema base type is unknown. This should never happen.");
      return targets;
    }

    if (baseType == TfType::GetUnknownType()) {
      TF_CODING_ERROR("Base type '%s' was not known to the TfType system", typeName.c_str());
      return targets;
    }

    UsdPrim rootPrim = self->GetPrimAtPath(rootPath);
    if (!rootPrim.IsValid()) {
      TF_CODING_ERROR("Invalid root path <%s>", rootPath.GetText());
      return targets;
    }

    {
      TfType curType = schemaBaseType.FindDerivedByName(rootPrim.GetTypeName().GetString());
      if (curType != TfType::GetUnknownType() && curType.IsA(baseType)) {
        targets.push_back(rootPrim.GetPath());
      }
    }

    for (auto&& p : rootPrim.GetAllDescendants()) {
      TfType curType = schemaBaseType.FindDerivedByName(p.GetTypeName().GetString());
      if (curType == TfType::GetUnknownType()) {
        continue;
      }
      if (curType.IsA(baseType)) {
        targets.push_back(p.GetPath());
      }
    }

    return targets;
  }
}

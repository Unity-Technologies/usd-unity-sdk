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

%module sdf

#define SDF_API

// TODO: switch to using the header version.
//%include "pxr/usd/sdf/declareHandles.h"
#define SDF_DECLARE_HANDLES(x)

%include "sdfTypes.i"
%include "sdfValueTypeName.i"
%include "sdfValueTypeNames.i"
%include "sdfAssetPath.i"
%include "sdfPath.i"

%include "sdfLayerOffset.i"
%include "sdfPayload.i"
%include "sdfNamespaceEdit.i"

//%include "sdfAttributeSpecHandle.i"
//%include "sdfFileFormatConstPtr.i"

%include "sdfLayerBase.i"
%include "sdfLayer.i"


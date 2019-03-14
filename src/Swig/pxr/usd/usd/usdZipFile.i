// Copyright 2018 Jeremy Cowles. All rights reserved.
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

%module usdZipFile

%{
#include "pxr/usd/usd/zipFile.h"
%}

%ignore UsdZipFile::Iterator;
%ignore UsdZipFile::Iterator::_ArrowProxy;

// Swig incorrectly attempts to copy a non-copyable object.
%ignore UsdZipFileWriter::CreateNew;
%ignore UsdZipFileWriter;

%include "pxr/usd/usd/zipFile.h"

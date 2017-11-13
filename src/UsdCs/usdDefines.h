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

#pragma once

#include <string>

#include "pxr/pxr.h"
PXR_NAMESPACE_USING_DIRECTIVE

#include "pxr/base/vt/types.h"

#include "pxr/base/vt/array.h"
#include "pxr/base/tf/token.h"
#include "pxr/base/gf/frustum.h"
#include "pxr/base/gf/half.h"
#include "pxr/base/gf/interval.h"
#include "pxr/base/gf/multiInterval.h"
#include "pxr/base/gf/matrix2f.h"
#include "pxr/base/gf/matrix3f.h"
#include "pxr/base/gf/matrix4f.h"
#include "pxr/base/gf/matrix2d.h"
#include "pxr/base/gf/matrix3d.h"
#include "pxr/base/gf/matrix4d.h"
#include "pxr/base/gf/quatd.h"
#include "pxr/base/gf/quatf.h"
#include "pxr/base/gf/quath.h"
#include "pxr/base/gf/quaternion.h"
#include "pxr/base/gf/range1d.h"
#include "pxr/base/gf/range1f.h"
#include "pxr/base/gf/range2d.h"
#include "pxr/base/gf/range2f.h"
#include "pxr/base/gf/range3d.h"
#include "pxr/base/gf/range3f.h"
#include "pxr/base/gf/rect2i.h"
#include "pxr/base/gf/vec2d.h"
#include "pxr/base/gf/vec2f.h"
#include "pxr/base/gf/vec2h.h"
#include "pxr/base/gf/vec2i.h"
#include "pxr/base/gf/vec3d.h"
#include "pxr/base/gf/vec3f.h"
#include "pxr/base/gf/vec3h.h"
#include "pxr/base/gf/vec3i.h"
#include "pxr/base/gf/vec4d.h"
#include "pxr/base/gf/vec4f.h"
#include "pxr/base/gf/vec4h.h"
#include "pxr/base/gf/vec4i.h"
#include "pxr/usd/sdf/assetPath.h"

typedef VtArray<SdfAssetPath> SdfAssetPathArray;
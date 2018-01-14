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

%module gf

#define GF_API

// Output parameters used in many Gf classes.
%apply double * INOUT { double * det };
%apply double * INOUT { double * thetaTw };
%apply double * INOUT { double * thetaFB };
%apply double * INOUT { double * thetaLR };
%apply double * INOUT { double * thetaSw };
%apply double * INOUT { double * swShift };

%include "gfBBox3d.i"
%include "gfInterval.i"

%include "gfHalf.i"

%include "gfRange1f.i"
%include "gfRange1d.i"

%include "gfRange2d.i"
%include "gfRange3d.i"

%include "gfMatrix2f.i"
%include "gfMatrix3f.i"
%include "gfMatrix4f.i"

%include "gfMatrix2d.i"
%include "gfMatrix3d.i"
%include "gfMatrix4d.i"

%include "gfVec2i.i"
%include "gfVec2h.i"
%include "gfVec2f.i"
%include "gfVec2d.i"

%include "gfVec3i.i"
%include "gfVec3h.i"
%include "gfVec3f.i"
%include "gfVec3d.i"

%include "gfVec4i.i"
%include "gfVec4h.i"
%include "gfVec4f.i"
%include "gfVec4d.i"

%include "gfRotation.i"
%include "gfQuaternion.i"
%include "gfQuath.i"
%include "gfQuatf.i"
%include "gfQuatd.i"

%apply double *OUTPUT { double *fieldOfView }
%apply double *OUTPUT { double *fieldOfViewHeight }
%apply double *OUTPUT { double *aspectRatio }
%apply double *OUTPUT { double *nearDistance }
%apply double *OUTPUT { double *farDistance }
%apply double *OUTPUT { double *left }
%apply double *OUTPUT { double *right }
%apply double *OUTPUT { double *bottom }
%apply double *OUTPUT { double *top }
%apply double *OUTPUT { double *nearPlane }
%apply double *OUTPUT { double *farPlane }
%apply double *OUTPUT { double *t }
%apply double *OUTPUT { double *rayDistance }
%apply double *OUTPUT { double *distance }
%apply double *OUTPUT { double *enterDistance }
%apply double *OUTPUT { double *exitDistance }

%include "gfRay.i"
%include "gfLine.i"
%include "gfLineSeg.i"
%include "gfPlane.i"
%include "gfFrustum.i"
%include "gfCamera.i"


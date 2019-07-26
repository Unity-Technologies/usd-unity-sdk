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

%typemap(cscode) UsdSkelTokens %{
  public static TfToken bindTransforms = new TfToken("bindTransforms");
  public static TfToken blendShapeWeights = new TfToken("blendShapeWeights");
  public static TfToken blendShapes = new TfToken("blendShapes");
  public static TfToken jointNames = new TfToken("jointNames");
  public static TfToken joints = new TfToken("joints");
  public static TfToken normalOffsets = new TfToken("normalOffsets");
  public static TfToken offsets = new TfToken("offsets");
  public static TfToken pointIndices = new TfToken("pointIndices");
  public static TfToken primvarsSkelGeomBindTransform = new TfToken("primvars:skel:geomBindTransform");
  public static TfToken primvarsSkelJointIndices = new TfToken("primvars:skel:jointIndices");
  public static TfToken primvarsSkelJointWeights = new TfToken("primvars:skel:jointWeights");
  public static TfToken restTransforms = new TfToken("restTransforms");
  public static TfToken rotations = new TfToken("rotations");
  public static TfToken scales = new TfToken("scales");
  public static TfToken skelAnimationSource = new TfToken("skel:animationSource");
  public static TfToken skelBlendShapeTargets = new TfToken("skel:blendShapeTargets");
  public static TfToken skelBlendShapes = new TfToken("skel:blendShapes");
  public static TfToken skelJoints = new TfToken("skel:joints");
  public static TfToken skelSkeleton = new TfToken("skel:skeleton");
  public static TfToken translations = new TfToken("translations");
  public static TfToken weight = new TfToken("weight");
%}

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

%typemap(cscode) UsdLuxTokens %{
  public static TfToken angular = new TfToken("angular");
  public static TfToken automatic = new TfToken("automatic");
  public static TfToken collectionFilterLinkIncludeRoot = new TfToken("collection:filterLink:includeRoot");
  public static TfToken collectionLightLinkIncludeRoot = new TfToken("collection:lightLink:includeRoot");
  public static TfToken collectionShadowLinkIncludeRoot = new TfToken("collection:shadowLink:includeRoot");
  public static TfToken consumeAndContinue = new TfToken("consumeAndContinue");
  public static TfToken consumeAndHalt = new TfToken("consumeAndHalt");
  public static TfToken cubeMapVerticalCross = new TfToken("cubeMapVerticalCross");
  public static TfToken cylinderLight = new TfToken("CylinderLight");
  public static TfToken diskLight = new TfToken("DiskLight");
  public static TfToken distantLight = new TfToken("DistantLight");
  public static TfToken domeLight = new TfToken("DomeLight");
  public static TfToken extent = new TfToken("extent");
  public static TfToken filterLink = new TfToken("filterLink");
  public static TfToken geometry = new TfToken("geometry");
  public static TfToken geometryLight = new TfToken("GeometryLight");
  public static TfToken guideRadius = new TfToken("guideRadius");
  public static TfToken ignore = new TfToken("ignore");
  public static TfToken independent = new TfToken("independent");
  public static TfToken inputsAngle = new TfToken("inputs:angle");
  public static TfToken inputsColor = new TfToken("inputs:color");
  public static TfToken inputsColorTemperature = new TfToken("inputs:colorTemperature");
  public static TfToken inputsDiffuse = new TfToken("inputs:diffuse");
  public static TfToken inputsEnableColorTemperature = new TfToken("inputs:enableColorTemperature");
  public static TfToken inputsExposure = new TfToken("inputs:exposure");
  public static TfToken inputsHeight = new TfToken("inputs:height");
  public static TfToken inputsIntensity = new TfToken("inputs:intensity");
  public static TfToken inputsLength = new TfToken("inputs:length");
  public static TfToken inputsNormalize = new TfToken("inputs:normalize");
  public static TfToken inputsRadius = new TfToken("inputs:radius");
  public static TfToken inputsShadowColor = new TfToken("inputs:shadow:color");
  public static TfToken inputsShadowDistance = new TfToken("inputs:shadow:distance");
  public static TfToken inputsShadowEnable = new TfToken("inputs:shadow:enable");
  public static TfToken inputsShadowFalloff = new TfToken("inputs:shadow:falloff");
  public static TfToken inputsShadowFalloffGamma = new TfToken("inputs:shadow:falloffGamma");
  public static TfToken inputsShapingConeAngle = new TfToken("inputs:shaping:cone:angle");
  public static TfToken inputsShapingConeSoftness = new TfToken("inputs:shaping:cone:softness");
  public static TfToken inputsShapingFocus = new TfToken("inputs:shaping:focus");
  public static TfToken inputsShapingFocusTint = new TfToken("inputs:shaping:focusTint");
  public static TfToken inputsShapingIesAngleScale = new TfToken("inputs:shaping:ies:angleScale");
  public static TfToken inputsShapingIesFile = new TfToken("inputs:shaping:ies:file");
  public static TfToken inputsShapingIesNormalize = new TfToken("inputs:shaping:ies:normalize");
  public static TfToken inputsSpecular = new TfToken("inputs:specular");
  public static TfToken inputsTextureFile = new TfToken("inputs:texture:file");
  public static TfToken inputsTextureFormat = new TfToken("inputs:texture:format");
  public static TfToken inputsWidth = new TfToken("inputs:width");
  public static TfToken latlong = new TfToken("latlong");
  public static TfToken lightFilterShaderId = new TfToken("lightFilter:shaderId");
  public static TfToken lightFilters = new TfToken("light:filters");
  public static TfToken lightLink = new TfToken("lightLink");
  public static TfToken lightList = new TfToken("lightList");
  public static TfToken lightListCacheBehavior = new TfToken("lightList:cacheBehavior");
  public static TfToken lightMaterialSyncMode = new TfToken("light:materialSyncMode");
  public static TfToken lightShaderId = new TfToken("light:shaderId");
  public static TfToken materialGlowTintsLight = new TfToken("materialGlowTintsLight");
  public static TfToken meshLight = new TfToken("MeshLight");
  public static TfToken mirroredBall = new TfToken("mirroredBall");
  public static TfToken noMaterialResponse = new TfToken("noMaterialResponse");
  public static TfToken orientToStageUpAxis = new TfToken("orientToStageUpAxis");
  public static TfToken portalLight = new TfToken("PortalLight");
  public static TfToken portals = new TfToken("portals");
  public static TfToken rectLight = new TfToken("RectLight");
  public static TfToken shadowLink = new TfToken("shadowLink");
  public static TfToken sphereLight = new TfToken("SphereLight");
  public static TfToken treatAsLine = new TfToken("treatAsLine");
  public static TfToken treatAsPoint = new TfToken("treatAsPoint");
  public static TfToken volumeLight = new TfToken("VolumeLight");
%}

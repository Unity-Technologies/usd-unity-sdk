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
  public static TfToken angle = new TfToken("angle");
  public static TfToken angular = new TfToken("angular");
  public static TfToken automatic = new TfToken("automatic");
  public static TfToken collectionFilterLinkIncludeRoot = new TfToken("collection:filterLink:includeRoot");
  public static TfToken collectionLightLinkIncludeRoot = new TfToken("collection:lightLink:includeRoot");
  public static TfToken collectionShadowLinkIncludeRoot = new TfToken("collection:shadowLink:includeRoot");
  public static TfToken color = new TfToken("color");
  public static TfToken colorTemperature = new TfToken("colorTemperature");
  public static TfToken consumeAndContinue = new TfToken("consumeAndContinue");
  public static TfToken consumeAndHalt = new TfToken("consumeAndHalt");
  public static TfToken cubeMapVerticalCross = new TfToken("cubeMapVerticalCross");
  public static TfToken diffuse = new TfToken("diffuse");
  public static TfToken enableColorTemperature = new TfToken("enableColorTemperature");
  public static TfToken exposure = new TfToken("exposure");
  public static TfToken filterLink = new TfToken("filterLink");
  public static TfToken filters = new TfToken("filters");
  public static TfToken geometry = new TfToken("geometry");
  public static TfToken height = new TfToken("height");
  public static TfToken ignore = new TfToken("ignore");
  public static TfToken intensity = new TfToken("intensity");
  public static TfToken latlong = new TfToken("latlong");
  public static TfToken length = new TfToken("length");
  public static TfToken lightLink = new TfToken("lightLink");
  public static TfToken lightList = new TfToken("lightList");
  public static TfToken lightListCacheBehavior = new TfToken("lightList:cacheBehavior");
  public static TfToken mirroredBall = new TfToken("mirroredBall");
  public static TfToken normalize = new TfToken("normalize");
  public static TfToken portals = new TfToken("portals");
  public static TfToken radius = new TfToken("radius");
  public static TfToken shadowColor = new TfToken("shadow:color");
  public static TfToken shadowDistance = new TfToken("shadow:distance");
  public static TfToken shadowEnable = new TfToken("shadow:enable");
  public static TfToken shadowExclude = new TfToken("shadow:exclude");
  public static TfToken shadowFalloff = new TfToken("shadow:falloff");
  public static TfToken shadowFalloffGamma = new TfToken("shadow:falloffGamma");
  public static TfToken shadowInclude = new TfToken("shadow:include");
  public static TfToken shadowLink = new TfToken("shadowLink");
  public static TfToken shapingConeAngle = new TfToken("shaping:cone:angle");
  public static TfToken shapingConeSoftness = new TfToken("shaping:cone:softness");
  public static TfToken shapingFocus = new TfToken("shaping:focus");
  public static TfToken shapingFocusTint = new TfToken("shaping:focusTint");
  public static TfToken shapingIesAngleScale = new TfToken("shaping:ies:angleScale");
  public static TfToken shapingIesFile = new TfToken("shaping:ies:file");
  public static TfToken specular = new TfToken("specular");
  public static TfToken textureFile = new TfToken("texture:file");
  public static TfToken textureFormat = new TfToken("texture:format");
  public static TfToken treatAsLine = new TfToken("treatAsLine");
  public static TfToken treatAsPoint = new TfToken("treatAsPoint");
  public static TfToken width = new TfToken("width");
%}

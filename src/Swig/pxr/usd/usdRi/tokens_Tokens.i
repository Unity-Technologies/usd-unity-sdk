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

%typemap(cscode) UsdRiTokens %{
  public static TfToken analytic = new TfToken("analytic");
  public static TfToken analyticApex = new TfToken("analytic:apex");
  public static TfToken analyticBlurAmount = new TfToken("analytic:blur:amount");
  public static TfToken analyticBlurExponent = new TfToken("analytic:blur:exponent");
  public static TfToken analyticBlurFarDistance = new TfToken("analytic:blur:farDistance");
  public static TfToken analyticBlurFarValue = new TfToken("analytic:blur:farValue");
  public static TfToken analyticBlurMidValue = new TfToken("analytic:blur:midValue");
  public static TfToken analyticBlurMidpoint = new TfToken("analytic:blur:midpoint");
  public static TfToken analyticBlurNearDistance = new TfToken("analytic:blur:nearDistance");
  public static TfToken analyticBlurNearValue = new TfToken("analytic:blur:nearValue");
  public static TfToken analyticBlurSMult = new TfToken("analytic:blur:sMult");
  public static TfToken analyticBlurTMult = new TfToken("analytic:blur:tMult");
  public static TfToken analyticDensityExponent = new TfToken("analytic:density:exponent");
  public static TfToken analyticDensityFarDistance = new TfToken("analytic:density:farDistance");
  public static TfToken analyticDensityFarValue = new TfToken("analytic:density:farValue");
  public static TfToken analyticDensityMidValue = new TfToken("analytic:density:midValue");
  public static TfToken analyticDensityMidpoint = new TfToken("analytic:density:midpoint");
  public static TfToken analyticDensityNearDistance = new TfToken("analytic:density:nearDistance");
  public static TfToken analyticDensityNearValue = new TfToken("analytic:density:nearValue");
  public static TfToken analyticDirectional = new TfToken("analytic:directional");
  public static TfToken analyticShearX = new TfToken("analytic:shearX");
  public static TfToken analyticShearY = new TfToken("analytic:shearY");
  public static TfToken analyticUseLightDirection = new TfToken("analytic:useLightDirection");
  public static TfToken aovName = new TfToken("aovName");
  public static TfToken argsPath = new TfToken("argsPath");
  public static TfToken barnMode = new TfToken("barnMode");
  public static TfToken bspline = new TfToken("bspline");
  public static TfToken catmullRom = new TfToken("catmullRom");
  public static TfToken clamp = new TfToken("clamp");
  public static TfToken colorContrast = new TfToken("color:contrast");
  public static TfToken colorMidpoint = new TfToken("color:midpoint");
  public static TfToken colorSaturation = new TfToken("color:saturation");
  public static TfToken colorTint = new TfToken("color:tint");
  public static TfToken colorWhitepoint = new TfToken("color:whitepoint");
  public static TfToken cone = new TfToken("cone");
  public static TfToken constant = new TfToken("constant");
  public static TfToken cookieMode = new TfToken("cookieMode");
  public static TfToken day = new TfToken("day");
  public static TfToken depth = new TfToken("depth");
  public static TfToken distanceToLight = new TfToken("distanceToLight");
  public static TfToken edgeBack = new TfToken("edge:back");
  public static TfToken edgeBottom = new TfToken("edge:bottom");
  public static TfToken edgeFront = new TfToken("edge:front");
  public static TfToken edgeLeft = new TfToken("edge:left");
  public static TfToken edgeRight = new TfToken("edge:right");
  public static TfToken edgeThickness = new TfToken("edgeThickness");
  public static TfToken edgeTop = new TfToken("edge:top");
  public static TfToken falloffRampBeginDistance = new TfToken("falloffRamp:beginDistance");
  public static TfToken falloffRampEndDistance = new TfToken("falloffRamp:endDistance");
  public static TfToken filePath = new TfToken("filePath");
  public static TfToken haziness = new TfToken("haziness");
  public static TfToken height = new TfToken("height");
  public static TfToken hour = new TfToken("hour");
  public static TfToken inPrimaryHit = new TfToken("inPrimaryHit");
  public static TfToken inReflection = new TfToken("inReflection");
  public static TfToken inRefraction = new TfToken("inRefraction");
  public static TfToken infoArgsPath = new TfToken("info:argsPath");
  public static TfToken infoFilePath = new TfToken("info:filePath");
  public static TfToken infoOslPath = new TfToken("info:oslPath");
  public static TfToken infoSloPath = new TfToken("info:sloPath");
  public static TfToken interpolation = new TfToken("interpolation");
  public static TfToken invert = new TfToken("invert");
  public static TfToken latitude = new TfToken("latitude");
  public static TfToken linear = new TfToken("linear");
  public static TfToken longitude = new TfToken("longitude");
  public static TfToken max = new TfToken("max");
  public static TfToken min = new TfToken("min");
  public static TfToken month = new TfToken("month");
  public static TfToken multiply = new TfToken("multiply");
  public static TfToken noEffect = new TfToken("noEffect");
  public static TfToken noLight = new TfToken("noLight");
  public static TfToken off = new TfToken("off");
  public static TfToken onVolumeBoundaries = new TfToken("onVolumeBoundaries");
  public static TfToken outputsRiDisplacement = new TfToken("outputs:ri:displacement");
  public static TfToken outputsRiSurface = new TfToken("outputs:ri:surface");
  public static TfToken outputsRiVolume = new TfToken("outputs:ri:volume");
  public static TfToken physical = new TfToken("physical");
  public static TfToken positions = new TfToken("positions");
  public static TfToken preBarnEffect = new TfToken("preBarnEffect");
  public static TfToken radial = new TfToken("radial");
  public static TfToken radius = new TfToken("radius");
  public static TfToken rampMode = new TfToken("rampMode");
  public static TfToken refineBack = new TfToken("refine:back");
  public static TfToken refineBottom = new TfToken("refine:bottom");
  public static TfToken refineFront = new TfToken("refine:front");
  public static TfToken refineLeft = new TfToken("refine:left");
  public static TfToken refineRight = new TfToken("refine:right");
  public static TfToken refineTop = new TfToken("refine:top");
  public static TfToken repeat = new TfToken("repeat");
  public static TfToken riCombineMode = new TfToken("ri:combineMode");
  public static TfToken riDensity = new TfToken("ri:density");
  public static TfToken riDiffuse = new TfToken("ri:diffuse");
  public static TfToken riExposure = new TfToken("ri:exposure");
  public static TfToken riIntensity = new TfToken("ri:intensity");
  public static TfToken riIntensityNearDist = new TfToken("ri:intensityNearDist");
  public static TfToken riInvert = new TfToken("ri:invert");
  public static TfToken riLightGroup = new TfToken("ri:lightGroup");
  public static TfToken riPortalIntensity = new TfToken("ri:portal:intensity");
  public static TfToken riPortalTint = new TfToken("ri:portal:tint");
  public static TfToken riSamplingFixedSampleCount = new TfToken("ri:sampling:fixedSampleCount");
  public static TfToken riSamplingImportanceMultiplier = new TfToken("ri:sampling:importanceMultiplier");
  public static TfToken riShadowThinShadow = new TfToken("ri:shadow:thinShadow");
  public static TfToken riSpecular = new TfToken("ri:specular");
  public static TfToken riTextureGamma = new TfToken("ri:texture:gamma");
  public static TfToken riTextureSaturation = new TfToken("ri:texture:saturation");
  public static TfToken riTraceLightPaths = new TfToken("ri:trace:lightPaths");
  public static TfToken scaleDepth = new TfToken("scale:depth");
  public static TfToken scaleHeight = new TfToken("scale:height");
  public static TfToken scaleWidth = new TfToken("scale:width");
  public static TfToken screen = new TfToken("screen");
  public static TfToken skyTint = new TfToken("skyTint");
  public static TfToken spherical = new TfToken("spherical");
  public static TfToken spline = new TfToken("spline");
  public static TfToken sunDirection = new TfToken("sunDirection");
  public static TfToken sunSize = new TfToken("sunSize");
  public static TfToken sunTint = new TfToken("sunTint");
  public static TfToken textureFillColor = new TfToken("texture:fillColor");
  public static TfToken textureInvertU = new TfToken("texture:invertU");
  public static TfToken textureInvertV = new TfToken("texture:invertV");
  public static TfToken textureMap = new TfToken("texture:map");
  public static TfToken textureOffsetU = new TfToken("texture:offsetU");
  public static TfToken textureOffsetV = new TfToken("texture:offsetV");
  public static TfToken textureScaleU = new TfToken("texture:scaleU");
  public static TfToken textureScaleV = new TfToken("texture:scaleV");
  public static TfToken textureWrapMode = new TfToken("texture:wrapMode");
  public static TfToken useColor = new TfToken("useColor");
  public static TfToken useThroughput = new TfToken("useThroughput");
  public static TfToken values = new TfToken("values");
  public static TfToken width = new TfToken("width");
  public static TfToken year = new TfToken("year");
  public static TfToken zone = new TfToken("zone");
%}

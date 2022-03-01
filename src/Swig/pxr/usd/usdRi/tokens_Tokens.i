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
  public static TfToken bspline = new TfToken("bspline");
  public static TfToken catmullRom = new TfToken("catmull-rom");
  public static TfToken constant = new TfToken("constant");
  public static TfToken interpolation = new TfToken("interpolation");
  public static TfToken linear = new TfToken("linear");
  public static TfToken outputsRiDisplacement = new TfToken("outputs:ri:displacement");
  public static TfToken outputsRiSurface = new TfToken("outputs:ri:surface");
  public static TfToken outputsRiVolume = new TfToken("outputs:ri:volume");
  public static TfToken positions = new TfToken("positions");
  public static TfToken renderContext = new TfToken("ri");
  public static TfToken riTextureGamma = new TfToken("ri:texture:gamma");
  public static TfToken riTextureSaturation = new TfToken("ri:texture:saturation");
  public static TfToken spline = new TfToken("spline");
  public static TfToken values = new TfToken("values");
%}

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

%typemap(cscode) UsdShadeTokens %{
  public static TfToken allPurpose = new TfToken("");
  public static TfToken bindMaterialAs = new TfToken("bindMaterialAs");
  public static TfToken connectedSourceFor = new TfToken("connectedSourceFor:");
  public static TfToken derivesFrom = new TfToken("derivesFrom");
  public static TfToken displacement = new TfToken("displacement");
  public static TfToken fallbackStrength = new TfToken("fallbackStrength");
  public static TfToken full = new TfToken("full");
  public static TfToken id = new TfToken("id");
  public static TfToken infoId = new TfToken("info:id");
  public static TfToken infoImplementationSource = new TfToken("info:implementationSource");
  public static TfToken inputs = new TfToken("inputs:");
  public static TfToken interfaceOnly = new TfToken("interfaceOnly");
  public static TfToken interfaceRecipientsOf = new TfToken("interfaceRecipientsOf:");
  public static TfToken interface_ = new TfToken("interface:");
  public static TfToken materialBind = new TfToken("materialBind");
  public static TfToken materialBinding = new TfToken("material:binding");
  public static TfToken materialBindingCollection = new TfToken("material:binding:collection");
  public static TfToken materialVariant = new TfToken("materialVariant");
  public static TfToken outputs = new TfToken("outputs:");
  public static TfToken outputsDisplacement = new TfToken("outputs:displacement");
  public static TfToken outputsSurface = new TfToken("outputs:surface");
  public static TfToken outputsVolume = new TfToken("outputs:volume");
  public static TfToken preview = new TfToken("preview");
  public static TfToken sdrMetadata = new TfToken("sdrMetadata");
  public static TfToken sourceAsset = new TfToken("sourceAsset");
  public static TfToken sourceCode = new TfToken("sourceCode");
  public static TfToken strongerThanDescendants = new TfToken("strongerThanDescendants");
  public static TfToken surface = new TfToken("surface");
  public static TfToken universalRenderContext = new TfToken("");
  public static TfToken universalSourceType = new TfToken("");
  public static TfToken volume = new TfToken("volume");
  public static TfToken weakerThanDescendants = new TfToken("weakerThanDescendants");
%}

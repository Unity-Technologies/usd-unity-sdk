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

namespace USD.NET.Unity {

  [UsdSchema("Shader")]
  public class StandardShaderSample : ShaderSample {
    // Albedo will either be a solid color or connected to a texture.
    [UsdRelationship()]
    public string albedoTexture;
    public UnityEngine.Color albedoValue;
  }

}

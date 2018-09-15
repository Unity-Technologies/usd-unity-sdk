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

using System.Collections.Generic;
using pxr;
using UnityEngine;

namespace USD.NET.Unity {
  public class MaterialMap {

    private Dictionary<Color, Material> m_colorMap = new Dictionary<Color, Material>();

    public Material SolidColorMasterMaterial { get; set; }
    
    public MaterialMap(Material solidColorMasterMaterial) {
      SolidColorMasterMaterial = solidColorMasterMaterial;
    }

    public Material GetMaterialForSolidColor(Color color) {
      Material material;
      if (m_colorMap.TryGetValue(color, out material)) {
        return material;
      }

      material = Material.Instantiate(SolidColorMasterMaterial);
      material.color = color;
      m_colorMap[color] = material;

      return material;
    }

  }
}

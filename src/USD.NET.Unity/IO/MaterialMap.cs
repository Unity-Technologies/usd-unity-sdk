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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// A mapping from USD shader ID to Unity Material.
  /// </summary>
  public class MaterialMap : IEnumerable<KeyValuePair<string, Material>> {

    /// <summary>
    /// A mapping from USD shader ID to Unity material.
    /// </summary>
    private Dictionary<string, Material> m_map = new Dictionary<string, Material>();

    private Dictionary<Color, Material> m_colorMap = new Dictionary<Color, Material>();

    /// <summary>
    /// A material to use when no material could be found.
    /// </summary>
    public Material FallbackMasterMaterial { get; set; }

    public MaterialMap() {
    }

    public MaterialMap(Material solidColorMaterial, Material fallbackMaterial) {
      FallbackMasterMaterial = fallbackMaterial;
    }

    /// <summary>
    /// Returns a shared instance of the mapped material, else null. Does not throw exceptions.
    /// </summary>
    /// <param name="shaderId">The USD shader ID</param>
    public Material this[string shaderId]
    {
      get
      {
        Material mat;
        if (m_map.TryGetValue(shaderId, out mat)) {
          return mat;
        }

        // Allow invalid material lookups.
        return null;
      }
      set
      {
        m_map[shaderId] = value;
      }
    }

    /// <summary>
    /// Returns an instance of the solid color material, setting the color. If the same color is
    /// requested multiple times, a shared material is returned.
    /// </summary>
    public Material InstantiateSolidColor(Color color) {
      Material material;
      if (m_colorMap.TryGetValue(color, out material)) {
        return material;
      }

      material = Material.Instantiate(FallbackMasterMaterial);
      AssignColor(material, color);
      m_colorMap[color] = material;

      return material;
    }

    public static void AssignColor(Material material, Color color) {
      if (material.HasProperty("_Color")) {
        material.color = color;
      }
      if (material.HasProperty("_BaseColor")) {
        material.SetColor("_BaseColor", color);
      }
    }

    public IEnumerator GetEnumerator() {
      return ((IEnumerable)m_map).GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, Material>>
        IEnumerable<KeyValuePair<string, Material>>.GetEnumerator() {
      return m_map.GetEnumerator();
    }
  }
}

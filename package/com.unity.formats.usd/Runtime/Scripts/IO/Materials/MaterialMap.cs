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
    /// A callback to preform the material binding when requesting bindings.
    /// </summary>
    /// <param name="mat">The new material to bind.</param>
    public delegate void MaterialBinder(Scene scene,
                                        Material mat,
                                        List<string> uvPrimvars);

    /// <summary>
    /// A mapping from USD shader ID to Unity material.
    /// </summary>
    private Dictionary<string, Material> m_map = new Dictionary<string, Material>();

    private Dictionary<Color, Material> m_colorMap = new Dictionary<Color, Material>();

    /// <summary>
    /// Bindings requested, to be processed in bulk.
    /// </summary>
    private Dictionary<string, MaterialBinder> m_requestedBindings = new Dictionary<string, MaterialBinder>();

    private Dictionary<string, List<string>> m_primvars = new Dictionary<string, List<string>>();

    /// <summary>
    /// A material to use when no material could be found.
    /// </summary>
    public Material FallbackMasterMaterial { get; set; }

    public Material SpecularWorkflowMaterial { get; set; }
    public Material MetallicWorkflowMaterial { get; set; }

    public MaterialMap() {
      var pipeline = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
      if (!pipeline) {
        // Fallback to the built-in render pipeline, assume Standard PBS shader.
        SpecularWorkflowMaterial = new Material(Shader.Find("Standard (Specular setup)"));
        MetallicWorkflowMaterial = new Material(Shader.Find("Standard"));
        FallbackMasterMaterial = new Material(Shader.Find("USD/StandardVertexColor"));
      } else {
        SpecularWorkflowMaterial = Material.Instantiate(pipeline.defaultMaterial);
        MetallicWorkflowMaterial = Material.Instantiate(pipeline.defaultMaterial);
        FallbackMasterMaterial = new Material(Shader.Find("USD/SrpVertexColor"));
      }
    }

    /// <summary>
    /// Clears the requested bindings, returning the current queue.
    /// </summary>
    public Dictionary<string, MaterialBinder> ClearRequestedBindings() {
      var bindings = m_requestedBindings;
      m_requestedBindings = new Dictionary<string, MaterialBinder>();
      
      return bindings;
    }

    /// <summary>
    /// Request an object to be bound at a later time in a vectorized request.
    /// </summary>
    /// <param name="usdPath">The USD path to the object.</param>
    /// <param name="binder">
    /// A callback which will accept the material and bind it to a Unity object
    /// </param>
    public void RequestBinding(string usdPath, MaterialBinder binder) {
      m_requestedBindings.Add(usdPath, binder);
    }

    /// <summary>
    /// Returns a shared instance of the mapped material, else null. Does not throw exceptions.
    /// </summary>
    /// <param name="shaderId">The USD path to the material</param>
    public Material this[string path]
    {
      get
      {
        Material mat;
        if (m_map.TryGetValue(path, out mat)) {
          return mat;
        }

        // Allow invalid material lookups.
        return null;
      }
      set
      {
        m_map[path] = value;
      }
    }

    public List<string> GetPrimvars(string materialPath) {
      List<string> primvars;
      m_primvars.TryGetValue(materialPath, out primvars);
      return primvars;
    }

    public void SetPrimvars(string materialPath, List<string> primvars) {
      m_primvars[materialPath] = primvars;
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

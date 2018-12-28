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
using System.Collections.Generic;
using UnityEngine;

namespace USD.NET.Unity {
  public abstract class MaterialAdapter {
    public Material Material { get; private set; }
    public bool IsMetallicWorkflow { get; private set; }

    public Color? Diffuse;
    public Texture2D DiffuseMap;

    public Color? Specular;
    public Texture2D SpecularMap;

    public Vector3? Normal;
    public Texture2D NormalMap;

    public float? Displacement;
    public Texture2D DisplacementMap;

    public float? Occlusion;
    public Texture2D OcclusionMap;

    public Color? Emission;
    public Texture2D EmissionMap;

    public float? Roughness;
    public Texture2D RoughnessMap;

    public float? Metallic;
    public Texture2D MetallicMap;

    public MaterialAdapter(Material material) {
      Material = material;
    }

    abstract public void ImportFromUsd();
    abstract public void ExportToUsd();

    protected void ImportColorOrMap(Scene scene,
                                    Connectable<Vector3> usdParam,
                                    SceneImportOptions options,
                                    ref Texture2D map,
                                    ref Color? value,
                                    out string uvPrimvar) {
      uvPrimvar = null;
      if (usdParam.IsConnected()) {
        map = MaterialImporter.ImportConnectedTexture(scene, usdParam, options, out uvPrimvar);
      } else {
        var rgb = usdParam.defaultValue;
        value = new Color(rgb.x, rgb.y, rgb.z).gamma;
      }
    }

    protected void ImportValueOrMap<T>(Scene scene,
                                    Connectable<T> usdParam,
                                    SceneImportOptions options,
                                    ref Texture2D map,
                                    ref T? value,
                                    out string uvPrimvar) where T: struct {
      uvPrimvar = null;
      if (usdParam.IsConnected()) {
        map = MaterialImporter.ImportConnectedTexture(scene, usdParam, options, out uvPrimvar);
      } else {
        value = usdParam.defaultValue;
      }
    }

    private void MergePrimvars(string newPrimvar, List<string> primvars) {
      if (string.IsNullOrEmpty(newPrimvar)) {
        return;
      }
      string localName = newPrimvar;
      if (!string.IsNullOrEmpty(primvars.Find(str => str == localName))) {
        return;
      }
      primvars.Add(localName);
    }

    public virtual void ImportParametersFromUsd(Scene scene,
                                                string materialPath,
                                                MaterialSample materialSample,
                                                PreviewSurfaceSample previewSurf,
                                                SceneImportOptions options) {
      var primvars = new List<string>();
      string uvPrimvar = null;
      ImportColorOrMap(scene, previewSurf.diffuseColor, options, ref DiffuseMap, ref Diffuse, out uvPrimvar);
      MergePrimvars(uvPrimvar, primvars);

      ImportColorOrMap(scene, previewSurf.emissiveColor, options, ref EmissionMap, ref Emission, out uvPrimvar);
      MergePrimvars(uvPrimvar, primvars);

      ImportValueOrMap(scene, previewSurf.normal, options, ref NormalMap, ref Normal, out uvPrimvar);
      MergePrimvars(uvPrimvar, primvars);

      ImportValueOrMap(scene, previewSurf.displacement, options, ref DisplacementMap, ref Displacement, out uvPrimvar);
      MergePrimvars(uvPrimvar, primvars);

      ImportValueOrMap(scene, previewSurf.occlusion, options, ref OcclusionMap, ref Occlusion, out uvPrimvar);
      MergePrimvars(uvPrimvar, primvars);

      ImportValueOrMap(scene, previewSurf.roughness, options, ref RoughnessMap, ref Roughness, out uvPrimvar);
      MergePrimvars(uvPrimvar, primvars);

      if (previewSurf.useSpecularWorkflow.defaultValue == 1) {
        ImportColorOrMap(scene, previewSurf.specularColor, options, ref SpecularMap, ref Specular, out uvPrimvar);
        MergePrimvars(uvPrimvar, primvars);
      } else {
        ImportValueOrMap(scene, previewSurf.metallic, options, ref MetallicMap, ref Metallic, out uvPrimvar);
        MergePrimvars(uvPrimvar, primvars);
      }

      options.materialMap.SetPrimvars(materialPath, primvars);
    }

  }

  public class StandardShaderAdapter : MaterialAdapter {

    public StandardShaderAdapter(Material material) : base(material) {
    }

    public override void ExportToUsd() {
      throw new NotImplementedException();
    }

    public override void ImportFromUsd() {
      Material mat = Material;

      if (DiffuseMap) {
        mat.SetTexture("_MainTex", DiffuseMap);
      } else {
        mat.color = Diffuse.GetValueOrDefault(mat.color);
      }

      if (NormalMap) {
        mat.SetTexture("_BumpMap", NormalMap);
      } else {
        // TODO: Unity has no notion of a constant normal map.
      }

      if (DisplacementMap) {
        mat.SetTexture("_ParallaxMap", DisplacementMap);
      } else {
        // TODO: Unity has no notion of a parallax map.
      }

      if (OcclusionMap) {
        mat.SetTexture("_OcclusionMap", OcclusionMap);
      } else {
        // TODO: Unity has no notion of a constant occlusion value.
      }

      if (EmissionMap) {
        mat.SetTexture("_EmissionMap", EmissionMap);
        mat.EnableKeyword("_EMISSION");
      } else {
        var rgb = Emission.GetValueOrDefault(Color.black);
        mat.SetColor("_EmissionColor", rgb);
        if (rgb.r > 0 || rgb.g > 0 || rgb.b > 0) {
          mat.EnableKeyword("_EMISSION");
        }
      }

      if (!IsMetallicWorkflow) {
        if (SpecularMap) {
          mat.SetTexture("_SpecGlossMap", SpecularMap);
        } else {
          var rgb = Specular.GetValueOrDefault(Color.gray);
          mat.SetColor("_SpecColor", rgb);
        }

        if (RoughnessMap) {
          // Roughness for spec setup is tricky, since it may require merging two textures.
          // For now, just detect that case and issue a warning (e.g. when roughness has a map,
          // but isn't the albedo or spec map).
          // Roughness also needs to be converted to glossiness.
          if (RoughnessMap != SpecularMap && SpecularMap != null) {
            var specGlossTex = MaterialImporter.CombineRoughnessToGloss(SpecularMap, RoughnessMap);
            mat.SetTexture("_SpecGlossMap", specGlossTex);
            mat.EnableKeyword("_SPECGLOSSMAP");
          } else if (SpecularMap == null && RoughnessMap != DiffuseMap) {
            var mainGlossTex = MaterialImporter.CombineRoughnessToGloss(DiffuseMap, RoughnessMap);
            mat.SetTexture("_SpecGlossMap", mainGlossTex);
            mat.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
          } else {
            // TODO: create a new texture with constant spec value, combined with roughness texture.
          }
        } else {
          mat.SetFloat("_Glossiness", 1 - Roughness.GetValueOrDefault(.5f));
        }
      } else {
        if (MetallicMap) {
          mat.SetTexture("_MetallicGlossMap", MetallicMap);
        } else {
          mat.SetFloat("_Metallic", Metallic.GetValueOrDefault(0));
        }

        if (RoughnessMap) {
          // In this case roughness get its own map, but still must be converted to glossiness.
          mat.SetTexture("_SpecGlossMap", RoughnessMap);
        } else {
          mat.SetFloat("_Glossiness", 1 - Roughness.GetValueOrDefault(.5f));
        }
      }
    }
  }

}

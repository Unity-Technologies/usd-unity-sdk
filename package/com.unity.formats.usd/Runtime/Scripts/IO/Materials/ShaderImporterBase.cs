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
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public abstract class ShaderImporterBase
    {
        public Material Material { get; private set; }
        public bool IsSpecularWorkflow { get; private set; }

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

        public float? Clearcoat;
        public Texture2D ClearcoatMap;

        public float? ClearcoatRoughness;

        public float? OpacityThreshold;

        public float? Opacity;
        public Texture2D OpacityMap;

        public ShaderImporterBase(Material material)
        {
            Material = material;
        }

        abstract public void ImportFromUsd();

        protected void ImportColorOrMap(Scene scene,
            Connectable<Vector3> usdParam,
            bool isNormalMap,
            SceneImportOptions options,
            ref Texture2D map,
            ref Color? value,
            out string uvPrimvar)
        {
            uvPrimvar = null;
            if (usdParam.IsConnected())
            {
                map = MaterialImporter.ImportConnectedTexture(scene, usdParam, isNormalMap, options, out uvPrimvar);
            }
            else
            {
                var rgb = usdParam.defaultValue;
                value = new Color(rgb.x, rgb.y, rgb.z).gamma;
            }
        }

        protected void ImportValueOrMap<T>(Scene scene,
            Connectable<T> usdParam,
            bool isNormalMap,
            SceneImportOptions options,
            ref Texture2D map,
            ref T? value,
            out string uvPrimvar) where T : struct
        {
            uvPrimvar = null;
            if (usdParam.IsConnected())
            {
                map = MaterialImporter.ImportConnectedTexture(scene, usdParam, isNormalMap, options, out uvPrimvar);
            }
            else
            {
                value = usdParam.defaultValue;
            }
        }

        private void MergePrimvars(string newPrimvar, List<string> primvars)
        {
            if (string.IsNullOrEmpty(newPrimvar))
            {
                return;
            }

            string localName = newPrimvar;
            if (!string.IsNullOrEmpty(primvars.Find(str => str == localName)))
            {
                return;
            }

            primvars.Add(localName);
        }

        public virtual void ImportParametersFromUsd(Scene scene,
            string materialPath,
            MaterialSample materialSample,
            PreviewSurfaceSample previewSurf,
            SceneImportOptions options)
        {
            var primvars = new List<string>();
            string uvPrimvar = null;

            IsSpecularWorkflow = previewSurf.useSpecularWorkflow.defaultValue == 1;

            ImportColorOrMap(scene, previewSurf.diffuseColor, false, options, ref DiffuseMap, ref Diffuse,
                out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            ImportColorOrMap(scene, previewSurf.emissiveColor, false, options, ref EmissionMap, ref Emission,
                out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            ImportValueOrMap(scene, previewSurf.normal, true, options, ref NormalMap, ref Normal, out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            ImportValueOrMap(scene, previewSurf.displacement, false, options, ref DisplacementMap, ref Displacement,
                out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            ImportValueOrMap(scene, previewSurf.occlusion, false, options, ref OcclusionMap, ref Occlusion,
                out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            ImportValueOrMap(scene, previewSurf.roughness, false, options, ref RoughnessMap, ref Roughness,
                out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            ImportValueOrMap(scene, previewSurf.clearcoat, false, options, ref ClearcoatMap, ref Clearcoat,
                out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            ClearcoatRoughness = previewSurf.clearcoatRoughness.defaultValue;
            OpacityThreshold = previewSurf.opacityThreshold.defaultValue;

            ImportValueOrMap(scene, previewSurf.opacity, false, options, ref OpacityMap, ref Opacity,
                out uvPrimvar);
            MergePrimvars(uvPrimvar, primvars);

            if (IsSpecularWorkflow)
            {
                ImportColorOrMap(scene, previewSurf.specularColor, false, options, ref SpecularMap, ref Specular,
                    out uvPrimvar);
                MergePrimvars(uvPrimvar, primvars);
            }
            else
            {
                ImportValueOrMap(scene, previewSurf.metallic, false, options, ref MetallicMap, ref Metallic,
                    out uvPrimvar);
                MergePrimvars(uvPrimvar, primvars);
            }

            options.materialMap.SetPrimvars(materialPath, primvars);
        }
    }
}

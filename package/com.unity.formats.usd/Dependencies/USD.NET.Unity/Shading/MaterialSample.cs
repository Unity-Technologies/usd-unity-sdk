// Copyright 2021 Unity Technologies. All rights reserved.
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

using System;
using pxr;

namespace USD.NET.Unity
{
    [Serializable]
    [UsdSchema("Material")]
    public class MaterialSample : SampleBase
    {
        public MaterialSample() : base()
        {
        }

        public MaterialSample(string surfacePath) : base()
        {
            surface.SetConnectedPath(surfacePath);
        }

        public MaterialSample(string surfacePath, string displacementPath) : base()
        {
            surface.SetConnectedPath(surfacePath);
            displacement.SetConnectedPath(displacementPath);
        }

        [UsdNamespace("outputs")]
        public Connectable<TfToken> surface = new Connectable<TfToken>();

        [UsdNamespace("outputs")]
        public Connectable<TfToken> displacement = new Connectable<TfToken>();

        // A material instance may require specific keywords to be enabled with respect to the shader
        // configuration. This attribute enables the material writer to include required keywords.
        public string[] requiredKeywords;

        // ------------------------------------------------------------------------------------------ //
        // Helper functions.
        // ------------------------------------------------------------------------------------------ //

        /// <summary>
        /// Reads the material associated with the geometry at the target path. If a valid material is
        /// bound, the "surface" shader is inspected, if valid, the shader ID is queried and returned
        /// via the shaderId output parameter. Returns true when shaderId is set.
        /// </summary>
        static public bool ReadMaterial(Scene scene,
            string geometryPath,
            MaterialSample materialSample,
            out string shaderId)
        {
            // Discover the material bound to the geometry.
            // Note that with this structure, the geometry type does not matter.
            var geomMaterialBinding = new MaterialBindingSample();

            // Discover the binding for the geometry.
            // Note that the details of how to import geometry is handled in a different example.
            scene.Read(geometryPath, geomMaterialBinding);

            // In general, there is no guarantee that any prim is bound, so this null check is always
            // required.
            if (geomMaterialBinding.binding.targetPaths == null)
            {
                // In this example, exceptions are thrown on invalid data, but in practice invalid bindings
                // should probably be collected into a summary report and emitted as a single warning.
                throw new System.Exception("Expected bound material");
            }

            // Similarly, there is no guarantee the source file is well formed, so an invalid number of
            // target paths must also be handled.
            if (geomMaterialBinding.binding.targetPaths.Length != 1)
            {
                throw new System.Exception("Expected exactly one target path for material binding");
            }

            // Read the material, which will provide the shader binding.
            scene.Read(geomMaterialBinding.binding.targetPaths[0], materialSample);

            // Again, the surface may be invalid, so protect against broken scene description.
            if (materialSample.surface == null || string.IsNullOrEmpty(materialSample.surface.connectedPath))
            {
                throw new System.Exception("Material had no surface bound");
            }

            string shaderPath = materialSample.surface.connectedPath.Replace(".outputs:out", "");
            var prim = scene.Stage.GetPrimAtPath(new pxr.SdfPath(shaderPath));
            if (prim == null || !prim.IsValid())
            {
                throw new System.Exception("Invalid shader prim");
            }

            var val = new VtValue();
            if (!prim.GetAttributeValue(new TfToken("info:id"), val, scene.Time ?? UsdTimeCode.Default()))
            {
                throw new System.Exception("Shader prim had no identifier");
            }

            shaderId = pxr.UsdCs.VtValueToTfToken(val).ToString();
            return true;
        }

        /// <summary>
        /// Binds the prim to the given material.
        /// https://graphics.pixar.com/usd/docs/api/class_usd_shade_material.html#a116ed8259600f7a48f3792364252a4e1
        /// </summary>
        static public void Bind(Scene scene, string primPath, string materialPath)
        {
            var materialPrim = scene.Stage.GetPrimAtPath(new SdfPath(materialPath));
            if (!materialPrim.IsValid())
            {
                throw new ApplicationException("Invalid material prim <" + materialPath + ">");
            }
            var mat = new UsdShadeMaterial(materialPrim);
            if (!mat.GetPrim().IsValid())
            {
                throw new ApplicationException("Invalid material on prim <" + materialPath + ">");
            }
            var bindingApi = new UsdShadeMaterialBindingAPI(scene.Stage.GetPrimAtPath(new SdfPath(primPath)));
            bindingApi.Bind(mat);
        }

        /// <summary>
        /// Unbinds the prim from all materials.
        /// https://graphics.pixar.com/usd/docs/api/class_usd_shade_material.html#a6fc9d56d2e2e826aa3d96711c7b0e605
        /// </summary>
        static public void Unbind(Scene scene, string primPath)
        {
            // TODO: verify all objects before use, throw exceptions
            var prim = scene.Stage.GetPrimAtPath(new SdfPath(primPath));
            if (!prim.IsValid())
            {
                throw new ApplicationException("Invalid prim <" + primPath + ">");
            }
            var bindingApi = new UsdShadeMaterialBindingAPI(prim);
            bindingApi.UnbindAllBindings();
        }
    }
}

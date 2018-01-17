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

namespace USD.NET.Unity {

  [UsdSchema("Material")]
  public class MaterialSample : SampleBase {

    [UsdNamespace("outputs:glslfx")]
    public Connectable<UnityEngine.Color> surface = new Connectable<UnityEngine.Color>();

    /// <summary>
    /// Binds the prim to the given material.
    /// https://graphics.pixar.com/usd/docs/api/class_usd_shade_material.html#a116ed8259600f7a48f3792364252a4e1
    /// </summary>
    public void Bind(Scene scene, string primPath, string materialPath) {
      var prim = scene.Stage.GetPrimAtPath(new SdfPath(materialPath));
      if (!prim.IsValid()) {
        throw new ApplicationException("Invalid prim <" + primPath + ">");
      }
      var mat = new UsdShadeMaterial(prim);
      if (!mat.GetPrim().IsValid()) {
        throw new ApplicationException("Invalid material <" + materialPath + ">");
      }
      mat.Bind(scene.Stage.GetPrimAtPath(new SdfPath(primPath)));
    }

    /// <summary>
    /// Unbinds the prim from all materials.
    /// https://graphics.pixar.com/usd/docs/api/class_usd_shade_material.html#a6fc9d56d2e2e826aa3d96711c7b0e605
    /// </summary>
    public void Unbind(Scene scene, string primPath) {
      // TODO: verify all objects before use, throw exceptions
      var prim = scene.Stage.GetPrimAtPath(new SdfPath(primPath));
      if (!prim.IsValid()) {
        throw new ApplicationException("Invalid prim <" + primPath + ">");
      }
      UsdShadeMaterial.Unbind(prim);
    }
  }

}

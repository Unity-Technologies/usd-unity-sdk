// Copyright 2018 Google Inc. All rights reserved.
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

using System.Linq;
using pxr;
using USD.NET;
using USD.NET.Unity;
using UnityEngine;

namespace Tests.Cases {
  class UsdPreviewSurfaceTests : UnitTest {

    public static void ReadWriteTest() {

      // Game plan:
      //   1. Create a cube
      //   2. Create a material
      //   3. Create a shader
      //   4. Create a texture
      //   5. Connect the material to the shader's output
      //   6. Connect the shader's albedo parameter to the texture's output
      //   7. Connect the texture to the source file on disk
      //   8. Write all values
      //   9. Bind the cube to the material
      var scene = Scene.Create();

      var cubePath     = "/Model/Geom/Cube";
      var materialPath = "/Model/Materials/SimpleMat";
      var shaderPath   = "/Model/Materials/SimpleMat/PreviewMaterial";
      var texturePath  = "/Model/Materials/SimpleMat/TextureReader";
      var primvarReaderPath  = "/Model/Materials/SimpleMat/UvReader";

      var cube = new CubeSample();
      cube.size = 1;

      var material = new MaterialSample();
      material.surface.SetConnectedPath(shaderPath, "outputs:result");

      var shader = new PreviewSurfaceSample();
      shader.diffuseColor.defaultValue = Vector3.one;
      shader.diffuseColor.SetConnectedPath(texturePath, "outputs:rgb");

      var texture = new TextureReaderSample();
      texture.file.defaultValue = new SdfAssetPath(@"C:\A\Bogus\Texture\Path.png");

      var primvarReader = new PrimvarReaderSample<Vector2>();
      primvarReader.varname.defaultValue = new TfToken("st");

      scene.Write("/Model", new XformSample());
      scene.Write("/Model/Geom", new XformSample());
      scene.Write("/Model/Materials", new XformSample());
      scene.Write(cubePath, cube);
      scene.Write(materialPath, material);
      scene.Write(shaderPath, shader);
      scene.Write(texturePath, texture);
      scene.Write(primvarReaderPath, primvarReader);

      MaterialSample.Bind(scene, cubePath, materialPath);

      var material2 = new MaterialSample();
      var shader2 = new PreviewSurfaceSample();
      var texture2 = new TextureReaderSample();
      var primvarReader2 = new PrimvarReaderSample<Vector2>();

      scene.Read(materialPath, material2);
      scene.Read(shaderPath, shader2);
      scene.Read(texturePath, texture2);
      scene.Read(primvarReaderPath, primvarReader2);

      var param = shader2.GetInputParameters().First();
      AssertEqual(shader.diffuseColor.connectedPath, param.connectedPath);
      AssertEqual("diffuseColor", param.usdName);
      AssertEqual(shader.diffuseColor.defaultValue, param.value);
      AssertEqual("_DiffuseColor", param.unityName);

      AssertEqual(material.surface.defaultValue, material2.surface.defaultValue);
      AssertEqual(material.surface.connectedPath, material2.surface.connectedPath);
      AssertEqual(shader.diffuseColor.defaultValue, shader2.diffuseColor.defaultValue);
      AssertEqual(shader.diffuseColor.connectedPath, shader2.diffuseColor.connectedPath);
      AssertEqual(shader.id, shader2.id);
      AssertEqual(texture.file.defaultValue, texture2.file.defaultValue);
      AssertEqual(primvarReader.varname.defaultValue, primvarReader.varname.defaultValue);

      PrintScene(scene);
    }
  }
}

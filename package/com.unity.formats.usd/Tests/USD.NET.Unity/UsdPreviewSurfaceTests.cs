using System.Linq;
using NUnit.Framework;
using pxr;
using UnityEngine;

namespace USD.NET.Unity.Tests
{
    class UsdPreviewSurfaceTests : UsdTests
    {
        [Test]
        public static void ReadWriteTest()
        {
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

            var cubePath = "/Model/Geom/Cube";
            var materialPath = "/Model/Materials/SimpleMat";
            var shaderPath = "/Model/Materials/SimpleMat/PreviewMaterial";
            var texturePath = "/Model/Materials/SimpleMat/TextureReader";
            var primvarReaderPath = "/Model/Materials/SimpleMat/UvReader";

            var cube = new CubeSample {size = 1};

            var material = new MaterialSample();
            material.surface.SetConnectedPath(shaderPath, "outputs:result");

            var shader = new PreviewSurfaceSample {diffuseColor = {defaultValue = Vector3.one}};
            shader.diffuseColor.SetConnectedPath(texturePath, "outputs:rgb");

            var texture = new TextureReaderSample
            {
                file = {defaultValue = new SdfAssetPath(@"C:\A\Bogus\Texture\Path.png")}
            };

            var primvarReader = new PrimvarReaderSample<Vector2> {varname = {defaultValue = new TfToken("st")}};

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
            Assert.AreEqual(shader.diffuseColor.connectedPath, param.connectedPath);
            Assert.AreEqual("diffuseColor", param.usdName);
            Assert.AreEqual(shader.diffuseColor.defaultValue, param.value);
            Assert.AreEqual("_DiffuseColor", param.unityName);

            Assert.AreEqual(material.surface.defaultValue, material2.surface.defaultValue);
            Assert.AreEqual(material.surface.connectedPath, material2.surface.connectedPath);
            Assert.AreEqual(shader.diffuseColor.defaultValue, shader2.diffuseColor.defaultValue);
            Assert.AreEqual(shader.diffuseColor.connectedPath, shader2.diffuseColor.connectedPath);
            Assert.AreEqual(shader.id, shader2.id);
            Assert.AreEqual(texture.file.defaultValue, texture2.file.defaultValue);
            Assert.AreEqual(primvarReader.varname.defaultValue, primvarReader.varname.defaultValue);
        }
    }
}

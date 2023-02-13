using System.Linq;
using NUnit.Framework;
using UnityEngine;
using USD.NET.Tests;

namespace USD.NET.Unity.Tests
{
    class UsdShadeTests : UsdTests
    {
        /// <summary>
        /// Read and write materials with as little code as possible.
        /// </summary>
        [Test]
        public static void MaterialIoTest()
        {
            var scene = Scene.Create();
            scene.Write("/Model/Geom/Cube", new CubeSample(1.0));
            scene.Write("/Model/Geom/Cube", new MaterialBindingSample("/Model/Materials/SampleMat"));
            scene.Write("/Model/Materials/SampleMat",
                new MaterialSample("/Model/Materials/PrevewShader.outputs:result"));

            var previewSurface = new PreviewSurfaceSample();
            previewSurface.diffuseColor.SetConnectedPath("/Model/Materials/Tex.outputs:result");
            scene.Write("/Model/Materials/PrevewShader", previewSurface);
            scene.Write("/Model/Materials/Tex", new TextureReaderSample("C:\\foo\\bar.png"));

            var cube = new CubeSample();
            scene.Read("/Model/Geom/Cube", cube);
            var binding = new MaterialBindingSample();
            scene.Read("/Model/Geom/Cube", binding);
            var material = new MaterialSample();
            scene.Read(binding.binding.GetOnlyTarget(), material);
            var shader = new PreviewSurfaceSample();
            scene.Read(material.surface.GetConnectedPath(), shader);
        }

        [Test]
        public static void MaterialBindTest()
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
            var shaderPath = "/Model/Materials/SimpleMat/StandardShader";
            var texturePath = "/Model/Materials/SimpleMat/AlbedoTexture";

            var cube = new CubeSample { size = 1 };

            var material = new MaterialSample();
            material.surface.SetConnectedPath(shaderPath, "outputs:out");

            var shader = new StandardShaderSample { albedo = { defaultValue = Color.white } };
            shader.albedo.SetConnectedPath(texturePath, "outputs:out");
            Assert.AreEqual(shader.albedo.connectedPath, texturePath + ".outputs:out");

            var texture = new Texture2DSample
            {
                sourceFile = { defaultValue = @"C:\A\Bogus\Texture\Path.png" },
                sRgb = true
            };

            scene.Write("/Model", new XformSample());
            scene.Write("/Model/Geom", new XformSample());
            scene.Write("/Model/Materials", new XformSample());
            scene.Write(cubePath, cube);
            scene.Write(materialPath, material);
            scene.Write(shaderPath, shader);
            scene.Write(texturePath, texture);

            MaterialSample.Bind(scene, cubePath, materialPath);

            var material2 = new MaterialSample();
            var shader2 = new StandardShaderSample();
            var texture2 = new Texture2DSample();

            scene.Read(materialPath, material2);
            scene.Read(shaderPath, shader2);
            scene.Read(texturePath, texture2);

            var param = shader2.GetInputParameters().First();
            Assert.AreEqual(shader.albedo.connectedPath, param.connectedPath);
            Assert.AreEqual("albedo", param.usdName);
            Assert.AreEqual(shader.albedo.defaultValue, param.value);
            Assert.AreEqual("_Color", param.unityName);

            var paramTex = shader2.GetInputTextures().First();
            Assert.AreEqual(shader.albedoMap.connectedPath, paramTex.connectedPath);
            Assert.AreEqual("albedoMap", paramTex.usdName);
            Assert.AreEqual(shader.albedoMap.defaultValue, paramTex.value);
            Assert.AreEqual("_MainTex", paramTex.unityName);

            Assert.AreEqual(material.surface.defaultValue, material2.surface.defaultValue);
            Assert.AreEqual(material.surface.connectedPath, material2.surface.connectedPath);
            Assert.AreEqual(shader.albedo.defaultValue, shader2.albedo.defaultValue);
            Assert.AreEqual(shader.albedo.connectedPath, shader2.albedo.connectedPath);
            Assert.AreEqual(shader.id, shader2.id);
            Assert.AreEqual(texture.sourceFile.defaultValue, texture2.sourceFile.defaultValue);
            Assert.AreEqual(texture.sRgb, texture2.sRgb);
        }
    }
}

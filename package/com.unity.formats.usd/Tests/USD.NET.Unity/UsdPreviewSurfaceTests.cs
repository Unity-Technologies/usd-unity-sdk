using System.Linq;
using NUnit.Framework;
using pxr;
using UnityEngine;
using USD.NET.Tests;

namespace USD.NET.Unity.Tests
{
    class UsdPreviewSurfaceTests : UsdTests
    {
        const string k_cubePath = "/Model/Geom/Cube";
        const string k_materialPath = "/Model/Materials/SimpleMat";
        const string k_shaderPath = "/Model/Materials/SimpleMat/PreviewMaterial";
        const string k_texturePath = "/Model/Materials/SimpleMat/TextureReader";
        const string k_primvarReaderPath = "/Model/Materials/SimpleMat/UvReader";

        Scene m_USDScene;
        CubeSample m_originalCube;
        MaterialSample m_originalMaterial;
        PreviewSurfaceSample m_originalShader;
        TextureReaderSample m_originalTexture;

        MaterialSample m_USDReadMaterial = new MaterialSample();
        PreviewSurfaceSample m_USDReadShader = new PreviewSurfaceSample();
        TextureReaderSample m_USDReadTexture = new TextureReaderSample();

        // Test plan:
        //   1. Create a cube
        //   2. Create a material
        //   3. Create a shader
        //   4. Create a texture
        //   5. Connect the material to the shader's output
        //   6. Connect the shader's albedo parameter to the texture's output
        //   7. Connect the texture to the source file on disk
        //   8. Write all values
        //   9. Bind the cube to the material
        //   10. Check the values read back

        [SetUp]
        public void SetupScene()
        {
            // Init the fields to be written in to the USD scene
            m_USDScene = Scene.Create();

            m_originalCube = new CubeSample { size = 1 };

            m_originalMaterial = new MaterialSample();
            m_originalMaterial.surface.SetConnectedPath(k_shaderPath, "outputs:result");

            m_originalShader = new PreviewSurfaceSample { diffuseColor = { defaultValue = Vector3.one } };
            m_originalShader.diffuseColor.SetConnectedPath(k_texturePath, "outputs:rgb");

            m_originalTexture = new TextureReaderSample
            {
                file = { defaultValue = new SdfAssetPath(@"C:\A\Bogus\Texture\Path.png") }
            };

            // Write the data
            WriteDataToScene();

            // Init the fields to be read back into from the USD Scene
            m_USDReadMaterial = new MaterialSample();
            m_USDReadShader = new PreviewSurfaceSample();
            m_USDReadTexture = new TextureReaderSample();
        }

        void WriteDataToScene()
        {
            m_USDScene.Write("/Model", new XformSample());
            m_USDScene.Write("/Model/Geom", new XformSample());
            m_USDScene.Write("/Model/Materials", new XformSample());
            m_USDScene.Write(k_cubePath, m_originalCube);
            m_USDScene.Write(k_materialPath, m_originalMaterial);
            m_USDScene.Write(k_shaderPath, m_originalShader);
            m_USDScene.Write(k_texturePath, m_originalTexture);
        }

        void ReadPrimsFromScene()
        {
            m_USDScene.Read(k_materialPath, m_USDReadMaterial);
            m_USDScene.Read(k_shaderPath, m_USDReadShader);
            m_USDScene.Read(k_texturePath, m_USDReadTexture);
        }

        void CheckShaderParams()
        {
            var param = m_USDReadShader.GetInputParameters().First();
            Assert.AreEqual(m_originalShader.diffuseColor.connectedPath, param.connectedPath);
            Assert.AreEqual("diffuseColor", param.usdName);
            Assert.AreEqual(m_originalShader.diffuseColor.defaultValue, param.value);
            Assert.AreEqual("_DiffuseColor", param.unityName);
        }

        void CheckMaterialValues()
        {
            Assert.AreEqual(m_originalMaterial.surface.defaultValue, m_USDReadMaterial.surface.defaultValue);
            Assert.AreEqual(m_originalMaterial.surface.connectedPath, m_USDReadMaterial.surface.connectedPath);
            Assert.AreEqual(m_originalShader.diffuseColor.defaultValue, m_USDReadShader.diffuseColor.defaultValue);
            Assert.AreEqual(m_originalShader.diffuseColor.connectedPath, m_USDReadShader.diffuseColor.connectedPath);
            Assert.AreEqual(m_originalShader.id, m_USDReadShader.id);
            Assert.AreEqual(m_originalTexture.file.defaultValue, m_USDReadTexture.file.defaultValue);
        }

        // Use the 'export' version of the PrimvarReaderSample, because this uses a TfToken typed varname to match the USD 20.08 specification instead of
        // the 21.11+ spec of string.
        [Test]
        public void WritingAndReadingMaterialToUSDScene_ValuesAreCorrect()
        {
            PrimvarReaderExportSample<Vector2> primvarReaderTfToken = new PrimvarReaderExportSample<Vector2> { varname = { defaultValue = new TfToken("uv") } };

            m_USDScene.Write(k_primvarReaderPath, primvarReaderTfToken);

            MaterialSample.Bind(m_USDScene, k_cubePath, k_materialPath);

            PrimvarReaderImportSample<Vector2> USDReadPrimvarReader = new PrimvarReaderImportSample<Vector2>();

            ReadPrimsFromScene();
            m_USDScene.Read(k_primvarReaderPath, USDReadPrimvarReader);

            CheckShaderParams();
            CheckMaterialValues();

            Assert.AreEqual(primvarReaderTfToken.varname.defaultValue.GetText(), USDReadPrimvarReader.varname.defaultValue);
        }

        // The default PrimvarReaderSample has varname as a string type, to match USD spec 21.11+. Test that in this case the export and import still works.
        // PrimvarReaderImportSample uses a string varname, so exporting this will match 21.11+.
        [Test]
        public void WritingAndReadingStringPrimvarReaderToUSDScene_ValuesAreCorrect()
        {
            PrimvarReaderImportSample<Vector2> primvarReaderString = new PrimvarReaderImportSample<Vector2> { varname = { defaultValue = new TfToken("uv") } }; // TfToken is automatically cast to string

            m_USDScene.Write(k_primvarReaderPath, primvarReaderString);

            MaterialSample.Bind(m_USDScene, k_cubePath, k_materialPath);

            PrimvarReaderImportSample<Vector2> USDReadPrimvarReader = new PrimvarReaderImportSample<Vector2>();

            ReadPrimsFromScene();
            m_USDScene.Read(k_primvarReaderPath, USDReadPrimvarReader);

            CheckShaderParams();
            CheckMaterialValues();

            Assert.AreEqual(primvarReaderString.varname.defaultValue, USDReadPrimvarReader.varname.defaultValue);
        }

        // TODO: Add a test for a connected varname too
    }
}

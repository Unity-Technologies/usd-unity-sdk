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

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD.Examples
{
    public class ImportMaterialsExample : MonoBehaviour
    {
        private const string kCubePath = "/Model/Geom/Cube";
        private static string m_localPath;

        // The USD Shader ID is used to index into this map to find the corresponding ShaderPair,
        // i.e. the Unity / USD shader pair that should be instantiated for the ID.
        private Dictionary<string, ShaderPair> m_shaderMap;

        // This struct associates a Unity shader with a USD shader and combines them into a single
        // object that can be stored in the shader map.
        public struct ShaderPair
        {
            public Shader unityShader;
            public ShaderSample usdShader;

            public ShaderPair(Shader unityShader,
                              ShaderSample usdShader)
            {
                this.unityShader = unityShader;
                this.usdShader = usdShader;
            }
        }

        private Transform m_cube;

        void Update()
        {
            if (!m_cube)
            {
                return;
            }

            var rot = m_cube.localEulerAngles;
            rot.y += Time.deltaTime * 10 + Mathf.Sin(Time.time) * .1f + .05f;
            m_cube.localEulerAngles = rot;
        }

        //
        // Technically, this entire example could be written in either Awake() or Start(), but Awake is
        // used here to emphasize the one-time-only initialization from the work that could
        // potentially be executed at run-time.
        //

        void Awake()
        {
            // Init USD.
            InitUsd.Initialize();

            // In this example, we assume there is a set of well known shaders, we will look up the
            // material based on the associated ID. This map allows the identifier in USD to differ from
            // the identifier specified in the Unity shader.
            m_shaderMap = new Dictionary<string, ShaderPair>();

            // Setup the Standard shader USD <-> Unity mapping.
            m_shaderMap.Add("Unity.Standard",
                new ShaderPair(UnityEngine.Shader.Find("Standard"), new StandardShaderSample()));
        }

        //
        // Start generates a USD scene procedurally, containing a single cube with a material, shader
        // and texture bound. It then inspects the cube to discover the material. A Unity material is
        // constructed and the parameters are copied in a generic way. Similarly, the texture is
        // discovered and loaded as a Unity Texture2D and bound to the material.
        //
        // Also See: https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
        //
        void Start()
        {
            // Get the current local path
            m_localPath = PackageUtils.GetCallerRelativeToProjectFolderPath();

            // Create a scene for this test, but could also be read from disk.
            Scene usdScene = CreateSceneWithShading();

            // Read the material and shader ID.
            var usdMaterial = new MaterialSample();
            string shaderId;

            // ReadMaterial was designed for Unity and assumes there is one "surface" shader bound.
            if (!MaterialSample.ReadMaterial(usdScene, kCubePath, usdMaterial, out shaderId))
            {
                throw new System.Exception("Failed to read material");
            }

            // Map the shader ID to the corresponding Unity/USD shader pair.
            ShaderPair shader;
            if (shaderId == null || !m_shaderMap.TryGetValue(shaderId, out shader))
            {
                throw new System.Exception("Material had no surface bound");
            }

            //
            // Read and process the shader-specific parameters.
            //

            // UsdShade requires all connections target an attribute, but we actually want to deserialize
            // the entire prim, so we get just the prim path here.
            var shaderPath = new pxr.SdfPath(usdMaterial.surface.connectedPath).GetPrimPath();
            usdScene.Read(shaderPath, shader.usdShader);

            //
            // Construct material & process the inputs, textures, and keywords.
            //

            var mat = new UnityEngine.Material(shader.unityShader);

            // Apply material keywords.
            foreach (string keyword in usdMaterial.requiredKeywords ?? new string[0])
            {
                mat.EnableKeyword(keyword);
            }

            // Iterate over all input parameters and copy values and/or construct textures.
            foreach (var param in shader.usdShader.GetInputParameters())
            {
                if (!SetMaterialParameter(mat, param.unityName, param.value))
                {
                    throw new System.Exception("Incompatible shader data type: " + param.ToString());
                }
            }

            foreach (var param in shader.usdShader.GetInputTextures())
            {
                if (string.IsNullOrEmpty(param.connectedPath))
                {
                    // Not connected to a texture.
                    continue;
                }

                // Only 2D textures are supported in this example.
                var usdTexture = new Texture2DSample();

                // Again, we want the prim path, not the attribute path.
                var texturePath = new pxr.SdfPath(param.connectedPath).GetPrimPath();
                usdScene.Read(texturePath, usdTexture);

                // This example also only supports explicit sourceFiles, they cannot be connected.
                if (string.IsNullOrEmpty(usdTexture.sourceFile.defaultValue))
                {
                    continue;
                }

                // For details, see: https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
                foreach (string keyword in param.requiredShaderKeywords)
                {
                    mat.EnableKeyword(keyword);
                }

                var data = System.IO.File.ReadAllBytes(usdTexture.sourceFile.defaultValue);
                var unityTex = new Texture2D(2, 2);
                unityTex.LoadImage(data);
                mat.SetTexture(param.unityName, unityTex);
                Debug.Log("Set " + param.unityName + " to " + usdTexture.sourceFile.defaultValue);

                unityTex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
            }

            //
            // Create and bind the geometry.
            //

            // Create a cube and set the material.
            // Note that geometry is handled minimally here and is incomplete.
            var cubeSample = new CubeSample();
            usdScene.Read(kCubePath, cubeSample);

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(transform, worldPositionStays: false);
            go.transform.localScale = Vector3.one * (float)cubeSample.size;
            m_cube = transform;

            go.GetComponent<MeshRenderer>().material = mat;
        }

        // ------------------------------------------------------------------------------------------ //
        // Unity parameter value helper.
        // ------------------------------------------------------------------------------------------ //

        private bool SetMaterialParameter(Material mat, string unityParamName, object value)
        {
            if (value.GetType() == typeof(Color))
            {
                mat.SetColor(unityParamName, (Color)value);
            }
            else if (value.GetType() == typeof(Color[]))
            {
                mat.SetColorArray(unityParamName, (Color[])value);
            }
            else if (value.GetType() == typeof(List<Color>))
            {
                mat.SetColorArray(unityParamName, (List<Color>)value);
            }
            else if (value.GetType() == typeof(bool))
            {
                mat.SetFloat(unityParamName, ((bool)value) ? 1f : 0f);
            }
            else if (value.GetType() == typeof(float))
            {
                mat.SetFloat(unityParamName, (float)value);
            }
            else if (value.GetType() == typeof(float[]))
            {
                mat.SetFloatArray(unityParamName, (float[])value);
            }
            else if (value.GetType() == typeof(List<float>))
            {
                mat.SetFloatArray(unityParamName, (List<float>)value);
            }
            else if (value.GetType() == typeof(int))
            {
                mat.SetInt(unityParamName, (int)value);
            }
            else if (value.GetType() == typeof(Matrix4x4))
            {
                mat.SetMatrix(unityParamName, (Matrix4x4)value);
            }
            else if (value.GetType() == typeof(Matrix4x4[]))
            {
                mat.SetMatrixArray(unityParamName, (Matrix4x4[])value);
            }
            else if (value.GetType() == typeof(List<Matrix4x4>))
            {
                mat.SetMatrixArray(unityParamName, (List<Matrix4x4>)value);
            }
            else if (value.GetType() == typeof(Vector2))
            {
                mat.SetVector(unityParamName, (Vector2)value);
            }
            else if (value.GetType() == typeof(Vector3))
            {
                mat.SetVector(unityParamName, (Vector3)value);
            }
            else if (value.GetType() == typeof(Vector4))
            {
                mat.SetVector(unityParamName, (Vector4)value);
            }
            else if (value.GetType() == typeof(Vector4[]))
            {
                mat.SetVectorArray(unityParamName, (Vector4[])value);
            }
            else if (value.GetType() == typeof(List<Vector4>))
            {
                mat.SetVectorArray(unityParamName, (List<Vector4>)value);
            }
            else
            {
                return false;
            }

            Debug.Log("Set " + unityParamName + " = " + value);
            return true;
        }

        // ------------------------------------------------------------------------------------------ //
        // Create a USD scene for testing
        // ------------------------------------------------------------------------------------------ //
        static private Scene CreateSceneWithShading()
        {
            var scene = Scene.Create();

            var cubePath = kCubePath;
            var materialPath = "/Model/Materials/SimpleMat";
            var shaderPath = "/Model/Materials/SimpleMat/StandardShader";

            var albedoMapPath = "/Model/Materials/SimpleMat/AlbedoMap";
            var normalMapPath = "/Model/Materials/SimpleMat/NormalMap";
            var emissionMapPath = "/Model/Materials/SimpleMat/EmissionMap";
            var metallicMapPath = "/Model/Materials/SimpleMat/MetallicMap";
            var occlusionMapPath = "/Model/Materials/SimpleMat/OcclusionMap";
            var parallaxMapPath = "/Model/Materials/SimpleMat/ParallaxMap";
            var detailNormalMapPath = "/Model/Materials/SimpleMat/DetailNormalMap";
            var detailMaskPath = "/Model/Materials/SimpleMat/DetailMask";

            var textureFilePath = Path.Combine(m_localPath, "Textures");

            var cube = new CubeSample();
            cube.size = 7;
            cube.transform = Matrix4x4.identity;

            //
            // Setup Material.
            //
            var material = new MaterialSample();
            material.surface.SetConnectedPath(shaderPath, "outputs:out");

            // Various shader keywords are required to enable the standard shader to work as intended,
            // while these can be encoded as part of the schema, often they require some logic (e.g. is
            // emission color != black?).
            material.requiredKeywords = new string[] { "_EMISSION" };

            //
            // Setup Shader.
            //
            var shader = new StandardShaderSample();
            shader.id = new pxr.TfToken("Unity.Standard");

            // Note that USD requires all connections target attributes, hence "outputs:out" appended to
            // the paths below. Think of this as a shader network graph, where a texture object could
            // be executed per pixel with a sampler,  "outputs:out" would be the sampled color.

            shader.smoothnessScale.defaultValue = 0.99f;

            shader.enableSpecularHighlights.defaultValue = true;
            shader.enableGlossyReflections.defaultValue = true;

            shader.albedo.defaultValue = Color.white;
            shader.albedoMap.SetConnectedPath(albedoMapPath, "outputs:out");

            shader.normalMapScale.defaultValue = 0.76f;
            shader.normalMap.SetConnectedPath(normalMapPath, "outputs:out");

            shader.emission.defaultValue = Color.white * 0.3f;
            shader.emissionMap.SetConnectedPath(emissionMapPath, "outputs:out");

            shader.metallicMap.SetConnectedPath(metallicMapPath, "outputs:out");

            shader.occlusionMapScale.defaultValue = 0.65f;
            shader.occlusionMap.SetConnectedPath(occlusionMapPath, "outputs:out");

            shader.parallaxMapScale.defaultValue = 0.1f;
            shader.parallaxMap.SetConnectedPath(parallaxMapPath, "outputs:out");

            shader.detailMask.SetConnectedPath(detailMaskPath);
            shader.detailNormalMapScale.defaultValue = .05f;
            shader.detailNormalMap.SetConnectedPath(detailNormalMapPath, "outputs:out");

            //
            // Setup Textures.
            //
            var albedoTexture = new Texture2DSample();
            albedoTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "albedoMap.png");
            albedoTexture.sRgb = true;

            var normalTexture = new Texture2DSample();
            normalTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "normalMap.png");
            normalTexture.sRgb = true;

            var emissionTexture = new Texture2DSample();
            emissionTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "emissionMap.png");
            emissionTexture.sRgb = true;

            var metallicTexture = new Texture2DSample();
            metallicTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "metallicMap.png");
            metallicTexture.sRgb = true;

            var occlusionTexture = new Texture2DSample();
            occlusionTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "occlusionMap.png");
            occlusionTexture.sRgb = true;

            var parallaxTexture = new Texture2DSample();
            parallaxTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "parallaxMap.png");
            parallaxTexture.sRgb = true;

            var detailNormalTexture = new Texture2DSample();
            detailNormalTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "detailMap.png");
            detailNormalTexture.sRgb = true;

            var detailMaskTexture = new Texture2DSample();
            detailMaskTexture.sourceFile.defaultValue = Path.Combine(textureFilePath, "metallicMap.png");
            detailMaskTexture.sRgb = true;

            //
            // Write Everything.
            //
            scene.Write(cubePath, cube);
            scene.Write(materialPath, material);
            scene.Write(shaderPath, shader);

            scene.Write(albedoMapPath, albedoTexture);
            scene.Write(normalMapPath, normalTexture);
            scene.Write(emissionMapPath, emissionTexture);
            scene.Write(metallicMapPath, metallicTexture);
            scene.Write(occlusionMapPath, occlusionTexture);
            scene.Write(parallaxMapPath, parallaxTexture);
            scene.Write(detailNormalMapPath, detailNormalTexture);
            scene.Write(detailMaskPath, detailMaskTexture);

            //
            // Bind Material.
            //
            MaterialSample.Bind(scene, cubePath, materialPath);

            //
            // Write out the scene as a string, for debugging.
            //
            string s;
            scene.Stage.ExportToString(out s);
            Debug.Log("Loading:\n" + s);
            return scene;
        }
    }
}

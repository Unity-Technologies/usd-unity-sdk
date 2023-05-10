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

using pxr;
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

namespace Unity.Formats.USD
{
    public class ShaderExporterBase
    {
        public enum ConversionType
        {
            None,
            SwapRASmoothnessToBGRoughness,
            InvertAlpha,
            UnpackNormal,
            MaskMapToORM
        }

        static Material _metalGlossChannelSwapMaterial = null;
        static Material _normalChannelMaterial = null;

        /// <summary>
        /// Exports the given texture to the destination texture path and wires up the preview surface.
        /// </summary>
        /// <returns>
        /// Returns the path to the USD texture object.
        /// </returns>
        protected static string SetupTexture(Scene scene,
            string usdShaderPath,
            Material material,
            PreviewSurfaceSample surface,
            Vector4 scale,
            string destTexturePath,
            string textureName,
            string textureOutput,
            ConversionType conversionType = ConversionType.None)
        {
            // We have to handle multiple cases here:
            // - file exists on disk
            //   - file is a supported format => can be directly copied
            //   - file is not in a supported format => need to blit / export
            // - file is only in memory
            //   - a Texture2D
            //   - a Texture
            //   - a RenderTexture
            //   - needs special care if marked as Normal Map
            //     (can probably only be detected in an Editor context, and heuristically at runtime)
            //   => need to blit / export
            // - file is not supported at all (or not yet)
            //   - a 3D texture
            //   => needs to be ignored, log Warning

            bool textureIsExported = false;

            string filePath = null;
            string fileName = null;

            var srcTexture2d = material.GetTexture(textureName);

            bool needsConversion = false;
            switch (conversionType)
            {
                case ConversionType.None:
                    break;
                case ConversionType.UnpackNormal:
#if UNITY_EDITOR
                    if (UnityEditor.AssetDatabase.Contains(srcTexture2d))
                    {
                        // normal needs to be converted if the one on disk isn't really a normal map
                        // (e.g. created from greyscale)
                        UnityEditor.TextureImporter importer =
                            (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(
                                UnityEditor.AssetDatabase.GetAssetPath(srcTexture2d));
                        if (importer.textureType != UnityEditor.TextureImporterType.NormalMap)
                        {
                            Debug.LogWarning("Texture " + textureName + " is set as NormalMap but isn't marked as such",
                                srcTexture2d);
                        }

                        UnityEditor.TextureImporterSettings dst = new UnityEditor.TextureImporterSettings();
                        importer.ReadTextureSettings(dst);
                        // if this NormalMap is created from greyscale we will export the NormalMap from memory.
                        if (dst.convertToNormalMap)
                        {
                            needsConversion = true;
                            break;
                        }
                    }
#endif
                    break;
                default:
                    needsConversion = true;
                    break;
            }

#if UNITY_EDITOR
            // only export from disk if there's no need to do any type of data conversion here
            if (!needsConversion)
            {
                var srcPath = UnityEditor.AssetDatabase.GetAssetPath(srcTexture2d);

                if (!string.IsNullOrEmpty(srcPath))
                {
#if UNITY_2019_2_OR_GREATER
                    // Since textures might be inside of packages for various reasons we should support that.
                    // Usually this would just be "Path.GetFullPath(srcPath)", but USD export messes with the CWD (Working Directory)
                    // and so we have to do a bit more path wrangling here.
                    if (srcPath.StartsWith("Packages"))
                    {
                        var pi = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(srcPath);
                        srcPath = pi.resolvedPath + srcPath.Substring(("Packages/" + pi.name).Length);
                    }
#endif
                    if (srcPath.StartsWith("Assets"))
                    {
                        srcPath = Application.dataPath + "/" + srcPath.Substring("Assets/".Length);
                    }

                    fileName = System.IO.Path.GetFileName(srcPath);
                    filePath = System.IO.Path.Combine(destTexturePath, fileName);

                    if (System.IO.File.Exists(srcPath))
                    {
                        // USDZ officially only supports png / jpg / jpeg
                        // https://graphics.pixar.com/usd/docs/Usdz-File-Format-Specification.html

                        var ext = System.IO.Path.GetExtension(srcPath).ToLowerInvariant();
                        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                        {
                            System.IO.File.Copy(srcPath, filePath, overwrite: true);
                            if (System.IO.File.Exists(filePath))
                            {
                                textureIsExported = true;
                            }
                        }
                    }
                }
            }
#endif

            if (!textureIsExported)
            {
                // Since this is a texture we can't directly export from disk, we need to blit it and output it as PNG.
                // To avoid collisions, e.g. with multiple different textures named the same, each texture gets a pseudo-random name.
                // This will also avoid collisions when exporting multiple models to the same folder, e.g. with a a RenderTexture called "RT"
                // in each of them that might look different between exports.
                // TODO Future work could, if necessary, generate a texture content hash to avoid exporting identical textures multiple times
                // (Unity's content hash isn't reliable for some types of textures unfortunately, e.g. RTs)
#if UNITY_EDITOR
                if (srcTexture2d is Texture2D)
                    fileName = srcTexture2d.name + "_" + srcTexture2d.imageContentsHash.ToString();
                else
                    fileName = srcTexture2d.name + "_" + Random.Range(10000000, 99999999).ToString();
#else
                fileName = srcTexture2d.name + "_" + Random.Range(10000000, 99999999).ToString();
#endif
                filePath = System.IO.Path.Combine(destTexturePath, fileName + ".png");

                // TODO extra care has to be taken of Normal Maps etc., since these are in a converted format in memory (for example 16 bit AG instead of 8 bit RGBA, depending on platform)
                // An example of this conversion in a shader is in Khronos' UnityGLTF implementation.
                // Basically, the blit has do be done with the right unlit conversion shader to get a proper "file-based" tangent space normal map back

                // Blit the texture and get it back to CPU
                // Note: Can't use RenderTexture.GetTemporary because that doesn't properly clear alpha channel
                bool preserveLinear = false;
                switch (conversionType)
                {
                    case ConversionType.UnpackNormal:
                        preserveLinear = true;
                        break;
                }

                var rt = new RenderTexture(srcTexture2d.width, srcTexture2d.height, 0, RenderTextureFormat.ARGB32,
                    preserveLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default);
                var resultTex2d = new Texture2D(srcTexture2d.width, srcTexture2d.height, TextureFormat.ARGB32, true,
                    preserveLinear ? true : false);
                var activeRT = RenderTexture.active;
                try
                {
                    RenderTexture.active = rt;
                    GL.Clear(true, true, Color.clear);

                    // conversion material
                    if (_metalGlossChannelSwapMaterial == null)
                    {
                        _metalGlossChannelSwapMaterial = new Material(Shader.Find("Hidden/USD/ChannelCombiner"));
                    }

                    if (_normalChannelMaterial == null)
                    {
                        _normalChannelMaterial = new Material(Shader.Find("Hidden/USD/NormalChannel"));
                    }

                    _metalGlossChannelSwapMaterial.SetTexture("_R", srcTexture2d);
                    _metalGlossChannelSwapMaterial.SetTexture("_G", srcTexture2d);
                    _metalGlossChannelSwapMaterial.SetTexture("_B", srcTexture2d);
                    _metalGlossChannelSwapMaterial.SetTexture("_A", srcTexture2d);

                    switch (conversionType)
                    {
                        case ConversionType.None:
                            Graphics.Blit(srcTexture2d, rt);
                            break;
                        case ConversionType.SwapRASmoothnessToBGRoughness:
                            _metalGlossChannelSwapMaterial.SetVector("_Invert",
                                new Vector4(0, 1, 0, 1)); // invert resulting g channel, make sure alpha is 1
                            _metalGlossChannelSwapMaterial.SetVector("_RScale", new Vector4(0, 0, 0, 0));
                            _metalGlossChannelSwapMaterial.SetVector("_GScale",
                                new Vector4(0, 0, 0, 1)); // use a channel from _G texture for resulting g
                            _metalGlossChannelSwapMaterial.SetVector("_BScale",
                                new Vector4(1, 0, 0, 0)); // use r channel from _B texture for resulting b
                            _metalGlossChannelSwapMaterial.SetVector("_AScale", new Vector4(0, 0, 0, 0));

                            Graphics.Blit(srcTexture2d, rt, _metalGlossChannelSwapMaterial);
                            break;
                        case ConversionType.InvertAlpha:
                            _metalGlossChannelSwapMaterial.SetVector("_Invert",
                                new Vector4(0, 0, 0, 1)); // invert alpha result
                            _metalGlossChannelSwapMaterial.SetVector("_RScale",
                                new Vector4(1, 0, 0, 0)); // use all color channels as-is
                            _metalGlossChannelSwapMaterial.SetVector("_GScale", new Vector4(0, 1, 0, 0));
                            _metalGlossChannelSwapMaterial.SetVector("_BScale", new Vector4(0, 0, 1, 0));
                            _metalGlossChannelSwapMaterial.SetVector("_AScale", new Vector4(0, 0, 0, 1));

                            Graphics.Blit(srcTexture2d, rt, _metalGlossChannelSwapMaterial);
                            break;
                        case ConversionType.MaskMapToORM:
                            // Input is RGBA (Metallic, Occlusion, Detail, Smoothness)
                            // Output is RGB1 (Occlusion, Roughness = 1 - Smoothness, Metallic, 1)
                            _metalGlossChannelSwapMaterial.SetVector("_Invert",
                                new Vector4(0, 1, 0, 1)); // smoothness to roughness, solid alpha
                            _metalGlossChannelSwapMaterial.SetVector("_RScale", new Vector4(0, 1, 0, 0));
                            _metalGlossChannelSwapMaterial.SetVector("_GScale", new Vector4(0, 0, 0, 1));
                            _metalGlossChannelSwapMaterial.SetVector("_BScale", new Vector4(1, 0, 0, 0));
                            _metalGlossChannelSwapMaterial.SetVector("_AScale", new Vector4(0, 0, 0, 0));

                            Graphics.Blit(srcTexture2d, rt, _metalGlossChannelSwapMaterial);
                            break;
                        case ConversionType.UnpackNormal:
                            Graphics.Blit(srcTexture2d, rt, _normalChannelMaterial);
                            break;
                    }

                    resultTex2d.ReadPixels(new Rect(0, 0, srcTexture2d.width, srcTexture2d.height), 0, 0);
                    resultTex2d.Apply();

                    System.IO.File.WriteAllBytes(filePath, resultTex2d.EncodeToPNG());
                    if (System.IO.File.Exists(filePath))
                    {
                        textureIsExported = true;
                    }
                }
                finally
                {
                    RenderTexture.active = activeRT;
                    rt.Release();
                    GameObject.DestroyImmediate(rt);
                    GameObject.DestroyImmediate(resultTex2d);
                }
            }

            if (!textureIsExported)
            {
                var tmpTex2d = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                try
                {
                    tmpTex2d.SetPixel(0, 0, Color.white);
                    tmpTex2d.Apply();
                    System.IO.File.WriteAllBytes(filePath, tmpTex2d.EncodeToPNG());
                    if (System.IO.File.Exists(filePath))
                    {
                        textureIsExported = true;
                    }
                }
                finally
                {
                    GameObject.DestroyImmediate(tmpTex2d);
                }
            }

            if (textureIsExported)
            {
                // Make file path baked into USD relative to scene file and use forward slashes.
                filePath = ImporterBase.MakeRelativePath(scene.FilePath, filePath);
                filePath = filePath.Replace("\\", "/");

                var uvReader = new PrimvarReaderExportSample<Vector2>();
                uvReader.varname.defaultValue = new TfToken("st");
                scene.Write(usdShaderPath + "/uvReader", uvReader);
                var usdTexReader = new TextureReaderSample(filePath, usdShaderPath + "/uvReader.outputs:result");
                usdTexReader.wrapS =
                    new Connectable<TextureReaderSample.WrapMode>(
                        TextureReaderSample.GetWrapMode(srcTexture2d.wrapModeU));
                usdTexReader.wrapT =
                    new Connectable<TextureReaderSample.WrapMode>(
                        TextureReaderSample.GetWrapMode(srcTexture2d.wrapModeV));
                if (scale != Vector4.one)
                {
                    usdTexReader.scale = new Connectable<Vector4>(scale);
                }

                // usdTexReader.isSRGB = new Connectable<TextureReaderSample.SRGBMode>(TextureReaderSample.SRGBMode.Auto);
                scene.Write(usdShaderPath + "/" + textureName, usdTexReader);
                return usdShaderPath + "/" + textureName + ".outputs:" + textureOutput;
            }
            else
            {
                Debug.LogError(
                    "Texture wasn't exported: " + srcTexture2d.name + " (" + textureName + " from material " + material,
                    srcTexture2d);
                return null;
            }
        }
    }
}

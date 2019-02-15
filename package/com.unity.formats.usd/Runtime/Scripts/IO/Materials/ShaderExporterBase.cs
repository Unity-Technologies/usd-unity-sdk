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

namespace USD.NET.Unity {

  public class ShaderExporterBase {

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
                                 string destTexturePath,
                                 string textureName,
                                 string textureOutput) {
#if UNITY_EDITOR
      var srcPath = UnityEditor.AssetDatabase.GetAssetPath(material.GetTexture(textureName));
      srcPath = srcPath.Substring("Assets/".Length);
      srcPath = Application.dataPath + "/" + srcPath;
      var fileName = System.IO.Path.GetFileName(srcPath);
      var filePath = System.IO.Path.Combine(destTexturePath, fileName);
      System.IO.File.Copy(srcPath, filePath, overwrite: true);
#else
      // Not supported at run-time, too many things can go wrong
      // (can't encode compressed textures, etc).
      throw new System.Exception("Not supported at run-time");
#endif
      var uvReader = new PrimvarReaderSample<Vector2>();
      uvReader.varname.defaultValue = new TfToken("st");
      scene.Write(usdShaderPath + "/uvReader", uvReader);
      var tex = new USD.NET.Unity.TextureReaderSample(filePath, usdShaderPath + "/uvReader.outputs:result");
      scene.Write(usdShaderPath + "/" + textureName, tex);
      return usdShaderPath + "/" + textureName + ".outputs:" + textureOutput;
    }

  }

}

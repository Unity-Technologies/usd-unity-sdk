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

using UnityEngine;

namespace USD.NET.Unity {
  /// <summary>
  /// The following is based on the Pixar specification found here:
  /// https://graphics.pixar.com/usd/docs/UsdPreviewSurface-Proposal.html
  /// </summary>
  [System.Serializable]
  [UsdSchema("Shader")]
  public class TextureReaderSample : ShaderSample {

    public TextureReaderSample() : base() {
      id = new pxr.TfToken("UsdUVTexture");
    }

    public TextureReaderSample(string filePath) : this() {
      file.defaultValue = new pxr.SdfAssetPath(filePath);
    }

    public TextureReaderSample(string filePath, string stConnectionPath) : this() {
      file.defaultValue = new pxr.SdfAssetPath(filePath);
      st.SetConnectedPath(stConnectionPath);
    }

    public enum WrapMode {
      Black,   // Black outside the unit square.
      Clamp,   // Extend edge alues outside the unit square.
      Repeat,  // Repeat texture outside the unit square.
      Mirror,  // Flip and repeat texture outside the unit square.
    }

    /// <summary>
    /// Path to the texture.  Following the 1.36 MaterialX spec, Mari UDIM substitution in file
    /// values uses the "UDIM" token, so for example in USD, we might see a value
    /// @textures/occlusion.UDIM.tex@
    /// </summary>
    [InputParameter("_File")]
    public Connectable<pxr.SdfAssetPath> file =
        new Connectable<pxr.SdfAssetPath>(new pxr.SdfAssetPath(""));

    /// <summary>
    /// Texture coordinate to use to fetch this texture.  This node defines a mathematical/cartesian
    /// mapping from st to uv to image space: the (0, 0) st coordinate maps to a (0, 0) uv
    /// coordinate that samples the lower-left-hand corner of the texture image, as viewed on a
    /// monitor, while the (1, 1) st coordinate maps to a (1, 1) uv coordinate that samples the
    /// upper-right-hand corner of the texture image, as viewed on a monitor.
    /// </summary>
    [InputParameter("_St")]
    public Connectable<Vector2> st = new Connectable<Vector2>(new Vector2(0, 0));

    /// <summary>
    /// Wrap mode when reading this texture.  Irrelevant for implementations that handle UDIMs
    /// inside the shader, when given a UDIM filename.
    /// </summary>
    [InputParameter("_WrapS")]
    public Connectable<WrapMode> wrapS = new Connectable<WrapMode>(WrapMode.Black);

    /// <summary>
    /// See WrapS.
    /// </summary>
    [InputParameter("_WrapT")]
    public Connectable<WrapMode> wrapT = new Connectable<WrapMode>(WrapMode.Black);

    /// <summary>
    /// Fallback value used when texture can not be read.
    /// </summary>
    [InputParameter("_Fallback")]
    public Connectable<Vector4> fallback = new Connectable<Vector4>(new Vector4(0, 0, 0, 1));

    /// <summary>
    /// Scale to be applied to all components of the texture. value * scale + bias
    /// </summary>
    [InputParameter("_Scale")]
    public Connectable<Vector4> scale = new Connectable<Vector4>(Vector4.one);

    /// <summary>
    /// Bias to be applied to all components of the texture. value * scale + bias 
    /// </summary>
    [InputParameter("_Bias")]
    public Connectable<Vector4> bias = new Connectable<Vector4>(Vector4.zero);

    // TODO(jcowles): outputs in the UsdPreviewSurface cannot have values assigned to them, however
    //                USD .NET has no way to stop this from happening for value types currently. 
    //                https://github.com/googlevr/usd-unity-sdk/issues/12

    /*
    /// <remarks>
    /// Outputs one or more values. If the texture is 8 bit per component [0, 255] values will first
    /// be converted to floating point [0, 1] and apply any transformations (bias, scale) indicated.
    /// Otherwise it will just apply any transformation (bias, scale).  If a single-channel texture
    /// is fed into a UsdUVTexture, the r, g, and b components of the rgb and rgba outputs will
    /// repeat the channel's value, while both the single a output and the a component of the rgba
    /// outputs will be set to 1.0.  If a two-channel texture is fed into a UsdUVTexture, the r, g,
    /// and b components of the rgb and rgba outputs will repeat the first channel's value, while
    /// both the single a output and the a component of the rgba outputs will be set to the second
    /// channel's value.
    /// </remarks>
    public class Outputs : SampleBase {
      public float r;
      public float g;
      public float b;
      public float a;
      public Vector3 rgb;
      public Vector4 rgba;
    }

    [UsdNamespace("outputs")]
    public Outputs outputs = new Outputs();
    */
  }

}

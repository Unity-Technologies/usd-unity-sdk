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
using System.Collections.Generic;
using System.Linq;
using pxr;

namespace USD.NET.Unity {
  public class UnityTypeConverter : IntrinsicTypeConverter {

    // ----------------------------------------------------------------------------------------- //
    // Paths
    // ----------------------------------------------------------------------------------------- //
    /// <summary>
    /// Returns a valid UsdPath for the given Unity GameObject.
    /// </summary>
    /// <remarks>
    /// Note that illegal characters are converted into legal characters, so invalid names may
    /// collide in the USD namespace.
    /// </remarks>
    static public string GetPath(UnityEngine.Transform unityObj) {
      // Base case.
      if (unityObj == null) {
        return "";
      }

      // Build the path from root to leaf.
      return GetPath(unityObj.transform.parent)
           + "/" + UnityTypeConverter.MakeValidIdentifier(unityObj.name);
    }

    // ----------------------------------------------------------------------------------------- //
    // Matrix4x4, Matrix4d
    // Note that Unity uses float matrices, but USD favors double-valued matrices.
    // ----------------------------------------------------------------------------------------- //
    static public UnityEngine.Matrix4x4 GetLocalToParentXf(UnityEngine.Transform unityXf) {
      return UnityEngine.Matrix4x4.TRS(unityXf.localPosition,
                                       unityXf.localRotation,
                                       unityXf.localScale);
    }

    static public GfMatrix4d ToGfMatrix(UnityEngine.Transform unityXf) {
      return ToGfMatrix(GetLocalToParentXf(unityXf));
    }

    static public GfMatrix4d ToGfMatrix(UnityEngine.Matrix4x4 unityMat4) {
      // Note that USD is row-major and Unity is column-major, the arguments below are pivoted
      // to account for this.
      return new GfMatrix4d(unityMat4[0, 0], unityMat4[1, 0], unityMat4[2, 0], unityMat4[3, 0],
                            unityMat4[0, 1], unityMat4[1, 1], unityMat4[2, 1], unityMat4[3, 1],
                            unityMat4[0, 2], unityMat4[1, 2], unityMat4[2, 2], unityMat4[3, 2],
                            unityMat4[0, 3], unityMat4[1, 3], unityMat4[2, 3], unityMat4[3, 3]);
    }
    
    static public UnityEngine.Matrix4x4 FromMatrix(GfMatrix4d gfMat) {
      // Note that USD is row-major and Unity is column-major, the arguments below are pivoted
      // to account for this.
      UnityEngine.Matrix4x4 ret = new UnityEngine.Matrix4x4();
      double[] tmp = UsdIo.ArrayAllocator.Malloc<double>(16);
      gfMat.CopyToArray(tmp);
      
      ret[0, 0] = (float)tmp[0];
      ret[1, 0] = (float)tmp[1];
      ret[2, 0] = (float)tmp[2];
      ret[3, 0] = (float)tmp[3];

      ret[0, 1] = (float)tmp[4+0];
      ret[1, 1] = (float)tmp[4+1];
      ret[2, 1] = (float)tmp[4+2];
      ret[3, 1] = (float)tmp[4+3];

      ret[0, 2] = (float)tmp[8 + 0];
      ret[1, 2] = (float)tmp[8 + 1];
      ret[2, 2] = (float)tmp[8 + 2];
      ret[3, 2] = (float)tmp[8 + 3];

      ret[0, 3] = (float)tmp[12 + 0];
      ret[1, 3] = (float)tmp[12 + 1];
      ret[2, 3] = (float)tmp[12 + 2];
      ret[3, 3] = (float)tmp[12 + 3];
      UsdIo.ArrayAllocator.Free(tmp.GetType(), (uint)tmp.Length, tmp);
      return ret;
    }

    // ----------------------------------------------------------------------------------------- //
    // Color32 direct conversion
    // ----------------------------------------------------------------------------------------- //
    static public GfVec4f Color32ToVec4f(UnityEngine.Color32 c) {
      return ColorToVec4f(c);
    }

    static public UnityEngine.Color32 Vec4fToColor32(GfVec4f v) {
      return Vec4fToColor(v);
    }

    static public VtVec4fArray ToVtArray(List<UnityEngine.Color32> input) {
      return ToVtArray(input.ToArray());
    }

    static public void ToVtArray(List<UnityEngine.Color32> input,
                                 out VtVec3fArray rgb,
                                 out VtFloatArray alpha) {
      ToVtArray(input.ToArray(), out rgb, out alpha);
    }

    static public VtVec4fArray ToVtArray(UnityEngine.Color32[] input) {
      var color = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>((uint)input.Length);
      // PERFORMANCE: this sucks, should implement conversion in C++.
      for (int i = 0; i < input.Length; i++) {
        color[i] = input[i];
      }
      return ToVtArray(color);
    }

    static public UnityEngine.Color32[] Color32FromVtArray(VtVec4fArray input) {
      UnityEngine.Color32[] ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color32>(input.size());
      UnityEngine.Vector4[] rgb = FromVtArray(input);

      for (int i = 0; i < ret.Length; i++) {
        ret[i] = new UnityEngine.Color(rgb[i][0], rgb[i][1], rgb[i][2], rgb[i][3]);
      }

      return ret;
    }

    // ----------------------------------------------------------------------------------------- //
    // Color direct conversion
    // ----------------------------------------------------------------------------------------- //
    static public GfVec4f ColorToVec4f(UnityEngine.Color c) {
      return new GfVec4f(c.r, c.g, c.b, c.a);
    }

    static public UnityEngine.Color Vec4fToColor(GfVec4f v) {
      return new UnityEngine.Color(v[0], v[1], v[2], v[3]);
    }

    static public VtVec4fArray ToVtArray(List<UnityEngine.Color> input) {
      return ToVtArray(input.ToArray());
    }

    static public void ToVtArray(List<UnityEngine.Color> input,
                                 out VtVec3fArray rgb,
                                 out VtFloatArray alpha) {
      ToVtArray(input.ToArray(), out rgb, out alpha);
    }

    static public VtVec4fArray ToVtArray(UnityEngine.Color[] input) {
      var output = new VtVec4fArray((uint)input.Length);
      unsafe
      {
        fixed (UnityEngine.Color* p = input) {
          output.CopyFromArray((IntPtr)p);
        }
      }
      return output;
    }

    static public UnityEngine.Color[] ColorFromVtArray(VtVec4fArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>(input.size());
      return ColorFromVtArray(input, ref output);
    }

    static public UnityEngine.Color[] ColorFromVtArray(VtVec4fArray input,
                                              ref UnityEngine.Color[] output) {
      if (output.Length != input.size()) {
        output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>(input.size());
      }
      unsafe
      {
        fixed (UnityEngine.Color* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
      return output;
    }


    // ----------------------------------------------------------------------------------------- //
    // Color32 / Vec3f,Float split conversion
    // ----------------------------------------------------------------------------------------- //
    static public void ToVtArray(UnityEngine.Color32[] input,
                                 out VtVec3fArray rgb,
                                 out VtFloatArray alpha) {
      // Unfortunate, but faster than using the USD bindings currently.
      var unityRgb = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)input.Length);
      float[] unityAlpha = UsdIo.ArrayAllocator.Malloc<float>((uint)input.Length); ;
      for (int i = 0; i < input.Length; i++) {
        unityAlpha[i] = input[i].a;
        unityRgb[i] = new UnityEngine.Vector3(input[i].r, input[i].g, input[i].b);
      }

      // Convert to Vt.
      rgb = ToVtArray(unityRgb);
      alpha = ToVtArray(unityAlpha);
    }

    /// Returns just the RGB elements of a Color array as a new Vector3 array.
    static public UnityEngine.Vector3[] ExtractRgb(UnityEngine.Color32[] colors) {
      var ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)colors.Length);
      for (int i = 0; i < colors.Length; i++) {
        ret[i] = new UnityEngine.Vector3(colors[i].r / 255f,
                                         colors[i].g / 255f,
                                         colors[i].b / 255f);
      }
      return ret;
    }

    /// Returns just the alpha component of a Color array as a new array of float.
    static public float[] ExtractAlpha(UnityEngine.Color32[] colors) {
      var ret = UsdIo.ArrayAllocator.Malloc<float>((uint)colors.Length);
      for (int i = 0; i < colors.Length; i++) {
        ret[i] = colors[i].a / 255f;
      }
      return ret;
    }

    // ----------------------------------------------------------------------------------------- //
    // Color / Vec3f,Float
    // ----------------------------------------------------------------------------------------- //
    static public void ToVtArray(UnityEngine.Color[] input,
                                 out VtVec3fArray rgb,
                                 out VtFloatArray alpha) {
      // Unfortunate, but faster than using the USD bindings currently.
      var unityRgb = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)input.Length);
      var unityAlpha = UsdIo.ArrayAllocator.Malloc<float>((uint)input.Length);
      for (int i = 0; i < input.Length; i++) {
        unityAlpha[i] = input[i].a;
        unityRgb[i] = new UnityEngine.Vector3(input[i].r, input[i].g, input[i].b);
      }

      // Convert to Vt.
      rgb = ToVtArray(unityRgb);
      alpha = ToVtArray(unityAlpha);
    }

    static public UnityEngine.Color[] FromVtArray(VtVec3fArray rgbIn, VtFloatArray alphaIn) {
      var ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Color>(rgbIn.size());
      float[] alpha = FromVtArray(alphaIn);
      UnityEngine.Vector3[] rgb = FromVtArray(rgbIn);

      for (int i = 0; i < rgbIn.size(); i++) {
        ret[i].r = rgb[i][0];
        ret[i].g = rgb[i][1];
        ret[i].b = rgb[i][2];
        ret[i].a = alpha[i];
      }

      return ret;
    }

    /// Returns just the RGB elements of a Color array as a new Vector3 array.
    static public UnityEngine.Vector3[] ExtractRgb(UnityEngine.Color[] colors) {
      var ret = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>((uint)colors.Length);
      for (int i = 0; i < colors.Length; i++) {
        ret[i] = new UnityEngine.Vector3(colors[i].r, colors[i].g, colors[i].b);
      }
      return ret;
    }

    /// Returns just the alpha component of a Color array as a new array of float.
    static public float[] ExtractAlpha(UnityEngine.Color[] colors) {
      var ret = UsdIo.ArrayAllocator.Malloc<float>((uint)colors.Length);
      for (int i = 0; i < colors.Length; i++) {
        ret[i] = colors[i].a;
      }
      return ret;
    }

    // ----------------------------------------------------------------------------------------- //
    // Quaternion
    // ----------------------------------------------------------------------------------------- //
    //
    // USD GfQuaternion is layed out in memory as [real] [i0, i1, i2]
    // Unity Quaternion is [i0, i1, i2] [real]
    // Swapping seems preferable to making a temp array copy.

    static private void SwapQuaternionReal(ref UnityEngine.Quaternion[] input) {
      float tmp;
      for (int i = 0; i < input.Length; i++) {
        tmp = input[i][0];
        input[i][0] = input[i][3];
        input[i][3] = tmp;
      }
    }

    static public GfQuatf QuaternionToQuatf(UnityEngine.Quaternion quaternion) {
      // See pxr/unity quaternion layout above.
      return new GfQuatf(quaternion.w, quaternion.x, quaternion.y, quaternion.z);
    }

    static public UnityEngine.Quaternion QuatfToQuaternion(GfQuatf quat) {
      // See pxr/unity quaternion layout above.
      GfVec3f img = quat.GetImaginary();
      return new UnityEngine.Quaternion(img[0], img[1], img[2], quat.GetReal());
    }

    static public VtQuatfArray ToVtArray(UnityEngine.Quaternion[] input) {
      SwapQuaternionReal(ref input);

      var output = new VtQuatfArray((uint)input.Length);
      unsafe
      {
        // Copy to USD/C++
        fixed (UnityEngine.Quaternion* p = input)
        {
          output.CopyFromArray((IntPtr)p);
        }
      }

      // Swap back
      SwapQuaternionReal(ref input);

      return output;
    }

    static public UnityEngine.Quaternion[] FromVtArray(VtQuatfArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Quaternion>(input.size());
      unsafe
      {
        fixed (UnityEngine.Quaternion* p = output)
        {
          input.CopyToArray((IntPtr)p);
        }
      }
      // Swap real component for USD/Unity mis-match.
      SwapQuaternionReal(ref output);
      return output;
    }

    static public VtQuatfArray ListToVtArray(List<UnityEngine.Quaternion> input) {
      return ToVtArray(input.ToArray());
    }

    static public List<UnityEngine.Quaternion> ListFromVtArray(VtQuatfArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Quaternion>(input.size());
      unsafe
      {
        fixed (UnityEngine.Quaternion* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
      // Swap real component for USD/Unity mis-match.
      SwapQuaternionReal(ref output);
      return output.ToList();
    }

    // ----------------------------------------------------------------------------------------- //
    // Vector4 / Vec4f
    // ----------------------------------------------------------------------------------------- //
    static public VtVec4fArray ToVtArray(UnityEngine.Vector4[] input) {
      var output = new VtVec4fArray((uint)input.Length);
      unsafe
      {
        fixed (UnityEngine.Vector4* p = input) {
          output.CopyFromArray((IntPtr)p);
        }
      }
      return output;
    }

    static public UnityEngine.Vector4[] FromVtArray(VtVec4fArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector4>(input.size());
      unsafe
      {
        fixed (UnityEngine.Vector4* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
      return output;
    }

    static public VtVec4fArray ListToVtArray(List<UnityEngine.Vector4> input) {
      return ToVtArray(input.ToArray());
    }

    static public List<UnityEngine.Vector4> ListFromVtArray(VtVec4fArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector4>(input.size());
      unsafe
      {
        fixed (UnityEngine.Vector4* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
      return output.ToList();
    }

    // ----------------------------------------------------------------------------------------- //
    // VtVec3f[] / Bounds
    // ----------------------------------------------------------------------------------------- //
    static public VtVec3fArray BoundsToVtArray(UnityEngine.Bounds input) {
      var vtArr = new VtVec3fArray(2);
      vtArr[0] = new GfVec3f(input.min[0], input.min[1], input.min[2]);
      vtArr[1] = new GfVec3f(input.max[0], input.max[1], input.max[2]);
      return vtArr;
    }

    static public UnityEngine.Bounds BoundsFromVtArray(VtValue vtBounds) {
      var bbox = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(2);
      var v3 = (VtVec3fArray)UsdIo.ArrayAllocator.MallocHandle(typeof(VtVec3fArray));
      UsdCs.VtValueToVtVec3fArray(vtBounds, v3);
      FromVtArray(v3, ref bbox);
      UsdIo.ArrayAllocator.FreeHandle(v3.GetType(), v3);
      UnityEngine.Bounds bnds = new UnityEngine.Bounds();
      bnds.min = bbox[0];
      bnds.max = bbox[1];
      UsdIo.ArrayAllocator.Free(bbox.GetType(), (uint)bbox.Length, bbox);
      return bnds;
    }

    static public UnityEngine.Bounds BoundsFromVtArray(VtValue vtBounds,
                                                       UnityEngine.Vector3[] bbox) {
      //UnityEngine.Vector3[] bbox = new UnityEngine.Vector3[2];
      System.Diagnostics.Debug.Assert(bbox.Length == 2);
      FromVtArray(vtBounds, ref bbox);
      UnityEngine.Bounds bnds = new UnityEngine.Bounds();
      bnds.min = bbox[0];
      bnds.max = bbox[1];
      return bnds;
    }

    // ----------------------------------------------------------------------------------------- //
    // Vector3 / Vec3f
    // ----------------------------------------------------------------------------------------- //
    static public VtVec3fArray ToVtArray(UnityEngine.Vector3[] input) {
      var output = new VtVec3fArray((uint)input.Length);
      unsafe
      {
        fixed (UnityEngine.Vector3* p = input) {
          output.CopyFromArray((IntPtr)p);
        }
      }
      return output;
    }

    // Convenience API: generates garbage, do not use when performance matters.
    static public UnityEngine.Vector3[] FromVtArray(VtVec3fArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(input.size()); ;
      FromVtArray(input, ref output);
      return output;
    }

    static public void FromVtArray(VtVec3fArray input, ref UnityEngine.Vector3[] output) {
      if (output.Length != input.size()) {
        output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(input.size());
      }
      unsafe
      {
        fixed (UnityEngine.Vector3* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
    }

    static public VtVec3fArray ListToVtArray(List<UnityEngine.Vector3> input) {
      return ToVtArray(input.ToArray());
    }

    static public List<UnityEngine.Vector3> ListFromVtArray(VtVec3fArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector3>(input.size());
      unsafe
      {
        fixed (UnityEngine.Vector3* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
      return output.ToList();
    }

    // ----------------------------------------------------------------------------------------- //
    // Vector2 / Vec2f
    // ----------------------------------------------------------------------------------------- //
    static public VtVec2fArray ToVtArray(UnityEngine.Vector2[] input) {
      var output = new VtVec2fArray((uint)input.Length);
      unsafe
      {
        fixed (UnityEngine.Vector2* p = input) {
          output.CopyFromArray((IntPtr)p);
        }
      }
      return output;
    }

    static public UnityEngine.Vector2[] FromVtArray(VtVec2fArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector2>(input.size());
      unsafe
      {
        fixed (UnityEngine.Vector2* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
      return output;
    }

    static public VtVec2fArray ListToVtArray(List<UnityEngine.Vector2> input) {
      return ToVtArray(input.ToArray());
    }

    static public List<UnityEngine.Vector2> ListFromVtArray(VtVec2fArray input) {
      var output = UsdIo.ArrayAllocator.Malloc<UnityEngine.Vector2>(input.size());
      unsafe
      {
        fixed (UnityEngine.Vector2* p = output) {
          input.CopyToArray((IntPtr)p);
        }
      }
      return output.ToList();
    }


    // ----------------------------------------------------------------------------------------- //
    // Vector2/Vec2
    // ----------------------------------------------------------------------------------------- //
    static public GfVec2f Vector2ToVec2f(UnityEngine.Vector2 vec2) {
      return new GfVec2f(vec2[0], vec2[1]);
    }

    static public UnityEngine.Vector2 Vec2fToVector2(GfVec2f value) {
      GfVec2f obj = pxr.UsdCs.VtValueToGfVec2f(value);
      return new UnityEngine.Vector2(obj[0], obj[1]);
    }

    // ----------------------------------------------------------------------------------------- //
    // Vector3/Vec3
    // ----------------------------------------------------------------------------------------- //
    static public GfVec3f Vector3ToVec3f(UnityEngine.Vector3 vec3) {
      return new pxr.GfVec3f(vec3[0], vec3[1], vec3[2]);
    }

    static public UnityEngine.Vector3 Vec3fToVector3(GfVec3f v3) {
      return new UnityEngine.Vector3(v3[0], v3[1], v3[2]);
    }

    // ----------------------------------------------------------------------------------------- //
    // Vector4/Vec4
    // ----------------------------------------------------------------------------------------- //
    static public pxr.GfVec4f Vector4ToVec4f(UnityEngine.Vector4 vector4) {
      return new pxr.GfVec4f(vector4[0], vector4[1], vector4[2], vector4[3]);
    }

    static public UnityEngine.Vector4 Vec4fToVector4(GfVec4f v4) {
      return new UnityEngine.Vector4(v4[0], v4[1], v4[2], v4[3]);
    }

    // ----------------------------------------------------------------------------------------- //
    // Rect
    // ----------------------------------------------------------------------------------------- //
    static public GfVec4f RectToVtVec4(UnityEngine.Rect rect) {
      return new pxr.GfVec4f(rect.x, rect.y, rect.width, rect.height);
    }

    static public UnityEngine.Rect Vec4fToRect(GfVec4f v4) {
      return new UnityEngine.Rect(v4[0], v4[1], v4[2], v4[3]);
    }
  }
}

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

using System;
using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// A collection of methods used for importing USD Xform data into Unity.
  /// </summary>
  public static class XformImporter {

    /// <summary>
    /// Copies the transform value from USD to Unity, optionally changing handedness in the
    /// process.
    /// </summary>
    public static void BuildXform(XformableSample usdXf,
                                  GameObject go,
                                  SceneImportOptions options) {
      if (options.changeHandedness == BasisTransformation.SlowAndSafe) {
        usdXf.ConvertTransform();
      }
      go.transform.localPosition = ExtractPosition(usdXf.transform);
      go.transform.localScale = ExtractScale(usdXf.transform);
      go.transform.localRotation = ExtractRotation(usdXf.transform);
    }

    /// <summary>
    /// Build the root of a scene under which more USD data will be imported. If the handedness
    /// is changed here, no subsequent changes are required below, however the root will contain
    /// a negative scale.
    /// </summary>
    public static void BuildSceneRoot(Scene scene, Transform root, SceneImportOptions options) {

      // Handle configurable up-axis (Y or Z).
      float invert = options.changeHandedness == BasisTransformation.FastAndDangerous ? -1 : 1;
      if (scene.UpAxis == Scene.UpAxes.Z) {
        root.transform.localRotation = Quaternion.AngleAxis(invert * 90, Vector3.right);
      }

      if (options.changeHandedness == BasisTransformation.FastAndDangerous) {
        // Convert from right-handed (USD) to left-handed (Unity).
        // The math below works out to either (1, -1, 1) or (1, 1, -1), depending on up.
        // TODO: this is not robust. The points should be converted instead.
        Vector3 up = GetUpVector(scene);
        if (scene.UpAxis == Scene.UpAxes.Z) {
          root.localScale = new Vector3(1, -1, 1);
        } else {
          root.localScale = new Vector3(1, 1, -1);
        }

        //root.localScale = -1 * up + -1 * (Vector3.one - up);
      }
    }

    // ------------------------------------------------------------------------------------------ //
    // Private helpers.
    // ------------------------------------------------------------------------------------------ //

    /// <summary>
    /// Returns the up vector for the USD scene, which may be Y or Z.
    /// </summary>
    static Vector3 GetUpVector(Scene scene) {
      // Note: currently Y and Z are the only valid values.
      // https://graphics.pixar.com/usd/docs/api/group___usd_geom_up_axis__group.html

      switch (scene.UpAxis) {
      case Scene.UpAxes.Y:
        // Note, this is also Unity's default up vector.
        return Vector3.up;
      case Scene.UpAxes.Z:
        return new Vector3(0, 0, 1);
      default:
        throw new Exception("Invalid upAxis value: " + scene.UpAxis.ToString());
      }
    }

    static Quaternion ExtractRotation(Matrix4x4 mat4) {
      Vector3 forward;
      forward.x = mat4.m02;
      forward.y = mat4.m12;
      forward.z = mat4.m22;
      Vector3 up;
      up.x = mat4.m01;
      up.y = mat4.m11;
      up.z = mat4.m21;
      return Quaternion.LookRotation(forward, up);
    }

    static Vector3 ExtractPosition(Matrix4x4 mat4) {
      Vector3 position;
      position.x = mat4.m03;
      position.y = mat4.m13;
      position.z = mat4.m23;
      return position;
    }

    static Vector3 ExtractScale(Matrix4x4 mat4) {
      Vector3 scale;
      scale.x = new Vector4(mat4.m00, mat4.m10, mat4.m20, mat4.m30).magnitude;
      scale.y = new Vector4(mat4.m01, mat4.m11, mat4.m21, mat4.m31).magnitude;
      scale.z = new Vector4(mat4.m02, mat4.m12, mat4.m22, mat4.m32).magnitude;
      return scale;
    }

  }
}

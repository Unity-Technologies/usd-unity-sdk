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

namespace USD.NET.Unity {


  [UsdSchema("Camera")]
  public class CameraSample : XformSample {

    public enum ProjectionType {
      Perspective,
      Orthographic,
    }

    public enum StereoRole {
      Mono,
      Left,
      Right,
    }

    public class Shutter : SampleBase {
      public double open;
      public double close;
    }

    // Core Camera parameters
    public ProjectionType projection;
    public float horizontalAperture;
    public float horizontalApertureOffset;
    public float verticalAperture;
    public float verticalApertureOffset;

    // Scale: 10ths of a world unit (e.g. if working in cm this value is in mm).
    public float focalLength;

    public UnityEngine.Vector2 clippingRange;
    public UnityEngine.Vector4[] clippingPlanes;

    // Depth of Field
    public float fStop;
    public float focusDistance;

    // Stereo
    [UsdVariability(Variability.Uniform)]
    public StereoRole stereoRole;

    // Motion Blur
    [UsdNamespace("shutter")]
    public Shutter shutter;
  }
}

// Copyright 2021 Unity Technologies. All rights reserved.
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

namespace USD.NET.Unity
{
    [System.Serializable]
    [UsdSchema("Camera")]
    public class CameraSample : XformSample
    {
        [System.Serializable]
        public enum ProjectionType
        {
            Perspective,
            Orthographic,
        }

        [System.Serializable]
        public enum StereoRole
        {
            Mono,
            Left,
            Right,
        }

        [System.Serializable]
        public class Shutter : SampleBase
        {
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

        public CameraSample()
        {
        }

        public CameraSample(UnityEngine.Camera fromCamera)
        {
            CopyFromCamera(fromCamera);
        }

        /// <summary>
        /// Copys all relevant values of the given camera to the current camera sample.
        /// </summary>
        /// <param name="camera">The camera to copy.</param>
        /// <param name="convertTransformToUsd">
        ///   If true, converts the transform matrix from left handed(Unity) to right handed(USD). If
        ///   you're unsure about this parameter, it should be left to the default value of true.
        /// </param>
        public void CopyFromCamera(UnityEngine.Camera camera, bool convertTransformToUsd = true)
        {
            // GfCamera is a gold mine of camera math.
            pxr.GfCamera c = new pxr.GfCamera();

            // Setup focalLength & apertures.
            if (camera.orthographic)
            {
                projection = ProjectionType.Orthographic;
                // USD Orthographic size is 2x the Unity size (Unity uses half aperture for ortho size).
                c.SetOrthographicFromAspectRatioAndSize(camera.aspect,
                    camera.orthographicSize * 2,
                    pxr.GfCamera.FOVDirection.FOVVertical);
            }
            else
            {
                projection = ProjectionType.Perspective;
                c.SetPerspectiveFromAspectRatioAndFieldOfView(camera.aspect,
                    camera.fieldOfView,
                    pxr.GfCamera.FOVDirection.FOVVertical);
            }

            clippingRange = new UnityEngine.Vector2(camera.nearClipPlane, camera.farClipPlane);
            focalLength = c.GetFocalLength();
            horizontalAperture = c.GetHorizontalAperture();
            verticalAperture = c.GetVerticalAperture();

            var tr = camera.transform;
            transform = UnityEngine.Matrix4x4.TRS(tr.localPosition,
                tr.localRotation,
                tr.localScale);
            if (convertTransformToUsd)
            {
                ConvertTransform();
            }
        }

        /// <summary>
        /// Copies the current sample values to the given camera.
        /// </summary>
        public void CopyToCamera(UnityEngine.Camera camera, bool setTransform)
        {
            // GfCamera is a gold mine of camera math.
            pxr.GfCamera c = new pxr.GfCamera(UnityTypeConverter.ToGfMatrix(transform),
                projection == ProjectionType.Perspective ? pxr.GfCamera.Projection.Perspective
                : pxr.GfCamera.Projection.Orthographic,
                this.horizontalAperture,
                this.verticalAperture,
                this.horizontalApertureOffset,
                this.verticalApertureOffset,
                this.focalLength);

            camera.orthographic = c.GetProjection() == pxr.GfCamera.Projection.Orthographic;
            camera.fieldOfView = c.GetFieldOfView(pxr.GfCamera.FOVDirection.FOVVertical);
            camera.aspect = c.GetAspectRatio();
            camera.nearClipPlane = clippingRange.x;
            camera.farClipPlane = clippingRange.y;

            if (camera.orthographic)
            {
                // Note that USD default scale is cm and aperture is in mm.
                // Also Unity ortho size is the half aperture, so divide USD by 2.
                camera.orthographicSize = (verticalAperture / 10.0f) / 2.0f;
            }

            if (setTransform)
            {
                var tr = camera.transform;
                var xf = transform;
                UnityTypeConverter.SetTransform(xf, tr);
            }
        }
    }
}

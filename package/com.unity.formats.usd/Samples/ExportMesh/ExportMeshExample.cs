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
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Examples
{
    /// <remarks>
    /// Export Mesh Example
    ///
    ///  * StartRecording:
    ///    * Create and configure a USD scene.
    ///    * Traverse the Unity scene, for each GameObject:
    ///      * Create an association between the Unity object and a USD prim.
    ///      * Assign an ExportFunction that will export the data for the object.
    ///
    ///  * Export unvarying data:
    ///    * Export mesh topology and any other data that doesn't change from frame-to-frame.
    ///
    ///  * On Update, export time-varying data:
    ///    * Traverse the map of GameObjects, for each object:
    ///      * Call the associated export function.
    ///
    ///  * StopRecording:
    ///    * Save and close the USD scene.
    ///    * Release the association map and USD scene.
    /// </remarks>
    public class ExportMeshExample : MonoBehaviour
    {
        [Serializable]
        public enum TimeCode
        {
            Frames,
            Seconds
        }

        [Serializable]
        public enum FrameRate
        {
            Fps6 = 6,
            Fps12 = 12,
            Fps15 = 15,
            Fps24 = 24,
            Fps30 = 30,
            Fps48 = 48,
            Fps60 = 60,
            Fps72 = 72,
            Fps75 = 75,
            Fps90 = 90,
        }

        // The root GameObject to export to USD.
        public GameObject m_exportRoot;
        public GameObject[] m_trackedRoots;

        public int m_curFrame;

        // How time is represened in USD.
        public TimeCode m_timeUnits = TimeCode.Frames;

        // The desired playback rate.
        public FrameRate m_frameRate = FrameRate.Fps60;

        // The number of frames to capture after hitting record;
        [Range(1, 500)] public int m_frameCount = 100;

        public bool m_exportMaterials = true;
        public BasisTransformation m_convertHandedness = BasisTransformation.SlowAndSafe;
        public ActiveExportPolicy m_activePolicy = ActiveExportPolicy.ExportAsVisibility;

        // The path to where the USD file will be written.
        // If null/empty, the file will be created in memory only.
        public string m_usdFile;

        // The scene object to which the recording will be saved.
        private Scene m_usdScene;

        // Recording start time in frames.
        private int m_startFrame;

        // Recording start time in seconds.
        private float m_startTime;

        ExportContext m_context = new ExportContext();

        // Used by the custom editor to determine recording state.
        public bool IsRecording { get; private set; }

        // ------------------------------------------------------------------------------------------ //
        // Recording Control.
        // ------------------------------------------------------------------------------------------ //

        public void StartRecording()
        {
            if (IsRecording)
            {
                return;
            }

            if (!m_exportRoot)
            {
                Debug.LogError("ExportRoot not assigned.");
                return;
            }

            if (m_usdScene != null)
            {
                m_usdScene.Close();
                m_usdScene = null;
            }

            try
            {
                if (string.IsNullOrEmpty(m_usdFile))
                {
                    m_usdScene = Scene.Create();
                }
                else
                {
                    m_usdScene = Scene.Create(m_usdFile);
                }

                // USD operates on frames, so the frame rate is required for playback.
                // We could also set this to 1 to indicate that the TimeCode is in seconds.
                Application.targetFrameRate = (int)m_frameRate;

                // This forces Unity to use a fixed time step, resulting in evenly spaced
                // time samples in USD. Unfortunately, non-integer frame rates are not supported.
                // When non-integer frame rates are needed, time can be manually paused and
                // and advanced
                Time.captureFramerate = Application.targetFrameRate;

                // Set the frame rate in USD  as well.
                //
                // This both set the "time samples per second" and the playback rate.
                // Setting times samples per second allows the authoring code to express samples as integer
                // values, avoiding floating point error; so by setting FrameRate = 60, the samples written
                // at time=0 through time=59 represent the first second of playback.
                //
                // Stage.TimeCodesPerSecond is set implicitly to 1 / FrameRate.
                m_usdScene.FrameRate = Application.targetFrameRate;

                // When authoring in terms of seconds, at any frame rate the samles written at
                // time = 0.0 through time = 1.0 represent the first second of playback. The framerate
                // above will only be used as a target frame rate.
                if (m_timeUnits == TimeCode.Seconds)
                {
                    m_usdScene.Stage.SetTimeCodesPerSecond(1);
                }

                m_usdScene.StartTime = 0;
                m_usdScene.EndTime = m_frameCount;

                // For simplicity in this example, adding game objects while recording is not supported.
                m_context = new ExportContext();
                m_context.scene = m_usdScene;
                m_context.basisTransform = m_convertHandedness;

                // Do this last, in case an exception is thrown above.
                IsRecording = true;

                // Set the start frame and add one because the button event fires after update, so the first
                // frame update sees while recording is (frameCount + 1).
                m_startFrame = Time.frameCount + 1;
                m_startTime = Time.timeSinceLevelLoad;
            }
            catch
            {
                if (m_usdScene != null)
                {
                    m_usdScene.Close();
                    m_usdScene = null;
                }

                throw;
            }
        }

        public void StopRecording()
        {
            if (!IsRecording)
            {
                return;
            }

            m_context = new ExportContext();

            // In a real exporter, additional error handling should be added here.
            if (!string.IsNullOrEmpty(m_usdFile))
            {
                // We could use SaveAs here, which is fine for small scenes, though it will require
                // everything to fit in memory and another step where that memory is copied to disk.
                m_usdScene.Save();
            }

            // Release memory associated with the scene.
            m_usdScene.Close();
            m_usdScene = null;

            IsRecording = false;
        }

        // ------------------------------------------------------------------------------------------ //
        // Unity Behavior Events.
        // ------------------------------------------------------------------------------------------ //

        void Awake()
        {
            // Init USD.
            InitUsd.Initialize();
        }

        // Why LateUpdate()?
        // Because Update fires before the animation system applies computed values.
        void LateUpdate()
        {
            if (!IsRecording)
            {
                return;
            }

            // On the first frame, export all the unvarying data (e.g. mesh topology).
            // On subsequent frames, skip unvarying data to avoid writing redundant data.
            if (Time.frameCount == m_startFrame)
            {
                // First write materials and unvarying values (mesh topology, etc).
                m_context.exportMaterials = m_exportMaterials;
                m_context.scene.Time = null;
                m_context.activePolicy = ActiveExportPolicy.ExportAsVisibility;

                SceneExporter.SyncExportContext(m_exportRoot, m_context);
                SceneExporter.Export(m_exportRoot, m_context, zeroRootTransform: true);
            }

            // Set the time at which to read samples from USD.
            // If the FramesPerSecond is set to 1 above, this should be Time.time instead of frame count.
            m_curFrame = Time.frameCount - m_startFrame;
            if (m_timeUnits == TimeCode.Frames)
            {
                m_context.scene.Time = m_curFrame;
            }
            else
            {
                m_context.scene.Time = Time.timeSinceLevelLoad - m_startTime;
            }

            m_context.exportMaterials = false;

            // Exit once we've recorded all frames.
            if (m_curFrame > m_frameCount)
            {
                StopRecording();
                return;
            }

            // Record the time varying data that changes from frame to frame.
            if (m_trackedRoots != null && m_trackedRoots.Length > 0)
            {
                foreach (var root in m_trackedRoots)
                {
                    SceneExporter.Export(root, m_context, zeroRootTransform: false);
                }
            }
            else
            {
                SceneExporter.Export(m_exportRoot, m_context, zeroRootTransform: true);
            }
        }

        public static void Export(GameObject root,
            Scene scene,
            BasisTransformation basisTransform)
        {
            SceneExporter.Export(root, scene, basisTransform,
                exportUnvarying: true, zeroRootTransform: false);
        }
    }
}

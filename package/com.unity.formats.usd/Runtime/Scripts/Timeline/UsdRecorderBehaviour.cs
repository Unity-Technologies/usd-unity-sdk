// Copyright 2019 Jeremy Cowles. All rights reserved.
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
using UnityEngine.Playables;
using USD.NET;
using System.IO;
using pxr;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Unity.Formats.USD
{
    public class UsdRecorderBehaviour : PlayableBehaviour
    {
        // Conversion to keyframes (60 frames per second) to work around QuickLook bug
        const int kExportFrameRate = 60;
        bool m_isPaused = false;
        public UsdRecorderClip Clip;
        string usdcFileName;
        string usdzFileName;
        string usdzFilePath;
        string currentDir;
        DirectoryInfo usdzTemporaryDir;
        GameObject _root;

        // ------------------------------------------------------------------------------------------ //
        // Recording Control.
        // ------------------------------------------------------------------------------------------ //

        public void BeginRecording(double currentTime, GameObject root)
        {
            InitUsd.Initialize();
            _root = root;

            if (!root)
            {
                Debug.LogError("ExportRoot not assigned.");
                return;
            }

            if (Clip.UsdScene != null)
            {
                Clip.UsdScene.Close();
                Clip.UsdScene = null;
            }

            // Keep the current directory to restore it at the end.
            currentDir = Directory.GetCurrentDirectory();
            var localScale = root.transform.localScale;
            try
            {
                if (string.IsNullOrEmpty(Clip.m_usdFile))
                {
                    Clip.UsdScene = Scene.Create();
                }
                else if (Clip.IsUSDZ)
                {
                    // Setup a temporary directory to export the wanted USD file and zip it.
                    string tmpDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    usdzTemporaryDir = Directory.CreateDirectory(tmpDirPath);

                    // Get the usd file name to export and the usdz file name of the archive.
                    usdcFileName = Path.GetFileNameWithoutExtension(Clip.m_usdFile) + ".usdc";
                    usdzFileName = Path.GetFileName(Clip.m_usdFile);
                    var fi = new FileInfo(Clip.m_usdFile);
                    usdzFilePath = fi.FullName;

                    // Set the current working directory to the tmp directory to export with relative paths.
                    Directory.SetCurrentDirectory(tmpDirPath);

                    Clip.UsdScene = ExportHelpers.InitForSave(Path.Combine(tmpDirPath, usdcFileName));
                    // set the unit to centimeters for usdz
                    Clip.UsdScene.MetersPerUnit = 0.01;
                }
                else
                {
                    Clip.UsdScene = Scene.Create(Clip.m_usdFile);
                }

                // Set the frame rate in USD  as well.
                //
                // This both set the "time samples per second" and the playback rate.
                // Setting times samples per second allows the authoring code to express samples as integer
                // values, avoiding floating point error; so by setting FrameRate = 60, the samples written
                // at time=0 through time=59 represent the first second of playback.
                //
                // Stage.TimeCodesPerSecond is set implicitly to 1 / FrameRate.
                //m_usdScene.FrameRate = Clip.frame;

                // When authoring in terms of seconds, at any frame rate the samles written at
                // time = 0.0 through time = 1.0 represent the first second of playback. The framerate
                // above will only be used as a target frame rate.
                //if (m_timeUnits == TimeCode.Seconds) {
                //  m_usdScene.Stage.SetTimeCodesPerSecond(1);
                //}

                // Regardless of the actual sampling rate (e.g. Timeline playback speed), we are converting
                // the timecode from seconds to frames with a sampling rate of 60 FPS. This has the nice quality
                // of adding additional numerical stability.
                // In the event that the timeline is not configured for 60 FPS playback, we rely on USD's linear
                // interpolation mode to up-sample to 60 FPS.
                Clip.UsdScene.FrameRate = kExportFrameRate;
                Clip.UsdScene.Stage.SetInterpolationType(pxr.UsdInterpolationType.UsdInterpolationTypeLinear);

                // For simplicity in this example, adding game objects while recording is not supported.
                Clip.Context = new ExportContext();
                Clip.Context.scene = Clip.UsdScene;
                Clip.Context.basisTransform = Clip.m_convertHandedness;
                Clip.Context.activePolicy = Clip.m_activePolicy;
                Clip.Context.exportMaterials = Clip.m_exportMaterials;
                // USDZ is in centimeters.
                Clip.Context.scale = Clip.IsUSDZ ? 100.0f : 1.0f;

                Clip.UsdScene.StartTime = currentTime * kExportFrameRate;

                // Export the "default" frame, that is, all data which doesn't vary over time.
                Clip.UsdScene.Time = null;

                SceneExporter.SyncExportContext(root, Clip.Context);
                SceneExporter.Export(root,
                    Clip.Context,
                    zeroRootTransform: false);
            }
            catch
            {
                if (Clip.UsdScene != null)
                {
                    Debug.LogError("Set scene to null");
                    Clip.UsdScene.Close();
                    Clip.UsdScene = null;
                }

                if (Clip.IsUSDZ)
                {
                    usdzTemporaryDir.Delete(recursive: true);
                }

                throw;
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDir);
            }
        }

        public void StopRecording(double currentTime)
        {
            if (Clip.UsdScene == null)
            {
                // If an error occured, avoid spewing on every frame.
                return;
            }

            try
            {
                if (Clip.IsUSDZ && usdzTemporaryDir != null)
                    Directory.SetCurrentDirectory(usdzTemporaryDir.FullName);

                Clip.Context = new ExportContext();
                Clip.UsdScene.EndTime = currentTime * kExportFrameRate;
                // In a real exporter, additional error handling should be added here.
                if (!string.IsNullOrEmpty(Clip.m_usdFile))
                {
                    // We could use SaveAs here, which is fine for small scenes, though it will require
                    // everything to fit in memory and another step where that memory is copied to disk.
                    Clip.UsdScene.Save();
                }

                // Release memory associated with the scene.
                Clip.UsdScene.Close();
                Clip.UsdScene = null;
                if (Clip.IsUSDZ)
                {
                    SdfAssetPath assetPath = new SdfAssetPath(usdcFileName);
                    bool success = pxr.UsdCs.UsdUtilsCreateNewARKitUsdzPackage(assetPath, usdzFileName);

                    if (!success)
                    {
                        Debug.LogError("Couldn't export " + _root.name + " to the usdz file: " + usdzFilePath);
                        return;
                    }

                    // needed if we export into temp folder first
                    File.Copy(usdzFileName, usdzFilePath, overwrite: true);
                }
            }
            finally
            {
                // Clean up temp files.
                Directory.SetCurrentDirectory(currentDir);
                if (Clip.IsUSDZ && usdzTemporaryDir != null && usdzTemporaryDir.Exists)
                {
                    usdzTemporaryDir.Delete(recursive: true);
                }
            }
        }

        void ProcessRecording(double currentTime, GameObject root)
        {
            if (!root || m_isPaused)
            {
                return;
            }

            if (Clip.UsdScene == null)
            {
                Debug.LogError("Process: clip.scene is null");
            }

            if (Clip.Context.scene == null)
            {
                Debug.LogError("Process: context.scene is null");
            }

            Clip.UsdScene.Time = currentTime * kExportFrameRate;
            Clip.Context.exportMaterials = false;
            SceneExporter.Export(root, Clip.Context, zeroRootTransform: false);
        }

        bool IsPlaying()
        {
#if UNITY_EDITOR
            return EditorApplication.isPlaying;
#else
            return true;
#endif
        }

        // ------------------------------------------------------------------------------------------ //
        // Timeline Events.
        // ------------------------------------------------------------------------------------------ //

        public override void OnPlayableCreate(Playable playable)
        {
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if (!IsPlaying())
            {
                return;
            }

            StopRecording(playable.GetTime());
        }

        public override void OnGraphStart(Playable playable)
        {
            if (!IsPlaying())
            {
                return;
            }

            BeginRecording(playable.GetTime(), Clip.GetExportRoot(playable.GetGraph()));
        }

        public override void OnGraphStop(Playable playable)
        {
            if (!IsPlaying())
            {
                return;
            }

            StopRecording(playable.GetTime());
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!IsPlaying())
            {
                return;
            }

            var frameRate = Time.captureFramerate;
            if (frameRate < 1)
            {
                frameRate = Application.targetFrameRate;
            }

            UsdWaitForEndOfFrame.Add(() => OnFrameEnd(playable, info, playerData));
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            m_isPaused = false;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            m_isPaused = true;
        }

        public void OnFrameEnd(Playable playable, FrameData info, object playerData)
        {
            if (!playable.IsValid())
            {
                return;
            }

            ProcessRecording(playable.GetTime(), Clip.GetExportRoot(playable.GetGraph()));
        }
    }
}

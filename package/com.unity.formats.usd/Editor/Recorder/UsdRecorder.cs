#if RECORDER_AVAILABLE
using System;
using System.IO;
using pxr;
using Unity.Formats.USD;
using UnityEditor.Recorder;
using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    public class UsdRecorder : GenericRecorder<UsdRecorderSettings>
    {
        ExportContext context;
        UsdRecorderInput Input => m_Inputs[0] as UsdRecorderInput;

        // Stateful stuff used during the recording that should be cleaned
        DirectoryInfo usdzTemporaryDir;
        string usdcFileName;
        string usdzFileName;
        string usdzFilePath;
        string currentDir;
        protected override void SessionCreated(RecordingSession session)
        {
            base.SessionCreated(session);

            InitUsd.Initialize();
            var outputFile = Settings.FileNameGenerator.BuildAbsolutePath(session);
            if (Settings.ExportFormat == UsdRecorderSettings.Format.USDZ) // FIXME Support USDz
            {
                try
                {
                    currentDir = Directory.GetCurrentDirectory();
                    // Setup a temporary directory to export the wanted USD file and zip it.
                    var tmpDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    usdzTemporaryDir = Directory.CreateDirectory(tmpDirPath);

                    // Get the usd file name to export and the usdz file name of the archive.
                    usdcFileName = Path.GetFileNameWithoutExtension(outputFile) + ".usdc";
                    usdzFileName = Path.GetFileName(outputFile);
                    var fi = new FileInfo(outputFile);
                    usdzFilePath = fi.FullName;

                    // Set the current working directory to the tmp directory to export with relative paths.
                    Directory.SetCurrentDirectory(tmpDirPath);

                    outputFile = Path.Combine(tmpDirPath, usdcFileName);
                }
                finally
                {
                    Directory.SetCurrentDirectory(currentDir);
                }
            }

            try
            {
                context = new ExportContext
                {
                    scene = ExportHelpers.InitForSave(outputFile)
                };
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"The file is already open in Unity. Please close all references to it and try again: {outputFile}");
            }


            context.scene.FrameRate = Settings.FrameRate; // Variable framerate support ?
            context.scene.Stage.SetInterpolationType(Settings.InterpolationType); // User Option

            context.basisTransform = Settings.BasisTransformation;
            context.activePolicy = Settings.ActivePolicy;
            context.exportMaterials = Settings.ExportMaterials;

            context.scale = Settings.Scale;

            context.scene.StartTime = 0; // Absolute vs relative Time

            // Export the "default" frame, that is, all data which doesn't vary over time.
            context.scene.Time = null;

            Input.Context = context;
        }

        protected override void EndRecording(RecordingSession session)
        {
            context.scene.EndTime = session.recorderTime * session.settings.FrameRate;
            context.scene.Save();
            context.scene.Close();

            if (Settings.ExportFormat == UsdRecorderSettings.Format.USDZ)
            {
                try
                {
                    Directory.SetCurrentDirectory(usdzTemporaryDir.FullName);
                    var assetPath = new SdfAssetPath(usdcFileName);
                    var success = UsdCs.UsdUtilsCreateNewARKitUsdzPackage(assetPath, usdzFileName);

                    if (!success)
                    {
                        Debug.LogError("Couldn't export to the usdz file: " + usdzFilePath);
                        return;
                    }

                    // needed if we export into temp folder first
                    File.Copy(usdzFileName, usdzFilePath, overwrite: true);
                }
                finally
                {
                    // Clean up temp files.
                    Directory.SetCurrentDirectory(currentDir);
                    if (usdzTemporaryDir != null && usdzTemporaryDir.Exists)
                    {
                        usdzTemporaryDir.Delete(recursive: true);
                    }
                }
            }

            context = null;
            Input.Context = null;

            base.EndRecording(session);
        }

        protected override void RecordFrame(RecordingSession ctx) // Weird
        {
            //
        }
    }
}
#endif

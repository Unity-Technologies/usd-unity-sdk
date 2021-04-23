using System.IO;
using Unity.Formats.USD;
using UnityEditor.Recorder;
using USD.NET;

#if RECORDER_AVAILABLE
namespace UnityEditor.Formats.USD.Recorder
{
    public class UsdRecorder : GenericRecorder<UsdRecorderSettings>
    {
        ExportContext Context;
        UsdRecorderInput Input => m_Inputs[0] as UsdRecorderInput;

        protected override void SessionCreated(RecordingSession session)
        {
            base.SessionCreated(session);

            InitUsd.Initialize();
            if (Settings.ExportFormat == UsdRecorderSettings.Format.Usd) // FIXME Support USDz
            {
                Context = new ExportContext
                {
                    scene = Scene.Create(Settings.FileNameGenerator.BuildAbsolutePath(session))
                };
            }

            Context.scene.FrameRate = Settings.FrameRate; // Variable framerate support ?
            Context.scene.Stage.SetInterpolationType(pxr.UsdInterpolationType.UsdInterpolationTypeLinear); // User Option

            // FIXME User optionbs
            Context.basisTransform = BasisTransformation.SlowAndSafe;//Clip.m_convertHandedness;
            Context.activePolicy = ActiveExportPolicy.ExportAsActive;
            Context.exportMaterials = true;

            // USDZ is in centimeters.
            Context.scale = Settings.ExportFormat == UsdRecorderSettings.Format.UsdZ ? 100.0f : 1.0f;

            Context.scene.StartTime = 0; // Absolute vs relative Time

            // Export the "default" frame, that is, all data which doesn't vary over time.
            Context.scene.Time = null;

            Input.Context = Context;
        }

        protected override void EndRecording(RecordingSession session)
        {
            // Support USDz
            Context.scene.Save();
            Context.scene.Close();
            Context = null;
            Input.Context = null;
        }

        protected override void RecordFrame(RecordingSession ctx) // Weird
        {
            //
        }
    }
}
#endif

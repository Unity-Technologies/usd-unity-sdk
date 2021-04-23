using System.IO;
using Unity.Formats.USD;
using UnityEditor.Recorder;
using USD.NET;

#if RECORDER_AVAILABLE
namespace UnityEditor.Formats.USD.Recorder
{
    public class UsdRecorder : GenericRecorder<UsdRecorderSettings>
    {
        ExportContext context;
        UsdRecorderInput Input => m_Inputs[0] as UsdRecorderInput;

        protected override void SessionCreated(RecordingSession session)
        {
            base.SessionCreated(session);

            InitUsd.Initialize();
            if (Settings.ExportFormat == UsdRecorderSettings.Format.Usd) // FIXME Support USDz
            {
                context = new ExportContext
                {
                    scene = Scene.Create(Settings.FileNameGenerator.BuildAbsolutePath(session))
                };
            }

            context.scene.FrameRate = Settings.FrameRate; // Variable framerate support ?
            context.scene.Stage.SetInterpolationType(Settings.InterpolationType); // User Option

            // FIXME User optionbs
            context.basisTransform = Settings.BasisTransformation;
            context.activePolicy = Settings.ActivePolicy;
            context.exportMaterials = Settings.ExportMaterials;
            // Scale

            // USDZ is in centimeters.
            context.scale = Settings.Scale;

            context.scene.StartTime = 0; // Absolute vs relative Time

            // Export the "default" frame, that is, all data which doesn't vary over time.
            context.scene.Time = null;

            Input.Context = context;
        }

        protected override void EndRecording(RecordingSession session)
        {
            context.scene.EndTime = session.recorderTime * session.settings.FrameRate;
            // Support USDz
            context.scene.Save();
            context.scene.Close();
            context = null;
            Input.Context = null;
        }

        protected override void RecordFrame(RecordingSession ctx) // Weird
        {
            //
        }
    }
}
#endif

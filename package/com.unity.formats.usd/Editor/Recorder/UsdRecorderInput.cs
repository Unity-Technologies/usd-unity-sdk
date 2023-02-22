#if RECORDER_AVAILABLE
using Unity.Formats.USD;
using UnityEditor.Recorder;

namespace UnityEditor.Formats.USD.Recorder
{
    public class UsdRecorderInput : RecorderInput
    {
        internal ExportContext Context { get; set; }
        UsdRecorderInputSettings Settings => settings as UsdRecorderInputSettings;

        protected override void BeginRecording(RecordingSession session)
        {
            SceneExporter.SyncExportContext(Settings.GameObject, Context);
            SceneExporter.Export(Settings.GameObject,
                Context,
                zeroRootTransform: false);
        }

        protected override void NewFrameReady(RecordingSession session)
        {
            Context.scene.Time = session.recorderTime * session.settings.FrameRate;
            Context.exportMaterials = false;
            SceneExporter.Export(Settings.GameObject, Context, zeroRootTransform: false);
        }
    }
}

#endif

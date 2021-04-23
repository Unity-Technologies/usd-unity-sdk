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
            SceneExporter.SyncExportContext(Settings.gameObject, Context);
            SceneExporter.Export(Settings.gameObject,
                Context,
                zeroRootTransform: false);
        }

        protected override void NewFrameReady(RecordingSession session)
        {
            Context.scene.Time = session.recorderTime;
            Context.exportMaterials = false;
            SceneExporter.Export(Settings.gameObject, Context, zeroRootTransform: false);
        }
    }
}

#endif

#if RECORDER_AVAILABLE
using System.IO;
using Unity.Formats.USD;
using UnityEditor.Recorder;
using USD.NET;

namespace UnityEditor.Formats.USD.Recorder
{
    public class UsdRecorderInput : RecorderInput
    {
        internal ExportContext Context { get; set; }
        UsdRecorderInputSettings Settings => settings as UsdRecorderInputSettings;

        protected override void BeginRecording(RecordingSession session)
        {
            if (Context.exportTransformOverrides)
            {
                // Settings.
                UsdAsset usdAsset = Settings.GameObject.GetComponentInParent<UsdAsset>(); // Get the UsdAsset component in this GameObject or its nearest parent
                if (usdAsset != null)
                {
                    Context.scene.AddSubLayer(usdAsset.GetScene());
                    Context.scene.WriteMode = Scene.WriteModes.Over;
                }
                else
                    UnityEngine.Debug.LogError($"Unable to perform a 'transform overrides only' recording as <{Settings.GameObject.name}> is not a UsdAsset.");
            }
            SceneExporter.SyncExportContext(Settings.GameObject, Context);
            SceneExporter.Export(Settings.GameObject,
                Context,
                zeroRootTransform: false);

            if (Context.exportTransformOverrides)
            {
                // this is very brittle- if we have the chance of other sublayers in future we should store the index it was added at and only erase that one.
                Context.scene.Stage.GetRootLayer().GetSubLayerPaths().Erase(0);
            }
        }

        protected override void NewFrameReady(RecordingSession session)
        {
            Context.scene.Time = session.recorderTime * session.settings.FrameRate;
            Context.exportMaterials = false;
            SceneExporter.Export(Settings.GameObject, Context, zeroRootTransform: false);
        }

        protected override void EndRecording(RecordingSession session)
        {
            string extension = Path.GetExtension(Context.scene.FilePath);
            UsdEditorAnalytics.SendRecorderExportEvent(extension, true, Context.exportTransformOverrides, session.frameIndex); // VRC: Where to record failures?
        }
    }
}

#endif

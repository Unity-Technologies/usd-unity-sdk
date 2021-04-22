using UnityEditor.Recorder;

#if RECORDER_AVAILABLE
namespace UnityEditor.Formats.USD.Recorder
{
    public class UsdRecorder : GenericRecorder<UsdRecorderSettings>
    {
        protected override void RecordFrame(RecordingSession ctx)
        {
            //throw new System.NotImplementedException();
        }
    }
}
#endif

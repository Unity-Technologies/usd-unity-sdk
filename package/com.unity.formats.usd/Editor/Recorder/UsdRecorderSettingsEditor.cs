#if RECORDER_AVAILABLE
using UnityEditor.Recorder;

namespace UnityEditor.Formats.USD.Recorder
{
    [CustomEditor(typeof(UsdRecorderSettings))]
    public class UsdRecorderSettingsEditor : RecorderEditor
    {

    }
}
#endif

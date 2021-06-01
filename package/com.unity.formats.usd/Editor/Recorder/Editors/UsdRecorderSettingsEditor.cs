#if RECORDER_AVAILABLE
using UnityEditor.Recorder;

namespace UnityEditor.Formats.USD.Recorder
{
    [CustomEditor(typeof(UsdRecorderSettings))]
    public class UsdRecorderSettingsEditor : RecorderEditor
    {
        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exportFormat"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("interpolationType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coordinateConversion"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activePolicy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exportMaterials"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scale"));
        }
    }
}
#endif

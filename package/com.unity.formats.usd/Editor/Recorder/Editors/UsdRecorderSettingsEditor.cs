#if RECORDER_AVAILABLE
using UnityEditor.Recorder;
using UnityEngine;

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exportTransformOverrides"), new GUIContent("Override Setting"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exportMaterials"), new GUIContent("Materials"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scale"));
        }
    }
}
#endif

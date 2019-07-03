// using System;
// using UnityEngine;
// using UnityEditor.Experimental.AssetImporters;
// using UnityEngine.Formats.USD.Importer;


// namespace UnityEditor.Formats.USD.Importer
// {
//     [CustomEditor(typeof(USDImporter)), CanEditMultipleObjects]
//     internal class USDImporterEditor : ScriptedImporterEditor
//     {
//         public override void OnInspectorGUI()
//         {
//             Debug.Log("Coucou");
//             USDImporter importer = serializedObject.targetObject as USDImporter;
//             string settingsPath = "settings.";

//             EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);
//             {
//                 EditorGUI.indentLevel++;

//                 EditorGUILayout.PropertyField(serializedObject.FindProperty(settingsPath + "scaleFactor"),
//                     new GUIContent("Scale Factor", "The scale factor of the stage."));
//                 DisplayEnumProperty(serializedObject.FindProperty(settingsPath + "payloadPolicy"), 
//                     Enum.GetNames(typeof(PayloadPolicy)),
//                     new GUIContent("Payload Policy", "Whether to load or not payload location."));

//                 EditorGUILayout.Separator();

//                 EditorGUILayout.PropertyField(serializedObject.FindProperty(settingsPath + "importCamera"),
//                     new GUIContent("Import Camera", ""));
//                 EditorGUILayout.PropertyField(serializedObject.FindProperty(settingsPath + "importMesh"),
//                     new GUIContent("Import Mesh", ""));
//                 EditorGUILayout.PropertyField(serializedObject.FindProperty(settingsPath + "importSkinning"),
//                     new GUIContent("Import Skinning", ""));
//                 EditorGUILayout.PropertyField(serializedObject.FindProperty(settingsPath + "importTransform"),
//                     new GUIContent("Import Transform", ""));

//                 EditorGUILayout.Separator();

//                 EditorGUI.indentLevel--;
//             }

//             base.ApplyRevertGUI();
//         }

//         static void DisplayEnumProperty(SerializedProperty property, string[] displayNames, GUIContent guiContent)
//         {
//             Debug.Log("prop.intValue " + property.intValue);
//             Debug.Log("displayNames " + displayNames.ToString());

//             var rect = EditorGUILayout.GetControlRect();

//             EditorGUI.BeginProperty(rect, guiContent, property);
//             EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
//             EditorGUI.BeginChangeCheck();

//             GUIContent[] options = new GUIContent[displayNames.Length];
//             for (int i = 0; i < options.Length; ++i)
//             {
//                 options[i] = new GUIContent(ObjectNames.NicifyVariableName(displayNames[i]), "");
//             }

//             var normalsModeNew = EditorGUI.Popup(rect, guiContent, property.intValue, options);
//             if (EditorGUI.EndChangeCheck())
//             {
//                 property.intValue = normalsModeNew;
//             }

//             EditorGUI.showMixedValue = false;
//             EditorGUI.EndProperty();
//         }
//     }
// }

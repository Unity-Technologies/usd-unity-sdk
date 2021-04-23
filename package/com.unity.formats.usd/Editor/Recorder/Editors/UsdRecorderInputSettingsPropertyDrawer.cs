using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    [CustomPropertyDrawer(typeof(UsdRecorderInputSettings))]
    class UsdRecorderInputSettingsPropertyDrawer : TargetedPropertyDrawer<UsdRecorderInputSettings>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            //var target = (UsdRecorderInputSettings)property.objectReferenceValue;
            using (var changed = new EditorGUI.ChangeCheckScope())
            {
                var go = EditorGUI.ObjectField(position, label, target.gameObject, typeof(GameObject), true) as GameObject;
                if (changed.changed)
                {
                    target.gameObject = go;
                }
            }
        }
    }
}

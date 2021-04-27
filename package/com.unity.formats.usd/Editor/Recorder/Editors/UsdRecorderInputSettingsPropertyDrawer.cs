#if RECORDER_AVAILABLE
using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    [CustomPropertyDrawer(typeof(UsdRecorderInputSettings))]
    class UsdRecorderInputSettingsPropertyDrawer : TargetedPropertyDrawer<UsdRecorderInputSettings>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            using (var changed = new EditorGUI.ChangeCheckScope())
            {
                var go = EditorGUI.ObjectField(position, label, target.GameObject, typeof(GameObject), true) as GameObject;
                if (changed.changed)
                {
                    target.GameObject = go;
                }
            }
        }
    }
}
#endif

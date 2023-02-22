using System.Reflection;

namespace UnityEditor.Formats.USD.Recorder
{
    class TargetedPropertyDrawer<T> : PropertyDrawer where T : class
    {
        protected T target;

        protected virtual void Initialize(SerializedProperty prop)
        {
            if (target == null)
            {
                var path = prop.propertyPath.Split('.');
                object obj = prop.serializedObject.targetObject;

                foreach (var pathNode in path)
                    obj = GetSerializedField(obj, pathNode).GetValue(obj);

                target = obj as T;
            }
        }

        static FieldInfo GetSerializedField(object target, string pathNode)
        {
            return target.GetType().GetField(pathNode, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }
}

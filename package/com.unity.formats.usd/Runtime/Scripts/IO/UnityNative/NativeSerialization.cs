// Copyright 2018 Jeremy Cowles. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_EDITOR
using USD.NET;
using USD.NET.Unity;
using UnityEngine;
using UnityEditor;

namespace Unity.Formats.USD
{
    static public class NativeSerialization
    {
        /// <summary>
        /// Converts a serialized property to a string, e.g. for debugging.
        /// </summary>
        static public string ValueToString(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue.ToString();
                case SerializedPropertyType.ArraySize:
                    return prop.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return prop.boolValue.ToString();
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue.ToString();
                case SerializedPropertyType.BoundsInt:
                    return prop.boundsIntValue.ToString();
                case SerializedPropertyType.Character:
                    return prop.intValue.ToString();
                case SerializedPropertyType.Color:
                    return prop.colorValue.ToString();
                case SerializedPropertyType.Enum:
                    return prop.enumDisplayNames[prop.enumValueIndex];
                case SerializedPropertyType.ExposedReference:
                    return prop.exposedReferenceValue.ToString();
                case SerializedPropertyType.FixedBufferSize:
                    return prop.fixedBufferSize.ToString();
                case SerializedPropertyType.Float:
                    return prop.floatValue.ToString();
                case SerializedPropertyType.Generic:
                    return "GENERIC";
                case SerializedPropertyType.Gradient:
                    // TODO: gradientValue accessor is not public. wat?
                    return "Gradient";
                case SerializedPropertyType.Integer:
                    return prop.intValue.ToString();
                case SerializedPropertyType.LayerMask:
                    return prop.intValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    var obj = prop.objectReferenceValue;
                    if (obj == null)
                    {
                        return "NULL";
                    }

                    var pathId = prop.FindPropertyRelative("m_PathID").intValue;
                    var fileId = prop.FindPropertyRelative("m_FileID").intValue;
                    return string.Format("FileID: {0} PathID: {1} -- {2}", fileId, pathId, obj.ToString());
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue.ToString();
                case SerializedPropertyType.Rect:
                    return prop.rectValue.ToString();
                case SerializedPropertyType.RectInt:
                    return prop.rectIntValue.ToString();
                case SerializedPropertyType.String:
                    return prop.stringValue.ToString();
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value.ToString();
                case SerializedPropertyType.Vector2Int:
                    return prop.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value.ToString();
                case SerializedPropertyType.Vector3Int:
                    return prop.vector3IntValue.ToString();
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value.ToString();
            }

            return "UNKNOWN";
        }

        /// <summary>
        /// Converts a SerializedProperty to a VtValue, for writing to USD.
        /// </summary>
        static public pxr.VtValue PropToVtValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    // TODO: needs to be broken down into atoms.
                    return new pxr.VtValue();
                case SerializedPropertyType.ArraySize:
                    return prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Bounds:
                    return UnityTypeConverter.BoundsToVtArray(prop.boundsValue);
                case SerializedPropertyType.BoundsInt:
                    // TODO: add this to UnityTypeConverter.
                    var bi = prop.boundsIntValue;
                    var bnds = new Bounds(bi.center, bi.size);
                    return UnityTypeConverter.BoundsToVtArray(bnds);
                case SerializedPropertyType.Character:
                    return prop.intValue;
                case SerializedPropertyType.Color:
                    return UnityTypeConverter.ColorToVec4f(prop.colorValue);
                case SerializedPropertyType.Enum:
                    return prop.enumDisplayNames[prop.enumValueIndex];
                case SerializedPropertyType.ExposedReference:
                    // TODO.
                    //return prop.exposedReferenceValue;
                    return new pxr.VtValue();
                case SerializedPropertyType.FixedBufferSize:
                    return prop.fixedBufferSize;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                case SerializedPropertyType.Generic:
                    return "GENERIC";
                case SerializedPropertyType.Gradient:
                    // TODO: gradientValue accessor is not public. wat?
                    return "Gradient";
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                case SerializedPropertyType.LayerMask:
                    return prop.intValue;
                case SerializedPropertyType.ObjectReference:
                    var obj = prop.objectReferenceValue;
                    if (obj == null)
                    {
                        return new pxr.VtValue("");
                    }

                    // For object references in the scene, the asset path will be empty/null.
                    // However, for mesh and material instances in the scen, what do we want to do here?
                    // They are serialized to .unity files with just a file id, rather than fileid, pathid, and guid.
                    string assetPath = AssetDatabase.GetAssetPath(prop.objectReferenceValue);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        return new pxr.VtValue("");
                    }

                    var fileId = prop.FindPropertyRelative("m_FileID").intValue;
                    var pathId = prop.FindPropertyRelative("m_PathID").intValue;
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    return prop.FindPropertyRelative("m_PathID").intValue + ":" + guid + ":" + fileId;

                case SerializedPropertyType.Quaternion:
                    return UnityTypeConverter.QuaternionToQuatf(prop.quaternionValue);
                case SerializedPropertyType.Rect:
                    return UnityTypeConverter.RectToVtVec4(prop.rectValue);
                case SerializedPropertyType.RectInt:
                    // TODO: add this to UnityTypeConverter.
                    var ri = prop.rectIntValue;
                    return new pxr.GfVec4i(ri.x, ri.y, ri.width, ri.height);
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Vector2:
                    return UnityTypeConverter.Vector2ToVec2f(prop.vector2Value);
                case SerializedPropertyType.Vector2Int:
                    // TODO: add this to UnityTypeConverter.
                    return new pxr.GfVec2i(prop.vector2IntValue.x, prop.vector2IntValue.y);
                case SerializedPropertyType.Vector3:
                    return UnityTypeConverter.Vector3ToVec3f(prop.vector3Value);
                case SerializedPropertyType.Vector3Int:
                    var v3 = prop.vector3IntValue;
                    // TODO: add this to UnityTypeConverter.
                    return new pxr.GfVec3i(v3.x, v3.y, v3.z);
                case SerializedPropertyType.Vector4:
                    return UnityTypeConverter.Vector4ToVec4f(prop.vector4Value);
            }

            return "UNKNOWN";
        }

        /// <summary>
        /// Converts a VtValue to a SerializedProperty, to reconstruct the USD scene in Unity.
        /// </summary>
        static public void VtValueToProp(SerializedProperty prop, pxr.VtValue val)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    // TODO: needs to be broken down into atoms.
                    throw new System.NotImplementedException();
                case SerializedPropertyType.ArraySize:
                    //prop.intValue = (int)val;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)val;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = UnityTypeConverter.BoundsFromVtArray(val);
                    break;
                case SerializedPropertyType.BoundsInt:
                    // TODO: add this to UnityTypeConverter.
                    var bnds = UnityTypeConverter.BoundsFromVtArray(val);
                    var center = new Vector3Int((int)bnds.center.x, (int)bnds.center.y, (int)bnds.center.z);
                    var size = new Vector3Int((int)bnds.size.x, (int)bnds.size.y, (int)bnds.size.z);
                    prop.boundsIntValue = new BoundsInt(center, size);
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = (int)val;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = UnityTypeConverter.Vec4fToColor(val);
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = (int)val;
                    break;
                case SerializedPropertyType.ExposedReference:
                    // TODO.
                    //prop.exposedReferenceValue;
                    throw new System.NotImplementedException();
                case SerializedPropertyType.FixedBufferSize:
                    //prop.fixedBufferSize = (int)val;
                    // TODO.
                    throw new System.NotImplementedException();
                case SerializedPropertyType.Float:
                    prop.floatValue = (float)val;
                    break;
                case SerializedPropertyType.Generic:
                    throw new System.Exception();
                case SerializedPropertyType.Gradient:
                    // TODO: gradientValue accessor is not public. wat?
                    throw new System.NotImplementedException();
                case SerializedPropertyType.Integer:
                    prop.intValue = (int)val;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = (int)val;
                    break;
                case SerializedPropertyType.ObjectReference:
                    /*
                    var v2i = (pxr.GfVec2i)val;
                    if (v2i[0] == 0 && v2i[1] == 0) {
                      break;
                    }
                    Debug.Log("FileID: " + v2i[0] + " PathID: " + v2i[1]);
                    */
                    if (val.IsEmpty())
                    {
                        break;
                    }

                    string strValue = pxr.UsdCs.VtValueTostring(val);
                    if (string.IsNullOrEmpty(strValue))
                    {
                        break;
                    }

                    string[] names = strValue.Split(':');
                    int pathId = int.Parse(names[0]);
                    var guid = names[1];
                    int fileId = int.Parse(names[2]);

                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                    Object obj = objs[pathId];

                    Debug.Log("pathId: " + pathId
                        + " fileId: " + fileId
                        + " guid: " + guid.ToString()
                        + " obj: " + obj.ToString());

                    //break;
                    /* TODO:
                    string expectedName = names[2];
                    if (objs[index].name != expectedName) {
                      Debug.LogWarning("Expected name '" + expectedName + "' but found '" + objs[index].name + "'");
                    }
                     */
                    prop.FindPropertyRelative("m_PathID").intValue = pathId;
                    prop.FindPropertyRelative("m_FileID").intValue = fileId;
                    prop.objectReferenceValue = obj;

                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = UnityTypeConverter.QuatfToQuaternion(val);
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = UnityTypeConverter.Vec4fToRect(val);
                    break;
                case SerializedPropertyType.RectInt:
                    var rect = UnityTypeConverter.Vec4fToRect(val);
                    prop.rectIntValue = new RectInt((int)rect.xMin, (int)rect.yMin,
                        (int)rect.width, (int)rect.height);
                    break;
                case SerializedPropertyType.String:
                    var s = (string)val;
                    if (s == null)
                    {
                        break;
                    }

                    prop.stringValue = (string)val;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = UnityTypeConverter.Vec2fToVector2(val);
                    break;
                case SerializedPropertyType.Vector2Int:
                    // TODO: add this to UnityTypeConverter.
                    var v2 = (pxr.GfVec2i)val;
                    prop.vector2IntValue = new Vector2Int(v2[0], v2[1]);
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = UnityTypeConverter.Vec3fToVector3(val);
                    break;
                case SerializedPropertyType.Vector3Int:
                    // TODO: add this to UnityTypeConverter.
                    var v3 = (pxr.GfVec3i)val;
                    prop.vector3IntValue = new Vector3Int(v3[0], v3[1], v3[2]);
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = UnityTypeConverter.Vec4fToVector4(val);
                    break;
            }
        }

        /// <summary>
        /// Returns the SdfValueType for a given property, useful for creating attributes in USD to
        /// hold SerializedProperties.
        /// </summary>
        static public pxr.SdfValueTypeName GetSdfType(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    // TODO: needs to be broken down into atoms.
                    throw new System.NotImplementedException();
                case SerializedPropertyType.ArraySize:
                    return SdfValueTypeNames.Int;
                case SerializedPropertyType.Boolean:
                    return SdfValueTypeNames.Bool;
                case SerializedPropertyType.Bounds:
                    return SdfValueTypeNames.Float3Array;
                case SerializedPropertyType.BoundsInt:
                    return SdfValueTypeNames.Int3Array;
                case SerializedPropertyType.Character:
                    return SdfValueTypeNames.Int;
                case SerializedPropertyType.Color:
                    return SdfValueTypeNames.Float4;
                case SerializedPropertyType.Enum:
                    return SdfValueTypeNames.String;
                case SerializedPropertyType.ExposedReference:
                    // TODO.
                    //prop.exposedReferenceValue;
                    throw new System.NotImplementedException();
                case SerializedPropertyType.FixedBufferSize:
                    //prop.fixedBufferSize = (int)val;
                    // TODO.
                    throw new System.NotImplementedException();
                case SerializedPropertyType.Float:
                    return SdfValueTypeNames.Float;
                case SerializedPropertyType.Generic:
                    throw new System.Exception();
                case SerializedPropertyType.Gradient:
                    // TODO: gradientValue accessor is not public. wat?
                    throw new System.NotImplementedException();
                case SerializedPropertyType.Integer:
                    return SdfValueTypeNames.Int;
                case SerializedPropertyType.LayerMask:
                    return SdfValueTypeNames.Int;
                case SerializedPropertyType.ObjectReference:
                    return SdfValueTypeNames.String;
                case SerializedPropertyType.Quaternion:
                    return SdfValueTypeNames.Quatf;
                case SerializedPropertyType.Rect:
                    return SdfValueTypeNames.Float4;
                case SerializedPropertyType.RectInt:
                    return SdfValueTypeNames.Float4;
                case SerializedPropertyType.String:
                    return SdfValueTypeNames.String;
                case SerializedPropertyType.Vector2:
                    return SdfValueTypeNames.Float2;
                case SerializedPropertyType.Vector2Int:
                    return SdfValueTypeNames.Int2;
                case SerializedPropertyType.Vector3:
                    return SdfValueTypeNames.Float3;
                case SerializedPropertyType.Vector3Int:
                    return SdfValueTypeNames.Int3;
                case SerializedPropertyType.Vector4:
                    return SdfValueTypeNames.Float4;
            }

            throw new System.NotImplementedException();
        }
    }
}

#endif

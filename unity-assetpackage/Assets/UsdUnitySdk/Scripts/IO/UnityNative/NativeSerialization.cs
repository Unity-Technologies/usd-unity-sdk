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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace USD.NET.Unity {
  internal static class NativeSerialization {
    static bool m_initialized = false;

    static public void Init() {
      if (!m_initialized) {
        m_initialized = true;
        var bindings = UsdIo.Bindings;
        bindings.BindType(typeof(Component), new UsdTypeBinding(ComponentToVt, VtToComponent, SdfValueTypeNames.String));
        bindings.BindType(typeof(Object), new UsdTypeBinding(ObjectToVt, VtToObject, SdfValueTypeNames.String));
        bindings.BindType(typeof(Object[]), new UsdTypeBinding(ObjectArrayToVt, VtToObjectArray, SdfValueTypeNames.String));
      }
    }

    //
    // Useful reference:
    // https://docs.unity3d.com/Manual/script-Serialization.html
    //

    static private pxr.VtValue ComponentToVt(object obj) {
      var comp = (UnityEngine.Component)obj;
      string assetPath = AssetDatabase.GetAssetPath(comp);
      string guid = AssetDatabase.AssetPathToGUID(assetPath);
      return new pxr.VtValue(guid);
    }
    static private object VtToComponent(pxr.VtValue value) {
      // WORK IN PROGRESS
      var guid = pxr.UsdCs.VtValueTostring(value);
      return new pxr.VtValue(guid);
    }

    static private pxr.VtValue MeshToVt(object obj) {
      var mesh = (Mesh)obj;
      if (mesh == null) { return new pxr.VtValue(); }
      string assetPath = AssetDatabase.GetAssetPath(mesh);
      if (string.IsNullOrEmpty(assetPath)) {
        return new pxr.VtValue();
      }
      Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
      int i = 0;
      string name = "";
      foreach (var o in objs) {
        if (mesh == o) {
          name = objs[i].name;
          break;
        }
        i++;
      }
      string guid = AssetDatabase.AssetPathToGUID(assetPath);
      return new pxr.VtValue(i.ToString() + ":" + guid + ":" + name);
    }
    static private object VtToMesh(pxr.VtValue value) {
      if (value.IsEmpty()) { return null; }
      string[] names = pxr.UsdCs.VtValueTostring(value).Split(':');
      int index = int.Parse(names[0]);
      var guid = names[1];
      string expectedName = names[2];
      string assetPath = AssetDatabase.GUIDToAssetPath(guid);
      Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
      if (objs[index].name != expectedName) {
        Debug.LogWarning("Expected name '" + expectedName + "' but found '" + objs[index].name + "'");
      }
      return objs[index];
    }

    // -------------------------------------------------------------------------------------------- //
    // Serialization Functions
    // -------------------------------------------------------------------------------------------- //

    static private string ObjectToString(UnityEngine.Object unityObj) {
      string assetPath = AssetDatabase.GetAssetPath(unityObj);
      if (string.IsNullOrEmpty(assetPath)) {
        return new pxr.VtValue();
      }

      int i = 0;
      string name = "";
      Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);

      foreach (var o in objs) {
        if (unityObj == o) {
          name = objs[i].name;
          break;
        }
        i++;
      }

      string guid = AssetDatabase.AssetPathToGUID(assetPath);
      return i.ToString() + ":" + guid + ":" + name;
    }

    static private UnityEngine.Object StringToObject(string objStr) {
      string[] names = objStr.Split(':');
      int index = int.Parse(names[0]);
      var guid = names[1];
      string expectedName = names[2];
      string assetPath = AssetDatabase.GUIDToAssetPath(guid);
      Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
      if (objs[index].name != expectedName) {
        Debug.LogWarning("Expected name '" + expectedName + "' but found '" + objs[index].name + "'");
      }
      return objs[index];
    }

    static private pxr.VtValue ObjectToVt(object obj) {
      var unityObj = (UnityEngine.Object)obj;
      if (unityObj == null) { return new pxr.VtValue(); }
      return new pxr.VtValue(ObjectToString(unityObj));
    }
    static private object VtToObject(pxr.VtValue value) {
      if (value.IsEmpty()) { return null; }
      return StringToObject(pxr.UsdCs.VtValueTostring(value));
    }

    static private pxr.VtValue ObjectArrayToVt(object csObj) {
      var unityObjects = csObj as Object[];
      var builder = new System.Text.StringBuilder();
      bool primed = false;
      foreach (Object unityObj in unityObjects) {
        if (unityObj == null) { continue; }
        builder.Append(ObjectToString(unityObj));
        if (primed) {
          builder.Append("|");
        }
        primed = true;
      }
      return new pxr.VtValue(builder.ToString());
    }
    static private object VtToObjectArray(pxr.VtValue value) {
      if (value.IsEmpty()) { return null; }

      var unityObjArray = new List<UnityEngine.Object>();
      foreach (var str in pxr.UsdCs.VtValueTostring(value).Split('|')) {
        var unityObj = StringToObject(str);
        if (unityObj == null) { continue; }
        unityObjArray.Add(unityObj);
      }

      return unityObjArray.ToArray();
    }
    // --------- //

    static private string MaterialToString(Material material) {
      string assetPath = AssetDatabase.GetAssetPath(material);
      if (string.IsNullOrEmpty(assetPath)) {
        return new pxr.VtValue();
      }
      Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
      int i = 0;
      string name = "";
      foreach (var o in objs) {
        if (material == o) {
          name = objs[i].name;
          break;
        }
        i++;
      }
      string guid = AssetDatabase.AssetPathToGUID(assetPath);
      return i.ToString() + ":" + guid + ":" + name;
    }

    static private Material StringToMaterial(string matStr) {
      string[] names = matStr.Split(':');
      int index = int.Parse(names[0]);
      var guid = names[1];
      string expectedName = names[2];
      string assetPath = AssetDatabase.GUIDToAssetPath(guid);
      Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
      if (objs[index].name != expectedName) {
        Debug.LogWarning("Expected name '" + expectedName + "' but found '" + objs[index].name + "'");
      }
      return objs[index] as Material;
    }

    static private pxr.VtValue MaterialToVt(object obj) {
      var material = (Material)obj;
      if (material == null) { return new pxr.VtValue(); }
      return new pxr.VtValue(MaterialToString(material));
    }
    static private object VtToMaterial(pxr.VtValue value) {
      if (value.IsEmpty()) { return null; }
      return StringToMaterial(pxr.UsdCs.VtValueTostring(value));
    }

    static private pxr.VtValue MaterialArrayToVt(object obj) {
      var materials = (Material[])obj;
      var builder = new System.Text.StringBuilder();
      bool primed = false;
      foreach (Material m in materials) {
        if (m == null) { continue; }
        builder.Append(MaterialToString(m));
        if (primed) {
          builder.Append("|");
        }
        primed = true;
      }
      return new pxr.VtValue(builder.ToString());
    }
    static private object VtToMaterialArray(pxr.VtValue value) {
      if (value.IsEmpty()) { return null; }

      var matArray = new List<Material>();
      foreach (var str in pxr.UsdCs.VtValueTostring(value).Split('|')) {
        var mat = StringToMaterial(str);
        if (mat == null) { continue; }
        matArray.Add(mat);
      }
      return matArray.ToArray();
    }
  }
}
#endif
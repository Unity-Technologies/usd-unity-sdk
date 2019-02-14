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

namespace USD.NET.Unity {

  // -------------------------------------------------------------------------------------------- //
  // Data Structures
  // -------------------------------------------------------------------------------------------- //

  internal struct PathPair {
    public string usdPath;
    public string unityPath;
  }

  public struct PendingComponent {
    public pxr.UsdRelationship usdRel;
    public Component value;
  }

  internal struct PendingConnection {
    public pxr.UsdRelationship usdRel;
    public object targetObject;
    public System.Reflection.PropertyInfo propInfo;
    public System.Reflection.FieldInfo fieldInfo;
  }

  internal struct IoContext {
    public Scene usdScene;
    //public Dictionary<UsdPrefab, USD.NET.Scene> sceneMap;
    public Dictionary<string, GameObject> pathMap;
    public Dictionary<Component, string> compPaths;
    public List<PendingComponent> pendingComponents;
    public List<PendingConnection> pendingConnections;
    public pxr.TfTokenVector propertyOrder;

    public IoContext(Scene scene) {
      usdScene = scene;
      //this.sceneMap = sceneMap;
      compPaths = new Dictionary<Component, string>();
      pathMap = new Dictionary<string, GameObject>();
      pendingComponents = new List<PendingComponent>();
      pendingConnections = new List<PendingConnection>();
      propertyOrder = new pxr.TfTokenVector();
    }
  }

  public class NativeExporter {
    //
    // Useful reference:
    // https://docs.unity3d.com/Manual/script-Serialization.html
    //

    // -------------------------------------------------------------------------------------------- //
    // Path & Identifier Builders
    // -------------------------------------------------------------------------------------------- //

    public static string BuildComponentAttrName(Component comp, string nameSuffix, string memberName) {
      return "unity:component:" + comp.GetType().Name + nameSuffix + ":" + memberName;
    }

    // -------------------------------------------------------------------------------------------- //
    // Composition Helpers
    // -------------------------------------------------------------------------------------------- //
#if false
  static private void AddReferences(pxr.UsdPrim usdPrim,
                                    UsdPrefab[] references,
                                    IoContext context) {
    if (references == null || references.Length == 0) {
      return;
    }
    foreach (var refPrefab in references) {
      USD.NET.Scene refScene;
      if (!context.sceneMap.TryGetValue(refPrefab, out refScene)) {
        Debug.LogWarning("Failed to create reference, no USD scene was serialized for UsdPrefab: "
                         + refPrefab.name);
        continue;
      }
      string identifier = refScene.Stage.GetRootLayer().GetIdentifier();
      usdPrim.GetReferences().AddReference(identifier);
    }
  }
#endif

    // -------------------------------------------------------------------------------------------- //
    // Serialize Unity to -> USD
    // -------------------------------------------------------------------------------------------- //

    static public void ExportObject(ObjectContext objContext,
                                    ExportContext exportContext) {
#if false
      if (!USD.NET.Examples.InitUsd.Initialize()) {
        throw new System.ApplicationException("Failed to initialize USD");
      }
      NativeSerialization.Init();

      /*
      string relPath = null;
      foreach (var elem in usdPrefab.usdFilePath.Split('/')) {
        if (relPath == null) {
          // Skip one element.
          relPath = "";
          continue;
        }
        relPath += "../";
      }
      */

      var usdPrim = ExportObject_(objContext, exportContext);
#endif
    }

    static pxr.UsdPrim ExportObject_(ObjectContext objContext, ExportContext exportContext) {
      // Because Unity allows path aliasing and special characters, we need to store both the original
      // path and the Unity path.
      var unityObj = objContext.gameObject;

      var ugo = new UsdGameObjectSample();
      ugo.gameObject.name = unityObj.name;
      ugo.gameObject.activeSelf = unityObj.activeSelf;
      ugo.gameObject.layer = unityObj.layer;
      ugo.gameObject.hideFlags = unityObj.hideFlags;
      ugo.gameObject.isStatic = unityObj.isStatic;
      ugo.gameObject.tag = unityObj.tag;

      if (!(new pxr.SdfPath(objContext.path)).IsRootPrimPath()) {
        ugo.gameObject.localPosition = unityObj.transform.localPosition;
        ugo.gameObject.localScale = unityObj.transform.localScale;
        ugo.gameObject.localRotation = unityObj.transform.localRotation;
      } else {
        ugo.gameObject.localPosition = Vector3.zero;
        ugo.gameObject.localScale = Vector3.one;
        ugo.gameObject.localRotation = Quaternion.identity;
      }

      exportContext.scene.Write(objContext.path, ugo);

      var dict = new Dictionary<System.Type, int>();

      var usdPrim = exportContext.scene.GetPrimAtPath(new pxr.SdfPath(objContext.path));
      var propertyOrder = new pxr.TfTokenVector();

      foreach (Component comp in unityObj.GetComponents(typeof(Component))) {
        if (comp.GetType() == typeof(Transform)) {
          continue;
        }

        if (!dict.ContainsKey(comp.GetType())) {
          dict.Add(comp.GetType(), 0);
        }

        int count = dict[comp.GetType()] + 1;
        dict[comp.GetType()] = count;

        string suffix = "";
        if (count > 1) {
          suffix = "_" + count.ToString();
        }

        SerializeComponent(objContext, exportContext, propertyOrder, usdPrim, comp, suffix);

        var attr = usdPrim.CreateAttribute(
            new pxr.TfToken("unity:component:" + comp.GetType().Name + suffix + ":type"),
            SdfValueTypeNames.String);
        attr.Set(comp.GetType().AssemblyQualifiedName);

        // Disabled for now.
        //context.compPaths.Add(comp, attr.Get().ToString());
      }

      usdPrim.SetPropertyOrder(propertyOrder);

      return usdPrim;
    }

    // TODO:
    // GameObjects & Components
    //  - Find the path to the object
    //  - If nested, store a relationship to the relative nested path
    //  - If absolute, store nothing

    static void SerializeComponent(ObjectContext objContext,
                                   ExportContext exportContext,
                                   pxr.TfTokenVector propertyOrder,
                                   pxr.UsdPrim usdPrim,
                                   Component comp,
                                   string suffix) {
      var fields = comp.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      var props = comp.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty);

      foreach (System.Reflection.FieldInfo fieldInfo in fields) {
        if (fieldInfo.IsStatic || fieldInfo.IsNotSerialized || fieldInfo.IsLiteral || fieldInfo.IsInitOnly) {
          continue;
        }
        if (fieldInfo.IsPrivate && fieldInfo.GetCustomAttributes(typeof(UnityEngine.SerializeField), true).Length == 0) {
          continue;
        }
        if (fieldInfo.GetCustomAttributes(typeof(System.ObsoleteAttribute), true).Length != 0) {
          continue;
        }

        SerializeType(objContext, exportContext, propertyOrder, usdPrim, fieldInfo.FieldType, fieldInfo.GetValue(comp), comp, fieldInfo.Name, suffix);
      }

      foreach (var propInfo in props) {
        // Name and tag are inherited from the game object.
        if (propInfo.Name == "name") { continue; }
        if (propInfo.Name == "tag") { continue; }

        // By the Unity serialization rules, read-only members are not serialized.
        if (!propInfo.CanWrite) {
          continue;
        }

        // Skip deprectated and explicitly non-serialized members.
        if (propInfo.GetCustomAttributes(typeof(System.ObsoleteAttribute), true).Length != 0) {
          continue;
        }
        if (propInfo.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length != 0) {
          continue;
        }

        // Yuck: special cases for Unity built-in objects that don't play by the rules.
        if (propInfo.Name == "mesh" && comp.GetType() == typeof(MeshFilter)) { continue; }
        if (propInfo.Name == "materials" && comp.GetType() == typeof(MeshRenderer)) { continue; }
        if (propInfo.Name == "material" && comp.GetType() == typeof(MeshRenderer)) { continue; }
        if (propInfo.Name == "material" && comp.GetType() == typeof(BoxCollider)) { continue; }

        object csValue = propInfo.GetValue(comp, null);
        SerializeType(objContext, exportContext, propertyOrder, usdPrim,
                      propInfo.PropertyType, csValue, comp, propInfo.Name, suffix);
      }
    }

    static void SerializeType(ObjectContext objContext,
                              ExportContext exportContext,
                              pxr.TfTokenVector propertyOrder,
                              pxr.UsdPrim usdPrim,
                              System.Type expectedType,
                              object csValue,
                              Component comp,
                              string compMemberName,
                              string nameSuffix) {
      UsdTypeBinding binding;

      var attrName = new pxr.TfToken(BuildComponentAttrName(comp, nameSuffix, compMemberName));

      if (csValue == null) {
        return;
      }

      if (expectedType == typeof(Component) || expectedType.IsSubclassOf(typeof(Component))) {
        var otherComp = csValue as Component;
        var rel = usdPrim.CreateRelationship(attrName);
        exportContext.pendingComponents.Add(new PendingComponent { usdRel = rel, value = otherComp });
        propertyOrder.Add(rel.GetName());
        return;

      } else if (expectedType.IsArray && expectedType.GetElementType().IsSubclassOf(typeof(Component))) {
        var otherComps = csValue as Component[];
        var rel = usdPrim.CreateRelationship(attrName);
        foreach (var otherComp in otherComps) {
          exportContext.pendingComponents.Add(new PendingComponent { usdRel = rel, value = otherComp });
        }
        propertyOrder.Add(rel.GetName());
        return;

      } else if (expectedType == typeof(GameObject) || expectedType.IsSubclassOf(typeof(GameObject))) {
        var go = csValue as GameObject;
        if (!go) { return; }
        var rel = usdPrim.CreateRelationship(attrName);
        string path = UnityTypeConverter.GetPath((csValue as GameObject).transform);
        rel.AddTarget(new pxr.SdfPath(path));
        propertyOrder.Add(rel.GetName());
        return;

      } else if (csValue.GetType().IsSubclassOf(typeof(UnityEngine.Object))
              && USD.NET.UsdIo.Bindings.GetBinding(typeof(UnityEngine.Object), out binding)) {
#if false
      Debug.Log(usdPrim.GetPath().ToString() + "." + comp.GetType().Name + "." + compMemberName +
        " IsForeign: " + AssetDatabase.IsForeignAsset(obj) + 
        " IsMain: " + AssetDatabase.IsMainAsset(obj) + 
        " IsNative: " + AssetDatabase.IsNativeAsset(obj) + 
        " IsSubAsset: " + AssetDatabase.IsSubAsset(obj)
        );
#endif
      } else if (csValue.GetType().IsSubclassOf(typeof(UnityEngine.Object[]))
              && USD.NET.UsdIo.Bindings.GetBinding(typeof(UnityEngine.Object[]), out binding)) {
      } else if (USD.NET.UsdIo.Bindings.GetBinding(expectedType, out binding)) {
      } else {
        Debug.LogWarning("Cannot serialize type: " + expectedType + " " + comp.gameObject.name + "." + comp.GetType().Name + "." + compMemberName);
        return;
      }

      var value = binding.toVtValue(csValue);
      if (!value.IsEmpty()) {
        var attr = usdPrim.CreateAttribute(attrName, binding.sdfTypeName);
        attr.Set(value);
        propertyOrder.Add(attr.GetName());
      }
    }
  }

}

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

namespace USD.NET.Unity {
  public class NativeImporter {
    // -------------------------------------------------------------------------------------------- //
    // Deserialize USD to -> Unity
    // -------------------------------------------------------------------------------------------- //

    static public void ImportObject(Scene usdScene, GameObject go, pxr.UsdPrim prim) {
      // TODO: Handle objects correctly that have no native data.
#if false
      pxr.UsdPrim rootPrim = usdScene.Stage.GetPseudoRoot();
      var context = new IoContext(usdScene);
      PrimToObject(usdScene, go, prim);

      var reverseComp = new Dictionary<string, Component>();
      foreach (var kvp in context.compPaths) {
        reverseComp.Add(/*component*/kvp.Value, /*string*/kvp.Key);
      }

      foreach (var pending in context.pendingConnections) {
        var targets = pending.usdRel.GetTargets();
        if (targets == null || targets.Count == 0) {
          continue;
        }

        string path = targets[0];
        Object targetObject;

        if (reverseComp.ContainsKey(path)) {
          targetObject = reverseComp[path];
        } else if (context.pathMap.ContainsKey(path)) {
          targetObject = context.pathMap[path];
        } else {
          Debug.LogWarning("Could not find path: " + path);
          continue;
        }

        if (pending.fieldInfo != null) {
          pending.fieldInfo.SetValue(pending.targetObject, targetObject);
        } else if (pending.propInfo != null) {
          pending.propInfo.SetValue(pending.targetObject, targetObject, null);

        } else {
          Debug.LogError("Null member info: " + pending.usdRel.GetPath());
        }
      }
#endif
    }

    static void PrimToObject(Scene scene, GameObject newObj, pxr.UsdPrim usdPrim) {
      var usdGameObj = new UsdGameObjectSample();
      var nsDelim = pxr.UsdProperty.GetNamespaceDelimiter().ToString().ToCharArray();

      if (usdPrim.IsPseudoRoot()) {
        return;
      }
      // TODO: Feels like this should return a bool or must throw.
      scene.Read(usdPrim.GetPath().ToString(), usdGameObj);

      // TODO: collect all component types here.
      newObj.name = usdGameObj.gameObject.name;
      newObj.transform.localPosition = usdGameObj.gameObject.localPosition;
      newObj.transform.localRotation = usdGameObj.gameObject.localRotation;
      newObj.transform.localScale = usdGameObj.gameObject.localScale;

      //context.pathMap.Add(usdPrim.GetPath(), newObj);

      var compMap = new Dictionary<string, Component>();

      pxr.TfTokenVector props = usdPrim.GetPropertyOrder();

      foreach (pxr.TfToken propName in props) {
        if (!propName.ToString().StartsWith("unity:component:")) {
          continue;
        }

        pxr.UsdProperty prop = usdPrim.GetProperty(propName);

        // Yuck. Property should convert to attribute easily.
        var attr = usdPrim.GetAttribute(prop.GetName());
        var rel = usdPrim.GetRelationship(prop.GetName());

        if (!attr.IsValid() && !rel.IsValid()) { continue; }

        string[] names = attr.GetName().ToString().Split(nsDelim);
        
        string unityPrefix = names[0];
        string componentPrefix = names[1];
        string componentName = names[2];
        string memberName = names[3];
        string typeAttrName = unityPrefix + ":" + componentPrefix + ":" + componentName + ":type";

        Component comp;
        compMap.TryGetValue(componentName, out comp);

        if (comp == null) {
          var typeAttr = usdPrim.GetAttribute(new pxr.TfToken(typeAttrName));
          string assemblyQualifiedName = pxr.UsdCs.VtValueTostring(typeAttr.Get());
          System.Type.GetTypeFromHandle(new System.RuntimeTypeHandle());
          var compType = System.Type.GetType(assemblyQualifiedName);
          comp = newObj.GetComponent(compType);
          if (comp == null) {
            comp = newObj.AddComponent(compType);
          }
          //context.compPaths.Add(comp, typeAttr.GetPath().ToString());
          compMap[componentName] = comp;
        }

        // TODO: "type" should be namespace protected.
        //       Alternatively, it could be unity:components:Foo, the Component name itself.
        if (memberName == "type") {
          continue;
        }

        if (comp == null) {
          var typeAttr = usdPrim.GetAttribute(new pxr.TfToken(typeAttrName));
          string assemblyQualifiedName = pxr.UsdCs.VtValueTostring(typeAttr.Get());
          Debug.LogError("Unable to construct component: " + assemblyQualifiedName);
          continue;
        }

        // TODO: handle component is null.
        var propertyInfo = comp.GetType().GetProperty(memberName);
        var fieldInfo = comp.GetType().GetField(memberName);

        System.Type memberType = null;

        if (fieldInfo != null) {
          memberType = fieldInfo.FieldType;
        } else if (propertyInfo != null) {
          memberType = propertyInfo.PropertyType;
        } else {
          throw new System.Exception("Unexpected - " + prop.GetPath());
        }

        UsdTypeBinding binding;

        if (rel.IsValid()) {
          Debug.Log("TODO");
#if false
          context.pendingConnections.Add(
            new PendingConnection {
              targetObject = comp,
              fieldInfo = fieldInfo,
              propInfo = propertyInfo,
              usdRel = rel
            });
#endif
          continue;

        } else if (memberType.IsSubclassOf(typeof(UnityEngine.Object))
          && UsdIo.Bindings.GetBinding(typeof(UnityEngine.Object), out binding)) {
          // Found binding.

        } else if (memberType.IsSubclassOf(typeof(UnityEngine.Object[]))
          && UsdIo.Bindings.GetBinding(typeof(UnityEngine.Object[]), out binding)) {
          // Found binding.

        } else if (!UsdIo.Bindings.GetBinding(memberType, out binding)) {
          Debug.LogWarning("Cannot deserialize type: "
                           + memberType + " - " + prop.GetPath().ToString());
          continue;
        }

        object value = null;

        try {
          value = binding.toCsObject(attr.Get());
        } catch {
          Debug.LogError("Failed: " + attr.GetPath().ToString());
          continue;
          // Reverse lookup is causing issues.
          //throw;
        }

        try {
          if (fieldInfo != null) {
            fieldInfo.SetValue(comp, value);
          } else if (propertyInfo != null) {
            propertyInfo.SetValue(comp, value, null);
          } else {
            throw new System.Exception("Unexpected");
          }
        } catch (System.ArgumentException) {
          Debug.LogError("Failed to deserialize "
                         + attr.GetTypeName().ToString()
                         + " - object(" + value.GetType()
                         + ") expected(" + memberType.ToString() + ")");
        }
      }

      newObj.SetActive(usdGameObj.gameObject.activeSelf);
      newObj.hideFlags = usdGameObj.gameObject.hideFlags;
      newObj.isStatic = usdGameObj.gameObject.isStatic;
      newObj.layer = usdGameObj.gameObject.layer;
      newObj.tag = usdGameObj.gameObject.tag;
    }

  } // End Class
} // End Namespace

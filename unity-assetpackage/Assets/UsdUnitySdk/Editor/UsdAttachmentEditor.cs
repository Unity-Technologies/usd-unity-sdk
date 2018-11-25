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
using UnityEngine;
using UnityEditor;
using pxr;

namespace USD.NET.Unity {

  [CustomEditor(typeof(UsdAttachment))]
  public class UsdAttachmentEditor : Editor {

    public override void OnInspectorGUI() {
      var attachment = (UsdAttachment)target;
      var stageRoot = attachment.GetComponentInParent<StageRoot>();

      if (!stageRoot) {
        Debug.LogError("No stage root found");
        return;
      }

      var scene = stageRoot.GetScene();
      var prim = scene.GetPrimAtPath(attachment.m_usdPrimPath);

      //
      // BEGIN GUI.
      //

      GUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("USD Prim: ", GUILayout.Width(100));
      EditorGUILayout.SelectableLabel(attachment.m_usdPrimPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Prim Type: ", GUILayout.Width(100));
      EditorGUILayout.SelectableLabel(prim.GetTypeName(), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      EditorGUILayout.SelectableLabel(attachment.m_usdPrimPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
      GUILayout.EndHorizontal();

      EditorGUILayout.LabelField("USD Time: " + stageRoot.m_usdTime);

      // Separator.
      EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

      EditorGUILayout.LabelField("Composition Graph", EditorStyles.boldLabel);
      WalkNodes(prim.GetPrimIndex().GetRootNode());

      // Separator.
      EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

      DrawAttributeGui(prim, scene);
    }

    void WalkNodes(PcpNodeRef node) {
      GUI.enabled = node.HasSpecs();
      EditorGUILayout.LabelField("[" + node.GetArcType().ToString().Replace("PcpArcType", "") + "] < " + node.GetPath() + ">");
      WalkLayers(node, node.GetLayerStack().GetLayerTree(), 1);
      foreach (PcpNodeRef child in node.GetChildren()) {
        WalkNodes(child);
      }
      GUI.enabled = true;
    }

    void WalkLayers(PcpNodeRef node, SdfLayerTreeHandle tree, int indent) {
      GUILayout.BeginHorizontal();
      GUILayout.Space(indent * 20);
      EditorGUILayout.LabelField(tree.GetLayer().GetIdentifier());
      GUILayout.EndHorizontal();
      foreach (var childTree in tree.GetChildTrees()) {
        WalkLayers(node, childTree, indent++);
      }
    }

    void DrawAttributeGui(UsdPrim prim, Scene usdScene) {
      double usdTime = usdScene.Time.GetValueOrDefault();

      foreach (UsdAttribute attr in prim.GetAttributes()) {
        bool hasAuthoredValue = attr.HasAuthoredValueOpinion();
        bool hasValue = attr.HasValue();
        var displayName = attr.GetDisplayName();
        displayName = !string.IsNullOrEmpty(displayName) ? displayName : attr.GetName();
        
        GUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 100;

        if (hasAuthoredValue) {
          EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
        } else {
          EditorGUILayout.LabelField(displayName);
        }

        EditorGUIUtility.labelWidth = 10;
        var suffix = "";
        if (attr.GetTypeName().IsArray()) {
          suffix = "[]";
        }
        GUI.enabled = false;
        EditorGUILayout.LabelField(attr.GetTypeName().GetTfType().GetTypeName() + suffix);
        GUI.enabled = true;

        GUI.enabled = hasValue;
        if (GUILayout.Button("Value") ) {
          WriteAttrValue(attr, usdTime);
        }

        if (GUILayout.Button("Default")) {
          WriteAttrValue(attr, UsdTimeCode.Default());
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();
      }
    }

    void DrawEditor(UsdAttribute attr, UsdTimeCode time) {
      UsdTypeBinding binding;
      if (!UsdIo.Bindings.GetReverseBinding(attr.GetTypeName(), out binding)) {
        return;
      }
      var csObj = binding.toCsObject(attr.Get(time));

    }

    void WriteAttrValue(UsdAttribute attr, UsdTimeCode time) {
      var tmp = Scene.Create();
      var tmpPrim = tmp.Stage.DefinePrim(attr.GetPath().GetPrimPath(), attr.GetPrim().GetTypeName());
      var tmpAttr = tmpPrim.CreateAttribute(attr.GetName(), attr.GetTypeName());
      var val = attr.Get(time);
      var arraySize = -1;
      if (val.IsArrayValued()) {
        arraySize = (int)val.GetArraySize();
      }

      tmpAttr.Set(val, time);

      var stringified = "";
      tmp.Stage.ExportToString(out stringified, addSourceFileComment: false);
      tmp.Close();
      tmp = null;
      if (arraySize > -1) {
        stringified = "Array Size: " + arraySize + "\n" + stringified;
      }
      Debug.Log("Path: <" + attr.GetPath().ToString() + ">\n" + stringified);
    }

    void WriteAttrMetadata(UsdAttribute attr, UsdTimeCode time) {
      var tmp = Scene.Create();
      var tmpPrim = tmp.Stage.DefinePrim(attr.GetPath().GetPrimPath(), attr.GetPrim().GetTypeName());
      var tmpAttr = tmpPrim.CreateAttribute(attr.GetName(), attr.GetTypeName());

      // TODO: need better metadata binding.
      UsdMetadataValueMap metaData = attr.GetAllMetadata();
      tmpAttr.Set(attr.Get(time), time);

      var stringified = "";
      tmp.Stage.ExportToString(out stringified, addSourceFileComment: false);
      tmp.Close();
      tmp = null;
      Debug.Log("Path: <" + attr.GetPath().ToString() + ">\n" + stringified);
    }
  }
}

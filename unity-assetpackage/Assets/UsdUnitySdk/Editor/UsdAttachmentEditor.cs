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

    UsdAttribute selectedAttribute;

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

      EditorGUILayout.LabelField("USD Time: " + stageRoot.m_usdTime);

      //
      // Attribute Grid.
      //
      // Separator.
      EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
      DrawAttributeGui(prim, scene);

      //
      // Prim Composition Graph.
      //
      // Separator.
      EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
      EditorGUILayout.LabelField("Prim Composition Graph", EditorStyles.boldLabel);

      // Since composition can only happen at the prim level, the prim is used to construct the
      // composition graph. The Pcp (Prim Cache Population) API is the lowest level of the
      // composition system, which means it's most powerful and most complicated. PcpNodeRefs
      // provide direct access to the in-memory composition structure. Note that unloaded prims
      // will not have a PrimIndex.
      //
      // The composition graph is typically less useful for users, since it's a complicated
      // structure to understand, but often there is a small handful of power users on a
      // production responsible for setting up the pipeline composition structure; for them this
      // visualization can be extremely helpful.
      WalkNodes(prim.GetPrimIndex().GetRootNode());

      //
      // Attribute Composition.
      //
      if (selectedAttribute != null && selectedAttribute.IsValid()) {
        // Separator.
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Property Opinions", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Name: " + selectedAttribute.GetName());
        EditorGUILayout.LabelField("Type: " + selectedAttribute.GetTypeName().GetTfType().GetTypeName());
        EditorGUILayout.LabelField("IsArray: " + selectedAttribute.GetTypeName().IsArray());
        EditorGUILayout.LabelField("Composed Path: " + selectedAttribute.GetPath());
        EditorGUILayout.LabelField("Variability: " + selectedAttribute.GetVariability());
        EditorGUILayout.LabelField("Is Custom: " + selectedAttribute.IsCustom());

        // Spec are the low level API  in Sdf, the enable one to read and write a layer without
        // going through the composition graph. This is the fastest way to write USD data, but
        // also the most complicated.
        //
        // A property stack is an ordered list of all opinions about what this value should be.
        // The visualization below is a great debugging aid in production when something looks
        // wrong, but you have no idea why your value (opinion) isn't winning. It lets the user
        // understand how composition is working and shows them exactly what files are contributing
        // the final result.

        var specs = selectedAttribute.GetPropertyStack();
        if (specs.Count == 0) {
          EditorGUILayout.LabelField("[ Attribute has no authored opinions ]", EditorStyles.boldLabel);
        }
        foreach (var propSpec in specs) {
          EditorGUILayout.LabelField("<" + propSpec.GetPath() + ">");
          GUILayout.BeginHorizontal();
          GUILayout.Space(20);
          EditorGUILayout.LabelField(propSpec.GetLayer().GetIdentifier());
          GUILayout.EndHorizontal();
        }
        
      }
    }

    #region "Composition Graph"
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
    #endregion

    #region "Attribute Grid"
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
        if (GUILayout.Button("Value")) {
          DebugPrintAttr(usdScene, attr, usdTime);
        }

        if (GUILayout.Button("Compostion")) {
          if (selectedAttribute == attr) {
            selectedAttribute = null;
          } else {
            selectedAttribute = attr;
          }
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();
      }
    }

    /// <summary>
    /// Serializes the attribute value to USD-ASCII and writes it to Debug.Log.
    /// </summary>
    void DebugPrintAttr(Scene srcScene, UsdAttribute attr, UsdTimeCode time) {

      // Copy the desired attribute into a temp scene, with nothing other than this one attr.
      var tmpScene = Scene.Create();

      // Up-Axis is unrelated to the attribute, but it makes the generated USDA look more correct,
      // since it will include an up-axis.
      tmpScene.UpAxis = srcScene.UpAxis;

      // Define the owning prim for the attribute.
      var tmpPrim = tmpScene.Stage.DefinePrim(attr.GetPath().GetPrimPath(), attr.GetPrim().GetTypeName());

      // Define the attribute itself.
      var tmpAttr = tmpPrim.CreateAttribute(attr.GetName(), attr.GetTypeName());

      // Gather the default value and the value at the current time.
      var defaultVal = attr.Get(UsdTimeCode.Default());
      var valAtTime = attr.Get(time);

      // Copy them to the new attribute.
      tmpAttr.Set(defaultVal);
      tmpAttr.Set(valAtTime, time);

      // If this is an array, get the size.
      var arraySize = -1;
      if (valAtTime.IsArrayValued()) {
        arraySize = (int)valAtTime.GetArraySize();
      }

      // Metadata cannot be time-varying.
      UsdMetadataValueMap metaData = attr.GetAllMetadata();
      foreach (var key in metaData.GetKeys()) {
        tmpAttr.SetMetadata(key, metaData.GetValue(key));
      }

      // Export the single attribute to a string.
      var stringified = "";
      tmpScene.Stage.ExportToString(out stringified, addSourceFileComment: false);

      // Dispose the tmp scene.
      tmpScene.Close();
      tmpScene = null;

      // Print.
      if (arraySize > -1) {
        stringified = "Array Size: " + arraySize + "\n" + stringified;
      }
      Debug.Log("Path: <" + attr.GetPath().ToString() + ">\n" + stringified);
    }
    #endregion

    /// <summary>
    /// Returns the C# value from USD or null if there was no value or the value could not be converted.
    /// </summary>
    object GetCSharpValue(UsdAttribute attr, UsdTimeCode time) {
      UsdTypeBinding binding;
      if (!UsdIo.Bindings.GetReverseBinding(attr.GetTypeName(), out binding)) {
        // No binding for this type.
        return null;
      }
      return binding.toCsObject(attr.Get(time));
    }

  }
}

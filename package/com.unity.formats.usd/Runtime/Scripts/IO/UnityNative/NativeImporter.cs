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
using USD.NET;
#if UNITY_EDITOR
using UnityEditor;

namespace Unity.Formats.USD
{
    public class NativeImporter
    {
        // -------------------------------------------------------------------------------------------- //
        // Deserialize USD to -> Unity
        // -------------------------------------------------------------------------------------------- //

        static public void ImportObject(Scene scene,
            GameObject go,
            pxr.UsdPrim usdPrim,
            SceneImportOptions options)
        {
            if (!options.importMonoBehaviours)
            {
                return;
            }

            var comps = usdPrim.GetAuthoredPropertiesInNamespace("unity:component");
            foreach (var compProp in comps)
            {
                var compAttr = usdPrim.GetAttribute(compProp.GetName());
                string assemblyQualifiedName = (string)compAttr.Get(0);
                var compType = System.Type.GetType(assemblyQualifiedName);

                // TODO: Handle multiple components of the same type.
                Component comp = go.GetComponent(compType);
                if (comp == null)
                {
                    comp = go.AddComponent(compType);
                }

                var so = new SerializedObject(comp);
                var prop = so.GetIterator();
                prop.Next(true);
                var sb = new System.Text.StringBuilder();

                // TODO: Handle multiple components of the same type.
                PropertyFromUsd(usdPrim, prop, sb, comp.GetType().Name);

                so.ApplyModifiedProperties();
                Debug.Log(sb.ToString());
            }
        }

        /// <summary>
        /// Constructs Unity SerializedProperties from USD.
        /// </summary>
        static void PropertyFromUsd(pxr.UsdPrim prim,
            SerializedProperty prop,
            System.Text.StringBuilder sb,
            string propPrefix)
        {
            if (prim == null)
            {
                Debug.LogError("Null prim - " + propPrefix);
            }

            if (!prim.IsValid())
            {
                Debug.LogError("Invalid prim: " + prim.GetPath().ToString());
            }

            string prefix = "";
            try
            {
                var nameStack = new List<string>();
                nameStack.Add("unity");
                if (!string.IsNullOrEmpty(propPrefix))
                {
                    nameStack.Add(propPrefix);
                }

                string lastName = "";
                int lastDepth = 0;

                while (prop.Next(prop.propertyType == SerializedPropertyType.Generic && !prop.isArray))
                {
                    string tabIn = "";
                    for (int i = 0; i < prop.depth; i++)
                    {
                        tabIn += "  ";
                    }

                    if (prop.depth > lastDepth)
                    {
                        Debug.Assert(lastName != "");
                        nameStack.Add(lastName);
                    }
                    else if (prop.depth < lastDepth)
                    {
                        nameStack.RemoveRange(nameStack.Count - (lastDepth - prop.depth), lastDepth - prop.depth);
                    }

                    lastDepth = prop.depth;
                    lastName = prop.name;

                    if (nameStack.Count > 0)
                    {
                        prefix = string.Join(":", nameStack.ToArray());
                        prefix += ":";
                    }
                    else
                    {
                        prefix = "";
                    }

                    sb.Append(tabIn + prefix + prop.name + "[" + prop.propertyType.ToString() + "] = ");
                    if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
                    {
                        // TODO.
                        sb.AppendLine("ARRAY");
                    }
                    else if (prop.propertyType == SerializedPropertyType.Generic)
                    {
                        sb.AppendLine("Generic");
                    }
                    else if (prop.propertyType == SerializedPropertyType.AnimationCurve ||
                             prop.propertyType == SerializedPropertyType.Gradient)
                    {
                        // TODO.
                        sb.AppendLine(NativeSerialization.ValueToString(prop));
                    }
                    else
                    {
                        sb.AppendLine(NativeSerialization.ValueToString(prop));
                        var attrName = new pxr.TfToken(prefix + prop.name);
                        var attr = prim.GetAttribute(attrName);

                        if (attr == null)
                        {
                            Debug.LogError("Null attr: " + prim.GetPath().ToString() + "." + attrName.ToString());
                        }

                        if (!attr.IsValid())
                        {
                            Debug.LogError("Attribute not found:" + attr.GetPath().ToString());
                        }

                        NativeSerialization.VtValueToProp(prop, attr.Get(0));
                    }
                }
            }
            catch
            {
                Debug.LogWarning("Failed on: " + prim.GetPath() + "." + prefix + prop.name);
                throw;
            }
        }
    } // End Class
} // End Namespace
#else
namespace Unity.Formats.USD
{
    public class NativeImporter
    {
        static public void ImportObject(Scene scene,
            GameObject go,
            pxr.UsdPrim usdPrim,
            SceneImportOptions options)
        { }
    }
}
#endif

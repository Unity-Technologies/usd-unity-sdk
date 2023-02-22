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

namespace Unity.Formats.USD
{
    [CustomEditor(typeof(UsdVariantSet))]
    public class UsdVariantSetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var variantSet = (UsdVariantSet)this.target;

            int selectedIndex = 0;
            int varIdx = 0;
            int setIdx = 0;
            bool selChanged = false;

            GUILayout.Label("Variant Sets");

            foreach (string setName in variantSet.m_variantSetNames)
            {
                var options = new string[variantSet.m_variantCounts[setIdx] + 1];
                selectedIndex = 0;
                options[0] = " ";
                for (int i = 0; i < variantSet.m_variantCounts[setIdx]; i++)
                {
                    options[i + 1] = variantSet.m_variants[varIdx + i];
                    if (options[i + 1] == variantSet.m_selected[setIdx])
                    {
                        selectedIndex = i + 1;
                    }
                }

                int newSel = EditorGUILayout.Popup(setName, selectedIndex, options);

                if (selectedIndex != newSel)
                {
                    selChanged = true;
                }

                if (newSel == 0)
                {
                    variantSet.m_selected[setIdx] = "";
                }
                else
                {
                    variantSet.m_selected[setIdx] = variantSet.m_variants[varIdx + newSel - 1];
                }

                varIdx += variantSet.m_variantCounts[setIdx];
                setIdx += 1;
            }

            if (!selChanged)
            {
                return;
            }

            variantSet.ApplyVariantSelections();
        }
    }
}

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
using pxr;

namespace Unity.Formats.USD.Examples
{
    /// <summary>
    /// For matching Usd Paths, set the HideFlags on the GameObject.
    /// </summary>
    /// <seealso cref="UnityEngine.HideFlags"/>
    public class ImportProcessorExample_SetHideFlags : RegexImportProcessor, IImportPostProcessHierarchy
    {
        public HideFlags hideFlagsSettings = HideFlags.DontSave;

        public enum Operation
        {
            Overwrite,
            LogicalAND,
            LogicalOR,
            LogicalXOR
        }

        [Tooltip("The logical operator to use when setting the flags. In most cases Or will produce expected results")]
        public Operation operation = Operation.LogicalOR;

        public void PostProcessHierarchy(PrimMap primMap, SceneImportOptions sceneImportOptions)
        {
            Debug.Log("PostProcessHierarchy:");

            InitRegex();

            foreach (KeyValuePair<SdfPath, GameObject> kvp in primMap)
            {
                if (!IsMatch(kvp.Key)) continue;
                GameObject go = kvp.Value;
                if (go == this.gameObject) continue;
                switch (operation)
                {
                    case Operation.LogicalAND:
                        go.hideFlags &= hideFlagsSettings;
                        break;
                    case Operation.LogicalOR:
                        go.hideFlags |= hideFlagsSettings;
                        break;
                    case Operation.LogicalXOR:
                        go.hideFlags ^= hideFlagsSettings;
                        break;
                    default:
                        go.hideFlags = hideFlagsSettings;
                        break;
                }
                Debug.Log($"Applying Object <{SampleUtils.SetTextColor(SampleUtils.TextColor.Blue, go.name)}> HideFlags with <{SampleUtils.SetTextColor(SampleUtils.TextColor.Green, hideFlagsSettings.ToString())}>");
            }

            Debug.Log("PostProcessHierarchy End");
        }

        public void RefreshUSD()
        {
            this.GetComponent<UsdAsset>().Reload(true);
        }

        void Reset()
        {
            matchExpression = @"^\/.+";
            isNot = false;
            matchType = EMatchType.Regex;
            compareAgainst = ECompareAgainst.UsdPath;
        }
    }
}

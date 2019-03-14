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

namespace Unity.Formats.USD.Examples {

  /// <summary>
  /// For matching Usd Paths, set the HideFlags on the GameObject.
  /// </summary>
  /// <seealso cref="UnityEngine.HideFlags"/>
  public class SetHideFlags : RegexImportProcessor, IImportPostProcessHierarchy {

    public HideFlags hideFlagsSettings = HideFlags.DontSave;

    public void PostProcessHierarchy(PrimMap primMap, SceneImportOptions sceneImportOptions) {
      InitRegex();

      foreach (KeyValuePair<SdfPath, GameObject> kvp in primMap) {
        if (!IsMatch(kvp.Key)) continue;
        GameObject go = kvp.Value;
        go.hideFlags = hideFlagsSettings;
      }
    }

    void Reset() {
      matchExpression = "/";
      isNot = true;
      matchType = EMatchType.Wildcard;
      compareAgainst = ECompareAgainst.UsdName;
    }
  }
}

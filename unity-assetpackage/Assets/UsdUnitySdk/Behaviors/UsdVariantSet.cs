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

using System.Linq;
using UnityEngine;

namespace USD.NET.Unity {
  public class UsdVariantSet : MonoBehaviour {
    public string[] m_variantSetNames;
    public string[] m_selected;
    public string[] m_variants;
    public int[] m_variantCounts;
    public string m_primPath;

    public void SyncVariants(pxr.UsdPrim prim, pxr.UsdVariantSets variantSets) {
      var setNames = variantSets.GetNames();
      m_variantSetNames = setNames.ToArray();
      m_selected = m_variantSetNames.Select(setName => variantSets.GetVariantSelection(setName)).ToArray();
      m_variants = m_variantSetNames.SelectMany(setName => variantSets.GetVariantSet(setName).GetVariantNames()).ToArray();
      m_variantCounts = m_variantSetNames.Select(setName => variantSets.GetVariantSet(setName).GetVariantNames().Count).ToArray();
      m_primPath = prim.GetPath();
    }
  }
}

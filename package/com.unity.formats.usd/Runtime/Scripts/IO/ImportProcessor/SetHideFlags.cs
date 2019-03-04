using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using pxr;

//possibly move this to Examples
namespace Unity.Formats.USD
{
    public class SetHideFlags : RegexImportProcessor, IImportPostProcessHierarchy
    {
        public HideFlags hideFlagsSettings = HideFlags.DontSave;
        public void PostProcessHierarchy(PrimMap primMap, SceneImportOptions sceneImportOptions)
        {
            InitRegex();

            foreach (KeyValuePair<SdfPath, GameObject> kvp in primMap)
            {
                if (!IsMatch(kvp.Key)) continue;
                GameObject go = kvp.Value;
                go.hideFlags = hideFlagsSettings;
            }
        }

        void Reset()
        {
            matchExpression = "/";
            isNot = true;
            matchType = EMatchType.Wildcard;
            compareAgainst = ECompareAgainst.UsdName;
        }
    }
}

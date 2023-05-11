// Copyright 2023 Unity Technologies. All rights reserved.
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
using System.Text.RegularExpressions;
using pxr;

namespace Unity.Formats.USD.Examples
{
    /// <summary>
    /// Abstract class meant as base class for certain IImportPostProcessHierarchy
    /// implmentations.  Has methods for filtering Usd Paths based on glob-style wildcards or regex.
    /// </summary>
    public abstract class RegexImportProcessor : MonoBehaviour
    {
        public enum ECompareAgainst
        {
            UsdName,
            UsdPath,
        }

        public enum EMatchType
        {
            Wildcard,
            Regex,
        }

        [Tooltip(
            "UsdName matches only the name of the GameObject/UsdPrim, while UsdPath matches against the entire path of the object starting at /")]
        public ECompareAgainst compareAgainst;

        [Tooltip("Wildcard matches using simple filesystem-style globbing, Regex uses strict regular expression")]
        public EMatchType matchType;

        [Tooltip("If true, matches are ignored, failed matches are proccessed.")]
        public bool isNot = false;

        [Tooltip("The wildcard or regex expression to test against")]
        public string matchExpression = "*";

        private Regex m_regex;

        protected bool IsMatch(SdfPath sdfPath)
        {
            string test = compareAgainst == ECompareAgainst.UsdName ? sdfPath.GetName() : sdfPath.ToString();
            return isNot ? !m_regex.IsMatch(test) : m_regex.IsMatch(test);
        }

        protected void InitRegex()
        {
            switch (matchType)
            {
                case EMatchType.Wildcard:
                    m_regex = new Regex(WildcardToRegex(matchExpression));
                    break;
                case EMatchType.Regex:
                    m_regex = new Regex(matchExpression);
                    break;
            }
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }
    }
}

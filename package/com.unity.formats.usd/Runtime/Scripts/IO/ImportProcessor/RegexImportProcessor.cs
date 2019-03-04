using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using pxr;

//possibly move this to Examples
namespace Unity.Formats.USD
{
    public abstract class RegexImportProcessor : MonoBehaviour
    {

        public enum ECompareAgainst { UsdName, UsdPath }
        public ECompareAgainst compareAgainst;

        public enum EMatchType { Wildcard, Regex }
        public EMatchType matchType;

        public bool isNot = false;

        public string expression = "*";
        private Regex m_regex;

        protected bool IsMatch(SdfPath sdfPath)
        {
            string test = compareAgainst == ECompareAgainst.UsdName ? sdfPath.GetName() : sdfPath.ToString();
            return isNot ? ! m_regex.IsMatch(test) : m_regex.IsMatch(test);
        }

        protected void InitRegex()
        {
            switch (matchType)
            {
                case EMatchType.Wildcard:
                    m_regex = new Regex(WildcardToRegex(expression));
                    break;
                case EMatchType.Regex:
                    m_regex = new Regex(expression);
                    break;
            }
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
            Replace("\\*", ".*").
            Replace("\\?", ".") + "$";
        }
    }
}

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

using System;
using System.IO;
using UnityEngine;

namespace Unity.Formats.USD
{
    /// <summary>
    /// Basic functionality shared among Importers.
    /// </summary>
    public static class ImporterBase
    {
        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        public static String MakeRelativePath(string anchorPath, string pathToMakeRelative)
        {
            if (string.IsNullOrEmpty(anchorPath)) throw new ArgumentNullException("pathToMakeRelative");
            if (string.IsNullOrEmpty(pathToMakeRelative)) throw new ArgumentNullException("anchorPath");
            if (anchorPath == pathToMakeRelative)
            {
                return Path.GetFileName(pathToMakeRelative);
            }

            Uri fromUri = new Uri(anchorPath);
            Uri toUri = new Uri(pathToMakeRelative);

            // path can't be made relative.
            if (fromUri.Scheme != toUri.Scheme)
            {
                return pathToMakeRelative;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static bool ApproximatelyEqual(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            bool equal = true;
            for (int i = 0; i < 16; i++)
            {
                equal = equal && (Mathf.Approximately(lhs[i], rhs[i]));
            }

            return equal;
        }

        /// <summary>
        /// Moves the given component to be first in the list on the GameObject.
        /// If not in editor, this function is a no-op.
        /// </summary>
        public static void MoveComponentFirst(Component comp)
        {
#if UNITY_EDITOR
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp(comp))
            {
            }
#else
            Debug.LogWarning("Cannot reorder component, not in editor");
#endif
        }

        /// <summary>
        /// Moves the given component to be last in the list on the GameObject.
        /// If not in editor, this function is a no-op.
        /// </summary>
        public static void MoveComponentLast(Component comp)
        {
#if UNITY_EDITOR
            while (UnityEditorInternal.ComponentUtility.MoveComponentDown(comp))
            {
            }

#else
            Debug.LogWarning("Cannot reorder component, not in editor");
#endif
        }

        public static T GetOrAddComponent<T>(GameObject go, bool insertFirst = false) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (!comp)
            {
                comp = go.AddComponent<T>();
            }

            if (insertFirst)
            {
                MoveComponentFirst(comp);
            }

            return comp;
        }
    }
}

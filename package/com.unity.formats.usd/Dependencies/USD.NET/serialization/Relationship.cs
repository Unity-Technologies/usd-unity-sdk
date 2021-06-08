// Copyright 2017 Google Inc. All rights reserved.
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
using System.Collections.Generic;
using pxr;

namespace USD.NET
{
    /// <summary>
    /// A UsdRelationship allows a prim to target another object in the scenegraph.
    /// The semantic meaning of the relationship is defined by the schema, for example UsdShade
    /// uses a "material:binding" relationship to indicate the material which should be bound to
    /// the prim declaring the attribute.
    /// </summary>
    public class Relationship
    {
        public Relationship()
        {
        }

        public Relationship(string targetPath)
        {
            targetPaths = new string[] { targetPath };
        }

        public Relationship(string[] targetPaths)
        {
            this.targetPaths = targetPaths;
        }

        public string[] targetPaths;

        /// <summary>
        /// Returns the i_th target or null if fewer than i+1 targets are specified.
        /// </summary>
        public string GetTarget(int index)
        {
            if (targetPaths == null) { return null; }
            if (targetPaths.Length < index + 1) { return null; }
            return targetPaths[index];
        }

        /// <summary>
        /// Returns the first target path. If there is not exactly one target path, an exception is
        /// thrown.
        /// </summary>
        public string GetOnlyTarget()
        {
            var t = GetTarget(0);
            if (t == null)
            {
                throw new ArgumentException("The relationship had no targets");
            }
            else if (targetPaths.Length > 1)
            {
                throw new ArgumentException("The relationship had more than one target");
            }
            return t;
        }

        public static implicit operator Relationship(string path)
        {
            var r = new Relationship();
            if (path == null)
            {
                r.targetPaths = null;
            }
            else if (path == string.Empty)
            {
                r.targetPaths = new string[0];
            }
            else
            {
                r.targetPaths = new string[] { path };
            }
            return r;
        }

        public static implicit operator Relationship(string[] paths)
        {
            var r = new Relationship();
            r.targetPaths = paths;
            return r;
        }
    }
}

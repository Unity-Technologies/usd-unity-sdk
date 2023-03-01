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
using System.Collections.Generic;

namespace USD.NET
{
    /// <summary>
    /// Provides access to a single generic field.
    /// </summary>
    public interface ValueAccessor
    {
        object GetValue();
        void SetValue(object value);
        Type GetValueType();
    }

    /// <summary>
    /// This allows to add arbitrary primvars to a sample
    /// </summary>
    public interface IArbitraryPrimvars
    {
        void AddPrimvars(List<string> primvars);

        Dictionary<string, Primvar<object>> GetArbitraryPrimvars();
    }

    /// <summary>
    /// The PrimvarBase class enables strongly typed access to non-generic fields.
    /// See the UsdGeom Primvar reference for details:
    /// http://graphics.pixar.com/usd/docs/api/class_usd_geom_primvar.html#details
    /// </summary>
    public class PrimvarBase
    {
        /// <summary>
        /// Interpolation indicates how many values are present in the primvar value array and how
        /// those values are interpolated over the primative surface.
        /// </summary>
        [UsdMetadata]
        public PrimvarInterpolation interpolation = PrimvarInterpolation.Constant;

        /// <summary>
        /// The number of elements per interpolated value. For example, a mesh with a float4[] primvar
        /// per-vertex, should have an element size of 4. An int[] primvar should have elementSize of
        /// 1.
        /// </summary>
        [UsdMetadata]
        public int elementSize = 1;

        /// <summary>
        /// A primvar can be indexed, exactly like positions and indices in a mesh. These indices
        /// indicate individual values in the value array.
        /// </summary>
        [UsdMetadata]
        public int[] indices;

        /// <summary>
        /// A convenience method for converting the C# interpolation enum to a USD token.
        /// </summary>
        public pxr.TfToken GetInterpolationToken()
        {
            switch (interpolation)
            {
                case PrimvarInterpolation.Constant:
                    return pxr.UsdGeomTokens.constant;
                case PrimvarInterpolation.FaceVarying:
                    return pxr.UsdGeomTokens.faceVarying;
                case PrimvarInterpolation.Uniform:
                    return pxr.UsdGeomTokens.uniform;
                case PrimvarInterpolation.Varying:
                    return pxr.UsdGeomTokens.varying;
                case PrimvarInterpolation.Vertex:
                    return pxr.UsdGeomTokens.vertex;
                default:
                    throw new Exception("Unknown primvar interpolation");
            }
        }

        /// <summary>
        /// A convenience method for converting a USD token into C# interpolation enum
        /// </summary>
        public void SetInterpolationToken(pxr.TfToken token)
        {
            if (token == pxr.UsdGeomTokens.constant)
            {
                interpolation = PrimvarInterpolation.Constant;
                return;
            }
            else if (token == pxr.UsdGeomTokens.faceVarying)
            {
                interpolation = PrimvarInterpolation.FaceVarying;
                return;
            }
            else if (token == pxr.UsdGeomTokens.uniform)
            {
                interpolation = PrimvarInterpolation.Uniform;
                return;
            }
            else if (token == pxr.UsdGeomTokens.varying)
            {
                interpolation = PrimvarInterpolation.Varying;
                return;
            }
            else if (token == pxr.UsdGeomTokens.vertex)
            {
                interpolation = PrimvarInterpolation.Vertex;
                return;
            }
            else
            {
                throw new Exception("Unknown primvar interpolation token");
            }
        }
    }

    /// <summary>
    /// Represents a PrimVar in USD. Since primvars have associated metadata which is often required
    /// for I/O, this class enables reading that data in a consistent way. It is implicit that any
    /// Primvar object will be in the namespace "primvars".
    /// </summary>
    public class Primvar<T> : PrimvarBase, ValueAccessor
    {
        public Primvar()
        {
        }

        public T value;

        public object GetValue()
        {
            return value;
        }

        public void SetValue(object o)
        {
            value = (T)o;
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        public bool IsArray => typeof(T).IsArray;

        public int Length => (IsArray && value != null) ? (value as Array).Length : 0;

        // Furture work: support IdTargets. See "ID Attribute API" here:
        // http://graphics.pixar.com/usd/docs/api/class_usd_geom_primvar.html
    }
}

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

namespace USD.NET
{
    /// <summary>
    /// Declares the variability of the UsdAttribute that will be generated.
    /// </summary>
    ///
    /// <remarks>
    /// Setting the value to Variability.Uniform indicates the UsdAttribute may not have
    /// time samples and can only hold a single value. By default, UsdAttributes will be
    /// declared as Varying unless writing to a schema that previously declared the attribute
    /// to be Uniform.
    /// </remarks>
    public class UsdVariabilityAttribute : Attribute
    {
        public pxr.SdfVariability SdfVariability
        {
            get;
            private set;
        }

        public UsdVariabilityAttribute(Variability variability)
        {
            this.SdfVariability = variability == Variability.Uniform
                ? pxr.SdfVariability.SdfVariabilityUniform
                : pxr.SdfVariability.SdfVariabilityVarying;
        }
    }

    /// <summary>
    /// Declares the USD namespace that should be prepended to this attribute.
    /// </summary>
    ///
    /// <remarks>
    /// For example setting the namespace to "foo" on an attribute called "bar" will generate a USD
    /// attribute named foo:bar. Namespaces will be nested when multiple namespaces apply to a single
    /// attribute, e.g. foo:bar:baz.
    /// </remarks>
    public class UsdNamespaceAttribute : Attribute
    {
        public string Name { get; private set; }
        public UsdNamespaceAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Declares the USD schema to use when serializing an object.
    /// </summary>
    ///
    /// <remarks>
    /// For example, any C# object may be serialized as a UsdGeomMesh by declaring the schema to be
    /// "Mesh". The C++ class name or any registered aliases may be used, e.g. UsdSchema("Mesh") and
    /// UsdSchema("UsdGeomMesh") are equivalent.
    /// </remarks>
    public class UsdSchemaAttribute : Attribute
    {
        public string Name { get; private set; }
        public UsdSchemaAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Declares a string/string[] valued member to be serialized as a UsdRelationship.
    /// </summary>
    /// <remarks>
    /// Relationships may have multiple targets; when only a single target is valid, the member should
    /// be declared with type string, since there can only be one value. Similarly, when the
    /// relationship may have multiple values, the corresponding type is string[].
    /// </remarks>
    public class UsdRelationshipAttribute : Attribute
    {
    }

    /// <summary>
    /// Declares a string value member to be serialized as an SdfAssetPath.
    /// </summary>
    public class UsdAssetPathAttribute : Attribute
    {
    }

    /// <summary>
    /// Declares the attribute should be stored as metadata under the "customData" key.
    /// </summary>
    public class CustomDataAttribute : Attribute
    {
        public CustomDataAttribute()
        {
        }
    }

    /// <summary>
    /// Declares the attribute should be stored as metadata. Note that the field must be registered
    /// with USD.
    /// </summary>
    public class UsdMetadataAttribute : Attribute
    {
        public UsdMetadataAttribute()
        {
        }
    }

    /// <summary>
    /// Declares the attribute as vertex data which can be made available to the shader at render
    /// time and enables repteated value compression.
    /// </summary>
    ///
    /// <remarks>
    /// This declaration is not needed for round-trip C# serialization, however it formats the
    /// USD file so that it can be rendered outside of C#.
    /// </remarks>
    public class VertexDataAttribute : Attribute
    {
        public static pxr.TfToken Interpolation = new pxr.TfToken("vertex");

        private int m_elementSize;

        public int ElementSize
        {
            get { return m_elementSize; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("elementSize must be greater than zero");
                }
                m_elementSize = value;
            }
        }

        public VertexDataAttribute()
        {
            ElementSize = 1;
        }

        public VertexDataAttribute(int elementSize)
        {
            ElementSize = elementSize;
        }
    }

    /// <summary>
    /// Fused displayColor and displayOpacity.
    /// </summary>
    /// <remarks>
    /// This special case is entirely for performance. The cost of iterating over display color and
    /// opacity in C# to fuse them back into a single vector is too great, this avoids that cost.
    /// </remarks>
    public class FusedDisplayColorAttribute : Attribute
    {
        public FusedDisplayColorAttribute()
        {
        }
    }

    /// <summary>
    /// Indicates that this is a USD transform should should be fused into a single GfMatrix4d.
    /// </summary>
    /// <remarks>
    /// This special case is for performance and simplicity. Transforms in USD can be composed of an
    /// arbitrary number of component (translate/rotate/scale) operations, however this API is
    /// designed for ease of use and performance. In this case, all component operations are
    /// collapsed into a single 4x4 matrix.
    /// </remarks>
    public class FusedTransformAttribute : Attribute
    {
        public FusedTransformAttribute()
        {
        }
    }


    /// <summary>
    /// Indicates that no namespace should be added to this field.
    /// </summary>
    /// <remarks>
    /// The serialization code typically adds an extra namespace to dictionary fields.
    /// If you want to hold multiple primvars in a dictionary without adding the extra namespace
    /// you should add this attribute to your field.
    /// </remarks>
    public class ForceNoNamespaceAttribute : Attribute
    {
        public ForceNoNamespaceAttribute()
        {
        }
    }
}

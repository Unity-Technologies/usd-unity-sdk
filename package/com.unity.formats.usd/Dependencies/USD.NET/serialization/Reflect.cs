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
using System.Reflection;

namespace USD.NET
{
    /// <summary>
    /// Helper class for reflection operations.
    ///
    /// Types are not expected to change at runtime, so reflection queries are cached internally as
    /// a performance optimization.
    /// </summary>
    internal class Reflect
    {
        // ----------------------------------------------------------------------------------------- //
        // Public API
        // ----------------------------------------------------------------------------------------- //

        /// <summary>
        /// Constructs and caches a vector of PropertyInfo for reflection. The caching of these values
        /// avoides garbage and any associated overhead with retrieving and filtering the property list.
        /// </summary>
        /// <param name="type">The type from which to extract PeropertyInfo.</param>
        /// <returns>A vector of PeropertyInfo for the given type.</returns>
        public static PropertyInfo[] GetCachedProperties(Type type)
        {
            PropertyInfo[] pi;
            lock (propertyInfoCache) {
                if (!propertyInfoCache.TryGetValue(type, out pi))
                {
                    pi = type.GetProperties(GetPublicBindingFlags());
                    propertyInfoCache[type] = pi;
                }
            }
            return pi;
        }

        /// <summary>
        /// Constructs and caches a vector of FieldInfo for reflection. The caching of these values
        /// avoides garbage and any associated overhead with retrieving and filtering the field list.
        /// </summary>
        /// <param name="type">The type from which to extract FieldInfo.</param>
        /// <returns>A vector of FieldInfo for the given type.</returns>
        public static FieldInfo[] GetCachedFields(Type type)
        {
            FieldInfo[] fi;
            lock (fieldInfoCache) {
                if (!fieldInfoCache.TryGetValue(type, out fi))
                {
                    fi = type.GetFields(GetPublicBindingFlags());
                    fieldInfoCache[type] = fi;
                }
            }
            return fi;
        }

        /// <summary>
        /// Indicates the variability of the member. Varying indicates the member may store multiple
        /// values over time, Uniform indicates the member has one value for all time.
        /// </summary>
        public static pxr.SdfVariability GetVariability(MemberInfo info)
        {
            return GetCacheEntry(info).sdfVariability;
        }

        /// <summary>
        /// Primvars in USD (and RenderMan) are a generalization of vertex attributes. The can be
        /// constant (one value per object), per vertex, per face, or per-vertex-per-face. The use
        /// here is limited to per vertex only.
        /// </summary>
        public static bool IsPrimvar(MemberInfo info)
        {
            return GetCacheEntry(info).isPrimvar;
        }

        /// <summary>
        /// When this member info represents a Primvar (see IsPrimvar), the element size indicates the
        /// number of array elements to be aggregated per element on the primitive surface.
        /// </summary>
        public static int GetPrimvarElementSize(MemberInfo info)
        {
            return GetCacheEntry(info).primvarElementSize;
        }

        /// <summary>
        /// Indicates DisplayColor and DisplayOpacity have been fused into a single object in Unity.
        /// </summary>
        public static bool IsFusedDisplayColor(MemberInfo info)
        {
            return GetCacheEntry(info).isFusedDisplayColor;
        }

        /// <summary>
        /// Indicates that the attribute represents a USD transform and may consist of many component
        /// T/S/R operations, which should be automatically collapsed into a single matrix.
        /// </summary>
        public static bool IsFusedTransform(MemberInfo info)
        {
            return GetCacheEntry(info).isFusedTransform;
        }

        /// <summary>
        /// Returns true if the MemberInfo is intened to be serialized as metadata in the custom data
        /// field.
        /// </summary>
        public static bool IsCustomData(MemberInfo info)
        {
            return GetCacheEntry(info).isCustomData;
        }

        /// <summary>
        /// Returns true if the MemberInfo is intened to be serialized as metadata.
        /// </summary>
        public static bool IsMetadata(MemberInfo info)
        {
            return GetCacheEntry(info).isMetadata;
        }

        /// <summary>
        /// Returns true if the MemberInfo is inteneded to represent a relationship.
        /// </summary>
        public static bool IsRelationship(MemberInfo info)
        {
            return GetCacheEntry(info).isRelationship;
        }

        /// <summary>
        /// Returns true if the MemberInfo is inteneded to represent an SdfAssetPath.
        /// </summary>
        public static bool IsAssetPath(MemberInfo info)
        {
            return GetCacheEntry(info).isAssetPath;
        }

        /// <summary>
        /// Indicates the member should not be serialized.
        /// </summary>
        public static bool IsNonSerialized(MemberInfo info)
        {
            return GetCacheEntry(info).isNonSerialized;
        }

        /// <summary>
        /// In USD, properties can have one or more namespaces, which manifest as a colon delimited
        /// prefix, such as foo:bar:points, where "foo:bar" is the namespace and "points is the
        /// property name.
        ///
        /// Serialization uses namespaces to in-line structures and containers, for example when
        /// serializing a Dictionary,
        /// </summary>
        public static string GetNamespace(MemberInfo info)
        {
            return GetCacheEntry(info).usdNamespace;
        }

        /// <summary>
        /// Returns the USD Schema associated with the given classType.
        /// </summary>
        /// <param name="classType">The object type to be serialized</param>
        /// <returns>The name of the USD schema.</returns>
        public static string GetSchema(Type classType)
        {
            return GetPrimCacheEntry(classType).usdSchema;
        }

        /// <summary>
        /// Returns true if the member should not be namespaced
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool ForceNoNamespace(MemberInfo info)
        {
            return GetCacheEntry(info).forceNoNamespace;
        }

        /// <summary>
        /// Visits all properties and fields which are an Array type and returns the held array value,
        /// if not null. Sets each extracted value to null on the object.
        /// </summary>
        public static IEnumerable<Array> ExtractArrays<T>(T obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(GetPublicBindingFlags());

            foreach (PropertyInfo csProp in properties)
            {
                if (csProp.PropertyType.IsArray)
                {
                    Array array = (Array)csProp.GetValue(obj, index: null);
                    if (array == null) { continue; }
                    yield return array;
                    csProp.SetValue(obj, null, index: null);
                }
            }

            FieldInfo[] fields = type.GetFields(GetPublicBindingFlags());
            foreach (FieldInfo csField in fields)
            {
                if (csField.FieldType.IsArray)
                {
                    Array array = (Array)csField.GetValue(obj);
                    if (array == null) { continue; }
                    yield return array;
                    csField.SetValue(obj, null);
                }
                else if (csField.FieldType == typeof(object))
                {
                    Array array = (Array)csField.GetValue(obj);
                    if (array == null) { continue; }
                    yield return array;
                    csField.SetValue(obj, null);
                }
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // Reflection InfoCache
        // ----------------------------------------------------------------------------------------- //

        static Dictionary<MemberInfo, InfoEntry> infoCache = new Dictionary<MemberInfo, InfoEntry>();
        static Dictionary<Type, PrimInfoEntry> primInfoCache = new Dictionary<Type, PrimInfoEntry>();
        static Dictionary<Type, PropertyInfo[]> propertyInfoCache = new Dictionary<Type, PropertyInfo[]>();
        static Dictionary<Type, FieldInfo[]> fieldInfoCache = new Dictionary<Type, FieldInfo[]>();

        /// <summary>
        /// A struct of relevant metadata synthesized from member attributes.
        /// </summary>
        private struct InfoEntry
        {
            public bool isPrimvar;
            public int primvarElementSize;
            public bool isCustomData;
            public bool isMetadata;
            public pxr.SdfVariability sdfVariability;
            public bool isNonSerialized;
            public string usdNamespace;
            public bool isFusedDisplayColor;
            public bool isFusedTransform;
            public bool isRelationship;
            public bool isAssetPath;
            public bool forceNoNamespace;
        }

        /// <summary>
        /// A struct of relevant UsdPrim metadata synthesized from attributes.
        /// </summary>
        private struct PrimInfoEntry
        {
            public string usdSchema;
        }

        public static BindingFlags GetPublicBindingFlags()
        {
            return BindingFlags.Public | BindingFlags.Instance;
        }

        /// <summary>
        /// Returns the cache entry for the given member info. If the cache entry doesn't exist,
        /// the entry for the info is populated on demand.
        /// </summary>
        private static InfoEntry GetCacheEntry(MemberInfo info)
        {
            InfoEntry cachedInfo = new InfoEntry();
            lock (infoCache) {
                if (infoCache.TryGetValue(info, out cachedInfo))
                {
                    return cachedInfo;
                }
            }

            //
            // SdfVariability
            //
            var attrs = (UsdVariabilityAttribute[])info.GetCustomAttributes(
                typeof(UsdVariabilityAttribute), true);
            cachedInfo.sdfVariability = attrs.Length == 0
                ? pxr.SdfVariability.SdfVariabilityVarying
                : attrs[0].SdfVariability;

            //
            // IsPrimvar
            //
            var attrs2 = (VertexDataAttribute[])info.
                GetCustomAttributes(typeof(VertexDataAttribute), true);
            cachedInfo.isPrimvar = attrs2.Length > 0;
            cachedInfo.primvarElementSize = cachedInfo.isPrimvar ? attrs2[0].ElementSize : 1;

            //
            // IsFusedDisplayColor
            //
            var attrs7 = (FusedDisplayColorAttribute[])info.
                GetCustomAttributes(typeof(FusedDisplayColorAttribute), true);
            cachedInfo.isFusedDisplayColor = attrs7.Length > 0;

            //
            // IsFusedTransform
            //
            var attrs8 = (FusedTransformAttribute[])info.
                GetCustomAttributes(typeof(FusedTransformAttribute), true);
            cachedInfo.isFusedTransform = attrs8.Length > 0;

            //
            // IsCustomData
            //
            var attrs6 = (CustomDataAttribute[])info.
                GetCustomAttributes(typeof(CustomDataAttribute), true);
            cachedInfo.isCustomData = attrs6.Length > 0;

            //
            // IsMetaData
            //
            var attrs9 = (UsdMetadataAttribute[])info.
                GetCustomAttributes(typeof(UsdMetadataAttribute), true);
            cachedInfo.isMetadata = attrs9.Length > 0;

            //
            // IsNonSerialized
            //
            var attrs3 = (NonSerializedAttribute[])info.
                GetCustomAttributes(typeof(NonSerializedAttribute), true);
            cachedInfo.isNonSerialized = attrs3.Length > 0;

            //
            // UsdNamespace
            //
            var attrs4 = (UsdNamespaceAttribute[])info.
                GetCustomAttributes(typeof(UsdNamespaceAttribute), true);
            cachedInfo.usdNamespace = attrs4.Length == 0 ? "" : attrs4[0].Name;

            //
            // IsRelationship
            //
            var attrs11 = (UsdRelationshipAttribute[])info.
                GetCustomAttributes(typeof(UsdRelationshipAttribute), true);
            cachedInfo.isRelationship = attrs9.Length > 0;

            //
            // IsAssetPath
            //
            var attrs10 = (UsdAssetPathAttribute[])info.
                GetCustomAttributes(typeof(UsdAssetPathAttribute), true);
            cachedInfo.isAssetPath = attrs10.Length > 0;

            // ForceNoNamespace
            var attrs12 = (ForceNoNamespaceAttribute[])info.GetCustomAttributes(typeof(ForceNoNamespaceAttribute), true);
            cachedInfo.forceNoNamespace = attrs12.Length > 0;
            //
            // Cache & return.
            //
            lock (infoCache) {
                infoCache[info] = cachedInfo;
            }
            return cachedInfo;
        }

        /// <summary>
        /// Returns the Prim cache entry for the given type. If the cache entry doesn't exist,
        /// the entry for the info is populated on demand.
        /// </summary>
        private static PrimInfoEntry GetPrimCacheEntry(Type type)
        {
            PrimInfoEntry cachedPrimInfo = new PrimInfoEntry();
            if (primInfoCache.TryGetValue(type, out cachedPrimInfo))
            {
                return cachedPrimInfo;
            }

            //
            // UsdSchema
            //
            var attrs5 = (UsdSchemaAttribute[])type.
                GetCustomAttributes(typeof(UsdSchemaAttribute), true);
            cachedPrimInfo.usdSchema = attrs5.Length == 0 ? "" : attrs5[0].Name;

            //
            // Cache & return.
            //
            primInfoCache[type] = cachedPrimInfo;
            return cachedPrimInfo;
        }
    }
}

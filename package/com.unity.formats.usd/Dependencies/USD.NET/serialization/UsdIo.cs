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
using pxr;

namespace USD.NET
{
    /// <summary>
    /// The USD serialization run-time. This class leverages the ArrayAllocator, TypeBinder,
    /// TokenCache, ReflectionCache and SampleBase to perform efficient reflection based serialization
    /// of arbitrary C# data types to and from USD.
    /// </summary>
    public class UsdIo
    {
        // Static copy of UsdTime::Default() to avoid reconstructing it on every use.
        static readonly pxr.UsdTimeCode kDefaultUsdTime = pxr.UsdTimeCode.Default();

        // XXX: Global allocator state is not awesome, this should be threaded through the API, though
        // that's not awesome in a different way.
        // TODO(jcowles): fix this static initialization mess -- ArrayAllocator must be declared before
        // TypeBinder below, due to transitive dependency in TypeBinder constructor.
        public static ArrayPool ArrayAllocator = new ArrayUnpool();

        // Avoid churning through tokens to avoid the overhead of both p/invoke and garbage generation.
        // TODO: This has detremental effect on multi-threaded performance, making it thread-specific
        //       would make it more efficient.
        static TokenCache sm_tokenCache = new TokenCache();

        // Data driven type bindings allow clients to add bindings for new, custom data types.
        // TODO: Static initialization hides library-level configuration exceptions, for example when a
        //       DLL cannot be found. The error manifests as TypeBinder failed to initialize, which is
        //       not ideal since the error is confusing until this is understood. It would be preferable
        //       to trigger this initialization based on a user action, so the library initialization
        //       errors would surface.
        static TypeBinder sm_bindings = new TypeBinder();

        /// <summary>
        /// Provides access to the system TypeBinder. Only one binder may be active and the binder can
        /// be used to add support for new custom data types.
        /// </summary>
        public static TypeBinder Bindings { get { return sm_bindings; } }

        // A lock used to protect the stage from multi-threaded writes.
        private object m_stageLock;

        // Sparse value writer that keeps a map of UsdAttributes to UsdUtilsSparseAttrValueWriter objects.
        // Used when setting time samples, to avoid writing redundant values.
        private pxr.UsdUtilsSparseValueWriter m_sparseValueWriter;

        public UsdIo(object stageLock)
        {
            m_stageLock = stageLock;
            m_sparseValueWriter = new pxr.UsdUtilsSparseValueWriter();
        }

        /// <summary>
        /// Serializes an arbitrary object descending from SampleBase from C# to USD.
        /// </summary>
        /// <typeparam name="T">Any type which inherits from SampleBase</typeparam>
        /// <param name="t">The object/data to be serialized.</param>
        /// <param name="prim">The UsdPrim to which the object should be written.</param>
        /// <param name="usdTime">The tiem at which key frames should be created.</param>
        /// <param name="usdNamespace">The USD namespace (if any) of the object.</param>
        public void Serialize<T>(T t,
            pxr.UsdPrim prim,
            pxr.UsdTimeCode usdTime,
            string usdNamespace = null)
        {
            PropertyInfo[] properties = Reflect.GetCachedProperties(t.GetType());
            FieldInfo[] fields = Reflect.GetCachedFields(t.GetType());
            var imgble = new pxr.UsdGeomImageable(prim);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo csProp = properties[i];
                Type csType = csProp.PropertyType;
                if (csType == typeof(object))
                {
                    if (Reflect.IsCustomData(csProp) || Reflect.IsMetadata(csProp))
                    {
                        throw new ArgumentException("Writing metadata/customdata with type of object is not currently allowed");
                    }
                    object o = csProp.GetValue(t, index: null);
                    if (o != null)
                    {
                        csType = o.GetType();
                    }
                }
                if (!WriteAttr(csProp.Name, csType, csProp.GetValue(t, index:null),
                    usdTime, prim, imgble, csProp, usdNamespace))
                {
                    // TODO: add options to dictate behavior here
                }
            }

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo csField = fields[i];
                Type csType = csField.FieldType;
                if (csType == typeof(object))
                {
                    if (Reflect.IsCustomData(csField) || Reflect.IsMetadata(csField))
                    {
                        throw new ArgumentException("Writing metadata/customdata with type of object is not currently allowed");
                    }
                    object o = csField.GetValue(t);
                    if (o != null)
                    {
                        csType = o.GetType();
                    }
                }
                if (!WriteAttr(csField.Name, csType, csField.GetValue(t),
                    usdTime, prim, imgble, csField, usdNamespace))
                {
                    // TODO: add options to dictate behavior here
                }
            }
        }

        /// <summary>
        /// Deserialize a single field to a single value.
        /// </summary>
        /// <param name="fieldValue">The referenced value of the field to populate.</param>
        /// <param name="prim">The USD prim from which to read the value.</param>
        /// <param name="usdTime">The time at which to sample key frames.</param>
        /// <param name="field">The field to deserialize.</param>
        /// <param name="accessMap">A list of members to include when reading.</param>
        /// <param name="mayVary">Indicates if any of the members read from this prim were time-varying.</param>
        /// <param name="usdNamespace">The USD namespace, if any.</param>
        public void Deserialize(ref object fieldValue,
            pxr.UsdPrim prim,
            pxr.UsdTimeCode usdTime,
            FieldInfo field,
            HashSet<MemberInfo> accessMap,
            ref bool? mayVary,
            string usdNamespace = null)
        {
            if (Reflect.IsNonSerialized(field))
            {
                return;
            }

            if (!ReadAttr(field.Name,
                field.FieldType,
                ref fieldValue,
                usdTime,
                prim,
                field,
                accessMap,
                ref mayVary,
                usdNamespace))
            {
                // TODO: add options to dictate behavior here
            }
        }

        /// <summary>
        /// Deserialize a single property to a single value.
        /// </summary>
        /// <param name="propValue">The referenced value of the property to populate.</param>
        /// <param name="prim">The USD prim from which to read the value.</param>
        /// <param name="usdTime">The time at which to sample key frames.</param>
        /// <param name="field">The field to deserialize.</param>
        /// <param name="accessMap">A list of members to includ when reading.</param>
        /// <param name="mayVary">If non-null, is populated to indicate if the value varies over time.</param>
        /// <param name="usdNamespace">The USD namespace, if any.</param>
        public void Deserialize(ref object propValue,
            pxr.UsdPrim prim,
            pxr.UsdTimeCode usdTime,
            PropertyInfo field,
            HashSet<MemberInfo> accessMap,
            ref bool? mayVary,
            string usdNamespace = null)
        {
            if (Reflect.IsNonSerialized(field))
            {
                return;
            }

            if (!ReadAttr(field.Name,
                field.PropertyType,
                ref propValue,
                usdTime,
                prim,
                field,
                accessMap,
                ref mayVary,
                usdNamespace))
            {
                // TODO: add options to dictate behavior here
            }
        }

        /// <summary>
        /// Deserializes an arbitrary object descending from SampleBase from USD to C#.
        /// </summary>
        /// <typeparam name="T">The type to serialize, descending from SampleBase</typeparam>
        /// <param name="t">The object to to populate.</param>
        /// <param name="prim">The USD prim from which to read data.</param>
        /// <param name="usdTime">The time at which to read key frames.</param>
        /// <param name="accessMap">A list of memebers to include when reading.</param>
        /// <param name="mayVary">Indicates if this prim had any time-varying members.</param>
        /// <param name="usdNamespace">The object namespace, if any.</param>
        public void Deserialize<T>(T t,
            pxr.UsdPrim prim,
            pxr.UsdTimeCode usdTime,
            HashSet<MemberInfo> accessMap,
            ref bool? mayVary,
            string usdNamespace = null) where T : SampleBase
        {
            if (t == null)
            {
                return;
            }

            PropertyInfo[] properties = Reflect.GetCachedProperties(t.GetType());
            FieldInfo[] fields = Reflect.GetCachedFields(t.GetType());
            var localVarMap = accessMap;
            bool mayVaryWasNull = mayVary == null;
            if (mayVary == null)
            {
                localVarMap = null;
            }

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo csProp = properties[i];
                if (Reflect.IsNonSerialized(csProp))
                {
                    continue;
                }
                if (accessMap != null && mayVary == null && !accessMap.Contains(csProp))
                {
                    continue;
                }
                object propValue = csProp.GetValue(t, null);
                Deserialize(ref propValue, prim, usdTime, csProp, localVarMap, ref mayVary, usdNamespace);
                csProp.SetValue(t, propValue, index: null);
                if ((mayVary == null) != mayVaryWasNull)
                {
                    throw new ApplicationException("Deserialize modified mayVary to be non-null");
                }
            }

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo csField = fields[i];
                if (Reflect.IsNonSerialized(csField))
                {
                    continue;
                }
                if (accessMap != null && mayVary == null && !accessMap.Contains(csField))
                {
                    continue;
                }
                object fieldValue = csField.GetValue(t);
                Deserialize(ref fieldValue, prim, usdTime, csField, localVarMap, ref mayVary, usdNamespace);
                csField.SetValue(t, fieldValue);
                if ((mayVary == null) != mayVaryWasNull)
                {
                    throw new ApplicationException("Deserialize modified mayVary to be non-null");
                }
            }
        }

        /// <summary>
        /// Internal helper for serializing data to USD.
        /// </summary>
        /// <param name="attrName">The USD attribute name.</param>
        /// <param name="csType">The C# type.</param>
        /// <param name="csValue">The C# value.</param>
        /// <param name="usdTime">The time at which to sample key frames.</param>
        /// <param name="prim">The USD prim at which to write values.</param>
        /// <param name="imgble">The UsdGeomImagable attrbiute, used when writing PrimVars.</param>
        /// <param name="memberInfo">The field/property providing serialization metadata.</param>
        /// <param name="usdNamespace">The optional USD namespace at which values live.</param>
        /// <param name="srcObject">The source object name, used when remapping names.</param>
        /// <returns>True on success.</returns>
        /// <remarks>
        /// Note that "success" in the return value does not indicate data was written, rather it
        /// indicates that no unexpected states were encountered. E.g. calling WriteAttr on a field
        /// marked as [NotSerialized] does not cause this method to return false, since non-serialized
        /// fields are an expected state this function may encounter.
        /// </remarks>
        bool WriteAttr(string attrName, Type csType, object csValue, pxr.UsdTimeCode usdTime,
            pxr.UsdPrim prim, pxr.UsdGeomImageable imgble, MemberInfo memberInfo,
            string usdNamespace, string srcObject = null)
        {
            if (Reflect.IsNonSerialized(memberInfo))
            {
                Console.WriteLine("Non serialized");
                return true;
            }

            // If serializing a Primvar<T>, extract the held value and save it in csValue, allowing the
            // all downstream logic to act as if it's operating on the held value itself.
            PrimvarBase pvBase = null;

            if (csType.IsGenericType && csType.GetGenericTypeDefinition() == typeof(Primvar<>))
            {
                if (csValue == null)
                {
                    // Object not written, still considered success.
                    return true;
                }

                pvBase = (PrimvarBase)csValue;
                csValue = (csValue as ValueAccessor).GetValue();
                if (csValue == null)
                {
                    // Object not written, still considered success.
                    return true;
                }

                csType = csValue.GetType();
            }

            bool isCustomData = Reflect.IsCustomData(memberInfo);
            bool isMetaData = Reflect.IsMetadata(memberInfo);
            bool isPrimvar = Reflect.IsPrimvar(memberInfo);
            bool isNewPrimvar = pvBase != null;
            int primvarElementSize = Reflect.GetPrimvarElementSize(memberInfo);

            string ns = IntrinsicTypeConverter.JoinNamespace(usdNamespace,
                Reflect.GetNamespace(memberInfo));

            // If holding a dictionary, immediately recurse and write keys as attributes.
            if (csValue != null
                && csType.IsGenericType
                && csType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && csType.GetGenericArguments()[0] == typeof(string))
            {
                isNewPrimvar = csType.GetGenericArguments()[1].IsGenericType
                    && csType.GetGenericArguments()[1].GetGenericTypeDefinition() == typeof(Primvar<>);

                // Ensure the immediate dictionary member is always namespaced.
                if (!Reflect.ForceNoNamespace(memberInfo) && string.IsNullOrEmpty(Reflect.GetNamespace(memberInfo)))
                {
                    usdNamespace = IntrinsicTypeConverter.JoinNamespace(usdNamespace, attrName);
                }

                var dict = csValue as System.Collections.IDictionary;
                foreach (System.Collections.DictionaryEntry kvp in dict)
                {
                    object value = kvp.Value;
                    WriteAttr((string)kvp.Key, value.GetType(), value,
                        usdTime, prim, imgble, memberInfo, usdNamespace, srcObject: attrName);
                }
                return true;
            }

            pxr.TfToken sdfAttrName = sm_tokenCache[attrName];

            if (csType == typeof(Relationship) && csValue != null)
            {
                string[] targetStrings = ((Relationship)csValue).targetPaths;
                if (targetStrings != null)
                {
                    //
                    // Write Relationship
                    //
                    string[] arr = IntrinsicTypeConverter.JoinNamespace(ns, sdfAttrName).Split(':');
                    pxr.StdStringVector elts = new pxr.StdStringVector(arr.Length);
                    foreach (var s in arr)
                    {
                        elts.Add(s);
                    }

                    pxr.UsdRelationship rel = null;
                    lock (m_stageLock) {
                        rel = prim.CreateRelationship(elts, custom: false);
                    }

                    if (!rel.IsValid())
                    {
                        throw new ApplicationException("Failed to create relationship <"
                            + prim.GetPath().AppendProperty(
                                new pxr.TfToken(
                                    IntrinsicTypeConverter.JoinNamespace(ns, sdfAttrName))).ToString() + ">");
                    }

                    var targets = new pxr.SdfPathVector();
                    foreach (var path in ((Relationship)csValue).targetPaths)
                    {
                        targets.Add(new pxr.SdfPath(path));
                    }
                    lock (m_stageLock) {
                        rel.SetTargets(targets);
                    }
                }
                return true;
            }

            //
            // Write Attribute
            //

            // FUTURE: When writing sparse overrides, if the csValue is null exit here and avoid
            // defining the target attribute. However, sparse authoring is not yet supported.

            UsdTypeBinding binding;

            // Extract the value and type from the connectable.
            var conn = csValue as Connectable;
            if (conn != null)
            {
                csType = conn.GetValueType();
                csValue = conn.GetValue();
            }

            // Get the binding for the value about to be serialized.
            if (!sm_bindings.GetBinding(csType, out binding) && !csType.IsEnum)
            {
                if (csValue == null)
                {
                    return true;
                }

                if (string.IsNullOrEmpty(ns))
                {
                    return false;
                }

                var sample = csValue as SampleBase;
                if (sample == null && csValue != null)
                {
                    throw new ArgumentException("Type does not inherit from SampleBase: " + attrName);
                }

                Serialize(csValue, prim, usdTime, usdNamespace: ns);
                return true;
            }

            // Determine metadata for the attribtue, note that in the case of connections and primvars
            // these will be the attributes on the outter object, e.g. declared on the Connection<T> or
            // Primvar<T>.
            pxr.SdfVariability variability = Reflect.GetVariability(memberInfo);
            pxr.SdfValueTypeName sdfTypeName = binding.sdfTypeName;
            pxr.UsdTimeCode time = variability == pxr.SdfVariability.SdfVariabilityUniform
                ? pxr.UsdTimeCode.Default()
                : usdTime;

            bool custom = false;
            pxr.UsdAttribute attr;
            if (isCustomData || isMetaData)
            {
                // no-op
                attr = null;
            }
            else if (!isPrimvar && !isNewPrimvar)
            {
                if (string.IsNullOrEmpty(ns))
                {
                    //
                    // Create non-namespaced attribute.
                    //
                    lock (m_stageLock) {
                        attr = prim.CreateAttribute(sdfAttrName, csType.IsEnum ? SdfValueTypeNames.Token : sdfTypeName, custom, variability);
                    }
                }
                else
                {
                    //
                    // Create namespaced attribute.
                    //
                    string[] arr = IntrinsicTypeConverter.JoinNamespace(ns, sdfAttrName).Split(':');
                    pxr.StdStringVector elts = new pxr.StdStringVector(arr.Length);
                    foreach (var s in arr)
                    {
                        elts.Add(s);
                    }
                    lock (m_stageLock) {
                        attr = prim.CreateAttribute(elts, sdfTypeName, custom, variability);
                    }
                }
            }
            else
            {
                //
                // Create Primvar attribute.
                //
                lock (m_stageLock) {
                    var fullAttrName = IntrinsicTypeConverter.JoinNamespace(ns, sdfAttrName);
                    var primvar = imgble.CreatePrimvar(new pxr.TfToken(fullAttrName), sdfTypeName,
                        VertexDataAttribute.Interpolation);
                    if (isNewPrimvar)
                    {
                        primvar.SetElementSize(pvBase.elementSize);
                        if (pvBase.indices != null)
                        {
                            var vtIndices = IntrinsicTypeConverter.ToVtArray(pvBase.indices);
                            primvar.SetIndices(vtIndices, time);
                        }
                        primvar.SetInterpolation(pvBase.GetInterpolationToken());
                    }
                    else
                    {
                        primvar.SetElementSize(primvarElementSize);
                    }
                    attr = primvar.GetAttr();
                }
            }

            if (attr != null && conn != null && conn.GetConnectedPath() != null)
            {
                // TODO: Pool temp vector, possibly add a single item overload for SetConnections.
                var paths = new pxr.SdfPathVector();
                var connPath = conn.GetConnectedPath();
                if (connPath != string.Empty)
                {
                    paths.Add(new pxr.SdfPath(conn.GetConnectedPath()));
                }
                attr.SetConnections(paths);
            }

            // This may happen when a connection is present, but has a null default value.
            // Because the connection is applied just before this point, this is the earliest possible
            // exit point.
            if (csValue == null)
            {
                return true;
            }


            pxr.VtValue vtValue = binding.toVtValue(csValue);
            lock (m_stageLock) {
                if (isMetaData)
                {
                    prim.SetMetadata(sdfAttrName, vtValue);
                }
                else if (isCustomData)
                {
                    prim.SetCustomDataByKey(sdfAttrName, vtValue);
                }
                else if (Reflect.IsFusedDisplayColor(memberInfo))
                {
                    pxr.UsdCs.SetFusedDisplayColor(prim, vtValue, time);
                }
                else
                {
                    // use the sparse attribute value writer, to skip redundant time samples
                    m_sparseValueWriter.SetAttribute(attr, vtValue, time);
                }
            }

            if (!isCustomData && srcObject != null)
            {
                lock (m_stageLock) {
                    attr.SetCustomDataByKey(sm_tokenCache["sourceMember"], srcObject);
                }
            }
            return true;
        }

        /// <summary>
        /// Internal helper for reading data from USD.
        /// </summary>
        /// <param name="attrName">The USD attribute name.</param>
        /// <param name="csType">The C# type.</param>
        /// <param name="csValue">The C# value to populate.</param>
        /// <param name="usdTime">The time at which to sample key frames.</param>
        /// <param name="prim">The USD prim from which to read data.</param>
        /// <param name="memberInfo">The field/property providing serialization metadata.</param>
        /// <param name="usdNamespace">The optional USD namespace at which values live.</param>
        /// <param name="accessMap">A map of members to include when reading.</param>
        /// <param name="mayVary">When not null, is populated with variability.</param>
        /// <returns>True on success.</returns>
        /// <remarks>
        /// Note that "success" in the return value does not indicate data was read, rather it
        /// indicates that no unexpected states were encountered. E.g. calling ReadAttr on a field
        /// with no value stored in USD will not return false, since that is not considered a failure
        /// state.
        /// </remarks>
        bool ReadAttr(string attrName, Type csType, ref object csValue, pxr.UsdTimeCode usdTime,
            pxr.UsdPrim prim, MemberInfo memberInfo,
            HashSet<MemberInfo> accessMap, ref bool? mayVary,
            string usdNamespace)
        {
            bool isNewPrimvar = csValue != null
                && csType.IsGenericType
                && csType.GetGenericTypeDefinition() == typeof(Primvar<>);  // This is true for Primvar type only
            bool isPrimvar = Reflect.IsPrimvar(memberInfo) || isNewPrimvar; // This is true for VertexData + Primvar type...
            string namespaceToRead = IntrinsicTypeConverter.JoinNamespace(usdNamespace,
                Reflect.GetNamespace(memberInfo));


            // ----------------------------------------- //
            // Dictionaries, read, early exit, recurse.
            // ----------------------------------------- //
            // If holding a dictionary, immediately recurse and write keys as attributes.
            if (csValue != null
                && csType.IsGenericType
                && csType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && csType.GetGenericArguments()[0] == typeof(string))
            {
                Type genericTypeDef = csType.GetGenericArguments()[1].IsGenericType
                    ? csType.GetGenericArguments()[1].GetGenericTypeDefinition()
                    : null;

                isNewPrimvar = genericTypeDef == typeof(Primvar<>);
                bool isRelationship = csType.GetGenericArguments()[1] == typeof(Relationship);
                bool isConnection = genericTypeDef == typeof(Connectable<>);

                // String dictionaries are unrolled directly into the object.
                // So the namespace is either the incoming namespace or empty, meaning each string value in
                // the dictionary becomes an attribute on the prim.

                // Ensure there is always a namespace immediately around this member.
                if (!Reflect.ForceNoNamespace(memberInfo) && string.IsNullOrEmpty(Reflect.GetNamespace(memberInfo)))
                {
                    namespaceToRead = IntrinsicTypeConverter.JoinNamespace(namespaceToRead, attrName);
                    usdNamespace = IntrinsicTypeConverter.JoinNamespace(usdNamespace, attrName);
                }

                // Unfortunately, the primvars prefixing logic must be replicated here so we can discover
                // the dictionary member from USD.
                if (isPrimvar || isNewPrimvar)
                {
                    namespaceToRead = IntrinsicTypeConverter.JoinNamespace("primvars", namespaceToRead);
                }

                var dict = csValue as System.Collections.IDictionary;
                ConstructorInfo ctor = (isNewPrimvar || isConnection || isRelationship)
                    ? csType.GetGenericArguments()[1].GetConstructor(new Type[0])
                    : null;
                dict.Clear();
                foreach (var prop in prim.GetAuthoredPropertiesInNamespace(namespaceToRead))
                {
                    object value = null;
                    if (ctor != null)
                    {
                        value = ctor.Invoke(new object[0]);
                    }
                    // The recursive call will also discover that this is a primvar and any associated namespace.
                    if (ReadAttr(prop.GetBaseName(),
                        csType.GetGenericArguments()[1],
                        ref value,
                        usdTime,
                        prim,
                        memberInfo,
                        accessMap,
                        ref mayVary,
                        usdNamespace))
                    {
                        if (value != null)
                        {
                            // trim off the dictionary namespace - keep the rest of the namespace to avoid key collisions
                            var fullName = prop.GetName().ToString();
                            dict.Add(fullName.Substring(namespaceToRead.Length + 1), value);
                        }
                    }
                }
                return true;
            }

            pxr.TfToken sdfAttrName = sm_tokenCache[namespaceToRead, attrName];

            // ----------------------------------------- //
            // Relationship, read + early exit.
            // ----------------------------------------- //

            if (csType == typeof(Relationship))
            {
                // mayVary is explicitly not set here because it has accumulation semantics:
                //   mayVary = mayVary || false;
                // Which is equivalent to the no-op:
                //   mayVary = mayVary;

                pxr.UsdRelationship rel = null;
                lock (m_stageLock) {
                    rel = prim.GetRelationship(sm_tokenCache[sdfAttrName]);
                }

                var relationship = new Relationship();
                csValue = relationship;

                if (rel == null || !rel.IsValid())
                {
                    return true;
                }

                pxr.SdfPathVector paths = rel.GetTargets();
                string[] result = new string[paths.Count];
                for (int i = 0; i < paths.Count; i++)
                {
                    result[i] = paths[i].ToString();
                }

                relationship.targetPaths = result;
                return true;
            }

            // ----------------------------------------- //
            // Connection Setup.
            // ----------------------------------------- //

            Connectable conn = null;
            if (csValue != null
                && csType.IsGenericType
                && csType.GetGenericTypeDefinition() == typeof(Connectable<>))
            {
                conn = csValue as Connectable;
                if (conn != null)
                {
                    // Since this is a Connectable<T>, the held value T is what's being read from USD,
                    // so replace csValue with the held T value itself. csValue must be restored before
                    // returning.
                    csValue = conn.GetValue();

                    // Same treatment for the type.
                    csType = conn.GetValueType();
                }
            }

            // ----------------------------------------- //
            // Primvar Setup.
            // ----------------------------------------- //

            ValueAccessor pvAccessor = null;
            PrimvarBase pvBase = null;
            if (isNewPrimvar)
            {
                pvAccessor = csValue as ValueAccessor;
                pvBase = (PrimvarBase)csValue;
                // Since this is a Primvar<T>, the held value T is what's being read from USD,
                // so replace csVAlue with the held T value itself. csValue must be restored before
                // returning.
                csValue = pvAccessor.GetValue();

                // Same treatment for the type.
                csType = pvAccessor.GetValueType();
            }

            // ----------------------------------------- //
            // Lookup Type Conversion Delegate.
            // ----------------------------------------- //
            UsdTypeBinding binding;

            if (!sm_bindings.GetBinding(csType, out binding)
                && !csType.IsEnum
                && csType != typeof(object))
            {
                if (string.IsNullOrEmpty(namespaceToRead))
                {
                    return false;
                }

                var sample = csValue as SampleBase;
                if (csValue == null)
                {
                    // This could attempt to automatically constuct the needed object, then nullable objects
                    // could be used instead to drive deserialization.
                    return false;
                }
                else if (sample == null)
                {
                    // In this case, csValue is not null, but also cannot be converted to SampleBase.
                    throw new ArgumentException("Type does not inherit from SampleBase: " + attrName);
                }

                Deserialize((SampleBase)csValue, prim, usdTime, accessMap, ref mayVary, usdNamespace: namespaceToRead);
                return true;
            }

            // ----------------------------------------- //
            // Prep to Read.
            // ----------------------------------------- //

            // Restore C# value to the actual property value.
            if (conn != null)
            {
                csValue = conn;
            }
            else if (pvAccessor != null)
            {
                csValue = pvAccessor;
            }

            // Append "primvars:" namespace to primvars.
            if (isPrimvar)
            {
                var joinedName = IntrinsicTypeConverter.JoinNamespace(namespaceToRead, attrName);
                sdfAttrName = sm_tokenCache["primvars", joinedName];
            }

            // Adjust time for variability.
            pxr.SdfVariability variability = Reflect.GetVariability(memberInfo);
            pxr.UsdTimeCode time = variability == pxr.SdfVariability.SdfVariabilityUniform
                ? kDefaultUsdTime
                : usdTime;

            // Allocate a temp VtValue.
            pxr.VtValue vtValue = (pxr.VtValue)ArrayAllocator.MallocHandle(typeof(pxr.VtValue));

            try
            {
                // ----------------------------------------- //
                // Read Connected Paths.
                // ----------------------------------------- //
                if (conn != null)
                {
                    // Connection paths cannot be animated, so mayVary is not affected.
                    var sources = new pxr.SdfPathVector();
                    if (prim.GetAttribute(sdfAttrName).GetConnections(sources))
                    {
                        if (sources.Count > 0)
                        {
                            conn.SetConnectedPath(sources[0].ToString());
                        }
                    }
                }

                // ----------------------------------------- //
                // Read Associated Primvar Data.
                // ----------------------------------------- //
                // If this is a Primvar<T>, read the associated primvar metadata and indices.
                if (pvBase != null)
                {
                    UsdAttribute attr = null;
                    if (Reflect.IsFusedDisplayColor(memberInfo))
                    {
                        var gprim = new pxr.UsdGeomGprim(prim);
                        if (gprim)
                            attr = gprim.GetDisplayColorAttr();
                    }
                    else
                    {
                        attr = prim.GetAttribute(sdfAttrName);
                    }

                    if (attr)
                    {
                        var pv = new pxr.UsdGeomPrimvar(attr);
                        // ElementSize and Interpolation are not animatable, so they do not affect mayVary.
                        pvBase.elementSize = pv.GetElementSize();
                        pvBase.SetInterpolationToken(pv.GetInterpolation());

                        // Primvars can be indexed and indices are a first class attribute and may vary over time.
                        var indices = pv.GetIndicesAttr();
                        if (indices)
                        {
                            if (accessMap != null)
                            {
                                if (indices.GetVariability() == pxr.SdfVariability.SdfVariabilityVarying
                                    && indices.ValueMightBeTimeVarying())
                                {
                                    accessMap.Add(memberInfo);
                                    mayVary |= true;
                                }
                            }
                            indices.Get(vtValue, time);
                            if (!vtValue.IsEmpty())
                            {
                                var vtIntArray = pxr.UsdCs.VtValueToVtIntArray(vtValue);
                                pvBase.indices = IntrinsicTypeConverter.FromVtArray(vtIntArray);
                            }
                        }
                    }
                }

                // ----------------------------------------- //
                // Read the value of csValue.
                // ----------------------------------------- //

                if (Reflect.IsMetadata(memberInfo))
                {
                    vtValue = prim.GetMetadata(sdfAttrName);
                    // Metadata cannot vary over time.
                }
                else if (Reflect.IsCustomData(memberInfo))
                {
                    vtValue = prim.GetCustomDataByKey(sdfAttrName);
                    // Custom data is metadata, which cannot vary over time.
                }
                else if (Reflect.IsFusedDisplayColor(memberInfo))
                {
                    vtValue = pxr.UsdCs.GetFusedDisplayColor(prim, time);

                    if (accessMap != null)
                    {
                        // Display color is actually two attributes, primvars:displayColor and
                        // primvars:displayOpacity.
                        var gprim = new pxr.UsdGeomGprim(prim);
                        if (gprim && gprim.GetDisplayColorAttr().ValueMightBeTimeVarying())
                        {
                            accessMap.Add(memberInfo);
                            mayVary |= true;
                        }
                    }
                }
                else if (Reflect.IsFusedTransform(memberInfo))
                {
                    vtValue = pxr.UsdCs.GetFusedTransform(prim, time);

                    if (accessMap != null)
                    {
                        // Transforms are complicated :/
                        var xformable = new pxr.UsdGeomXformable(prim);
                        if (xformable)
                        {
                            bool dummy;
                            var orderAttr = xformable.GetXformOpOrderAttr();
                            if (orderAttr)
                            {
                                if (orderAttr.GetVariability() == pxr.SdfVariability.SdfVariabilityVarying
                                    && orderAttr.ValueMightBeTimeVarying())
                                {
                                    mayVary |= true;
                                    accessMap.Add(memberInfo);
                                }
                                else
                                {
                                    foreach (var op in xformable.GetOrderedXformOps(out dummy))
                                    {
                                        var opAttr = op.GetAttr();
                                        if (!opAttr) { continue; }
                                        if (opAttr.GetVariability() == pxr.SdfVariability.SdfVariabilityVarying
                                            && opAttr.ValueMightBeTimeVarying())
                                        {
                                            mayVary |= true;
                                            accessMap.Add(memberInfo);
                                            break;
                                        }
                                    } // foreach
                                }
                            } // orderAttr
                        } // xformable
                    } // mayVary
                }
                else
                {
                    if (accessMap != null)
                    {
                        var attr = prim.GetAttribute(sdfAttrName);
                        if (attr.GetVariability() == pxr.SdfVariability.SdfVariabilityVarying
                            && attr.ValueMightBeTimeVarying())
                        {
                            accessMap.Add(memberInfo);
                            mayVary |= true;
                        }
                    }
                    if (!prim.GetAttributeValue(sdfAttrName, vtValue, time))
                    {
                        // Object has no value, still considered success.
                        return true;
                    }
                }

                if (vtValue.IsEmpty())
                {
                    // Object has no value, still considered success.
                    return true;
                }

                // ------------------------------------------ //
                // Infer C# type from USD when Type == Object
                // ------------------------------------------ //
                if (csType == typeof(object))
                {
                    // Blind object serialization needs special handling, since we won't know the C# type a priori.
                    // Instead, do a reverse lookup on the SdfTypeName and let USD dictate the C# type.
                    pxr.UsdAttribute attr = prim.GetAttribute(sdfAttrName);
                    if (attr != null && attr.IsValid())
                    {
                        // TODO: Assuming the reverse lookup is successful for the binding, the caller may be
                        // surprised by the result, since the USD <-> C# types are not 1-to-1. For example,
                        // a List<Vector2> may have been serialized, but Vector2[] may be read.
                        if (!sm_bindings.GetReverseBinding(attr.GetTypeName(), out binding))
                        {
                            if (string.IsNullOrEmpty(namespaceToRead))
                            {
                                return false;
                            }

                            // TODO: readback nested object declared as object -- maybe just disable this?
                            //Deserialize(ref csValue, prim, usdTime, usdNamespace: ns);
                            //return true;
                            return false;
                        }
                    }
                    else
                    {
                        // TODO: Allow reading metadata declared as object in C#
                        return false;
                    }
                }

                // ------------------------------------------ //
                // Convert USD's VtValue -> Strong C# Type.
                // ------------------------------------------ //
                csValue = binding.toCsObject(vtValue);

                // ------------------------------------------ //
                // Restore csValue.
                // ------------------------------------------ //
                if (conn != null && csValue != null)
                {
                    // Hack to handle a re-typed connected value in PrimvarReader in USD 21.11+.
                    // Previously it was a token type, now it's a string, but we need to handle both.
                    // The serialization automatically picks it up as a string, so we get an incorrect empty string.
                    // In this case of type mismatch, perform an explicit cast to get the actual value.
                    if (conn.GetValueType() == typeof(string) && vtValue.GetTypeName() == "TfToken")
                    {
                        csValue = (string)VtValue.CastToTypeOf(vtValue, new VtValue(""));
                    }

                    conn.SetValue(csValue);
                    csValue = conn;
                }
                if (pvAccessor != null)
                {
                    pvAccessor.SetValue(csValue);
                    csValue = pvAccessor;
                }
            }
            finally
            {
                // Would prefer RAII handle, but introduces garbage.
                ArrayAllocator.FreeHandle(vtValue);
            }

            return true;
        }
    }
}

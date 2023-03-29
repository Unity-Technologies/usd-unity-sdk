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
using System.Runtime.CompilerServices;
using pxr;

namespace USD.NET
{
    public delegate pxr.VtValue ToVtConverter(object value);
    public delegate object ToCsConverter(pxr.VtValue value);
    internal delegate object ToCsCopyConverter(pxr.VtValue value, object vtArray);

    /// Conversions discovered automatically via reflection on VtValue.
    public class DefaultConversions
    {
        public static pxr.VtValue ToVtValue(object value) { return CastToVtValue(value, value.GetType()); }

        public static pxr.VtValue CastToVtValue<T>(T t, Type trueType) where T : class
        {
            var ctor = typeof(pxr.VtValue).GetConstructor(new Type[] { trueType });
            if (ctor == null)
            {
                throw new ApplicationException(String.Format("Cannot construct VtValue from T({0})", trueType.Name));
            }

            return ctor.Invoke(new object[] { t }) as pxr.VtValue;
        }
    }

    public struct UsdTypeBinding
    {
        public ToVtConverter toVtValue;
        public ToCsConverter toCsObject;
        public pxr.SdfValueTypeName sdfTypeName;

        public UsdTypeBinding(ToVtConverter toVtConverter,
                              ToCsConverter toCsConverter,
                              pxr.SdfValueTypeName sdfName)
        {
            toVtValue = toVtConverter;
            toCsObject = toCsConverter;
            sdfTypeName = sdfName;
        }
    }

    public class TypeBinder
    {
        /// <summary>
        /// Use the JIT compiler to emit new functions (fast path) for type bindings.
        /// Set to false for platforms which do not support JIT compilation.
        /// </summary>
        public static bool EnableCodeGeneration = true;

        static Dictionary<Type, UsdTypeBinding> bindings = new Dictionary<Type, UsdTypeBinding>();

        // TODO: change these in Python generator to match .NET System types rather than C# types
        Dictionary<string, string> typeNameMapping = new Dictionary<string, string>();
        Dictionary<Type, Dictionary<pxr.TfToken, Enum>> enumMaps = new Dictionary<Type, Dictionary<pxr.TfToken, Enum>>();

        /// <summary>
        /// USD types can be aliased to represent different roles float2/texcoord2, vec3/point/color and drive the interpretation
        /// Add aliases if your target application need to map multiple USD types to a single type.
        /// </summary>
        Dictionary<string, string> typeAliases = new Dictionary<string, string>();

        public TypeBinder()
        {
            // TODO: kill this, see above.
            typeNameMapping.Add("Boolean", "bool");
            typeNameMapping.Add("Byte", "byte");
            typeNameMapping.Add("Int32", "int");
            typeNameMapping.Add("UInt32", "uint");
            typeNameMapping.Add("Int64", "long");
            typeNameMapping.Add("UInt64", "ulong");
            typeNameMapping.Add("String", "string");
            typeNameMapping.Add("Single", "float");
            typeNameMapping.Add("Double", "double");

            RegisterIntrinsicTypes();
        }

        private bool IsCodeGenEnabled()
        {
#if NET_4_6
            return EnableCodeGeneration;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get a binding from a USD type
        /// </summary>
        /// <remarks>
        /// Used the C# type needs to be derived from the USD type.
        /// Example: UVs can be of type float2, float3, texcoord2f, texcoord2f
        /// </remarks>
        public bool GetReverseBinding(pxr.SdfValueTypeName key, out UsdTypeBinding binding)
        {
            // Check if the given sdf type name is an alias
            // https://graphics.pixar.com/usd/docs/api/_usd__page__datatypes.html#Usd_Roles
            string name;
            if (!typeAliases.TryGetValue(key.GetAsToken(), out name))
            {
                name = key.GetAsToken();
            }

            // TODO: we could keep a reverse mapping, but waiting for deeper performance analysis first.
            foreach (var kvp in bindings)
            {
                if (kvp.Value.sdfTypeName.GetAsToken() == name)
                {
                    binding = kvp.Value;
                    return true;
                }
            }
            binding = new UsdTypeBinding();
            return false;
        }

        public bool GetReverseBinding(Type key, out UsdTypeBinding binding)
        {
            // TODO: we could keep a reverse mapping, but waiting for deeper performance analysis first.
            foreach (var kvp in bindings)
            {
                if (kvp.Value.toCsObject.Method.GetParameters()[0].ParameterType == key)
                {
                    binding = kvp.Value;
                    return true;
                }
            }
            binding = new UsdTypeBinding();
            return false;
        }

        public bool GetBinding(Type key, out UsdTypeBinding binding)
        {
            lock (UsdIo.Bindings) {
                // First try the exact type requested.
                if (bindings.TryGetValue(key, out binding))
                {
                    return true;
                }

                // If the first lookup failed, perhaps this is a nullable type?
                if (key.IsGenericType && key.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && bindings.TryGetValue(key.GetGenericArguments()[0], out binding))
                {
                    return true;
                }

                // If this is an Enum type, then use the generic Enum handler.
                if (!key.IsEnum)
                {
                    return false;
                }

                //
                // Enumerations.
                // To reduce special cases of client code, all enums are special cased into a single
                // converter. This converter is only selected if no specific type has been registered.
                //
                binding = BindEnum(key);

                // Memoize the binding so it doesn't get regenerated on every call.

                bindings.Add(key, binding);
            }

            return true;
        }

        public void BindType(Type csType, UsdTypeBinding binding)
        {
            bindings[csType] = binding;
        }

        /// <summary>
        /// Binds the specified C# type to the given USD array and scene description (Sdf) types,
        /// looking for ConverterT.ToVtArray(csType) and ConverterT.FromVtArray(vtArrayType).
        /// </summary>
        ///
        /// <typeparam name="ConverterT">
        /// The C# class type providing type conversion rules ToVtArray and FromVtArray.
        /// </typeparam>
        ///
        /// <param name="csType">The C# type to be mapped to USD</param>
        /// <param name="vtArrayType">The USD C++ value type (Vt)</param>
        /// <param name="sdfName">The USD scene description (Sdf) type</param>
        ///
        /// TODO: The C++ type can be inferred from the Sdf type, so vtArrayType is not needed.
        ///
        public void BindArrayType<ConverterT>(Type csType,
            Type vtArrayType,
            pxr.SdfValueTypeName sdfName,
            string methodNamePrefix = "")
        {
            // ConverterT and the function being found will be something like:
            //   class IntrinsicTypeConverter {
            //     static public VtTokenArray ToVtArray(string[] input);
            //
            var csToVtArray = typeof(ConverterT)
                .GetMethod(methodNamePrefix + "ToVtArray", new Type[] { csType });
            if (csToVtArray == null)
            {
                throw new ArgumentException(string.Format("No ToVtArray overload found for type {0}",
                    csType.ToString()));
            }

            // ConverterT and the function being found will be something like:
            //   class IntrinsicTypeConverter {
            //     static public string[] FromVtArray(VtTokenArray input);
            //
            var vtToCsArray = typeof(ConverterT)
                .GetMethod(methodNamePrefix + "FromVtArray", new Type[] { vtArrayType });
            if (vtToCsArray == null)
            {
                throw new ArgumentException(string.Format("No FromVtArray overload found for type {0}",
                    vtArrayType.ToString()));
            }

            // The specific UsdCs method being located here will be somthing like:
            //   class UsdCs {
            //     public static void VtValueToVtTokenArray(VtValue value, VtTokenArray output);
            //
            var valToVtArray = typeof(pxr.UsdCs).GetMethod("VtValueTo" + vtArrayType.Name,
                new Type[] { typeof(pxr.VtValue), vtArrayType });

            if (valToVtArray == null)
            {
                throw new ArgumentException(string.Format("No VtValueTo{...} converter found for type {0}",
                    vtArrayType.ToString()));
            }

            // The following code constructs functions to:
            //
            //   1) Convert the VtValue (type-erased container) to a specific VtArray<T> type and then
            //      convert the VtArray<T> to a native C# type.
            //
            //   2) Convert a strongly typed C# array to a strongly typed VtArray<T> and then
            //      convert the VtArray<T> to a type-erased VtValue.
            //
            // For example, to will convert:
            //
            //   1) VtValue -> VtArray<TfToken> -> string[]
            //   2) string[] -> VtArray<TfToken> -> VtValue
            //

            ToCsConverter toCs = null;
            ToVtConverter toVt = null;


            if (IsCodeGenEnabled())
            {
                // EmitToCs and EmitToVt are not defined when not using NET_4_6
#if NET_4_6
                var copyConverter = (ToCsCopyConverter)CodeGen.EmitToCs<ToCsCopyConverter>(valToVtArray, vtToCsArray);
                toCs = (vtValue) => ToCsConvertHelper(vtValue, vtArrayType, copyConverter);
                toVt = (ToVtConverter)CodeGen.EmitToVt<ToVtConverter>(csToVtArray, csType, vtArrayType);
#endif
            }
            else
            {
                // In .NET2 or when IL2CPP is enabled , we cannot dynamically emit code.
                // Instead, we use late binding, which is slower, but also doesn't crash.
                // In the future, we should generate code to do these conversions, rather than using late binding.
                toCs = (vtValue) => ToCsDynamicConvertHelper(vtValue, vtArrayType, valToVtArray, vtToCsArray);
                toVt = CsArrayToVtValue(csToVtArray, csType, vtArrayType);
            }

            bindings[csType] = new UsdTypeBinding(toVt, toCs, sdfName);
        }

        /// <summary>
        /// Returns a function that converts a C# array to a VtValue holding a VtArray of T, using (slow) late binding.
        /// </summary>
        ToVtConverter CsArrayToVtValue(System.Reflection.MethodInfo csToVtArray, Type csType, Type vtArrayType)
        {
            // Generates a function of the form:
            //   VtValue ToVt(object CSharpNativeArray) {
            //     VtArray<T> = converter( (StrongC#Type)CSharpNativeArray) );
            //     return VtValue(vtArray<T>);
            //   }

            // For example:
            //   class IntrinsicTypeConverter {
            //     static public VtTokenArray ToVtArray(string[] input);
            var ctor = typeof(pxr.VtValue).GetConstructor(new Type[] { vtArrayType });
            return (object csArray) => (pxr.VtValue)ctor.Invoke(new object[] { Convert.ChangeType(csToVtArray.Invoke(null, new object[] { Convert.ChangeType(csArray, csType) }), vtArrayType) });
        }

        /// <summary>
        /// Converts a VtValue holding a VtArray of T to a C#-native array type, using (slow) late binding.
        /// </summary>
        object ToCsDynamicConvertHelper(pxr.VtValue vtValue, Type vtArrayType, System.Reflection.MethodInfo valToVtArray, System.Reflection.MethodInfo vtToCsArray)
        {
            // Intentionally not tracking size here, since USD will resize the array for us.
            object vtArrayObject = UsdIo.ArrayAllocator.MallocHandle(vtArrayType);

            // Convert value to VtArray<T> and convert that to the target C# array type.

            // For example:
            //   class UsdCs {
            //     public static void VtValueToVtTokenArray(VtValue value, VtTokenArray output);
            valToVtArray.Invoke(null, new object[] { vtValue, vtArrayObject });

            // For example:
            //   class IntrinsicTypeConverter {
            //     static public string[] FromVtArray(VtTokenArray input);
            object csArray = vtToCsArray.Invoke(null, new object[] { vtArrayObject });

            // Free the handle back to the allocator.
            UsdIo.ArrayAllocator.FreeHandle(vtArrayType, vtArrayObject);

            // Return the C# array.
            return csArray;
        }

        /// <summary>
        /// Converts a VtValue holding a VtArray of T to a C#-native array type, using a generated (fast) function, toCs.
        /// </summary>
        object ToCsConvertHelper(pxr.VtValue val, Type vtArrayType, ToCsCopyConverter toCs)
        {
            // Intentionally not tracking size here, since USD will resize the array for us.
            object vtArrayObject = UsdIo.ArrayAllocator.MallocHandle(vtArrayType);

            // Convert value to VtArray<T> and convert that to the target C# array type.
            object csArray = toCs(val, vtArrayObject);

            // Free the handle back to the allocator.
            UsdIo.ArrayAllocator.FreeHandle(vtArrayType, vtArrayObject);

            // Return the C# array.
            return csArray;
        }

        public void BindNativeType(Type csType, pxr.SdfValueTypeName sdfName)
        {
            string name;
            if (!typeNameMapping.TryGetValue(csType.Name, out name))
            {
                name = csType.Name;
            }
            var converter = typeof(pxr.UsdCs).GetMethod("VtValueTo" + name,
                new Type[] { typeof(pxr.VtValue) });

            if (converter == null)
            {
                throw new ArgumentException(string.Format("No VtValueTo... converter found for type {0}, VtValueTo{1}",
                    csType.ToString(), name));
            }
            bindings[csType] = new UsdTypeBinding(DefaultConversions.ToVtValue,
                (x) => converter.Invoke(null, new object[] { x }),
                sdfName);
        }

        private UsdTypeBinding BindEnum(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("BindEnum is only applicable to enum types");
            }
            return new UsdTypeBinding(
                (x) => new pxr.VtValue(new pxr.TfToken(x.ToString()[0].ToString().ToLower() + x.ToString().Substring(1))),
                (x) => UsdEnumToCs(x, enumType),
                SdfValueTypeNames.Token);
        }

        object UsdEnumToCs(pxr.VtValue vtValue, Type enumType)
        {
            //using (var t = new PooledHandle<pxr.TfToken>(UsdIo.ArrayAllocator)) {
            var t = (pxr.TfToken)UsdIo.ArrayAllocator.MallocHandle(typeof(pxr.TfToken));
            try
            {
                pxr.UsdCs.VtValueToTfToken(vtValue, t);
                Enum enm;
                Dictionary<pxr.TfToken, Enum> enumMap;
                lock (UsdIo.Bindings) {
                    if (!enumMaps.TryGetValue(enumType, out enumMap))
                    {
                        enumMap = new Dictionary<pxr.TfToken, Enum>();
                        enumMaps.Add(enumType, enumMap);
                    }
                    if (!enumMap.TryGetValue(t, out enm))
                    {
                        string s = t.ToString();
                        System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(s));
                        enm = (Enum)Enum.Parse(enumType, string.Concat(char.ToUpper(s[0]), s.Substring(1)));
                        enumMap.Add(t, enm);
                    }
                }
                return enm;
            }
            finally
            {
                UsdIo.ArrayAllocator.FreeHandle(t);
            }
        }

        pxr.VtValue GuidToVt_Bytes(object guid)
        {
            byte[] bytes = ((Guid)guid).ToByteArray();
            return new pxr.VtValue(IntrinsicTypeConverter.ToVtArray(bytes));
        }

        object GuidToCs_Bytes(pxr.VtValue vtValue)
        {
            var vtBytes = (pxr.VtUCharArray)UsdIo.ArrayAllocator.MallocHandle(typeof(pxr.VtUCharArray));
            pxr.UsdCs.VtValueToVtUCharArray(vtValue, vtBytes);
            byte[] bytes = IntrinsicTypeConverter.FromVtArray(vtBytes);
            var ret = new Guid(bytes);
            UsdIo.ArrayAllocator.FreeHandle(vtBytes.GetType(), vtBytes);
            UsdIo.ArrayAllocator.Free(bytes.GetType(), (uint)bytes.Length, bytes);
            return ret;
        }

        /*
        pxr.VtValue GuidToVt_String(object guid)
        {
            return new pxr.VtValue(((Guid)guid).ToString());
        }

        // Removing so that strings default to the native binding instead.
        object GuidToCs_String(pxr.VtValue vtValue)
        {
            string newString = pxr.UsdCs.VtValueTostring(vtValue);
            Guid newGuid;
            bool result = Guid.TryParse(newString, out newGuid);
            return result ? newGuid : newString;
        }
        */

        public void AddTypeAlias(SdfValueTypeName alias, SdfValueTypeName target)
        {
            typeAliases.Add(alias.GetAsToken(), target.GetAsToken());
        }

        public bool RemoveTypeAlias(SdfValueTypeName alias)
        {
            return typeAliases.Remove(alias.GetAsToken());
        }

        private void RegisterIntrinsicTypes()
        {
            // --------------------------------------------------------------------------------------- //
            // Establish bindings from intrinsic C# type -> USD C++ type -> USD Type
            // --------------------------------------------------------------------------------------- //

            // TODO: The C++ -> USD type could probably be discovered at runtime.
            //       Note that multiple USD types map to a single C++ type, so the mapping
            //       needs to be discovered from USD type -> C++ type. The C++ type is known
            //       by the TfType system, but it's unclear how to go from C++ type to C# type.

            // --------------------------------------------------------------------------------------- //

            //
            // Custom C# conversions
            //

            // The bytes version of Guid, which generates less garbage, can't be used until versioning is
            // more fully supported.

            //bindings[typeof(Guid)] = new UsdTypeBinding(GuidToVt_Bytes, GuidToCs_Bytes, SdfValueTypeNames.UCharArray);

            // With arbitrary primvars supported, we have a lot more attributes coming through with string types.
            // This binding causes any SdfValueTypeNames.String to be treated as a Guid when trying to use reverse bindings,
            // which causes errors when attempting to create a Guid object with a string not in Guid format.
            // As USD doesn't really use Guids, removing for now.
            //bindings[typeof(Guid)] = new UsdTypeBinding(GuidToVt_String, GuidToCs_String, SdfValueTypeNames.String); 

            /*
             * These throw exceptions because there is no VtValueTo...ListOp, because those types are not declared in
             * SdfValueTypeNames. Bug filed: https://github.com/PixarAnimationStudios/USD/issues/639
             *
            BindNativeType(typeof(pxr.SdfInt64ListOp), SdfValueTypeNames.Int64);
            BindNativeType(typeof(pxr.SdfUInt64ListOp), SdfValueTypeNames.UInt64);
            BindNativeType(typeof(pxr.SdfIntListOp), SdfValueTypeNames.Int);
            BindNativeType(typeof(pxr.SdfUIntListOp), SdfValueTypeNames.UInt);
            BindNativeType(typeof(pxr.SdfStringListOp), SdfValueTypeNames.String);
            BindNativeType(typeof(pxr.SdfTokenListOp), SdfValueTypeNames.Token);
            BindNativeType(typeof(pxr.SdfReferenceListOp), SdfValueTypeNames.Asset);
            BindNativeType(typeof(pxr.SdfPathListOp), SdfValueTypeNames.String);
            */

            //
            // Bool
            //
            BindNativeType(typeof(bool), SdfValueTypeNames.Bool);
            BindNativeType(typeof(pxr.VtBoolArray), SdfValueTypeNames.BoolArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(bool[]), typeof(pxr.VtBoolArray), SdfValueTypeNames.BoolArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<bool>), typeof(pxr.VtBoolArray), SdfValueTypeNames.BoolArray, "List");

            //
            // UChar/byte
            //
            BindNativeType(typeof(byte), SdfValueTypeNames.UChar);
            BindNativeType(typeof(pxr.VtUCharArray), SdfValueTypeNames.UCharArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(byte[]), typeof(pxr.VtUCharArray), SdfValueTypeNames.UCharArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<byte>), typeof(pxr.VtUCharArray), SdfValueTypeNames.UCharArray, "List");

            //
            // String
            //
            BindNativeType(typeof(string), SdfValueTypeNames.String);
            BindNativeType(typeof(pxr.TfToken), SdfValueTypeNames.Token);
            BindNativeType(typeof(pxr.VtStringArray), SdfValueTypeNames.StringArray);
            BindNativeType(typeof(pxr.VtTokenArray), SdfValueTypeNames.TokenArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(string[]), typeof(pxr.VtTokenArray), SdfValueTypeNames.TokenArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<string>), typeof(pxr.VtTokenArray), SdfValueTypeNames.TokenArray, "List");

            //
            // SdfAssetPath
            //
            //BindType(typeof(pxr.SdfAssetPath), new UsdTypeBinding((obj) => new pxr.VtValue((pxr.SdfAssetPath)obj), (obj) => (pxr.SdfAssetPath)obj, SdfValueTypeNames.Asset));
            BindNativeType(typeof(pxr.SdfAssetPath), SdfValueTypeNames.Asset);
            BindArrayType<IntrinsicTypeConverter>(typeof(pxr.SdfAssetPath[]), typeof(pxr.SdfAssetPathArray), SdfValueTypeNames.AssetArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<pxr.SdfAssetPath>), typeof(pxr.SdfAssetPathArray), SdfValueTypeNames.AssetArray, "List");

            //
            // Int
            //
            BindNativeType(typeof(int), SdfValueTypeNames.Int);
            BindNativeType(typeof(pxr.VtIntArray), SdfValueTypeNames.IntArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(int[]), typeof(pxr.VtIntArray), SdfValueTypeNames.IntArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<int>), typeof(pxr.VtIntArray), SdfValueTypeNames.IntArray, "List");

            //
            // UInt
            //
            BindNativeType(typeof(uint), SdfValueTypeNames.UInt);
            BindNativeType(typeof(pxr.VtUIntArray), SdfValueTypeNames.UIntArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(uint[]), typeof(pxr.VtUIntArray), SdfValueTypeNames.UIntArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<uint>), typeof(pxr.VtUIntArray), SdfValueTypeNames.UIntArray, "List");

            //
            // Long
            //
            BindNativeType(typeof(long), SdfValueTypeNames.Int64);
            BindNativeType(typeof(pxr.VtInt64Array), SdfValueTypeNames.Int64Array);
            BindArrayType<IntrinsicTypeConverter>(typeof(long[]), typeof(pxr.VtInt64Array), SdfValueTypeNames.Int64Array);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<long>), typeof(pxr.VtInt64Array), SdfValueTypeNames.Int64Array, "List");

            //
            // ULong
            //
            BindNativeType(typeof(ulong), SdfValueTypeNames.UInt64);
            BindNativeType(typeof(pxr.VtUInt64Array), SdfValueTypeNames.UInt64Array);
            BindArrayType<IntrinsicTypeConverter>(typeof(ulong[]), typeof(pxr.VtUInt64Array), SdfValueTypeNames.UInt64Array);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<ulong>), typeof(pxr.VtUInt64Array), SdfValueTypeNames.UInt64Array, "List");

            //
            // Half
            //
            BindNativeType(typeof(pxr.GfHalf), SdfValueTypeNames.Half);
            BindNativeType(typeof(pxr.VtHalfArray), SdfValueTypeNames.HalfArray);
            BindNativeType(typeof(pxr.VtVec2hArray), SdfValueTypeNames.Half2Array);
            BindNativeType(typeof(pxr.VtVec3hArray), SdfValueTypeNames.Half3Array);
            BindNativeType(typeof(pxr.VtVec4hArray), SdfValueTypeNames.Half4Array);

            //
            // Float
            //
            BindNativeType(typeof(float), SdfValueTypeNames.Float);
            BindNativeType(typeof(pxr.VtFloatArray), SdfValueTypeNames.FloatArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(float[]), typeof(pxr.VtFloatArray), SdfValueTypeNames.FloatArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<float>), typeof(pxr.VtFloatArray), SdfValueTypeNames.FloatArray, "List");

            //
            // Double
            //
            BindNativeType(typeof(double), SdfValueTypeNames.Double);
            BindNativeType(typeof(pxr.VtDoubleArray), SdfValueTypeNames.DoubleArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(double[]), typeof(pxr.VtDoubleArray), SdfValueTypeNames.DoubleArray);
            BindArrayType<IntrinsicTypeConverter>(typeof(List<double>), typeof(pxr.VtDoubleArray), SdfValueTypeNames.DoubleArray, "List");

            //
            // Quaternion
            //
            BindNativeType(typeof(pxr.GfQuath), SdfValueTypeNames.Quath);
            BindNativeType(typeof(pxr.VtQuathArray), SdfValueTypeNames.QuathArray);

            BindNativeType(typeof(pxr.GfQuatf), SdfValueTypeNames.Quatf);
            BindNativeType(typeof(pxr.VtQuatfArray), SdfValueTypeNames.QuatfArray);

            BindNativeType(typeof(pxr.GfQuatd), SdfValueTypeNames.Quatd);
            BindNativeType(typeof(pxr.VtQuatdArray), SdfValueTypeNames.QuatdArray);

            // Below the Vt array types are commented out, because they will be found in reverse
            // binding searches, which we don't want. Still, it is desireable to be able to
            // serialize these types, though not a primary use case.

            //
            // Vec2
            //
            BindNativeType(typeof(pxr.GfVec2i), SdfValueTypeNames.Int2);
            BindNativeType(typeof(pxr.GfVec2h), SdfValueTypeNames.Half2);
            //BindNativeType(typeof(pxr.VtVec2hArray), SdfValueTypeNames.Half2Array);

            BindNativeType(typeof(pxr.GfVec2f), SdfValueTypeNames.Float2);
            //BindNativeType(typeof(pxr.VtVec2fArray), SdfValueTypeNames.Float2Array);

            //
            // Vec3
            //
            BindNativeType(typeof(pxr.GfVec3i), SdfValueTypeNames.Int3);
            //BindNativeType(typeof(pxr.VtVec3iArray), SdfValueTypeNames.Int3Array);
            BindNativeType(typeof(pxr.GfVec3h), SdfValueTypeNames.Half3);
            //BindNativeType(typeof(pxr.VtVec3hArray), SdfValueTypeNames.Half3Array);

            BindNativeType(typeof(pxr.GfVec3f), SdfValueTypeNames.Float3);
            //BindNativeType(typeof(pxr.VtVec3fArray), SdfValueTypeNames.Float3Array);

            //
            // Vec4
            //
            BindNativeType(typeof(pxr.GfVec4i), SdfValueTypeNames.Int4);
            //BindNativeType(typeof(pxr.VtVec4iArray), SdfValueTypeNames.Int4Array);
            BindNativeType(typeof(pxr.GfVec4h), SdfValueTypeNames.Half4);
            //BindNativeType(typeof(pxr.VtVec4hArray), SdfValueTypeNames.Half4Array);

            BindNativeType(typeof(pxr.GfVec4f), SdfValueTypeNames.Float4);
            //BindNativeType(typeof(pxr.VtVec4fArray), SdfValueTypeNames.Float4Array);

            // --------------------------------------------------------------------------------------- //
        }
    }
}

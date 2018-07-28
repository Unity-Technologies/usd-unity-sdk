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

namespace USD.NET {

  /// <summary>
  /// The USD serialization run-time. This class leverages the ArrayAllocator, TypeBinder,
  /// TokenCache, ReflectionCache and SampleBase to perform efficient reflection based serialization
  /// of arbitrary C# data types to and from USD.
  /// </summary>
  public class UsdIo {

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

    public UsdIo(object stageLock) {
      m_stageLock = stageLock;
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
                             string usdNamespace = null) {
      PropertyInfo[] properties = Reflect.GetCachedProperties(t.GetType());
      FieldInfo[] fields = Reflect.GetCachedFields(t.GetType());
      var imgble = new pxr.UsdGeomImageable(prim);

      for(int i = 0; i < properties.Length; i++) {
        PropertyInfo csProp = properties[i];
        Type csType = csProp.PropertyType;
        if (csType == typeof(object)) {
          if (Reflect.IsCustomData(csProp)) {
            throw new ArgumentException("Writing metadata/customdata with type of object is not currently allowed");
          }
          object o = csProp.GetValue(t, index:null);
          if (o != null) {
            csType = o.GetType();
          }
        }
        if (!WriteAttr(csProp.Name, csType, csProp.GetValue(t, index:null),
            usdTime, prim, imgble, csProp, usdNamespace)) {
          // TODO: add options to dictate behavior here
        }
      }

      for(int i = 0; i < fields.Length; i++) {
        FieldInfo csField = fields[i];
        Type csType = csField.FieldType;
        if (csType == typeof(object)) {
          if (Reflect.IsCustomData(csField)) {
            throw new ArgumentException("Writing metadata/customdata with type of object is not currently allowed");
          }
          object o = csField.GetValue(t);
          if (o != null) {
            csType = o.GetType();
          }
        }
        if (!WriteAttr(csField.Name, csType, csField.GetValue(t),
            usdTime, prim, imgble, csField, usdNamespace)) {
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
    /// <param name="usdNamespace">The USD namespace, if any.</param>
    public void Deserialize(ref object fieldValue,
                     pxr.UsdPrim prim,
                     pxr.UsdTimeCode usdTime,
                     FieldInfo field,
                     string usdNamespace = null) {
      if (Reflect.IsNonSerialized(field)) {
        return;
      }

      if (!ReadAttr(field.Name, field.FieldType, ref fieldValue, usdTime, prim, field, usdNamespace)) {
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
    /// <param name="usdNamespace">The USD namespace, if any.</param>
    public void Deserialize(ref object propValue,
                 pxr.UsdPrim prim,
                 pxr.UsdTimeCode usdTime,
                 PropertyInfo field,
                 string usdNamespace = null) {
      if (Reflect.IsNonSerialized(field)) {
        return;
      }

      if (!ReadAttr(field.Name, field.PropertyType, ref propValue, usdTime, prim, field, usdNamespace)) {
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
    /// <param name="usdNamespace">The object namespace, if any.</param>
    public void Deserialize<T>(T t,
                         pxr.UsdPrim prim,
                         pxr.UsdTimeCode usdTime,
                         string usdNamespace = null) where T : SampleBase {
      if (t == null) {
        return;
      }

      PropertyInfo[] properties = Reflect.GetCachedProperties(t.GetType());
      FieldInfo[] fields = Reflect.GetCachedFields(t.GetType());
      object value = t;

      for (int i = 0; i < properties.Length; i++) {
        PropertyInfo csProp = properties[i];
        if (Reflect.IsNonSerialized(csProp)) {
          continue;
        }
        object propValue = csProp.GetValue(t, null);
        Deserialize(ref propValue, prim, usdTime, csProp, usdNamespace);
        csProp.SetValue(t, propValue, index: null);
      }

      for (int i = 0; i < fields.Length; i++) {
        FieldInfo csField = fields[i];
        if (Reflect.IsNonSerialized(csField)) {
          continue;
        }
        object fieldValue = csField.GetValue(t);
        Deserialize(ref fieldValue, prim, usdTime, csField, usdNamespace);
        csField.SetValue(t, fieldValue);
      }

      t = (T)value;
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
                      string usdNamespace, string srcObject = null) {
      if (Reflect.IsNonSerialized(memberInfo)) {
        return true;
      }

      // If holding a dictionary, immediately recurse and write keys as attributes.
      if (csType == typeof(Dictionary<string, object>)) {
        Dictionary<string, object> dict = csValue as Dictionary<string, object>;
        foreach (var kvp in dict) {
          object value = kvp.Value;
          WriteAttr(kvp.Key, value.GetType(), value,
            usdTime, prim, imgble, memberInfo, usdNamespace, srcObject: attrName);
        }
        return true;
      }

      string ns = IntrinsicTypeConverter.JoinNamespace(usdNamespace,
          Reflect.GetNamespace(memberInfo));
      pxr.TfToken sdfAttrName = sm_tokenCache[attrName];

      if (csType == typeof(Relationship) && csValue != null) {
        string[] targetStrings = ((Relationship)csValue).targetPaths;
        if (targetStrings != null) {
          //
          // Write Relationship
          //
          string[] arr = IntrinsicTypeConverter.JoinNamespace(ns, sdfAttrName).Split(':');
          pxr.StdStringVector elts = new pxr.StdStringVector(arr.Length);
          foreach (var s in arr) {
            elts.Add(s);
          }

          pxr.UsdRelationship rel = null;
          lock (m_stageLock) {
            rel = prim.CreateRelationship(elts, custom: false);
          }

          if (!rel.IsValid()) {
            throw new ApplicationException("Failed to create relationship <"
                + prim.GetPath().AppendProperty(
                  new pxr.TfToken(
                    IntrinsicTypeConverter.JoinNamespace(ns, sdfAttrName))).ToString() + ">");
          }

          var targets = new pxr.SdfPathVector();
          foreach (var path in ((Relationship)csValue).targetPaths) {
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

      // Object not written, still considered success.
      if (csValue == null) { return true; }

      bool isCustomData = Reflect.IsCustomData(memberInfo);
      bool isPrimvar = Reflect.IsPrimvar(memberInfo);
	  int primvarElementSize = Reflect.GetPrimvarElementSize(memberInfo);

      UsdTypeBinding binding;

      var conn = csValue as Connectable;
      if (conn != null) {
        csType = conn.GetValue().GetType();
        csValue = conn.GetValue();
      }

      if (!sm_bindings.GetBinding(csType, out binding) && !csType.IsEnum) {
        if (string.IsNullOrEmpty(ns)) {
          return false;
        }

        Serialize(csValue, prim, usdTime, usdNamespace: ns);
        return true;
      }

      pxr.SdfVariability variability = Reflect.GetVariability(memberInfo);
      pxr.SdfValueTypeName sdfTypeName = binding.sdfTypeName;
      pxr.UsdTimeCode time = variability == pxr.SdfVariability.SdfVariabilityUniform
                                          ? pxr.UsdTimeCode.Default()
                                          : usdTime;

      bool custom = false;
      pxr.UsdAttribute attr;
      if (isCustomData) {
        // no-op
        attr = null;
      } else if (!isPrimvar) {
        if (string.IsNullOrEmpty(ns)) {
          lock (m_stageLock) {
            attr = prim.CreateAttribute(sdfAttrName, csType.IsEnum ? SdfValueTypeNames.Token : sdfTypeName, custom, variability);
          }
        } else {
          string[] arr = IntrinsicTypeConverter.JoinNamespace(ns, sdfAttrName).Split(':');
          pxr.StdStringVector elts = new pxr.StdStringVector(arr.Length);
          foreach (var s in arr) {
            elts.Add(s);
          }
          lock (m_stageLock) {
            attr = prim.CreateAttribute(elts, sdfTypeName, custom, variability);
          }
        }
      } else {
        // Primvars do not support additional namespaces.
        lock (m_stageLock) {
          var primvar = imgble.CreatePrimvar(sdfAttrName, sdfTypeName,
              VertexDataAttribute.Interpolation);
		  primvar.SetElementSize(primvarElementSize);
		  attr = primvar.GetAttr();
        }
      }

      if (attr != null && conn != null && conn.GetConnectedPath() != null) {
        // TODO: Pool temp vector, possibly add a single item overload for SetConnections.
        var paths = new pxr.SdfPathVector();
        var connPath = conn.GetConnectedPath();
        if (connPath != string.Empty) {
          paths.Add(new pxr.SdfPath(conn.GetConnectedPath()));
        }
        attr.SetConnections(paths);
      }

      pxr.VtValue vtValue = binding.toVtValue(csValue);
      lock (m_stageLock) {
        if (isCustomData) {
          prim.SetCustomDataByKey(sdfAttrName, vtValue);
        } else if (Reflect.IsFusedDisplayColor(memberInfo)) {
          pxr.UsdCs.SetFusedDisplayColor(prim, vtValue, time);
        } else {
          attr.Set(vtValue, time);
        }
      }

      if (!isCustomData && srcObject != null) {
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
    /// <param name="srcObject">The source object name, used when remapping names.</param>
    /// <returns>True on success.</returns>
    /// <remarks>
    /// Note that "success" in the return value does not indicate data was read, rather it
    /// indicates that no unexpected states were encountered. E.g. calling ReadAttr on a field
    /// with no value stored in USD will not return false, since that is not considered a failure
    /// state.
    /// </remarks>
    bool ReadAttr(string attrName, Type csType, ref object csValue, pxr.UsdTimeCode usdTime,
                      pxr.UsdPrim prim, MemberInfo memberInfo,
                      string usdNamespace, string srcObject = null) {

      bool isPrimvar = Reflect.IsPrimvar(memberInfo);
      string ns = IntrinsicTypeConverter.JoinNamespace(usdNamespace,
          Reflect.GetNamespace(memberInfo));

      // If holding a dictionary, immediately recurse and write keys as attributes.
      if (csType == typeof(Dictionary<string, object>)) {
        string sourceMember;

        if (isPrimvar) {
          ns = "primvars";
          sourceMember = attrName;
        } else {
          ns = IntrinsicTypeConverter.JoinNamespace(ns, attrName);
          sourceMember = null;
        }

        var dict = csValue as Dictionary<string, object>;

        foreach (var prop in prim.GetAuthoredPropertiesInNamespace(ns)) {
          object value = null;
          if (!string.IsNullOrEmpty(sourceMember)) {
            pxr.VtValue valSrcMember = prop.GetCustomDataByKey(sm_tokenCache["sourceMember"]);
            if (valSrcMember.IsEmpty() || sourceMember != (string)valSrcMember) {
              continue;
            }
          }
          if (isPrimvar) {
            // The recursive call will also discover that this is a primvar.
            ns = "";
          }
          if (ReadAttr(prop.GetBaseName(), typeof(Object), ref value, usdTime, prim, memberInfo, ns, srcObject)) {
            if (value != null) {
              dict.Add(prop.GetBaseName(), value);
            }
          }
        }
        return true;
      }

      pxr.TfToken sdfAttrName = sm_tokenCache[ns, attrName];

      if (csType == typeof(Relationship)) {

        //
        // Read Relationship
        //

        pxr.UsdRelationship rel = null;
        lock (m_stageLock) {
          rel = prim.GetRelationship(sm_tokenCache[sdfAttrName]);
        }

        var relationship = new Relationship();
        csValue = relationship;

        if (rel == null || !rel.IsValid()) {
          return true;
        }

        pxr.SdfPathVector paths = rel.GetTargets();
        string[] result = new string[paths.Count];
        for (int i = 0; i < paths.Count; i++) {
          result[i] = paths[i].ToString();
        }

        relationship.targetPaths = result;
        return true;
      }

      UsdTypeBinding binding;

      Connectable conn = null;
      if (csValue != null
          && csType.IsGenericType
          && csType.GetGenericTypeDefinition() == typeof(Connectable<>)) {
        conn = csValue as Connectable;
        if (conn != null) {
          csValue = conn.GetValue();
          csType = conn.GetValueType();
        }
      }

      if (!sm_bindings.GetBinding(csType, out binding)
          && !csType.IsEnum
          && csType != typeof(object)) {
        if (string.IsNullOrEmpty(ns)) {
          return false;
        }

        var sample = csValue as SampleBase;
        if (sample == null) {
          throw new Exception("Could not deserialize: Prim: " + prim.GetPath() + " namespace: " + ns);
        }
        Deserialize((SampleBase)csValue, prim, usdTime, usdNamespace: ns);
        return true;
      }

      if (conn != null) {
        csValue = conn;
      }

      pxr.SdfVariability variability = Reflect.GetVariability(memberInfo);

      // Note that namespaced primvars are not supported, so "primvars" will replace the incoming
      // namespace. This will happen if a nested/namespaced object has a member declared as a
      // primvar.
      if (isPrimvar) {
        System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(ns));
        sdfAttrName = sm_tokenCache["primvars", attrName];
      }

      pxr.UsdTimeCode time = variability == pxr.SdfVariability.SdfVariabilityUniform
                                          ? kDefaultUsdTime
                                          : usdTime;

      //using (var valWrapper = new PooledHandle<pxr.VtValue>(ArrayAllocator)) {
      pxr.VtValue vtValue = (pxr.VtValue)ArrayAllocator.MallocHandle(typeof(pxr.VtValue));
      try {
        if (conn != null) {
          var sources = new pxr.SdfPathVector();
          if (prim.GetAttribute(sdfAttrName).GetConnections(sources)) {
            if (sources.Count > 0) {
              conn.SetConnectedPath(sources[0].ToString());
            }
          }
        }

        if (Reflect.IsCustomData(memberInfo)) {
          vtValue = prim.GetCustomDataByKey(sdfAttrName);
        } else if (Reflect.IsFusedDisplayColor(memberInfo)) {
          vtValue = pxr.UsdCs.GetFusedDisplayColor(prim, time);
        } else if (Reflect.IsFusedTransform(memberInfo)) {
          vtValue = pxr.UsdCs.GetFusedTransform(prim, time);
        } else {
          if (!prim.GetAttributeValue(sdfAttrName, vtValue, time)) {
            // Object has no value, still considered success.
            return true;
          }
        }

        if (vtValue.IsEmpty()) {
          // Object has no value, still considered success.
          return true;
        }

        if (csType == typeof(object)) {
          // Blind object serialization needs special handling, since we won't know the C# type a priori.
          // Instead, do a reverse lookup on the SdfTypeName and let USD dictate the C# type.
          pxr.UsdAttribute attr = prim.GetAttribute(sdfAttrName);
          if (attr != null && attr.IsValid()) {
            // TODO: Assuming the reverse lookup is successful for the binding, the caller may be
            // surprised by the result, since the USD <-> C# types are not 1-to-1. For example,
            // a List<Vector2> may have been serialized, but Vector2[] may be read.
            if (!sm_bindings.GetReverseBinding(attr.GetTypeName(), out binding)) {
              if (string.IsNullOrEmpty(ns)) {
                return false;
              }

              // TODO: readback nested object declared as object -- maybe just disable this?
              //Deserialize(ref csValue, prim, usdTime, usdNamespace: ns);
              //return true;
              return false;
            }
          } else {
            // TODO: Allow reading metadata declared as object in C#
            return false;
          }
        }

        csValue = binding.toCsObject(vtValue);
        if (conn != null && csValue != null) {
          conn.SetValue(csValue);
          csValue = conn;
        }
      } finally {
        // Would prefer RAII handle, but introduces garbage.
        ArrayAllocator.FreeHandle(vtValue);
      }

      // Need to deal with this
      //if (srcObject != null) {
      //  attr.SetCustomDataByKey(m_tokenCache["sourceMember"], srcObject);
      //}
      return true;
    }
  }
}
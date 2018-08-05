# Copyright 2017 Google Inc. All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from pxr import Sdf

values = Sdf.ValueTypeNames.__dict__

castFromPre = "%typemap(cscode) VtValue %{"
castFrom = """
    public static implicit operator {csTypeName} (VtValue value) {{
        return UsdCs.VtValueTo{csTypeName}(value);
    }}"""
castFromPost = "%}"

castToPre = "%typemap(cscode) VtValue %{"
castTo = """
    public static implicit operator VtValue ({csTypeName} value) {{
        return new VtValue(value);
    }}"""
castToPost = "%}"

accessorPre = """%inline %{
// This code manifests in UsdCs class.
"""
accessor = """
extern {typeName} VtValueTo{csTypeName}(VtValue const& value) {{
  if (value.IsHolding<{typeName}>()) {{
    return value.UncheckedGet<{typeName}>();
  }}
  return {typeName}();
}}
extern void VtValueTo{csTypeName}(VtValue const& value, {typeName}* output) {{
  if (value.IsHolding<{typeName}>()) {{
    *output = value.UncheckedGet<{typeName}>();
  }}
}}"""
#
# accessorAlt is required because some compilers don't see unsigned int() as the type constructor.
# unsigned int and unsigned byte are the two types known to cause issues.
#
accessorAlt = """
extern {typeName} VtValueTo{csTypeName}(VtValue const& value) {{
  if (value.IsHolding<{typeName}>()) {{
    return value.UncheckedGet<{typeName}>();
  }}
  return ({typeName})(0);
}}
extern void VtValueTo{csTypeName}(VtValue const& value, {typeName}* output) {{
  if (value.IsHolding<{typeName}>()) {{
    *output = value.UncheckedGet<{typeName}>();
  }}
}}"""
accessorPost = "%}"

arrayDeclPre =""
arrayDecl = """CSHARP_ARRAYS({scalarType}, {scalarTypeCs});
WRAP_EQUAL({typeName})
%template ({typeName}) {cppTypeName};
typedef {cppTypeName} {typeName};
"""
arrayDeclPost = ""

valueCtor = "%template(VtValue) VtValue::VtValue<{typeName}>;"

def translateTypeIds(tn):
    return tn.replace("__int64", "int64_t").replace(" ", "")

def translateTypes(tn):
    return tn.replace("string", "std::string").\
              replace("__int64", "int64_t").\
              replace("pxr_half::half", "GfHalf").\
              replace("unsigned int64_t", "uint64_t")

def translateCsTypes(tn):
    return tn.replace("unsigned __int64", "ulong").\
              replace("__int64", "long").\
              replace("unsigned char", "byte").\
              replace("unsigned int", "uint").\
              replace("pxr_half::half", "GfHalf")

def genVtValue(basePath, copyright):
    vtValueTypes = basePath + "vt/vtValue_Types.i"
    vtValueCasts = basePath + "vt/vtValue_Casts.i"
    vtValueAccessors = basePath + "vt/vtValue_Accessors.i"

    vtArrayTypes = basePath + "vt/vtArray_Types.i"

    typeInfos = {}
    class typeInfo:
        typeId = ""
        cppTypeName = ""
        csTypeName = ""
        scalarType = ""
        scalarTypeCs = ""
        isPod = False
        isArray = False

    for n in values:
        if not isinstance(values[n], type(values["Token"])):
            continue

        ti = typeInfo()
        ti.isPod = values[n].fget().type.isPlainOldDataType
        ti.isArray = values[n].fget().isArray
        ti.scalarType = translateTypes(values[n].fget().scalarType.type.typeName)
        ti.scalarTypeCs = translateCsTypes(values[n].fget().scalarType.type.typeName)
        if values[n].fget().type.pythonClass is None:
            tn = values[n].fget().type.typeName
            ti.cppTypeName = translateTypes(tn)
            ti.csTypeName = translateCsTypes(tn)
        else:
            module = values[n].fget().type.pythonClass.__module__
            module = module.replace("pxr.", "")
            friendlyName = values[n].fget().type.pythonClass.__name__
            tn = module + friendlyName
            ti.cppTypeName = translateTypes(values[n].fget().type.typeName)
            ti.csTypeName = translateCsTypes(tn)
        ti.typeId = translateTypeIds(tn)
        typeInfos[translateTypes(tn)] = ti

    with open(vtValueTypes, "w") as f:
        print >> f, copyright
        for tn in sorted(typeInfos.keys()):
            print >> f, valueCtor.format(typeName=tn)

    with open(vtValueCasts, "w") as f:
        print >> f, copyright
        print >> f, castFromPre
        for tn in sorted(typeInfos.keys()):
            print >> f, castFrom.format(csTypeName=typeInfos[tn].csTypeName)
        for tn in sorted(typeInfos.keys()):
            print >> f, castTo.format(csTypeName=typeInfos[tn].csTypeName)
        print >> f, castToPost

    with open(vtValueAccessors, "w") as f:
        print >> f, copyright
        print >> f, accessorPre
        for tn in sorted(typeInfos.keys()):
            ti = typeInfos[tn]
            # unsigned int/char need special handling so they compile on OSX.
            if tn == "unsigned int" or tn == "unsigned char":
              print >> f, accessorAlt.format(typeName=tn, typeNameId=ti.typeId, csTypeName=ti.csTypeName)
            else:
              print >> f, accessor.format(typeName=tn, typeNameId=ti.typeId, csTypeName=ti.csTypeName)
        print >> f, accessorPost

    with open(vtArrayTypes, "w") as f:
        print >> f, copyright
        print >> f, arrayDeclPre
        for tn in sorted(typeInfos.keys()):
            ti = typeInfos[tn]
            if not ti.isArray:
                continue
            print >> f, arrayDecl.format(typeName=tn, cppTypeName=ti.cppTypeName, scalarType=ti.scalarType, scalarTypeCs=ti.scalarTypeCs)
        print >> f, arrayDeclPost
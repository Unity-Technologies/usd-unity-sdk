//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace pxr {

public class UsdGeomSubset : UsdTyped {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdGeomSubset(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdGeomSubset_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdGeomSubset obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdGeomSubset(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdGeomSubset(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdGeomSubset__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdGeomSubset() : this(UsdCsPINVOKE.new_UsdGeomSubset__SWIG_1(), true) {
  }

  public UsdGeomSubset(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdGeomSubset__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdGeomSubset_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdGeomSubset_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public new static UsdGeomSubset Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubset Define(UsdStageWeakPtr stage, SdfPath path) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_Define(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute GetElementTypeAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_GetElementTypeAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateElementTypeAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateElementTypeAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateElementTypeAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateElementTypeAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateElementTypeAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateElementTypeAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetIndicesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_GetIndicesAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateIndicesAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateIndicesAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateIndicesAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateIndicesAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateIndicesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateIndicesAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetFamilyNameAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_GetFamilyNameAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateFamilyNameAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateFamilyNameAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateFamilyNameAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateFamilyNameAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateFamilyNameAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSubset_CreateFamilyNameAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public static UsdGeomSubset CreateGeomSubset(UsdGeomImageable geom, TfToken subsetName, TfToken elementType, VtIntArray indices, TfToken familyName, TfToken familyType) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_CreateGeomSubset__SWIG_0(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(subsetName), TfToken.getCPtr(elementType), VtIntArray.getCPtr(indices), TfToken.getCPtr(familyName), TfToken.getCPtr(familyType)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubset CreateGeomSubset(UsdGeomImageable geom, TfToken subsetName, TfToken elementType, VtIntArray indices, TfToken familyName) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_CreateGeomSubset__SWIG_1(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(subsetName), TfToken.getCPtr(elementType), VtIntArray.getCPtr(indices), TfToken.getCPtr(familyName)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubset CreateGeomSubset(UsdGeomImageable geom, TfToken subsetName, TfToken elementType, VtIntArray indices) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_CreateGeomSubset__SWIG_2(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(subsetName), TfToken.getCPtr(elementType), VtIntArray.getCPtr(indices)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubset CreateUniqueGeomSubset(UsdGeomImageable geom, TfToken subsetName, TfToken elementType, VtIntArray indices, TfToken familyName, TfToken familyType) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_CreateUniqueGeomSubset__SWIG_0(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(subsetName), TfToken.getCPtr(elementType), VtIntArray.getCPtr(indices), TfToken.getCPtr(familyName), TfToken.getCPtr(familyType)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubset CreateUniqueGeomSubset(UsdGeomImageable geom, TfToken subsetName, TfToken elementType, VtIntArray indices, TfToken familyName) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_CreateUniqueGeomSubset__SWIG_1(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(subsetName), TfToken.getCPtr(elementType), VtIntArray.getCPtr(indices), TfToken.getCPtr(familyName)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubset CreateUniqueGeomSubset(UsdGeomImageable geom, TfToken subsetName, TfToken elementType, VtIntArray indices) {
    UsdGeomSubset ret = new UsdGeomSubset(UsdCsPINVOKE.UsdGeomSubset_CreateUniqueGeomSubset__SWIG_2(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(subsetName), TfToken.getCPtr(elementType), VtIntArray.getCPtr(indices)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubsetVector GetAllGeomSubsets(UsdGeomImageable geom) {
    UsdGeomSubsetVector ret = new UsdGeomSubsetVector(UsdCsPINVOKE.UsdGeomSubset_GetAllGeomSubsets(UsdGeomImageable.getCPtr(geom)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubsetVector GetGeomSubsets(UsdGeomImageable geom, TfToken elementType, TfToken familyName) {
    UsdGeomSubsetVector ret = new UsdGeomSubsetVector(UsdCsPINVOKE.UsdGeomSubset_GetGeomSubsets__SWIG_0(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(elementType), TfToken.getCPtr(familyName)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubsetVector GetGeomSubsets(UsdGeomImageable geom, TfToken elementType) {
    UsdGeomSubsetVector ret = new UsdGeomSubsetVector(UsdCsPINVOKE.UsdGeomSubset_GetGeomSubsets__SWIG_1(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(elementType)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSubsetVector GetGeomSubsets(UsdGeomImageable geom) {
    UsdGeomSubsetVector ret = new UsdGeomSubsetVector(UsdCsPINVOKE.UsdGeomSubset_GetGeomSubsets__SWIG_2(UsdGeomImageable.getCPtr(geom)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SWIGTYPE_p_std__setT_TfToken_TfTokenFastArbitraryLessThan_t GetAllGeomSubsetFamilyNames(UsdGeomImageable geom) {
    SWIGTYPE_p_std__setT_TfToken_TfTokenFastArbitraryLessThan_t ret = new SWIGTYPE_p_std__setT_TfToken_TfTokenFastArbitraryLessThan_t(UsdCsPINVOKE.UsdGeomSubset_GetAllGeomSubsetFamilyNames(UsdGeomImageable.getCPtr(geom)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool SetFamilyType(UsdGeomImageable geom, TfToken familyName, TfToken familyType) {
    bool ret = UsdCsPINVOKE.UsdGeomSubset_SetFamilyType(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(familyName), TfToken.getCPtr(familyType));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TfToken GetFamilyType(UsdGeomImageable geom, TfToken familyName) {
    TfToken ret = new TfToken(UsdCsPINVOKE.UsdGeomSubset_GetFamilyType(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(familyName)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static VtIntArray GetUnassignedIndices(UsdGeomSubsetVector subsets, uint elementCount, UsdTimeCode time) {
    VtIntArray ret = new VtIntArray(UsdCsPINVOKE.UsdGeomSubset_GetUnassignedIndices__SWIG_0(UsdGeomSubsetVector.getCPtr(subsets), elementCount, UsdTimeCode.getCPtr(time)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static VtIntArray GetUnassignedIndices(UsdGeomSubsetVector subsets, uint elementCount) {
    VtIntArray ret = new VtIntArray(UsdCsPINVOKE.UsdGeomSubset_GetUnassignedIndices__SWIG_1(UsdGeomSubsetVector.getCPtr(subsets), elementCount), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool ValidateSubsets(UsdGeomSubsetVector subsets, uint elementCount, TfToken familyType, /*cstype*/ out string reason) {
    bool ret = UsdCsPINVOKE.UsdGeomSubset_ValidateSubsets(UsdGeomSubsetVector.getCPtr(subsets), elementCount, TfToken.getCPtr(familyType), out reason);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool ValidateFamily(UsdGeomImageable geom, TfToken elementType, TfToken familyName, /*cstype*/ out string reason) {
    bool ret = UsdCsPINVOKE.UsdGeomSubset_ValidateFamily(UsdGeomImageable.getCPtr(geom), TfToken.getCPtr(elementType), TfToken.getCPtr(familyName), out reason);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static readonly UsdSchemaKind schemaKind = (UsdSchemaKind)UsdCsPINVOKE.UsdGeomSubset_schemaKind_get();
}

}

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

public class UsdRiSplineAPI : UsdAPISchemaBase {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdRiSplineAPI(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdRiSplineAPI_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdRiSplineAPI obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdRiSplineAPI(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdRiSplineAPI(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdRiSplineAPI__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdRiSplineAPI() : this(UsdCsPINVOKE.new_UsdRiSplineAPI__SWIG_1(), true) {
  }

  public UsdRiSplineAPI(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdRiSplineAPI__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdRiSplineAPI_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdRiSplineAPI_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public UsdRiSplineAPI(UsdPrim prim, TfToken splineName, SdfValueTypeName valuesTypeName, bool doesDuplicateBSplineEndpoints) : this(UsdCsPINVOKE.new_UsdRiSplineAPI__SWIG_3(UsdPrim.getCPtr(prim), TfToken.getCPtr(splineName), SdfValueTypeName.getCPtr(valuesTypeName), doesDuplicateBSplineEndpoints), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdRiSplineAPI(UsdSchemaBase schemaObj, TfToken splineName, SdfValueTypeName valuesTypeName, bool doesDuplicateBSplineEndpoints) : this(UsdCsPINVOKE.new_UsdRiSplineAPI__SWIG_4(UsdSchemaBase.getCPtr(schemaObj), TfToken.getCPtr(splineName), SdfValueTypeName.getCPtr(valuesTypeName), doesDuplicateBSplineEndpoints), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool DoesDuplicateBSplineEndpoints() {
    bool ret = UsdCsPINVOKE.UsdRiSplineAPI_DoesDuplicateBSplineEndpoints(swigCPtr);
    return ret;
  }

  public SdfValueTypeName GetValuesTypeName() {
    SdfValueTypeName ret = new SdfValueTypeName(UsdCsPINVOKE.UsdRiSplineAPI_GetValuesTypeName(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetInterpolationAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_GetInterpolationAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateInterpolationAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreateInterpolationAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateInterpolationAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreateInterpolationAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateInterpolationAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreateInterpolationAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetPositionsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_GetPositionsAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreatePositionsAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreatePositionsAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreatePositionsAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreatePositionsAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreatePositionsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreatePositionsAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetValuesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_GetValuesAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateValuesAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreateValuesAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateValuesAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreateValuesAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateValuesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiSplineAPI_CreateValuesAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public bool Validate(/*cstype*/ out string reason) {
    bool ret = UsdCsPINVOKE.UsdRiSplineAPI_Validate(swigCPtr, out reason);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}

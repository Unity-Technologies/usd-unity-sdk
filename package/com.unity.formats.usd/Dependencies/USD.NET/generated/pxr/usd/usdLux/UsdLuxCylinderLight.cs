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

public class UsdLuxCylinderLight : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UsdLuxCylinderLight(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdLuxCylinderLight obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UsdLuxCylinderLight() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdLuxCylinderLight(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public UsdLuxCylinderLight(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdLuxCylinderLight__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdLuxCylinderLight() : this(UsdCsPINVOKE.new_UsdLuxCylinderLight__SWIG_1(), true) {
  }

  public UsdLuxCylinderLight(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdLuxCylinderLight__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdLuxCylinderLight_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdLuxCylinderLight_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public static UsdLuxCylinderLight Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdLuxCylinderLight ret = new UsdLuxCylinderLight(UsdCsPINVOKE.UsdLuxCylinderLight_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdLuxCylinderLight Define(UsdStageWeakPtr stage, SdfPath path) {
    UsdLuxCylinderLight ret = new UsdLuxCylinderLight(UsdCsPINVOKE.UsdLuxCylinderLight_Define(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute GetLengthAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_GetLengthAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateLengthAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateLengthAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateLengthAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateLengthAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateLengthAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateLengthAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetRadiusAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_GetRadiusAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateRadiusAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateRadiusAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateRadiusAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateRadiusAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateRadiusAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateRadiusAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetTreatAsLineAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_GetTreatAsLineAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateTreatAsLineAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateTreatAsLineAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateTreatAsLineAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateTreatAsLineAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateTreatAsLineAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdLuxCylinderLight_CreateTreatAsLineAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

}

}

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

public class UsdGeomSphere : UsdGeomGprim {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdGeomSphere(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdGeomSphere_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdGeomSphere obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdGeomSphere(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdGeomSphere(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdGeomSphere__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdGeomSphere() : this(UsdCsPINVOKE.new_UsdGeomSphere__SWIG_1(), true) {
  }

  public UsdGeomSphere(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdGeomSphere__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdGeomSphere_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdGeomSphere_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public new static UsdGeomSphere Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdGeomSphere ret = new UsdGeomSphere(UsdCsPINVOKE.UsdGeomSphere_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomSphere Define(UsdStageWeakPtr stage, SdfPath path) {
    UsdGeomSphere ret = new UsdGeomSphere(UsdCsPINVOKE.UsdGeomSphere_Define(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute GetRadiusAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_GetRadiusAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateRadiusAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_CreateRadiusAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateRadiusAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_CreateRadiusAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateRadiusAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_CreateRadiusAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public new UsdAttribute GetExtentAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_GetExtentAttr(swigCPtr), true);
    return ret;
  }

  public new UsdAttribute CreateExtentAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_CreateExtentAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public new UsdAttribute CreateExtentAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_CreateExtentAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public new UsdAttribute CreateExtentAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomSphere_CreateExtentAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public static bool ComputeExtent(double radius, VtVec3fArray extent) {
    bool ret = UsdCsPINVOKE.UsdGeomSphere_ComputeExtent__SWIG_0(radius, VtVec3fArray.getCPtr(extent));
    return ret;
  }

  public static bool ComputeExtent(double radius, GfMatrix4d transform, VtVec3fArray extent) {
    bool ret = UsdCsPINVOKE.UsdGeomSphere_ComputeExtent__SWIG_1(radius, GfMatrix4d.getCPtr(transform), VtVec3fArray.getCPtr(extent));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}

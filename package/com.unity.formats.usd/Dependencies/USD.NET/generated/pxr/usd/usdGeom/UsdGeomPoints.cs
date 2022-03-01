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

public class UsdGeomPoints : UsdGeomPointBased {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdGeomPoints(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdGeomPoints_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdGeomPoints obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdGeomPoints(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdGeomPoints(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdGeomPoints__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdGeomPoints() : this(UsdCsPINVOKE.new_UsdGeomPoints__SWIG_1(), true) {
  }

  public UsdGeomPoints(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdGeomPoints__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdGeomPoints_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdGeomPoints_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public new static UsdGeomPoints Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdGeomPoints ret = new UsdGeomPoints(UsdCsPINVOKE.UsdGeomPoints_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdGeomPoints Define(UsdStageWeakPtr stage, SdfPath path) {
    UsdGeomPoints ret = new UsdGeomPoints(UsdCsPINVOKE.UsdGeomPoints_Define(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute GetWidthsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_GetWidthsAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateWidthsAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_CreateWidthsAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateWidthsAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_CreateWidthsAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateWidthsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_CreateWidthsAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetIdsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_GetIdsAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateIdsAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_CreateIdsAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateIdsAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_CreateIdsAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateIdsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdGeomPoints_CreateIdsAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public TfToken GetWidthsInterpolation() {
    TfToken ret = new TfToken(UsdCsPINVOKE.UsdGeomPoints_GetWidthsInterpolation(swigCPtr), true);
    return ret;
  }

  public bool SetWidthsInterpolation(TfToken interpolation) {
    bool ret = UsdCsPINVOKE.UsdGeomPoints_SetWidthsInterpolation(swigCPtr, TfToken.getCPtr(interpolation));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool ComputeExtent(VtVec3fArray points, VtFloatArray widths, VtVec3fArray extent) {
    bool ret = UsdCsPINVOKE.UsdGeomPoints_ComputeExtent__SWIG_0(VtVec3fArray.getCPtr(points), VtFloatArray.getCPtr(widths), VtVec3fArray.getCPtr(extent));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool ComputeExtent(VtVec3fArray points, VtFloatArray widths, GfMatrix4d transform, VtVec3fArray extent) {
    bool ret = UsdCsPINVOKE.UsdGeomPoints_ComputeExtent__SWIG_1(VtVec3fArray.getCPtr(points), VtFloatArray.getCPtr(widths), GfMatrix4d.getCPtr(transform), VtVec3fArray.getCPtr(extent));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public uint GetPointCount(UsdTimeCode timeCode) {
    uint ret = UsdCsPINVOKE.UsdGeomPoints_GetPointCount__SWIG_0(swigCPtr, UsdTimeCode.getCPtr(timeCode));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public uint GetPointCount() {
    uint ret = UsdCsPINVOKE.UsdGeomPoints_GetPointCount__SWIG_1(swigCPtr);
    return ret;
  }

  public static readonly UsdSchemaKind schemaKind = (UsdSchemaKind)UsdCsPINVOKE.UsdGeomPoints_schemaKind_get();
}

}

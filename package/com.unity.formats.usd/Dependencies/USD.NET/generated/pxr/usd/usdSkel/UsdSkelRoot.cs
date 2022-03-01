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

public class UsdSkelRoot : UsdGeomBoundable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdSkelRoot(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdSkelRoot_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdSkelRoot obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdSkelRoot(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdSkelRoot(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdSkelRoot__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdSkelRoot() : this(UsdCsPINVOKE.new_UsdSkelRoot__SWIG_1(), true) {
  }

  public UsdSkelRoot(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdSkelRoot__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdSkelRoot_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdSkelRoot_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public new static UsdSkelRoot Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdSkelRoot ret = new UsdSkelRoot(UsdCsPINVOKE.UsdSkelRoot_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdSkelRoot Define(UsdStageWeakPtr stage, SdfPath path) {
    UsdSkelRoot ret = new UsdSkelRoot(UsdCsPINVOKE.UsdSkelRoot_Define(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdSkelRoot Find(UsdPrim prim) {
    UsdSkelRoot ret = new UsdSkelRoot(UsdCsPINVOKE.UsdSkelRoot_Find(UsdPrim.getCPtr(prim)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static readonly UsdSchemaKind schemaKind = (UsdSchemaKind)UsdCsPINVOKE.UsdSkelRoot_schemaKind_get();
}

}

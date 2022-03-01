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

public class UsdVolVolume : UsdGeomGprim {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdVolVolume(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdVolVolume_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdVolVolume obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdVolVolume(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdVolVolume(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdVolVolume__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdVolVolume() : this(UsdCsPINVOKE.new_UsdVolVolume__SWIG_1(), true) {
  }

  public UsdVolVolume(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdVolVolume__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdVolVolume_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdVolVolume_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public new static UsdVolVolume Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdVolVolume ret = new UsdVolVolume(UsdCsPINVOKE.UsdVolVolume_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdVolVolume Define(UsdStageWeakPtr stage, SdfPath path) {
    UsdVolVolume ret = new UsdVolVolume(UsdCsPINVOKE.UsdVolVolume_Define(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_std__mapT_TfToken_SdfPath_std__lessT_TfToken_t_t GetFieldPaths() {
    SWIGTYPE_p_std__mapT_TfToken_SdfPath_std__lessT_TfToken_t_t ret = new SWIGTYPE_p_std__mapT_TfToken_SdfPath_std__lessT_TfToken_t_t(UsdCsPINVOKE.UsdVolVolume_GetFieldPaths(swigCPtr), true);
    return ret;
  }

  public bool HasFieldRelationship(TfToken name) {
    bool ret = UsdCsPINVOKE.UsdVolVolume_HasFieldRelationship(swigCPtr, TfToken.getCPtr(name));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SdfPath GetFieldPath(TfToken name) {
    SdfPath ret = new SdfPath(UsdCsPINVOKE.UsdVolVolume_GetFieldPath(swigCPtr, TfToken.getCPtr(name)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool CreateFieldRelationship(TfToken name, SdfPath fieldPath) {
    bool ret = UsdCsPINVOKE.UsdVolVolume_CreateFieldRelationship(swigCPtr, TfToken.getCPtr(name), SdfPath.getCPtr(fieldPath));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool BlockFieldRelationship(TfToken name) {
    bool ret = UsdCsPINVOKE.UsdVolVolume_BlockFieldRelationship(swigCPtr, TfToken.getCPtr(name));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static readonly UsdSchemaKind schemaKind = (UsdSchemaKind)UsdCsPINVOKE.UsdVolVolume_schemaKind_get();
}

}

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

public class UsdPrimCompositionQueryArc : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UsdPrimCompositionQueryArc(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdPrimCompositionQueryArc obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UsdPrimCompositionQueryArc() {
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
          UsdCsPINVOKE.delete_UsdPrimCompositionQueryArc(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public PcpNodeRef GetTargetNode() {
    PcpNodeRef ret = new PcpNodeRef(UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetTargetNode(swigCPtr), true);
    return ret;
  }

  public PcpNodeRef GetIntroducingNode() {
    PcpNodeRef ret = new PcpNodeRef(UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetIntroducingNode(swigCPtr), true);
    return ret;
  }

  public SdfLayerHandle GetIntroducingLayer() {
    SdfLayerHandle ret = new SdfLayerHandle(UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetIntroducingLayer(swigCPtr), true);
    return ret;
  }

  public SdfPath GetIntroducingPrimPath() {
    SdfPath ret = new SdfPath(UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetIntroducingPrimPath(swigCPtr), true);
    return ret;
  }

  public bool GetIntroducingListEditor(SWIGTYPE_p_SdfReferenceEditorProxy editor, SdfReference ref_) {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetIntroducingListEditor__SWIG_0(swigCPtr, SWIGTYPE_p_SdfReferenceEditorProxy.getCPtr(editor), SdfReference.getCPtr(ref_));
    return ret;
  }

  public bool GetIntroducingListEditor(SWIGTYPE_p_SdfPayloadEditorProxy editor, SdfPayload payload) {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetIntroducingListEditor__SWIG_1(swigCPtr, SWIGTYPE_p_SdfPayloadEditorProxy.getCPtr(editor), SdfPayload.getCPtr(payload));
    return ret;
  }

  public bool GetIntroducingListEditor(SWIGTYPE_p_SdfPathEditorProxy editor, SdfPath path) {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetIntroducingListEditor__SWIG_2(swigCPtr, SWIGTYPE_p_SdfPathEditorProxy.getCPtr(editor), SdfPath.getCPtr(path));
    return ret;
  }

  public bool GetIntroducingListEditor(SWIGTYPE_p_SdfNameEditorProxy editor, SWIGTYPE_p_std__string name) {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetIntroducingListEditor__SWIG_3(swigCPtr, SWIGTYPE_p_SdfNameEditorProxy.getCPtr(editor), SWIGTYPE_p_std__string.getCPtr(name));
    return ret;
  }

  public PcpArcType GetArcType() {
    PcpArcType ret = (PcpArcType)UsdCsPINVOKE.UsdPrimCompositionQueryArc_GetArcType(swigCPtr);
    return ret;
  }

  public bool IsImplicit() {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_IsImplicit(swigCPtr);
    return ret;
  }

  public bool IsAncestral() {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_IsAncestral(swigCPtr);
    return ret;
  }

  public bool HasSpecs() {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_HasSpecs(swigCPtr);
    return ret;
  }

  public bool IsIntroducedInRootLayerStack() {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_IsIntroducedInRootLayerStack(swigCPtr);
    return ret;
  }

  public bool IsIntroducedInRootLayerPrimSpec() {
    bool ret = UsdCsPINVOKE.UsdPrimCompositionQueryArc_IsIntroducedInRootLayerPrimSpec(swigCPtr);
    return ret;
  }

}

}
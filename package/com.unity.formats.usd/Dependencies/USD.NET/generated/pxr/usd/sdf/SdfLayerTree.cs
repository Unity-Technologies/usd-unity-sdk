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

public class SdfLayerTree : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  private bool swigCMemOwnBase;

  internal SdfLayerTree(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwnBase = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(SdfLayerTree obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }
{
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwnBase) {
          swigCMemOwnBase = false;
          $imcall;
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }
  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_SdfLayerTree(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static SdfLayerTreeHandle New(SdfLayerHandle layer, SdfLayerTreeHandleVector childTrees, SdfLayerOffset cumulativeOffset) {
    SdfLayerTreeHandle ret = new SdfLayerTreeHandle(UsdCsPINVOKE.SdfLayerTree_New__SWIG_0(SdfLayerHandle.getCPtr(layer), SdfLayerTreeHandleVector.getCPtr(childTrees), SdfLayerOffset.getCPtr(cumulativeOffset)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SdfLayerTreeHandle New(SdfLayerHandle layer, SdfLayerTreeHandleVector childTrees) {
    SdfLayerTreeHandle ret = new SdfLayerTreeHandle(UsdCsPINVOKE.SdfLayerTree_New__SWIG_1(SdfLayerHandle.getCPtr(layer), SdfLayerTreeHandleVector.getCPtr(childTrees)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SdfLayerHandle GetLayer() {
    SdfLayerHandle ret = new SdfLayerHandle(UsdCsPINVOKE.SdfLayerTree_GetLayer(swigCPtr), false);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SdfLayerOffset GetOffset() {
    SdfLayerOffset ret = new SdfLayerOffset(UsdCsPINVOKE.SdfLayerTree_GetOffset(swigCPtr), false);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SdfLayerTreeHandleVector GetChildTrees() {
    SdfLayerTreeHandleVector ret = new SdfLayerTreeHandleVector(UsdCsPINVOKE.SdfLayerTree_GetChildTrees(swigCPtr), false);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}

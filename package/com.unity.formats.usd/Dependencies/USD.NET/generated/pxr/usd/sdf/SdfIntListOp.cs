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

public class SdfIntListOp : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal SdfIntListOp(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(SdfIntListOp obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~SdfIntListOp() {
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
          UsdCsPINVOKE.delete_SdfIntListOp(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static SdfIntListOp CreateExplicit(StdIntVector explicitItems) {
    SdfIntListOp ret = new SdfIntListOp(UsdCsPINVOKE.SdfIntListOp_CreateExplicit__SWIG_0(StdIntVector.getCPtr(explicitItems)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SdfIntListOp CreateExplicit() {
    SdfIntListOp ret = new SdfIntListOp(UsdCsPINVOKE.SdfIntListOp_CreateExplicit__SWIG_1(), true);
    return ret;
  }

  public static SdfIntListOp Create(StdIntVector prependedItems, StdIntVector appendedItems, StdIntVector deletedItems) {
    SdfIntListOp ret = new SdfIntListOp(UsdCsPINVOKE.SdfIntListOp_Create__SWIG_0(StdIntVector.getCPtr(prependedItems), StdIntVector.getCPtr(appendedItems), StdIntVector.getCPtr(deletedItems)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SdfIntListOp Create(StdIntVector prependedItems, StdIntVector appendedItems) {
    SdfIntListOp ret = new SdfIntListOp(UsdCsPINVOKE.SdfIntListOp_Create__SWIG_1(StdIntVector.getCPtr(prependedItems), StdIntVector.getCPtr(appendedItems)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SdfIntListOp Create(StdIntVector prependedItems) {
    SdfIntListOp ret = new SdfIntListOp(UsdCsPINVOKE.SdfIntListOp_Create__SWIG_2(StdIntVector.getCPtr(prependedItems)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SdfIntListOp Create() {
    SdfIntListOp ret = new SdfIntListOp(UsdCsPINVOKE.SdfIntListOp_Create__SWIG_3(), true);
    return ret;
  }

  public SdfIntListOp() : this(UsdCsPINVOKE.new_SdfIntListOp(), true) {
  }

  public void Swap(SdfIntListOp rhs) {
    UsdCsPINVOKE.SdfIntListOp_Swap(swigCPtr, SdfIntListOp.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool HasKeys() {
    bool ret = UsdCsPINVOKE.SdfIntListOp_HasKeys(swigCPtr);
    return ret;
  }

  public bool HasItem(int item) {
    bool ret = UsdCsPINVOKE.SdfIntListOp_HasItem(swigCPtr, item);
    return ret;
  }

  public bool IsExplicit() {
    bool ret = UsdCsPINVOKE.SdfIntListOp_IsExplicit(swigCPtr);
    return ret;
  }

  public StdIntVector GetExplicitItems() {
    StdIntVector ret = new StdIntVector(UsdCsPINVOKE.SdfIntListOp_GetExplicitItems(swigCPtr), false);
    return ret;
  }

  public StdIntVector GetAddedItems() {
    StdIntVector ret = new StdIntVector(UsdCsPINVOKE.SdfIntListOp_GetAddedItems(swigCPtr), false);
    return ret;
  }

  public StdIntVector GetPrependedItems() {
    StdIntVector ret = new StdIntVector(UsdCsPINVOKE.SdfIntListOp_GetPrependedItems(swigCPtr), false);
    return ret;
  }

  public StdIntVector GetAppendedItems() {
    StdIntVector ret = new StdIntVector(UsdCsPINVOKE.SdfIntListOp_GetAppendedItems(swigCPtr), false);
    return ret;
  }

  public StdIntVector GetDeletedItems() {
    StdIntVector ret = new StdIntVector(UsdCsPINVOKE.SdfIntListOp_GetDeletedItems(swigCPtr), false);
    return ret;
  }

  public StdIntVector GetOrderedItems() {
    StdIntVector ret = new StdIntVector(UsdCsPINVOKE.SdfIntListOp_GetOrderedItems(swigCPtr), false);
    return ret;
  }

  public StdIntVector GetItems(SdfListOpType type) {
    StdIntVector ret = new StdIntVector(UsdCsPINVOKE.SdfIntListOp_GetItems(swigCPtr, (int)type), false);
    return ret;
  }

  public void SetExplicitItems(StdIntVector items) {
    UsdCsPINVOKE.SdfIntListOp_SetExplicitItems(swigCPtr, StdIntVector.getCPtr(items));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetAddedItems(StdIntVector items) {
    UsdCsPINVOKE.SdfIntListOp_SetAddedItems(swigCPtr, StdIntVector.getCPtr(items));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetPrependedItems(StdIntVector items) {
    UsdCsPINVOKE.SdfIntListOp_SetPrependedItems(swigCPtr, StdIntVector.getCPtr(items));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetAppendedItems(StdIntVector items) {
    UsdCsPINVOKE.SdfIntListOp_SetAppendedItems(swigCPtr, StdIntVector.getCPtr(items));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetDeletedItems(StdIntVector items) {
    UsdCsPINVOKE.SdfIntListOp_SetDeletedItems(swigCPtr, StdIntVector.getCPtr(items));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetOrderedItems(StdIntVector items) {
    UsdCsPINVOKE.SdfIntListOp_SetOrderedItems(swigCPtr, StdIntVector.getCPtr(items));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetItems(StdIntVector items, SdfListOpType type) {
    UsdCsPINVOKE.SdfIntListOp_SetItems(swigCPtr, StdIntVector.getCPtr(items), (int)type);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Clear() {
    UsdCsPINVOKE.SdfIntListOp_Clear(swigCPtr);
  }

  public void ClearAndMakeExplicit() {
    UsdCsPINVOKE.SdfIntListOp_ClearAndMakeExplicit(swigCPtr);
  }

  public bool ReplaceOperations(SdfListOpType op, uint index, uint n, StdIntVector newItems) {
    bool ret = UsdCsPINVOKE.SdfIntListOp_ReplaceOperations(swigCPtr, (int)op, index, n, StdIntVector.getCPtr(newItems));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void ComposeOperations(SdfIntListOp stronger, SdfListOpType op) {
    UsdCsPINVOKE.SdfIntListOp_ComposeOperations(swigCPtr, SdfIntListOp.getCPtr(stronger), (int)op);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

}

}

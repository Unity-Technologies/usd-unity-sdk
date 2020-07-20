//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace pxr {

[Preserve]
public class VtVec2dArray : Vt_ArrayBase {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal VtVec2dArray(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.VtVec2dArray_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(VtVec2dArray obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~VtVec2dArray() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_VtVec2dArray(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

	  public GfVec2d this[int index] {
		  get { return GetValue(index); }
		  set { SetValue(index, value); }
	  }
  
  public VtVec2dArray() : this(UsdCsPINVOKE.new_VtVec2dArray__SWIG_0(), true) {
  }

  public VtVec2dArray(VtVec2dArray other) : this(UsdCsPINVOKE.new_VtVec2dArray__SWIG_1(VtVec2dArray.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public VtVec2dArray(uint n) : this(UsdCsPINVOKE.new_VtVec2dArray__SWIG_3(n), true) {
  }

  public VtVec2dArray(uint n, GfVec2d value) : this(UsdCsPINVOKE.new_VtVec2dArray__SWIG_4(n, GfVec2d.getCPtr(value)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void push_back(GfVec2d elem) {
    UsdCsPINVOKE.VtVec2dArray_push_back(swigCPtr, GfVec2d.getCPtr(elem));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void pop_back() {
    UsdCsPINVOKE.VtVec2dArray_pop_back(swigCPtr);
  }

  public uint size() {
    uint ret = UsdCsPINVOKE.VtVec2dArray_size(swigCPtr);
    return ret;
  }

  public uint capacity() {
    uint ret = UsdCsPINVOKE.VtVec2dArray_capacity(swigCPtr);
    return ret;
  }

  public bool empty() {
    bool ret = UsdCsPINVOKE.VtVec2dArray_empty(swigCPtr);
    return ret;
  }

  public void reserve(uint num) {
    UsdCsPINVOKE.VtVec2dArray_reserve(swigCPtr, num);
  }

  public void resize(uint newSize) {
    UsdCsPINVOKE.VtVec2dArray_resize(swigCPtr, newSize);
  }

  public void clear() {
    UsdCsPINVOKE.VtVec2dArray_clear(swigCPtr);
  }

  public void assign(uint n, GfVec2d fill) {
    UsdCsPINVOKE.VtVec2dArray_assign(swigCPtr, n, GfVec2d.getCPtr(fill));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void swap(VtVec2dArray other) {
    UsdCsPINVOKE.VtVec2dArray_swap(swigCPtr, VtVec2dArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool IsIdentical(VtVec2dArray other) {
    bool ret = UsdCsPINVOKE.VtVec2dArray_IsIdentical(swigCPtr, VtVec2dArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool Equals(VtVec2dArray lhs, VtVec2dArray rhs) {
    bool ret = UsdCsPINVOKE.VtVec2dArray_Equals(VtVec2dArray.getCPtr(lhs), VtVec2dArray.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override string ToString() {
    string ret = UsdCsPINVOKE.VtVec2dArray_ToString(swigCPtr);
    return ret;
  }

  public void CopyToArray(GfVec2d[] dest) {
    UsdCsPINVOKE.VtVec2dArray_CopyToArray__SWIG_0(swigCPtr, dest);
  }

  public void CopyFromArray(GfVec2d[] src) {
    UsdCsPINVOKE.VtVec2dArray_CopyFromArray__SWIG_0(swigCPtr, src);
  }

  public void CopyToArray(System.IntPtr dest) {
    UsdCsPINVOKE.VtVec2dArray_CopyToArray__SWIG_1(swigCPtr, dest);
  }

  public void CopyFromArray(System.IntPtr src) {
    UsdCsPINVOKE.VtVec2dArray_CopyFromArray__SWIG_1(swigCPtr, src);
  }

  protected GfVec2d GetValue(int index) {
    GfVec2d ret = new GfVec2d(UsdCsPINVOKE.VtVec2dArray_GetValue(swigCPtr, index), false);
    return ret;
  }

  protected void SetValue(int index, GfVec2d value) {
    UsdCsPINVOKE.VtVec2dArray_SetValue(swigCPtr, index, GfVec2d.getCPtr(value));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

}

}

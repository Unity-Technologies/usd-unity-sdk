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
public class VtVec4iArray : Vt_ArrayBase {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal VtVec4iArray(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.VtVec4iArray_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(VtVec4iArray obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~VtVec4iArray() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_VtVec4iArray(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

	  public GfVec4i this[int index] {
		  get { return GetValue(index); }
		  set { SetValue(index, value); }
	  }
  
  public VtVec4iArray() : this(UsdCsPINVOKE.new_VtVec4iArray__SWIG_0(), true) {
  }

  public VtVec4iArray(VtVec4iArray other) : this(UsdCsPINVOKE.new_VtVec4iArray__SWIG_1(VtVec4iArray.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public VtVec4iArray(uint n) : this(UsdCsPINVOKE.new_VtVec4iArray__SWIG_3(n), true) {
  }

  public VtVec4iArray(uint n, GfVec4i value) : this(UsdCsPINVOKE.new_VtVec4iArray__SWIG_4(n, GfVec4i.getCPtr(value)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void push_back(GfVec4i elem) {
    UsdCsPINVOKE.VtVec4iArray_push_back(swigCPtr, GfVec4i.getCPtr(elem));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void pop_back() {
    UsdCsPINVOKE.VtVec4iArray_pop_back(swigCPtr);
  }

  public uint size() {
    uint ret = UsdCsPINVOKE.VtVec4iArray_size(swigCPtr);
    return ret;
  }

  public uint capacity() {
    uint ret = UsdCsPINVOKE.VtVec4iArray_capacity(swigCPtr);
    return ret;
  }

  public bool empty() {
    bool ret = UsdCsPINVOKE.VtVec4iArray_empty(swigCPtr);
    return ret;
  }

  public void reserve(uint num) {
    UsdCsPINVOKE.VtVec4iArray_reserve(swigCPtr, num);
  }

  public void resize(uint newSize) {
    UsdCsPINVOKE.VtVec4iArray_resize(swigCPtr, newSize);
  }

  public void clear() {
    UsdCsPINVOKE.VtVec4iArray_clear(swigCPtr);
  }

  public void assign(uint n, GfVec4i fill) {
    UsdCsPINVOKE.VtVec4iArray_assign(swigCPtr, n, GfVec4i.getCPtr(fill));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void swap(VtVec4iArray other) {
    UsdCsPINVOKE.VtVec4iArray_swap(swigCPtr, VtVec4iArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool IsIdentical(VtVec4iArray other) {
    bool ret = UsdCsPINVOKE.VtVec4iArray_IsIdentical(swigCPtr, VtVec4iArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool Equals(VtVec4iArray lhs, VtVec4iArray rhs) {
    bool ret = UsdCsPINVOKE.VtVec4iArray_Equals(VtVec4iArray.getCPtr(lhs), VtVec4iArray.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override string ToString() {
    string ret = UsdCsPINVOKE.VtVec4iArray_ToString(swigCPtr);
    return ret;
  }

  public void CopyToArray(GfVec4i[] dest) {
    UsdCsPINVOKE.VtVec4iArray_CopyToArray__SWIG_0(swigCPtr, dest);
  }

  public void CopyFromArray(GfVec4i[] src) {
    UsdCsPINVOKE.VtVec4iArray_CopyFromArray__SWIG_0(swigCPtr, src);
  }

  public void CopyToArray(System.IntPtr dest) {
    UsdCsPINVOKE.VtVec4iArray_CopyToArray__SWIG_1(swigCPtr, dest);
  }

  public void CopyFromArray(System.IntPtr src) {
    UsdCsPINVOKE.VtVec4iArray_CopyFromArray__SWIG_1(swigCPtr, src);
  }

  protected GfVec4i GetValue(int index) {
    GfVec4i ret = new GfVec4i(UsdCsPINVOKE.VtVec4iArray_GetValue(swigCPtr, index), false);
    return ret;
  }

  protected void SetValue(int index, GfVec4i value) {
    UsdCsPINVOKE.VtVec4iArray_SetValue(swigCPtr, index, GfVec4i.getCPtr(value));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

}

}
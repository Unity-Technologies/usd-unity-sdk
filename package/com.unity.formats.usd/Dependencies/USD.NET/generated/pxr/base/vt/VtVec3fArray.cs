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

[Preserve]
public class VtVec3fArray : Vt_ArrayBase {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal VtVec3fArray(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.VtVec3fArray_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(VtVec3fArray obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_VtVec3fArray(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

	  public GfVec3f this[int index] {
		  get { return GetValue(index); }
		  set { SetValue(index, value); }
	  }
  
  public VtVec3fArray() : this(UsdCsPINVOKE.new_VtVec3fArray__SWIG_0(), true) {
  }

  public VtVec3fArray(VtVec3fArray other) : this(UsdCsPINVOKE.new_VtVec3fArray__SWIG_1(VtVec3fArray.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public VtVec3fArray(uint n) : this(UsdCsPINVOKE.new_VtVec3fArray__SWIG_3(n), true) {
  }

  public VtVec3fArray(uint n, GfVec3f value) : this(UsdCsPINVOKE.new_VtVec3fArray__SWIG_4(n, GfVec3f.getCPtr(value)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void push_back(GfVec3f elem) {
    UsdCsPINVOKE.VtVec3fArray_push_back(swigCPtr, GfVec3f.getCPtr(elem));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void pop_back() {
    UsdCsPINVOKE.VtVec3fArray_pop_back(swigCPtr);
  }

  public uint size() {
    uint ret = UsdCsPINVOKE.VtVec3fArray_size(swigCPtr);
    return ret;
  }

  public uint capacity() {
    uint ret = UsdCsPINVOKE.VtVec3fArray_capacity(swigCPtr);
    return ret;
  }

  public bool empty() {
    bool ret = UsdCsPINVOKE.VtVec3fArray_empty(swigCPtr);
    return ret;
  }

  public void reserve(uint num) {
    UsdCsPINVOKE.VtVec3fArray_reserve(swigCPtr, num);
  }

  public void resize(uint newSize) {
    UsdCsPINVOKE.VtVec3fArray_resize(swigCPtr, newSize);
  }

  public void clear() {
    UsdCsPINVOKE.VtVec3fArray_clear(swigCPtr);
  }

  public void assign(uint n, GfVec3f fill) {
    UsdCsPINVOKE.VtVec3fArray_assign(swigCPtr, n, GfVec3f.getCPtr(fill));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void swap(VtVec3fArray other) {
    UsdCsPINVOKE.VtVec3fArray_swap(swigCPtr, VtVec3fArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool IsIdentical(VtVec3fArray other) {
    bool ret = UsdCsPINVOKE.VtVec3fArray_IsIdentical(swigCPtr, VtVec3fArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool Equals(VtVec3fArray lhs, VtVec3fArray rhs) {
    bool ret = UsdCsPINVOKE.VtVec3fArray_Equals(VtVec3fArray.getCPtr(lhs), VtVec3fArray.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override string ToString() {
    string ret = UsdCsPINVOKE.VtVec3fArray_ToString(swigCPtr);
    return ret;
  }

  public void CopyToArray(GfVec3f[] dest) {
    UsdCsPINVOKE.VtVec3fArray_CopyToArray__SWIG_0(swigCPtr, dest);
  }

  public void CopyFromArray(GfVec3f[] src) {
    UsdCsPINVOKE.VtVec3fArray_CopyFromArray__SWIG_0(swigCPtr, src);
  }

  public void CopyToArray(System.IntPtr dest) {
    UsdCsPINVOKE.VtVec3fArray_CopyToArray__SWIG_1(swigCPtr, dest);
  }

  public void CopyFromArray(System.IntPtr src) {
    UsdCsPINVOKE.VtVec3fArray_CopyFromArray__SWIG_1(swigCPtr, src);
  }

  protected GfVec3f GetValue(int index) {
    GfVec3f ret = new GfVec3f(UsdCsPINVOKE.VtVec3fArray_GetValue(swigCPtr, index), false);
    return ret;
  }

  protected void SetValue(int index, GfVec3f value) {
    UsdCsPINVOKE.VtVec3fArray_SetValue(swigCPtr, index, GfVec3f.getCPtr(value));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

}

}

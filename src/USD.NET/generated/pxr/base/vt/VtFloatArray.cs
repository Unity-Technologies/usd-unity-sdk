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
public class VtFloatArray : Vt_ArrayBase {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal VtFloatArray(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.VtFloatArray_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(VtFloatArray obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~VtFloatArray() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_VtFloatArray(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

	  public float this[int index] {
		  get { return GetValue(index); }
		  set { SetValue(index, value); }
	  }
  
  public VtFloatArray() : this(UsdCsPINVOKE.new_VtFloatArray__SWIG_0(), true) {
  }

  public VtFloatArray(Vt_ArrayForeignDataSource foreignSrc, SWIGTYPE_p_float data, uint size, bool addRef) : this(UsdCsPINVOKE.new_VtFloatArray__SWIG_1(Vt_ArrayForeignDataSource.getCPtr(foreignSrc), SWIGTYPE_p_float.getCPtr(data), size, addRef), true) {
  }

  public VtFloatArray(Vt_ArrayForeignDataSource foreignSrc, SWIGTYPE_p_float data, uint size) : this(UsdCsPINVOKE.new_VtFloatArray__SWIG_2(Vt_ArrayForeignDataSource.getCPtr(foreignSrc), SWIGTYPE_p_float.getCPtr(data), size), true) {
  }

  public VtFloatArray(VtFloatArray other) : this(UsdCsPINVOKE.new_VtFloatArray__SWIG_3(VtFloatArray.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public VtFloatArray(uint n, float value) : this(UsdCsPINVOKE.new_VtFloatArray__SWIG_5(n, value), true) {
  }

  public VtFloatArray(uint n) : this(UsdCsPINVOKE.new_VtFloatArray__SWIG_6(n), true) {
  }

  public void push_back(float elem) {
    UsdCsPINVOKE.VtFloatArray_push_back(swigCPtr, elem);
  }

  public void pop_back() {
    UsdCsPINVOKE.VtFloatArray_pop_back(swigCPtr);
  }

  public uint size() {
    uint ret = UsdCsPINVOKE.VtFloatArray_size(swigCPtr);
    return ret;
  }

  public uint capacity() {
    uint ret = UsdCsPINVOKE.VtFloatArray_capacity(swigCPtr);
    return ret;
  }

  public bool empty() {
    bool ret = UsdCsPINVOKE.VtFloatArray_empty(swigCPtr);
    return ret;
  }

  public void reserve(uint num) {
    UsdCsPINVOKE.VtFloatArray_reserve(swigCPtr, num);
  }

  public void resize(uint newSize) {
    UsdCsPINVOKE.VtFloatArray_resize(swigCPtr, newSize);
  }

  public void clear() {
    UsdCsPINVOKE.VtFloatArray_clear(swigCPtr);
  }

  public void assign(uint n, float fill) {
    UsdCsPINVOKE.VtFloatArray_assign(swigCPtr, n, fill);
  }

  public void swap(VtFloatArray other) {
    UsdCsPINVOKE.VtFloatArray_swap(swigCPtr, VtFloatArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool IsIdentical(VtFloatArray other) {
    bool ret = UsdCsPINVOKE.VtFloatArray_IsIdentical(swigCPtr, VtFloatArray.getCPtr(other));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static bool Equals(VtFloatArray lhs, VtFloatArray rhs) {
    bool ret = UsdCsPINVOKE.VtFloatArray_Equals(VtFloatArray.getCPtr(lhs), VtFloatArray.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override string ToString() {
    string ret = UsdCsPINVOKE.VtFloatArray_ToString(swigCPtr);
    return ret;
  }

  public void CopyToArray(float[] dest) {
    UsdCsPINVOKE.VtFloatArray_CopyToArray__SWIG_0(swigCPtr, dest);
  }

  public void CopyFromArray(float[] src) {
    UsdCsPINVOKE.VtFloatArray_CopyFromArray__SWIG_0(swigCPtr, src);
  }

  public void CopyToArray(System.IntPtr dest) {
    UsdCsPINVOKE.VtFloatArray_CopyToArray__SWIG_1(swigCPtr, dest);
  }

  public void CopyFromArray(System.IntPtr src) {
    UsdCsPINVOKE.VtFloatArray_CopyFromArray__SWIG_1(swigCPtr, src);
  }

  protected float GetValue(int index) {
    float ret = UsdCsPINVOKE.VtFloatArray_GetValue(swigCPtr, index);
    return ret;
  }

  protected void SetValue(int index, float value) {
    UsdCsPINVOKE.VtFloatArray_SetValue(swigCPtr, index, value);
  }

}

}

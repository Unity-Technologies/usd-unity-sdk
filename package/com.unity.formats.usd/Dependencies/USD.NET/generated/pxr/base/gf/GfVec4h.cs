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

public class GfVec4h : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal GfVec4h(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(GfVec4h obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~GfVec4h() {
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
          UsdCsPINVOKE.delete_GfVec4h(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public GfVec4h() : this(UsdCsPINVOKE.new_GfVec4h__SWIG_0(), true) {
  }

  public GfVec4h(GfHalf value) : this(UsdCsPINVOKE.new_GfVec4h__SWIG_1(GfHalf.getCPtr(value)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public GfVec4h(GfHalf s0, GfHalf s1, GfHalf s2, GfHalf s3) : this(UsdCsPINVOKE.new_GfVec4h__SWIG_2(GfHalf.getCPtr(s0), GfHalf.getCPtr(s1), GfHalf.getCPtr(s2), GfHalf.getCPtr(s3)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public GfVec4h(GfVec4d other) : this(UsdCsPINVOKE.new_GfVec4h__SWIG_4(GfVec4d.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public GfVec4h(GfVec4f other) : this(UsdCsPINVOKE.new_GfVec4h__SWIG_5(GfVec4f.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public GfVec4h(GfVec4i other) : this(UsdCsPINVOKE.new_GfVec4h__SWIG_6(GfVec4i.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public static GfVec4h XAxis() {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_XAxis(), true);
    return ret;
  }

  public static GfVec4h YAxis() {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_YAxis(), true);
    return ret;
  }

  public static GfVec4h ZAxis() {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_ZAxis(), true);
    return ret;
  }

  public static GfVec4h WAxis() {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_WAxis(), true);
    return ret;
  }

  public static GfVec4h Axis(uint i) {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_Axis(i), true);
    return ret;
  }

  public GfVec4h Set(GfHalf s0, GfHalf s1, GfHalf s2, GfHalf s3) {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_Set__SWIG_0(swigCPtr, GfHalf.getCPtr(s0), GfHalf.getCPtr(s1), GfHalf.getCPtr(s2), GfHalf.getCPtr(s3)), false);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public GfVec4h Set(GfHalf a) {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_Set__SWIG_1(swigCPtr, GfHalf.getCPtr(a)), false);
    return ret;
  }

  public GfVec4h GetProjection(GfVec4h v) {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_GetProjection(swigCPtr, GfVec4h.getCPtr(v)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public GfVec4h GetComplement(GfVec4h b) {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_GetComplement(swigCPtr, GfVec4h.getCPtr(b)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public GfHalf GetLengthSq() {
    GfHalf ret = new GfHalf(UsdCsPINVOKE.GfVec4h_GetLengthSq(swigCPtr), true);
    return ret;
  }

  public GfHalf GetLength() {
    GfHalf ret = new GfHalf(UsdCsPINVOKE.GfVec4h_GetLength(swigCPtr), true);
    return ret;
  }

  public GfHalf Normalize(GfHalf eps) {
    GfHalf ret = new GfHalf(UsdCsPINVOKE.GfVec4h_Normalize__SWIG_0(swigCPtr, GfHalf.getCPtr(eps)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public GfHalf Normalize() {
    GfHalf ret = new GfHalf(UsdCsPINVOKE.GfVec4h_Normalize__SWIG_1(swigCPtr), true);
    return ret;
  }

  public GfVec4h GetNormalized(GfHalf eps) {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_GetNormalized__SWIG_0(swigCPtr, GfHalf.getCPtr(eps)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public GfVec4h GetNormalized() {
    GfVec4h ret = new GfVec4h(UsdCsPINVOKE.GfVec4h_GetNormalized__SWIG_1(swigCPtr), true);
    return ret;
  }

  public static bool Equals(GfVec4h lhs, GfVec4h rhs) {
    bool ret = UsdCsPINVOKE.GfVec4h_Equals(GfVec4h.getCPtr(lhs), GfVec4h.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  override public int GetHashCode() {
    int ret = UsdCsPINVOKE.GfVec4h_GetHashCode(swigCPtr);
    return ret;
  }

    public static bool operator==(GfVec4h lhs, GfVec4h rhs){
      // The Swig binding glue will re-enter this operator comparing to null, so 
      // that case must be handled explicitly to avoid an infinite loop. This is still
      // not great, since it crosses the C#/C++ barrier twice. A better approache might
      // be to return a simple value from C++ that can be compared in C#.
      bool lnull = lhs as object == null;
      bool rnull = rhs as object == null;
      return (lnull == rnull) && ((lnull && rnull) || GfVec4h.Equals(lhs, rhs));
    }

    public static bool operator !=(GfVec4h lhs, GfVec4h rhs) {
        return !(lhs == rhs);
    }

    override public bool Equals(object rhs) {
      return GfVec4h.Equals(this, rhs as GfVec4h);
    }
  
  protected float GetValue(int index) {
    float ret = UsdCsPINVOKE.GfVec4h_GetValue(swigCPtr, index);
    return ret;
  }

  protected void SetValue(int index, float value) {
    UsdCsPINVOKE.GfVec4h_SetValue(swigCPtr, index, value);
  }

  public float this[int index] {
    get { return GetValue(index); }
    set { SetValue(index, value); }
  }
  
  public static readonly uint dimension = UsdCsPINVOKE.GfVec4h_dimension_get();
}

}

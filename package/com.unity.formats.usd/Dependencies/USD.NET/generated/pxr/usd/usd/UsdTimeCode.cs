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

public class UsdTimeCode : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UsdTimeCode(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdTimeCode obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UsdTimeCode() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdTimeCode(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

    public static implicit operator UsdTimeCode (double value) {
        return new UsdTimeCode(value);
    }
  public static bool operator==(UsdTimeCode lhs, UsdTimeCode rhs){
    // The Swig binding glew will re-enter this operator comparing to null, so
    // that case must be handled explicitly to avoid an infinite loop. This is still
    // not great, since it crosses the C#/C++ barrier twice. A better approache might
    // be to return a simple value from C++ that can be compared in C#.
        bool lnull = lhs as object == null;
        bool rnull = rhs as object == null;
        return (lnull == rnull) && ((lnull && rnull) || UsdTimeCode.Equals(lhs, rhs));
    }
    public static bool operator !=(UsdTimeCode lhs, UsdTimeCode rhs) {
        return !(lhs == rhs);
    }
  override public bool Equals(object rhs) {
    return UsdTimeCode.Equals(this, rhs as UsdTimeCode);
  }

  public UsdTimeCode(double t) : this(UsdCsPINVOKE.new_UsdTimeCode__SWIG_0(t), true) {
  }

  public UsdTimeCode() : this(UsdCsPINVOKE.new_UsdTimeCode__SWIG_1(), true) {
  }

  public UsdTimeCode(SdfTimeCode timeCode) : this(UsdCsPINVOKE.new_UsdTimeCode__SWIG_2(SdfTimeCode.getCPtr(timeCode)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public static UsdTimeCode EarliestTime() {
    UsdTimeCode ret = new UsdTimeCode(UsdCsPINVOKE.UsdTimeCode_EarliestTime(), true);
    return ret;
  }

  public static UsdTimeCode Default() {
    UsdTimeCode ret = new UsdTimeCode(UsdCsPINVOKE.UsdTimeCode_Default(), true);
    return ret;
  }

  public static double SafeStep(double maxValue, double maxCompression) {
    double ret = UsdCsPINVOKE.UsdTimeCode_SafeStep__SWIG_0(maxValue, maxCompression);
    return ret;
  }

  public static double SafeStep(double maxValue) {
    double ret = UsdCsPINVOKE.UsdTimeCode_SafeStep__SWIG_1(maxValue);
    return ret;
  }

  public static double SafeStep() {
    double ret = UsdCsPINVOKE.UsdTimeCode_SafeStep__SWIG_2();
    return ret;
  }

  public bool IsEarliestTime() {
    bool ret = UsdCsPINVOKE.UsdTimeCode_IsEarliestTime(swigCPtr);
    return ret;
  }

  public bool IsDefault() {
    bool ret = UsdCsPINVOKE.UsdTimeCode_IsDefault(swigCPtr);
    return ret;
  }

  public bool IsNumeric() {
    bool ret = UsdCsPINVOKE.UsdTimeCode_IsNumeric(swigCPtr);
    return ret;
  }

  public double GetValue() {
    double ret = UsdCsPINVOKE.UsdTimeCode_GetValue(swigCPtr);
    return ret;
  }

  override public int GetHashCode() {
    int ret = UsdCsPINVOKE.UsdTimeCode_GetHashCode(swigCPtr);
    return ret;
  }

}

}

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

public class UsdShadeUtils : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UsdShadeUtils(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdShadeUtils obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UsdShadeUtils() {
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
          UsdCsPINVOKE.delete_UsdShadeUtils(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static string GetPrefixForAttributeType(SWIGTYPE_p_UsdShadeAttributeType sourceType) {
    string ret = UsdCsPINVOKE.UsdShadeUtils_GetPrefixForAttributeType(SWIGTYPE_p_UsdShadeAttributeType.getCPtr(sourceType));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SWIGTYPE_p_UsdShadeAttributeType GetType(TfToken fullName) {
    SWIGTYPE_p_UsdShadeAttributeType ret = new SWIGTYPE_p_UsdShadeAttributeType(UsdCsPINVOKE.UsdShadeUtils_GetType(TfToken.getCPtr(fullName)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TfToken GetFullName(TfToken baseName, SWIGTYPE_p_UsdShadeAttributeType type) {
    TfToken ret = new TfToken(UsdCsPINVOKE.UsdShadeUtils_GetFullName(TfToken.getCPtr(baseName), SWIGTYPE_p_UsdShadeAttributeType.getCPtr(type)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SWIGTYPE_p_UsdShadeAttributeVector GetValueProducingAttributes(UsdShadeInput input, bool shaderOutputsOnly) {
    SWIGTYPE_p_UsdShadeAttributeVector ret = new SWIGTYPE_p_UsdShadeAttributeVector(UsdCsPINVOKE.UsdShadeUtils_GetValueProducingAttributes__SWIG_0(UsdShadeInput.getCPtr(input), shaderOutputsOnly), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SWIGTYPE_p_UsdShadeAttributeVector GetValueProducingAttributes(UsdShadeInput input) {
    SWIGTYPE_p_UsdShadeAttributeVector ret = new SWIGTYPE_p_UsdShadeAttributeVector(UsdCsPINVOKE.UsdShadeUtils_GetValueProducingAttributes__SWIG_1(UsdShadeInput.getCPtr(input)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SWIGTYPE_p_UsdShadeAttributeVector GetValueProducingAttributes(UsdShadeOutput output, bool shaderOutputsOnly) {
    SWIGTYPE_p_UsdShadeAttributeVector ret = new SWIGTYPE_p_UsdShadeAttributeVector(UsdCsPINVOKE.UsdShadeUtils_GetValueProducingAttributes__SWIG_2(UsdShadeOutput.getCPtr(output), shaderOutputsOnly), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SWIGTYPE_p_UsdShadeAttributeVector GetValueProducingAttributes(UsdShadeOutput output) {
    SWIGTYPE_p_UsdShadeAttributeVector ret = new SWIGTYPE_p_UsdShadeAttributeVector(UsdCsPINVOKE.UsdShadeUtils_GetValueProducingAttributes__SWIG_3(UsdShadeOutput.getCPtr(output)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdShadeUtils() : this(UsdCsPINVOKE.new_UsdShadeUtils(), true) {
  }

}

}

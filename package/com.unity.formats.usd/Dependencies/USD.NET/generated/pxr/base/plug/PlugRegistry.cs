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

public class PlugRegistry : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal PlugRegistry(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(PlugRegistry obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~PlugRegistry() {
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
          throw new global::System.MethodAccessException("C++ destructor does not have public access");
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static PlugRegistry GetInstance() {
    PlugRegistry ret = new PlugRegistry(UsdCsPINVOKE.PlugRegistry_GetInstance(), false);
    return ret;
  }

  public SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector RegisterPlugins(string pathToPlugInfo) {
    SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector ret = new SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector(UsdCsPINVOKE.PlugRegistry_RegisterPlugins__SWIG_0(swigCPtr, pathToPlugInfo), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector RegisterPlugins(StdStringVector pathsToPlugInfo) {
    SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector ret = new SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector(UsdCsPINVOKE.PlugRegistry_RegisterPlugins__SWIG_1(swigCPtr, StdStringVector.getCPtr(pathsToPlugInfo)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TfType FindTypeByName(string typeName) {
    TfType ret = new TfType(UsdCsPINVOKE.PlugRegistry_FindTypeByName(typeName), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TfType FindDerivedTypeByName(TfType base_, string typeName) {
    TfType ret = new TfType(UsdCsPINVOKE.PlugRegistry_FindDerivedTypeByName(TfType.getCPtr(base_), typeName), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TfTypeVector GetDirectlyDerivedTypes(TfType base_) {
    TfTypeVector ret = new TfTypeVector(UsdCsPINVOKE.PlugRegistry_GetDirectlyDerivedTypes(TfType.getCPtr(base_)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void GetAllDerivedTypes(TfType base_, SWIGTYPE_p_std__setT_TfType_t result) {
    UsdCsPINVOKE.PlugRegistry_GetAllDerivedTypes(TfType.getCPtr(base_), SWIGTYPE_p_std__setT_TfType_t.getCPtr(result));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__Ptr GetPluginForType(TfType t) {
    SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__Ptr ret = new SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__Ptr(UsdCsPINVOKE.PlugRegistry_GetPluginForType(swigCPtr, TfType.getCPtr(t)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector GetAllPlugins() {
    SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector ret = new SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__PtrVector(UsdCsPINVOKE.PlugRegistry_GetAllPlugins(swigCPtr), true);
    return ret;
  }

  public SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__Ptr GetPluginWithName(string name) {
    SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__Ptr ret = new SWIGTYPE_p_TfDeclarePtrsT_PlugPlugin_t__Ptr(UsdCsPINVOKE.PlugRegistry_GetPluginWithName(swigCPtr, name), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public string GetStringFromPluginMetaData(TfType type, string key) {
    string ret = UsdCsPINVOKE.PlugRegistry_GetStringFromPluginMetaData(swigCPtr, TfType.getCPtr(type), key);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public JsValue GetDataFromPluginMetaData(TfType type, string key) {
    JsValue ret = new JsValue(UsdCsPINVOKE.PlugRegistry_GetDataFromPluginMetaData(swigCPtr, TfType.getCPtr(type), key), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}

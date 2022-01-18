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

public class UsdRiMaterialAPI : UsdAPISchemaBase {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdRiMaterialAPI(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdRiMaterialAPI_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdRiMaterialAPI obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdRiMaterialAPI(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdRiMaterialAPI(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdRiMaterialAPI__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdRiMaterialAPI() : this(UsdCsPINVOKE.new_UsdRiMaterialAPI__SWIG_1(), true) {
  }

  public UsdRiMaterialAPI(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdRiMaterialAPI__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdRiMaterialAPI_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdRiMaterialAPI_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public static UsdRiMaterialAPI Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdRiMaterialAPI ret = new UsdRiMaterialAPI(UsdCsPINVOKE.UsdRiMaterialAPI_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdRiMaterialAPI Apply(UsdPrim prim) {
    UsdRiMaterialAPI ret = new UsdRiMaterialAPI(UsdCsPINVOKE.UsdRiMaterialAPI_Apply(UsdPrim.getCPtr(prim)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute GetSurfaceAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_GetSurfaceAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateSurfaceAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateSurfaceAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateSurfaceAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateSurfaceAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateSurfaceAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateSurfaceAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetDisplacementAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_GetDisplacementAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateDisplacementAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateDisplacementAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateDisplacementAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateDisplacementAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateDisplacementAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateDisplacementAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetVolumeAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_GetVolumeAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateVolumeAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateVolumeAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateVolumeAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateVolumeAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateVolumeAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdRiMaterialAPI_CreateVolumeAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdRiMaterialAPI(UsdShadeMaterial material) : this(UsdCsPINVOKE.new_UsdRiMaterialAPI__SWIG_3(UsdShadeMaterial.getCPtr(material)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdShadeOutput GetSurfaceOutput() {
    UsdShadeOutput ret = new UsdShadeOutput(UsdCsPINVOKE.UsdRiMaterialAPI_GetSurfaceOutput(swigCPtr), true);
    return ret;
  }

  public UsdShadeOutput GetDisplacementOutput() {
    UsdShadeOutput ret = new UsdShadeOutput(UsdCsPINVOKE.UsdRiMaterialAPI_GetDisplacementOutput(swigCPtr), true);
    return ret;
  }

  public UsdShadeOutput GetVolumeOutput() {
    UsdShadeOutput ret = new UsdShadeOutput(UsdCsPINVOKE.UsdRiMaterialAPI_GetVolumeOutput(swigCPtr), true);
    return ret;
  }

  public bool SetSurfaceSource(SdfPath surfacePath) {
    bool ret = UsdCsPINVOKE.UsdRiMaterialAPI_SetSurfaceSource(swigCPtr, SdfPath.getCPtr(surfacePath));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool SetDisplacementSource(SdfPath displacementPath) {
    bool ret = UsdCsPINVOKE.UsdRiMaterialAPI_SetDisplacementSource(swigCPtr, SdfPath.getCPtr(displacementPath));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool SetVolumeSource(SdfPath volumePath) {
    bool ret = UsdCsPINVOKE.UsdRiMaterialAPI_SetVolumeSource(swigCPtr, SdfPath.getCPtr(volumePath));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdShadeShader GetSurface(bool ignoreBaseMaterial) {
    UsdShadeShader ret = new UsdShadeShader(UsdCsPINVOKE.UsdRiMaterialAPI_GetSurface__SWIG_0(swigCPtr, ignoreBaseMaterial), true);
    return ret;
  }

  public UsdShadeShader GetSurface() {
    UsdShadeShader ret = new UsdShadeShader(UsdCsPINVOKE.UsdRiMaterialAPI_GetSurface__SWIG_1(swigCPtr), true);
    return ret;
  }

  public UsdShadeShader GetDisplacement(bool ignoreBaseMaterial) {
    UsdShadeShader ret = new UsdShadeShader(UsdCsPINVOKE.UsdRiMaterialAPI_GetDisplacement__SWIG_0(swigCPtr, ignoreBaseMaterial), true);
    return ret;
  }

  public UsdShadeShader GetDisplacement() {
    UsdShadeShader ret = new UsdShadeShader(UsdCsPINVOKE.UsdRiMaterialAPI_GetDisplacement__SWIG_1(swigCPtr), true);
    return ret;
  }

  public UsdShadeShader GetVolume(bool ignoreBaseMaterial) {
    UsdShadeShader ret = new UsdShadeShader(UsdCsPINVOKE.UsdRiMaterialAPI_GetVolume__SWIG_0(swigCPtr, ignoreBaseMaterial), true);
    return ret;
  }

  public UsdShadeShader GetVolume() {
    UsdShadeShader ret = new UsdShadeShader(UsdCsPINVOKE.UsdRiMaterialAPI_GetVolume__SWIG_1(swigCPtr), true);
    return ret;
  }

  public SWIGTYPE_p_std__unordered_mapT_UsdShadeInput_std__vectorT_UsdShadeInput_t_UsdShadeInput__Hash_t ComputeInterfaceInputConsumersMap(bool computeTransitiveConsumers) {
    SWIGTYPE_p_std__unordered_mapT_UsdShadeInput_std__vectorT_UsdShadeInput_t_UsdShadeInput__Hash_t ret = new SWIGTYPE_p_std__unordered_mapT_UsdShadeInput_std__vectorT_UsdShadeInput_t_UsdShadeInput__Hash_t(UsdCsPINVOKE.UsdRiMaterialAPI_ComputeInterfaceInputConsumersMap__SWIG_0(swigCPtr, computeTransitiveConsumers), true);
    return ret;
  }

  public SWIGTYPE_p_std__unordered_mapT_UsdShadeInput_std__vectorT_UsdShadeInput_t_UsdShadeInput__Hash_t ComputeInterfaceInputConsumersMap() {
    SWIGTYPE_p_std__unordered_mapT_UsdShadeInput_std__vectorT_UsdShadeInput_t_UsdShadeInput__Hash_t ret = new SWIGTYPE_p_std__unordered_mapT_UsdShadeInput_std__vectorT_UsdShadeInput_t_UsdShadeInput__Hash_t(UsdCsPINVOKE.UsdRiMaterialAPI_ComputeInterfaceInputConsumersMap__SWIG_1(swigCPtr), true);
    return ret;
  }

}

}

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

public class UsdSkelAnimation : UsdTyped {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal UsdSkelAnimation(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.UsdSkelAnimation_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdSkelAnimation obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          UsdCsPINVOKE.delete_UsdSkelAnimation(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public UsdSkelAnimation(UsdPrim prim) : this(UsdCsPINVOKE.new_UsdSkelAnimation__SWIG_0(UsdPrim.getCPtr(prim)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdSkelAnimation() : this(UsdCsPINVOKE.new_UsdSkelAnimation__SWIG_1(), true) {
  }

  public UsdSkelAnimation(UsdSchemaBase schemaObj) : this(UsdCsPINVOKE.new_UsdSkelAnimation__SWIG_2(UsdSchemaBase.getCPtr(schemaObj)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public new static TfTokenVector GetSchemaAttributeNames(bool includeInherited) {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdSkelAnimation_GetSchemaAttributeNames__SWIG_0(includeInherited), false);
    return ret;
  }

  public new static TfTokenVector GetSchemaAttributeNames() {
    TfTokenVector ret = new TfTokenVector(UsdCsPINVOKE.UsdSkelAnimation_GetSchemaAttributeNames__SWIG_1(), false);
    return ret;
  }

  public new static UsdSkelAnimation Get(UsdStageWeakPtr stage, SdfPath path) {
    UsdSkelAnimation ret = new UsdSkelAnimation(UsdCsPINVOKE.UsdSkelAnimation_Get(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UsdSkelAnimation Define(UsdStageWeakPtr stage, SdfPath path) {
    UsdSkelAnimation ret = new UsdSkelAnimation(UsdCsPINVOKE.UsdSkelAnimation_Define(UsdStageWeakPtr.getCPtr(stage), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute GetJointsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_GetJointsAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateJointsAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateJointsAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateJointsAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateJointsAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateJointsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateJointsAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetTranslationsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_GetTranslationsAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateTranslationsAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateTranslationsAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateTranslationsAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateTranslationsAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateTranslationsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateTranslationsAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetRotationsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_GetRotationsAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateRotationsAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateRotationsAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateRotationsAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateRotationsAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateRotationsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateRotationsAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetScalesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_GetScalesAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateScalesAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateScalesAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateScalesAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateScalesAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateScalesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateScalesAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetBlendShapesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_GetBlendShapesAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateBlendShapesAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateBlendShapesAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateBlendShapesAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateBlendShapesAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateBlendShapesAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateBlendShapesAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public UsdAttribute GetBlendShapeWeightsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_GetBlendShapeWeightsAttr(swigCPtr), true);
    return ret;
  }

  public UsdAttribute CreateBlendShapeWeightsAttr(VtValue defaultValue, bool writeSparsely) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateBlendShapeWeightsAttr__SWIG_0(swigCPtr, VtValue.getCPtr(defaultValue), writeSparsely), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateBlendShapeWeightsAttr(VtValue defaultValue) {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateBlendShapeWeightsAttr__SWIG_1(swigCPtr, VtValue.getCPtr(defaultValue)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public UsdAttribute CreateBlendShapeWeightsAttr() {
    UsdAttribute ret = new UsdAttribute(UsdCsPINVOKE.UsdSkelAnimation_CreateBlendShapeWeightsAttr__SWIG_2(swigCPtr), true);
    return ret;
  }

  public bool GetTransforms(VtMatrix4dArray xforms, UsdTimeCode time) {
    bool ret = UsdCsPINVOKE.UsdSkelAnimation_GetTransforms__SWIG_0(swigCPtr, VtMatrix4dArray.getCPtr(xforms), UsdTimeCode.getCPtr(time));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool GetTransforms(VtMatrix4dArray xforms) {
    bool ret = UsdCsPINVOKE.UsdSkelAnimation_GetTransforms__SWIG_1(swigCPtr, VtMatrix4dArray.getCPtr(xforms));
    return ret;
  }

  public bool SetTransforms(VtMatrix4dArray xforms, UsdTimeCode time) {
    bool ret = UsdCsPINVOKE.UsdSkelAnimation_SetTransforms__SWIG_0(swigCPtr, VtMatrix4dArray.getCPtr(xforms), UsdTimeCode.getCPtr(time));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool SetTransforms(VtMatrix4dArray xforms) {
    bool ret = UsdCsPINVOKE.UsdSkelAnimation_SetTransforms__SWIG_1(swigCPtr, VtMatrix4dArray.getCPtr(xforms));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}

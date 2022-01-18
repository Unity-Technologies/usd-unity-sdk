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

public class PcpPrimIndex : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal PcpPrimIndex(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(PcpPrimIndex obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~PcpPrimIndex() {
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
          UsdCsPINVOKE.delete_PcpPrimIndex(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public PcpPrimIndex() : this(UsdCsPINVOKE.new_PcpPrimIndex__SWIG_0(), true) {
  }

  public PcpPrimIndex(PcpPrimIndex rhs) : this(UsdCsPINVOKE.new_PcpPrimIndex__SWIG_1(PcpPrimIndex.getCPtr(rhs)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Swap(PcpPrimIndex rhs) {
    UsdCsPINVOKE.PcpPrimIndex_Swap(swigCPtr, PcpPrimIndex.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void swap(PcpPrimIndex rhs) {
    UsdCsPINVOKE.PcpPrimIndex_swap(swigCPtr, PcpPrimIndex.getCPtr(rhs));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool IsValid() {
    bool ret = UsdCsPINVOKE.PcpPrimIndex_IsValid(swigCPtr);
    return ret;
  }

  public void SetGraph(SWIGTYPE_p_TfDeclarePtrsT_PcpPrimIndex_Graph_t__RefPtr graph) {
    UsdCsPINVOKE.PcpPrimIndex_SetGraph(swigCPtr, SWIGTYPE_p_TfDeclarePtrsT_PcpPrimIndex_Graph_t__RefPtr.getCPtr(graph));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public SWIGTYPE_p_TfDeclarePtrsT_PcpPrimIndex_Graph_t__Ptr GetGraph() {
    SWIGTYPE_p_TfDeclarePtrsT_PcpPrimIndex_Graph_t__Ptr ret = new SWIGTYPE_p_TfDeclarePtrsT_PcpPrimIndex_Graph_t__Ptr(UsdCsPINVOKE.PcpPrimIndex_GetGraph(swigCPtr), true);
    return ret;
  }

  public PcpNodeRef GetRootNode() {
    PcpNodeRef ret = new PcpNodeRef(UsdCsPINVOKE.PcpPrimIndex_GetRootNode(swigCPtr), true);
    return ret;
  }

  public SdfPath GetPath() {
    SdfPath ret = new SdfPath(UsdCsPINVOKE.PcpPrimIndex_GetPath(swigCPtr), false);
    return ret;
  }

  public bool HasSpecs() {
    bool ret = UsdCsPINVOKE.PcpPrimIndex_HasSpecs(swigCPtr);
    return ret;
  }

  public bool HasAnyPayloads() {
    bool ret = UsdCsPINVOKE.PcpPrimIndex_HasAnyPayloads(swigCPtr);
    return ret;
  }

  public bool IsUsd() {
    bool ret = UsdCsPINVOKE.PcpPrimIndex_IsUsd(swigCPtr);
    return ret;
  }

  public bool IsInstanceable() {
    bool ret = UsdCsPINVOKE.PcpPrimIndex_IsInstanceable(swigCPtr);
    return ret;
  }

  public SWIGTYPE_p_PcpNodeRange GetNodeRange(PcpRangeType rangeType) {
    SWIGTYPE_p_PcpNodeRange ret = new SWIGTYPE_p_PcpNodeRange(UsdCsPINVOKE.PcpPrimIndex_GetNodeRange__SWIG_0(swigCPtr, (int)rangeType), true);
    return ret;
  }

  public SWIGTYPE_p_PcpNodeRange GetNodeRange() {
    SWIGTYPE_p_PcpNodeRange ret = new SWIGTYPE_p_PcpNodeRange(UsdCsPINVOKE.PcpPrimIndex_GetNodeRange__SWIG_1(swigCPtr), true);
    return ret;
  }

  public SWIGTYPE_p_PcpPrimRange GetPrimRange(PcpRangeType rangeType) {
    SWIGTYPE_p_PcpPrimRange ret = new SWIGTYPE_p_PcpPrimRange(UsdCsPINVOKE.PcpPrimIndex_GetPrimRange__SWIG_0(swigCPtr, (int)rangeType), true);
    return ret;
  }

  public SWIGTYPE_p_PcpPrimRange GetPrimRange() {
    SWIGTYPE_p_PcpPrimRange ret = new SWIGTYPE_p_PcpPrimRange(UsdCsPINVOKE.PcpPrimIndex_GetPrimRange__SWIG_1(swigCPtr), true);
    return ret;
  }

  public SWIGTYPE_p_PcpPrimRange GetPrimRangeForNode(PcpNodeRef node) {
    SWIGTYPE_p_PcpPrimRange ret = new SWIGTYPE_p_PcpPrimRange(UsdCsPINVOKE.PcpPrimIndex_GetPrimRangeForNode(swigCPtr, PcpNodeRef.getCPtr(node)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public PcpNodeRef GetNodeProvidingSpec(SdfPrimSpecHandle primSpec) {
    PcpNodeRef ret = new PcpNodeRef(UsdCsPINVOKE.PcpPrimIndex_GetNodeProvidingSpec__SWIG_0(swigCPtr, SdfPrimSpecHandle.getCPtr(primSpec)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public PcpNodeRef GetNodeProvidingSpec(SdfLayerHandle layer, SdfPath path) {
    PcpNodeRef ret = new PcpNodeRef(UsdCsPINVOKE.PcpPrimIndex_GetNodeProvidingSpec__SWIG_1(swigCPtr, SdfLayerHandle.getCPtr(layer), SdfPath.getCPtr(path)), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_PcpErrorVector GetLocalErrors() {
    SWIGTYPE_p_PcpErrorVector ret = new SWIGTYPE_p_PcpErrorVector(UsdCsPINVOKE.PcpPrimIndex_GetLocalErrors(swigCPtr), true);
    return ret;
  }

  public void PrintStatistics() {
    UsdCsPINVOKE.PcpPrimIndex_PrintStatistics(swigCPtr);
  }

  public string DumpToString(bool includeInheritOriginInfo, bool includeMaps) {
    string ret = UsdCsPINVOKE.PcpPrimIndex_DumpToString__SWIG_0(swigCPtr, includeInheritOriginInfo, includeMaps);
    return ret;
  }

  public string DumpToString(bool includeInheritOriginInfo) {
    string ret = UsdCsPINVOKE.PcpPrimIndex_DumpToString__SWIG_1(swigCPtr, includeInheritOriginInfo);
    return ret;
  }

  public string DumpToString() {
    string ret = UsdCsPINVOKE.PcpPrimIndex_DumpToString__SWIG_2(swigCPtr);
    return ret;
  }

  public void DumpToDotGraph(string filename, bool includeInheritOriginInfo, bool includeMaps) {
    UsdCsPINVOKE.PcpPrimIndex_DumpToDotGraph__SWIG_0(swigCPtr, filename, includeInheritOriginInfo, includeMaps);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void DumpToDotGraph(string filename, bool includeInheritOriginInfo) {
    UsdCsPINVOKE.PcpPrimIndex_DumpToDotGraph__SWIG_1(swigCPtr, filename, includeInheritOriginInfo);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void DumpToDotGraph(string filename) {
    UsdCsPINVOKE.PcpPrimIndex_DumpToDotGraph__SWIG_2(swigCPtr, filename);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void ComputePrimChildNames(TfTokenVector nameOrder, SWIGTYPE_p_TfDenseHashSetT_TfToken_TfToken__HashFunctor_t prohibitedNameSet) {
    UsdCsPINVOKE.PcpPrimIndex_ComputePrimChildNames(swigCPtr, TfTokenVector.getCPtr(nameOrder), SWIGTYPE_p_TfDenseHashSetT_TfToken_TfToken__HashFunctor_t.getCPtr(prohibitedNameSet));
  }

  public void ComputePrimPropertyNames(TfTokenVector nameOrder) {
    UsdCsPINVOKE.PcpPrimIndex_ComputePrimPropertyNames(swigCPtr, TfTokenVector.getCPtr(nameOrder));
  }

  public SWIGTYPE_p_std__mapT_std__string_std__string_std__lessT_std__string_t_t ComposeAuthoredVariantSelections() {
    SWIGTYPE_p_std__mapT_std__string_std__string_std__lessT_std__string_t_t ret = new SWIGTYPE_p_std__mapT_std__string_std__string_std__lessT_std__string_t_t(UsdCsPINVOKE.PcpPrimIndex_ComposeAuthoredVariantSelections(swigCPtr), true);
    return ret;
  }

  public string GetSelectionAppliedForVariantSet(string variantSet) {
    string ret = UsdCsPINVOKE.PcpPrimIndex_GetSelectionAppliedForVariantSet(swigCPtr, variantSet);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}

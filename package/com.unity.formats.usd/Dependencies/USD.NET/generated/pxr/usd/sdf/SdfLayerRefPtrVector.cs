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

public class SdfLayerRefPtrVector : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IEnumerable<SdfLayer>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal SdfLayerRefPtrVector(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(SdfLayerRefPtrVector obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~SdfLayerRefPtrVector() {
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
          UsdCsPINVOKE.delete_SdfLayerRefPtrVector(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public SdfLayerRefPtrVector(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (SdfLayer element in c) {
      this.Add(element);
    }
  }

  public SdfLayerRefPtrVector(global::System.Collections.Generic.IEnumerable<SdfLayer> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (SdfLayer element in c) {
      this.Add(element);
    }
  }

  public bool IsFixedSize {
    get {
      return false;
    }
  }

  public bool IsReadOnly {
    get {
      return false;
    }
  }

  public SdfLayer this[int index]  {
    get {
      return getitem(index);
    }
    set {
      setitem(index, value);
    }
  }

  public int Capacity {
    get {
      return (int)capacity();
    }
    set {
      if (value < size())
        throw new global::System.ArgumentOutOfRangeException("Capacity");
      reserve((uint)value);
    }
  }

  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }

  public void CopyTo(SdfLayer[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(SdfLayer[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, SdfLayer[] array, int arrayIndex, int count)
  {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (index < 0)
      throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (count < 0)
      throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (index+count > this.Count || arrayIndex+count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");
    for (int i=0; i<count; i++)
      array.SetValue(getitemcopy(index+i), arrayIndex+i);
  }

  public SdfLayer[] ToArray() {
    SdfLayer[] array = new SdfLayer[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<SdfLayer> global::System.Collections.Generic.IEnumerable<SdfLayer>.GetEnumerator() {
    return new SdfLayerRefPtrVectorEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new SdfLayerRefPtrVectorEnumerator(this);
  }

  public SdfLayerRefPtrVectorEnumerator GetEnumerator() {
    return new SdfLayerRefPtrVectorEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class SdfLayerRefPtrVectorEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<SdfLayer>
  {
    private SdfLayerRefPtrVector collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public SdfLayerRefPtrVectorEnumerator(SdfLayerRefPtrVector collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public SdfLayer Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (SdfLayer)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = collectionRef.Count;
      bool moveOkay = (currentIndex+1 < size) && (size == currentSize);
      if (moveOkay) {
        currentIndex++;
        currentObject = collectionRef[currentIndex];
      } else {
        currentObject = null;
      }
      return moveOkay;
    }

    public void Reset() {
      currentIndex = -1;
      currentObject = null;
      if (collectionRef.Count != currentSize) {
        throw new global::System.InvalidOperationException("Collection modified.");
      }
    }

    public void Dispose() {
        currentIndex = -1;
        currentObject = null;
    }
  }

  public void Clear() {
    UsdCsPINVOKE.SdfLayerRefPtrVector_Clear(swigCPtr);
  }

  public void Add(SdfLayer x) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_Add(swigCPtr, SdfLayer.getCPtr(x));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = UsdCsPINVOKE.SdfLayerRefPtrVector_size(swigCPtr);
    return ret;
  }

  private uint capacity() {
    uint ret = UsdCsPINVOKE.SdfLayerRefPtrVector_capacity(swigCPtr);
    return ret;
  }

  private void reserve(uint n) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_reserve(swigCPtr, n);
  }

  public SdfLayerRefPtrVector() : this(UsdCsPINVOKE.new_SdfLayerRefPtrVector__SWIG_0(), true) {
  }

  public SdfLayerRefPtrVector(SdfLayerRefPtrVector other) : this(UsdCsPINVOKE.new_SdfLayerRefPtrVector__SWIG_1(SdfLayerRefPtrVector.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public SdfLayerRefPtrVector(int capacity) : this(UsdCsPINVOKE.new_SdfLayerRefPtrVector__SWIG_2(capacity), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  private SdfLayer getitemcopy(int index) {
    global::System.IntPtr cPtr = UsdCsPINVOKE.SdfLayerRefPtrVector_getitemcopy(swigCPtr, index);
    SdfLayer ret = (cPtr == global::System.IntPtr.Zero) ? null : new SdfLayer(cPtr, true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private SdfLayer getitem(int index) {
    global::System.IntPtr cPtr = UsdCsPINVOKE.SdfLayerRefPtrVector_getitem(swigCPtr, index);
    SdfLayer ret = (cPtr == global::System.IntPtr.Zero) ? null : new SdfLayer(cPtr, true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, SdfLayer val) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_setitem(swigCPtr, index, SdfLayer.getCPtr(val));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(SdfLayerRefPtrVector values) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_AddRange(swigCPtr, SdfLayerRefPtrVector.getCPtr(values));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public SdfLayerRefPtrVector GetRange(int index, int count) {
    global::System.IntPtr cPtr = UsdCsPINVOKE.SdfLayerRefPtrVector_GetRange(swigCPtr, index, count);
    SdfLayerRefPtrVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new SdfLayerRefPtrVector(cPtr, true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, SdfLayer x) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_Insert(swigCPtr, index, SdfLayer.getCPtr(x));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, SdfLayerRefPtrVector values) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_InsertRange(swigCPtr, index, SdfLayerRefPtrVector.getCPtr(values));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_RemoveAt(swigCPtr, index);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_RemoveRange(swigCPtr, index, count);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public static SdfLayerRefPtrVector Repeat(SdfLayer value, int count) {
    global::System.IntPtr cPtr = UsdCsPINVOKE.SdfLayerRefPtrVector_Repeat(SdfLayer.getCPtr(value), count);
    SdfLayerRefPtrVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new SdfLayerRefPtrVector(cPtr, true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    UsdCsPINVOKE.SdfLayerRefPtrVector_Reverse__SWIG_0(swigCPtr);
  }

  public void Reverse(int index, int count) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_Reverse__SWIG_1(swigCPtr, index, count);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, SdfLayerRefPtrVector values) {
    UsdCsPINVOKE.SdfLayerRefPtrVector_SetRange(swigCPtr, index, SdfLayerRefPtrVector.getCPtr(values));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

}

}

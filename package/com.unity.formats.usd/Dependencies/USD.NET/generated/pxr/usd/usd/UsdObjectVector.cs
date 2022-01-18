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

public class UsdObjectVector : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IEnumerable<UsdObject>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UsdObjectVector(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UsdObjectVector obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UsdObjectVector() {
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
          UsdCsPINVOKE.delete_UsdObjectVector(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public UsdObjectVector(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (UsdObject element in c) {
      this.Add(element);
    }
  }

  public UsdObjectVector(global::System.Collections.Generic.IEnumerable<UsdObject> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (UsdObject element in c) {
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

  public UsdObject this[int index]  {
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

  public void CopyTo(UsdObject[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(UsdObject[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, UsdObject[] array, int arrayIndex, int count)
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

  public UsdObject[] ToArray() {
    UsdObject[] array = new UsdObject[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<UsdObject> global::System.Collections.Generic.IEnumerable<UsdObject>.GetEnumerator() {
    return new UsdObjectVectorEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new UsdObjectVectorEnumerator(this);
  }

  public UsdObjectVectorEnumerator GetEnumerator() {
    return new UsdObjectVectorEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class UsdObjectVectorEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<UsdObject>
  {
    private UsdObjectVector collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public UsdObjectVectorEnumerator(UsdObjectVector collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public UsdObject Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (UsdObject)currentObject;
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
    UsdCsPINVOKE.UsdObjectVector_Clear(swigCPtr);
  }

  public void Add(UsdObject x) {
    UsdCsPINVOKE.UsdObjectVector_Add(swigCPtr, UsdObject.getCPtr(x));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = UsdCsPINVOKE.UsdObjectVector_size(swigCPtr);
    return ret;
  }

  private uint capacity() {
    uint ret = UsdCsPINVOKE.UsdObjectVector_capacity(swigCPtr);
    return ret;
  }

  private void reserve(uint n) {
    UsdCsPINVOKE.UsdObjectVector_reserve(swigCPtr, n);
  }

  public UsdObjectVector() : this(UsdCsPINVOKE.new_UsdObjectVector__SWIG_0(), true) {
  }

  public UsdObjectVector(UsdObjectVector other) : this(UsdCsPINVOKE.new_UsdObjectVector__SWIG_1(UsdObjectVector.getCPtr(other)), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdObjectVector(int capacity) : this(UsdCsPINVOKE.new_UsdObjectVector__SWIG_2(capacity), true) {
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  private UsdObject getitemcopy(int index) {
    UsdObject ret = new UsdObject(UsdCsPINVOKE.UsdObjectVector_getitemcopy(swigCPtr, index), true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private UsdObject getitem(int index) {
    UsdObject ret = new UsdObject(UsdCsPINVOKE.UsdObjectVector_getitem(swigCPtr, index), false);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, UsdObject val) {
    UsdCsPINVOKE.UsdObjectVector_setitem(swigCPtr, index, UsdObject.getCPtr(val));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(UsdObjectVector values) {
    UsdCsPINVOKE.UsdObjectVector_AddRange(swigCPtr, UsdObjectVector.getCPtr(values));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public UsdObjectVector GetRange(int index, int count) {
    global::System.IntPtr cPtr = UsdCsPINVOKE.UsdObjectVector_GetRange(swigCPtr, index, count);
    UsdObjectVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new UsdObjectVector(cPtr, true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, UsdObject x) {
    UsdCsPINVOKE.UsdObjectVector_Insert(swigCPtr, index, UsdObject.getCPtr(x));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, UsdObjectVector values) {
    UsdCsPINVOKE.UsdObjectVector_InsertRange(swigCPtr, index, UsdObjectVector.getCPtr(values));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    UsdCsPINVOKE.UsdObjectVector_RemoveAt(swigCPtr, index);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    UsdCsPINVOKE.UsdObjectVector_RemoveRange(swigCPtr, index, count);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public static UsdObjectVector Repeat(UsdObject value, int count) {
    global::System.IntPtr cPtr = UsdCsPINVOKE.UsdObjectVector_Repeat(UsdObject.getCPtr(value), count);
    UsdObjectVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new UsdObjectVector(cPtr, true);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    UsdCsPINVOKE.UsdObjectVector_Reverse__SWIG_0(swigCPtr);
  }

  public void Reverse(int index, int count) {
    UsdCsPINVOKE.UsdObjectVector_Reverse__SWIG_1(swigCPtr, index, count);
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, UsdObjectVector values) {
    UsdCsPINVOKE.UsdObjectVector_SetRange(swigCPtr, index, UsdObjectVector.getCPtr(values));
    if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
  }

}

}

//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace pxr
{
    [Preserve]
    public class VtStringArray : Vt_ArrayBase
    {
        private global::System.Runtime.InteropServices.HandleRef swigCPtr;

        internal VtStringArray(global::System.IntPtr cPtr, bool cMemoryOwn) : base(UsdCsPINVOKE.VtStringArray_SWIGUpcast(cPtr), cMemoryOwn)
        {
            swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
        }

        internal static global::System.Runtime.InteropServices.HandleRef getCPtr(VtStringArray obj)
        {
            return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
        }

        ~VtStringArray()
        {
            Dispose();
        }

        public override void Dispose()
        {
            lock (this) {
                if (swigCPtr.Handle != global::System.IntPtr.Zero)
                {
                    if (swigCMemOwn)
                    {
                        swigCMemOwn = false;
                        UsdCsPINVOKE.delete_VtStringArray(swigCPtr);
                    }
                    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
                }
                global::System.GC.SuppressFinalize(this);
                base.Dispose();
            }
        }

        public string this[int index]
        {
            get { return GetValue(index); }
            set { SetValue(index, value); }
        }

        public VtStringArray() : this(UsdCsPINVOKE.new_VtStringArray__SWIG_0(), true)
        {
        }

        public VtStringArray(VtStringArray other) : this(UsdCsPINVOKE.new_VtStringArray__SWIG_1(VtStringArray.getCPtr(other)), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public VtStringArray(uint n) : this(UsdCsPINVOKE.new_VtStringArray__SWIG_3(n), true)
        {
        }

        public VtStringArray(uint n, string value) : this(UsdCsPINVOKE.new_VtStringArray__SWIG_4(n, value), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void push_back(string elem)
        {
            UsdCsPINVOKE.VtStringArray_push_back(swigCPtr, elem);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void pop_back()
        {
            UsdCsPINVOKE.VtStringArray_pop_back(swigCPtr);
        }

        public uint size()
        {
            uint ret = UsdCsPINVOKE.VtStringArray_size(swigCPtr);
            return ret;
        }

        public uint capacity()
        {
            uint ret = UsdCsPINVOKE.VtStringArray_capacity(swigCPtr);
            return ret;
        }

        public bool empty()
        {
            bool ret = UsdCsPINVOKE.VtStringArray_empty(swigCPtr);
            return ret;
        }

        public void reserve(uint num)
        {
            UsdCsPINVOKE.VtStringArray_reserve(swigCPtr, num);
        }

        public void resize(uint newSize)
        {
            UsdCsPINVOKE.VtStringArray_resize(swigCPtr, newSize);
        }

        public void clear()
        {
            UsdCsPINVOKE.VtStringArray_clear(swigCPtr);
        }

        public void assign(uint n, string fill)
        {
            UsdCsPINVOKE.VtStringArray_assign(swigCPtr, n, fill);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void swap(VtStringArray other)
        {
            UsdCsPINVOKE.VtStringArray_swap(swigCPtr, VtStringArray.getCPtr(other));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public bool IsIdentical(VtStringArray other)
        {
            bool ret = UsdCsPINVOKE.VtStringArray_IsIdentical(swigCPtr, VtStringArray.getCPtr(other));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public static bool Equals(VtStringArray lhs, VtStringArray rhs)
        {
            bool ret = UsdCsPINVOKE.VtStringArray_Equals(VtStringArray.getCPtr(lhs), VtStringArray.getCPtr(rhs));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public override string ToString()
        {
            string ret = UsdCsPINVOKE.VtStringArray_ToString(swigCPtr);
            return ret;
        }

        public void CopyToArray(string[] dest)
        {
            UsdCsPINVOKE.VtStringArray_CopyToArray__SWIG_0(swigCPtr, dest);
        }

        public void CopyFromArray(string[] src)
        {
            UsdCsPINVOKE.VtStringArray_CopyFromArray__SWIG_0(swigCPtr, src);
        }

        public void CopyToArray(System.IntPtr dest)
        {
            UsdCsPINVOKE.VtStringArray_CopyToArray__SWIG_1(swigCPtr, dest);
        }

        public void CopyFromArray(System.IntPtr src)
        {
            UsdCsPINVOKE.VtStringArray_CopyFromArray__SWIG_1(swigCPtr, src);
        }

        protected string GetValue(int index)
        {
            string ret = UsdCsPINVOKE.VtStringArray_GetValue(swigCPtr, index);
            return ret;
        }

        protected void SetValue(int index, string value)
        {
            UsdCsPINVOKE.VtStringArray_SetValue(swigCPtr, index, value);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }
    }
}

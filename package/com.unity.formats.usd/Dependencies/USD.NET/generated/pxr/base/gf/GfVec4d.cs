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
    public class GfVec4d : global::System.IDisposable
    {
        private global::System.Runtime.InteropServices.HandleRef swigCPtr;
        protected bool swigCMemOwn;

        internal GfVec4d(global::System.IntPtr cPtr, bool cMemoryOwn)
        {
            swigCMemOwn = cMemoryOwn;
            swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
        }

        internal static global::System.Runtime.InteropServices.HandleRef getCPtr(GfVec4d obj)
        {
            return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
        }

        ~GfVec4d()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            lock (this) {
                if (swigCPtr.Handle != global::System.IntPtr.Zero)
                {
                    if (swigCMemOwn)
                    {
                        swigCMemOwn = false;
                        UsdCsPINVOKE.delete_GfVec4d(swigCPtr);
                    }
                    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
                }
                global::System.GC.SuppressFinalize(this);
            }
        }

        public GfVec4d() : this(UsdCsPINVOKE.new_GfVec4d__SWIG_0(), true)
        {
        }

        public GfVec4d(double value) : this(UsdCsPINVOKE.new_GfVec4d__SWIG_1(value), true)
        {
        }

        public GfVec4d(double s0, double s1, double s2, double s3) : this(UsdCsPINVOKE.new_GfVec4d__SWIG_2(s0, s1, s2, s3), true)
        {
        }

        public GfVec4d(GfVec4f other) : this(UsdCsPINVOKE.new_GfVec4d__SWIG_4(GfVec4f.getCPtr(other)), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public GfVec4d(GfVec4h other) : this(UsdCsPINVOKE.new_GfVec4d__SWIG_5(GfVec4h.getCPtr(other)), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public GfVec4d(GfVec4i other) : this(UsdCsPINVOKE.new_GfVec4d__SWIG_6(GfVec4i.getCPtr(other)), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public static GfVec4d XAxis()
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_XAxis(), true);
            return ret;
        }

        public static GfVec4d YAxis()
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_YAxis(), true);
            return ret;
        }

        public static GfVec4d ZAxis()
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_ZAxis(), true);
            return ret;
        }

        public static GfVec4d WAxis()
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_WAxis(), true);
            return ret;
        }

        public static GfVec4d Axis(uint i)
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_Axis(i), true);
            return ret;
        }

        public GfVec4d Set(double s0, double s1, double s2, double s3)
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_Set(swigCPtr, s0, s1, s2, s3), false);
            return ret;
        }

        public GfVec4d GetProjection(GfVec4d v)
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_GetProjection(swigCPtr, GfVec4d.getCPtr(v)), true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public GfVec4d GetComplement(GfVec4d b)
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_GetComplement(swigCPtr, GfVec4d.getCPtr(b)), true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public double GetLengthSq()
        {
            double ret = UsdCsPINVOKE.GfVec4d_GetLengthSq(swigCPtr);
            return ret;
        }

        public double GetLength()
        {
            double ret = UsdCsPINVOKE.GfVec4d_GetLength(swigCPtr);
            return ret;
        }

        public double Normalize(double eps)
        {
            double ret = UsdCsPINVOKE.GfVec4d_Normalize__SWIG_0(swigCPtr, eps);
            return ret;
        }

        public double Normalize()
        {
            double ret = UsdCsPINVOKE.GfVec4d_Normalize__SWIG_1(swigCPtr);
            return ret;
        }

        public GfVec4d GetNormalized(double eps)
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_GetNormalized__SWIG_0(swigCPtr, eps), true);
            return ret;
        }

        public GfVec4d GetNormalized()
        {
            GfVec4d ret = new GfVec4d(UsdCsPINVOKE.GfVec4d_GetNormalized__SWIG_1(swigCPtr), true);
            return ret;
        }

        public static bool Equals(GfVec4d lhs, GfVec4d rhs)
        {
            bool ret = UsdCsPINVOKE.GfVec4d_Equals(GfVec4d.getCPtr(lhs), GfVec4d.getCPtr(rhs));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        override public int GetHashCode()
        {
            int ret = UsdCsPINVOKE.GfVec4d_GetHashCode(swigCPtr);
            return ret;
        }

        public static bool operator==(GfVec4d lhs, GfVec4d rhs)
        {
            // The Swig binding glue will re-enter this operator comparing to null, so
            // that case must be handled explicitly to avoid an infinite loop. This is still
            // not great, since it crosses the C#/C++ barrier twice. A better approache might
            // be to return a simple value from C++ that can be compared in C#.
            bool lnull = lhs as object == null;
            bool rnull = rhs as object == null;
            return (lnull == rnull) && ((lnull && rnull) || GfVec4d.Equals(lhs, rhs));
        }

        public static bool operator!=(GfVec4d lhs, GfVec4d rhs)
        {
            return !(lhs == rhs);
        }

        override public bool Equals(object rhs)
        {
            return GfVec4d.Equals(this, rhs as GfVec4d);
        }

        protected double GetValue(int index)
        {
            double ret = UsdCsPINVOKE.GfVec4d_GetValue(swigCPtr, index);
            return ret;
        }

        protected void SetValue(int index, double value)
        {
            UsdCsPINVOKE.GfVec4d_SetValue(swigCPtr, index, value);
        }

        public double this[int index]
        {
            get { return GetValue(index); }
            set { SetValue(index, value); }
        }

        public static readonly uint dimension = UsdCsPINVOKE.GfVec4d_dimension_get();
    }
}

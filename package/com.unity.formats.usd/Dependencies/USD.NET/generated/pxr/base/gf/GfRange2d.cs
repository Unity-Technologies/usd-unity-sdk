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
    public class GfRange2d : global::System.IDisposable
    {
        private global::System.Runtime.InteropServices.HandleRef swigCPtr;
        protected bool swigCMemOwn;

        internal GfRange2d(global::System.IntPtr cPtr, bool cMemoryOwn)
        {
            swigCMemOwn = cMemoryOwn;
            swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
        }

        internal static global::System.Runtime.InteropServices.HandleRef getCPtr(GfRange2d obj)
        {
            return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
        }

        ~GfRange2d()
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
                        UsdCsPINVOKE.delete_GfRange2d(swigCPtr);
                    }
                    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
                }
                global::System.GC.SuppressFinalize(this);
            }
        }

        public void SetEmpty()
        {
            UsdCsPINVOKE.GfRange2d_SetEmpty(swigCPtr);
        }

        public GfRange2d() : this(UsdCsPINVOKE.new_GfRange2d__SWIG_0(), true)
        {
        }

        public GfRange2d(GfVec2d min, GfVec2d max) : this(UsdCsPINVOKE.new_GfRange2d__SWIG_1(GfVec2d.getCPtr(min), GfVec2d.getCPtr(max)), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public GfVec2d GetMin()
        {
            GfVec2d ret = new GfVec2d(UsdCsPINVOKE.GfRange2d_GetMin(swigCPtr), false);
            return ret;
        }

        public GfVec2d GetMax()
        {
            GfVec2d ret = new GfVec2d(UsdCsPINVOKE.GfRange2d_GetMax(swigCPtr), false);
            return ret;
        }

        public GfVec2d GetSize()
        {
            GfVec2d ret = new GfVec2d(UsdCsPINVOKE.GfRange2d_GetSize(swigCPtr), true);
            return ret;
        }

        public GfVec2d GetMidpoint()
        {
            GfVec2d ret = new GfVec2d(UsdCsPINVOKE.GfRange2d_GetMidpoint(swigCPtr), true);
            return ret;
        }

        public void SetMin(GfVec2d min)
        {
            UsdCsPINVOKE.GfRange2d_SetMin(swigCPtr, GfVec2d.getCPtr(min));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void SetMax(GfVec2d max)
        {
            UsdCsPINVOKE.GfRange2d_SetMax(swigCPtr, GfVec2d.getCPtr(max));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public bool IsEmpty()
        {
            bool ret = UsdCsPINVOKE.GfRange2d_IsEmpty(swigCPtr);
            return ret;
        }

        public void ExtendBy(GfVec2d point)
        {
            UsdCsPINVOKE.GfRange2d_ExtendBy__SWIG_0(swigCPtr, GfVec2d.getCPtr(point));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void ExtendBy(GfRange2d range)
        {
            UsdCsPINVOKE.GfRange2d_ExtendBy__SWIG_1(swigCPtr, GfRange2d.getCPtr(range));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public bool Contains(GfVec2d point)
        {
            bool ret = UsdCsPINVOKE.GfRange2d_Contains__SWIG_0(swigCPtr, GfVec2d.getCPtr(point));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public bool Contains(GfRange2d range)
        {
            bool ret = UsdCsPINVOKE.GfRange2d_Contains__SWIG_1(swigCPtr, GfRange2d.getCPtr(range));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public bool IsInside(GfVec2d point)
        {
            bool ret = UsdCsPINVOKE.GfRange2d_IsInside__SWIG_0(swigCPtr, GfVec2d.getCPtr(point));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public bool IsInside(GfRange2d range)
        {
            bool ret = UsdCsPINVOKE.GfRange2d_IsInside__SWIG_1(swigCPtr, GfRange2d.getCPtr(range));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public bool IsOutside(GfRange2d range)
        {
            bool ret = UsdCsPINVOKE.GfRange2d_IsOutside(swigCPtr, GfRange2d.getCPtr(range));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public static GfRange2d GetUnion(GfRange2d a, GfRange2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_GetUnion(GfRange2d.getCPtr(a), GfRange2d.getCPtr(b)), true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public GfRange2d UnionWith(GfRange2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_UnionWith__SWIG_0(swigCPtr, GfRange2d.getCPtr(b)), false);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public GfRange2d UnionWith(GfVec2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_UnionWith__SWIG_1(swigCPtr, GfVec2d.getCPtr(b)), false);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public static GfRange2d Union(GfRange2d a, GfRange2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_Union__SWIG_0(GfRange2d.getCPtr(a), GfRange2d.getCPtr(b)), true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public GfRange2d Union(GfVec2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_Union__SWIG_2(swigCPtr, GfVec2d.getCPtr(b)), false);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public static GfRange2d GetIntersection(GfRange2d a, GfRange2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_GetIntersection(GfRange2d.getCPtr(a), GfRange2d.getCPtr(b)), true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public static GfRange2d Intersection(GfRange2d a, GfRange2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_Intersection__SWIG_0(GfRange2d.getCPtr(a), GfRange2d.getCPtr(b)), true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public GfRange2d IntersectWith(GfRange2d b)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_IntersectWith(swigCPtr, GfRange2d.getCPtr(b)), false);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public double GetDistanceSquared(GfVec2d p)
        {
            double ret = UsdCsPINVOKE.GfRange2d_GetDistanceSquared(swigCPtr, GfVec2d.getCPtr(p));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public GfVec2d GetCorner(uint i)
        {
            GfVec2d ret = new GfVec2d(UsdCsPINVOKE.GfRange2d_GetCorner(swigCPtr, i), true);
            return ret;
        }

        public GfRange2d GetQuadrant(uint i)
        {
            GfRange2d ret = new GfRange2d(UsdCsPINVOKE.GfRange2d_GetQuadrant(swigCPtr, i), true);
            return ret;
        }

        public static GfRange2d UnitSquare
        {
            get
            {
                global::System.IntPtr cPtr = UsdCsPINVOKE.GfRange2d_UnitSquare_get();
                GfRange2d ret = (cPtr == global::System.IntPtr.Zero) ? null : new GfRange2d(cPtr, false);
                return ret;
            }
        }

        public static bool Equals(GfRange2d lhs, GfRange2d rhs)
        {
            bool ret = UsdCsPINVOKE.GfRange2d_Equals(GfRange2d.getCPtr(lhs), GfRange2d.getCPtr(rhs));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        override public int GetHashCode()
        {
            int ret = UsdCsPINVOKE.GfRange2d_GetHashCode(swigCPtr);
            return ret;
        }

        public static bool operator==(GfRange2d lhs, GfRange2d rhs)
        {
            // The Swig binding glue will re-enter this operator comparing to null, so
            // that case must be handled explicitly to avoid an infinite loop. This is still
            // not great, since it crosses the C#/C++ barrier twice. A better approache might
            // be to return a simple value from C++ that can be compared in C#.
            bool lnull = lhs as object == null;
            bool rnull = rhs as object == null;
            return (lnull == rnull) && ((lnull && rnull) || GfRange2d.Equals(lhs, rhs));
        }

        public static bool operator!=(GfRange2d lhs, GfRange2d rhs)
        {
            return !(lhs == rhs);
        }

        override public bool Equals(object rhs)
        {
            return GfRange2d.Equals(this, rhs as GfRange2d);
        }

        public static readonly uint dimension = UsdCsPINVOKE.GfRange2d_dimension_get();
    }
}

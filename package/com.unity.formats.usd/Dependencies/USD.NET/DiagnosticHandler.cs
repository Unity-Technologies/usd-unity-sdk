using System;
using System.Runtime.InteropServices;

namespace pxr
{
    public abstract class DiagnosticHandler : global::System.IDisposable
    {
        private static DiagnosticHandler ms_Instance;


        #region "Unmanaged Callback Interface"

        delegate void UsdLogCallback(int logType, [MarshalAs(UnmanagedType.LPStr)] string msg);

        [DllImport("UsdCs", CallingConvention = CallingConvention.Cdecl)]
        static extern void RegisterUsdLogCallback(UsdLogCallback cb);

        [MonoPInvokeCallback]
        static void OnUsdLogEvent(int logType, [MarshalAs(UnmanagedType.LPStr)] string msg)
        {
            if (ms_Instance == null)
            {
                return;
            }

            switch (logType)
            {
                case 0:
                    ms_Instance.OnInfo(msg);
                    break;
                case 1:
                    ms_Instance.OnWarning(msg);
                    break;
                case 2:
                    ms_Instance.OnError(msg);
                    break;
                case 3:
                    ms_Instance.OnFatalError(msg);
                    break;
            }
        }

        #endregion


        public DiagnosticHandler()
        {
            ms_Instance = this;
            RegisterUsdLogCallback(OnUsdLogEvent);
        }

        ~DiagnosticHandler()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (ms_Instance == this)
            {
                ms_Instance = null;
            }
        }

        public abstract void OnInfo(string msg);
        public abstract void OnWarning(string msg);
        public abstract void OnError(string msg);
        public abstract void OnFatalError(string msg);
    }
}

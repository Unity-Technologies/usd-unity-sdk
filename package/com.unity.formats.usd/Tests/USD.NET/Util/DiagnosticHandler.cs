using System;
using UnityEngine;

namespace USD.NET.Tests
{
    internal class DiagnosticHandler : pxr.DiagnosticHandler
    {
        public DiagnosticHandler() : base()
        {
        }

        /// <summary>
        /// Messages sent when an unrecoverable fatal error occured in the USD API.
        /// </summary>
        public override void OnFatalError(string msg)
        {
            // Note: the system is about to abort().
            Debug.LogException(new Exception("USD FATAL ERROR: " + msg));
        }

        /// <summary>
        /// Messages sent when an error occured in the USD API, but was not fatal.
        /// </summary>
        public override void OnError(string msg)
        {
            // This error comes from UsdAttributeQuery, but there is some debate that it should be an
            // error at all. UsdSkelCache::Populate triggers it to be spewed and given that it only
            // functions to confus the user, it's suppressed here.
            if (msg == "Invalid attribute")
            {
                return;
            }

            // Report all other non-fatal errors.
            Debug.LogException(new ApplicationException("USD ERROR: " + msg));
        }

        /// <summary>
        /// Warning messages sent from the USD API upon misuse or unexpected run-time conditions.
        /// </summary>
        public override void OnWarning(string msg)
        {
            Debug.LogWarning("USD: " + msg);
        }

        /// <summary>
        /// Informational messages sent from the USD API.
        /// </summary>
        public override void OnInfo(string msg)
        {
            Debug.Log("USD: " + msg);
        }
    }
}
